using ComputeSharp;
using ShapeScape.Forms;
using ShapeScape.ImageCache;
using ShapeScape.Rendering;
using ShapeScape.Shader;
using ShapeScape.Shader.Shaders;
using ShapeScape.Shapes;
using ShapeScape.Utils;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Rectangle = ShapeScape.Shapes.Rectangle;

namespace ShapeScape
{
    // TODO : Ok so the original program got its colors from the image palette.
    // We should really try doing that
    // The rectangles used the color at their center, and the NPOLYGONS used a random color from the image
    // We can probably just cache all the colors in the image into memory and then refer that when initalising the shapelist
    // This actually makes alot of sense for all the problems im having
    public static class Program
    {
        // Values set by the user
        #region Generation settings

        /// <summary>
        /// Seed used to control <see cref="Program.rand"/>. <br/>
        /// Defaults to -1 (Random seed)
        /// </summary>
        public static int Seed = -1;

        /// <summary>
        /// Name of the image file to be reconstructed.<br/>
        /// Defaults to null
        /// </summary>
        public static string Filename = null;

        /// <summary>
        /// Total amount of shapes that will get drawn to complete the image <br/> 
        /// Defaults to 512
        /// </summary>
        public static int TotalShapes = 512;

        /// <summary>
        /// Total amount of shapes being evolved in each cycle <br/>
        /// Defaults to 500
        /// </summary>
        public static int ShapePopulation = 2500;

        /// <summary>
        /// How many times to cycle the shapes before selecting one to add to the canvas <br/>
        /// Defaults to 10
        /// </summary>
        public static int EvolutionCycles = 6;

        /// <summary>
        /// The shapes with the <see cref="SurvivalThreshold"/> lowest scores survive, the rest all die
        /// </summary>
        public static int SurvivalThreshold = 50;

        /// <summary>
        /// Controls how strong mutation effects between generations are.<br/>
        /// Advised to keep around 50
        /// </summary>
        public static int MutationStrength = 60;

        /// <summary>
        /// Whether to initalise <see cref="PalletteCache"/> and use Pallette based colors when generating shapes <br/>
        /// Defaults to false
        /// </summary>
        public static bool Palletise = false;

        /// <summary>
        /// Factor to downscale <see cref="BaseImageBuffer"/> by before generation <br/>
        /// Defaults to 20
        /// </summary>
        public static float DownscaleFactor = 20f;

        #endregion

        // Values used internally and values calculated from user settings
        #region Fields

        /// <summary>
        /// Calculate from <see cref="ShapePopulation"/> and <see cref="SurvivalThreshold"/>.<br/>
        /// At the end of an evolution cycle, every shape will produce this many children
        /// </summary>
        public static int ChildCount => (int)Math.Round((double)ShapePopulation / (double)SurvivalThreshold) - 1; // -1 to account for the parent

        // TODO : In a "release build" or something i don't think we'd use a working directory?
        // Either user just submits file path or somehow i get a file selector form thingy working
        // Actually we might just make it so the "workingDirectory" is whatever folder the program is stored in
        // Just modify FileUtils.WorkingDirectory to not combine with "WorkingDirectory" and boom
        /// <summary>
        /// File path to the target image
        /// </summary>
        public static string ImagePath => Path.Combine(FileUtils.WorkingDirectory, Filename);

        /// <summary>
        /// True dimensions of the output image as <see cref="BaseImageBuffer"/> is reduced in size drastically
        /// </summary>
        public static int2 ScaledDimensions => new int2((int)(Dimensions.X * DownscaleFactor), (int)(Dimensions.Y * DownscaleFactor));

        // Techinically this should be grouped into <see cref="RandomUtils"/> because it would make more sense there, but i'm very used to MainClass.rand.Next() calls
        /// <summary>
        /// Random class shared program-wide. <br/>
        /// </summary>
        public static Random rand = new Random();

        /// <summary>
        /// The texture of the base image
        /// </summary>
        public static ReadOnlyTexture2D<Rgba32, float4> BaseImageBuffer;

        /// <summary>
        /// Width and height of <see cref="Program.BaseImageBuffer"/>
        /// </summary>
        public static int2 Dimensions => new int2(BaseImageBuffer.Width, BaseImageBuffer.Height);

        /// <summary>
        /// Interal timer used for generation time feedback
        /// </summary>
        private static Stopwatch sw = new Stopwatch();

        /// <summary>
        /// For displaying to <see cref="Rendering.ImageRenderer"/>
        /// </summary>
        private static Bitmap ResultMap;

        #endregion

        
        #region Constants

        /// <summary>
        /// The highest number of tessels the program can ever encounter in a single polygon is 12 <br/>
        /// This comes from NPolygon having a max vertex count of 8, and applying the formula 2N - 4 to calculate tessel count from vertices
        /// </summary>
        public const int MAX_TESSELS = 12;

        #endregion


        #region Methods
        [STAThread]
        static void Main(string[] args)
        {
            // Set all generation settings
            GetUserInput();
            if (Filename == "NOFILE") // They pressed x on the window
            {
                return;
            }
            // Start the clock!
            Stopwatch sw = Stopwatch.StartNew();

            // Load the target image into memory and downscale it
            BaseImageBuffer = GraphicsDevice.GetDefault().LoadReadOnlyTexture2D<Rgba32, float4>(ImagePath);
            BaseImageBuffer = GraphicsDevice.GetDefault().AllocateReadOnlyTexture2D<Rgba32, float4>(BitmapUtils.Downscale(BaseImageBuffer.ToArray(), (int)DownscaleFactor));

            // Bring this up before potential pallettisation so the user knows the program is working
            ResultMap = new Bitmap(ScaledDimensions.X, ScaledDimensions.Y);
            ImageRenderer.Update(ResultMap);
            if (Palletise)
            {
                PalletteCache.CreatePallette();
            }

            // Create canvas
            using var constructorCanvasBuffer = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<Rgba32, float4>(Dimensions.X, Dimensions.Y);
            using var outputBuffer = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<Rgba32, float4>(ScaledDimensions.X, ScaledDimensions.Y);

            // Create score array so it can be re-used constantly
            float[] score = new float[ShapePopulation];
            using var scoreBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<float>(ShapePopulation);

            /* Array of tesselation arrays for each polygon
            * This system is a bit terrible and confusing but that's what happens when Computesharp has ZERO documentation and nobody using it
            * Still better than trying to learn any of the terriblness that is (Insert any other shader framework here)
            * Basically there's a 2d map of tessels, each column represents a polygon, and the y is every tessel that composes that polygon
            * So for example, if there was a rectangle and a NPolygon made of 4 tessels, the map might be like this:
            * [polygon 0 (rectangle), tessel 0],[polygon 1 (NPolygon), tessel 0]
            * [polygon 0 (rectangle), tessel 1],[polygon 1 (Npolygon), tessel 1]
            * [blank tessel because rect has 2],[polygon 1 (Npolygon), tessel 2]
            * and so on along the x for each polygon
            * the y or column height is always a fixed 12 because we have to pre-allocate all the space even though we only use it for the rare case of a max Npolygon
            * the reason i can't use varying height is because computeSharp won't support arrays of arrays (float[][]), or lists
            * i then have to lay out a sepearate for each row, and i'd like it to really work the other way around where it's fixed 12 columns and each row represents a polygon,
            * but that just increases complexity with indexing and organising
            * and yes, i am forced to create 12 seperate buffers
            * there is no way to have any buffer of buffers, buffer of arrays in this situation, or anything like that
            * 
            * "But why not make it a texture2D? or any form of value with a accessable y value so you can just implement a map?
            * lets get it working and then find out why i didnt do that before
            */


            Tessel[][] tessLists = new Tessel[MAX_TESSELS][];
            for (int i = 0; i < MAX_TESSELS; i++)
            {
                tessLists[i] = new Tessel[ShapePopulation];
            }
            using var t1 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t2 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t3 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t4 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t5 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t6 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t7 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t8 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t9 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t10 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t11 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);
            using var t12 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(ShapePopulation);

            // Fill canvas with blank color before we start
            GraphicsDevice.GetDefault().For(Dimensions.X, Dimensions.Y, new Shaders.FillColor(constructorCanvasBuffer, PalletteCache.MostCommonColor()));
            GraphicsDevice.GetDefault().For(ScaledDimensions.X, ScaledDimensions.Y, new Shaders.FillColor(outputBuffer, PalletteCache.MostCommonColor()));

            // Main loop - Every cycle of this, one shape is added to the final image
            for (int i = 0; i < TotalShapes; i++)
            {
                sw.Start();

                // Initalise the shape array with random shapes
                BasePolygon[] polygons = new BasePolygon[ShapePopulation];
                
                CreateShapes(ref polygons);

                // Cycle through killing and breeding polygons 
                for (int e = 0; e < EvolutionCycles; e++)
                {
                    for (int j = 0; j < polygons.Length; j++)
                    {
                        Tessel[] tessArray = Tesselator.TessellatePolygon(polygons[j]);

                        // "Unzip" the tessellation array into the lists
                        for (int t = 0; t < tessArray.Length && t < MAX_TESSELS; t++)
                        {
                            tessLists[t][j] = tessArray[t];
                            tessLists[t][j].nothing = new int1x1(1); // This flag indicates this tessel can be ignored in the shader. 
                        }
                    }

                    // Load data into buffers
                    t1.CopyFrom(tessLists[0]);
                    t2.CopyFrom(tessLists[1]);
                    t3.CopyFrom(tessLists[2]);
                    t4.CopyFrom(tessLists[3]);
                    t5.CopyFrom(tessLists[4]);
                    t6.CopyFrom(tessLists[5]);
                    t7.CopyFrom(tessLists[6]);
                    t8.CopyFrom(tessLists[7]);
                    t9.CopyFrom(tessLists[8]);
                    t10.CopyFrom(tessLists[9]);
                    t11.CopyFrom(tessLists[10]);
                    t12.CopyFrom(tessLists[11]);

                    // Run shader
                    GraphicsDevice.GetDefault().For(ShapePopulation, new Shaders.ScoreAllShapes(
                    t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12,
                    BaseImageBuffer, constructorCanvasBuffer, scoreBuffer));
                    scoreBuffer.CopyTo(score);

                    // Sort both of the arrays at once
                    var sorted = score.Zip(polygons,(s,p) => new {s,p})
                        .OrderBy(x => x.s)
                        .ToArray();
                    score = sorted.Select(x => x.s).ToArray();
                    polygons = sorted.Select(x => x.p).ToArray();

                    // on the last iteration we don't make any more kids
                    if (e < EvolutionCycles - 1)
                    {
                        // Keep the top N and kill the rest
                        polygons = polygons.Take(SurvivalThreshold).ToArray();

                        // Create child shapes and re-populate the polygon array
                        List<BasePolygon> polygons1 = new List<BasePolygon>(polygons);
                        int limit = polygons1.Count;
                        for (int j = 0; j < limit; j++)
                        {
                            BasePolygon polygon = polygons1[j];
                            polygon.CreateChildren(ChildCount, MutationStrength, ref polygons1);
                        }

                        polygons = polygons1.ToArray();
                    }

                    Console.WriteLine($"Evolution cycle {e} score : {score[0]} : Duration : {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                }



                // polygons remain sorted at the end of final evolution cycle, select best one and draw it to the canvas.
                BasePolygon winner = polygons[0];
                
                // Modify the constructor canvas and draw the winning shape to it
                Tessel[] tesselWinner = Tesselator.TessellatePolygon(winner);
                using var _tesselationBuffer = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(tesselWinner);
                GraphicsDevice.GetDefault().For(Dimensions.X, Dimensions.Y, new Shaders.DrawToTexture(constructorCanvasBuffer, _tesselationBuffer));
                _tesselationBuffer.Dispose();

                // Re-scale the verticies to the correct size
                Tessel[] clone = (Tessel[])tesselWinner.Clone();
                for (int a = 0; a < clone.Length; a++)
                {
                    clone[a].v0 = new float2(clone[a].v0.X * DownscaleFactor, clone[a].v0.Y * DownscaleFactor);
                    clone[a].v1 = new float2(clone[a].v1.X * DownscaleFactor, clone[a].v1.Y * DownscaleFactor);
                    clone[a].v2 = new float2(clone[a].v2.X * DownscaleFactor, clone[a].v2.Y * DownscaleFactor);
                }
                using var cloneBuffer = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(clone);
                GraphicsDevice.GetDefault().For(ScaledDimensions.X, ScaledDimensions.Y, new Shaders.DrawToTexture(outputBuffer, cloneBuffer));

                // This is relativley performance costly so dont do it every update
                if (i % 2 == 0)
                {
                    ResultMap = outputBuffer.ToArray().ToBitmap();
                    ImageRenderer.Update(ResultMap);
                }
                // Arguably more costly so do it less often
                if (i % 15 == 0)
                {
                    outputBuffer.SaveImage("Progress.png");
                }
                
                sw.Stop();
                Console.WriteLine($"Created shape {i}/{TotalShapes} in {sw.ElapsedMilliseconds}ms with a score of {score[0]}");
                sw.Restart();
            }
            // everything's finished, we save the final image
            outputBuffer.SaveImage("Result.png");

            // cleanup
            constructorCanvasBuffer.Dispose();
            BaseImageBuffer.Dispose();
        }

        // TODO : Okay so we can't use Console at all in winforms (YOU ARE KIDDING ME)
        // Sooooo we need to create the form for setting generation values, and modify the form that displays drawing progress to display other shit that was done in console.writeline()
        public static void GetUserInput()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (UserInputForm form = new UserInputForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string t = form.Filename;
                    if (t is null)
                    {
                        Filename = "NOFILE";
                    }
                    else
                    {
                        Filename = t;
                    }
                    if (form.Seed.HasValue) Seed = form.Seed.Value;
                    if (form.DownscaleFactor.HasValue) DownscaleFactor = form.DownscaleFactor.Value;
                    if (form.TotalShapes.HasValue) TotalShapes = form.TotalShapes.Value;
                    if (form.ShapePopulation.HasValue) ShapePopulation = form.ShapePopulation.Value;
                    if (form.SurvivalThreshold.HasValue) SurvivalThreshold = form.SurvivalThreshold.Value;
                    if (form.EvolutionCycles.HasValue) EvolutionCycles = form.EvolutionCycles.Value;
                    if (form.MutationStrength.HasValue) MutationStrength = form.MutationStrength.Value;
                    Palletise = form.Palletise;
                }
            }
        }



        /// <summary>
        /// Populates the shape array with random shapes
        /// </summary>
        private static void CreateShapes(ref BasePolygon[] polygons)
        {
            Console.WriteLine("Initalising shape arrays.");
            for (int i = 0; i < polygons.Length; i++)
            {
                int shapeType = rand.Next(2);
                switch (shapeType)
                {
                    case 0:
                        polygons[i] = new Triangle();
                        break;
                    case 1:
                        polygons[i] = new Rectangle();
                        break;
                    default:
                        Console.WriteLine($"Something went wrong creating shapes in {CreateShapes}");
                        polygons[i] = new Triangle();
                        break;
                }
            }
        }
        #endregion
    }
}

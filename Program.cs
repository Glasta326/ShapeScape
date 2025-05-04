using ComputeSharp;
using ShapeScape.Shader;
using ShapeScape.Shader.Shaders;
using ShapeScape.Shapes;
using ShapeScape.Utils;
using System.Diagnostics;

namespace ShapeScape
{
    public static class Program
    {
        /// <summary>
        /// Random class shared program-wide.
        /// </summary>
        public static Random rand = new Random();

        /// <summary>
        /// The dimensions of the canvas the program is working on
        /// </summary>
        public static int2 Dimensions;

        /// <summary>
        /// Name of the file being reconstructed
        /// </summary>
        public static string Filename = "SolverLogo.png";
        public static string ImagePath = Path.Combine(FileUtils.WorkingDirectory, Filename);

        /// <summary>
        /// Keeps track of the total score of our image
        /// </summary>
        public static float ScoreTracker = float.PositiveInfinity;

        /// <summary>
        /// The highest number of tessels the program can ever encounter in a single polygon is 12 <br/>
        /// This comes from NPolygon having a max vertex count of 8, and applying the formula 2N - 4 to calculate tessel count from vertices
        /// </summary>
        public const int MAX_TESSELS = 12;

        #region Settings
        /// <summary>
        /// The number of shapes the final image will be comprised of
        /// </summary>
        public static int ShapeLimit = 1000;

        /// <summary>
        /// Starts out with this many completley random shapes. on the first cycle, these are culled down to <see cref="PopulationSize"/>
        /// </summary>
        public static int InitalPopulation = 500;

        /// <summary>
        /// The number of shapes being evolved
        /// </summary>
        public static int PopulationSize = 500;

        /// <summary>
        /// Top N% survive, the rest are removed
        /// </summary>
        public static int TopNSurvive = 50;

        /// <summary>
        /// How many times the shapes get evolved
        /// </summary>
        public static int EvolutionSteps = 6;

        /// <summary>
        /// The number of children each shape will have after population culling
        /// </summary>
        private static int Childcount = 9;

        /// <summary>
        /// Affects how crazy mutations are. Advised to keep around 50
        /// </summary>
        private static int MutationStrength = 60;

        #endregion

        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // Load the target image into memory
            using var baseImageBuffer = GraphicsDevice.GetDefault().LoadReadOnlyTexture2D<Rgba32, float4>(ImagePath);
            Dimensions = new int2(baseImageBuffer.Width, baseImageBuffer.Height);

            // Create both canvases
            using var constructorCanvasBuffer = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<Rgba32, float4>(Dimensions.X, Dimensions.Y);
            using var constructorCanvasCopyBuffer = GraphicsDevice.GetDefault().AllocateReadWriteTexture2D<Rgba32, float4>(Dimensions.X, Dimensions.Y);

            // Create score array so it can be re-used constantly
            float[] score = new float[PopulationSize];
            using var scoreBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<float>(PopulationSize);

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
                tessLists[i] = new Tessel[PopulationSize];
            }
            using var t1 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t2 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t3 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t4 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t5 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t6 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t7 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t8 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t9 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t10 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t11 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);
            using var t12 = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(PopulationSize);

            // Fill canvas with blank color before we start
            GraphicsDevice.GetDefault().For(Dimensions.X, Dimensions.Y, new Shaders.FillColor(constructorCanvasBuffer));

            // Main loop - Every cycle of this, one shape is added to the final image
            for (int i = 0; i < ShapeLimit; i++)
            {
                sw.Start();

                // Initalise the shape array with random shapes
                Polygon[] polygons = new Polygon[InitalPopulation];
                
                Init(ref polygons);

                // Cycle through killing and breeding polygons 
                for (int e = 0; e < EvolutionSteps; e++)
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
                    GraphicsDevice.GetDefault().For(PopulationSize, new Shaders.ScoreAllShapes(
                    t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12,
                    baseImageBuffer, constructorCanvasBuffer, constructorCanvasCopyBuffer, scoreBuffer));
                    scoreBuffer.CopyTo(score);

                    // Sort both of the arrays at once
                    var sorted = score.Zip(polygons,(s,p) => new {s,p})
                        .OrderBy(x => x.s)
                        .ToArray();
                    score = sorted.Select(x => x.s).ToArray();
                    polygons = sorted.Select(x => x.p).ToArray();

                    // on the last iteration we don't make any more kids
                    if (e < EvolutionSteps - 1)
                    {
                        // Keep the top N and kill the rest
                        Console.WriteLine(score[0]);
                        polygons = polygons.Take(TopNSurvive).ToArray();

                        // Create child shapes and re-populate the polygon array
                        List<Polygon> polygons1 = new List<Polygon>(polygons);
                        int limit = polygons1.Count;
                        for (int j = 0; j < limit; j++)
                        {
                            Polygon polygon = polygons1[j];
                            polygon.CreateChildren(Childcount, MutationStrength, ref polygons1);
                        }

                        polygons = polygons1.ToArray();
                    }
                }



                // polygons remain sorted at the end of final evolution cycle, select best one and draw it to the canvas.
                Polygon winner = polygons[0];
                if (false)//winner.Score > ScoreTracker)
                {
                    sw.Stop();
                    Console.WriteLine($"All shapes reduced image quality. no shape was drawn in {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                    continue;
                }
                
                // Technicallyyyy we are re-tesselating this polygon again but the data for that is jumbled across the buffer sandwich  so this is significantly more convienent.
                // Performance doesn't really matter outside of the evo loop anyway
                Tessel[] tesselWinner = Tesselator.TessellatePolygon(winner);
                using var _tesselationBuffer = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(tesselWinner);
                GraphicsDevice.GetDefault().For(Dimensions.X, Dimensions.Y, new Shaders.DrawToConstructor(constructorCanvasBuffer, _tesselationBuffer));
                _tesselationBuffer.Dispose();

                // just to keep progress
                constructorCanvasBuffer.SaveImage("Progress.png");
                sw.Stop();
                Console.WriteLine($"Created shape {i}/{ShapeLimit} in {sw.ElapsedMilliseconds}ms with a score of {score[0]}");
                ScoreTracker = winner.Score;
                sw.Restart();
            }
            // everything's finished, we save the final image
            constructorCanvasBuffer.SaveImage("Result.png");

            // cleanup
            constructorCanvasCopyBuffer.Dispose();
            constructorCanvasBuffer.Dispose();
            baseImageBuffer.Dispose();
        }

        /// <summary>
        /// Populates the shape array with random shapes
        /// </summary>
        private static void Init(ref Polygon[] polygons)
        {
            for (int i = 0; i < polygons.Length; i++)
            {
                polygons[i] = CreateRandomShape();
            }
        }
        private static Polygon CreateRandomShape()
        {
            int shapeType = rand.Next(3); // Adjust the number based on how many shape types you have
            return shapeType switch
            {
                0 => new NPolygon(),
                1 => new NPolygon(),
                2 => new Triangle(),
                _ => throw new InvalidOperationException("Unexpected shape type")
            };
        }
        public static T[][] Split2DArray<T>(T[,] input)
        {

            int width = input.GetLength(0);
            T[][] result = new T[12][];

            for (int i = 0; i < 12; i++)
            {
                result[i] = new T[width];
                for (int j = 0; j < width; j++)
                {
                    result[i][j] = input[i, j];
                }
            }

            return result;
        }

    }
}

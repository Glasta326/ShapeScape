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
        public static string Filename = "MDTrailer.jpg";
        public static string ImagePath = Path.Combine(FileUtils.WorkingDirectory, Filename);

        /// <summary>
        /// Keeps track of the total score of our image
        /// </summary>
        public static float ScoreTracker = float.PositiveInfinity;

        #region Settings
        /// <summary>
        /// The number of shapes the final image will be comprised of
        /// </summary>
        public static int ShapeLimit = 1;

        /// <summary>
        /// Starts out with this many completley random shapes. on the first cycle, these are culled down to <see cref="PopulationSize"/>
        /// </summary>
        public static int InitalPopulation = 50;

        /// <summary>
        /// The number of shapes being evolved
        /// </summary>
        public static int PopulationSize = 50;

        /// <summary>
        /// Top N% survive, the rest are removed
        /// </summary>
        public static int TopNSurvive = 20;

        /// <summary>
        /// How many times the shapes get evolved
        /// </summary>
        public static int EvolutionSteps = 1;

        /// <summary>
        /// The number of children each shape will have after population culling
        /// </summary>
        private static int Childcount = 0;

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
            float[,] score = new float[PopulationSize, 12];

            // Array of tesselation arrays for each polygon
            // Maximum amount of tessels for a shape will be from an NPolygon with 8 points (calculated by 2N - 4)
            // i kind of hate this and wish we could just have a varying Y column for the amount of tessels in the polygon but eh
            // cpu can suffer anyway go copy data bitch

            // Please see line 152 in shaders.cs for further details]
            const int MAX_TESSEL_SLOTS = 12; // Maximum tessellation pieces per polygo
            Tessel[][] tessLists = new Tessel[MAX_TESSEL_SLOTS][];
            for (int i = 0; i < MAX_TESSEL_SLOTS; i++)
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

            // Load the score array 
            using var scoreBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<float>(PopulationSize);

            // fill color
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
                    // Loop through each polygon, tessellate it, and assign each tessellation
                    // to the corresponding list.
                    for (int j = 0; j < polygons.Length; j++)
                    {
                        // Tessellate the polygon. The returned array may have a length less than MAX_TESSEL_SLOTS.
                        Tessel[] tessArray = Tesselator.TessellatePolygon(polygons[j]);

                        // "Unzip" the tessellation array into the lists
                        for (int t = 0; t < tessArray.Length && t < MAX_TESSEL_SLOTS; t++)
                        {
                            tessLists[t][j] = tessArray[t];
                            tessLists[t][j].nothing = new int1x1(1);
                        }
                    }

                    // losing it
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
                    GraphicsDevice.GetDefault().For(PopulationSize, new Shaders.ScoreAllShapes(
                        t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12,
                        baseImageBuffer, constructorCanvasBuffer, constructorCanvasCopyBuffer, scoreBuffer));

                    constructorCanvasCopyBuffer.SaveImage("TestResult.png");

                    // Sort the polygons by their scores and keep the top TopNSurvive ratio of them. the rest are nullified
                    polygons = polygons.OrderBy(d => d.Score).ToArray();
                    Console.WriteLine(polygons[0].Score);
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



                // Find the best polygon and draw it to the constructor canvas
                Polygon winner = polygons.OrderBy(d => d.Score).ToArray()[0];
                if (winner.Score > ScoreTracker)
                {
                    sw.Stop();
                    Console.WriteLine($"All shapes reduced image quality. no shape was drawn in {sw.ElapsedMilliseconds}ms");
                    sw.Restart();
                    continue;
                }

                Tessel[] tesselWinner = Tesselator.TessellatePolygon(winner);
                using var tesselationBuffer = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(tesselWinner);
                GraphicsDevice.GetDefault().For(Dimensions.X, Dimensions.Y, new Shaders.DrawToConstructor(constructorCanvasBuffer, tesselationBuffer));
                tesselationBuffer.Dispose();

                // just to keep progress
                constructorCanvasBuffer.SaveImage("Progress.png");
                sw.Stop();
                Console.WriteLine($"Created shape {i}/{ShapeLimit} in {sw.ElapsedMilliseconds}ms with a score of {winner.Score}");
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
                0 => new Triangle(),
                1 => new NPolygon(),
                2 => new NPolygon(),
                _ => throw new InvalidOperationException("Unexpected shape type")
            };;
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

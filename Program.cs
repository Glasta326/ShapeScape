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
        public static int ShapeLimit = 2000;

        /// <summary>
        /// Starts out with this many completley random shapes. on the first cycle, these are culled down to <see cref="PopulationSize"/>
        /// </summary>
        public static int InitalPopulation = 2000;

        /// <summary>
        /// The number of shapes being evolved
        /// </summary>
        public static int PopulationSize = 250;

        /// <summary>
        /// Top N% survive, the rest are removed
        /// </summary>
        public static int TopNSurvive = 5;

        /// <summary>
        /// How many times the shapes get evolved
        /// </summary>
        public static int EvolutionSteps = 8;

        /// <summary>
        /// The number of children each shape will have after population culling
        /// </summary>
        private static int Childcount = 49;

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
            float[] score = new float[Dimensions.X * Dimensions.Y];

            // Load the score array 
            using var scoreBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<float>(score);

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
                    for (int j = 0;j < polygons.Length; j++)
                    {
                        // Give the polygon a tesselation if it does not already have one
                        if (polygons[j].Tesselation is null)
                        {
                            polygons[j].Tesselation = Tesselator.TessellatePolygon(polygons[j]);
                        }

                        // Allocate the tesselation
                        using var TesselationBuffer = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(polygons[j].Tesselation);

                        // Run the color difference shader and copy the score into the score array
                        GraphicsDevice.GetDefault().For(Dimensions.X, Dimensions.Y, new Shaders.DrawAndScore(
                            baseImageBuffer, constructorCanvasBuffer, constructorCanvasCopyBuffer, TesselationBuffer, scoreBuffer));
                        scoreBuffer.CopyTo(score);

                        // Mark that polygon with its new score
                        polygons[j].Score = score.AsParallel().Sum();
                    }

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

                // Score the polygons one more time, to account for the new children from the end of the main evo loop
                for (int j = 0; j < polygons.Length; j++)
                {
                    // Give the polygon a tesselation if it does not already have oen
                    if (polygons[j].Tesselation is null)
                    {
                        polygons[j].Tesselation = Tesselator.TessellatePolygon(polygons[j]);
                    }

                    // Load the score array and tessel array into GPU
                    // Note : We load score into GPU without resetting its contents, this is because the values get overriden in GPU so it doesnt matter
                    using var TesselationBuffer = GraphicsDevice.GetDefault().AllocateReadOnlyBuffer<Tessel>(polygons[j].Tesselation);

                    // Run the color difference shader and copy the score into the score array
                    GraphicsDevice.GetDefault().For(Dimensions.X, Dimensions.Y, new Shaders.DrawAndScore(
                        baseImageBuffer, constructorCanvasBuffer, constructorCanvasCopyBuffer, TesselationBuffer, scoreBuffer));
                    scoreBuffer.CopyTo(score);

                    // Mark that polygon with its new score
                    polygons[j].Score = score.AsParallel().Sum();
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
    }
}

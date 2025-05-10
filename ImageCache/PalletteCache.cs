using ComputeSharp;
using ShapeScape.Shader.Shaders;
using ShapeScape.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.ImageCache
{
    public static class PalletteCache
    {
        public static Dictionary<float4, int> Pallette;

        /// <summary>
        /// True when <see cref="PalletteCache"/> has been initalised with data from <see cref="Program.BaseImageBuffer"/>
        /// </summary>
        public static bool Active = false;

        /// <summary>
        /// Cached value for <see cref="PalletteColorWeighted"/>
        /// </summary>
        private static int totalPixels;

        /// <summary>
        /// Initalise the <see cref="Pallette"/> with values and frequency of colors in the image
        /// </summary>
        public static void CreatePallette()
        {
            Console.WriteLine("Creating Image pallette. This may take some time...");
            Pallette = new Dictionary<Float4, int>();

            // Convert the image into a 1d array of float4 colors via shader 
            float4[] colors = new Float4[Program.Dimensions.X * Program.Dimensions.Y];
            using ReadWriteBuffer<float4> colorBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<float4>(colors);
            GraphicsDevice.GetDefault().For(Program.Dimensions.X, Program.Dimensions.Y, new Shaders.Pallettise(Program.BaseImageBuffer, colorBuffer));
            colorBuffer.CopyTo(colors);
            colorBuffer.Dispose();


            Console.WriteLine("Grouping colors");
            // Multi-threaded color grouping
            // Tons of lambda nonsense basically just arrange them by how many times they appear
            // This could likely be done better but it's not runtime overhead so we can excuse it with a "Loading..."
            var transform = colors.AsParallel().GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count()); // This takes a LONG time on big images

            Console.WriteLine("Populating dictionary");
            foreach (var kvp in transform.OrderByDescending(kvp => kvp.Value))
            {
                Pallette[kvp.Key] = kvp.Value;
            }

            totalPixels = Pallette.Values.Sum();

            Active = true;
            Console.WriteLine("Image pallette complete.");
        }

        /// <summary>
        /// Returns the most common color from the pallete
        /// </summary>
        public static float4 MostCommonColor()
        {
            return Active ? Pallette.Keys.First() : RandomUtils.RandomColor(); // If de-activated then just random color
        }

        /// <summary>
        /// Returns a random color from the pallete
        /// </summary>
        public static float4 PalletteColor()
        {
            int index = Program.rand.Next(0, Pallette.Count);
            
            return Pallette.Keys.ElementAt(index);
        }

        /// <summary>
        /// Returns a random color from the pallette based on how frequently it appears
        /// </summary>
        /// <returns></returns>
        public static float4 PalletteColorWeighted()
        {
            int upper = Program.rand.Next(0, totalPixels);
            foreach (var kvp in Pallette)
            {
                if (upper < kvp.Value)
                {
                    return kvp.Key;
                }
                upper -= kvp.Value;
            }

            throw new InvalidOperationException("Weighted color failed");
        }

        /// <summary>
        /// Returns the color at a given pixel co-ordinate in the image
        /// </summary>
        public static float4 ColorAt(this ReadOnlyTexture2D<Rgba32,float4> baseImage, float2 position)
        {
            return baseImage[(int)position.X, (int)position.Y];
        }
    }
}

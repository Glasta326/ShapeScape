using ShapeScape.ImageCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Utils
{
    public static class RandomUtils
    {
        /// <summary>
        /// Gets a float4 with random values ranging from 0-1 in each slot
        /// </summary>
        public static float4 RandomColor()
        {
            if (PalletteCache.Active)
            {
                return PalletteCache.PalletteColor();
            }
            else
            {
                float4 value = new float4((float)Program.rand.NextDouble(),
                (float)Program.rand.NextDouble(),
                (float)Program.rand.NextDouble(),
                (float)Program.rand.NextDouble());
                return value;
            }
        }

        /// <summary>
        /// Gets a float2 coordinate located on the canvas
        /// </summary>
        /// <param name="Tolerance">How much outside the canvas this position is allowed to deviate</param>
        public static float2 RandomCanvasPosition(float Tolerance = 0)
        {
            // the + 1 is because we want to be able to draw right on the boundry of the image, and .Next would return the maxDimension - 1 as an upper limit
            float x = Program.rand.NextFloat(-Tolerance, (float)Program.Dimensions.X + 1f + Tolerance);
            float y = Program.rand.NextFloat(-Tolerance, (float)Program.Dimensions.Y + 1f + Tolerance);
            return new float2(x, y);
        }

        /// <summary>
        /// Returns -1 or 1 randomly
        /// </summary>
        /// <returns></returns>
        public static int Coinflip()
        {
            return Program.rand.Next(0, 2) == 1 ? 1 : -1;
        }


        public static float NextFloat(this Random random, float min, float max)
        {
            return (random.NextSingle() * (max - min)) + min;
        }
    }
}

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
            float4 value = new float4((float)Program.rand.NextDouble(),
                (float)Program.rand.NextDouble(),
                (float)Program.rand.NextDouble(),
                (float)Program.rand.NextDouble());
            return value;
        }
    }
}

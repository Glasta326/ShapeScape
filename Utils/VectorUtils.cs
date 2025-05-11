using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Utils
{
    public static class VectorUtils
    {
        /// <summary>
        /// Rotates a vector around another vector
        /// </summary>
        public static Vector2 RotatedBy(this Vector2 spinningpoint, double radians, Vector2 center = default(Vector2))
        {
            // Taken from source code for TModLoader in Utils.cs line 1470
            float num = (float)Math.Cos(radians);
            float num2 = (float)Math.Sin(radians);
            Vector2 vector = spinningpoint - center;
            Vector2 result = center;
            result.X += vector.X * num - vector.Y * num2;
            result.Y += vector.X * num2 + vector.Y * num;
            return result;
        }
    }
}

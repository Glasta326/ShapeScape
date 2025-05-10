using ShapeScape.Shader;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace ShapeScape.Shapes
{
    /// <summary>
    /// Base class for any shape.
    /// </summary>
    public abstract class BasePolygon
    {
        private int name = Program.rand.Next(0, 100);
        private float2[] _verticies;

        /// <summary>
        /// Array of distinct points on a 2d surface indicating the corners of this shape.
        /// </summary>
        public virtual float2[] Verticies
        {
            get
            {
                return _verticies;
            }
            set
            {
                if (value.Length < 3)
                {
                    throw new Exception("Shape contains less than 3 verticies.");
                }
                _verticies = value;
            }
        }

        /// <summary>
        /// Array of tessels that compose this shape entirely
        /// </summary>
        public virtual Tessel[] Tesselation { get; set; }

        /// <summary>
        /// Color this shape will fill in. Typically random or inherited from parent
        /// </summary>
        public virtual float4 Color { get; set; }

        /// <summary>
        /// A number associated to this shape that indicates how "good" it is.
        /// </summary>
        public virtual float Score { get; set; } = 0f;

        public virtual void CreateChildren(int childcount, int mutationStrength, ref List<BasePolygon> polygons) { }

        public static float2[] ForceUnique(float2[] array, int nudge = 1)
        {
            Vector2[] temp = new Vector2[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                temp[i].X = array[i].X;
                temp[i].Y = array[i].Y;
            }

            // Keep track of the already seen vectors
            var seenVectors = new HashSet<Vector2>();

            for (int i = 0; i < temp.Length; i++)
            {
                // Check if the current vector already exists in the set
                while (seenVectors.Contains(temp[i]))
                {
                    // Slightly nudge the vector (adjust x or y by a small amount)
                    temp[i] = new Vector2(temp[i].X + 1f, temp[i].Y + 1f);
                }

                // Add the new unique vector to the set
                seenVectors.Add(temp[i]);
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i].X = temp[i].X;
                array[i].Y = temp[i].Y;
            }
            return array;
        }

    }
}


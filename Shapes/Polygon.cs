

namespace ShapeScape.Shapes
{
    /// <summary>
    /// Base class for any shape.
    /// </summary>
    public abstract class Polygon
    {
        /// <summary>
        /// Array of distinct points on a 2d surface indicating the corners of this shape.
        /// </summary>
        public virtual float2[] Verticies
        {
            get
            {
                return Verticies;
            }
            set
            {
                if (Verticies.Length < 3)
                {
                    throw new Exception("Shape contains less than 3 verticies.");
                }
                Verticies = value;
            }
        }

        /// <summary>
        /// Color this shape will fill in. Typically random or inherited from parent
        /// </summary>
        public virtual float4 Color { get; set; }

        /// <summary>
        /// A number associated to this shape that indicates how "good" it is.
        /// </summary>
        public virtual float Score { get; set; } = 0f;
    }
}

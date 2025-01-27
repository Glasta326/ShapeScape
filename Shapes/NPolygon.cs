using ShapeScape.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Shapes
{
    /// <summary>
    /// A shape with an arbirtary number of verticies, that do not connect via any rules
    /// </summary>
    public class NPolygon : Polygon
    {
        /// <summary>
        /// Limit on how many verticies an NPolygon can have
        /// </summary>
        const int VLimit = 8;

        /// <summary>
        /// Default constructor, assigns values randomly
        /// </summary>
        public NPolygon()
        {
            this.Verticies = new float2[VLimit];
            this.Color = RandomUtils.RandomColor();
        }

        /// <summary>
        /// Constructor that sets supplied values. Designed for child shapes which inherit values from parents
        /// </summary>
        public NPolygon(float2[] Verticies, float4 color)
        {
            this.Verticies = Verticies;
            this.Color = color;
        }
    }
}

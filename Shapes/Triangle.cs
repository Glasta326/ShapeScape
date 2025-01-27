using ShapeScape.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Shapes
{
    /// <summary>
    /// Triangle class with three vertexes and a color. Final result of tesselation.
    /// </summary>
    public class Triangle : Polygon
    {
        /// <summary>
        /// Default constructor, Assigns random verticies and color.
        /// </summary>
        public Triangle()
        {
            // Three verticies
            this.Verticies = new float2[3];
            this.Color = RandomUtils.RandomColor();

            for (int i = 0; i < 3; i++)
            {
                this.Verticies[i] = RandomUtils.RandomCanvasPosition();
            }
        }

        /// <summary>
        /// Constructor that sets supplied values. Designed for child shapes which inherit values from parents
        /// </summary>
        /// <param name="verticies">Float2 array of [v0, v1 ,v2]</param>
        /// <param name="color">Float4 of (0-1, 0-1, 0-1, 0-1)</param>
        public Triangle(float2[] verticies, float4 color)
        {
            this.Verticies = verticies;
            this.Color = color;
        }

        /// <summary>
        /// Returns this triangle as a Tessel instance. Used to skip Tesselating
        /// </summary>
        public void asTessel()
        {
            throw new NotImplementedException();
        }

        // .v0 .v1 and .v2 extensions for easy vertex access
        #region Shorthand Accessors

        public float2 v0
        {
            get
            {
                return this.Verticies[0];
            }
            set
            {
                this.Verticies[0] = value;
            }
        }
        public float2 v1
        {
            get
            {
                return this.Verticies[1];
            }
            set
            {
                this.Verticies[1] = value;
            }
        }
        public float2 v2
        {
            get
            {
                return this.Verticies[2];
            }
            set
            {
                this.Verticies[2] = value;
            }
        }

        #endregion
    }
}

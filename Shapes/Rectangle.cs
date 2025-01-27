using ShapeScape.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Shapes
{
    /// <summary>
    /// Rectangle class with a Topleft and Dimensions.
    /// </summary>
    public class Rectangle : Polygon
    {
        private float2 dimensions;
        /// <summary>
        /// Default constructor, For creating random rectangles
        /// </summary>
        public Rectangle()
        {
            // 4 verticies
            this.Verticies = new float2[4];
            this.Color = RandomUtils.RandomColor();

            // pick a spot to start the rectangle
            // then pick a random width and height
            // Note : width and height and the remaining 3 verticies can contain negatives
            topLeft = RandomUtils.RandomCanvasPosition();

            dimensions = new float2(Program.rand.Next(-Program.Dimensions.X, Program.Dimensions.X + 1),
                Program.rand.Next(-Program.Dimensions.Y, Program.Dimensions.Y + 1));

            SetVerticies();

            // Note : any reference to direction like "top left" or "bottom right" are kind of just names
            // If the topLeft value is at 800, 800. and the dimensions are -100,-100. then the topLeft value would actually be at the bottom right of the rectangle
        }

        /// <summary>
        /// Constructor with parameters, designed for child shapes
        /// </summary>
        public Rectangle(float2 TopLeft, float2 Dimensions, float4 color)
        {
            topLeft = TopLeft;
            dimensions = Dimensions;
            this.Color = color;

            SetVerticies();
        }

        /// <summary>
        /// Once topleft and dimensions are known, this just sets the four vertex values of the shape
        /// </summary>
        private void SetVerticies()
        {
            // set the top right, bottom left and bottom right corners
            Verticies[1] = new float2(topLeft.X + dimensions.X, topLeft.Y);
            Verticies[2] = new float2(topLeft.X, topLeft.Y + dimensions.Y);
            Verticies[3] = new float2(topLeft.X + dimensions.X, topLeft.Y + dimensions.Y);
        }

        #region Shorthand Accessors

        public float2 topLeft
        {
            get
            {
                return Verticies[0];
            }
            set
            {
                Verticies[0] = value;
            }
        }

        #endregion
    }
}

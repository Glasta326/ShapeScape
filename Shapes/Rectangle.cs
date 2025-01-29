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
        public float2 dimensions;
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
            // 4 verticies
            this.Verticies = new float2[4];

            topLeft = TopLeft;
            dimensions = Dimensions;
            this.Color = color;

            SetVerticies();
        }

        /// <summary>
        /// Creates more rectangles based on this rectangle with similar but different properties
        /// </summary>
        public override void CreateChildren(int childcount, int mutationStrength, ref List<Polygon> polygons)
        {
            for (int i = 0; i < childcount; i++)
            {
                float2 topLeftGenes = this.topLeft;
                float2 dimGenes = this.dimensions;
                float4 colorGenes = this.Color;

                // shuffle around the topLeft a bit
                topLeftGenes.X += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                topLeftGenes.Y += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);

                // Scale the dimensions by a bit
                dimGenes *= 1 + ( RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength) / 200f);

                // modify color
                int channel = Program.rand.Next(0, 4);
                Color[channel] += RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength) / 200f;
                

                polygons.Add(new Rectangle(topLeftGenes, dimGenes, colorGenes));
            }
        }

        /// <summary>
        /// Once topleft and dimensions are known, this just sets the four vertex values of the shape
        /// </summary>
        private void SetVerticies()
        {
            // set the top right, bottom left and bottom right corners
            Verticies[1] = new float2(topLeft.X + dimensions.X + 1, topLeft.Y);
            Verticies[2] = new float2(topLeft.X, topLeft.Y + dimensions.Y + 2);
            Verticies[3] = new float2(topLeft.X + dimensions.X + 3, topLeft.Y + dimensions.Y + 4);
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

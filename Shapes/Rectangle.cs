using ShapeScape.ImageCache;
using ShapeScape.Shader;
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
    public class Rectangle : BasePolygon
    {
        public float2 dimensions;
        /// <summary>
        /// Default constructor, For creating random rectangles
        /// </summary>
        public Rectangle()
        {
            // Four verticies compose a rectangle
            this.Verticies = new float2[4];
            this.Color = RandomUtils.RandomColor();

            // Set the position this rectangle is drawn relative to
            topLeft = RandomUtils.RandomCanvasPosition();
            // The rectangle can have width or height ranging half the size of the canvas each way
            dimensions = new float2(
                Program.rand.NextFloat(-Program.Dimensions.X / 2f, Program.Dimensions.X / 2f),
                Program.rand.NextFloat(-Program.Dimensions.Y / 2f, Program.Dimensions.Y / 2f)
            );





            // This vertex order makes more sense for triangulating
            this.Verticies[1] = new float2(topLeft.X + 0, topLeft.Y + dimensions.Y); // Bottom left
            this.Verticies[2] = new float2(topLeft.X + dimensions.X, topLeft.Y + 0); // Top right
            this.Verticies[3] = new float2(topLeft.X + dimensions.X, topLeft.Y + dimensions.Y); // Bottom right

            // Manually tesselate because the Tesselator really really dislikes rectangles
            this.Tesselation = new Tessel[2];
            Tesselation[0] = new Tessel(this.Verticies[0], this.Verticies[1], this.Verticies[2], this.Color);
            Tesselation[1] = new Tessel(this.Verticies[1], this.Verticies[2], this.Verticies[3], this.Color);
        }

        /// <summary>
        /// Constructor with parameters, designed for child shapes
        /// </summary>
        public Rectangle(float2 TopLeft, float2 Dimensions, float4 color)
        {
            // 4 verticies
            this.Verticies = new float2[4];
            this.Color = new float4(color.X, color.Y, color.Z, color.A);

            this.topLeft = new float2(TopLeft.X, TopLeft.Y);
            this.dimensions = new float2(Dimensions.X, Dimensions.Y);

            // This vertex order makes more sense for triangulating
            this.Verticies[1] = new float2(topLeft.X + 0, topLeft.Y + dimensions.Y); // Bottom left
            this.Verticies[2] = new float2(topLeft.X + dimensions.X, topLeft.Y + 0); // Top right
            this.Verticies[3] = new float2(topLeft.X + dimensions.X, topLeft.Y + dimensions.Y); // Bottom right

            // Manually tesselate because the Tesselator really really dislikes rectangles
            this.Tesselation = new Tessel[2];
            Tesselation[0] = new Tessel(this.Verticies[0], this.Verticies[1], this.Verticies[2], this.Color);
            Tesselation[1] = new Tessel(this.Verticies[1], this.Verticies[2], this.Verticies[3], this.Color);
        }

        /// <summary>
        /// Creates more rectangles based on this rectangle with similar but different properties
        /// </summary>
        public override void CreateChildren(int childcount, int mutationStrength, ref List<BasePolygon> polygons)
        {
            for (int i = 0; i < childcount; i++)
            {

                float2 topLeftGenes = new float2(this.topLeft.X, this.topLeft.Y);
                float2 dimGenes = new float2(this.dimensions.X, this.dimensions.Y);
                float4 colorGenes = new float4(Color.X, Color.Y, Color.Z, Color.A);
               
                
                // shuffle around the topLeft a bit
                topLeftGenes.X += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                topLeftGenes.Y += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);


                // modify color
                int channel = Program.rand.Next(0, 4);
                float diff = RandomUtils.Coinflip() * (float)Program.rand.Next(0, mutationStrength) / 200f;
                switch (channel)
                {
                    case 0:
                        colorGenes.X += diff;
                        break;
                    case 1:
                        colorGenes.Y += diff;
                        break;
                    case 2:
                        colorGenes.Z += diff;
                        break;
                    case 3:
                        colorGenes.W += diff;
                        break;
                }


                polygons.Add(new Rectangle(topLeftGenes, dimGenes, colorGenes));
            }
        }

        #region Shorthand Accessors

        /// <summary>
        /// Shorthand to Verticies[0]
        /// </summary>
        public float2 topLeft
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

        #endregion
    }
}

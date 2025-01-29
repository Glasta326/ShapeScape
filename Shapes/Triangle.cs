using ComputeSharp;
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
    /// Tessel class with three vertexes and a color. Final result of tesselation.
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
            this.Verticies = ForceUnique(this.Verticies);
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

            this.Verticies = ForceUnique(this.Verticies);
        }

        /// <summary>
        /// Creates more triangles based on this triangle with similar but different properties
        /// </summary>
        public override void CreateChildren(int childcount, int mutationStrength, ref List<Polygon> polygons)
        {
            for (int i = 0; i < childcount; i++)
            {
                float2[] vertGenes = this.Verticies;
                float4 colorGenes = this.Color;
                
                // Pick a random verticie and mess with it a bit
                int vert = Program.rand.Next(0, 3);
                vertGenes[vert].X += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                vertGenes[vert].Y += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                // Pick another one and do the same (could be the same vertex)
                vert = Program.rand.Next(0, 3);
                vertGenes[vert].X += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                vertGenes[vert].Y += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);

                // modify color
                int channel = Program.rand.Next(0, 4);
                Color[channel] += RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength)/200f;
                

                polygons.Add(new Triangle(vertGenes, colorGenes));
            }
        }

        /// <summary>
        /// Returns this triangle as a Tessel instance. Used to skip Tesselating
        /// </summary>
        public Tessel asTessel()
        {
            return new Tessel(v0, v1, v2, Color);
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

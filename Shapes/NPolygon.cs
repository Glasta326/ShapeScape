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
            int verts = Program.rand.Next(4, VLimit + 1);
            this.Verticies = new float2[verts];

            this.Color = RandomUtils.RandomColor();

            for (int i = 0; i < verts; i++)
            {
                this.Verticies[i] = RandomUtils.RandomCanvasPosition();
            }

            this.Verticies = ForceUnique(this.Verticies);
        }

        /// <summary>
        /// Constructor that sets supplied values. Designed for child shapes which inherit values from parents
        /// </summary>
        public NPolygon(float2[] verticies, float4 color)
        {
            this.Verticies = new float2[verticies.Length];
            for (int i = 0; i < verticies.Length; i++)
            {
                this.Verticies[i] = verticies[i];
            }

            this.Color = new float4(color.X, color.Y, color.Z, color.A);

            this.Verticies = ForceUnique(this.Verticies);
        }

        /// <summary>
        /// Creates more NPolygons based on this NPolygons with similar but different properties
        /// </summary>
        public override void CreateChildren(int childcount, int mutationStrength, ref List<Polygon> polygons)
        {
            for (int i = 0; i < childcount; i++)
            {
                // Create a new array to avoid mutating parent's Verticies
                float2[] vertGenes = new float2[this.Verticies.Length];
                for (int j = 0; j < this.Verticies.Length; j++)
                {
                    vertGenes[j] = this.Verticies[j];
                }
                float4 colorGenes = new float4(Color.X, Color.Y, Color.Z, Color.A);

                // Pick a random verticie and mess with it a bit
                int vert = Program.rand.Next(0, vertGenes.Length);
                vertGenes[vert].X += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                vertGenes[vert].Y += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);

                // Keep rolling for another mutation untill the gambling train stops
                while (RandomUtils.Coinflip() == 1)
                {
                    vert = Program.rand.Next(0, vertGenes.Length);
                    vertGenes[vert].X += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                    vertGenes[vert].Y += (float)Program.rand.NextDouble() * RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength);
                }

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


                polygons.Add(new NPolygon(vertGenes, colorGenes));
            }
        }
    }
}

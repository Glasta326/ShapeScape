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
        public NPolygon(float2[] Verticies, float4 color)
        {
            this.Verticies = Verticies;
            this.Color = color;

            this.Verticies = ForceUnique(this.Verticies);
        }

        /// <summary>
        /// Creates more NPolygons based on this NPolygons with similar but different properties
        /// </summary>
        public override void CreateChildren(int childcount, int mutationStrength, ref List<Polygon> polygons)
        {
            for (int i = 0; i < childcount; i++)
            {
                float2[] vertGenes = this.Verticies;
                float4 colorGenes = this.Color;

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
                Color[channel] += RandomUtils.Coinflip() * Program.rand.Next(0, mutationStrength) / 200f;


                polygons.Add(new NPolygon(vertGenes, colorGenes));
            }
        }
    }
}

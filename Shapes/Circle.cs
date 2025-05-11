using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ShapeScape.Utils;
using ShapeScape.Shader;

namespace ShapeScape.Shapes
{
    internal class Circle : BasePolygon
    {
        private Vector2 _Center => new Vector2(Center.X, Center.Y);
        public float2 Center { get; set; }
        public float Radius {  get; set; }
        public const int divs = 32;
        public Circle()
        {
            this.Verticies = new float2[divs];
            this.Tesselation = new Tessel[divs];

            this.Center = RandomUtils.RandomCanvasPosition();
            // Radius from 0 - average of the two dimensions of the image (then halved again)
            this.Radius = Program.rand.NextFloat(0f, ((float)Program.Dimensions.X + (float)Program.Dimensions.Y) / 4f);
            this.Color = RandomUtils.RandomColor();


            for (int i = 0; i < divs; i++)
            {
                float angle = ((2f * MathF.PI) / divs) * i;

                // I do -Radius because i prefer the first one being north of the center in my mind but the canvas coordinates make Negative y go north
                Vector2 point = this._Center + new Vector2(0, -Radius).RotatedBy(angle);
                this.Verticies[i] = new float2(point.X, point.Y);
            }

            for (int i = 0; i < divs; i++)
            {
                float2 v0;
                float2 v1;
                float2 v2;

                v0 = this.Verticies[i];
                v1 = this.Center;
                if (i < divs - 1)
                {
                    v2 = this.Verticies[i + 1];
                }
                else
                {
                    v2 = this.Verticies[0];
                }
                this.Tesselation[i] = new Tessel(v0, v1, v2,this.Color);
            }
        }


    }
}

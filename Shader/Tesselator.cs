using ComputeSharp;
using MIConvexHull;
using ShapeScape.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = ShapeScape.Shapes.Rectangle;

namespace ShapeScape.Shader
{
    public static class Tesselator
    {
        /// <summary>
        /// Subdivides a <see cref="BasePolygon"/> into many <see cref="Tessel"/> objects which are then rendered and scored with the GPU
        /// </summary>
        public static Tessel[] TessellatePolygon(BasePolygon shape)
        {
            // Pre-Computed tessel skipping
            // NPolygon is broken right now and it's actually faster to pre-compute tesselations anyway so currently the tesselator is unused
            if (shape is Triangle t)
            {
                return [t.asTessel()];
            }
            if (shape is Rectangle r)
            {
                return r.Tesselation;
            }
            if (shape is Circle c)
            {
                return c.Tesselation;
            }
            // Unused
            else
            {
                List<Tessel> result = new List<Tessel>();

                float2[] verticies = shape.Verticies;

                // Convert Float2 points to Float2Vertex objects
                var vertices = verticies.Select(p => new Float2Vertex(p)).ToArray();


                // Perform Delaunay triangulation
                var triangulation = DelaunayTriangulation<Float2Vertex, Float2Cell>.Create(vertices, 2);
                // Print triangles
                int i = 0;
                foreach (var triangle in triangulation.Cells)
                {
                    var v0 = triangle.Vertices[0].Point;
                    var v1 = triangle.Vertices[1].Point;
                    var v2 = triangle.Vertices[2].Point;
                    result.Add(new Tessel(v0, v1, v2, shape.Color));

                    i++;
                }
                return result.ToArray();
            }
        }

        // Triangle cell representation
        private class Float2Cell : TriangulationCell<Float2Vertex, Float2Cell>
        {
        }

        private class Float2Vertex : IVertex
        {
            public Float2 Point { get; }

            public double[] Position => new[] { (double)Point.X, (double)Point.Y };

            public Float2Vertex(Float2 point)
            {
                Point = point;
            }

            public override string ToString() => $"({Point.X}, {Point.Y})";
        }
    }
}

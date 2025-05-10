using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Shader
{
    /// <summary>
    /// Extremely basic triangle struct for the tesselator and shaders to use
    /// </summary>
    public struct Tessel(float2 Vertex0, float2 Vertex1, float2 Vertex2, float4 Color)
    {
        public float2 v0 = new float2(Vertex0.X, Vertex0.Y);
        public float2 v1 = new float2(Vertex1.X, Vertex1.Y);
        public float2 v2 = new float2(Vertex2.X, Vertex2.Y);
        public float4 color = Color;
        public ComputeSharp.Int1x1 nothing = new int1x1(1); // used to idenify real tessels from the phantom tessels generated in the tesselation unzipping
    }
}

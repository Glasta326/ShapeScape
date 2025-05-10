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
        public int2 v0 = new int2((int)Vertex0.X, (int)Vertex0.Y);
        public int2 v1 = new int2((int)Vertex1.X, (int)Vertex1.Y);
        public int2 v2 = new int2((int)Vertex2.X, (int)Vertex2.Y);
        public float4 color = Color;
        public ComputeSharp.Int1x1 nothing = new int1x1(1); // used to idenify real tessels from the phantom tessels generated in the tesselation unzipping
    }
}

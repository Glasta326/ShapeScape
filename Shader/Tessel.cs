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
        public float2 v0 = Vertex0;
        public float2 v1 = Vertex1;
        public float2 v2 = Vertex2;
        public float4 color = Color;
        public ComputeSharp.Int1x1 nothing = new int1x1(1); // used to idenify real tessels from the phantom tessels generated in the te
    }
}

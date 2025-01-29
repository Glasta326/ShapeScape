using ComputeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Shader.Shaders
{
    public partial class Shaders
    {
        /// <summary>
        /// Draws tessels onto the <see cref="constructorCopy"/> and runs a color difference test to get a numerical score for how "good" a polygon is
        /// </summary>
        /// <param name="baseImage">The target image the program is trying to reconstruct</param>
        /// <param name="constructorImage">The canvas that shapes are drawn onto to make progress</param>
        /// <param name="constructorCopy">The canvas used to test out new shapes, identical to <paramref name="constructorImage"/></param>
        /// <param name="tesselation">The provided tessels that make up the polygon we are scoring</param>
        /// <param name="score">The score per pixel this shape resulted with</param>
        [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
        [GeneratedComputeShaderDescriptor]
        public readonly partial struct DrawAndScore(ReadOnlyTexture2D<Rgba32, float4> baseImage, ReadWriteTexture2D<Rgba32, float4> constructorImage, ReadWriteTexture2D<Rgba32, float4> constructorCopy, ReadOnlyBuffer<Tessel> tesselation, ReadWriteBuffer<float> score) : IComputeShader
        {
            public void Execute()
            {
                int x = ThreadIds.X;
                int y = ThreadIds.Y;

                // create a blank slate of whatever the constructor canvas looks like
                constructorCopy[x, y] = constructorImage[x, y];
                

                // draw the tesselations onto the construcor 
                float2 pixel = new float2(x, y);
                for (int i = 0; i < tesselation.Length; i++)
                {
                    Tessel t = tesselation[i];
                    if (IsPointInTriangle(pixel, t.v0, t.v1, t.v2))
                    {
                        float4 destColor = constructorCopy[x, y].RGBA;

                        float r = (t.color.R * t.color.A) + (destColor.R * (1 - t.color.A));
                        float g = (t.color.G * t.color.A) + (destColor.G * (1 - t.color.A));
                        float b = (t.color.B * t.color.A) + (destColor.B * (1 - t.color.A));
                        float a = t.color.A + (destColor.A * (1 - t.color.A));

                        constructorCopy[x,y].RGBA = new float4(r, g, b, a);
                    }
                }

                // find color difference between constructor copy and base image
                float diff = Hlsl.Distance(constructorCopy[x, y].RGBA, baseImage[x, y].RGBA);

                score[baseImage.Width * y + x] = diff;
            }

            static bool IsPointInTriangle(float2 p, float2 v0, float2 v1, float2 v2)
            {
                float area0 = Area(p, v0, v1);
                float area1 = Area(p, v1, v2);
                float area2 = Area(p, v2, v0);

                bool hasNeg = (area0 < 0) || (area1 < 0) || (area2 < 0);
                bool hasPos = (area0 > 0) || (area1 > 0) || (area2 > 0);

                return !(hasNeg && hasPos);
            }

            static float Area(float2 a, float2 b, float2 c)
            {
                return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            }
        }

        /// <summary>
        /// Draws a given tesselation onto the constructor canvas for usage in the next evolution cycle
        /// </summary>
        /// <param name="constructorImage">The canvas that shapes are drawn onto to make progress</param>
        /// <param name="tesselation">The provided tessels that make up the polygon we are drawing</param>
        [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
        [GeneratedComputeShaderDescriptor]
        public readonly partial struct DrawToConstructor(ReadWriteTexture2D<Rgba32, float4> constructorImage, ReadOnlyBuffer<Tessel> tesselation) : IComputeShader
        {
            public void Execute()
            {
                int x = ThreadIds.X;
                int y = ThreadIds.Y;

                for (int i = 0; i < tesselation.Length; i++)
                {
                    Tessel t = tesselation[i];
                    if (IsPointInTriangle(new float2(x, y), t.v0, t.v1, t.v2))
                    {
                        float4 destColor = constructorImage[x, y].RGBA;

                        float r = (t.color.R * t.color.A) + (destColor.R * (1 - t.color.A));
                        float g = (t.color.G * t.color.A) + (destColor.G * (1 - t.color.A));
                        float b = (t.color.B * t.color.A) + (destColor.B * (1 - t.color.A));
                        float a = t.color.A + (destColor.A * (1 - t.color.A));

                        constructorImage[x, y].RGBA = new float4(r, g, b, a);
                    }
                }
            }


            static bool IsPointInTriangle(float2 p, float2 v0, float2 v1, float2 v2)
            {
                float area0 = Area(p, v0, v1);
                float area1 = Area(p, v1, v2);
                float area2 = Area(p, v2, v0);

                bool hasNeg = (area0 < 0) || (area1 < 0) || (area2 < 0);
                bool hasPos = (area0 > 0) || (area1 > 0) || (area2 > 0);

                return !(hasNeg && hasPos);
            }

            static float Area(float2 a, float2 b, float2 c)
            {
                return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            }
        }

        /// <summary>
        /// Draws a given tesselation onto the constructor canvas for usage in the next evolution cycle
        /// </summary>
        /// <param name="constructorImage">The canvas that shapes are drawn onto to make progress</param>
        /// <param name="tesselation">The provided tessels that make up the polygon we are drawing</param>
        [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
        [GeneratedComputeShaderDescriptor]
        public readonly partial struct FillColor(ReadWriteTexture2D<Rgba32, float4> constructorImage) : IComputeShader
        {
            public void Execute()
            {
                int x = ThreadIds.X;
                int y = ThreadIds.Y;

                constructorImage[x, y].RGBA = new float4(0f, 0f, 0f, 1);
            }


        }
    }
}

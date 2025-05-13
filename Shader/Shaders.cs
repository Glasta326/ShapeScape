using ComputeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace ShapeScape.Shader.Shaders
{
    public partial class Shaders
    {
        /// <summary>
        /// Calculates the color difference and returns a numerical score for that pixel
        /// </summary>
        /// <param name="baseImage">The target image the program is trying to reconstruct</param>
        /// <param name="constructorImage">The canvas that shapes are drawn onto to make progress</param>
        /// <param name="tesselation">The provided tessels that make up the polygon we are scoring</param>
        /// <param name="score">The score per pixel this shape resulted with</param>
        [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
        [GeneratedComputeShaderDescriptor]
        public readonly partial struct DrawAndScore(ReadOnlyTexture2D<Rgba32, float4> baseImage, ReadWriteTexture2D<Rgba32, float4> constructorImage, ReadOnlyBuffer<Tessel> tesselation, ReadWriteBuffer<float> score) : IComputeShader
        {
            public void Execute()
            {
                int x = ThreadIds.X;
                int y = ThreadIds.Y;
                float4 pixel = constructorImage[x, y];

                // draw the tesselations onto the construcor 
                float2 coord = new float2(x, y);
                for (int i = 0; i < tesselation.Length; i++)
                {
                    Tessel t = tesselation[i];
                    if (IsPointInTriangle(coord, t.v0, t.v1, t.v2))
                    {
                        float4 destColor = constructorImage[x, y].RGBA;

                        float r = (t.color.R * t.color.A) + (destColor.R * (1 - t.color.A));
                        float g = (t.color.G * t.color.A) + (destColor.G * (1 - t.color.A));
                        float b = (t.color.B * t.color.A) + (destColor.B * (1 - t.color.A));
                        float a = t.color.A + (destColor.A * (1 - t.color.A));

                        pixel = new float4(r, g, b, a);
                    }
                }

                // find color difference between constructor copy and base image
                float diff = Hlsl.Distance(pixel.RGBA, baseImage[x, y].RGBA);

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
        /// Draws a given tesselation onto the provided canvas for usage in the next evolution cycle
        /// </summary>
        /// <param name="texture">The canvas that shapes are drawn onto to make progress</param>
        /// <param name="tesselation">The provided tessels that make up the polygon we are drawing</param>
        [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
        [GeneratedComputeShaderDescriptor]
        public readonly partial struct DrawToTexture(ReadWriteTexture2D<Rgba32, float4> texture, ReadOnlyBuffer<Tessel> tesselation) : IComputeShader
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
                        float4 destColor = texture[x, y].RGBA;

                        float r = (t.color.R * t.color.A) + (destColor.R * (1 - t.color.A));
                        float g = (t.color.G * t.color.A) + (destColor.G * (1 - t.color.A));
                        float b = (t.color.B * t.color.A) + (destColor.B * (1 - t.color.A));
                        float a = t.color.A + (destColor.A * (1 - t.color.A));

                        texture[x, y].RGBA = new float4(r, g, b, a);
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
        public readonly partial struct FillColor(ReadWriteTexture2D<Rgba32, float4> constructorImage, float4 color) : IComputeShader
        {
            public void Execute()
            {
                int x = ThreadIds.X;
                int y = ThreadIds.Y;

                constructorImage[x, y].RGBA = color;
            }
        }

        // TODO : why the fuck does this act differently to the other function?????
        // It clearly kind of works but something is fucked with the alpha channel? and i cant tell
        /// <summary>
        /// Takes in a 2D map of tessels, where each x is a shape id and y is the list of tessels that compose that shape. it draws all shapes at once to get a score for that shape and returns a score map of each tessel
        /// </summary>
        [ThreadGroupSize(DefaultThreadGroupSizes.X)]
        [GeneratedComputeShaderDescriptor]
        public readonly partial struct ScoreAllShapes(
            // Hate. Let me tell you how much I've come to hate this since I began this project.
            // There are 387.44 million lines of code in utility functions that fill my codebase.
            // If the word 'hate' was commented on every single line of those hundreds of millions of lines,
            // it would not equal one one-billionth of the hate I feel for this function at this micro-instant.
            // For this.
            // Hate.
            // Hate. 
            ReadOnlyBuffer<Tessel> t0 ,
            ReadOnlyBuffer<Tessel> t1,
            //ReadOnlyBuffer<Tessel> t2,
            //ReadOnlyBuffer<Tessel> t3,
            //ReadOnlyBuffer<Tessel> t4,
            //ReadOnlyBuffer<Tessel> t5,
            //ReadOnlyBuffer<Tessel> t6,
            //ReadOnlyBuffer<Tessel> t7,
            //ReadOnlyBuffer<Tessel> t8,
            //ReadOnlyBuffer<Tessel> t9,
            //ReadOnlyBuffer<Tessel> t10,
            //ReadOnlyBuffer<Tessel> t11,
            ReadOnlyTexture2D<Rgba32, float4> baseImage, ReadWriteTexture2D<Rgba32, float4> constructorImage, ReadWriteBuffer<float> scores) : IComputeShader
        {
            
            public void Execute()
            {
                // You never really do appreicate something untill it gets taken away from you, huh
                int x = ThreadIds.X;

                Tessel tessel0 = t0[x];
                Tessel tessel1 = t1[x];
                //Tessel tessel2 = t2[x];
                //Tessel tessel3 = t3[x];
                //Tessel tessel4 = t4[x];
                //Tessel tessel5 = t5[x];
                //Tessel tessel6 = t6[x];
                //Tessel tessel7 = t7[x];
                //Tessel tessel8 = t8[x];
                //Tessel tessel9 = t9[x];
                //Tessel tessel10 = t10[x];
                //Tessel tessel11 = t11[x];
                // all tessels should be same color

                float scoreSum = 0f;

                for (int i = 0; i < constructorImage.Width; i++)
                {
                    for (int j = 0; j < constructorImage.Height; j++)
                    {
                        // Calculate RGBA color of the pixel on the constructor image would be if we drew this color to it
                        // (Color blending)
                        float4 destColor = constructorImage[i, j];
                        float r = (tessel0.color.R * tessel0.color.A) + (destColor.R * (1 - tessel0.color.A));
                        float g = (tessel0.color.G * tessel0.color.A) + (destColor.G * (1 - tessel0.color.A));
                        float b = (tessel0.color.B * tessel0.color.A) + (destColor.B * (1 - tessel0.color.A));
                        float a = tessel0.color.A + (destColor.A * (1 - tessel0.color.A));



                        // Default value is the color of the pixel on the constructor image.
                        // If this pixel is outside any of the tessels, then we don't draw to it, so the color remains unchanged
                        float4 Pixel = constructorImage[i, j];

                        // If this pixel is inside any of the tessels that compose this shape, that means the pixel is inside our shape bounds and we need to "draw it on the canvas"
                        // Even though we dont actually bother drawing to the canvas because there's no point in doing so
                        if (IsPointInTriangle(tessel0, new float2(i,j)) && tessel0.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel1, new float2(i, j)) && tessel1.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        /*
                        if (IsPointInTriangle(tessel2, new float2(i, j)) && tessel2.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel3, new float2(i, j)) && tessel3.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel4, new float2(i, j)) && tessel4.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel5, new float2(i, j)) && tessel5.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel6, new float2(i, j)) && tessel6.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel7, new float2(i, j)) && tessel7.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel8, new float2(i, j)) && tessel8.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel9, new float2(i, j)) && tessel9.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel10, new float2(i, j)) && tessel10.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        if (IsPointInTriangle(tessel11, new float2(i, j)) && tessel11.nothing.M11 == 1)
                        {
                            Pixel = new float4(r, g, b, a);
                        }
                        */
                        // Calculate color difference for this pixel.
                        // If it wasn't inside any of the tessel boundaries ( so if it wasnt in our shape ) then it wasn't draw, so pixel color is identical to canvas color, so color difference will be zero
                        // We compare to baseimage and not constructor because that's what we're trying to reconstruct, who cares what this shape drawing does to the constructor if it brings us closer to the base image we want it
                        float diff = Hlsl.Distance(Pixel.RGBA, baseImage[i, j].RGBA);
                        scoreSum += diff;
                    }
                }

                scores[x] = scoreSum; // Average diff
            }

            

            static bool IsPointInTriangle(Tessel t, float2 p)
            {
                float2 v0 = t.v0;
                float2 v1 = t.v1;
                float2 v2 = t.v2;

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
        /// Converts the base image into an array of colors
        /// </summary>
        /// <param name="baseImage"></param>
        /// <param name="palletteMap"></param>
        [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
        [GeneratedComputeShaderDescriptor]
        public readonly partial struct Pallettise(ReadOnlyTexture2D<Rgba32, float4> baseImage, ReadWriteBuffer<float4> palletteMap) : IComputeShader
        {
            public void Execute()
            {
                int x = ThreadIds.X;
                int y = ThreadIds.Y;

                palletteMap[baseImage.Width * y + x] = baseImage[x, y];
            }
        }
    }
}

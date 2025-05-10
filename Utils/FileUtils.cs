using ComputeSharp;
using System.IO;

namespace ShapeScape.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// The working directory all files are read from and output to
        /// </summary>
        public static string WorkingDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "WorkingDirectory");



        /// <summary>
        /// Saves a texture to the correct folder automatically
        /// </summary>
        public static void SaveImage(this ReadWriteTexture2D<Rgba32, float4> texture ,string Filename)
        {
            texture.Save(Path.Combine(WorkingDirectory, Filename));
        }

        /// <summary>
        /// Saves a texture to the correct folder automatically
        /// </summary>
        public static void SaveImage(this ReadOnlyTexture2D<Rgba32, float4> texture, string Filename)
        {
            texture.Save(Path.Combine(WorkingDirectory, Filename));
        }
    }
}

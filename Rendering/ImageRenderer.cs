using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Rendering
{
    /// <summary>
    /// Display window to show the image being generated
    /// </summary>
    public class ImageRenderer : Form
    {
        private static ImageRenderer Instance;
        private PictureBox _PictureBox; // "PictureBox" overlaps with the type name so good enough

        private ImageRenderer()
        {
            this.Text = "Image Render";

            // Not too big, not too small 
            this.Width = 800;
            this.Height = 600;

            _PictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            this.Controls.Add(_PictureBox);

            // Reset instance when form iwndow gets closed
            this.FormClosed += (sender, eventArgs) => Instance = null;
        }

        /// <summary>
        /// Updates the <see cref="ImageRenderer"/> with the provided image.<br/>
        /// Automatically creates an instance if it does not already exist
        /// </summary>
        /// <param name="image"></param>
        public static void Update(Image image)
        {
            // If we dont exist yet, create ourselves, and set image
            if (Instance == null || Instance.IsDisposed)
            {
                Instance = new ImageRenderer();
                Instance._PictureBox.Image = image;

                var formThread = new Thread(() =>
                {
                    Application.EnableVisualStyles();
                    Application.Run(Instance);
                });

                formThread.SetApartmentState(ApartmentState.STA);
                formThread.Start();
            }
            // If we do already exist, replace old image with new one
            else
            {
                // .Invoke because UI is run on seperate thread
                // Lambda is a bit funky but this should work
                Instance.Invoke((MethodInvoker)(() =>
                {
                    Instance._PictureBox.Image = image;
                }));
            }
        }
    }
}

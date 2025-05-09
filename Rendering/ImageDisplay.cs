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
    public class DisplayForm : Form
    {
        private static DisplayForm _instance;
        private PictureBox pictureBox;

        private DisplayForm()
        {
            this.Text = "Image Viewer";
            this.Width = 800;
            this.Height = 600;

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            this.Controls.Add(pictureBox);
            this.FormClosed += (s, e) => _instance = null; // Reset instance on close
        }

        public static void Display(Image image)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new DisplayForm();
                _instance.pictureBox.Image = image;

                // Run on a new thread so we don't block the main loop
                var formThread = new System.Threading.Thread(() =>
                {
                    Application.EnableVisualStyles();
                    Application.Run(_instance);
                });

                formThread.SetApartmentState(System.Threading.ApartmentState.STA);
                formThread.Start();
            }
            else
            {
                // Just update the image
                _instance.Invoke((MethodInvoker)(() =>
                {
                    _instance.pictureBox.Image = image;
                }));
            }
        }
    }
}

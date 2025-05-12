using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShapeScape.Forms
{
    public partial class UserInputForm : Form
    {
        public string Filename { get; private set; }
        public int? Seed => TryParseNullable(SeedText.Text);
        public int? DownscaleFactor => TryParseNullable(ScaleText.Text);
        public int? TotalShapes => TryParseNullable(TotalShapesText.Text);
        public int? ShapePopulation => TryParseNullable(ShapePopulationText.Text);
        public int? SurvivalThreshold => TryParseNullable(SurvivalThresholdText.Text);
        public int? EvolutionCycles => TryParseNullable(EvolutionCyclesText.Text);
        public int? MutationStrength => TryParseNullable(MutationStrengthText.Text);
        public bool Palletise => PalletiseBox.Checked;

        public UserInputForm()
        {
            InitializeComponent();
        }

        private void OnClickBrowseFileButton(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    TxtFilename.Text = dialog.FileName;
                }
            }
        }

        private void OnClickOkButton(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFilename.Text))
            {
                MessageBox.Show("Please select a file.");
                return;
            }

            Filename = TxtFilename.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// <see cref="int.TryParse(string?, out int)"/> but able to return null values used for default settings
        /// </summary>
        private int? TryParseNullable(string input)
        {
            if (int.TryParse(input, out int val))
                return val;
            return null;
        }
    }
}

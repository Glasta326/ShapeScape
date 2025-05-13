namespace ShapeScape.Forms
{
    partial class UserInputForm
    {
        // UI Elements
        private TextBox TxtFilename;
        private Button FileBrowseButton;
        private TextBox SeedText;
        private TextBox ScaleText;
        private TextBox TotalShapesText;
        private TextBox ShapePopulationText;
        private TextBox SurvivalThresholdText;
        private TextBox EvolutionCyclesText;
        private TextBox MutationStrengthText;
        private CheckBox PalletiseBox;
        private Button OkButton;

        private void InitializeComponent()
        {
            // I'd love to make this static but keeping everything encased in this one instance class means i could re-use it if i added some way to change settings mid-generation
            int y = 5;

            // Special buttons
            Label youCanLeaveBlankForDefault = new Label
            {
                Left = 10,
                Top = y,
                Text = "Leave boxes blank for default values",
                ForeColor = Color.Green,
                AutoSize = true
            };
            this.Controls.Add(youCanLeaveBlankForDefault);
            y += 20;

            Label theseAreTheDefaults = new Label
            {
                Left = 10,
                Top = y,
                Text = "Values in [] show default values",
                ForeColor = Color.Green,
                AutoSize = true
            };
            this.Controls.Add(theseAreTheDefaults);
            y += 20;

            TxtFilename = new TextBox 
            { 
                Left = 10, Top = y, Width = 250 
            };
            y += 0;

            FileBrowseButton = new Button 
            {
                Left = 270, Top = y, Text = "Browse..." 
            };
            FileBrowseButton.Click += OnClickBrowseFileButton;
            y += 30;

            SeedText = AddLabeledInput("Seed [Random]:", ref y);
            ScaleText = AddLabeledInput("Downscale factor [20]:", ref y);
            TotalShapesText = AddLabeledInput("Total Shapes [2048]:", ref y);
            ShapePopulationText = AddLabeledInput("Shape Population [2500]:", ref y);
            SurvivalThresholdText = AddLabeledInput("Survival Threshold [100]:", ref y);
            EvolutionCyclesText = AddLabeledInput("Evolution Cycles [6]:", ref y);
            MutationStrengthText = AddLabeledInput("Mutation Strength [60]:", ref y);

            PalletiseBox = new CheckBox { Left = 10, Top = y, Text = "Enable Palletising (Minor startup overhead)", AutoSize = true };
            y += 30;

            OkButton = new Button 
            {
                Left = 10, Top = y, Text = "OK", Width = 80 
            };
            OkButton.Click += OnClickOkButton;

            this.Controls.AddRange(new Control[] 
            {
                TxtFilename, FileBrowseButton, SeedText, TotalShapesText,
                ShapePopulationText, SurvivalThresholdText, EvolutionCyclesText,
                MutationStrengthText, PalletiseBox, OkButton
            });

            this.Text = "User Settings";
            this.ClientSize = new System.Drawing.Size(400, y + 50);
        }

        /// <summary>
        /// Adds a string input box with the given name
        /// </summary>
        /// <param name="label">The name of the input area presented to the user</param>
        private TextBox AddLabeledInput(string label, ref int y)
        {
            Label lbl = new Label { Left = 10, Top = y, Text = label, Width = 150 };
            TextBox txt = new TextBox { Left = 160, Top = y, Width = 180 };
            y += 30;
            this.Controls.Add(lbl);
            this.Controls.Add(txt);
            return txt;
        }
    }
}
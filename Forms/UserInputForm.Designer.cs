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
            // Special buttons
            Label youCanLeaveBlankForDefault = new Label
            {
                Left = 10,
                Top = 5,
                Text = "Leave boxes blank for default values",
                ForeColor = Color.Green,
                AutoSize = true
            };
            this.Controls.Add(youCanLeaveBlankForDefault);
            TxtFilename = new TextBox 
            { 
                Left = 10, Top = 30, Width = 250 
            };
            FileBrowseButton = new Button 
            {
                Left = 270, Top = 30, Text = "Browse..." 
            };
            FileBrowseButton.Click += OnClickBrowseFileButton;

            // I'd love to make this static but keeping everything encased in this one instance class means i could re-use it if i added some way to change settings mid-generation
            int y = 60;

            SeedText = AddLabeledInput("Seed:", ref y);
            ScaleText = AddLabeledInput("Downscale factor:", ref y);
            TotalShapesText = AddLabeledInput("Total Shapes:", ref y);
            ShapePopulationText = AddLabeledInput("Shape Population:", ref y);
            SurvivalThresholdText = AddLabeledInput("Survival Threshold:", ref y);
            EvolutionCyclesText = AddLabeledInput("Evolution Cycles:", ref y);
            MutationStrengthText = AddLabeledInput("Mutation Strength:", ref y);

            PalletiseBox = new CheckBox { Left = 10, Top = y, Text = "Enable Palletising (Heavy startup overhead!)", AutoSize = true };
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
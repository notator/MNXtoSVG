using MNX.Globals;
using Moritz.Symbols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

namespace MNX.Main
{
    public partial class MNX_MainForm1 : Form
    {
        private readonly List<Tuple<string, string>> _MNX_Form1Data_Paths = new List<Tuple<string, string>>();
        private bool _settingsHaveChanged = false;
        private int _numberOfMeasures; // the number of measures in the currently loaded MNX file
        private MNX.Common.MNX mnx = null;

        public MNX_MainForm1()
        {
            InitializeComponent();

            this.MNXSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            this.StafflineStemStrokeWidthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.GapSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            string[] mnxPathsArray = Directory.GetFiles(M.MNX_in_Folder, "*.mnx");
            string[] form1DataArray = Directory.GetFiles(M.Form1Data_Folder, "*.f1d");

            var mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();
            var form1DataPaths = new List<string>(form1DataArray);
            form1DataPaths.Sort();

            MNXSelect.SuspendLayout();
            MNXSelect.Items.Clear();
            for(var i = 0; i < form1DataPaths.Count; i++)
            {
                string mnxFilename = Path.GetFileName(mnxPaths[i]);
                _MNX_Form1Data_Paths.Add(new Tuple<string, string>(mnxPaths[i], form1DataPaths[i]));
                MNXSelect.Items.Add(mnxFilename);
            }
            MNXSelect.SelectedIndex = 0;
            MNXSelect.ResumeLayout();
        }

        private void WriteButton_Click(object sender, EventArgs e)
        {
            string form1DataPath = _MNX_Form1Data_Paths[MNXSelect.SelectedIndex].Item2;

            var form1StringData = new Form1StringData(form1DataPath);
            var form1Data = new Form1Data(form1StringData);

            SVGMIDIScore svgMIDIScore = new SVGMIDIScore(M.SVG_out_Folder, mnx, form1Data);
        }

        private void MNXSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSettings(MNXSelect.SelectedIndex); // can also be called to revert settings 

            _settingsHaveChanged = false;
            SetButtons(_settingsHaveChanged);
        }

        private void LoadSettings(int mnxSelectedIndex)
        {
            string mnxFilePath =_MNX_Form1Data_Paths[mnxSelectedIndex].Item1;

            ValidateXML(mnxFilePath); // If there is an error, a MessageBox is displayed, and the program stops.

            mnx = new MNX.Common.MNX(mnxFilePath);

            _numberOfMeasures = mnx.NumberOfMeasures;

            var form1DataPath = _MNX_Form1Data_Paths[mnxSelectedIndex].Item2;
            var svgds = new Form1StringData(form1DataPath);

            LoadControls(svgds, _numberOfMeasures);
        }

        // See: https://www.youtube.com/watch?v=wTgSS8X90aA&list=PL73qvSDlAVViXEuAWaRFKul4gmYX9D-qL&index=13
        private void ValidateXML(string filepath)
        {
            //bool rval = true;
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType= ValidationType.Schema;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler +=new System.Xml.Schema.ValidationEventHandler(ValidationEventHandler);

                using(XmlReader r = XmlReader.Create(filepath, settings))
                {
                    // iterate over the XML
                    while(r.Read())
                    {
                        // Do nothing.
                        // If something goes wrong call ValidationEventHandler(...)
                        // to throw an exception
                    }
                    Console.WriteLine("Validation passed.");
                }
            }
            catch(Exception ex)
            {
                string message = "File: " + Path.GetFileName(filepath) + "\n\n" + ex.Message;
                MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK);
                Environment.Exit(0); // stop the application
            }
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            // if we're here, something is wrong with the XML
            throw new Exception(args.Message);
        }

        private void LoadControls(Form1StringData svgds, int numberOfMeasures)
        {
            var page = svgds.Page;
            this.PageWidthTextBox.Text = page.Width;
            this.PageHeightTextBox.Text = page.Height;
            this.MarginTopPage1TextBox.Text = page.MarginTopPage1;
            this.MarginTopOtherPagesTextBox.Text = page.MarginTopOther;
            this.MarginRightTextBox.Text = page.MarginRight;
            this.MarginBottomTextBox.Text = page.MarginBottom;
            this.MarginLeftTextBox.Text = page.MarginLeft;

            var notes = svgds.Notation;
            this.StafflineStemStrokeWidthComboBox.SelectedIndex = GetIndex(StafflineStemStrokeWidthComboBox, notes.stafflineStemStrokeWidth);
            this.GapSizeComboBox.SelectedIndex = GetIndex(GapSizeComboBox, notes.gapSize);
            this.MinimumGapsBetweenStavesTextBox.Text = notes.minGapsBetweenStaves;
            this.MinimumGapsBetweenSystemsTextBox.Text = notes.minGapsBetweenSystems;
            this.SystemStartBarsLabel.Text = "system start bars [1.." + _numberOfMeasures.ToString() + "] ( must start at 1 )";
            this.SystemStartBarsTextBox.Text = notes.systemStartBars;
            this.CrotchetsPerMinuteTextBox.Text = notes.crotchetsPerMinute;

            var metadata = svgds.Metadata;
            this.MetadataTitleTextBox.Text = metadata.Title;
            this.MetadataAuthorTextBox.Text = metadata.Author;
            this.MetadataKeywordsTextBox.Text = metadata.Keywords;
            this.MetadataCommentTextBox.Text = metadata.Comment;

            var options = svgds.Options;
            this.OptionWritePage1TitlesCheckBox.CheckState = (options.WritePage1Titles == "true") ? CheckState.Checked : CheckState.Unchecked;
            this.OptionIncludeMIDIDataCheckBox.CheckState = (options.IncludeMIDIData == "true") ? CheckState.Checked : CheckState.Unchecked;
            this.OptionWriteScoreAsScrollCheckBox.CheckState = (options.WriteScrollScore == "true") ? CheckState.Checked : CheckState.Unchecked;
        }

        private int GetIndex(ComboBox comboBox, string value)
        {
            var items = comboBox.Items;
            int rval = 0;
            for(int i = 0; i < items.Count; i++)
            {
                if(items[i].ToString() == value)
                {
                    rval = i;
                    break;
                }
            }
            return rval;
        }

        private void SetButtons(bool settingsHaveChanged)
        {
            if(!settingsHaveChanged)
            {
                RevertFormatButton.Enabled = false;
                SaveFormatButton.Enabled = false;
                WriteButton.Enabled = true;
                WriteButton.Focus();
            }
            else // settings have changed
            {
                if(AllInputsAreErrorFree())
                {
                    RevertFormatButton.Enabled = true;
                    SaveFormatButton.Enabled = true;
                    WriteButton.Enabled = false;
                    SaveFormatButton.Focus();
                }
                else
                {
                    RevertFormatButton.Enabled = true;
                    SaveFormatButton.Enabled = false;
                    WriteButton.Enabled = false;
                }
            }
        }

        private bool AllInputsAreErrorFree()
        {
            bool rval = true;

            if(rval && M.HasError(this.PageWidthTextBox)) { rval = false; }
            if(rval && M.HasError(this.PageHeightTextBox)) { rval = false; }
            if(rval && M.HasError(this.MarginTopPage1TextBox)) { rval = false; }
            if(rval && M.HasError(this.MarginTopOtherPagesTextBox)) { rval = false; }
            if(rval && M.HasError(this.MarginRightTextBox)) { rval = false; }
            if(rval && M.HasError(this.MarginBottomTextBox)) { rval = false; }
            if(rval && M.HasError(this.MarginLeftTextBox)) { rval = false; }
            if(rval && M.HasError(this.MinimumGapsBetweenStavesTextBox)) { rval = false; }
            if(rval && M.HasError(this.MinimumGapsBetweenSystemsTextBox)) { rval = false; }
            if(rval && M.HasError(this.SystemStartBarsTextBox)) { rval = false; }
            if(rval && M.HasError(this.CrotchetsPerMinuteTextBox)) { rval = false; }

            return rval;
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void StringTextBox_Leave(object sender, EventArgs e)
        {
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void IntTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            M.CheckTextBoxIsUInt(textBox);
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void UnsignedDoubleTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            M.CheckTextBoxIsUnsignedDouble(textBox);
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void SystemStartBarsTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            M.CheckSystemStartBars(textBox, _numberOfMeasures);
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void TextBox_Changed(object sender, EventArgs e)
        {
            M.SetToWhite(sender as TextBox);
            RevertFormatButton.Enabled = true;
            SaveFormatButton.Enabled = true;
            WriteButton.Enabled = false;
        }

        private void SaveFormatButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Save settings?", "Save", MessageBoxButtons.YesNo);
            if(result == DialogResult.Yes)
            {
                SaveSettings();
                _settingsHaveChanged = false;
                WriteButton.Enabled = true;
                WriteButton.Focus();
            }

        }

        public void SaveSettings()
        {
            Form1StringData svgds = new Form1StringData(_MNX_Form1Data_Paths[MNXSelect.SelectedIndex].Item2);

            var page = svgds.Page;
            page.Width = PageWidthTextBox.Text;
            page.Height = PageHeightTextBox.Text;
            page.MarginTopPage1 = MarginTopPage1TextBox.Text;
            page.MarginTopOther = MarginTopOtherPagesTextBox.Text;
            page.MarginRight = MarginRightTextBox.Text;
            page.MarginLeft = MarginLeftTextBox.Text;
            page.MarginBottom = MarginBottomTextBox.Text;

            var notes = svgds.Notation;
            notes.stafflineStemStrokeWidth = (string)StafflineStemStrokeWidthComboBox.Items[StafflineStemStrokeWidthComboBox.SelectedIndex];
            notes.gapSize = (string)GapSizeComboBox.Items[GapSizeComboBox.SelectedIndex];
            notes.minGapsBetweenStaves = MinimumGapsBetweenStavesTextBox.Text;
            notes.minGapsBetweenSystems = MinimumGapsBetweenSystemsTextBox.Text;
            notes.systemStartBars = SystemStartBarsTextBox.Text;
            notes.crotchetsPerMinute = CrotchetsPerMinuteTextBox.Text;

            var metadata = svgds.Metadata;
            metadata.Title = this.MetadataTitleTextBox.Text;
            metadata.Author = this.MetadataAuthorTextBox.Text;
            metadata.Keywords = this.MetadataKeywordsTextBox.Text;
            metadata.Comment = this.MetadataCommentTextBox.Text;

            var options = svgds.Options;
            options.WritePage1Titles = (this.OptionWritePage1TitlesCheckBox.CheckState == CheckState.Checked) ? "true" : "false";
            options.IncludeMIDIData = (this.OptionIncludeMIDIDataCheckBox.CheckState == CheckState.Checked) ? "true" : "false";
            options.WriteScrollScore = (this.OptionWriteScoreAsScrollCheckBox.CheckState == CheckState.Checked) ? "true" : "false";

            svgds.SaveSettings();
        }

        private void RevertFormatButton_Click(object sender, EventArgs e)
        {
            LoadSettings(MNXSelect.SelectedIndex);

            _settingsHaveChanged = false;
            RevertFormatButton.Enabled = false;
            SaveFormatButton.Enabled = false;
            WriteButton.Enabled = true;
        }
    }
}

using MNX.Common;
using MNX.Globals;
using Moritz.Symbols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MNX.Main
{
    public partial class MNX_MainForm1 : Form
    {
        private readonly List<Tuple<string, string>> _MNX_Form1Data_Paths = new List<Tuple<string, string>>();
        private bool _settingsHaveChanged = false;
        private int _numberOfMeasures; // the number of measures in the currently loaded MNX file

        public MNX_MainForm1()
        {
            InitializeComponent();

            Form1OptionsStrings options = new OptionsForWriteAll().Options;
            OptionWritePage1TitlesCheckBox.CheckState = (options.WritePage1Titles == "true") ? CheckState.Checked : CheckState.Unchecked;
            OptionIncludeMIDIDataCheckBox.CheckState = (options.IncludeMIDIData == "true") ? CheckState.Checked : CheckState.Unchecked;
            OptionWriteScoreAsScrollCheckBox.CheckState = (options.WriteScrollScore == "true") ? CheckState.Checked : CheckState.Unchecked;

            this.MNXSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            this.StafflineStemStrokeWidthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.GapSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            string[] mnxPathsArray = Directory.GetFiles(M.MNX_in_Folder, "*.mnx");
            string[] form1DataArray = Directory.GetFiles(M.Form1Data_Folder, "*.f1d");

            var mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();
            var form1DataPaths = new List<string>(form1DataArray);
            form1DataPaths.Sort();

            MNXSelect.Items.Clear();
            MNXSelect.Items.Add("All Scores");
            for(var i = 0; i < form1DataPaths.Count; i++)
            {
                string mnxFilename = Path.GetFileName(mnxPaths[i]);
                _MNX_Form1Data_Paths.Add(new Tuple<string, string>(mnxPaths[i], form1DataPaths[i]));
                MNXSelect.Items.Add(mnxFilename);
            }
            MNXSelect.SelectedIndex = 0;

            _settingsHaveChanged = false;
            SetButtons(_settingsHaveChanged);
        }

        private void WriteButton_Click(object sender, EventArgs e)
        {
            var selectedIndex = MNXSelect.SelectedIndex;

            if(selectedIndex == 0)
            {
                OptionsForWriteAll optionsForWriteAll = new OptionsForWriteAll();
                for(var i = 0; i < _MNX_Form1Data_Paths.Count; i++)
                {
                    var form1StringData = new Form1StringData(_MNX_Form1Data_Paths[i].Item2);
                    form1StringData.Options = optionsForWriteAll.Options; // override when writing all scores
                    var form1Data = new Form1Data(form1StringData);

                    M.MillisecondsPerTick = 60000 / (M.TicksPerCrotchet * form1Data.Notation.CrotchetsPerMinute);

                    var mnx = new MNX(_MNX_Form1Data_Paths[i].Item1);
                    MNXCommonData mnxCommonData = mnx.MNXCommonData;

                    SVGMIDIScore svgMIDIScore = new SVGMIDIScore(M.SVG_out_Folder, mnxCommonData, form1Data);
                    if(i == 0) // temp
                    {
                        break;
                    }
                }
            }
            else
            {
                var form1StringData = new Form1StringData(_MNX_Form1Data_Paths[selectedIndex - 1].Item2);
                var form1Data = new Form1Data(form1StringData);

                M.MillisecondsPerTick = 60000 / (M.TicksPerCrotchet * form1Data.Notation.CrotchetsPerMinute);

                var mnx = new MNX(_MNX_Form1Data_Paths[selectedIndex - 1].Item1);
                MNXCommonData mnxCommonData = mnx.MNXCommonData;
                SVGMIDIScore svgMIDIScore = new SVGMIDIScore(M.SVG_out_Folder, mnxCommonData, form1Data);
            }
        }

        private void MNXSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(MNXSelect.SelectedIndex == 0)
            {
                // Write all scores
                EnableDisableControls(true);
            }
            else
            {
                LoadOneScore();                
            }
        }

        private void LoadOneScore()
        {
            // Load a score (edit form1DataStrings)
            var form1DataPath = _MNX_Form1Data_Paths[MNXSelect.SelectedIndex - 1].Item2;
            var svgds = new Form1StringData(form1DataPath);

            //var mnxPath = _MNX_Form1Data_Paths[MNXSelect.SelectedIndex - 1].Item1;
            //var mnx = new MNX(mnxPath);
            //_numberOfMeasures = mnx.MNXCommonData.NumberOfMeasures;
            _numberOfMeasures = 99;

            LoadControls(svgds, _numberOfMeasures);

            _settingsHaveChanged = false;
            SetButtons(_settingsHaveChanged);
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

        private void EnableDisableControls(bool disable)
        {
            if(disable)
            {
                RemoveControlEvents();
                DimensionsLabel.Enabled = false;
                PaperSizeGroupBox.Enabled = false;
                MarginsGroupBox.Enabled = false;
                NotationGroupBox.Enabled = false;
                SpeedGroupBox.Enabled = false;
                MetadataGroupBox.Enabled = false;
                OptionsGroupBox.Enabled = true;

                WriteButton.Text = "Write all Scores";
            }
            else
            {
                ReplaceControlEvents();
                DimensionsLabel.Enabled = true;
                PaperSizeGroupBox.Enabled = true;
                MarginsGroupBox.Enabled = true;
                NotationGroupBox.Enabled = true;
                SpeedGroupBox.Enabled = true;
                MetadataGroupBox.Enabled = true;
                OptionsGroupBox.Enabled = true;

                WriteButton.Text = "Write Score";
            }
        }

        private void ReplaceControlEvents()
        {
            PageWidthTextBox.Leave += IntTextBox_Leave;
            PageHeightTextBox.Leave += IntTextBox_Leave;
            MarginTopPage1TextBox.Leave += IntTextBox_Leave;
            MarginTopOtherPagesTextBox.Leave += IntTextBox_Leave;
            MarginRightTextBox.Leave += IntTextBox_Leave;
            MarginBottomTextBox.Leave += IntTextBox_Leave;
            MarginLeftTextBox.Leave += IntTextBox_Leave;
            StafflineStemStrokeWidthComboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            GapSizeComboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            MinimumGapsBetweenStavesTextBox.Leave += IntTextBox_Leave;
            MinimumGapsBetweenSystemsTextBox.Leave += IntTextBox_Leave;
            SystemStartBarsTextBox.Leave += SystemStartBarsTextBox_Leave;
            CrotchetsPerMinuteTextBox.Leave += IntTextBox_Leave;
        }

        private void RemoveControlEvents()
        {
            PageWidthTextBox.Leave -= IntTextBox_Leave; 
            PageHeightTextBox.Leave -= IntTextBox_Leave;
            MarginTopPage1TextBox.Leave -= IntTextBox_Leave;
            MarginTopOtherPagesTextBox.Leave -= IntTextBox_Leave;
            MarginRightTextBox.Leave -= IntTextBox_Leave;
            MarginBottomTextBox.Leave -= IntTextBox_Leave;
            MarginLeftTextBox.Leave -= IntTextBox_Leave;
            StafflineStemStrokeWidthComboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
            GapSizeComboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
            MinimumGapsBetweenStavesTextBox.Leave -= IntTextBox_Leave;
            MinimumGapsBetweenSystemsTextBox.Leave -= IntTextBox_Leave;
            SystemStartBarsTextBox.Leave -= SystemStartBarsTextBox_Leave;
            CrotchetsPerMinuteTextBox.Leave -= IntTextBox_Leave;
        }

        private void SetButtons(bool settingsHaveChanged)
        {
            EnableDisableControls(true);
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

            EnableDisableControls(MNXSelect.SelectedIndex == 0);
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
            Form1StringData svgds = null;
            if(MNXSelect.SelectedIndex == 0)
            {
                svgds = new OptionsForWriteAll();
                var options = svgds.Options;
                options.WritePage1Titles = (this.OptionWritePage1TitlesCheckBox.CheckState == CheckState.Checked) ? "true" : "false";
                options.IncludeMIDIData = (this.OptionIncludeMIDIDataCheckBox.CheckState == CheckState.Checked) ? "true" : "false";
                options.WriteScrollScore = (this.OptionWriteScoreAsScrollCheckBox.CheckState == CheckState.Checked) ? "true" : "false";
            }
            else
            {
                svgds = new Form1StringData(_MNX_Form1Data_Paths[MNXSelect.SelectedIndex - 1].Item2);

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
            }
            svgds.SaveSettings();
        }

        private void RevertFormatButton_Click(object sender, EventArgs e)
        {
            LoadOneScore();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MNX.AGlobals;
using Moritz.Spec;

namespace MNX_Main
{
    public partial class MNX_MainForm1 : Form
    {
        private readonly List<Tuple<string, string>> _mnxSVGDatas = new List<Tuple<string, string>>();
        private bool _settingsHaveChanged = false;

        public MNX_MainForm1()
        {
            InitializeComponent();

            this.MNXSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            this.StafflineStemStrokeWidthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.GapSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            string[] mnxPathsArray = Directory.GetFiles(A.MNX_in_Folder, "*.mnx");
            string[] svgDataArray = Directory.GetFiles(A.SVGData_Folder, "*.svgd");

            var mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();
            var svgDataPaths = new List<string>(svgDataArray);
            svgDataPaths.Sort();

            MNXSelect.Items.Clear();
            MNXSelect.Items.Add("All Scores");
            for(var i = 0; i < svgDataPaths.Count; i++)
            {
                string svgDataFilename = Path.GetFileNameWithoutExtension(svgDataPaths[i]);
                _mnxSVGDatas.Add(new Tuple<string, string>(mnxPaths[i], svgDataPaths[i]));
                MNXSelect.Items.Add(svgDataFilename);
            }
            MNXSelect.SelectedIndex = 0;
        }

        private void WriteButton_Click(object sender, EventArgs e)
        {
            var selectedIndex = MNXSelect.SelectedIndex;

            if(selectedIndex == 0)
            {
                for(var i = 0; i < _mnxSVGDatas.Count; i++)
                {
                    var mnx = new MNX(_mnxSVGDatas[i].Item1);
                    var svgds = new SVGDataStrings(_mnxSVGDatas[i].Item2);
                    var svgData = new SVGData(svgds);
                    //List<Bar> bars = mnx.ToBars();
                }
            }
            else
            {
                var mnx = new MNX(_mnxSVGDatas[selectedIndex - 1].Item1);
                var svgds = new SVGDataStrings(_mnxSVGDatas[selectedIndex - 1].Item2);
                var svgData = new SVGData(svgds);
                //List<Bar> bars = mnx.ToBars();
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
            // Load a score (edit svgDataStrings)
            var svgDataPath = _mnxSVGDatas[MNXSelect.SelectedIndex - 1].Item2;
            var svgds = new SVGDataStrings(svgDataPath);

            LoadControls(svgds);

            _settingsHaveChanged = false;
            SetButtons(_settingsHaveChanged);
        }

        private void LoadControls(SVGDataStrings svgds)
        {
            var page = svgds.Page;
            this.PageWidthTextBox.Text = page.width;
            this.PageHeightTextBox.Text = page.height;
            this.MarginTopPage1TextBox.Text = page.marginTopPage1;
            this.MarginTopOtherPagesTextBox.Text = page.marginTopOther;
            this.MarginRightTextBox.Text = page.marginRight;
            this.MarginBottomTextBox.Text = page.marginBottom;
            this.MarginLeftTextBox.Text = page.marginLeft;

            var notes = svgds.MNXCommonData;
            this.StafflineStemStrokeWidthComboBox.SelectedIndex = GetIndex(StafflineStemStrokeWidthComboBox, notes.stafflineStemStrokeWidth);
            this.GapSizeComboBox.SelectedIndex = GetIndex(GapSizeComboBox, notes.gapSize);
            this.MinimumGapsBetweenStavesTextBox.Text = notes.minGapsBetweenStaves;
            this.MinimumGapsBetweenSystemsTextBox.Text = notes.minGapsBetweenSystems;
            this.SystemStartBarsTextBox.Text = notes.systemStartBars;
            this.CrotchetsPerMinuteTextBox.Text = notes.crotchetsPerMinute;
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
            SystemStartBarsTextBox.Leave += IntTextBox_Leave;
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
            SystemStartBarsTextBox.Leave -= IntTextBox_Leave;
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

            if(rval && A.HasError(this.PageWidthTextBox)) { rval = false; }
            if(rval && A.HasError(this.PageHeightTextBox)) { rval = false; }
            if(rval && A.HasError(this.MarginTopPage1TextBox)) { rval = false; }
            if(rval && A.HasError(this.MarginTopOtherPagesTextBox)) { rval = false; }
            if(rval && A.HasError(this.MarginRightTextBox)) { rval = false; }
            if(rval && A.HasError(this.MarginBottomTextBox)) { rval = false; }
            if(rval && A.HasError(this.MarginLeftTextBox)) { rval = false; }
            if(rval && A.HasError(this.MinimumGapsBetweenStavesTextBox)) { rval = false; }
            if(rval && A.HasError(this.MinimumGapsBetweenSystemsTextBox)) { rval = false; }
            if(rval && A.HasError(this.SystemStartBarsTextBox)) { rval = false; }
            if(rval && A.HasError(this.CrotchetsPerMinuteTextBox)) { rval = false; }

            return rval;
        }

        private void IntTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            A.CheckTextBoxIsUInt(textBox);
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
            A.CheckTextBoxIsUnsignedDouble(textBox);
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void SystemStartBarsTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            A.CheckSystemStartBarsUnsignedIntList(textBox);
            _settingsHaveChanged = true;
            SetButtons(_settingsHaveChanged);
        }

        private void TextBox_Changed(object sender, EventArgs e)
        {
            A.SetToWhite(sender as TextBox);
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
            var svgds = new SVGDataStrings(_mnxSVGDatas[MNXSelect.SelectedIndex - 1].Item2);

            var page = svgds.Page;
            page.width = PageWidthTextBox.Text;
            page.height = PageHeightTextBox.Text;
            page.marginTopPage1 = MarginTopPage1TextBox.Text;
            page.marginTopOther = MarginTopOtherPagesTextBox.Text;
            page.marginRight = MarginRightTextBox.Text;
            page.marginLeft = MarginLeftTextBox.Text;
            page.marginBottom = MarginBottomTextBox.Text;

            var notes = svgds.MNXCommonData;
            notes.stafflineStemStrokeWidth = (string) StafflineStemStrokeWidthComboBox.Items[StafflineStemStrokeWidthComboBox.SelectedIndex];
            notes.gapSize = (string) GapSizeComboBox.Items[GapSizeComboBox.SelectedIndex];
            notes.minGapsBetweenStaves = MinimumGapsBetweenStavesTextBox.Text;
            notes.minGapsBetweenSystems = MinimumGapsBetweenSystemsTextBox.Text;
            notes.systemStartBars = SystemStartBarsTextBox.Text;
            notes.crotchetsPerMinute = CrotchetsPerMinuteTextBox.Text;

            svgds.SaveSettings();
        }

        private void RevertFormatButton_Click(object sender, EventArgs e)
        {
            LoadOneScore();
        }
    }
}

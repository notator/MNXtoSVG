using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MNX.AGlobals;

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
                    A.SVGData = new SVGData(_mnxSVGDatas[i].Item2);
                    mnx.WriteSVG(); // Writes the score to A.SVG_out
                }
            }
            else
            {
                var mnx = new MNX(_mnxSVGDatas[selectedIndex - 1].Item1);
                A.SVGData = new SVGData(_mnxSVGDatas[selectedIndex - 1].Item2);
                mnx.WriteSVG(); // Writes the score to A.SVG_out
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
                // Write one score (edit svgData)
                var svgDataPath = _mnxSVGDatas[MNXSelect.SelectedIndex - 1].Item2;
                SVGData svgd = new SVGData(svgDataPath);

                LoadControls(svgd);

                EnableDisableControls(false);

                _settingsHaveChanged = false;
            }
        }

        private void LoadControls(SVGData svgd)
        {
            this.PageWidthTextBox.Text = svgd.Page.width.ToString();
            this.PageHeightTextBox.Text = svgd.Page.height.ToString();
            this.MarginTopPage1TextBox.Text = svgd.Page.marginTopPage1.ToString();
            this.MarginTopOtherPagesTextBox.Text = svgd.Page.marginTopOther.ToString();
            this.MarginRightTextBox.Text = svgd.Page.marginRight.ToString();
            this.MarginBottomTextBox.Text = svgd.Page.marginBottom.ToString();
            this.MarginLeftTextBox.Text = svgd.Page.marginLeft.ToString();

            this.StafflineStemStrokeWidthComboBox.SelectedIndex = GetIndex(StafflineStemStrokeWidthComboBox, svgd.MNXCommonData.stafflineStemStrokeWidth);
            this.GapSizeComboBox.SelectedIndex = GetIndex(GapSizeComboBox, svgd.MNXCommonData.gapSize);
            this.MinimumGapsBetweenStavesTextBox.Text = svgd.MNXCommonData.minGapsBetweenStaves.ToString();
            this.MinimumGapsBetweenSystemsTextBox.Text = svgd.MNXCommonData.minGapsBetweenSystems.ToString();
            this.SystemStartBarsTextBox.Text = svgd.MNXCommonData.systemStartBars;

            this.CrotchetsPerMinuteTextBox.Text = svgd.MNXCommonData.crotchetsPerMinute.ToString(A.En_USNumberFormat);

        }

        private int GetIndex(ComboBox comboBox, double value)
        {
            string valueString = value.ToString(A.En_USNumberFormat);
            var items = comboBox.Items;
            int rval = 0;
            for(int i = 0; i < items.Count; i++)
            {
                if(items[i].ToString() == valueString)
                {
                    rval = i;
                    break;
                }
            }
            return rval;
        }

        private void EnableDisableControls(bool writeAll)
        {
            if(writeAll)
            {
                DimensionsLabel.Enabled = false;
                PaperSizeGroupBox.Enabled = false;
                MarginsGroupBox.Enabled = false;
                NotationGroupBox.Enabled = false;
                SpeedGroupBox.Enabled = false;
                SaveFormatButton.Enabled = false;

                WriteButton.Text = "Write all Scores";
                WriteButton.Enabled = true;
                WriteButton.Focus();
            }
            else
            {
                DimensionsLabel.Enabled = true;
                PaperSizeGroupBox.Enabled = true;
                MarginsGroupBox.Enabled = true;
                NotationGroupBox.Enabled = true;
                SpeedGroupBox.Enabled = true;

                SaveFormatButton.Enabled = false;

                WriteButton.Text = "Write Score";
                WriteButton.Enabled = true;
                WriteButton.Focus();
            }
        }

        private void SetButtons(TextBox textBox)
        {
            _settingsHaveChanged = true;

            if(AllInputsAreErrorFree())
            {
                RevertFormatButton.Enabled = true;
                SaveFormatButton.Enabled = true;
                WriteButton.Enabled = false;
            }
            else
            {
                RevertFormatButton.Enabled = true;
                SaveFormatButton.Enabled = false;
                WriteButton.Enabled = false;
            }
            SaveFormatButton.Focus();
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
            SetButtons(textBox);
        }

        private void UnsignedDoubleTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            A.CheckTextBoxIsUnsignedDouble(textBox);
            SetButtons(textBox);
        }

        private void SystemStartBarsTextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            A.CheckSystemStartBarsUnsignedIntList(textBox);
            SetButtons(textBox);
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
            }

        }

        private void SaveSettings()
        {
            throw new NotImplementedException();
        }


    }
}

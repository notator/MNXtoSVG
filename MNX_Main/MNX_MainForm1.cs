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

        private void WriteSVGButton_Click(object sender, EventArgs e)
        {
 
        }

        private void WriteAllSVGScoresButton_Click(object sender, EventArgs e)
        {
            for(var i = 0; i < _mnxSVGDatas.Count; i++)
            {
                var mnx = new MNX(_mnxSVGDatas[i].Item1);
                A.SVGData = new SVGData(_mnxSVGDatas[i].Item2);
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

            this.StafflineStemStrokeWidthComboBox.SelectedText = svgd.MNXCommonData.stafflineStemStrokeWidth.ToString(A.en_US_CultureInfo);
            this.GapSizeComboBox.SelectedText = svgd.MNXCommonData.gapSize.ToString(A.en_US_CultureInfo);
            this.MinimumGapsBetweenStavesTextBox.Text = svgd.MNXCommonData.minGapsBetweenStaves.ToString();
            this.MinimumGapsBetweenSystemsTextBox.Text = svgd.MNXCommonData.minGapsBetweenSystems.ToString();

            this.CrotchetsPerMinuteTextBox.Text = svgd.MNXCommonData.crotchetsPerMinute.ToString(A.en_US_CultureInfo);
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
                SaveSpeedAndPageSettingsButton.Enabled = false;

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

                SaveSpeedAndPageSettingsButton.Enabled = false;

                WriteButton.Text = "Write Score";
                WriteButton.Enabled = true;
                WriteButton.Focus();
            }
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            _settingsHaveChanged = true;
            SaveSpeedAndPageSettingsButton.Enabled = true;
            WriteButton.Enabled = false;
        }

        private void SaveSpeedAndPageSettingsButton_Click(object sender, EventArgs e)
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

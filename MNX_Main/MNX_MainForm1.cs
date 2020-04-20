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
        public MNX_MainForm1()
        {
            InitializeComponent();
        }

        private void WriteSVGButton_Click(object sender, EventArgs e)
        {
 
        }

        private void WriteAllSVGScoresButton_Click(object sender, EventArgs e)
        {
            string[] mnxPathsArray = Directory.GetFiles(A.MNX_in_Folder, "*.mnx");
            string[] svgDataArray = Directory.GetFiles(A.SVGData_Folder, "*.svgd");

            List<string> mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();
            List<string> svgDataPaths = new List<string>(svgDataArray);
            svgDataPaths.Sort();

            for(var i = 0; i < mnxPaths.Count; i++)
            {
                var mnx = new MNX(mnxPaths[i]);
                A.SVGData = new SVGData(svgDataPaths[i]);
                mnx.WriteSVG(); // Writes the score to A.SVG_out
            }
        }
    }
}

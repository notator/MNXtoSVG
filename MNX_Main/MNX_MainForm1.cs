using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MNX.AGlobals;

namespace MNX_Main
{
    public partial class MNXtoSVGForm1 : Form
    {
        public MNXtoSVGForm1()
        {
            InitializeComponent();
        }

        private void GOButton_Click(object sender, EventArgs e)
        {
            string programFolder = GetProgramFolder();
            string MNX_in_Directory = programFolder + @"\MNX_in\mnx\";
            string SVG_out_Directory = programFolder + @"\SVG_out\";

            List<MNX> mnxs = GetMNXs(MNX_in_Directory);

            AdjustMNXs(mnxs);

            //foreach(var mnx in mnxs)
            //{
            //    var svgPath = SVG_out_Directory + mnx.FileName + ".svg";
            //    mnx.WriteSVG(svgPath);
            //}  
        }

        private string GetProgramFolder()
        {
            string directory = Directory.GetCurrentDirectory();

            string directoryName = Path.GetFileName(directory);
            while(directoryName != "MNX_Main")
            {
                var startIndex = directory.IndexOf(directoryName) - 1;
                directory = directory.Remove(startIndex);
                directoryName = Path.GetFileName(directory);
            }

            return directory;
        }

        /// <summary>
        /// Resolve directions hierarchies etc.
        /// </summary>
        /// <param name="mnxs"></param>
        private void AdjustMNXs(List<MNX> mnxs)
        {

        }

        private List<MNX> GetMNXs(string MNX_in_Folder)
        {
            string[] mnxPathsArray = Directory.GetFiles(MNX_in_Folder, "*.mnx");

            List<string> mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();

            List<MNX> mnxs = new List<MNX>();
            string mnxPath = null;

            for(var i = 0; i < mnxPaths.Count; i++)
            {
                mnxPath = mnxPaths[i];
                mnxs.Add(new MNX(mnxPath));
            }

            return mnxs;
        }
    }
}

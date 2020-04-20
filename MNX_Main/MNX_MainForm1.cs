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

        private void WriteSVGButton_Click(object sender, EventArgs e)
        {
 
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
        /// Resolve mnx directions hierarchies etc.
        /// </summary>
        /// <param name="mnxs"></param>
        private void AdjustMNXs(List<KeyValuePair<MNX, SVG_Data>> mnx_svgData_pairs)
        {

        }

        private List<KeyValuePair<MNX, SVG_Data>> GetMNX_SVGDataPairs(string MNX_in_Folder, string SVG_format_Folder)
        {
            List<KeyValuePair<MNX, SVG_Data>> rval = new List<KeyValuePair<MNX, SVG_Data>>();

            string[] mnxPathsArray = Directory.GetFiles(MNX_in_Folder, "*.mnx");
            string[] svgDataArray = Directory.GetFiles(SVG_format_Folder, "*.svgd");

            List<string> mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();
            List<string> svgDataPaths = new List<string>(svgDataArray);
            svgDataPaths.Sort();

            for(var i = 0; i < mnxPaths.Count; i++)
            {
                var mnx = new MNX(mnxPaths[i]);
                var svgData = new SVG_Data(svgDataPaths[i]);
                var pair = new KeyValuePair<MNX, SVG_Data>(mnx, svgData);
                rval.Add(pair);
            }

            return rval;
        }

        private void WriteAllSVGFilesButton_Click(object sender, EventArgs e)
        {
            string programFolder = GetProgramFolder();
            string MNX_in_Directory = programFolder + @"\MNX_in\mnx\";
            string SVG_format_Directory = programFolder + @"\SVG_format\";
            string SVG_out_Directory = programFolder + @"\SVG_out\";

            List<KeyValuePair<MNX, SVG_Data>> mnxs = GetMNX_SVGDataPairs(MNX_in_Directory, SVG_format_Directory);

            AdjustMNXs(mnxs);

            //foreach(var mnx in mnxs)
            //{
            //    var svgPath = SVG_out_Directory + mnx.FileName + ".svg";
            //    mnx.WriteSVG(svgPath);
            //} 

        }
    }
}

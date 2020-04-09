using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MNXtoSVG.Globals;
using System.Xml;
using System.Diagnostics;

namespace MNXtoSVG
{
    public partial class _Form1 : Form
    {
        public _Form1()
        {
            InitializeComponent();
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            List<MNX> mnxs = GetMNXs();

            AdjustMNXs(mnxs); 

            const string SVG_out_Directory = @"D:\Visual Studio\Projects\MNXtoSVG\MNXtoSVG\SVG_out\";

            foreach(var mnx in mnxs)
            {
                var svgPath = SVG_out_Directory + mnx.FileName + ".svg";
                mnx.WriteSVG(svgPath);
            }  
        }

        /// <summary>
        /// Resolve directions hierarchies etc.
        /// </summary>
        /// <param name="mnxs"></param>
        private void AdjustMNXs(List<MNX> mnxs)
        {

        }

        private List<MNX> GetMNXs()
        {
            const string MNX_in_Directory = @"D:\Visual Studio\Projects\MNXtoSVG\MNXtoSVG\MNX_in\mnx";
            string[] mnxPathsArray = Directory.GetFiles(MNX_in_Directory, "*.mnx");

            List<string> mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();

            List<MNX> mnxs = new List<MNX>();
            string mnxPath = null;

            for(var i = 0; i < mnxPaths.Count; i++)
            {
                try
                {
                    mnxPath = mnxPaths[i];
                    mnxs.Add(new MNX(mnxPath));
                }
                catch(Exception ex)
                {
                    string infoStr = ex.Message +
                        "\n\nError in File: " + Path.GetFileName(mnxPath);

                    MessageBox.Show(infoStr, "Error constructing MNX object", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return mnxs;
        }
    }


}

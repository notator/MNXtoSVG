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

namespace MNXtoSVG
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            const string MNX_in_Directory = @"D:\Visual Studio\Projects\MNXtoSVG\MNXtoSVG\MNX_in\";
            const string SVG_out_Directory = @"D:\Visual Studio\Projects\MNXtoSVG\MNXtoSVG\SVG_out\";

            string[] mnxPathsArray = Directory.GetFiles(MNX_in_Directory, "*.mnx");
            List<string> mnxCommonPaths = new List<string>(mnxPathsArray);
            mnxCommonPaths.Sort();

            // change the loop limits as required. (or add a control to Form1).
            for(var i = 0; i < 1; i++) 
            {
                var mnxCommonPath = mnxCommonPaths[i];
                MNX_Common mnxCommon = null;
                try
                {
                    mnxCommon = new MNX_Common(mnxCommonPath);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error constructing MNX_Common object", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                var svgPath = SVG_out_Directory + Path.GetFileNameWithoutExtension(mnxCommonPath) + ".svg";

                mnxCommon.WriteSVG(svgPath);
            }
        }

    }
}

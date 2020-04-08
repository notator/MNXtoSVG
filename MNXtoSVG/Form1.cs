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
            List<MNX> mnxs = GetMNXs();

            const string SVG_out_Directory = @"D:\Visual Studio\Projects\MNXtoSVG\MNXtoSVG\SVG_out\";

            foreach(var mnx in mnxs)
            {
                var svgPath = SVG_out_Directory + mnx.FileName + ".svg";
                mnx.WriteSVG(svgPath);
            }  
        }

        private List<MNX> GetMNXs()
        {
            const string MNX_in_Directory = @"D:\Visual Studio\Projects\MNXtoSVG\MNXtoSVG\MNX_in\mnx";
            string[] mnxPathsArray = Directory.GetFiles(MNX_in_Directory, "*.mnx");

            List<string> mnxPaths = new List<string>(mnxPathsArray);
            mnxPaths.Sort();

            List<MNX> mnxs = new List<MNX>();

            for(var i = 0; i < mnxPaths.Count; i++)
            {
                var mnxPath = mnxPaths[i];
                MNX mnx = null;
                try
                {
                    mnx = new MNX(mnxPath);
                    mnxs.Add(mnx);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error constructing MNX_Common object", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return mnxs;
        }
    }


}

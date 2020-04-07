using MNXtoSVG.Globals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MNXtoSVG
{
    class MNX_Common
    {
        public MNX_Common(string mnxCommonPath)
        {
            using(XmlReader r = XmlReader.Create(mnxCommonPath))
            {
                G.ReadToXmlElementTag(r, "mnx"); // check that this is an mnx file

                //G.Assert(r.Name == "notation" || r.Name == "krystals" || r.Name == "palettes");

                //while(r.Name == "notation" || r.Name == "krystals" || r.Name == "palettes")
                //{
                //    if(r.NodeType != XmlNodeType.EndElement)
                //    {
                //        switch(r.Name)
                //        {
                //            case "notation":
                //                GetNotation(r);
                //                break;
                //            case "krystals":
                //                GetKrystals(r);
                //                break;
                //            case "palettes":
                //                GetPalettes(r);
                //                break;
                //        }
                //    }
                //    G.ReadToXmlElementTag(r, "notation", "krystals", "palettes", "moritzKrystalScore");
                //}
                G.Assert(r.Name == "mnx"); // end of mnx
            }
        }

        internal void WriteSVG(string svgPath)
        {
            throw new NotImplementedException();
        }
    }
}

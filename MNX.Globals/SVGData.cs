
using System.IO;
using System.Xml;

namespace MNX.AGlobals
{
    // https://w3c.github.io/mnx/specification/common/#the-mnx-element
    public class SVGData
    {
        public readonly string FileName;

        public SVGData(string svgDataPath)
        {
            FileName = Path.GetFileNameWithoutExtension(svgDataPath);

            using(XmlReader r = XmlReader.Create(svgDataPath))
            {
                A.ReadToXmlElementTag(r, "svgInstantiationSettings"); // check that this is an svgInstantiationSettings file

                A.ReadToXmlElementTag(r, "paperSize", "margins", "notation", "time");

                while(r.Name == "paperSize" || r.Name == "margins" || r.Name == "notation" || r.Name == "time" || r.Name == "svgInstantiationSettings")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "paperSize":
                                break;
                            case "margins":
                                break;
                            case "notation":
                                break;
                            case "time":
                                break;

                        }
                        A.ReadToXmlElementTag(r, "paperSize", "margins", "notation", "time", "svgInstantiationSettings");
                    }
                    
                }
                A.Assert(r.Name == "svgInstantiationSettings"); // end of svgInstantiationSettings
            }
        }

        public override string ToString()
        {
            return FileName;
        }
    }

}

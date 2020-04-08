using MNXtoSVG.Globals;
using System.Collections.Generic;
using System.Xml;

namespace MNXtoSVG
{
    internal class Part
    {
        // https://w3c.github.io/mnx/specification/common/#the-part-element

        public Part(XmlReader r)
        {
            G.Assert(r.Name == "part");

            G.ReadToXmlElementTag(r, "part-name", "part-abbreviation", "instrument-sound", "measure");

            while(r.Name == "part-name" || r.Name == "part-abbreviation" || r.Name == "instrument-sound" || r.Name == "measure")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "part-name":
                            PartName = r.ReadElementContentAsString();
                            break;
                        case "part-abbreviation":
                            PartAbbreviation = r.ReadElementContentAsString();
                            break;
                        case "instrument-sound":
                            InstrumentSound = r.ReadElementContentAsString();
                            break;
                        case "measure":
                            Measures.Add(new Measure(r, "part"));
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "part-name", "part-abbreviation", "instrument-sound", "measure", "part");
            }
            G.Assert(r.Name == "part"); // end of part
        }

        public readonly string PartName;
        public readonly string PartAbbreviation;
        public readonly string InstrumentSound;
        public List<Measure> Measures = new List<Measure>();
    }
}
using MNXtoSVG.Globals;
using System.Collections.Generic;
using System.Xml;

namespace MNXtoSVG
{
    internal class Global
    {
        // https://w3c.github.io/mnx/specification/common/#the-global-element

        public Global(XmlReader r)
        {
            G.Assert(r.Name == "global");

            // can have a "parts" attribute
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "parts":
                        var partIDs = r.Value.Split(' ');
                        PartIDs = new List<string>(partIDs);
                        break;
                }
            }

            G.ReadToXmlElementTag(r, "measure");

            while(r.Name == "measure")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    Measures.Add(new Measure(r, "global"));
                }
                G.ReadToXmlElementTag(r, "measure", "global");
            }
            G.Assert(r.Name == "global"); // end of global

            G.Assert(Measures.Count > 0);
        }

        public List<string> PartIDs = null;
        public List<Measure> Measures = new List<Measure>();
    }
}
using MNX.Globals;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal class Global
    {
        public List<string> PartIDs = null;
        public List<Measure> Measures = new List<Measure>();

        public Global(XmlReader r)
        {
            A.Assert(r.Name == "global");
            // https://w3c.github.io/mnx/specification/common/#the-global-element

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

            A.ReadToXmlElementTag(r, "measure");

            while(r.Name == "measure")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    Measures.Add(new Measure(r, true));
                }
                A.ReadToXmlElementTag(r, "measure", "global");
            }
            A.Assert(r.Name == "global"); // end of global

            A.Assert(Measures.Count > 0);
        }
    }
}
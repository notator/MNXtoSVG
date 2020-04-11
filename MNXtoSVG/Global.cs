using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Global : IWritable
    {
        public List<string> PartIDs = null;
        public List<Measure> Measures = new List<Measure>();

        public Global(XmlReader r)
        {
            G.Assert(r.Name == "global");
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

            G.ReadToXmlElementTag(r, "measure");

            while(r.Name == "measure")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    Measures.Add(new Measure(r, true));
                }
                G.ReadToXmlElementTag(r, "measure", "global");
            }
            G.Assert(r.Name == "global"); // end of global

            G.Assert(Measures.Count > 0);
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new System.NotImplementedException();
        }




    }
}
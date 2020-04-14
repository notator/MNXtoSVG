using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Tie : Span, IWritable
    {
        public readonly string Target = null;
        public readonly MNXC_PositionInMeasure Location = null;

        public Tie(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-tied-element
            G.Assert(r.Name == "tied");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "target":
                        Target = r.Value;
                        break;
                    case "location":
                        Location = new MNXC_PositionInMeasure(r.Value);
                        break;
                }
            }
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
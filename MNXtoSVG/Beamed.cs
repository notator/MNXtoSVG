using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Beamed : IWritable
    {
        public readonly MNXC_Duration Value = null;
        public readonly string Continue = null;
        public readonly string ID = null;

        internal Sequence Seq => seq;
        private readonly Sequence seq = null;

        public Beamed(XmlReader r)
        {
            G.Assert(r.Name == "beamed");
            // https://w3c.github.io/mnx/specification/common/#the-beamed-element

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "value":
                        Value = new MNXC_Duration(r.Value);
                        break;
                    case "continue":
                        Continue = r.Value;
                        break;
                    case "id":
                        ID = r.Value;
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            seq = new Sequence(r, "beamed", false);

            G.Assert(r.Name == "beamed"); // end of (nested) beamed
        }



        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}

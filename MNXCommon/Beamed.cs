
using MNX.AGlobals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal class Beamed : EventGroup, ISeqComponent
    {
        public readonly DurationSymbol Duration = null;
        public readonly string Continue = null;
        public readonly string ID = null;

        public Beamed(XmlReader r)
        {
            A.Assert(r.Name == "beamed");
            // https://w3c.github.io/mnx/specification/common/#the-beamed-element

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "value":
                        Duration = new DurationSymbol(r.Value, B.CurrentTupletLevel);
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

            Seq = B.GetSequenceContent(r, "beamed", false);

            A.Assert(r.Name == "beamed"); // end of (nested) beamed
        }
    }
}

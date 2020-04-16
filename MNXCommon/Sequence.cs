using System;
using System.Collections.Generic;
using System.Xml;
using MNX.AGlobals;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-sequence-element
    internal class Sequence : EventGroup, ITicks, IPartMeasureComponent
    {
        public readonly Orientation? Orient = null; // default
        public readonly uint? StaffIndex = null; // default
        public readonly string VoiceID = null; // default

        public Sequence(XmlReader r, bool isGlobal)
        {
            A.Assert(r.Name == "sequence");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "orient":
                        if(r.Value == "up")
                            Orient = Orientation.up;
                        else if(r.Value == "down")
                            Orient = Orientation.down;
                        break;
                    case "staff":
                        StaffIndex = UInt32.Parse(r.Value);
                        break;
                    case "voice":
                        VoiceID = r.Value;
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            Seq = B.GetSequenceContent(r, "sequence", isGlobal);

            A.Assert(r.Name == "sequence");
        }
    }
}
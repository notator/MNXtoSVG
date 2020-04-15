using System;
using System.Collections.Generic;
using System.Xml;
using MNX.AGlobals;

namespace MNX.Common
{
    internal class Sequence : ITicks, ISequenceComponent
    {
        public readonly Orientation? Orient = null; // default
        public readonly uint? StaffIndex = null; // default
        public readonly string VoiceID = null; // default

        public readonly List<ISequenceComponent> Seq;

        public int Ticks
        {
            get
            {
                int ticks = 0;
                foreach(ISequenceComponent iw in Seq)
                {
                    if(iw is ITicks it)
                    {
                        ticks += it.Ticks; 
                    }
                }
                return ticks;
            }
        }

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
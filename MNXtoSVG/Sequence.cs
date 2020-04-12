using System;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public enum MNXOrientation
    {
        undefined,
        up,
        down
    }

    internal class Sequence : IWritable
    {
        public readonly MNXOrientation Orientation = MNXOrientation.undefined; // default
        public readonly uint? StaffIndex = null; // default
        public readonly string VoiceID = null; // default

        public readonly List<IWritable> Seq;

        public Sequence(XmlReader r, bool isGlobal)
        {
            G.Assert(r.Name == "sequence");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "orient":
                        if(r.Value == "up")
                            Orientation = MNXOrientation.up;
                        else if(r.Value == "down")
                            Orientation = MNXOrientation.down;
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

            Seq = G.GetSequenceContent(r, "sequence", isGlobal);

            G.Assert(r.Name == "sequence");
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
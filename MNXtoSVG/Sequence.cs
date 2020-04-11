using System;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Sequence : IWritable
    {
        public readonly G.MNXOrientation Orientation = G.MNXOrientation.undefined; // default
        public readonly uint? StaffIndex = null; // default
        public readonly string VoiceID = null; // default

        public readonly List<IWritable> Seq = new List<IWritable>();

        public Sequence(XmlReader r, string caller, bool isGlobal)
        {
            G.Assert(r.Name == caller);
            // https://w3c.github.io/mnx/specification/common/#elementdef-sequence

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "orient":
                        if(r.Value == "up")
                            Orientation = G.MNXOrientation.up;
                        else if(r.Value == "down")
                            Orientation = G.MNXOrientation.down;
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

            G.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "sequence");

            while(r.Name == "directions" || r.Name == "event" || r.Name == "grace" || r.Name == "beamed" || r.Name == "sequence")
            {
                if(r.Name == caller && r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "directions":
                            Seq.Add(new Directions(r, isGlobal));
                            break;
                        case "event":
                            Seq.Add(new Event(r));
                            break;
                        case "grace":
                            Seq.Add(new Grace(r));
                            break;
                        case "beamed":
                            Seq.Add(new Beamed(r));
                            break;
                    }
                }

                G.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "sequence");
            }

            CheckDirections(Seq, isGlobal);

            G.Assert(r.Name == caller); // end of sequence

        }

        private void CheckDirections(List<IWritable> seq, bool isGlobal)
        {
            // check the constraints on the contained directions here!
            // maybe silently correct any errors.
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
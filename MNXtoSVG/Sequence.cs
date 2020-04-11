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

        public Sequence(XmlReader r, bool isBeamed)
        {
            G.Assert(r.Name == "sequence" || isBeamed);
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

            G.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed"); // expand on these later...

            while(r.Name == "directions" || r.Name == "event" || r.Name == "grace" || r.Name == "beamed")
            {
                if(isBeamed && r.Name == "beamed" && r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "directions":
                            Seq.Add(new Directions(r, "sequence", "measure"));
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
                if(isBeamed)
                {
                    G.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed");
                }
                else
                {
                    G.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "sequence");
                }                
            }

            CheckDirections(Seq);

            if(isBeamed)
            {
                G.Assert(r.Name == "beamed"); // end of sequence in beamed
            }
            else
            {
                G.Assert(r.Name == "sequence"); // end of sequence
            }

        }

        private void CheckDirections(List<IWritable> seq)
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
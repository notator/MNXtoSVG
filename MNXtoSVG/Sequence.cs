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

        public readonly Directions Directions;
        public readonly List<IWritable> EventOrBeamed = new List<IWritable>();

        public Sequence(XmlReader r)
        {
            G.Assert(r.Name == "sequence");
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

                G.ReadToXmlElementTag(r, "directions", "event"); // expand on these later...

                while(r.Name == "directions" || r.Name == "event" || r.Name == "beamed")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "directions":
                                Directions = new Directions(r);
                                break;
                            case "event":
                                EventOrBeamed.Add(new Event(r));
                                break;
                            case "beamed":
                                EventOrBeamed.Add(new Beamed(r));
                                break;
                        }
                    }
                    G.ReadToXmlElementTag(r, "directions", "event", "beamed", "sequence");
                }
                G.Assert(r.Name == "sequence"); // end of sequence
            }
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
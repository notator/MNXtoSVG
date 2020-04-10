using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Event : IWritable
    {
        // Style Property
        public readonly G.MNXOrientation StemDirection = G.MNXOrientation.undefined;

        // Compulsory Attribute         
        //   value - the notated metrical duration of this event
        public readonly string Value = null;

        // Optional Attributes 
        //   measure - optional flag indicating that the event occupies the entire measure.
        //   orient - optional orientation of this event
        //   staff - optional staff index of this event
        //   duration - optional performed metrical duration, if different from value
        public readonly bool? Measure = null;
        public readonly G.MNXOrientation Orient = G.MNXOrientation.undefined;
        public readonly int Staff = 0;
        public readonly string Duration = null;

        // Contained objects        
        public readonly List<Note> Notes = null;// Either Notes or Rest must be non-null. Notes.Count can be 0;
        public readonly Rest Rest = null;// Either Notes or Rest must be non-null;
        public readonly List<Slur> Slurs = null;

        public Event(XmlReader r)
        {
            G.Assert(r.Name == "event");
            // https://w3c.github.io/mnx/specification/common/#the-event-element

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "value":
                        Value = r.Value;
                        break;
                    case "measure":
                        G.ThrowError("Not Implemented");
                        break;
                    case "orient":
                        G.ThrowError("Not Implemented");
                        break;
                    case "staff":
                        G.ThrowError("Not Implemented");
                        break;
                    case "duration":
                        Duration = r.Value;
                        break;
                }
            }

            // extend the contained elements as necessary..
            G.ReadToXmlElementTag(r, "note", "rest", "slur");

            while(r.Name == "note" || r.Name == "rest" || r.Name == "slur")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "note":
                            if(Notes == null && Rest == null)
                            {
                                Notes = new List<Note>();
                            }
                            Notes.Add(new Note(r));
                            break;
                        case "rest":
                            if(Notes == null && Rest == null)
                            {
                                Rest = new Rest(r);
                            }
                            break;
                        case "slur":
                            if(Slurs == null)
                            {
                                Slurs = new List<Slur>();
                            }
                            Slurs.Add(new Slur(r));
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "note", "rest", "slur", "event");
            }
            G.Assert(r.Name == "event"); // end of event

        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}

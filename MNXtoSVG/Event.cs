using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-event-element
    /// </summary>
    public class Event : IWritable , ITicks, ITicksSequenceComponent
    {
        #region MNX file attributes
        // Compulsory Attribute         
        //   value - the notated metrical duration of this event  ( /2, /4, /8 etc)
        public readonly MNXC_Duration Duration = null;

        // Optional Attributes 
        //   optional flag indicating that the event occupies the entire measure.    
        public readonly bool? Measure = null;
        //   optional orientation of this event
        public readonly MNXOrientation? StemDirection = null;
        //   optional staff index of this event (also Tuplet, Rest, Note)
        // (1-based) staff index of this tuplet. The spec says that the default is app-specific,
        // and that "The topmost staff in a part has a staff index of 1; staves below the topmost staff
        // are identified with successively increasing indices."
        public readonly int Staff = 1; // app-specific default
        //   duration - optional performed metrical duration, if different from value
        public readonly MNXC_Duration TicksOverride = null;
        #endregion  MNX file attributes

        #region runtime properties
        // Contained objects
        // Either Notes or Rest must be non-null. Notes.Count can be 0;
        public readonly List<Note> Notes = null;
        // Either Notes or Rest must be non-null;
        public readonly Rest Rest = null;
        public readonly List<Slur> Slurs = null;
        // TupletLevel is used when setting Duration.Ticks. It has the following meaning:
        //    0: The Event is not contained in a Tuplet.
        //    1: The Event is contained in a Tuplet.
        //    2: The Event is contained in a Tuplet nested in a Tuplet.
        //    etc. (This app copes with arbitrarily nested tuplets.)
        public readonly int TupletLevel;
        public int Ticks { get { return Duration.Ticks; }}

        #endregion runtime properties

        public Event(XmlReader r)
        {
            TupletLevel = G.CurrentTupletLevel;

            G.Assert(r.Name == "event");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "value":
                        Duration = new MNXC_Duration(r.Value, G.CurrentTupletLevel);
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
                        TicksOverride = new MNXC_Duration(r.Value, G.CurrentTupletLevel);
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
            if(Rest != null)
            {
                Rest.WriteSVG(w);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

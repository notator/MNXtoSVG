using MNX.Globals;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-event-element
    /// </summary>
    internal class Event : IHasTicks, ISeqComponent
    {
        #region MNX file attributes
        // Compulsory Attribute         
        //   value - the notated metrical duration of this event  ( /2, /4, /8 etc)
        public readonly DurationSymbol DSymbol = null;

        // Optional Attributes 
        //   optional flag indicating that the event occupies the entire measure.    
        public readonly bool? Measure = null;
        //   optional orientation of this event
        public readonly Orientation? StemDirection = null;
        //   optional staff index of this event (also Tuplet, Rest, Note)
        // (1-based) staff index of this tuplet. The spec says that the default is app-specific,
        // and that "The topmost staff in a part has a staff index of 1; staves below the topmost staff
        // are identified with successively increasing indices."
        public readonly int Staff = 1; // app-specific default
        //   duration - optional performed metrical duration, if different from value
        public readonly DurationSymbol TicksOverride = null;
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
        public int Ticks
        {
            get
            {
                if( _ticks == 0)
                {
                    _ticks = DSymbol.DefaultTicks;
                }
                return _ticks;
            }
            set
            {
                // this function should only be used when stealing ticks for Grace.
                A.Assert(value >= B.MinimumEventTicks);
                _ticks = value;
            }
        }
        private int _ticks = 0;

        #endregion runtime properties

        public Event(XmlReader r)
        {
            TupletLevel = B.CurrentTupletLevel;

            A.Assert(r.Name == "event");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "value":
                        DSymbol = new DurationSymbol(r.Value, B.CurrentTupletLevel);
                        break;
                    case "measure":
                        A.ThrowError("Not Implemented");
                        break;
                    case "orient":
                        A.ThrowError("Not Implemented");
                        break;
                    case "staff":
                        A.ThrowError("Not Implemented");
                        break;
                    case "duration":
                        TicksOverride = new DurationSymbol(r.Value, B.CurrentTupletLevel);
                        break;
                }
            }

            // extend the contained elements as necessary..
            A.ReadToXmlElementTag(r, "note", "rest", "slur");

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
                A.ReadToXmlElementTag(r, "note", "rest", "slur", "event");
            }
            A.Assert(r.Name == "event"); // end of event

        }
    }
}

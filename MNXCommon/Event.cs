using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-event-element
    /// </summary>
    public class Event : IHasTicks, ISeqComponent
    {
        #region MNX file attributes
        // Compulsory Attribute         
        //   value - the notated metrical duration of this event  ( /2, /4, /8 etc)
        public readonly MNXDurationSymbol MNXDurationSymbol = null;

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
        public readonly MNXDurationSymbol TicksOverride = null;
        public readonly string ID = null;
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
                    _ticks = MNXDurationSymbol.DefaultTicks;
                }
                return _ticks;
            }
            set
            {
                // this function should only be used when stealing ticks for Grace.
                M.Assert(value >= M.MinimumEventTicks);
                _ticks = value;
            }
        }
        private int _ticks = 0;

        public bool IsBeamStart
        {
            get { return _isBeamStart; }
            set
            {
                M.Assert(!_isBeamEnd);
                _isBeamStart = value;
            }
        }
        private bool _isBeamStart = false;

        public bool IsBeamEnd
        {
            get { return _isBeamEnd; }
            set
            {
                M.Assert(!_isBeamStart);
                _isBeamEnd = value;
            }
        }
        private bool _isBeamEnd = false;

        #endregion runtime properties

        #region IUniqueDef
        public override string ToString() => $"Event: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";

        /// <summary>
        /// (?) ISeqComponent objects are already unique, so no Clone is required. (?)
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this;
        }
        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(MsDuration * factor);
            M.Assert(MsDuration > 0, "An Event's MsDuration may not be set to zero!");
        }

        public int MsDuration
        {
            get
            {
                return _msDuration;
            }
            set
            {
                M.Assert(value > 0);
                _msDuration = value;
            }
        }
        private int _msDuration;

        public int MsPositionReFirstUD
        {
            get
            {
                M.Assert(_msPositionReFirstIUD >= 0);
                return _msPositionReFirstIUD;
            }
            set
            {
                M.Assert(value >= 0);
                _msPositionReFirstIUD = value;
            }
        }
        private int _msPositionReFirstIUD = 0;

        public OctaveShift OctaveShift { get { return _octaveShift; } internal set { _octaveShift = value; } }
        private OctaveShift _octaveShift = null;
        public bool EndOctaveShift { get { return _endOctaveShift; } set { _endOctaveShift = value; } }
        private bool _endOctaveShift = false;

        #endregion IUniqueDef

        public Event(XmlReader r)
        {
            TupletLevel = C.CurrentTupletLevel;

            M.Assert(r.Name == "event");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "value":
                        MNXDurationSymbol = new MNXDurationSymbol(r.Value, C.CurrentTupletLevel);
                        break;
                    case "measure":
                        M.ThrowError("Not Implemented");
                        break;
                    case "orient":
                        M.ThrowError("Not Implemented");
                        break;
                    case "staff":
                        M.ThrowError("Not Implemented");
                        break;
                    case "duration":
                        TicksOverride = new MNXDurationSymbol(r.Value, C.CurrentTupletLevel);
                        break;
                    case "id":
                        ID = r.Value;
                        break;
                }
            }

            // extend the contained elements as necessary..
            M.ReadToXmlElementTag(r, "note", "rest", "slur");

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
                M.ReadToXmlElementTag(r, "note", "rest", "slur", "event");
            }
            M.Assert(r.Name == "event"); // end of event

        }

        public void ShiftNoteheadPitches(OctaveShiftType octaveShiftType)
        {
            foreach(var note in Notes)
            {
                note.ShiftNoteheadPitch(octaveShiftType);
            }
        }
    }
}

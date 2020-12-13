using MNX.Globals;

using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-directions
    public class GlobalDirections : IGlobalMeasureComponent
    {
        // These are just the elements used in the first set of examples.
        // Other elements need to be added later.
        public readonly TimeSignature TimeSignature;
        public readonly Clef Clef;
        public readonly KeySignature KeySignature;
        public readonly OctaveShift OctaveShift;
        /// A measure can contain any number of RepeatEnd and RepeatBegin symbols.
        /// They are kept here in order of their PositionInMeasure.Ticks.
        /// If two Repeats have the same ticksPosition, they are kept in order RepeatEnd, RepeatBegin
        public readonly List<Repeat> Repeats;

        public readonly int TicksPosInScore = -1; // set in ctor
        public const int TicksDuration = 0; // all directions have 0 ticks.

        #region IUniqueDef
        public override string ToString() => $"GlobalDirections: TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration}";

        /// <summary>
        /// (?) See IUniqueDef Interface definition. (?)
        /// </summary>
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
            MsDuration = 0;
        }

        public int MsDuration { get { return 0; } set { M.Assert(false, "Application Error."); } }

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

        #endregion IUniqueDef

        public GlobalDirections(XmlReader r, TimeSignature currentTimeSignature, int ticksPosInScore)
        {
            M.Assert(r.Name == "directions");

            TicksPosInScore = ticksPosInScore;

            // These are just the elements used in the first set of examples.
            // Other elements need to be added later.
            M.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift", "repeat", "ending");

            while(r.Name == "time" || r.Name == "clef" || r.Name == "key" || r.Name == "octave-shift"
                || r.Name == "repeat" || r.Name == "ending")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "time":
                            // https://w3c.github.io/mnx/specification/common/#the-time-element
                            TimeSignature = new TimeSignature(r, ticksPosInScore);
                            currentTimeSignature = TimeSignature;
                            break;
                        case "clef":
                            Clef = new Clef(r, ticksPosInScore);
                            break;
                        case "key":
                            // https://w3c.github.io/mnx/specification/common/#the-key-element
                            KeySignature = new KeySignature(r, ticksPosInScore);
                            break;
                        case "octave-shift":
                            OctaveShift = new OctaveShift(r, ticksPosInScore);
                            break;
                        case "repeat":
                            if(Repeats == null)
                            {
                                Repeats = new List<Repeat>();
                            }
                            Repeat repeat = GetRepeat(r, currentTimeSignature);
                            AddRepeatToRepeats(repeat, Repeats);
                            break;
                        case "ending":
                            // TODO
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift", "repeat", "ending", "directions");
            }

            M.Assert(r.Name == "directions"); // end of "directions"
        }

        private void AddRepeatToRepeats(Repeat repeat, List<Repeat> repeats)
        {
            if(repeats.Count == 0)
            {
                repeats.Add(repeat);
            }
            else
            {
                int newTicksPos = repeat.PositionInMeasure.TickPositionInMeasure;
                for(int i = 0; i < repeats.Count; ++i)
                {
                    int existingTicksPos = repeats[i].PositionInMeasure.TickPositionInMeasure;
                    if(existingTicksPos < newTicksPos)
                    {
                        if(i == repeats.Count - 1)
                        {
                            repeats.Add(repeat);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if(existingTicksPos == newTicksPos)
                    {
                        M.Assert((repeat is RepeatEnd && repeats[i] is RepeatBegin) || (repeat is RepeatBegin && repeats[i] is RepeatEnd));
                        if(repeat is RepeatEnd)
                        {
                            repeats.Insert(i, repeat);
                        }
                        else
                        {
                            repeats.Insert(i + 1, repeat);
                        }
                        break;
                    }
                    else if(existingTicksPos > newTicksPos)
                    {
                        repeats.Insert(i, repeat);
                        break;
                    }
                }
            }
        }

        // returns either a RepeatBegin or RepeatEnd
        private Repeat GetRepeat(XmlReader r, TimeSignature currentTimeSignature)
        {
            M.Assert(r.Name == "repeat");

            bool? IsBegin = null;
            string Times = null;
            // when null, this defaults to 0 for RepeatBegin, and measure duration (= current time signature) for RepeatEnd.
            PositionInMeasure PositionInMeasure = null;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "type":
                    {
                        switch(r.Value)
                        {
                            case "start":
                                IsBegin = true;
                                break;
                            case "end":
                                IsBegin = false;
                                break;
                            default:
                                M.ThrowError("Unknown repeat type.");
                                break;

                        }
                        break;
                    }
                    case "times":
                    {
                        M.Assert(int.TryParse(r.Value, out _));
                        Times = r.Value;
                        break;
                    }
                    case "location":
                    {
                        PositionInMeasure = new PositionInMeasure(r.Value);
                        break;
                    }
                    default:
                        M.ThrowError("Unknown repeat attribute.");
                        break;
                }
            }
            // r.Name is now the name of the last repeat attribute that has been read.

            Repeat rval = null;
            switch (IsBegin)
            {
                case true:
                {
                    rval = new RepeatBegin(PositionInMeasure);
                    break;
                }
                case false:
                {
                    M.Assert(currentTimeSignature != null, "TimeSignature must be known here.");
                    rval = new RepeatEnd(PositionInMeasure, currentTimeSignature, Times);
                    break;
                }
                default: // null
                {
                    M.ThrowError("Undefined repeat type.");
                    break;
                }
            }

            return rval;
        }
    }
}
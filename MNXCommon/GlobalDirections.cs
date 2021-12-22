using MNX.Globals;

using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-directions
    public class GlobalDirections : IGlobalMeasureComponent
    {
        // public readonly Ending Ending;
        // public readonly Fine Fine;
        public readonly Jump Jump;
        // public readonly Key Key;
        /// A measure can contain any number of RepeatEnd and RepeatBegin symbols.
        /// They are kept here in order of their PositionInMeasure.Ticks.
        /// If two Repeats have the same ticksPosition, they are kept in order RepeatEnd, RepeatBegin
        public readonly List<Repeat> Repeats;
        // public readonly Segno Segno;
        // public readonly Tempo Tempo;
        public readonly TimeSignature TimeSignature;


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
            M.Assert(r.Name == "directions-global");

            TicksPosInScore = ticksPosInScore;

            M.ReadToXmlElementTag(r, "time", "repeat", "ending", "segno", "jump", "fine", "key", "tempo");

            while(r.Name == "time" || r.Name == "repeat" || r.Name == "ending" || r.Name == "segno"
                || r.Name == "jump" || r.Name == "fine" || r.Name == "key" || r.Name == "tempo")
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
                        case "repeat":
                            if(Repeats == null)
                            {
                                Repeats = new List<Repeat>();
                            }
                            Repeat repeat = GetRepeat(r);
                            AddRepeatToRepeats(repeat, Repeats);
                            break;
                        case "ending":
                            // TODO
                            break;
                        case "segno":
                            // TODO
                            break;
                        case "jump":
                            Jump = new Jump(r, ticksPosInScore);
                            break;
                        case "fine":
                            // TODO
                            break;
                        case "key":
                            // https://w3c.github.io/mnx/specification/common/#the-key-element
                            KeySignature = new KeySignature(r, ticksPosInScore);
                            break;
                        case "tempo":
                            // TODO
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "time", "repeat", "ending", "segno", "jump", "fine", "key", "tempo", "directions-global");
            }

            if(Repeats != null)
            {
                SetDefaultRepeatPositions(Repeats, currentTimeSignature);
            }

            M.Assert(r.Name == "directions-global"); // end of "directions-global"
        }

        private void SetDefaultRepeatPositions(List<Repeat> repeats, TimeSignature currentTimeSignature)
        {
            M.Assert(currentTimeSignature != null);
            foreach(var repeat in repeats)
            {
                repeat.SetDefaultPositionInMeasure(currentTimeSignature);
            }
        }

        private void AddRepeatToRepeats(Repeat repeat, List<Repeat> repeats)
        {
            if((repeats.Count == 0) || (repeat is RepeatEnd && repeat.PositionInMeasure == null))
            {
                repeats.Add(repeat);
            }
            else if(repeat is RepeatBegin && repeat.PositionInMeasure == null)
            {
                repeats.Insert(0, repeat);
            }
            else // mid-measure repeat symbols
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
                        RepeatEndBegin repeatEndBegin;
                        if(repeat is RepeatEnd)
                        {
                            repeatEndBegin = new RepeatEndBegin(repeat as RepeatEnd, repeats[i] as RepeatBegin);
                        }
                        else
                        {
                            repeatEndBegin = new RepeatEndBegin(repeats[i] as RepeatEnd, repeat as RepeatBegin);
                        }
                        repeats.RemoveAt(i);
                        repeats.Insert(i, repeatEndBegin);
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

        // returns either a RepeatBegin or RepeatEnd.
        private Repeat GetRepeat(XmlReader r)
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
                    rval = new RepeatEnd(PositionInMeasure, Times);
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
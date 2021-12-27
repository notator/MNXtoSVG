using MNX.Globals;

using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-directions
    public class GlobalDirections : IGlobalMeasureComponent
    {
        public readonly List<IGlobalDirectionsComponent> Components = new List<IGlobalDirectionsComponent>();
        /// <summary>
        /// The CurrentTimeSignature is continuously updated while the GlobalDirections are being constructed,
        /// and is the final time signature when the Global directions are complete. 
        /// </summary>
        public readonly TimeSignature CurrentTimeSignature;

        public readonly int TicksPosInScore = -1; // set in ctor

        #region IUniqueDef
        public override string ToString() => $"GlobalDirections: TicksPosInScore={TicksPosInScore}";

        /// <summary>
        /// (?) See IUniqueDef Interface definition. (?)
        /// </summary>
        public object Clone()
        {
            return this;
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
            CurrentTimeSignature = currentTimeSignature;
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
                            CurrentTimeSignature = new TimeSignature(r, ticksPosInScore);
                            Components.Add(CurrentTimeSignature);
                            break;
                        case "repeat":
                            Components.Add(GetRepeat(r, ticksPosInScore));
                            break;
                        case "ending":
                            // TODO
                            break;
                        case "segno":
                            Components.Add(new Segno(r, ticksPosInScore));
                            break;
                        case "jump":
                            Components.Add(new Jump(r, ticksPosInScore));
                            break;
                        case "fine":
                            Components.Add(new Fine(r, ticksPosInScore));
                            break;
                        case "key":
                            Components.Add(new KeySignature(r, ticksPosInScore));
                            break;
                        case "tempo":
                            // TODO
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "time", "repeat", "ending", "segno", "jump", "fine", "key", "tempo", "directions-global");
            }

            M.Assert(r.Name == "directions-global"); // end of "directions-global"
        }

        public GlobalDirections(TimeSignature currentTimeSignature, int ticksPosInScore)
        {
            CurrentTimeSignature = currentTimeSignature;
            TicksPosInScore = ticksPosInScore;
            // Components is an empty list
        }

        // returns either a RepeatBegin, RepeatEnd.
        private Repeat GetRepeat(XmlReader r, int ticksPosInScore)
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
                    rval = new RepeatBegin(PositionInMeasure, ticksPosInScore);
                    break;
                }
                case false:
                {
                    rval = new RepeatEnd(PositionInMeasure, Times, ticksPosInScore);
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
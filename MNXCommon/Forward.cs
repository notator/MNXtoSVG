using MNX.Globals;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-event-element
    /// </summary>
    public class Forward : IHasTicksDuration, IEvent, ISequenceComponent
    {
        #region MNX file attributes
        // Compulsory Attribute         
        //   the notated metrical duration of this event  ( /2, /4, /8 etc)
        public readonly MNXDurationSymbol MNXDurationSymbol = null;

        #endregion  MNX file attributes

        public int TicksPosInScore { get; set; }
        public int TicksDuration
        {
            get
            {
                return MNXDurationSymbol.TicksDuration;
            }
            set
            {
                // this function is used when setting tuplet event ticks and when stealing ticks for Grace.
                M.Assert(value >= M.MinimumEventTicks);
                MNXDurationSymbol.TicksDuration = value;
            }
        }

        public int MsPosInScore = -1;
        public override string ToString() => $"Forward: TicksDuration={TicksDuration} TicksPosInScore={TicksPosInScore} MsPosInScore={MsPosInScore} MsDuration={MsDuration}";


        #region IUniqueDef
        /// <summary>
        /// (?) ISeqComponent objects are already unique, so no Clone is required. (?)
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this;
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


        #endregion IUniqueDef

        public Forward(XmlReader r)
        {
            // TicksDuration is initally be related to the default ticksDuration of the MNXDurationSymbol,
            // but it changes if this event is part of a tuplet, and again when the file has been completely
            // read, to accomodate Grace notes.
            // TicksPosInScore is set correctly when the complete file has been parsed.
            TicksPosInScore = 0;

            M.Assert(r.Name == "forward");

            r.MoveToAttribute("duration");
            MNXDurationSymbol = new MNXDurationSymbol(r.Value);


            M.ReadToXmlElementTag(r, "forward");

            M.Assert(r.Name == "forward"); // end of forward

        }
    }
}

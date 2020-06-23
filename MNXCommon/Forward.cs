using MNX.Globals;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-event-element
    /// </summary>
    public class Forward : IHasTicks, ISeqComponent
    {
        #region MNX file attributes
        // Compulsory Attribute         
        //   the notated metrical duration of this event  ( /2, /4, /8 etc)
        public readonly MNXDurationSymbol DSymbol = null;
        #endregion  MNX file attributes

        #region runtime properties
        // TupletLevel is used when setting Duration.Ticks. It has the following meaning:
        //    0: The Forward is not contained in a Tuplet.
        //    1: The Forward is contained in a Tuplet.
        //    2: The Forward is contained in a Tuplet nested in a Tuplet.
        //    etc. (This app copes with arbitrarily nested tuplets.)
        public readonly int TupletLevel;

        public int TicksDuration { get { return DSymbol.DefaultTicks; } }
        public int TicksPosInScore { get; }
        public int MsPosInScore = -1;
        public override string ToString() => $"Forward: TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration} MsPosInScore={MsPosInScore} MsDuration={MsDuration}";
        #endregion runtime properties

        #region IUniqueDef
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
            M.Assert(MsDuration > 0, "A Forward's MsDuration may not be set to zero!");
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

        public Forward(XmlReader r, int ticksPosInScore)
        {
            TupletLevel = C.CurrentTupletLevel;
            TicksPosInScore = ticksPosInScore;

            M.Assert(r.Name == "forward");

            r.MoveToAttribute("duration");
            DSymbol = new MNXDurationSymbol(r.Value, C.CurrentTupletLevel);

            M.ReadToXmlElementTag(r, "forward");

            M.Assert(r.Name == "forward"); // end of forward

        }
    }
}

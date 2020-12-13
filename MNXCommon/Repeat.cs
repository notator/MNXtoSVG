using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public class Repeat : IUniqueDef, IDirectionsComponent
    {
        // when null, this defaults to 0 for RepeatBegin, and measure duration (= current time signature) for RepeatEnd.
        public PositionInMeasure PositionInMeasure { get; private set; } = null;

        #region IUniqueDef
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

        protected Repeat(PositionInMeasure positionInMeasure)
        {
            PositionInMeasure = positionInMeasure;
        }
    }

    public class RepeatBegin : Repeat
    {
        public RepeatBegin(PositionInMeasure positionInMeasure)
         : base((positionInMeasure == null) ? new PositionInMeasure("0") : positionInMeasure)
        {
        }
        public override string ToString()
        {
            return $"RepeatBegin: TickPositionInMeasure={PositionInMeasure.TickPositionInMeasure}";
        }
    }

    public class RepeatEnd : Repeat
    {
        public RepeatEnd(PositionInMeasure positionInMeasure, TimeSignature timeSignature, string times)
         : base((positionInMeasure == null) ? new PositionInMeasure(timeSignature.Signature) : positionInMeasure)
        {
            Times = times;
        }

        public override string ToString()
        {
            string times = (Times == null) ? "null" : Times;
            return $"RepeatEnd: Times={times} TickPositionInMeasure={PositionInMeasure.TickPositionInMeasure}";
        }

        public string Times { get; private set; } = null;

    }


}
using System;
using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public abstract class Repeat : IUniqueDef, IDirectionsComponent
    {
        // when null, this defaults to 0 for RepeatBegin, and measure duration (= current time signature) for RepeatEnd.
        public PositionInMeasure PositionInMeasure { get; protected set; } = null;

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

        protected Repeat()
        {
        }

        internal abstract void SetDefaultPositionInMeasure(TimeSignature currentTimeSignature);
    }

    public class RepeatBegin : Repeat
    {
        public RepeatBegin(PositionInMeasure positionInMeasure)
        {
            PositionInMeasure = positionInMeasure; // can be null
        }

        internal override void SetDefaultPositionInMeasure(TimeSignature currentTimeSignature)
        {
            PositionInMeasure = (PositionInMeasure == null) ? new PositionInMeasure("0") : PositionInMeasure;
        }

        public override string ToString()
        {
            string tickPos = (PositionInMeasure == null) ? "null" : $"{PositionInMeasure.TickPositionInMeasure}";
            return $"RepeatBegin: TickPositionInMeasure={tickPos}";
        }
    }

    public class RepeatEnd : Repeat
    {
        public RepeatEnd(PositionInMeasure positionInMeasure, string times)
        {
            PositionInMeasure = positionInMeasure; // can be null
            Times = times;
        }

        internal override void SetDefaultPositionInMeasure(TimeSignature currentTimeSignature)
        {
            PositionInMeasure = (PositionInMeasure == null) ? new PositionInMeasure(currentTimeSignature.Signature) : PositionInMeasure;
        }

        public override string ToString()
        {
            string times = (Times == null) ? "null" : Times;
            string tickPos = (PositionInMeasure == null) ? "null" : $"{PositionInMeasure.TickPositionInMeasure}";
            return $"RepeatEnd: Times={times} TickPositionInMeasure={tickPos}";
        }

        public string Times { get; private set; } = null;

    }

    /// <summary>
    /// Note that RepeatEndBegin symbols only occur mid-measure.
    /// They never occur on the barlines at each end of a measure,
    /// but exist to simplify horizontal justification.
    /// </summary>
    public class RepeatEndBegin : Repeat
    {
        public RepeatEndBegin(RepeatEnd repeatEnd, RepeatBegin repeatBegin)
        {
            M.Assert(repeatEnd.PositionInMeasure == repeatBegin.PositionInMeasure);

            PositionInMeasure = repeatEnd.PositionInMeasure;
            Times = repeatEnd.Times;
        }

        internal override void SetDefaultPositionInMeasure(TimeSignature currentTimeSignature)
        {
            M.Assert(false, "This function should never be called.");
        }

        public override string ToString()
        {
            string times = (Times == null) ? "null" : Times;
            string tickPos = (PositionInMeasure == null) ? "null" : $"{PositionInMeasure.TickPositionInMeasure}";
            return $"RepeatEndBegin: Times={times} TickPositionInMeasure={tickPos}";
        }

        public string Times { get; private set; } = null;

    }


}
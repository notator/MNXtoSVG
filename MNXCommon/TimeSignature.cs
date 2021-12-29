﻿using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public class TimeSignature : IUniqueDef, IGlobalDirectionsComponent
    {
        public readonly string Signature;
        public readonly string Measure;
        public int TicksPosInScore { get { return _ticksPosInScore; } }
        private readonly int _ticksPosInScore;

        #region IUniqueDef
        public override string ToString() => $"TimeSignature: {Signature} TicksPosInScore={TicksPosInScore} MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";
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

        public int TicksDuration { get; }

        private int _msPositionReFirstIUD = 0;

        #endregion IUniqueDef

        public TimeSignature(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "time");
            _ticksPosInScore = ticksPosInScore;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "signature":
                        Signature = r.Value;
                        break;
                    case "measure":
                        Measure = r.Value;
                        break;
                    default:
                        M.ThrowError("Unknown time attribute.");
                        break;
                }
            }

            MNXDurationSymbol mds = new MNXDurationSymbol(Signature, 0);
            TicksDuration = mds.TicksDuration;

            // r.Name is now the name of the last time attribute that has been read.
        }
    }
}
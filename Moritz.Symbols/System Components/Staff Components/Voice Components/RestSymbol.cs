﻿using System;
using System.Text;
using System.Diagnostics;

using MNX.Globals;
using Moritz.Xml;
using Moritz.Spec;
using MNX.Common;
using System.Collections.Generic;

namespace Moritz.Symbols
{
	internal abstract class RestSymbol : DurationSymbol
	{
        public RestSymbol(Voice voice, int msDuration, int absMsPosition, MNX.Common.Event mnxEventDef, double fontSize)
            : base(voice, msDuration, absMsPosition, mnxEventDef.MNXDurationSymbol, fontSize)
        {
            TupletDefs = mnxEventDef.TupletDefs; // can be null
        }

        /// <summary>
        /// Old constructor, currently not used (03.05.2020), but retained for future inspection
        /// </summary>
        public RestSymbol(Voice voice, IUniqueDef iumdd, int absMsPosition, int minimumCrotchetDurationMS, double fontHeight)
            : base(voice, iumdd.MsDuration, absMsPosition, minimumCrotchetDurationMS, fontHeight)
        {
            M.Assert(false); // 03.05.2020: don't use this constructor (to be inspected once work on midi info begins).

            if(iumdd is CautionaryChordDef)
            {
                Console.WriteLine("rest is CautionaryChordDef!");
            }
            LocalCautionaryChordDef = iumdd as CautionaryChordDef;
        }

		#region display attributes
        /// <summary>
        /// If LocalizedCautionaryChordDef is set:
        /// a) this rest is used like any other rest when justifying systems, but
        /// b) it is not displayed, and does not affect the temporal positions or durations of any chords. 
        /// </summary>
        public CautionaryChordDef LocalCautionaryChordDef = null;
		#endregion display attributes
		#region verticalPos attributes
		public bool Centered = false; // capella default
		public int Shift_Gap = 0; // capella default
		#endregion verticalPos attributes

        /// <summary>
        /// Returns this.Metrics cast to RestMetrics.
        /// Before accessing this property, this.Metrics must be assigned to an object of type RestMetrics.
        /// </summary>
        internal RestMetrics RestMetrics
        {
            get
            {
                RestMetrics restMetrics = Metrics as RestMetrics;
                M.Assert(restMetrics != null);
                return restMetrics;
            }
        }

        // Rest MsDuration should only be set when agglommerating consecutive Rests
        public override int MsDuration
        {
            get { return _msDuration; }
            set { _msDuration = value; } 
        }
    }
}

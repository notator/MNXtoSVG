using MNX.Common;
using MNX.Globals;
using Moritz.Spec;
using Moritz.Xml;
using System;

namespace Moritz.Symbols
{
    internal class OutputRestSymbol : RestSymbol
	{
        public OutputRestSymbol(Voice voice, MNX.Common.Event mnxEventDef, int absMsPosition, PageFormat pageFormat)
            :base(voice, mnxEventDef.MsDuration, absMsPosition, mnxEventDef.MNXDurationSymbol, pageFormat.MusicFontHeight)
        {

        }

        /// <summary>
        /// Old constructor, currently not used (03.05.2020), but retained for future inspection
        /// </summary>
        public OutputRestSymbol(Voice voice, IUniqueDef iumdd, int absMsPosition, PageFormat pageFormat)
			: base(voice, iumdd, absMsPosition, pageFormat.MinimumCrotchetDuration, pageFormat.MusicFontHeight)
        {
            M.Assert(false); // 03.05.2020: don't use this constructor (to be inspected once work on midi info begins).

            if(iumdd is MidiRestDef mrd)
            {
                _midiRestDef = mrd;
            }

			// This needs testing!!
			if(iumdd is CautionaryChordDef ccd)
            {
                Console.WriteLine("rest is CautionaryChordDef!");
                LocalCautionaryChordDef = ccd;
            }
        }

		/// <summary>
		/// This function should never be used. Use the other WriteSVG() instead.
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w)
		{
			M.Assert(false, "This function should never be called.");
		}

		public void WriteSVG(SvgWriter w, int channel, CarryMsgs carryMsgs, bool graphicsOnly)
        {
            if(LocalCautionaryChordDef == null)
            {
                M.Assert(_msDuration > 0);

				w.SvgStartGroup(CSSObjectClass.rest.ToString()); // "rest"

				if(!graphicsOnly)
				{
					w.WriteAttributeString("score", "alignment", null, ((Metrics.Left + Metrics.Right) / 2).ToString(M.En_USNumberFormat));

					_midiRestDef.WriteSVG(w, channel, carryMsgs);
				}

                if(this.Metrics != null)
                {
                    ((RestMetrics)this.Metrics).WriteSVG(w);
                }

                w.SvgEndGroup(); // "rest"
            }
        }

		public override string ToString() => "outputRest " + InfoString;

		MidiRestDef _midiRestDef = null;
    }
}

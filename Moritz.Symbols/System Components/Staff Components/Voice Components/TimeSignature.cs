using System;
using System.Text;
using MNX.Common;
using Moritz.Xml;
using MNX.Globals;

namespace Moritz.Symbols
{
	public class TimeSignature : NoteObject
	{
        public readonly string Signature;
        public ColorString CapellaColor = new ColorString("000000");

        public TimeSignature(Voice voice, MNX.Common.TimeSignature mnxTimeSigDef, double fontHeight)
            : base(voice)
        {
            Signature = mnxTimeSigDef.Signature;
            FontHeight = fontHeight;
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Writes a time signature to the SVG file.
        /// The metrics have been set in SvgSystem.Justify()
        /// </summary>
        public void WriteSVG(SvgWriter w, string timeSigSignature, double originX, double originY)
        {
            Metrics.WriteSVG(w);
        }

        public override string ToString()
		{
			return "TimeSignature: " + Signature;
		}
    }
}

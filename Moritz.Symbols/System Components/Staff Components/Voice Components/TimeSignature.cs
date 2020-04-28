using System;
using System.Text;
using MNX.Common;
using Moritz.Xml;
using MNX.Globals;

namespace Moritz.Symbols
{
	public class TimeSignature : NoteObject
	{
        private string _timeSigType = null;

        /// <summary>
        /// Creates a new time signature
        /// </summary>
        public TimeSignature(Voice voice, MNX.Common.TimeSignature mnxTimeSigDef, double fontHeight)
            : base(voice)
        {
            _timeSigType = mnxTimeSigDef.Signature; // e.g. "4/4"
            _fontHeight = fontHeight;
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Writes a time signature to the SVG file.
        /// The metrics have been set in SvgSystem.Justify()
        /// </summary>
        public void WriteSVG(SvgWriter w, string timeSigType, double originX, double originY)
        {
            CSSObjectClass timeSignatureClass = CSSObjectClass.timeSignature;                    
            w.SvgUseXY(timeSignatureClass, timeSigType, originX, originY);
        }

        public override string ToString()
		{
			return "TimeSignature: " + _timeSigType;
		}

		public string TimeSigType
		{
			get { return _timeSigType; }
			set
			{
                _timeSigType = value;
			}
		}
		public ColorString CapellaColor = new ColorString("000000");

    }
}

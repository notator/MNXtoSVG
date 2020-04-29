using System;
using System.Text;
using MNX.Common;
using Moritz.Xml;
using MNX.Globals;

namespace Moritz.Symbols
{
	public class KeySignature : NoteObject
	{
        public readonly int Fifths;
        public ColorString CapellaColor = new ColorString("000000");

        public KeySignature(Voice voice, MNX.Common.KeySignature mnxKeySigDef, double fontHeight)
            : base(voice)
        {
            Fifths = mnxKeySigDef.Fifths;
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
        public void WriteSVG(SvgWriter w, string fifths, double originX, double originY)
        {
            CSSObjectClass keySignatureClass = CSSObjectClass.keySignature;                    
            w.SvgUseXY(keySignatureClass, fifths, originX, originY);
        }

        public override string ToString()
		{
			return "KeySignature: " + Fifths.ToString();
		}
    }
}

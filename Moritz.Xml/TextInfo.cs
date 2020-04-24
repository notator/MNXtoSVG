
using System;
using System.Xml;
using System.Text;
using System.Diagnostics;

using MNX.Globals;

namespace Moritz.Xml
{
	public class TextInfo
	{
        public TextInfo(string text, string fontFamily, double fontHeight, TextHorizAlign textHorizAlign)
            : this(text, fontFamily, fontHeight, new ColorString("000000"), textHorizAlign)
        {
        }

        public TextInfo(string text, string fontFamily, double fontHeight, ColorString colorString, 
            TextHorizAlign textHorizAlign)
        {
            M.Assert(!String.IsNullOrEmpty(text));
            _text = text;
            _fontFamily = fontFamily;
            _fontHeight = fontHeight;
            _textHorizAlign = textHorizAlign;
            _colorString = colorString;
        }

        public string FontFamily { get { return _fontFamily; } }
        private readonly string _fontFamily = null;
        
        public string Text { get { return _text; } }
        private readonly string _text = null;

        public double FontHeight { get { return _fontHeight; } }
        private readonly double _fontHeight = 0;

        public TextHorizAlign TextHorizAlign { get { return _textHorizAlign; } }
        private readonly TextHorizAlign _textHorizAlign = 0;

        /// <summary>
		/// A string of 6 Hex digits (RRGGBB).
		/// </summary>
        public ColorString ColorString { get { return _colorString; } }
        private readonly ColorString _colorString = null; 
    }
}

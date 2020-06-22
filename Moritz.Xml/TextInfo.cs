
using System;
using System.Xml;
using System.Text;
using System.Diagnostics;

using MNX.Globals;


namespace Moritz.Xml
{
	public class TextInfo
	{
        public TextInfo(string text, string fontFamily, double fontHeight, SVGFontWeight svgFontWeight, SVGFontStyle svgFontStyle,
            ColorString colorString, TextHorizAlign textHorizAlign)
        {
            M.Assert(!String.IsNullOrEmpty(text));
            _text = text;
            _fontFamily = fontFamily;
            _svgFontWeight = svgFontWeight;
            _svgFontStyle = svgFontStyle;
            _fontHeight = fontHeight;
            _textHorizAlign = textHorizAlign;
            _colorString = colorString;
        }

        public TextInfo(string text, string fontFamily, double fontHeight, SVGFontWeight fontWeight, SVGFontStyle fontStyle, TextHorizAlign textHorizAlign)
            :this( text, fontFamily, fontHeight, fontWeight, fontStyle, new ColorString("000000"), textHorizAlign)
        {

        }


        public TextInfo(string text, string fontFamily, double fontHeight, ColorString colorString, TextHorizAlign textHorizAlign)
            : this(text, fontFamily, fontHeight, SVGFontWeight.normal, SVGFontStyle.normal, colorString, textHorizAlign)
        {

        }

        public TextInfo(string text, string fontFamily, double fontHeight, TextHorizAlign textHorizAlign)
            : this(text, fontFamily, fontHeight, new ColorString("000000"), textHorizAlign)
        {
        }


        public string FontFamily { get { return _fontFamily; } }
        private readonly string _fontFamily = null;

        public SVGFontStyle SVGFontStyle { get { return _svgFontStyle; } }
        private readonly SVGFontStyle _svgFontStyle = SVGFontStyle.normal;

        public SVGFontWeight SVGFontWeight { get { return _svgFontWeight; } }
        private readonly SVGFontWeight _svgFontWeight = SVGFontWeight.normal;

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

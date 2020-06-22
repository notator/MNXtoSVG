using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class TextMetrics : TextStyle
    {
        public TextMetrics(CSSObjectClass cssClass, Graphics graphics, TextInfo textInfo)
            : base(cssClass, textInfo.FontFamily, textInfo.FontHeight, textInfo.SVGFontWeight, textInfo.SVGFontStyle, textInfo.TextHorizAlign, textInfo.ColorString.String)
        {
            SetDefaultMetrics(graphics, textInfo);
            _textInfo = textInfo;
        }

        /// <summary>
        /// Used by Clone(cssClass)
        /// </summary>
        private TextMetrics(CSSObjectClass cssClass, double top, double right, double bottom, double left, double originX, double originY, TextInfo textInfo)
            : base(cssClass, textInfo.FontFamily, textInfo.FontHeight, textInfo.SVGFontWeight, textInfo.SVGFontStyle, textInfo.TextHorizAlign, textInfo.ColorString.String)
        {
            _top = top;
            _right = right;
            _bottom = bottom;
            _left = left;
            _originX = originX;
            _originY = originY;
            _textInfo = textInfo;
        }

        internal TextMetrics Clone(CSSObjectClass cssClass)
        {
            return new TextMetrics(cssClass, Top, Right, Bottom, Left, OriginX, OriginY, _textInfo);
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgText(CSSObjectClass, _textInfo.Text, _originX, _originY);
        }

        /// <summary>
        /// Sets the default Top, Right, Bottom, Left.
        ///   1. the width of the text is set to the value returned by MeasureText() (no padding)
        ///   2. the top and bottom metrics are set to values measured experimentally, using my
        ///   program: "../_demo projects/MeasureTextDemo/MeasureTextDemo.sln"
        ///		 _top is usually set here to the difference between the top and bottom line positions in that program
        ///		 _bottom is always set here to 0
        ///		 The fonts currently supported are:
        ///         "Open Sans"
        ///         "Open Sans Condensed"
        ///         "Arial"
        ///      These fonts have to be added to the Assistant Performer's fonts folder, and to its fontStyleSheet.css
        ///      so that they will work on any operating system.
        ///   3. moves the Metrics horizontally to take account of the textinfo.TextHorizAlign setting,
        ///      leaving OriginX and OriginY at 0F.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="textInfo"></param>
        private void SetDefaultMetrics(Graphics graphics, TextInfo textInfo)
        {
            //double maxFontSize = System.Single.MaxValue - 10;
            double maxFontSize = 1000;
            Size textMaxSize = new Size();
            try
            {
                textMaxSize = MeasureText(graphics, textInfo.Text, textInfo.FontFamily, maxFontSize);
            }
            catch(Exception ex)
            {
                M.Assert(false, ex.Message);
            }
            _left = 0;
            _right = textInfo.FontHeight * textMaxSize.Width / maxFontSize;
            switch(textInfo.FontFamily)
            {
                case "Open Sans": // titles
                case "Open Sans Condensed": // ornaments
                    _top = textInfo.FontHeight * -0.699; // The difference between the height
                    _bottom = 0;
                    break;
                case "Arial": // date stamp, lyrics, staff names
                              //_top = textInfo.FontHeight * -0.818; // using MeasureTextDemo
                    _top = textInfo.FontHeight * -0.71; // by experiment!
                    _bottom = 0;
                    break;
                //case "Times New Roman": // staff names
                //	_top = textInfo.FontHeight * -1.12;
                //	_bottom = 0;
                //	break;
                default:
                    M.Assert(false, "Unknown font");
                    break;
            }

            if(textInfo.TextHorizAlign == TextHorizAlign.center)
                Move(-(_right / 2F), 0F);
            else if(textInfo.TextHorizAlign == TextHorizAlign.right)
                Move(-_right, 0F);

            _originX = 0;
            _originY = 0; // SVG originY is the baseline of the text
        }

        private Size MeasureText(Graphics g, string text, string fontFace, double fontHeight)
        {
            Size maxSize = new Size(int.MaxValue, int.MaxValue);
            TextFormatFlags flags = TextFormatFlags.NoPadding;
            Size sizeOfString;
            using(Font sysFont = new Font(fontFace, (float)fontHeight))
            {
                sizeOfString = TextRenderer.MeasureText(g, text, sysFont, maxSize, flags);
            }
            return sizeOfString;
        }

        private readonly TextInfo _textInfo = null;
    }

    public class StaffNameMetrics : TextMetrics
    {
        public StaffNameMetrics(CSSObjectClass staffClass, Graphics graphics, TextInfo textInfo)
            : base(staffClass, graphics, textInfo)
        {
        }
    }

    internal class LyricMetrics : TextMetrics, ICloneable
    {
        public LyricMetrics(double gap, Graphics graphics, TextInfo textInfo, bool isBelow, CSSObjectClass lyricClass)
            : base(lyricClass, graphics, textInfo)
        {
            double width = _right - _left;
            double newWidth = width * 0.75;
            double widthMargin = (width - newWidth) / 2.0;
            _left += widthMargin;
            _right -= widthMargin;

            IsBelow = isBelow;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public readonly bool IsBelow;
    }
    internal class OrnamentMetrics : TextMetrics, ICloneable
    {
        public OrnamentMetrics(Graphics graphics, TextInfo textInfo, bool isBelow)
            : base(CSSObjectClass.ornament, graphics, textInfo)
        {
            IsBelow = isBelow;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public readonly bool IsBelow;
    }
}


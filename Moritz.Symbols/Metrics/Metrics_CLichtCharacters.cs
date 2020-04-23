﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class TextStyle : Metrics
    {
        public TextStyle(CSSObjectClass cssTextClass, string fontFamily, double fontHeight, TextHorizAlign textAnchor = TextHorizAlign.left, string fill = "black")
            : base(cssTextClass)
        {
            FontFamily = fontFamily;
            FontHeight = fontHeight;
            switch(textAnchor)
            {
                case (TextHorizAlign.left):
                    TextAnchor = "left";
                    break;
                case (TextHorizAlign.center):
                    TextAnchor = "middle";
                    break;
                case (TextHorizAlign.right):
                    TextAnchor = "right";
                    break;
            }
            if(fill == "#FFFFFF" || fill == "FFFFFF")
            {
                Fill = "white";
            }
            else if(fill == "#000000" || fill == "000000")
            {
                Fill = "black";
            }
            else if(fill == "none" || fill == "white" || fill == "black" || fill == "red")
            {
                Fill = fill;
            }
            else
            {
                if(fill[0] != '#')
                {
                    fill.Insert(0, "#");
                }
                M.Assert(Regex.IsMatch(fill, @"^#[0-9a-fA-F]{6}$"));
                Fill = fill; // a string of the form "#AAAAAA"
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public readonly string FontFamily = ""; // "Arial", "CLicht", "Open Sans", "Open Sans Condensed"
        public readonly double FontHeight = 0F;       
        public readonly string TextAnchor; // "left", "middle", "right"
        public readonly string Fill; // "none", "black", "white", "red", #AAAAAA" etc
    }

    internal class CLichtCharacterMetrics : TextStyle
	{
        /// <summary>
        /// Used by DynamicMetrics
        /// </summary>
		public CLichtCharacterMetrics(string characterString, double fontHeight, TextHorizAlign textHorizAlign, CSSObjectClass dynamicClass)
			: base(dynamicClass, "CLicht", fontHeight, textHorizAlign)
		{
			_characterString = characterString;

			M.Assert(_characterString != null);
			Metrics m = CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX[_characterString];

			_originY = 0;
			_top = m.Top * fontHeight;
			_bottom = m.Bottom * fontHeight;

			// move so that Left = 0.
			_left = 0;
			_right = (m.Right - m.Left) * fontHeight;
			_originX = -m.Left * fontHeight;

			_fontHeight = fontHeight;
			_textHorizAlign = textHorizAlign;
		}

        /// <summary>
        /// Used by RestMetrics and HeadMetrics
        /// </summary>
		public CLichtCharacterMetrics(DurationClass durationClass, double fontHeight, CSSObjectClass cssClass)
			: base(cssClass, "CLicht", fontHeight)
		{
			_characterString = GetClichtCharacterString(durationClass, cssClass == CSSObjectClass.rest);

			M.Assert(_characterString != null);
			Metrics m = CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX[_characterString];

			_originY = 0;
			_top = m.Top * fontHeight;
			_bottom = m.Bottom * fontHeight;

			// move so that Left = 0.
			_left = 0;
			_right = (m.Right - m.Left) * fontHeight;
			_originX = -m.Left * fontHeight;

			_fontHeight = fontHeight;
		}

        /// <summary>
        /// Used by AccidentalMetrics
        /// </summary>
		public CLichtCharacterMetrics(Head head, double fontHeight, CSSObjectClass cssClass)
			: base(cssClass, "CLicht", fontHeight)
		{
			_characterString = GetClichtCharacterString(head);

			M.Assert(_characterString != null);
			Metrics m = CLichtFontMetrics.CLichtGlyphBoundingBoxesDictPX[_characterString];

			_originY = 0;
			_top = m.Top * fontHeight;
			_bottom = m.Bottom * fontHeight;

			// move so that Left = 0.
			_left = 0;
			_right = (m.Right - m.Left) * fontHeight;
			_originX = -m.Left * fontHeight;

			_fontHeight = fontHeight;
		}

        public override void WriteSVG(SvgWriter w)
        {
			w.SvgText(CSSObjectClass, _characterString, _originX, _originY);
		}

		/// <summary>
		/// Clefs
		/// </summary>
		/// <param name="clefName">The Assistant Composer's name for the clef (e.g. "t1")</param>
		/// <returns></returns>
		private string GetClichtCharacterString(string clefName)
		{
			string cLichtCharacterString = null;
			switch(clefName)
			{
				#region clefs
				case "t": // trebleClef
				case "t1": // trebleClef8
				case "t2": // trebleClef2x8
				case "t3": // trebleClef3x8
					// N.B. t1, t2 and t3 are realised as <def> objects in combination with texts 8, 2x8 and 3x8.
					// cLicht's trebleclefoctavaalt character is not used.
					cLichtCharacterString = "&";
					break;
				case "b":
				case "b1": // bassClef8
				case "b2": // bassClef2x8
				case "b3": // bassClef3x8
					// N.B. b1, b2 and b3 are realised as <def> objects in combination with texts 8, 2x8 and 3x8.
					// cLicht's bassclefoctavaalt character is not used.
					cLichtCharacterString = "?";
					break;
				#endregion
			}
			return cLichtCharacterString;
		}
		/// <summary>
		/// Rests and noteheads
		/// </summary>
		/// <param name="durationClass"></param>
		/// <returns></returns>
		private string GetClichtCharacterString(DurationClass durationClass, bool isRest)
		{
			string cLichtCharacterString = null;
			if(isRest)
			{
				switch(durationClass)
				{
					#region rests
					case DurationClass.breve:
					case DurationClass.semibreve:
						cLichtCharacterString = "∑";
						break;
					case DurationClass.minim:
						cLichtCharacterString = "Ó";
						break;
					case DurationClass.crotchet:
						cLichtCharacterString = "Œ";
						break;
					case DurationClass.quaver:
						cLichtCharacterString = "‰";
						break;
					case DurationClass.semiquaver:
						cLichtCharacterString = "≈";
						break;
					case DurationClass.threeFlags:
						cLichtCharacterString = "®";
						break;
					case DurationClass.fourFlags:
						cLichtCharacterString = "Ù";
						break;
					case DurationClass.fiveFlags:
						cLichtCharacterString = "Â";
						break;
					#endregion
				}
			}
			else
			{
				switch(durationClass)
				{
					case DurationClass.breve:
						cLichtCharacterString = "›";
						break;
					case DurationClass.semibreve:
						cLichtCharacterString = "w";
						break;
					case DurationClass.minim:
						cLichtCharacterString = "˙";
						break;
					default:
						cLichtCharacterString = "œ";
						break;
				}
			}
			return cLichtCharacterString;
		}
		/// <summary>
		/// Accidentals
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private string GetClichtCharacterString(Head head)
		{
			string cLichtCharacterString = null;
			switch(head.Alteration)
			{
				case -1:
					cLichtCharacterString = "b";
					break;
				case 0:
					cLichtCharacterString = "n";
					break;
				case 1:
					cLichtCharacterString = "#";
					break;
				default:
					M.Assert(false, "unknown accidental type");
					break;
			}
			return cLichtCharacterString;
		}

        public string CharacterString { get { return _characterString; } }
        protected string _characterString = "";
		protected double _fontHeight;
		protected TextHorizAlign _textHorizAlign = TextHorizAlign.left;
    }
    internal class RestMetrics : CLichtCharacterMetrics
	{
		public RestMetrics(Graphics graphics, RestSymbol rest, double gap, int numberOfStafflines, double ledgerlineStrokeWidth, CSSObjectClass restClass)
			: base(rest.DurationClass, rest.FontHeight, restClass)
		{
			double dy = 0;
			if(numberOfStafflines > 1)
				dy = gap * (numberOfStafflines / 2);

			_top = _top + dy;
			_bottom += dy;
			_originY += dy; // the staffline on which the rest is aligned
			_ledgerlineStub = gap * 0.75F;
			Move((Left - Right) / 2, 0F); // centre the glyph horizontally
            CSSObjectClass llBlockClass = CSSObjectClass.ledgerlines;
			switch(rest.DurationClass)
			{
				case DurationClass.breve:
				case DurationClass.semibreve:
					Move(gap * -0.25, 0F);
					if(numberOfStafflines == 1)
						Move(0, gap);
					_ledgerlineBlockMetrics = new LedgerlineBlockMetrics(Left - _ledgerlineStub, Right + _ledgerlineStub, ledgerlineStrokeWidth, llBlockClass);
					_ledgerlineBlockMetrics.AddLedgerline(_originY - gap, 0F);
					_ledgerlineBlockMetrics.Move(gap * 0.17, 0F);
					_top -= (gap * 1.5F);
					break;
				case DurationClass.minim:
					Move(gap * 0.18, 0);
					_ledgerlineBlockMetrics = new LedgerlineBlockMetrics(Left - _ledgerlineStub, Right + _ledgerlineStub - (gap * 0.3F), ledgerlineStrokeWidth, llBlockClass);
					_ledgerlineBlockMetrics.AddLedgerline(_originY, 0F);
					_bottom += (gap * 1.5F);
					break;
				case DurationClass.quaver:
					_top -= gap * 0.5F;
					_bottom += gap * 0.5F;
					break;
				case DurationClass.semiquaver:
					_top -= gap * 0.5F;
					_bottom += gap * 0.5F;
					break;
				case DurationClass.threeFlags:
					_top -= gap * 0.5F;
					_right += gap * 0.2F;
					_bottom += gap * 0.5F;
					_left -= gap * 0.2F;
					break;
				case DurationClass.fourFlags:
					_top -= gap * 0.5F;
					_right += gap * 0.1F;
					_bottom += gap * 1.25F;
					_left -= gap * 0.1F;
					_originY += gap;
					break;
				case DurationClass.fiveFlags:
					_top -= gap * 1.5F;
					_right += gap * 0.2F;
					_bottom += gap * 1.25F;
					_left -= gap * 0.2F;
					_originY += gap;
					break;
			}

		}

        public override void Move(double dx, double dy)
		{
			base.Move(dx, dy);
			if(_ledgerlineBlockMetrics != null)
				_ledgerlineBlockMetrics.Move(dx, dy);
		}

        public override void WriteSVG(SvgWriter w)
        {
            w.WriteStartElement("g");
            w.WriteAttributeString("class", "graphics");

			w.SvgText(CSSObjectClass, _characterString, _originX, _originY);

            if(_ledgerlineBlockMetrics != null && _ledgerlineVisible)
            {
                _ledgerlineBlockMetrics.WriteSVG(w);
            }

            w.WriteEndElement(); // g graphics
        }

        /// <summary>
        /// Ledgerlines exist in breve, semibreve and minim rests.
        /// They are made visible when the rest is moved outside the staff on 2-voice staves.
        /// </summary>
        public bool LedgerlineVisible
		{
			set
			{
				if(_ledgerlineBlockMetrics != null && (!_ledgerlineVisible && value))
				{
					double width = _ledgerlineBlockMetrics.Right - _ledgerlineBlockMetrics.Left;
					double padding = width * 0.05F;
					_left -= (_ledgerlineStub + padding);
					_right += _ledgerlineStub + padding;
					_ledgerlineVisible = value;
				}
			}
		}
		private readonly double _ledgerlineStub;
		private bool _ledgerlineVisible = false;
		private LedgerlineBlockMetrics _ledgerlineBlockMetrics = null;
	}
	internal class HeadMetrics : CLichtCharacterMetrics
	{
		public HeadMetrics(ChordSymbol chord, Head head, double gapVBPX, CSSObjectClass headClass)
			: base(chord.DurationClass, chord.FontHeight, headClass)
		{
			Move((Left - Right) / 2, 0F); // centre horizontally

			double horizontalPadding = chord.FontHeight * 0.04F;
			_leftStemX = _left;
			_rightStemX = _right;
			_left -= horizontalPadding;
			_right += horizontalPadding;
            if(head != null)
            {
				CSSColorClass = head.ColorClass;
            }
		}

		/// <summary>
		/// Used when creating temporary heads for chord alignment purposes.
		/// </summary>
		public HeadMetrics(HeadMetrics otherHead, DurationClass durationClass)
			: base(durationClass, otherHead.FontHeight, CSSObjectClass.none)
		{
			// move to position of other head
			Move(otherHead.OriginX - _originX, otherHead.OriginY - OriginY);

			double horizontalPadding = otherHead.FontHeight * 0.04F;
			_leftStemX = _left;
			_rightStemX = _right;
			_left -= horizontalPadding;
			_right += horizontalPadding;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		/// <summary>
		/// Notehead metrics.Left and metrics.Right include horizontal padding,
		/// so head overlaps cannot be checked using the standard Metrics.Overlaps function.
		/// </summary>
		public bool OverlapsHead(HeadMetrics otherHeadMetrics)
		{
			// See the above constructor. Sorry, I didnt want to save the value in every Head!
			double thisHorizontalPadding = this._fontHeight * 0.04F;
			double thisRealLeft = _left + thisHorizontalPadding;
			double thisRealRight = _right - thisHorizontalPadding;

			double otherHorizontalPadding = otherHeadMetrics.FontHeight * 0.04F;
			double otherRealLeft = otherHeadMetrics.Left + thisHorizontalPadding;
			double otherRealRight = otherHeadMetrics.Right - thisHorizontalPadding;

			bool verticalOverlap = this.Bottom >= otherHeadMetrics.Top && this.Top <= otherHeadMetrics.Bottom;
			bool horizontalOverlap = thisRealRight >= otherRealLeft && thisRealLeft <= otherRealRight;

			return verticalOverlap && horizontalOverlap;
		}
		/// <summary>
		/// Notehead metrics.Left and metrics.Right include horizontal padding,
		/// so head overlaps cannot be checked using the standard Metrics.Overlaps function.
		/// </summary>
		public bool OverlapsStem(StemMetrics stemMetrics)
		{
			// See the above constructor. Sorry, I didnt want to save the value in every Head!
			double thisHorizontalPadding = this._fontHeight * 0.04F;
			double thisRealLeft = _left + thisHorizontalPadding;
			double thisRealRight = _right - thisHorizontalPadding;

			bool verticalOverlap = this.Bottom >= stemMetrics.Top && this.Top <= stemMetrics.Bottom;
			bool horizontalOverlap = thisRealRight >= stemMetrics.Left && thisRealLeft <= stemMetrics.Right;

			return verticalOverlap && horizontalOverlap;
		}

		public override void Move(double dx, double dy)
		{
			base.Move(dx, dy);
			_leftStemX += dx;
			_rightStemX += dx;
		}

		public override void WriteSVG(SvgWriter w)
		{
			w.SvgText(CSSObjectClass, CSSColorClass, _characterString, _originX, _originY);
		}

		public double LeftStemX { get { return _leftStemX; } }
		private double _leftStemX;
        public double RightStemX { get { return _rightStemX; } }
        private double _rightStemX;
    }
	internal class AccidentalMetrics : CLichtCharacterMetrics
	{
		public AccidentalMetrics(Head head, double fontHeight, double gap, CSSObjectClass cssClass)
			: base(head, fontHeight, cssClass)
		{
			double verticalPadding = gap / 5;
			_top -= verticalPadding;
			_bottom += verticalPadding;

			switch(_characterString)
			{
				case "b":
					_left -= gap * 0.2F;
					_right += gap * 0.2F;
					break;
				case "n":
					_left -= gap * 0.2F;
					_right += gap * 0.2F;
					break;
				case "#":
					_left -= gap * 0.1F;
					_right += gap * 0.1F;
					break;
			}
            if(head != null)
            {
				CSSColorClass = head.ColorClass;    
            }
        }

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public override void WriteSVG(SvgWriter w)
		{
			w.SvgText(CSSObjectClass, CSSColorClass, _characterString, _originX, _originY);
		}

	}
	internal class DynamicMetrics : CLichtCharacterMetrics, ICloneable
	{
		/// <summary>
		/// clichtDynamics: { "Ø", "∏", "π", "p", "P", "F", "f", "ƒ", "Ï", "Î" };
		///                  pppp, ppp,  pp,  p,   mp,  mf,  f,   ff, fff, ffff
		/// </summary>
		/// <param name="gap"></param>
		/// <param name="textInfo"></param>
		/// <param name="isBelow"></param>
		/// <param name="topBoundary"></param>
		/// <param name="bottomBoundary"></param>
		public DynamicMetrics(double gap, TextInfo textInfo, bool isBelow, CSSObjectClass dynamicClass)
			: base(textInfo.Text, textInfo.FontHeight, TextHorizAlign.left, dynamicClass)
		{
			// visually centre the "italic" dynamic characters
			if(textInfo.Text == "p" || textInfo.Text == "f") // p, f
			{
				Move(textInfo.FontHeight * 0.02, 0F);
			}
			else if(textInfo.Text == "F") // mf
			{
				Move(textInfo.FontHeight * 0.1, 0F);
			}
			else
			{
				Move(textInfo.FontHeight * 0.05, 0F);
			}
			double dynamicWidth = Right - Left;
			double moveLeftDelta = -(dynamicWidth / 2F) - (0.25F * gap); // "centre" italics
			Move(moveLeftDelta, 0F);

			IsBelow = isBelow;
		}
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		public bool IsBelow;
	}
}

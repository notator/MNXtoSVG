﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class LineMetrics : Metrics
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="csslineClass"></param>
        /// <param name="strokeWidthPixels"></param>
        /// <param name="stroke">"none", "black", "white", "red" or a string of 6 hex characters</param>
        /// <param name="fill">"none", "black", "white", "red" or a string of 6 hex characters</param>
        /// <param name="lineCap"></param>
        public LineMetrics(CSSObjectClass csslineClass,
            double strokeWidthPixels,
            string stroke = "none", 
            string fill = "none",
            CSSLineCap lineCap = CSSLineCap.butt)
            : base(csslineClass)
        {
            StrokeWidthPixels = strokeWidthPixels;
            Stroke = stroke.ToString();
            Fill = fill.ToString();
            LineCap = lineCap.ToString();
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public readonly double StrokeWidthPixels = 0;
        public readonly string Stroke = "none"; // "none", "black", "white", "#333" etc
        public readonly string Fill = "none"; // "none", "black", "white", "#333" etc
        public readonly string LineCap = "butt"; // "butt", "round", "square" 
    }
 
	internal class StemMetrics : LineMetrics
	{
		public StemMetrics(double top, double x, double bottom, double strokeWidth, VerticalDir verticalDir)
			: base(CSSObjectClass.stem, strokeWidth, "black")
		{
			_originX = x;
			_originY = top;
			_top = top;
			_right = x + strokeWidth;
			_bottom = bottom;
			_left = x - strokeWidth;
			VerticalDir = verticalDir;
			StrokeWidth = strokeWidth;
		}

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgLine(CSSObjectClass, _originX, _top, _originX, _bottom);
        }

        public object Clone()
		{
			return this.MemberwiseClone();
		}

		public readonly VerticalDir VerticalDir;
		public readonly double StrokeWidth;
	}
	internal class LedgerlineBlockMetrics : LineMetrics, ICloneable
	{      
        public LedgerlineBlockMetrics(double left, double right, double strokeWidth, CSSObjectClass ledgerlinesClass)
			: base(ledgerlinesClass, strokeWidth, "black")
		{
            /// The base class has deliberately been called with CSSClass.ledgerline (singular) here.
            /// This is so that its less confusing later when comparing the usage with stafflines/staffline.
            /// A ledgerline is always contained in a ledgerlines group.
            /// A staffline is always contained in a stafflines group.
            /// The CSS definition for ledgerlines is written if a ledgerline has been used.
            /// The CSS definition for stafflines is written if a staffline has been used.

            _left = left;
			_right = right;
			_strokeWidth = strokeWidth;
		}

		public void AddLedgerline(double newY, double gap)
		{
			if(Ys.Count == 0)
			{
				_top = newY - (gap / 2F);
				_bottom = newY + (gap / 2F);
			}
			else
				_bottom = newY + (gap / 2F);

			Ys.Add(newY);
		}

		public override void Move(double dx, double dy)
		{
			base.Move(dx, dy);
			for(int i = 0; i < Ys.Count; i++)
			{
				Ys[i] += dy;
			}
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public override void WriteSVG(SvgWriter w)
        {
            CSSObjectClass ledgerlineClass = CSSObjectClass.ledgerline;

            w.WriteStartElement("g");
            w.WriteAttributeString("class", CSSObjectClass.ToString());
            foreach(double y in Ys)
			{
				w.SvgLine(ledgerlineClass, _left + _strokeWidth, y, _right - _strokeWidth, y);
			}
            w.WriteEndElement();
		}

		private List<double> Ys = new List<double>();
		private readonly double _strokeWidth;
    }
	internal class CautionaryBracketMetrics : LineMetrics, ICloneable
	{
		public CautionaryBracketMetrics(bool isLeftBracket, double top, double right, double bottom, double left, double strokeWidth)
			: base(CSSObjectClass.cautionaryBracket, strokeWidth, "black")
		{
			_isLeftBracket = isLeftBracket;
			_top = top;
			_left = left;
			_bottom = bottom;
			_right = right;
			_strokeWidth = strokeWidth;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

        public override void WriteSVG(SvgWriter w)
        {
			w.SvgCautionaryBracket(CSSObjectClass, _isLeftBracket, _top, _right, _bottom, _left);
		}

		private readonly bool _isLeftBracket;
		private readonly double _strokeWidth;
	}
	internal class StafflineMetrics : LineMetrics
	{
		public StafflineMetrics(double left, double right, double originY)
			: base(CSSObjectClass.staffline, 0, "black")
		{
			_left = left;
			_right = right;
			_top = originY;
			_bottom = originY;
			_originY = originY;
		}

		/// <summary>
		/// This function should never be called.
		/// See Staff.WriteSVG(...).
		/// </summary>
		public override void WriteSVG(SvgWriter w)
		{
			throw new NotImplementedException();
		}
	}
	public class RegionFrameConnectorMetrics : LineMetrics
	{
		public RegionFrameConnectorMetrics(double x, double top, double bottom)
			: base(CSSObjectClass.regionFrameConnector, 0, "black")
		{
			_left = x;
			_right = x;
			_top = top;
			_bottom = bottom;
			_originY = bottom;
		}

		/// <summary>
		/// This function should never be called.
		/// See Staff.WriteSVG(...).
		/// </summary>
		public override void WriteSVG(SvgWriter w)
		{
			throw new NotImplementedException();
		}
	}
	/// <summary>
	/// Notehead extender lines are used when chord symbols cross barlines.
	/// </summary>
	internal class NoteheadExtenderMetrics : LineMetrics
	{
		public NoteheadExtenderMetrics(double left, double right, double originY, double strokeWidth, string strokeColor, double gap, bool drawExtender)
			: base(CSSObjectClass.noteExtender, strokeWidth, strokeColor)

        {
			_left = left;
			_right = right;

			// _top and _bottom are used when drawing barlines between staves
			_top = originY;
			_bottom = originY;
			if(drawExtender == false)
			{
				_top -= (gap / 2F);
				_bottom += (gap / 2F);
			}

			_originY = originY;
            _strokeColor = strokeColor;
			_strokeWidth = strokeWidth;
			_drawExtender = drawExtender;
		}

        public override void WriteSVG(SvgWriter w)
        {
            if(_drawExtender)
                w.SvgLine(CSSObjectClass, _left, _originY, _right, _originY);
        }

        public string StrokeColor { get { return _strokeColor; } }
        private readonly string _strokeColor;
		private readonly double _strokeWidth = 0;
		private readonly bool _drawExtender;
	}

	/// <summary>
	/// Metrics used by both Barlines and RepeatSymbols
	/// </summary>
	internal class BRMetrics : Metrics
	{
		public BRMetrics(double leftReOriginX, double rightReOriginX,
			CSSObjectClass lineClass1 = CSSObjectClass.normalBarline, CSSObjectClass lineClass2 = CSSObjectClass.normalBarline)
			: base(lineClass1, lineClass2)
		{
			_originX = 0;
			_left = leftReOriginX; // for a normal, thin barline: -(strokeWidth / 2);
			_right = rightReOriginX; // for a normal, thin barline: strokeWidth / 2;
		}

		//public override void Move(double dx, double dy)
		//{
		//	base.Move(dx, dy);
		//}

		/// <summary>
		/// Use Barline.WriteSVG(...) instead.
		/// </summary>
		public override void WriteSVG(SvgWriter w)
		{
			throw new ApplicationException();
		}

		public void SetLeft(double left)
		{
			_left = left;
		}

		public void SetRight(double right)
		{
			_right = right;
		}
	}

	internal class TupletBracketBoundaryMetrics : LineMetrics
	{
		public TupletBracketBoundaryMetrics(double top, double right, double bottom, double left, bool isOver)
			: base(CSSObjectClass.tupletBracket, M.PageFormat.TupletBracketStrokeWidth, "black")
		{
			_originX = left;
			_originY = top;
			_top = top;
			_right = right;
			_bottom = bottom;
			_left = left;
			IsOver = isOver;
		}

		public override void WriteSVG(SvgWriter w)
		{
			throw new NotImplementedException();
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

		public readonly bool IsOver;
	}
}

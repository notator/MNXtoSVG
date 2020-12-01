using System.Drawing;

using MNX.Globals;

using Moritz.Xml;


namespace Moritz.Symbols
{
    public abstract class RepeatSymbol : BRLine
	{
		public RepeatSymbol(Voice voice)
			: base(voice)
		{
			var dotMetrics = new DotMetrics(M.PageFormat.MusicFontHeight, M.PageFormat.GapVBPX, CSSObjectClass.dot);
			_dotWidth = (dotMetrics.Right - dotMetrics.Left) * 1.3; // strange but true!
		}

		protected void DrawDots(SvgWriter w, double topLineY, double gap, double dotsX)
		{
			double upperDotOriginY = topLineY + (gap * 1.5);
			double lowerDotOriginY = topLineY + (gap * 2.5);

			w.SvgText(CSSObjectClass.dot, ".", dotsX, upperDotOriginY);
			w.SvgText(CSSObjectClass.dot, ".", dotsX, lowerDotOriginY);
		}

		protected readonly double _dotWidth;
	}

	/// <summary>
	/// A RepeatSymbol consisting of: thickBarline, thinBarline, dots.
	/// The dots are only printed if the symbol crosses a staff.
	/// OriginX is the thick line's x-coordinate.
	/// </summary>
	public class RepeatBegin : RepeatSymbol
	{
		public RepeatBegin(Voice voice)
			: base(voice)
		{
		}

		/// <summary>
		/// Writes out the barline's vertical lines and dots.
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool drawDots)
		{
			double barlineTopY = TopY(topStafflineY, isEndOfSystem);
			double barlineBottomY = BottomY(bottomStafflineY, isEndOfSystem);

			double thickLeftLineOriginX = Metrics.OriginX;
			double thinRightLineOriginX = thickLeftLineOriginX + (ThickStrokeWidth / 2) + DoubleBarPadding + (ThinStrokeWidth / 2);
			double dotsX = thinRightLineOriginX + DoubleBarPadding; 

			w.SvgStartGroup(CSSObjectClass.repeatBeginBarline.ToString());
			DrawLines(w, thinRightLineOriginX, thickLeftLineOriginX, barlineTopY, barlineBottomY);
			if(drawDots)
			{
				DrawDots(w, topStafflineY, M.PageFormat.GapVBPX, dotsX);
			}
			w.SvgEndGroup();
		}

		public override string ToString()
		{
			return "repeatBegin: ";
		}

		// RepeatBeginBarline: thick, thin, dots
		public override void CreateMetrics(Graphics graphics)
		{
			double leftEdgeReOriginX = -(ThickStrokeWidth / 2F);
			double rightEdgeReOriginX = (ThickStrokeWidth / 2F) + DoubleBarPadding + ThinStrokeWidth;
			Metrics = new BRMetrics(leftEdgeReOriginX, rightEdgeReOriginX, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

			((BRMetrics)Metrics).SetRight(Metrics.Right + DoubleBarPadding + _dotWidth);
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
		}
	}

	/// <summary>
	/// A RepeatSymbol consisting of: dots, thinBarline, thickBarline.
	/// The dots are only printed if the symbol crosses a staff.
	/// OriginX is the thick line's x-coordinate.
	/// </summary>
	public class RepeatEnd : RepeatSymbol
	{
		public RepeatEnd(Voice voice)
			: base(voice)
		{
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool drawDots)
		{
			double barlineTopY = TopY(topStafflineY, isEndOfSystem);
			double barlineBottomY = BottomY(bottomStafflineY, isEndOfSystem);

			double thickRightLineOriginX = Metrics.OriginX;
			double thinLeftLineOriginX = thickRightLineOriginX - (ThickStrokeWidth / 2F) - DoubleBarPadding - (ThinStrokeWidth / 2F);
			double dotsX = thinLeftLineOriginX -(ThinStrokeWidth / 2F) - DoubleBarPadding - _dotWidth;

			w.SvgStartGroup(CSSObjectClass.repeatEndBarline.ToString());
			DrawLines(w, thinLeftLineOriginX, thickRightLineOriginX, barlineTopY, barlineBottomY);
			if(drawDots)
			{
				DrawDots(w, topStafflineY, M.PageFormat.GapVBPX, dotsX);
			}
			w.SvgEndGroup();
		}

		public override string ToString()
		{
			return "repeatEnd: ";
		}

		// RepeatEndBarline: dots, thin, thick
		public override void CreateMetrics(Graphics graphics)
		{
			double leftEdgeReOriginX = (ThickStrokeWidth / 2F) - DoubleBarPadding - ThinStrokeWidth - DoubleBarPadding - _dotWidth;
			double rightEdgeReOriginX = (ThickStrokeWidth / 2F);

			Metrics = new BRMetrics(leftEdgeReOriginX, rightEdgeReOriginX, CSSObjectClass.thickBarline, CSSObjectClass.thinBarline);

			((BRMetrics)Metrics).SetLeft(Metrics.Left - _dotWidth - DoubleBarPadding);
		}


		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
		}
	}

	/// <summary>
	/// A RepeatSymbol consisting of: dots, thinBarline, thickBarline, thinBarline, dots.
	/// The dots are only printed if the symbol crosses a staff.
	/// OriginX is the thick line's x-coordinate.
	/// </summary>
	public class RepeatEndBegin : RepeatSymbol
	{
		public RepeatEndBegin(Voice voice)
			: base(voice)
		{
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool drawDots)
		{
			double barlineTopY = TopY(topStafflineY, isEndOfSystem);
			double barlineBottomY = BottomY(bottomStafflineY, isEndOfSystem);
			double thickMiddleLineOriginX = Metrics.OriginX;
			double thinLeftLineOriginX = thickMiddleLineOriginX - (ThickStrokeWidth / 2F) - DoubleBarPadding - (ThinStrokeWidth / 2F);
			double thinRightLineOriginX = thickMiddleLineOriginX + (ThickStrokeWidth / 2F) + DoubleBarPadding + (ThinStrokeWidth / 2F);
			double leftDotsX = thinLeftLineOriginX - (ThinStrokeWidth / 2F) - DoubleBarPadding - _dotWidth;
			double rightDotsX = thinRightLineOriginX + (ThinStrokeWidth / 2F) + DoubleBarPadding;

			w.SvgStartGroup(CSSObjectClass.repeatEndBeginBarline.ToString());
			DrawLines(w, thinLeftLineOriginX, thickMiddleLineOriginX, barlineTopY, barlineBottomY);
			if(drawDots)
			{
				DrawDots(w, topStafflineY, M.PageFormat.GapVBPX, leftDotsX);
				DrawDots(w, topStafflineY, M.PageFormat.GapVBPX, rightDotsX);
			}
			w.SvgLine(CSSObjectClass.thinBarline, thinRightLineOriginX, barlineTopY, thinRightLineOriginX, barlineBottomY);
			w.SvgEndGroup();
		}

		public override string ToString()
		{
			return "repeatEndBegin: ";
		}

		// RepeatEndBeginBarline: dots, thin, thick, thin, dots
		public override void CreateMetrics(Graphics graphics)
		{
			double rightEdgeReOriginX = DoubleBarPadding + ThinStrokeWidth + DoubleBarPadding + _dotWidth;
			double leftEdgeReOriginX = -rightEdgeReOriginX;
			Metrics = new BRMetrics(leftEdgeReOriginX, rightEdgeReOriginX, CSSObjectClass.thickBarline, CSSObjectClass.thinBarline);

			((BRMetrics)Metrics).SetLeft(Metrics.Left - _dotWidth - DoubleBarPadding);
			((BRMetrics)Metrics).SetRight(Metrics.Right + DoubleBarPadding + _dotWidth);
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
		}
	}
}

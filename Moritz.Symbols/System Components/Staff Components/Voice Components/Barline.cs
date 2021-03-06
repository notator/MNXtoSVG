using MNX.Globals;
using Moritz.Xml;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace Moritz.Symbols
{
	/// <summary>
	/// Base class for both Barline and RepeatSymbol
	/// </summary>
	public abstract class BRLine : Anchor
	{
		protected BRLine(Voice voice)
			: base(voice)
		{
		}

		/// <summary>
		/// This function should not be called.
		/// Call the other WriteSVG(...) function to write the barline's vertical line(s),
		/// and WriteDrawObjectsSVG(...) to write any DrawObjects. 
		/// </summary>
		public override void WriteSVG(SvgWriter w)
		{
			throw new ApplicationException();
		}

		public abstract void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool writeDots);

		public abstract void CreateMetrics(Graphics graphics);

		/// <summary>
		/// Called by EndOfScoreBarline and Repeat types
		/// </summary>
		/// <param name="w"></param>
		/// <param name="thinLineOriginX"></param>
		/// <param name="thickLineOriginX"></param>
		/// <param name="topY"></param>
		/// <param name="bottomY"></param>
		protected void DrawLines(SvgWriter w, double thinLineOriginX, double thickLineOriginX, double topY, double bottomY)
		{
			w.SvgLine(CSSObjectClass.thinBarline, thinLineOriginX, topY, thinLineOriginX, bottomY);
			w.SvgLine(CSSObjectClass.thickBarline, thickLineOriginX, topY, thickLineOriginX, bottomY);
		}

		/// <summary>
		/// Default is true
		/// </summary>
		public bool IsVisible = true;

		protected double StafflineStrokeWidth { get { return M.PageFormat.StafflineStemStrokeWidthVBPX; } }
		protected double ThinStrokeWidth { get { return M.PageFormat.ThinBarlineStrokeWidth; } }
		protected double NormalStrokeWidth { get { return M.PageFormat.NormalBarlineStrokeWidth; } }
		protected double ThickStrokeWidth { get { return M.PageFormat.ThickBarlineStrokeWidth; } }
		protected double DoubleBarPadding { get { return M.PageFormat.ThickBarlineStrokeWidth * 0.5; } }
		protected double Gap { get { return M.PageFormat.GapVBPX; } }
		protected double TopY(double topStafflineY, bool isEndOfSystem)
		{
			double topY = topStafflineY;
			if(isEndOfSystem)
			{
				double halfStafflineWidth = (StafflineStrokeWidth / 2);
				topY -= halfStafflineWidth;
			}
			return topY;
		}
		protected double BottomY(double bottomStafflineY, bool isEndOfSystem)
		{
			double bottomY = bottomStafflineY;
			if(isEndOfSystem)
			{
				double halfStafflineWidth = (StafflineStrokeWidth / 2);
				bottomY += halfStafflineWidth;
			}
			return bottomY;
		}


	}

	/// <summary>
	/// Barlines maintain their line and drawObjects metrics separately.
	/// The lines are drawn using implementations of an abstract function,
	/// The drawObjects are drawn by calling BarlineDrawObjectsMetrics.WriteSVG().  
	/// </summary>
	public abstract class Barline : BRLine
	{
		protected Barline (Voice voice)
            : base(voice)
        {
		}

		protected void MoveBarnumberAboveRegionBox(BarnumberMetrics barnumberMetrics, FramedRegionInfoMetrics regionInfoMetrics)
		{			
			if(barnumberMetrics != null && regionInfoMetrics != null)
			{
				double padding = Gap * 1.5;
				double shift = barnumberMetrics.Bottom - regionInfoMetrics.Top + padding;
				barnumberMetrics.Move(0, -shift);
			}
		}

		protected void MoveFramedTextBottomToDefaultPosition(Metrics framedTextMetrics)
		{			
			double staffTop = this.Voice.Staff.Metrics.StafflinesTop;
			double defaultBottom = staffTop - (Gap * 3);
			if(framedTextMetrics != null)
			{
				framedTextMetrics.Move(0, defaultBottom - framedTextMetrics.Bottom);
			}
		}

		protected void MoveFramedTextAboveNoteObjects(Metrics framedTextMetrics, List<NoteObject> fixedNoteObjects)
		{
			if(framedTextMetrics != null) 
			{
				double bottomPadding = Gap * 1.5;
				double xPadding = Gap * 4;
				PaddedMetrics paddedMetrics = new PaddedMetrics(framedTextMetrics, 0, xPadding, bottomPadding, xPadding);

				foreach(NoteObject noteObject in fixedNoteObjects)
				{
					int overlaps = OverlapsHorizontally(paddedMetrics, noteObject);
					if(overlaps == 0)
					{
						MovePaddedMetricsAboveNoteObject(paddedMetrics, noteObject);
					}
					else if(overlaps == 1) // noteObject is left of framedText
					{
						if(noteObject is ChordSymbol chordSymbol)
						{
							if(chordSymbol.Stem.Direction == VerticalDir.up && chordSymbol.BeamBlock != null)
							{
								MoveFramedTextAboveBeamBlock(framedTextMetrics, chordSymbol.BeamBlock);
							}
							else if(chordSymbol.ChordMetrics.NoteheadExtendersMetrics != null)
							{
								MoveFramedTextAboveNoteheadExtenders(framedTextMetrics, chordSymbol.ChordMetrics.NoteheadExtendersMetrics);
							}
						}
					}
					else if(overlaps == -1) // noteObject is right of framed text, so we need look no further in these noteObjects.
					{
						break;
					}
				}
			}
		}

		/// <summary>
		/// returns
		/// -1 if metrics is entirely to the left of the fixedNoteObject;
		/// 0 if metrics overlaps the fixedNoteObject;
		/// 1 if metrics is entirely to the right of the fixedNoteObject;
		/// </summary>
		/// <returns></returns>
		private int OverlapsHorizontally(Metrics metrics, NoteObject fixedNoteObject)
		{
			int rval = 0;
			Metrics fixedMetrics = fixedNoteObject.Metrics;
			if(metrics.Right < fixedMetrics.Left)
			{
				rval = -1;
			}
			else if(metrics.Left > fixedMetrics.Right)
			{
				rval = 1;
			}
			return rval;
		}
		/// <summary>
		/// Move paddedMetrics above the fixedNoteObject if it is not already.
		/// </summary>
		private void MovePaddedMetricsAboveNoteObject(PaddedMetrics paddedMetrics, NoteObject fixedNoteObject)
		{
			double verticalOverlap = 0;
			if(fixedNoteObject.Metrics is ChordMetrics chordMetrics)
			{
				verticalOverlap = chordMetrics.OverlapHeight(paddedMetrics, 0F);
			}
			else if(fixedNoteObject.Metrics is RestMetrics restMetrics)
			{
				verticalOverlap = restMetrics.OverlapHeight(paddedMetrics, 0F);
			}
			else if(!(fixedNoteObject is Barline))
			{
				verticalOverlap = fixedNoteObject.Metrics.OverlapHeight(paddedMetrics, 0F);
			}

			if(verticalOverlap > 0)
			{
				verticalOverlap = (verticalOverlap > paddedMetrics.BottomPadding) ? verticalOverlap : paddedMetrics.BottomPadding;
				paddedMetrics.Move(0, -verticalOverlap);				
			}
		}

		private void MoveFramedTextAboveBeamBlock(Metrics framedTextMetrics, BeamBlock beamBlock)
		{
			double padding = Gap * 1.5;

			double verticalOverlap = beamBlock.OverlapHeight(framedTextMetrics, padding);
			if(verticalOverlap > 0)
			{
				verticalOverlap = (verticalOverlap > padding) ? verticalOverlap : padding;
				framedTextMetrics.Move(0, -verticalOverlap );
			}
		}

		private void MoveFramedTextAboveNoteheadExtenders(Metrics framedTextMetrics, List<NoteheadExtenderMetrics> noteheadExtendersMetrics)
		{
			double padding = Gap * 1.5;
			int indexOfTopExtender = 0;
			for(int i = 1; i < noteheadExtendersMetrics.Count; ++i)
			{
				indexOfTopExtender = (noteheadExtendersMetrics[indexOfTopExtender].Top < noteheadExtendersMetrics[i].Top) ? indexOfTopExtender : i;
			}
			NoteheadExtenderMetrics topExtender = noteheadExtendersMetrics[indexOfTopExtender];
			double verticalOverlap = topExtender.OverlapHeight(framedTextMetrics, padding);
			if(verticalOverlap > 0)
			{
				verticalOverlap = (verticalOverlap > padding) ? verticalOverlap : padding;
				framedTextMetrics.Move(0, -(verticalOverlap));
			}
		}

		/// <summary>
		/// This virtual function writes the staff name and barnumber to the SVG file (if they are present).
		/// Overrides write the region info (if present).
		/// The barline itself is drawn when the system (and staff edges) is complete.
		/// </summary>
		public virtual void WriteDrawObjectsSVG(SvgWriter w)
		{
			if(StaffNameMetrics != null)
			{
				StaffNameMetrics.WriteSVG(w);
			}
			if(BarnumberMetrics != null)
			{
				BarnumberMetrics.WriteSVG(w);
			}
		}

		protected virtual void DrawRegionFrameConnector(SvgWriter w, FramedRegionInfoMetrics framedRegionInfoMetrics)
		{			
			double x = this.Metrics.OriginX;
			double top = framedRegionInfoMetrics.Bottom;
			double bottom = this.Metrics.Top;

			w.SvgLine(CSSObjectClass.regionFrameConnector, x, top, x, bottom);
		}

		protected void SetDrawObjects(List<DrawObject> drawObjects)
		{
			DrawObjects.Clear();
			foreach(DrawObject drawObject in drawObjects)
			{
				drawObject.Container = this;
				DrawObjects.Add(drawObject);
			}
		}

		internal virtual void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			if(StaffNameMetrics != null)
			{
				staffMetrics.Add(StaffNameMetrics);
			}
			if(BarnumberMetrics != null)
			{
				staffMetrics.Add(BarnumberMetrics);
			}
		}

		internal abstract void AlignFramedTextsXY(List<NoteObject> noteObjects0);

		protected void AlignBarnumberX()
		{
			if(BarnumberMetrics != null)
			{
				BarnumberMetrics.Move(Metrics.OriginX - BarnumberMetrics.OriginX, 0);
			}
		}

		protected void AddBasicMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(StaffNameMetrics != null)
			{
				horizontalEdge.Add(StaffNameMetrics);
			}
			if(BarnumberMetrics != null)
			{
				horizontalEdge.Add(BarnumberMetrics);
			}
		}

		protected void SetCommonMetrics(Graphics graphics, List<DrawObject> drawObjects)
		{
			StaffMetrics staffMetrics = Voice.Staff.Metrics;
			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is StaffNameText staffNameText)
				{
					CSSObjectClass staffClass = CSSObjectClass.staffName;
					StaffNameMetrics = new StaffNameMetrics(staffClass, graphics, staffNameText.TextInfo);
					// move the staffname vertically to the middle of this staff
					double staffheight = staffMetrics.StafflinesBottom - staffMetrics.StafflinesTop;
					double dy = (staffheight * 0.5F) + (Gap * 0.8F);
					StaffNameMetrics.Move(0, dy);
				}
				if(drawObject is FramedBarNumberText framedBarNumberText)
				{
					BarnumberMetrics = new BarnumberMetrics(graphics, framedBarNumberText.TextInfo, framedBarNumberText.FrameInfo);
					// move the bar number to its default (=lowest) position above this staff.
					BarnumberMetrics.Move(0, staffMetrics.StafflinesTop - BarnumberMetrics.Bottom - (Gap * 3));
				}
			}
		}

		public StaffNameMetrics StaffNameMetrics = null;
		public BarnumberMetrics BarnumberMetrics = null;

	}

	/// <summary>
	/// A barline which is a single, thin line. OriginX is the line's x-coordinate.
	/// </summary>
	public class NormalBarline : Barline
	{
		public NormalBarline(Voice voice)
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
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool writeDots = false)
		{
			double topY = TopY(topStafflineY, isEndOfSystem);
			double bottomY = BottomY(bottomStafflineY, isEndOfSystem);

			w.SvgLine(CSSObjectClass.normalBarline, this.Metrics.OriginX, topY, this.Metrics.OriginX, bottomY);
		}

		public override string ToString()
		{
			return "normalBarline: ";
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			AddBasicMetricsToEdge(horizontalEdge);
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX
			base.AlignBarnumberX();
			#endregion
			MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);
		}

		public override void CreateMetrics(Graphics graphics)
		{
			Metrics = new BRMetrics(-(NormalStrokeWidth / 2F), (NormalStrokeWidth / 2F));
			SetCommonMetrics(graphics, DrawObjects);
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics metrics)
		{
			base.AddAncilliaryMetricsTo(metrics);
		}
	}

	/// <summary>
	/// A barline consisting of: thinBarline, thickBarline.
	/// OriginX is the thick line's x-coordinate.
	/// </summary>
	public class EndOfScoreBarline : NormalBarline
	{
		public EndOfScoreBarline(Voice voice)
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
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool drawDots = false)
		{
			double topY = TopY(topStafflineY, isEndOfSystem);
			double bottomY = BottomY(bottomStafflineY, isEndOfSystem);
			double thickRightLineOriginX = Metrics.OriginX;
			double thinLeftLineOriginX = thickRightLineOriginX - (ThickStrokeWidth / 2F) - DoubleBarPadding - (ThinStrokeWidth / 2F);

			w.SvgStartGroup(CSSObjectClass.endOfScoreBarline.ToString());
			DrawLines(w, thinLeftLineOriginX, thickRightLineOriginX, topY, bottomY);
			w.SvgEndGroup();
		}

		public override string ToString()
		{
			return "endOfScoreBarline: ";
		}

		// EndOfScoreBarline: thin, thick
		public override void CreateMetrics(Graphics graphics)
		{
			double rightEdgeReOriginX = (ThickStrokeWidth / 2F);
			double leftEdgeReOriginX = -DoubleBarPadding - (ThickStrokeWidth / 2) - (ThinStrokeWidth / 2);

			Metrics = new BRMetrics(leftEdgeReOriginX, rightEdgeReOriginX, CSSObjectClass.thickBarline, CSSObjectClass.thinBarline);
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			// nothing to do here since there are no drawObjects
			M.Assert(DrawObjects.Count == 0);
		}
	}

	#region AssistantPerformer Region barlines
	/// <summary>
	/// A barline whose 2 lines are (left to right) thick then thin. OriginX is the thick line's x-coordinate.
	/// </summary>
	public class StartRegionBarline : Barline
	{
		public StartRegionBarline(Voice voice, List<DrawObject> drawObjects)
			: base(voice)
		{
			SetDrawObjects(drawObjects);
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool writeDots = false)
		{
			double topY = TopY(topStafflineY, isEndOfSystem);
			double bottomY = BottomY(bottomStafflineY, isEndOfSystem);

			double thickLeftLineOriginX = Metrics.OriginX;
			w.SvgStartGroup(CSSObjectClass.startRegionBarline.ToString());
			w.SvgLine(CSSObjectClass.thickBarline, thickLeftLineOriginX, topY, thickLeftLineOriginX, bottomY);

			double thinRightLineOriginX = thickLeftLineOriginX + (ThickStrokeWidth / 2F) + DoubleBarPadding + (ThinStrokeWidth / 2F);
			w.SvgLine(CSSObjectClass.thinBarline, thinRightLineOriginX, topY, thinRightLineOriginX, bottomY);
			w.SvgEndGroup();
		}
		/// <summary>
		/// This function writes the staff name, barnumber and region info to the SVG file (if they are present).
		/// The barline itself is drawn when the system (and staff edges) is complete.
		/// </summary>
		public override void WriteDrawObjectsSVG(SvgWriter w)
		{
			base.WriteDrawObjectsSVG(w);

			if(FramedRegionStartTextMetrics != null)
			{
				DrawRegionFrameConnector(w, FramedRegionStartTextMetrics);
				FramedRegionStartTextMetrics.WriteSVG(w);
			}
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(FramedRegionStartTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionStartTextMetrics);
			}

			AddBasicMetricsToEdge(horizontalEdge);
		}

		public override string ToString()
		{
			return "startRegionBarline: ";
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX
			base.AlignBarnumberX();
			double originX = Metrics.OriginX;
			if(FramedRegionStartTextMetrics != null)
			{
				FramedRegionStartTextMetrics.Move(originX - FramedRegionStartTextMetrics.Left, 0);
			}
			#endregion

			MoveFramedTextBottomToDefaultPosition(FramedRegionStartTextMetrics);

			MoveFramedTextAboveNoteObjects(FramedRegionStartTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);

			MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionStartTextMetrics);
		}

		public override void CreateMetrics(Graphics graphics)
		{
			double leftEdge = -(ThickStrokeWidth / 2F);
			double rightEdge = (ThickStrokeWidth / 2F) + DoubleBarPadding + ThinStrokeWidth;
			Metrics = new BRMetrics(leftEdge, rightEdge, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

			SetCommonMetrics(graphics, DrawObjects);

			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is FramedRegionStartText frst)
				{
					FramedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo, Gap);
					RegionFrameConnectorMetrics = new RegionFrameConnectorMetrics(Metrics.OriginX, frst.FrameInfo.Bottom, Metrics.Top);
					break;
				}
			}			
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			base.AddAncilliaryMetricsTo(staffMetrics);

			if(FramedRegionStartTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionStartTextMetrics);
			}
		}

		public FramedRegionInfoMetrics FramedRegionStartTextMetrics = null;
		public RegionFrameConnectorMetrics RegionFrameConnectorMetrics = null;
	}

	/// <summary>
	/// A barline whose 2 lines are (left to right) thin then thick. OriginX is the thick line's x-coordinate.
	/// </summary>
	public class EndRegionBarline : Barline
	{
		public EndRegionBarline(Voice voice, List<DrawObject> drawObjects)
			: base(voice)
		{
			SetDrawObjects(drawObjects);
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool writeDots = false)
		{
			double topY = TopY(topStafflineY, isEndOfSystem);
			double bottomY = BottomY(bottomStafflineY, isEndOfSystem);

			double thinLeftLineOriginX = Metrics.OriginX - (ThickStrokeWidth / 2) - DoubleBarPadding - (ThinStrokeWidth / 2F);
			w.SvgStartGroup(CSSObjectClass.endRegionBarline.ToString());
			w.SvgLine(CSSObjectClass.thinBarline, thinLeftLineOriginX, topY, thinLeftLineOriginX, bottomY);

			double thickRightLineOriginX = Metrics.OriginX;
			w.SvgLine(CSSObjectClass.thickBarline, thickRightLineOriginX, topY, thickRightLineOriginX, bottomY);
			w.SvgEndGroup();
		}
		/// <summary>
		/// This function writes the staff name, barnumber and region info to the SVG file (if they are present).
		/// The barline itself is drawn when the system (and staff edges) is complete.
		/// </summary>
		public override void WriteDrawObjectsSVG(SvgWriter w)
		{
			base.WriteDrawObjectsSVG(w);

			if(FramedRegionEndTextMetrics != null)
			{
				DrawRegionFrameConnector(w, FramedRegionEndTextMetrics);
				FramedRegionEndTextMetrics.WriteSVG(w);
			}
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(FramedRegionEndTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionEndTextMetrics);
			}

			AddBasicMetricsToEdge(horizontalEdge);
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX
			base.AlignBarnumberX();
			double originX = Metrics.OriginX;
			if(FramedRegionEndTextMetrics != null)
			{
				FramedRegionEndTextMetrics.Move(originX - FramedRegionEndTextMetrics.Right, 0);
			}
			#endregion

			MoveFramedTextBottomToDefaultPosition(FramedRegionEndTextMetrics);

			MoveFramedTextAboveNoteObjects(FramedRegionEndTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);

			MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionEndTextMetrics);
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			base.AddAncilliaryMetricsTo(staffMetrics);
			if(FramedRegionEndTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionEndTextMetrics);
			}
		}

		public override void CreateMetrics(Graphics graphics)
		{
			double leftEdge = -((ThickStrokeWidth / 2F) + DoubleBarPadding + ThinStrokeWidth);
			double rightEdge = (ThickStrokeWidth / 2F);
			Metrics = new BRMetrics(leftEdge, rightEdge, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

			SetCommonMetrics(graphics, DrawObjects);

			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is FramedRegionEndText frst)
				{
					FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo, Gap);
					RegionFrameConnectorMetrics = new RegionFrameConnectorMetrics(Metrics.OriginX, frst.FrameInfo.Bottom, Metrics.Top);
					break;
				}
			}
		}

		public override string ToString()
		{
			return "endRegionBarline: ";
		}

		public FramedRegionInfoMetrics FramedRegionEndTextMetrics = null;
		public RegionFrameConnectorMetrics RegionFrameConnectorMetrics = null;
	}

	/// <summary>_
	/// A barline whose 3 lines are (left to right) thin, thick, thin. OriginX is the thick line's x-coordinate.
	/// </summary>
	public class EndAndStartRegionBarline : Barline
	{
		public EndAndStartRegionBarline(Voice voice, List<DrawObject> drawObjects)
			: base(voice)
		{
			SetDrawObjects(drawObjects);
		}

		/// <summary>
		/// Writes out the barline's vertical line(s).
		/// May be called twice per staff.barline:
		///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
		///     2. for the range between the staff's lower edge and the next staff's upper edge
		///        (if the staff's lower neighbour is in the same group)
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool writeDots = false)
		{
			double topY = TopY(topStafflineY, isEndOfSystem);
			double bottomY = BottomY(bottomStafflineY, isEndOfSystem);
			
			w.SvgStartGroup(CSSObjectClass.endAndStartRegionBarline.ToString());

			double thinLeftLineOriginX = Metrics.OriginX - (ThickStrokeWidth / 2F) - DoubleBarPadding - (ThinStrokeWidth / 2F);
			w.SvgLine(CSSObjectClass.thinBarline, thinLeftLineOriginX, topY, thinLeftLineOriginX, bottomY);

			double thickCentreLineOriginX = Metrics.OriginX;
			w.SvgLine(CSSObjectClass.thickBarline, thickCentreLineOriginX, topY, thickCentreLineOriginX, bottomY);

			double thinRightLineOriginX = thickCentreLineOriginX + (ThickStrokeWidth / 2F) + DoubleBarPadding + (ThinStrokeWidth / 2F);
			w.SvgLine(CSSObjectClass.thinBarline, thinRightLineOriginX, topY, thinRightLineOriginX, bottomY);

			w.SvgEndGroup();
		}
		/// <summary>
		/// This function writes the staff name, barnumber and region info to the SVG file (if they are present).
		/// The barline itself is drawn when the system (and staff edges) is complete.
		/// </summary>
		public override void WriteDrawObjectsSVG(SvgWriter w)
		{
			base.WriteDrawObjectsSVG(w);

			FramedRegionInfoMetrics upperBox = null;
			if(FramedRegionEndTextMetrics != null)
			{
				upperBox = FramedRegionEndTextMetrics;
			}
			if(FramedRegionStartTextMetrics != null)
			{
				upperBox = (upperBox != null && upperBox.Bottom < FramedRegionStartTextMetrics.Bottom) ? upperBox : FramedRegionStartTextMetrics;
			}
			if(upperBox != null)
			{
				DrawRegionFrameConnector(w, upperBox);
			}

			if(FramedRegionEndTextMetrics != null)
			{
				FramedRegionEndTextMetrics.WriteSVG(w);
			}

			if(FramedRegionStartTextMetrics != null)
			{
				FramedRegionStartTextMetrics.WriteSVG(w);
			}
		}

		public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		{
			if(FramedRegionEndTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionEndTextMetrics);
			}

			if(FramedRegionStartTextMetrics != null)
			{
				horizontalEdge.Add(FramedRegionStartTextMetrics);
			}

			AddBasicMetricsToEdge(horizontalEdge);
		}

		public override string ToString()
		{
			return "endAndStartRegionBarline: ";
		}

		internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		{
			#region alignX

			// An EndAndStartRegionBarline cannot be at the start of a system,
			// so it can't have a barnumber, and there's no reason to call base.AlignBarnumberX();
			M.Assert(BarnumberMetrics == null);

			double originX = Metrics.OriginX;
			if(FramedRegionEndTextMetrics != null)
			{
				FramedRegionEndTextMetrics.Move(originX - FramedRegionEndTextMetrics.Right, 0);
			}
			if(FramedRegionStartTextMetrics != null)
			{
				FramedRegionStartTextMetrics.Move(originX - FramedRegionStartTextMetrics.Left, 0);
			}
			#endregion

			MoveFramedTextBottomToDefaultPosition(FramedRegionStartTextMetrics);
			MoveFramedTextBottomToDefaultPosition(FramedRegionEndTextMetrics);

			MoveFramedTextAboveNoteObjects(FramedRegionStartTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveNoteObjects(FramedRegionEndTextMetrics, fixedNoteObjects);
			MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);
		}

		public override void CreateMetrics(Graphics graphics)
		{
			double rightEdge = (ThickStrokeWidth / 2F) + DoubleBarPadding + ThinStrokeWidth;
			double leftEdge = -rightEdge;
			Metrics = new BRMetrics(leftEdge, rightEdge, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

			SetCommonMetrics(graphics, DrawObjects);

			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is FramedRegionStartText frst)
				{
					FramedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo, Gap);
					if(RegionFrameConnectorMetrics == null)
					{
						RegionFrameConnectorMetrics = new RegionFrameConnectorMetrics(Metrics.OriginX, frst.FrameInfo.Bottom, Metrics.Top);
					}
				}
				if(drawObject is FramedRegionEndText fret)
				{
					FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, fret.Texts, fret.FrameInfo, Gap);
				}
			}
		}

		internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
		{
			base.AddAncilliaryMetricsTo(staffMetrics);

			if(FramedRegionStartTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionStartTextMetrics);
			}
			if(FramedRegionEndTextMetrics != null)
			{
				staffMetrics.Add(FramedRegionEndTextMetrics);
			}
		}

		public FramedRegionInfoMetrics FramedRegionStartTextMetrics = null;
		public FramedRegionInfoMetrics FramedRegionEndTextMetrics = null;
		public RegionFrameConnectorMetrics RegionFrameConnectorMetrics = null;
	}

	/// <summary>
	/// A barline whose 2 lines are (left to right) normal then thick. OriginX is the thick line's x-coordinate.
	/// This barline type is always used for the final barline in a score. It can have FramedEndRegionInfo.
	/// </summary>
	public class EndOfScoreRegionBarline : EndRegionBarline
	{
		public EndOfScoreRegionBarline(Voice voice, List<DrawObject> drawObjects)
			: base(voice, drawObjects)
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
		public override void WriteSVG(SvgWriter w, double topStafflineY, double bottomStafflineY, bool isEndOfSystem, bool writeDots = false)
		{
			double topY = TopY(topStafflineY, isEndOfSystem);
			double bottomY = BottomY(bottomStafflineY, isEndOfSystem);

			double normalLeftLineOriginX = Metrics.OriginX - (ThickStrokeWidth / 2) - DoubleBarPadding - (NormalStrokeWidth / 2F);
			w.SvgStartGroup(CSSObjectClass.endOfScoreRegionBarline.ToString());
			w.SvgLine(CSSObjectClass.normalBarline, normalLeftLineOriginX, topY, normalLeftLineOriginX, bottomY);

			double thickRightLineOriginX = Metrics.OriginX;
			w.SvgLine(CSSObjectClass.thickBarline, thickRightLineOriginX, topY, thickRightLineOriginX, bottomY);
			w.SvgEndGroup();
		}

		// The following functions are inherited from EndRegionBarline.
		// public override void WriteDrawObjectsSVG(SvgWriter w)
		// public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
		// internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
		// internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)

		public override void CreateMetrics(Graphics graphics)
		{
			double leftEdge = -((ThickStrokeWidth / 2F) + DoubleBarPadding + NormalStrokeWidth);
			double rightEdge = (ThickStrokeWidth / 2F);
			Metrics = new BRMetrics(leftEdge, rightEdge, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

			foreach(DrawObject drawObject in DrawObjects)
			{
				if(drawObject is FramedRegionEndText frst)
				{
					FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo, Gap);
					break;
				}
			}
		}

		public override string ToString() { return "endOfScoreRegionBarline: "; }
	}

	#endregion AssistantPerformer Region barlines

}

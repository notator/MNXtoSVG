﻿using System.Collections.Generic;
using System.Drawing;
using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    /// <summary>
    /// The base class for
    /// SystemMetrics, StaffMetrics, 
    /// BarnumberMetrics, FramedRegionInfoMetrics,
    /// TimeSignatureMetrics, KeySignatureMetrics,
    /// OctavaLineMetrics
    /// </summary>
	public class GroupMetrics : Metrics
	{
        public GroupMetrics(CSSObjectClass cssGroupClass)
            : base(cssGroupClass)
        {
        }

        /// <summary>
        /// Adds the metrics to the MetricsList and includes it in this object's boundary.
        /// The boundary is used for collision checking. All objects that should move together with this object
        /// must be added to the MetricsList.
        /// </summary>
        /// <param name="metrics"></param>
        public virtual void Add(Metrics metrics)
		{
			MetricsList.Add(metrics);
			ResetBoundary();
		}

		public void ResetBoundary()
		{
			_top = double.MaxValue;
			_right = double.MinValue;
			_bottom = double.MinValue;
			_left = double.MaxValue;
			foreach(Metrics metrics in MetricsList)
			{
				_top = _top < metrics.Top ? _top : metrics.Top;
				_right = _right > metrics.Right ? _right : metrics.Right;
				_bottom = _bottom > metrics.Bottom ? _bottom : metrics.Bottom;
				_left = _left < metrics.Left ? _left : metrics.Left;
			}
		}

		public override void Move(double dx, double dy)
		{
			base.Move(dx, dy);
			foreach(Metrics metrics in MetricsList)
			{
				metrics.Move(dx, dy);
			}
		}

		public override void WriteSVG(SvgWriter w)
		{
			w.SvgStartGroup(CSSObjectClass.ToString());
			foreach(Metrics metrics in MetricsList)
			{
				metrics.WriteSVG(w);
			}
			w.SvgEndGroup();
		}

		public readonly List<Metrics> MetricsList = new List<Metrics>();
	}

    public class BarnumberMetrics : GroupMetrics
    {
        public BarnumberMetrics(Graphics graphics, TextInfo textInfo, FramePadding framePadding)
            : base(CSSObjectClass.barNumber)
        {
            _barNumberNumberMetrics = new TextMetrics(CSSObjectClass.barNumberNumber, graphics, textInfo);
            _number = textInfo.Text;
            _top = _barNumberNumberMetrics.Top - framePadding.Top;
            _right = _barNumberNumberMetrics.Right + framePadding.Right;
            _bottom = _barNumberNumberMetrics.Bottom + framePadding.Bottom;
            _left = _barNumberNumberMetrics.Left - framePadding.Left;
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            _barNumberNumberMetrics.Move(dx, dy);
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup(CSSObjectClass.ToString());
            w.SvgRect(CSSObjectClass.barNumberFrame, _left, _top, _right - _left, _bottom - _top);
            w.SvgText(CSSObjectClass.barNumberNumber, _number, _barNumberNumberMetrics.OriginX, _barNumberNumberMetrics.OriginY);
            w.SvgEndGroup();
        }

        readonly TextMetrics _barNumberNumberMetrics = null;
        readonly string _number;
    }

    // FramedRegionInfoMetrics(graphics, framedRegionEndText.Texts, framedRegionEndText.FrameInfo)
    public class FramedRegionInfoMetrics : GroupMetrics
    {
        public FramedRegionInfoMetrics(Graphics graphics, List<Text> texts, FramePadding framePadding, double gap)
            : base(CSSObjectClass.framedRegionInfo)
        {
            Gap = gap;

            double maxWidth = 0;
            double nextTop = 0;


            foreach(Text text in texts)
            {
                TextMetrics tm = new TextMetrics(CSSObjectClass.regionInfoString, graphics, text.TextInfo);
                tm.Move(-tm.Left, -tm.Top);
                double width = tm.Right - tm.Left;
                maxWidth = (maxWidth > width) ? maxWidth : width;
                tm.Move(0, nextTop);
                nextTop = tm.Top + ((tm.Bottom - tm.Top) * 1.7);

                _textMetrics.Add(tm);
                _textStrings.Add(text.TextInfo.Text);
            }

            bool alignRight = (texts[0].TextInfo.TextHorizAlign != TextHorizAlign.left);
            if(alignRight)
            {
                foreach(TextMetrics tm in _textMetrics)
                {
                    double deltaX = maxWidth - (tm.Right - tm.Left);
                    tm.Move(deltaX, 0);

                    // move tm.OriginX so that the text is right aligned (OriginX is used by WriteSVG())
                    deltaX = (tm.Right - tm.Left) / 2;
                    tm.Move(-deltaX, 0);
                }
            }
            else // align left
            {
                foreach(TextMetrics tm in _textMetrics)
                {
                    // move tm.OriginX so that the text is left aligned (OriginX is used by WriteSVG())
                    double deltaX = (tm.Right - tm.Left) / 2;
                    tm.Move(deltaX, 0);
                }
            }

            _top = 0 - framePadding.Top;
            _right = maxWidth + framePadding.Right;
            _bottom = _textMetrics[_textMetrics.Count - 1].Bottom + framePadding.Bottom;
            _left = 0 - framePadding.Left;

            switch(texts[0].TextInfo.TextHorizAlign)
            {
                case TextHorizAlign.left:
                    Move(-_left, -_bottom); // set the origin to the bottom left corner
                    break;
                case TextHorizAlign.center:
                    Move(-((_left + _right) / 2), -_bottom); // set the origin to the middle of the bottom edge
                    break;
                case TextHorizAlign.right:
                    Move(-_right, -_bottom); // set the origin to the bottom right corner
                    break;
            }
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            foreach(TextMetrics tm in _textMetrics)
            {
                tm.Move(dx, dy);
            }
        }

        /// <summary>
        /// Moves this FramedRegionInfoMetrics object above the argument,
        /// but only if they overlap horizontally.
        /// </summary>
        /// <param name="framedRegionInfoMetrics"></param>
        internal void MoveAbove(FramedRegionInfoMetrics framedRegionInfoMetrics)
        {
            double verticalOverlap = this.OverlapHeight(framedRegionInfoMetrics, -1);
            if(verticalOverlap > 0)
            {
                this.Move(0, Bottom - (verticalOverlap - (2 * Gap)));
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup(CSSObjectClass.ToString());

            w.SvgRect(CSSObjectClass.regionInfoFrame, _left, _top, _right - _left, _bottom - _top);

            for(int i = 0; i < _textMetrics.Count; ++i)
            {
                TextMetrics textMetrics = _textMetrics[i];
                string textString = _textStrings[i];
                w.SvgText(CSSObjectClass.regionInfoString, textString, textMetrics.OriginX, textMetrics.OriginY);
            }

            w.SvgEndGroup();
        }

        readonly List<TextMetrics> _textMetrics = new List<TextMetrics>();
        readonly List<string> _textStrings = new List<string>();

        public double Gap { get; }
    }

    /// <summary>
    /// An ExtenderMetrics is a textMetrics followed by (possibly dotted) horizontal line with a solid vertical end marker line on its right.
    /// </summary>
    public class ExtenderMetrics : GroupMetrics
    {
        protected readonly TextMetrics _textMetrics = null;
        protected readonly string _strokeDashArray = null;
        protected readonly double _endMarkerHeight = 0;
        protected readonly bool _displayEndMarker = true;

        /// <summary>
        /// An ExtenderMetrics is a textMetrics followed by (possibly dotted) horizontal line with a solid vertical end marker line on its right.
        /// </summary>
        /// <param name="left">The left coordinate of the displayed extender (with or without text)</param>
        /// <param name="right">The right coordinate of the displayed extender</param>
        /// <param name="hLineY">The y-coordinate of the horizontal line.</param>
        /// <param name="strokeDashArray">If null, the line will be solid.</param>
        /// <param name="endMarkerHeight">Is negative if extender is under its containing staff.</param>
        public ExtenderMetrics(CSSObjectClass cssObjectClass, TextMetrics textMetrics, double left, double right, double hLineY, string strokeDashArray, double endMarkerHeight,
            bool displayEndMarker)
            :base(cssObjectClass)
        {
            _left = left;
            _right = right;
            _top = hLineY; // the real height of the extender is ignored
            _bottom = hLineY;
            _originX = _left;
            _originY = hLineY;

            _textMetrics = textMetrics;
            _strokeDashArray = strokeDashArray;
            _endMarkerHeight = endMarkerHeight;
            _displayEndMarker = displayEndMarker;

            MetricsList.Add(textMetrics); // will be moved automatically
        }

        public override void Move(double dx, double dy)
        {
            M.Assert(dx == 0);
            base.Move(dx, dy);
        }

        protected void WriteSVG(SvgWriter w, CSSObjectClass horizontalLineClass, CSSObjectClass verticalLineClass)
        {
            _textMetrics.WriteSVG(w);
            double textSpace = _textMetrics.Right - _textMetrics.Left;
            if(_textMetrics.TextInfo.Text.Length == 3)
            {
                textSpace *= 0.85; 
            }
            else if(_textMetrics.TextInfo.Text.Length == 4)
            {
                textSpace *= 0.9;
            }
            var lineleft = _left + textSpace;
            w.SvgLine(horizontalLineClass, lineleft, _originY, _right, _originY, _strokeDashArray);

            if(_displayEndMarker)
            {
                // verticalLineClass must have style stroke-linecap:square
                w.SvgLine(verticalLineClass, _right, _originY, _right, _originY + _endMarkerHeight, null);
            }
        }

        internal void AddToEdge(HorizontalEdge horizontalEdge)
        {
            if(horizontalEdge is TopEdge topEdge)
            {
                topEdge.Add(this);
            }
            else if(horizontalEdge is BottomEdge bottomEdge)
            {
                bottomEdge.Add(this);
            }
        }
    }

    public class OctaveShiftExtenderMetrics : ExtenderMetrics
    {
        public OctaveShiftExtenderMetrics(TextMetrics textMetrics, double leftChordLeft, double rightChordRight, double hLineY, string strokeDashArray, double endMarkerHeight,
            bool displayEndMarker)
            : base(CSSObjectClass.octaveShiftExtender, textMetrics, leftChordLeft, rightChordRight, hLineY, strokeDashArray, endMarkerHeight, displayEndMarker)
        {
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup(CSSObjectClass.octaveShiftExtender.ToString());
            base.WriteSVG(w, CSSObjectClass.octaveShiftExtenderHLine, CSSObjectClass.octaveShiftExtenderVLine);
            w.SvgEndGroup();
        }
    }

    public class TupletMetrics : GroupMetrics
    {
        // TupletMetrics(textMetrics, bracketHoriz, bracketLeft, bracketRight, bracketHeight, isOver)
        public TupletMetrics(TextMetrics textMetrics, double bracketHoriz, double bracketLeft, double bracketRight, double bracketHeight, bool isOver)
            : base(CSSObjectClass.tupletBracket)
        {
            _tupletTextMetrics = textMetrics;
            _text = textMetrics.TextInfo.Text;

            double bracketTop = (isOver) ? bracketHoriz: bracketHoriz - bracketHeight;
            double bracketBottom = (isOver) ? bracketHoriz + bracketHeight : bracketHoriz;

            _top = (textMetrics.Top < bracketTop) ? textMetrics.Top : bracketTop;
            _right = (textMetrics.Right > bracketRight) ? textMetrics.Right : bracketRight;
            _bottom = (textMetrics.Bottom > bracketBottom) ? textMetrics.Bottom : bracketBottom;
            _left = (textMetrics.Left < bracketLeft) ? textMetrics.Left : bracketLeft;

            _bracketBoundary = new TupletBracketBoundaryMetrics(bracketTop, bracketRight, bracketBottom, bracketLeft, isOver);

            MetricsList.Add(textMetrics);
            MetricsList.Add(_bracketBoundary);
        }

        public override void Move(double dx, double dy)
        {
            M.Assert(dx == 0);

            base.Move(dx, dy);
        }

        public override void WriteSVG(SvgWriter w)
        {
            double padding = (_tupletTextMetrics.Right - _tupletTextMetrics.Left) / 2;
            double leftH = _tupletTextMetrics.Left;
            double rightH = _tupletTextMetrics.Right + padding;
            if(_text.Length > 1)
            {
                padding = (_tupletTextMetrics.Right - _tupletTextMetrics.Left) / _text.Length;
                leftH = _tupletTextMetrics.Left + padding;
                rightH = _tupletTextMetrics.Right + padding;
            }

            string leftStr = M.DoubleToShortString(_bracketBoundary.Left);
            string rightStr = M.DoubleToShortString(_bracketBoundary.Right);
            string bracketBottomStr = M.DoubleToShortString(_bracketBoundary.Bottom);
            string bracketTopStr = M.DoubleToShortString(_bracketBoundary.Top);
            string leftHStr = M.DoubleToShortString(leftH);
            string rightHStr = M.DoubleToShortString(rightH);

            string leftDString;
            string rightDString;
            if(_bracketBoundary.IsOver)
            {
                leftDString = $"M{leftStr},{bracketBottomStr}V{bracketTopStr}H{leftHStr}";
                rightDString = $"M{rightStr},{bracketBottomStr}V{bracketTopStr}H{rightHStr}";
            }
            else
            {
                leftDString = $"M{leftStr},{bracketTopStr}V{bracketBottomStr}H{leftHStr}";
                rightDString = $"M{rightStr},{bracketTopStr}V{bracketBottomStr}H{rightHStr}";
            }

            w.SvgStartGroup(CSSObjectClass.tuplet.ToString());
            w.SvgPath(CSSObjectClass.tupletBracket, leftDString, null, null, null); // stroke, srtokeWidth and fill handled in CSS
            w.SvgPath(CSSObjectClass.tupletBracket, rightDString, null, null, null); // stroke, srtokeWidth and fill handled in CSS
            w.SvgText(CSSObjectClass.tupletText, _text, _tupletTextMetrics.OriginX, _tupletTextMetrics.OriginY);
            w.SvgEndGroup();
        }

        TextMetrics _tupletTextMetrics = null;
        TupletBracketBoundaryMetrics _bracketBoundary = null;

        readonly string _text;
    }

    public class AnchorMetrics : GroupMetrics
    {
        /// <summary>
        /// The relevant CSSObjectClasses are set when creating the anchorMetrics and drawObject.Metrics,
        /// so don't need to be mentioned again when calling the base constructor here.
        /// Note that the drawObject.Metrics added here will be used for both horizontal and vertical
        /// collision checking. Additional metrics can be added _after_ doing horizontal justification
        /// if that is not the intended behaviour -- using the virtual Add(metrics) function.
        /// </summary>
        /// <param name="firstMetrics">The first metrics object in this AnchorMetrics.</param>
        /// <param name="drawObject">An attached drawObject with drawObject.Metrics != null</param>
        public AnchorMetrics(Metrics firstMetrics, List<DrawObject> drawObjects)
            :base(CSSObjectClass.none)
        {
            _top = firstMetrics.Top;
            _right = firstMetrics.Right;
            _bottom = firstMetrics.Bottom;
            _left = firstMetrics.Left;
            MetricsList.Add(firstMetrics);

            foreach(var drawObject in drawObjects)
            {
                this.Add(drawObject.Metrics);
            }
        }
    }
}

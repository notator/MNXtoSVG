using Moritz.Xml;
using MNX.Globals;
using System.Drawing;
using System;

namespace Moritz.Symbols
{

    /// <summary>
    /// SlurTemplates and TieTemplates (and their Metrics) are created *after* the noteheads they connect have moved to their final
    /// left-right positions on the system, but before the system has moved to its final vertical position.
    /// SlurTemplate and TieTemplate Metrics are therefore only ever moved vertically.
    /// </summary>
    internal abstract class TieTemplate : DrawObject
    {
        protected Point _p1;
        protected Point _c1;
        protected Point _c2;
        protected Point _p2;

        protected static readonly string stroke = "#AA0000"; // a dark red
        protected static string strokeWidth
        {
            get
            {
                return $"{((int)(_gap / 2.2)).ToString(M.En_USNumberFormat)}px";
            }
        }
        protected static readonly string fill = "none";

        protected static double _gap;

        internal virtual void Move(double dy)
        {
            _p1.Y += (int)dy;
            _c1.Y += (int)dy;
            _c2.Y += (int)dy;
            _p2.Y += (int)dy;
        }
    }

    internal class ShortTieTemplate : TieTemplate
    {
        /// <summary>
        /// A Simple, two-point TieTemplate.
        /// Parameters p2 and p3 are the bezier control points
        /// The x and y coordinates are integers
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="p2"></param>
        internal ShortTieTemplate(Point p1, Point c1, Point c2, Point p2, double gap, bool isOver)
        {
            _p1 = p1;
            _c1 = c1;
            _c2 = c2;
            _p2 = p2;

            _gap = gap;

            Metrics = new SlurTieMetrics(CSSObjectClass.tieTemplate, gap, p1.X, p1.Y, p2.X, isOver);
        }

        public override void WriteSVG(SvgWriter w)
        {
            string dString = $"M{_p1.X},{_p1.Y}C{_c1.X},{_c1.Y},{_c2.X},{_c2.Y},{_p2.X},{_p2.Y}";

            w.SvgTemplatePath(CSSObjectClass.tieTemplate, dString, stroke, strokeWidth, fill);
        }
    }

    internal class LongTieTemplate : TieTemplate
    {
        /// <summary>
        /// A "long" three-point TieTemplate.
        /// Parameters p1, tp and p2 are points on the line,
        /// Parameters c1, tc and c2 are their respective bezier control points.
        /// The parameters are conceptually in clockwise order.
        /// The x and y coordinates are integers
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="c1"></param>
        /// <param name="tc"></param>
        /// <param name="tp"></param>
        /// <param name="c2"></param>
        /// <param name="p2"></param>
        /// <param name="gap"></param>
        /// <param name="isOver"></param>
        public LongTieTemplate(Point p1, Point c1, Point tc, Point tp, Point c2, Point p2, double gap, bool isOver)
        {
            _p1 = p1;
            _c1 = c1;
            _tc = tc;
            _tp = tp;
            _c2 = c2;
            _p2 = p2;

            _gap = gap;

            Metrics = new SlurTieMetrics(CSSObjectClass.tieTemplate, gap, p1.X, p1.Y, p2.X, isOver);
        }

        public override void WriteSVG(SvgWriter w)
        {
            string dString = $"M{_p1.X},{_p1.Y}C{_c1.X},{_c1.Y},{_tc.X},{_tc.Y},{_tp.X},{_tp.Y}S{_c2.X},{_c2.Y},{_p2.X},{_p2.Y}";

            w.SvgTemplatePath(CSSObjectClass.tieTemplate, dString, stroke, strokeWidth, fill);
        }

        internal override void Move(double dy)
        {
            base.Move(dy);
            _tc.Y += (int)dy;
            _tp.Y += (int)dy;
        }

        private Point _tc;
        private Point _tp;
    }
}
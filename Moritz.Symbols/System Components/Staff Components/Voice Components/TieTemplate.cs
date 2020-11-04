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
    internal class TieTemplate : DrawObject
    {
        private Point _p1;
        private Point _c1;
        private Point _p2;
        private Point _c2;
        private Point _p3;
        private Point _c3;
        private Point _p4;
        private Point _c4;

        private readonly string type;

        /// <summary>
        /// A Simple, two-point TieTemplate.
        /// Parameters p2 and p3 are the bezier control points
        /// The x and y coordinates are integers
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="p2"></param>
        public TieTemplate(Point p1, Point c1, Point c2, Point p2, double gap, bool isOver)
        {
            _p1 = p1;
            _c1 = c1;
            _c2 = c2;
            _p2 = p2;

            type = "short";

            Metrics = new SlurTieMetrics(CSSObjectClass.tieTemplate, gap, p1.X, p1.Y, p2.X, isOver);
        }

        /// <summary>
        /// A "long" three-point TieTemplate.
        /// Parameters p1, p2 and p3 are points on the line,
        /// Parameters c1, c2 and c3 are their respective bezier control points.
        /// The parameters are conceptually in clockwise order.
        /// The x and y coordinates are integers
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="p2"></param>
        /// <param name="c3"></param>
        /// <param name="p3"></param>
        /// <param name="c4"></param>
        /// <param name="p4"></param>
        public TieTemplate(Point p1, Point c1, Point c2, Point p2, Point c3, Point p3, Point c4, Point p4, double gap, bool isOver)
        {
            _p1 = p1;
            _c1 = c1;
            _p2 = p2;
            _c2 = c2;
            _p3 = p3;
            _c3 = c3;
            _p4 = p4;
            _c4 = c4;

            type = "long";

            Metrics = new SlurTieMetrics(CSSObjectClass.tieTemplate, gap, p1.X, p1.Y, p4.X, isOver);
        }

        public override void WriteSVG(SvgWriter w)
        {
            string dString = null;
            if(type == "short")
            {

                // A simple, two-point template path has a d-attribute consisting of an "M" and a "C" component:
                // <path class="tieTemplate" d="M p1x, p1y C c1x, c1y, c2x, c2y, p2x, p2y" />
                dString = $"M{_p1.X},{_p1.Y}C{_c1.X},{_c1.Y},{_c2.X},{_c2.Y},{_p2.X},{_p2.Y}";
            }
            else
            {
                // A four-point template path has the form:
                // <path class="slurTemplate" d="M p1x, p1y C c1x, c1y, c2x, c2y, p2x, p2y, c3x, c3y, p3x, p3y S c4x, c4y, p4x, p4y" />
                // Where the "p" components are points on the line, and the "c" components are their respective controls
                dString = $"M{_p1.X},{_p1.Y}C{_c1.X},{_c1.Y},{_c2.X},{_c2.Y},{_p2.X},{_p2.Y}S{_c3.X},{_c3.Y},{_p3.X},{_p3.Y}S{_c4.X},{_c4.Y},{_p4.X},{_p4.Y}";
            }

            string stroke = "#AA0000"; // a dark red
            string strokeWidth = "50px";
            string fill = "none";

            w.SvgTemplatePath(CSSObjectClass.tieTemplate, dString, stroke, strokeWidth, fill);
        }

        internal void Move(double dy)
        {
            _p1.Y += (int)dy;
            _c1.Y += (int)dy;
            _p2.Y += (int)dy;
            _c2.Y += (int)dy;
            _p3.Y += (int)dy;
            _c3.Y += (int)dy;
            _p4.Y += (int)dy;
            _c4.Y += (int)dy;
        }
    }
}
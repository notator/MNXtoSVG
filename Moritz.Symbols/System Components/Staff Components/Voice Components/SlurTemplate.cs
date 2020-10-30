using Moritz.Xml;
using MNX.Globals;
using System.Drawing;
using System;

namespace Moritz.Symbols
{
    /// <summary>
    /// SlurTemplates and Ties (and their Metrics) are created *after* the noteheads they connect have moved to their final
    /// left-right positions on the system, but before the system has moved to its final vertical position.
    /// SlurTemplate and Tie Metrics are therefore only ever moved vertically.
    /// </summary>
    internal class SlurTemplate : DrawObject
    {
        private Point _p1;
        private Point _p2;
        private Point _p3;
        private Point _p4;
        private Point _c1;
        private Point _c2;
        private Point _c3;
        private readonly string type;

        /// <summary>
        /// A Simple, two-point SlurTemplate.
        /// Parameters p2 and p3 are the bezier control points
        /// The x and y coordinates are integers
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        public SlurTemplate(Point p1, Point p2, Point p3, Point p4, double gap, bool isOver)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _p4 = p4;

            type = "short";

            Metrics = new SlurTieMetrics(CSSObjectClass.slurTemplate, gap, p1.X, p1.Y, p4.X, isOver);
        }

        /// <summary>
        /// A "long" three-point SlurTemplate.
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
        public SlurTemplate(Point p1, Point c1, Point c2, Point p2, Point c3, Point p3, double gap, bool isOver)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _c1 = c1;
            _c2 = c2;
            _c3 = c3;

            type = "long";

            Metrics = new SlurTieMetrics(CSSObjectClass.slurTemplate, gap, p1.X, p1.Y, p3.X, isOver);
        }

        public override void WriteSVG(SvgWriter w)
        {
            string dString = null;
            if(type == "short")
            {

                // A simple, two-point template path has a d-attribute consisting of an "M" and a "C" component:
                // <path class="slurTemplate" d="M p1x, p1y C c1x, c1y, c2x, c2y, p2x, p2y" />
                dString = $"M{_p1.X},{_p1.Y}C{_p2.X},{_p2.Y},{_p3.X},{_p3.Y},{_p4.X},{_p4.Y}";
            }
            else
            {
                // A three-point template path has the form:
                // <path class="slurTemplate" d="M p1x, p1y C c1x, c1y, c2x, c2y, p2x, p2y S c3x, c3y, p3x, p3y" />
                // Where the "p" components are points on the line, and the "c" components are their respective controls
                dString = $"M{_p1.X},{_p1.Y}C{_c1.X},{_c1.Y},{_c2.X},{_c2.Y},{_p2.X},{_p2.Y}S{_c3.X},{_c3.Y},{_p3.X},{_p3.Y}";
            }

            string stroke = "#0000AA"; // a dark blue
            string strokeWidth = "50px";
            string fill = "none";

            w.SvgTemplatePath(CSSObjectClass.slurTemplate, dString, stroke, strokeWidth, fill);
        }

        internal void Move(double dy)
        {
            _p1.Y += (int)dy;
            _p2.Y += (int)dy;
            _p3.Y += (int)dy;
            _p4.Y += (int)dy;
            _c1.Y += (int)dy;
            _c2.Y += (int)dy;
            _c3.Y += (int)dy;
        }
    }
}
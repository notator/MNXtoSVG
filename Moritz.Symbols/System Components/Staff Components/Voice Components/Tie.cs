using Moritz.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using MNX.Globals;

namespace Moritz.Symbols
{
    /// <summary>
    /// Ties (and their Metrics) are created *after* the noteheads they connect have moved to their final
    /// left-right positions on the system, but before the system has moved to its final vertical position.
    /// Tie (and Slur) Metrics are therefore only ever moved vertically.
    /// Tie widths are related to the x-distance between the noteheads they connect.
    /// </summary>
    internal class Tie : DrawObject
    {
        private string _dString = null;
        private double _scaleX = 1; // Can be set to a value less than 1 to compress short ties.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originX">The left end of the tie points at (originX, originY) (the centre of its left notehead)</param>
        /// <param name="originY">The left end of the tie points at (originX, originY) (the centre of its left notehead)</param>
        /// <param name="rightX">The right end of the tie points at (rightX, originY)</param>
        /// <param name="gap"></param>
        /// <param name="tieOver"></param>
        public Tie(double originX, double originY, double rightX, double gap, bool tieOver)
        {
            SetPathAttributes(originX, originY, rightX, gap, tieOver);
            Metrics = new SlurTieMetrics(CSSObjectClass.tie, gap, originX, originY, tieOver);            
        }

         /// <summary>
        /// The path attributes are this class's private fields:
        ///    _dString and _scaleX
        /// This function should do collision checking with respect to the bordering chords.
        /// </summary>
        private void SetPathAttributes(double leftHeadCx, double leftHeadCy, double rightHeadCx, double gap, bool tieOver)
        {
            // At(gap = 32), a long tie with h = 0 is 164 units wide from leftNoteheadCX to rightNoteheadCX(164 = 12 + 70 + h + 70 + 12)
            //< g id = "LongTieUnder" class="slurTie">
            //  <path d="m 12 22 c 14 31 62 31 70 31 h 36 c 8 0 56 0 70 -31 c -14 21 -62 21 -70 21 h -36 c -8 0 -56 0 -70 -21" stroke="black" stroke-width="1"/>
            //</g>
            //<g id = "LongTieOver" >
            //  < path d="m 12 -22 c 14 -31 62 -31 70 -31 h 36 c 8 0 56 0 70 31 c -14 -21 -62 -21 -70 -21 h -36 c -8 0 -56 0 -70 21" fill="black" stroke="black" stroke-width="1"/>
            //</g>
            double headXSeparation = rightHeadCx - leftHeadCx;
            double scale = gap / 32; // multiply the values in the template path by this scale.
            // template values for tieUnder
            double h = 0;
            //var m = new List<double>() { 12, 22 };
            var m = new List<double>() { 12, 22 };
            var c1 = new List<List<double>>() { new List<double>() { 14, 31 }, new List<double>() { 62, 31 }, new List<double>() { 70, 31 } };
            var c2 = new List<List<double>>() { new List<double>() { 8, 0 }, new List<double>() { 56, 0 }, new List<double>() { 70, -31 } };
            var c3 = new List<List<double>>() { new List<double>() { -14, 21 }, new List<double>() { -62, 21 }, new List<double>() { -70, 21 } };
            var c4 = new List<List<double>>() { new List<double>() { -8, 0 }, new List<double>() { -56, 0 }, new List<double>() { -70, -21 } };
            var allCs = new List<List<List<double>>>() { c1, c2, c3, c4 };

            #region scale the template
            m[0] *= scale;
            m[1] *= scale;
            foreach(var c in allCs)
            {
                foreach(var point in c)
                {
                    point[0] *= scale;
                    point[1] *= scale;
                }
            }
            #endregion

            if(tieOver)
            {
                #region flip the tie over
                m[1] *= -1;
                foreach(var c in allCs)
                {
                    foreach(var point in c)
                    {
                        point[1] *= -1;
                    }
                }
                #endregion
            }

            double tieWidth = (m[0] * 2) + (c1[2][0] * 2); // h is currently 0, so this is (12 + 70 + 0 + 70 + 12) * scale;
            h = headXSeparation - tieWidth;

            #region convert to strings
            var ms = new List<string>();
            var cs1 = new List<List<string>>();
            var cs2 = new List<List<string>>();
            var cs3 = new List<List<string>>();
            var cs4 = new List<List<string>>();
            var allCSs = new List<List<List<string>>>() { cs1, cs2, cs3, cs4 };
            ms.Add(m[0].ToString(M.En_USNumberFormat));
            ms.Add(m[1].ToString(M.En_USNumberFormat));
            for(var i = 0; i < allCs.Count; i++)
            {
                var c = allCs[i];
                var cs = allCSs[i];
                for(var j = 0; j < 3; j++)
                {
                    var cPoint = c[j];
                    var csPoint = new List<string>
                    {
                        cPoint[0].ToString(M.En_USNumberFormat),
                        cPoint[1].ToString(M.En_USNumberFormat)
                    };
                    cs.Add(csPoint);
                }
            }
            #endregion

            StringBuilder sb = new StringBuilder();
            sb.Append($"m {ms[0]} {ms[1]} c {cs1[0][0]} {cs1[0][1]} {cs1[1][0]} {cs1[1][1]} {cs1[2][0]} {cs1[2][1]} ");
            if(h >= 0)
            {
                sb.Append($"h {h.ToString(M.En_USNumberFormat)} ");
            }
            sb.Append($"c {cs2[0][0]} {cs2[0][1]} {cs2[1][0]} {cs2[1][1]} {cs2[2][0]} {cs2[2][1]} ");
            sb.Append($"c {cs3[0][0]} {cs3[0][1]} {cs3[1][0]} {cs3[1][1]} {cs3[2][0]} {cs3[2][1]} ");
            if(h >= 0)
            {
                sb.Append($"h {(-h).ToString(M.En_USNumberFormat)} ");
            }
            sb.Append($"c {cs4[0][0]} {cs4[0][1]} {cs4[1][0]} {cs4[1][1]} {cs4[2][0]} {cs4[2][1]}");

            _dString = sb.ToString();

            if(h < 0)
            {
                _scaleX = headXSeparation / tieWidth;
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgPath(CSSObjectClass.tie, _dString, Metrics.OriginX, Metrics.OriginY, _scaleX);
        }
    }

    /// <summary>
    /// The SlurTieMetrics class is only ever used to move a slur or tie vertically (together with a system).
    /// It is created only *after* the related noteheads have been moved to their final left-right positions on a system.
    /// </summary>
    class SlurTieMetrics : Metrics
    {
        internal SlurTieMetrics(CSSObjectClass slurOrTie, double gap, double originX, double originY, bool slurTieOver)
            :base(slurOrTie)
        {
            _left = originX; // never changes
            _right = 0; // never used
            _originX = originX; // never changes

            _originY = originY;
            if(slurTieOver)
            {
                _bottom = originY;
                _top = originY - (gap * 12 / 32);
            }
            else
            {
                _top = originY;
                _bottom = originY + (gap * 12 / 32);
            }
        }

        public override void Move(double dx, double dy)
        {
            M.Assert(dx == 0);
            _top += dy;
            _bottom += dy;
            _originY += dy;
        }

        /// <summary>
        /// This function should never be called
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
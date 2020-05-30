using Moritz.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using MNX.Globals;

namespace Moritz.Symbols
{
    /// <summary>
    /// Ties are drawn after the noteheads they connect have moved to their final positions,
    /// so they dont need to be moved using their (DrawObject).Metrics.
    /// They only do collision checking with respect to the connected chords.
    /// </summary>
    internal class Tie : DrawObject
    {
        private string _dString = null;
        private double _originX = 0;
        private double _originY = 0;
        private double _scaleX = 1; // Can be set to a value less than 1 to compress short ties.

        public Tie(OutputChordSymbol ocs, Head leftHead, OutputChordSymbol targetOCS, Head targetHead, double gap, bool tieOver)
        {
            Metrics = new SlurTieMetrics(CSSObjectClass.tie);

            List<Head> leftHeadsTopDown = ocs.HeadsTopDown;
            List<HeadMetrics> leftHeadMetricsTopDown = ocs.ChordMetrics.HeadsMetrics;
            var leftHeadIndex = leftHeadsTopDown.FindIndex(obj => obj == leftHead);
            HeadMetrics leftHeadMetrics = leftHeadMetricsTopDown[leftHeadIndex];
            
            List<Head> targetHeadsTopDown = targetOCS.HeadsTopDown;
            List<HeadMetrics> targetHeadMetricsTopDown = targetOCS.ChordMetrics.HeadsMetrics;
            var targetHeadIndex = targetHeadsTopDown.FindIndex(obj => obj == targetHead);
            HeadMetrics targetHeadMetrics = targetHeadMetricsTopDown[targetHeadIndex];

            double leftHeadCx = (leftHeadMetrics.Left + leftHeadMetrics.Right) / 2;
            double leftHeadCy = leftHeadMetrics.OriginY;
            double targetHeadCx = (targetHeadMetrics.Left + targetHeadMetrics.Right) / 2;
            double headCy = (leftHeadMetrics.Top + leftHeadMetrics.Bottom) / 2;

            SetPathAttributes(ocs, leftHeadCx, leftHeadCy, targetOCS, targetHeadCx, headCy, gap, tieOver);
        }

        public Tie(OutputChordSymbol ocs, Head leftHead, double systemRight, double gap, bool tieOver)
        {
            Metrics = new SlurTieMetrics(CSSObjectClass.tie);

            List<Head> leftHeadsTopDown = ocs.HeadsTopDown;
            List<HeadMetrics> leftHeadMetricsTopDown = ocs.ChordMetrics.HeadsMetrics;
            var leftHeadIndex = leftHeadsTopDown.FindIndex(obj => obj == leftHead);
            HeadMetrics leftHeadMetrics = leftHeadMetricsTopDown[leftHeadIndex];
            double leftHeadCx = (leftHeadMetrics.Left + leftHeadMetrics.Right) / 2;

            double leftHeadCy = (leftHeadMetrics.Top + leftHeadMetrics.Bottom) / 2;
            double targetHeadCx = systemRight + (gap * 2);
            double headCy = (leftHeadMetrics.Top + leftHeadMetrics.Bottom) / 2;

            SetPathAttributes(ocs, leftHeadCx, leftHeadCy, null, targetHeadCx, headCy, gap, tieOver);
        }

        /// <summary>
        /// The path attributes are this class's private fields:
        ///    _dString, _originX, _originY and _scaleX
        /// This function does collision checking with respect to the bordering chords.
        /// </summary>
        private void SetPathAttributes(OutputChordSymbol ocs, double leftHeadCx, double leftHeadCy, OutputChordSymbol targetOCS, double targetHeadCx, double headCy, double gap, bool tieOver)
        {
            // At(gap = 32), a long tie with h = 0 is 164 units wide from leftNoteheadCX to rightNoteheadCX(164 = 12 + 70 + h + 70 + 12)
            //< g id = "LongTieUnder" class="slurTie">
            //  <path d="m 12 22 c 14 31 62 31 70 31 h 36 c 8 0 56 0 70 -31 c -14 21 -62 21 -70 21 h -36 c -8 0 -56 0 -70 -21" stroke="black" stroke-width="1"/>
            //</g>
            //<g id = "LongTieOver" >
            //  < path d="m 12 -22 c 14 -31 62 -31 70 -31 h 36 c 8 0 56 0 70 31 c -14 -21 -62 -21 -70 -21 h -36 c -8 0 -56 0 -70 21" fill="black" stroke="black" stroke-width="1"/>
            //</g>
            double headXSeparation = targetHeadCx - leftHeadCx;
            double scale = gap / 32; // multiply the values in the template path by this scale.
            // template values for tieUnder
            double h = 0;
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
            _originX = leftHeadCx;
            _originY = leftHeadCy;
            if(h < 0)
            {
                _scaleX = headXSeparation / tieWidth;
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgPath(CSSObjectClass.tie, _dString, _originX, _originY, _scaleX);
        }
    }

    /// <summary>
    /// The SlurTieMetrics class is not used to move a slur or tie.
    /// It is created only *after* the related noteheads have been moved to their final positions.
    /// It serves only to register the slur or tie CSSObjectClass class as having been used,
    /// so that its definition will be written to the styles in the defs section of the SVG page.
    /// </summary>
    class SlurTieMetrics : Metrics
    {
        internal SlurTieMetrics(CSSObjectClass slurOrTie)
            :base(slurOrTie)
        {
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
using Moritz.Xml;
using System.Collections.Generic;

namespace Moritz.Symbols
{
    /// <summary>
    /// When drawing a Tie, use one or more of the following predefined objects (see tiesTest):
    ///   "LeftTieHookOver"
    ///   "RightTieHookOver"
    ///   "LeftTieHookUnder"
    ///   "RightTieHookUnder"
    ///   "ShortTieOver"
    ///   "ShortTieUnder"
    /// Possibly define only the ...Over versions, and use transform in SVG to flip vertically about headCy.
    /// </summary>
    internal class Tie : DrawObject
    {
        public Tie(OutputChordSymbol ocs, Head leftHead, OutputChordSymbol targetOCS, Head targetHead, double gap)
        {
            List<Head> leftHeadsTopDown = ocs.HeadsTopDown;
            List<HeadMetrics> leftHeadMetricsTopDown = ocs.ChordMetrics.HeadsMetrics;
            var leftHeadIndex = leftHeadsTopDown.FindIndex(obj => obj == leftHead);
            HeadMetrics leftHeadMetrics = leftHeadMetricsTopDown[leftHeadIndex];
            
            List<Head> targetHeadsTopDown = targetOCS.HeadsTopDown;
            List<HeadMetrics> targetHeadMetricsTopDown = targetOCS.ChordMetrics.HeadsMetrics;
            var targetHeadIndex = targetHeadsTopDown.FindIndex(obj => obj == targetHead);
            HeadMetrics targetHeadMetrics = targetHeadMetricsTopDown[targetHeadIndex];

            double leftHeadCx = (leftHeadMetrics.Left + leftHeadMetrics.Right) / 2;
            double targetHeadCx = (targetHeadMetrics.Left + targetHeadMetrics.Right) / 2;
            double headCy = (leftHeadMetrics.Top + leftHeadMetrics.Bottom) / 2;
        }

        public Tie(OutputChordSymbol ocs, Head leftHead, double systemRight, double gap)
        {
            List<Head> leftHeadsTopDown = ocs.HeadsTopDown;
            List<HeadMetrics> leftHeadMetricsTopDown = ocs.ChordMetrics.HeadsMetrics;
            var leftHeadIndex = leftHeadsTopDown.FindIndex(obj => obj == leftHead);
            HeadMetrics leftHeadMetrics = leftHeadMetricsTopDown[leftHeadIndex];
            double leftHeadCx = (leftHeadMetrics.Left + leftHeadMetrics.Right) / 2;

            double leftHeadCy = (leftHeadMetrics.Top + leftHeadMetrics.Bottom) / 2;
            double targetHeadCx = systemRight + (gap * 2);
            double headCy = (leftHeadMetrics.Top + leftHeadMetrics.Bottom) / 2;
        }



        public override void WriteSVG(SvgWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
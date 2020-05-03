using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class TimeSignatureMetrics : Metrics
    {
        public static Dictionary<string, List<TextMetrics>> TimeSigDefs = new Dictionary<string, List<TextMetrics>>();

        private readonly List<TextMetrics> _textMetricsList = new List<TextMetrics>();
        private readonly string _timeSigID;

        public TimeSignatureMetrics(Graphics graphics, double gap, int numberOfStafflines, TextInfo numeratorTextInfo, TextInfo denominatorTextInfo)
            :base(CSSObjectClass.timeSig)
        {
            string suffix = "_" + numeratorTextInfo.Text + "/" + denominatorTextInfo.Text;
            _timeSigID = CSSObjectClass.timeSig.ToString() + suffix;

            if( ! TimeSigDefs.ContainsKey(_timeSigID))
            {
                List<TextMetrics> textMetricss = new List<TextMetrics>
                {
                    new TextMetrics(CSSObjectClass.timeSigNumerator, graphics, numeratorTextInfo),
                    new TextMetrics(CSSObjectClass.timeSigDenominator, graphics, denominatorTextInfo)
                };

                var numerMetrics = textMetricss[0];
                var denomMetrics = textMetricss[1];

                SetThisMetrics(numerMetrics, denomMetrics, gap);

                TimeSigDefs.Add(_timeSigID, textMetricss);
            }

            var textMetricsList = TimeSigDefs[_timeSigID];
            TextMetrics numerTM = TimeSigDefs[_timeSigID][0];
            TextMetrics denomTM = TimeSigDefs[_timeSigID][1];
            TextMetrics numerCloneTM = numerTM.Clone(CSSObjectClass.timeSigNumerator);
            TextMetrics denomCloneTM = denomTM.Clone(CSSObjectClass.timeSigDenominator);

            _textMetricsList.Add(numerCloneTM);
            _textMetricsList.Add(denomCloneTM);

            SetThisMetrics(numerCloneTM, denomCloneTM, gap);
        }

        private void SetThisMetrics(TextMetrics numerMetrics, TextMetrics denomMetrics, double gap)
        {
            numerMetrics.Move(0, 0 - numerMetrics.Bottom);
            denomMetrics.Move(0, (gap * 0.05) - denomMetrics.Top);

            _originY = gap * 2; // middle line of staff
            _top = numerMetrics.Top;
            _bottom = denomMetrics.Bottom;

            double nWidth = numerMetrics.Right - numerMetrics.Left;
            double dWidth = denomMetrics.Right - denomMetrics.Left;
            double width = (nWidth > dWidth) ? nWidth : dWidth;

            _originX = 0; // left aligned
            if(nWidth < width)
            {
                numerMetrics.Move((width - nWidth) / 2, 0);
            }
            if(dWidth < width)
            {
                denomMetrics.Move((width - dWidth) / 2, 0);
            }
            _right = (numerMetrics.Right > denomMetrics.Right) ? numerMetrics.Right : denomMetrics.Right;
            _left = (numerMetrics.Left < denomMetrics.Left) ? numerMetrics.Left : denomMetrics.Left;
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            _textMetricsList[0].Move(dx, dy);
            _textMetricsList[1].Move(dx, dy);
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgUseXY(CSSObjectClass.timeSig, _timeSigID, _originX, _originY);
        }

    }
}
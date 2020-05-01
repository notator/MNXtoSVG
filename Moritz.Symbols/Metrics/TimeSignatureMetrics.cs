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
        TextMetrics _numeratorMetrics = null;
        TextMetrics _denominatorMetrics = null;
        
        readonly string _numerator = null;
        readonly string _denominator = null;
        readonly string _timeSigID = null;
        public static IReadOnlyList<string> UsedTimeSigIDs => _usedTimeSigIDs as IReadOnlyList<string>;
        private static List<string> _usedTimeSigIDs = new List<string>();

        public TimeSignatureMetrics(Graphics graphics, double gap, int numberOfStafflines, TextInfo numeratorTextInfo, TextInfo denominatorTextInfo)
            :base(CSSObjectClass.timeSig)
        {
            TextMetrics numerMetrics = new TextMetrics(CSSObjectClass.timeSigNumerator, graphics, numeratorTextInfo);
            TextMetrics denomMetrics = new TextMetrics(CSSObjectClass.timeSigDenominator, graphics, denominatorTextInfo);

            _originY = gap * 2;
            numerMetrics.Move(0, (gap * 1.95) - numerMetrics.Bottom);
            denomMetrics.Move(0, (gap * 2.05) - denomMetrics.Top);
            _top = numerMetrics.Top;
            _bottom = denomMetrics.Bottom;

            double nWidth = numerMetrics.Right - numerMetrics.Left;
            double dWidth = denomMetrics.Right - denomMetrics.Left;
            double width = (nWidth > dWidth) ? nWidth : dWidth;
            numerMetrics.Move(width / 2, 0);
            denomMetrics.Move(width / 2, 0);
            _right = width;
            _left = 0;
            _originX = 0;

            _numeratorMetrics = numerMetrics;
            _denominatorMetrics = denomMetrics;

            _numerator = numeratorTextInfo.Text;
            _denominator = denominatorTextInfo.Text;

            _timeSigID = CSSObjectClass.timeSig.ToString() + "_" + _numerator + "/" + _denominator;
            _usedTimeSigIDs.Add(_timeSigID);
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            _numeratorMetrics.Move(dx, dy);
            _denominatorMetrics.Move(dx, dy);
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgUseXY(CSSObjectClass.timeSig, _timeSigID, _originX, _originY);
        }

    }
}
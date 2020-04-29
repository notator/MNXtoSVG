using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class TimeSignatureMetrics : GroupMetrics
    {
        public TimeSignatureMetrics(Graphics graphics, double gap, int numberOfStafflines, TextInfo numeratorTextInfo, TextInfo denominatorTextInfo)
            :base(CSSObjectClass.timeSignature)
        {
            TextMetrics numerMetrics = new TextMetrics(CSSObjectClass.timeSignatureNumerator, graphics, numeratorTextInfo);
            TextMetrics denomMetrics = new TextMetrics(CSSObjectClass.timeSignatureDenominator, graphics, denominatorTextInfo);

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
        }

        public override void Move(double dx, double dy)
        {
            base.Move(dx, dy);
            _numeratorMetrics.Move(dx, dy);
            _denominatorMetrics.Move(dx, dy);
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup(CSSObjectClass.ToString());
            w.SvgText(CSSObjectClass.timeSignatureNumerator, _numerator, _numeratorMetrics.OriginX, _numeratorMetrics.OriginY);
            w.SvgText(CSSObjectClass.timeSignatureDenominator, _numerator, _denominatorMetrics.OriginX, _denominatorMetrics.OriginY);
            w.SvgEndGroup();
        }

        TextMetrics _numeratorMetrics = null;
        TextMetrics _denominatorMetrics = null;
        readonly string _numerator = null;
        readonly string _denominator = null;
    }
}
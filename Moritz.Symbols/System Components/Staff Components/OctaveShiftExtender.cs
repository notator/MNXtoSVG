using MNX.Common;
using Moritz.Spec;
using Moritz.Xml;
using System.Collections.Generic;
using System.Drawing;

namespace Moritz.Symbols
{
    public class OctaveShiftExtender
    {
        public OctaveShiftExtenderMetrics Metrics = null;

        public OctaveShiftExtender(OctaveShift octaveShift, Graphics graphics, double leftChordLeft, double rightChordRight, double chordsY, double gap,
            bool displayText, bool displayEndMarker)
        {
            string text = null;
            switch(octaveShift.Type)
            {
                case OctaveShiftType.down3Oct:
                    text = "3oct";
                    break;
                case OctaveShiftType.down2Oct:
                    text = "2oct";
                    break;
                case OctaveShiftType.down1Oct:
                    text = "8va";
                    break;
                case OctaveShiftType.up1Oct:
                    text = "8va"; // bassa
                    break;
                case OctaveShiftType.up2Oct:
                    text = "2oct"; // bassa
                    break;
                case OctaveShiftType.up3Oct:
                    text = "3oct"; // bassa
                    break;
            }

            double hLineY = 0;
            double textY = 0;

            double textFontHeight = gap * 1.5; // == PageFormat.OctaveShiftExtenderTextFontHeight { get { return (GapVBPX * 1.5); } }
            double endMarkerHeight = gap;
            if(octaveShift.Orient == MNX.Common.Orientation.up)
            {
                hLineY = chordsY - (gap * 2);
                textY = hLineY + (textFontHeight * 0.6);
            }
            else
            {
                hLineY = chordsY + (gap * 2);
                textY = hLineY;
                endMarkerHeight *= -1;
            }

            string dashArrayString = (gap / 2).ToString(); // https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/stroke-dasharray

            TextInfo textInfo = new TextInfo(text, "Arial", textFontHeight, SVGFontWeight.bold, SVGFontStyle.italic, TextHorizAlign.left);
            TextMetrics textMetrics = new TextMetrics(CSSObjectClass.octaveShiftExtenderText, graphics, textInfo);
            textMetrics.Move(leftChordLeft - textMetrics.Left, textY - textMetrics.OriginY);

            Metrics = new OctaveShiftExtenderMetrics(textMetrics, leftChordLeft, rightChordRight, hLineY, dashArrayString, endMarkerHeight, displayText, displayEndMarker);
        }
    }

}

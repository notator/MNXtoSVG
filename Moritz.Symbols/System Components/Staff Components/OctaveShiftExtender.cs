using MNX.Globals;
using MNX.Common;
using Moritz.Spec;
using Moritz.Xml;
using System.Collections.Generic;
using System.Drawing;

namespace Moritz.Symbols
{
    public class Extender
    {
        public ExtenderMetrics Metrics;
    }

    public class OctaveShiftExtender : Extender
    {
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

            
            //double textFontHeight = gap * 1.5; // == PageFormat.OctaveShiftExtenderTextFontHeight { get { return (GapVBPX * 1.5); } }

            double textFontHeight = M.PageFormat.OctaveShiftExtenderTextFontHeight;
            double endMarkerHeight = gap * 0.8;
            if(octaveShift.Orient == MNX.Common.Orientation.up)
            {
                hLineY = chordsY - gap;
                textY = hLineY + (textFontHeight * 0.6);
            }
            else
            {
                hLineY = chordsY + (gap * 1.2);
                textY = hLineY + (gap * 0.1);
                endMarkerHeight *= -1;
            }

            string dashArrayString = (gap / 2).ToString(); // https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/stroke-dasharray

            TextInfo textInfo = new TextInfo(text, M.PageFormat.OctaveShiftExtenderTextFontFamily, textFontHeight, SVGFontWeight.bold, SVGFontStyle.italic, TextHorizAlign.left);
            TextMetrics textMetrics = new TextMetrics(CSSObjectClass.octaveShiftExtenderText, graphics, textInfo);
            textMetrics.Move(leftChordLeft - textMetrics.Left, textY - textMetrics.OriginY);

            Metrics = new OctaveShiftExtenderMetrics(textMetrics, leftChordLeft, rightChordRight, hLineY, dashArrayString, endMarkerHeight, displayText, displayEndMarker);
        }
    }

}

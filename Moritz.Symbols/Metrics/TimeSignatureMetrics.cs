using System.Drawing;
using Moritz.Xml;

namespace Moritz.Symbols
{
    internal class TimeSignatureMetrics : Metrics
    {
        private Graphics graphics;
        private TimeSignature timeSignature;
        private double gap;
        private int numberOfStafflines;
        private double strokeWidth;
        private CSSObjectClass timeSignatureClass;

        public TimeSignatureMetrics(Graphics graphics, TimeSignature timeSignature, double gap, int numberOfStafflines, double strokeWidth, CSSObjectClass timeSignatureClass)
        {
            this.graphics = graphics;
            this.timeSignature = timeSignature;
            this.gap = gap;
            this.numberOfStafflines = numberOfStafflines;
            this.strokeWidth = strokeWidth;
            this.timeSignatureClass = timeSignatureClass;
        }
    }
}
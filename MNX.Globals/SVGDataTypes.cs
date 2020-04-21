using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNX.AGlobals
{
    public class PageSettings
    {
        public readonly int width;
        public readonly int height;
        public readonly int marginTopPage1;
        public readonly int marginTopOther;
        public readonly int marginRight;
        public readonly int marginBottom;
        public readonly int marginLeft;

        internal PageSettings(
            int width,
            int height,
            int marginTopPage1,
            int marginTopOther,
            int marginRight,
            int marginBottom,
            int marginLeft)
        {
            this.width = width;
            this.height = height;
            this.marginTopPage1 = marginTopPage1;
            this.marginTopOther = marginTopOther;
            this.marginRight = marginRight;
            this.marginBottom = marginBottom;
            this.marginLeft = marginLeft;
        }
    }

    public class NotationSettings
    {
        public readonly double stafflineStemStrokeWidth = 0;
        public readonly double gapSize = 0;
        public readonly int minGapsBetweenStaves = 0;
        public readonly int minGapsBetweenSystems = 0;
        public readonly string systemStartBars = null;
        public readonly double crotchetsPerMinute = 0;

        internal NotationSettings(
            double stafflineStemStrokeWidth,
            double gapSize,
            int minGapsBetweenStaves,
            int minGapsBetweenSystems,
            string systemStartBars,
            double crotchetsPerMinute)
        {
            this.stafflineStemStrokeWidth = stafflineStemStrokeWidth;
            this.gapSize = gapSize;
            this.minGapsBetweenStaves = minGapsBetweenStaves;
            this.minGapsBetweenSystems = minGapsBetweenSystems;
            this.systemStartBars = systemStartBars;
            this.crotchetsPerMinute = crotchetsPerMinute;

        }
    }
}


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MNX.AGlobals
{
    public class SVGData
    {
        public readonly int pageWidth = 0;
        public readonly int pageHeight = 0;
        public readonly int marginTopPage1 = 0;
        public readonly int marginTopOther = 0;
        public readonly int marginRight = 0;
        public readonly int marginBottom = 0;
        public readonly int marginLeft = 0;
        public readonly double stafflineStemStrokeWidth = 0;
        public readonly double gap = 0;
        public readonly int minGapsBetweenStaves = 0;
        public readonly int minGapsBetweenSystems = 0;
        public readonly List<int> systemStartBars = null;
        public readonly double crotchetsPerMinute = 0;

        public SVGData(SVGDataStrings svgds)
        {
            pageWidth = int.Parse(svgds.Page.width);
            pageHeight = int.Parse(svgds.Page.height);
            marginTopPage1 = int.Parse(svgds.Page.marginTopPage1);
            marginTopOther = int.Parse(svgds.Page.marginTopOther);
            marginRight = int.Parse(svgds.Page.marginRight);
            marginBottom = int.Parse(svgds.Page.marginBottom);
            marginLeft = int.Parse(svgds.Page.marginLeft);
            stafflineStemStrokeWidth = double.Parse(svgds.MNXCommonData.stafflineStemStrokeWidth, A.En_USNumberFormat);
            gap = double.Parse(svgds.MNXCommonData.gapSize, A.En_USNumberFormat);
            minGapsBetweenStaves = int.Parse(svgds.MNXCommonData.minGapsBetweenStaves);
            minGapsBetweenSystems = int.Parse(svgds.MNXCommonData.minGapsBetweenSystems);
            char[] delimiters = { ',', ' ' };
            systemStartBars = A.StringToIntList(svgds.MNXCommonData.systemStartBars, delimiters);
            crotchetsPerMinute = double.Parse(svgds.MNXCommonData.crotchetsPerMinute, A.En_USNumberFormat);
        }
    }



}


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MNX.Globals
{
    public class SVGData
    {
        public readonly int pageWidth = 0;
        public readonly int pageHeight = 0;
        public readonly int pageMarginTopPage1 = 0;
        public readonly int pageMarginTopOther = 0;
        public readonly int pageMarginRight = 0;
        public readonly int pageMarginBottom = 0;
        public readonly int pageMarginLeft = 0;

        public readonly double notationStafflineStemStrokeWidth = 0;
        public readonly double notationGap = 0;
        public readonly int notationMinGapsBetweenStaves = 0;
        public readonly int notationMinGapsBetweenSystems = 0;
        public readonly List<int> notationSystemStartBars = null;
        public readonly double notationCrotchetsPerMinute = 0;

        public readonly string metadataTitle = null;
        public readonly string metadataAuthor = null;
        public readonly string metadataKeywords = null;
        public readonly string metadataComment = null;

        public readonly bool optionsPrintPage1Titles = true;
        public readonly bool optionsIncludeMIDIData = true;
        public readonly bool optionsPrintScoreAsScroll = true;

        public SVGData(SVGDataStrings svgds)
        {
            pageWidth = int.Parse(svgds.Page.width);
            pageHeight = int.Parse(svgds.Page.height);
            pageMarginTopPage1 = int.Parse(svgds.Page.marginTopPage1);
            pageMarginTopOther = int.Parse(svgds.Page.marginTopOther);
            pageMarginRight = int.Parse(svgds.Page.marginRight);
            pageMarginBottom = int.Parse(svgds.Page.marginBottom);
            pageMarginLeft = int.Parse(svgds.Page.marginLeft);

            notationStafflineStemStrokeWidth = double.Parse(svgds.Notation.stafflineStemStrokeWidth, M.En_USNumberFormat);
            notationGap = double.Parse(svgds.Notation.gapSize, M.En_USNumberFormat);
            notationMinGapsBetweenStaves = int.Parse(svgds.Notation.minGapsBetweenStaves);
            notationMinGapsBetweenSystems = int.Parse(svgds.Notation.minGapsBetweenSystems);
            char[] delimiters = { ',', ' ' };
            notationSystemStartBars = M.StringToIntList(svgds.Notation.systemStartBars, delimiters);
            notationCrotchetsPerMinute = double.Parse(svgds.Notation.crotchetsPerMinute, M.En_USNumberFormat);

            metadataTitle = svgds.Metadata.Title;
            metadataAuthor = svgds.Metadata.Author;
            metadataKeywords = svgds.Metadata.Keywords;
            metadataComment = svgds.Metadata.Comment;

            optionsPrintPage1Titles = (svgds.Options.PrintPage1Titles == "true");
            optionsIncludeMIDIData = (svgds.Options.IncludeMIDIData == "true");
            optionsPrintScoreAsScroll = (svgds.Options.PrintScoreAsScroll == "true");
        }
    }



}

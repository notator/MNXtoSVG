
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MNX.Globals
{
    public class Form1Data
    {
        public readonly string FileNameWithoutExtension = null;
        public readonly Form1PageData Page = new Form1PageData();
        public readonly Form1NotationData Notation = new Form1NotationData();
        public readonly Form1MetadataData Metadata = new Form1MetadataData();
        public readonly Form1OptionsData Options = new Form1OptionsData();

        public Form1Data(Form1StringData form1DataStrings)
        {
            FileNameWithoutExtension = form1DataStrings._fileName;

            Page.Width = int.Parse(form1DataStrings.Page.Width);
            Page.Height = int.Parse(form1DataStrings.Page.Height);
            Page.MarginTopPage1 = int.Parse(form1DataStrings.Page.MarginTopPage1);
            Page.MarginTopOther = int.Parse(form1DataStrings.Page.MarginTopOther);
            Page.MarginRight = int.Parse(form1DataStrings.Page.MarginRight);
            Page.MarginBottom = int.Parse(form1DataStrings.Page.MarginBottom);
            Page.MarginLeft = int.Parse(form1DataStrings.Page.MarginLeft);

            Notation.StafflineStemStrokeWidth = double.Parse(form1DataStrings.Notation.stafflineStemStrokeWidth, M.En_USNumberFormat);
            Notation.Gap = double.Parse(form1DataStrings.Notation.gapSize, M.En_USNumberFormat);
            Notation.MinGapsBetweenStaves = int.Parse(form1DataStrings.Notation.minGapsBetweenStaves);
            Notation.MinGapsBetweenSystems = int.Parse(form1DataStrings.Notation.minGapsBetweenSystems);
            char[] delimiters = { ',', ' ' };
            Notation.SystemStartBars = M.StringToIntList(form1DataStrings.Notation.systemStartBars, delimiters);
            Notation.CrotchetsPerMinute = double.Parse(form1DataStrings.Notation.crotchetsPerMinute, M.En_USNumberFormat);

            Metadata.Title = form1DataStrings.Metadata.Title;
            Metadata.Author = form1DataStrings.Metadata.Author;
            Metadata.Keywords = form1DataStrings.Metadata.Keywords;
            Metadata.Comment = form1DataStrings.Metadata.Comment;

            Options.WritePage1Titles = (form1DataStrings.Options.WritePage1Titles == "true");
            Options.WriteScrollScore = (form1DataStrings.Options.WriteScrollScore == "true");
            Options.IncludeMIDIData = (form1DataStrings.Options.IncludeMIDIData == "true");
        }
    }



}


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MNX.Globals
{
    public class SVGDataStrings
    {
        public PageSettings Page = null;
        public NotationAndSpeedSettings Notation = null;
        public Metadata Metadata = null;
        public Options Options = null; 

        private readonly string _svgDataPath;
        public readonly string _fileName;

        public SVGDataStrings(string svgDataPath)
        {
            _svgDataPath = svgDataPath;
            _fileName = Path.GetFileNameWithoutExtension(svgDataPath);

            using(XmlReader r = XmlReader.Create(svgDataPath))
            {
                M.ReadToXmlElementTag(r, "svgData"); // check that this is an svgData file

                M.ReadToXmlElementTag(r, "page", "notation", "metadata", "options");

                while(r.Name == "page" || r.Name == "notation" || r.Name == "metadata" || r.Name == "options")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "page":
                                Page = GetPage(r);
                                break;
                            case "notation":
                                Notation = GetMNXCommonData(r);
                                break;
                            case "metadata":
                                Metadata = GetMetadata(r);
                                break;
                            case "options":
                                Options = GetOptions(r);
                                break;
                        }
                        M.ReadToXmlElementTag(r, "page", "notation", "metadata", "options", "svgData");
                    }
                    
                }
                M.Assert(r.Name == "svgData"); // end of svgData
            }
        }

        private PageSettings GetPage(XmlReader r)
        {
            PageSettings ps = new PageSettings();

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "width":
                        ps.width = r.Value;
                        break;
                    case "height":
                        ps.height = r.Value;
                        break;
                    case "marginTopPage1":
                        ps.marginTopPage1 = r.Value;
                        break;
                    case "marginTopOther":
                        ps.marginTopOther = r.Value;
                        break;
                    case "marginRight":
                        ps.marginRight = r.Value;
                        break;
                    case "marginBottom":
                        ps.marginBottom = r.Value;
                        break;
                    case "marginLeft":
                        ps.marginLeft = r.Value;
                        break;
                }
            }

            return ps;
        }
        private NotationAndSpeedSettings GetMNXCommonData(XmlReader r)
        {
            NotationAndSpeedSettings nss = new NotationAndSpeedSettings();

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "stafflineStemStrokeWidth":
                        nss.stafflineStemStrokeWidth = r.Value;
                        break;
                    case "gapSize":
                        nss.gapSize = r.Value;
                        break;
                    case "minGapsBetweenStaves":
                        nss.minGapsBetweenStaves = r.Value;
                        break;
                    case "minGapsBetweenSystems":
                        nss.minGapsBetweenSystems = r.Value;
                        break;
                    case "systemStartBars":
                        nss.systemStartBars = r.Value;
                        break;
                    case "crotchetsPerMinute":
                        nss.crotchetsPerMinute = r.Value;
                        break;
                }
            }

            return nss;
        }
        private Metadata GetMetadata(XmlReader r)
        {
            Metadata m = new Metadata(); // sets all values to "" by default
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "title":
                        m.Title = r.Value;
                        break;
                    case "author":
                        m.Author = r.Value;
                        break;
                    case "keywords":
                        m.Keywords = r.Value;
                        break;
                    case "comment":
                        m.Comment = r.Value;
                        break;
                }
            }

            return m;

        }
        private Options GetOptions(XmlReader r)
        {
            Options op = new Options();
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "printPage1Titles":
                        op.PrintPage1Titles = r.Value;
                        break;
                    case "includeMIDIData":
                        op.IncludeMIDIData = r.Value;
                        break;
                    case "printScoreAsScroll":
                        op.PrintScoreAsScroll = r.Value;
                        break;
                }
            }

            return op;
        }

        #region save settings
        public void SaveSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                NewLineOnAttributes = true,
                CloseOutput = false
            };
            using(XmlWriter w = XmlWriter.Create(_svgDataPath, settings))
            {
                w.WriteStartDocument();
                w.WriteComment("file created: " + M.NowString);

                w.WriteStartElement("svgData");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, "SchemasBaseFolder" + "/svgData.xsd");

                WritePage(w);
                WriteNotation(w);
                WriteMetadata(w);
                WriteOptions(w);
                w.WriteEndElement(); // closes the moritzKrystalScore element
                                     // the XmlWriter is closed automatically at the end of this using clause.
            }
        }

        private void WritePage(XmlWriter w)
        {
            var page = this.Page;
            if(page != null)
            {
                w.WriteStartElement("page");

                w.WriteAttributeString("width", page.width);
                w.WriteAttributeString("height", page.height);
                w.WriteAttributeString("marginTopPage1", page.marginTopPage1);
                w.WriteAttributeString("marginTopOther", page.marginTopOther);
                w.WriteAttributeString("marginRight", page.marginRight);
                w.WriteAttributeString("marginBottom", page.marginBottom);
                w.WriteAttributeString("marginLeft", page.marginLeft);

                w.WriteEndElement(); // page
            }

            
        }
        private void WriteNotation(XmlWriter w)
        {
            var notes = this.Notation;
            if(notes != null)
            {
                w.WriteStartElement("notation");

                w.WriteAttributeString("stafflineStemStrokeWidth", notes.stafflineStemStrokeWidth);
                w.WriteAttributeString("gapSize", notes.gapSize);
                w.WriteAttributeString("minGapsBetweenStaves", notes.minGapsBetweenStaves);
                w.WriteAttributeString("minGapsBetweenSystems", notes.minGapsBetweenSystems);
                w.WriteAttributeString("systemStartBars", notes.systemStartBars);
                w.WriteAttributeString("crotchetsPerMinute", notes.crotchetsPerMinute);

                w.WriteEndElement(); // notation
            }
        }
        private void WriteMetadata(XmlWriter w)
        {
            var m = this.Metadata;
            if(m != null)
            {
                if((String.IsNullOrEmpty(m.Title) == false)
                || (String.IsNullOrEmpty(m.Author) == false)
                || (String.IsNullOrEmpty(m.Keywords) == false)
                || (String.IsNullOrEmpty(m.Comment) == false))
                {
                    w.WriteStartElement("metadata");

                    if(String.IsNullOrEmpty(m.Title) == false)
                        w.WriteAttributeString("title", m.Title);
                    if(String.IsNullOrEmpty(m.Author) == false)
                        w.WriteAttributeString("author", m.Author);
                    if(String.IsNullOrEmpty(m.Keywords) == false)
                        w.WriteAttributeString("keywords", m.Keywords);
                    if(String.IsNullOrEmpty(m.Comment) == false)
                        w.WriteAttributeString("comment", m.Comment);

                    w.WriteEndElement(); // metadata
                }
            }
        }
        private void WriteOptions(XmlWriter w)
        {
            var op = this.Options;

            M.Assert(op != null);

            w.WriteStartElement("options");

            w.WriteAttributeString("printPage1Titles", op.PrintPage1Titles);
            w.WriteAttributeString("includeMIDIData", op.IncludeMIDIData);
            w.WriteAttributeString("printScoreAsScroll", op.PrintScoreAsScroll);

            w.WriteEndElement(); // options
        }
        #endregion save settings

        public override string ToString()
        {
            return _fileName;
        }
    }

    public class OptionsForWriteAll : SVGDataStrings
    {
        public OptionsForWriteAll()
                :base(M.OptionsForWriteAll_Path)
        {
        }
    }
}

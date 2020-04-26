
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MNX.Globals
{
    public class Form1StringData
    {
        public Form1PageStrings Page = null;
        public Form1NotationStrings Notation = null;
        public Form1MetadataStrings Metadata = null;
        public Form1OptionsStrings Options = null; 

        private readonly string _form1DataPath;
        public readonly string _fileName;

        public Form1StringData(string form1DataPath)
        {
            _form1DataPath = form1DataPath;
            _fileName = Path.GetFileNameWithoutExtension(form1DataPath);

            using(XmlReader r = XmlReader.Create(form1DataPath))
            {
                M.ReadToXmlElementTag(r, "form1Data"); // check that this is a form1Data file

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
                        M.ReadToXmlElementTag(r, "page", "notation", "metadata", "options", "form1Data");
                    }
                    
                }
                M.Assert(r.Name == "form1Data"); // end of form1Data
            }
        }

        private Form1PageStrings GetPage(XmlReader r)
        {
            Form1PageStrings ps = new Form1PageStrings();

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "width":
                        ps.Width = r.Value;
                        break;
                    case "height":
                        ps.Height = r.Value;
                        break;
                    case "marginTopPage1":
                        ps.MarginTopPage1 = r.Value;
                        break;
                    case "marginTopOther":
                        ps.MarginTopOther = r.Value;
                        break;
                    case "marginRight":
                        ps.MarginRight = r.Value;
                        break;
                    case "marginBottom":
                        ps.MarginBottom = r.Value;
                        break;
                    case "marginLeft":
                        ps.MarginLeft = r.Value;
                        break;
                }
            }

            return ps;
        }
        private Form1NotationStrings GetMNXCommonData(XmlReader r)
        {
            Form1NotationStrings nss = new Form1NotationStrings();

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
        private Form1MetadataStrings GetMetadata(XmlReader r)
        {
            Form1MetadataStrings m = new Form1MetadataStrings(); // sets all values to "" by default
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
        private Form1OptionsStrings GetOptions(XmlReader r)
        {
            Form1OptionsStrings op = new Form1OptionsStrings();
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "writePage1Titles":
                        op.WritePage1Titles = r.Value;
                        break;
                    case "writeScrollScore":
                        op.WriteScrollScore = r.Value;
                        break;
                    case "includeMIDIData":
                        op.IncludeMIDIData = r.Value;
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
            using(XmlWriter w = XmlWriter.Create(_form1DataPath, settings))
            {
                w.WriteStartDocument();
                w.WriteComment("file created: " + M.NowString);

                w.WriteStartElement("form1Data");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, "SchemasBaseFolder" + "/form1Data.xsd");

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

                w.WriteAttributeString("width", page.Width);
                w.WriteAttributeString("height", page.Height);
                w.WriteAttributeString("marginTopPage1", page.MarginTopPage1);
                w.WriteAttributeString("marginTopOther", page.MarginTopOther);
                w.WriteAttributeString("marginRight", page.MarginRight);
                w.WriteAttributeString("marginBottom", page.MarginBottom);
                w.WriteAttributeString("marginLeft", page.MarginLeft);

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

            w.WriteAttributeString("writePage1Titles", op.WritePage1Titles);
            w.WriteAttributeString("writeScrollScore", op.WriteScrollScore);
            w.WriteAttributeString("includeMIDIData", op.IncludeMIDIData);

            w.WriteEndElement(); // options
        }
        #endregion save settings

        public override string ToString()
        {
            return _fileName;
        }
    }

    public class OptionsForWriteAll : Form1StringData
    {
        public OptionsForWriteAll()
                :base(M.OptionsForWriteAll_Path)
        {
        }
    }
}

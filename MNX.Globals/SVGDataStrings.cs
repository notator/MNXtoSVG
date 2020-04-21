
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MNX.AGlobals
{
    public class SVGDataStrings
    {
        public PageSettings Page = null;
        public NotationAndSpeedSettings MNXCommonData = null;

        private readonly string _svgDataPath;
        public readonly string _fileName;

        public SVGDataStrings(string svgDataPath)
        {
            _svgDataPath = svgDataPath;
            _fileName = Path.GetFileNameWithoutExtension(svgDataPath);

            using(XmlReader r = XmlReader.Create(svgDataPath))
            {
                A.ReadToXmlElementTag(r, "svgData"); // check that this is an svgData file

                A.ReadToXmlElementTag(r, "page", "notation");

                while(r.Name == "page" || r.Name == "notation")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "page":
                                Page = GetPage(r);
                                break;
                            case "notation":
                                MNXCommonData = GetMNXCommonData(r);
                                break;
                        }
                        A.ReadToXmlElementTag(r, "page", "notation", "svgData");
                    }
                    
                }
                A.Assert(r.Name == "svgData"); // end of svgData
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
                w.WriteComment("file created: " + A.NowString);

                w.WriteStartElement("svgData");
                w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                w.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, "SchemasBaseFolder" + "/svgData.xsd");

                WritePage(w);
                WriteMNXCommon(w);
                w.WriteEndElement(); // closes the moritzKrystalScore element
                                     // the XmlWriter is closed automatically at the end of this using clause.
            }
        }

        private void WritePage(XmlWriter w)
        {
            var page = this.Page;
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
        private void WriteMNXCommon(XmlWriter w)
        {
            var notes = this.MNXCommonData;

            w.WriteStartElement("notation");

            w.WriteAttributeString("stafflineStemStrokeWidth", notes.stafflineStemStrokeWidth);
            w.WriteAttributeString("gapSize", notes.gapSize);
            w.WriteAttributeString("minGapsBetweenStaves", notes.minGapsBetweenStaves);
            w.WriteAttributeString("minGapsBetweenSystems", notes.minGapsBetweenSystems);
            w.WriteAttributeString("systemStartBars", notes.systemStartBars);
            w.WriteAttributeString("crotchetsPerMinute", notes.crotchetsPerMinute);

            w.WriteEndElement(); // notation
        }
        #endregion save settings

        public override string ToString()
        {
            return _fileName;
        }
    }
}

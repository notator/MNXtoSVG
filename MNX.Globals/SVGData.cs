
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MNX.AGlobals
{
    public class SVGData
    {
        public readonly PageSettings Page = null;
        public readonly NotationSettings MNXCommonData = null;

        private readonly string _svgDataPath;
        public readonly string _fileName;

        public SVGData(string svgDataPath)
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
            int width = 0;
            int height = 0;
            int marginTopPage1 = 0;
            int marginTopOther = 0;
            int marginRight = 0;
            int marginBottom = 0;
            int marginLeft = 0;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "width":
                        int.TryParse(r.Value, out width);
                        break;
                    case "height":
                        int.TryParse(r.Value, out height);
                        break;
                    case "marginTopPage1":
                        int.TryParse(r.Value, out marginTopPage1);
                        break;
                    case "marginTopOther":
                        int.TryParse(r.Value, out marginTopOther);
                        break;
                    case "marginRight":
                        int.TryParse(r.Value, out marginRight);
                        break;
                    case "marginBottom":
                        int.TryParse(r.Value, out marginBottom);
                        break;
                    case "marginLeft":
                        int.TryParse(r.Value, out marginLeft);
                        break;
                }
            }

            return new PageSettings(
                width, 
                height,
                marginTopPage1,
                marginTopOther,
                marginRight,
                marginBottom,
                marginLeft);
        }
        private NotationSettings GetMNXCommonData(XmlReader r)
        {
            double stafflineStemStrokeWidth = 0;
            double gapSize = 0;
            int minGapsBetweenStaves = 0;
            int minGapsBetweenSystems = 0;
            string systemStartBars = "";
            double crotchetsPerMinute = 0;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "stafflineStemStrokeWidth":
                        double.TryParse(r.Value, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out stafflineStemStrokeWidth);
                        break;
                    case "gapSize":
                        double.TryParse(r.Value, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out gapSize);
                        break;
                    case "minGapsBetweenStaves":
                        int.TryParse(r.Value, out minGapsBetweenStaves);
                        break;
                    case "minGapsBetweenSystems":
                        int.TryParse(r.Value, out minGapsBetweenSystems);
                        break;
                    case "systemStartBars":
                        systemStartBars = r.Value;
                        break;
                    case "crotchetsPerMinute":
                        double.TryParse(r.Value, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out crotchetsPerMinute);
                        break;
                }
            }

            return new NotationSettings(
                stafflineStemStrokeWidth,
                gapSize,
                minGapsBetweenStaves,
                minGapsBetweenSystems,
                systemStartBars,
                crotchetsPerMinute);
        }

        #region save settings
        public void SaveSettings()
        {
            A.Assert(!string.IsNullOrEmpty(_svgDataPath));

            A.CreateDirectoryIfItDoesNotExist(this._svgDataPath);

            #region do the save
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
            #endregion do the save

            //SetGroupBoxIsSaved(PageGroupBox, NotationGroupBox, TimeGroupBox);
        }

        private void WritePage(XmlWriter w)
        {
            w.WriteStartElement("page");

            //w.WriteAttributeString("width", WidthTextBox.Text);
            //w.WriteAttributeString("height", HeightTextBox.Text);
            //w.WriteAttributeString("marginTopPage1", MarginTopPage1TextBox.Text);
            //w.WriteAttributeString("marginTopOther", MarginTopOtherTextBox.Text);
            //w.WriteAttributeString("marginRight", MarginRightTextBox.Text);
            //w.WriteAttributeString("marginBottom", MarginBottomTextBox.Text);
            //w.WriteAttributeString("marginLeft", MarginLeftTextBox.Text);

            w.WriteEndElement(); // page

            
        }
        private void WriteMNXCommon(XmlWriter w)
        {
            w.WriteStartElement("notation");

            //w.WriteAttributeString("stafflineStemStrokeWidth", StafflineStemStrokeWidthComboBox.Text);
            //w.WriteAttributeString("gapSize", GapSizeComboBox.Text);
            //w.WriteAttributeString("minGapsBetweenStaves", MinGapsBetweenStavesTextBox.Text);
            //w.WriteAttributeString("minGapsBetweenSystems", MinGapsBetweenSystemsTextBox.Text);
            //w.WriteAttributeString("systemStartBars", SystemStartBarsTextBox.Text);
            //w.WriteAttributeString("crotchetsPerMinute", CrotchetsPerMinuteTextBox.Text);

            w.WriteEndElement(); // notation
        }
        #endregion save settings

        public override string ToString()
        {
            return _fileName;
        }
    }

}


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MNX.AGlobals
{
    public class SVGData
    {
        Page Page = null;
        MNXCommonData MNXCommonData = null;

        public readonly string FileName;

        public SVGData(string svgDataPath)
        {
            FileName = Path.GetFileNameWithoutExtension(svgDataPath);

            using(XmlReader r = XmlReader.Create(svgDataPath))
            {
                A.ReadToXmlElementTag(r, "svgData"); // check that this is an svgData file

                A.ReadToXmlElementTag(r, "page", "mnxCommon");

                while(r.Name == "page" || r.Name == "mnxCommon")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "page":
                                Page = GetPage(r);
                                break;
                            case "mnxCommon":
                                MNXCommonData = GetMNXCommonData(r);
                                break;
                        }
                        A.ReadToXmlElementTag(r, "page", "mnxCommon", "svgData");
                    }
                    
                }
                A.Assert(r.Name == "svgData"); // end of svgData
            }
        }

        private Page GetPage(XmlReader r)
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

            return new Page(
                width, 
                height,
                marginTopPage1,
                marginTopOther,
                marginRight,
                marginBottom,
                marginLeft);
        }

        private MNXCommonData GetMNXCommonData(XmlReader r)
        {
            double stafflineStemStrokeWidth = 0;
            double gapSize = 0;
            int minGapsBetweenStaves = 0;
            int minGapsBetweenSystems = 0;
            List<int> systemStartBars = new List<int>();
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
                        systemStartBars = A.StringToIntList(r.Value, ' ');
                        break;
                    case "crotchetsPerMinute":
                        double.TryParse(r.Value, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out crotchetsPerMinute);
                        break;
                }
            }

            return new MNXCommonData(
                stafflineStemStrokeWidth,
                gapSize,
                minGapsBetweenStaves,
                minGapsBetweenSystems,
                systemStartBars,
                crotchetsPerMinute);
        }

        public override string ToString()
        {
            return FileName;
        }
    }

}

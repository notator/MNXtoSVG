using MNX.Globals;

using System.Collections.Generic;
using System.Text;
using System.Xml;

using System.Xml.Schema;
using System.Xml.Serialization;

namespace MNX.Common
{
    public class TextBlock : IGlobalDirectionsComponent, IPartDirectionsComponent, ISequenceDirectionsComponent
    {
        public List<string> Lines = new List<string>();
        public readonly int TicksPosInScore = -1; // set in ctor

        public TextBlock(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "text-block");

            var xmlContent = r.ReadInnerXml();

            List<string> lines = GetLines(xmlContent);
            
            Lines.AddRange(lines);

            M.Assert(r.NodeType == XmlNodeType.Whitespace); // end of "text-block"
        }

        private List<string> GetLines(string xmlContent)
        {
            var brStrLength = GetLineBreakStringLength(xmlContent);

            xmlContent = RemoveComments(xmlContent);

            List<string> lines = new List<string>();

            while(xmlContent.Length > 0)
            {
                var brStart = xmlContent.IndexOf("<br");
                int charsToRemove;
                StringBuilder sb = new StringBuilder();
                if(brStart >= 0)
                {
                    sb.Append(xmlContent, 0, brStart);
                    charsToRemove = sb.Length + brStrLength;

                    string line = sb.ToString();
                    line = line.Trim();

                    lines.Add(line);
                }
                else
                {
                    xmlContent = xmlContent.Trim();
                    if(xmlContent.Length > 0)
                    {
                        lines.Add(xmlContent);
                    }
                    charsToRemove = xmlContent.Length;
                }

                xmlContent = xmlContent.Remove(0, charsToRemove);
            }

            return lines;
        }

        private string RemoveComments(string xmlContent)
        {
            if(!string.IsNullOrEmpty(xmlContent))
            {
                var start = 0;
                while(start >= 0)
                {
                    start = xmlContent.IndexOf("<!--");
                    if(start >= 0)
                    {
                        var end = xmlContent.IndexOf("-->", start);
                        int charsToRemove = end - start + 3;
                        xmlContent = xmlContent.Remove(start, charsToRemove);
                    }
                }
            }

            return xmlContent;
        }

        private int GetLineBreakStringLength(string xmlContent)
        {
            int lineBreakStringLength = 0;

            if(!string.IsNullOrEmpty(xmlContent))
            {
                var start = xmlContent.IndexOf("<br");
                if(start >= 0)
                {
                    var end = xmlContent.IndexOf("/>", start);
                    lineBreakStringLength = end - start + 2;
                }
            }

            return lineBreakStringLength;
        }
    }
}
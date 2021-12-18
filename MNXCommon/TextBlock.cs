using MNX.Globals;

using System.Collections.Generic;
using System.Xml;

using System.Xml.Schema;
using System.Xml.Serialization;

namespace MNX.Common
{
    public class TextBlock : IDirectionsComponent
    {
        public readonly string Lines = "";
        public readonly int TicksPosInScore = -1; // set in ctor

        public TextBlock(XmlReader r, int ticksPosInScore)
        {
            // returns all XmlNodes whose name is nodeName in the document tree
            List<XmlNode> GetNodesByName(XmlNode rootNode, string nodeName)
            {
                List<XmlNode> xmlNodes = new List<XmlNode>();

                void RecursiveGetNodes(XmlNode baseNode)
                {
                    if(baseNode.ChildNodes.Count > 0)
                    {
                        foreach(XmlNode childNode in baseNode.ChildNodes)
                        {
                            if(childNode.Name == nodeName)
                            {
                                xmlNodes.Add(childNode);
                            }
                            RecursiveGetNodes(childNode);
                        }  
                    }
                }

                RecursiveGetNodes(rootNode);

                return xmlNodes;
            }

            M.Assert(r.Name == "text-block");

            string filePath = r.BaseURI;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode root = doc.DocumentElement;
            List<XmlNode> textBlockNodes = GetNodesByName(root, "text-block");
            foreach(var textBlockNode in textBlockNodes)
            {
                List<XmlNode> brNodes = GetNodesByName(textBlockNode, "br");
                List<XmlNode> aNodes = GetNodesByName(textBlockNode, "a");
                List<XmlNode> iNodes = GetNodesByName(textBlockNode, "i");
                List<XmlNode> emNodes = GetNodesByName(textBlockNode, "em");
                List<XmlNode> spanNodes = GetNodesByName(textBlockNode, "span");
            }

            M.ReadToXmlElementTag(r, "text-block");

            M.Assert(r.Name == "text-block"); // end of "text-block"
        }
    }
}
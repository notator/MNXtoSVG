using MNX.Globals;

using System.Collections.Generic;
using System.Xml;

using System.Xml.Schema;
using System.Xml.Serialization;

namespace MNX.Common
{
    public class SimpleTextBlock : IDirectionsComponent
    {
        public readonly string Lines = "";
        public readonly int TicksPosInScore = -1; // set in ctor

        public SimpleTextBlock(XmlReader r, int ticksPosInScore)
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

            M.Assert(r.Name == "simple-text-block");

            string filePath = r.BaseURI;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode root = doc.DocumentElement;
            List<XmlNode> simpleTextBlockNodes = GetNodesByName(root, "simple-text-block");
            foreach(var simpleTextBlockNode in simpleTextBlockNodes)
            {
                List<XmlNode> lineNodes = GetNodesByName(simpleTextBlockNode, "line");
                foreach(var lineNode in lineNodes)
                {
                    List<XmlNode> aNodes = GetNodesByName(lineNode, "a");
                    List<XmlNode> iNodes = GetNodesByName(lineNode, "i");
                    List<XmlNode> emNodes = GetNodesByName(lineNode, "em");
                    List<XmlNode> spanNodes = GetNodesByName(lineNode, "span");
                }
            }

            M.ReadToXmlElementTag(r, "simple-text-block");

            M.Assert(r.Name == "simple-text-block"); // end of "text-block"
        }
    }
}
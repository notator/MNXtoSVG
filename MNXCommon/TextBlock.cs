﻿using MNX.Globals;

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
            List<XmlNode> pNodes = GetNodesByName(root, "p");
            List<XmlNode> divNodes = GetNodesByName(root, "div");
            List<XmlNode> textBlockNodes = GetNodesByName(root, "text-block");
            List<XmlNode> simpleTextBlockNodes = GetNodesByName(root, "simple-text-block");

            M.ReadToXmlElementTag(r, "text-block");

            M.Assert(r.Name == "text-block"); // end of "text-block"
        }
    }
}
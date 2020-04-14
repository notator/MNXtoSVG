using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    class MNX : IWritable
    {
        private readonly Head Head = null;
        private readonly Score Score = null;
        private readonly Collection Collection = null;
        public readonly string FileName;

        public MNX(string mnxPath)
        {
            FileName = Path.GetFileNameWithoutExtension(mnxPath);

            using(XmlReader r = XmlReader.Create(mnxPath))
            {
                G.ReadToXmlElementTag(r, "mnx"); // check that this is an mnx file

                // https://w3c.github.io/mnx/specification/common/#the-mnx-element

                G.ReadToXmlElementTag(r, "head", "score", "collection");
                bool headFound = false;
                while(r.Name == "head" || r.Name == "score" || r.Name == "collection")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "head":
                                G.Assert(headFound == false);
                                Head = new Head(r); // not implemented yet
                                headFound = true;
                                break;
                            case "score":
                                Score = new Score(r);
                                break;
                            case "collection":
                                Collection = new Collection(r); // not implemented yet
                                break;

                        }
                    }
                    G.ReadToXmlElementTag(r, "head", "score", "collection", "mnx");
                }
                G.Assert(r.Name == "mnx"); // end of mnx

                // Head is optional -- can be null here.
                // Score and Collection are alternatives
                G.Assert((Score != null && Collection == null) || (Score == null && Collection != null));
            }
        }

        internal void WriteSVG(string svgPath)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return FileName;
        }

        public void WriteSVG(XmlWriter w)
        {
            Score.WriteSVG(w);
        }
    }

}

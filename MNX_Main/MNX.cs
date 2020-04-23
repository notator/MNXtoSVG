using MNX.Globals;
using System;
using System.IO;
using System.Xml;
using Moritz.Spec;
using System.Collections.Generic;

namespace MNX_Main
{
    // https://w3c.github.io/mnx/specification/common/#the-mnx-element
    internal class MNX
    {
        private readonly Head Head = null;
        private readonly Score Score = null;
        private readonly Collection Collection = null;

        internal readonly string FileName;

        internal MNX(string mnxPath)
        {
            FileName = Path.GetFileNameWithoutExtension(mnxPath);

            using(XmlReader r = XmlReader.Create(mnxPath))
            {
                A.ReadToXmlElementTag(r, "mnx"); // check that this is an mnx file

                // https://w3c.github.io/mnx/specification/common/#the-mnx-element

                A.ReadToXmlElementTag(r, "head", "score", "collection");
                bool headFound = false;
                while(r.Name == "head" || r.Name == "score" || r.Name == "collection")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "head":
                                A.Assert(headFound == false);
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
                    A.ReadToXmlElementTag(r, "head", "score", "collection", "mnx");
                }
                A.Assert(r.Name == "mnx"); // end of mnx

                // Head is optional -- can be null here.
                // Score and Collection are alternatives
                A.Assert((Score != null && Collection == null) || (Score == null && Collection != null));
            }
        }

        internal List<Bar> ToBars()
        {
            List<Bar> rval = new List<Bar>();
            return rval;
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}

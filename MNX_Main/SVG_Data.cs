using MNX.AGlobals;
using System.IO;
using System.Xml;

namespace MNX_Main
{
    // https://w3c.github.io/mnx/specification/common/#the-mnx-element
    internal class SVG_Data
    {
        private readonly Head Head = null;
        private readonly Score Score = null;
        private readonly Collection Collection = null;

        public readonly string FileName;

        public SVG_Data(string mnxPath)
        {
            //FileName = Path.GetFileNameWithoutExtension(mnxPath);

            //using(XmlReader r = XmlReader.Create(mnxPath))
            //{
            //    A.ReadToXmlElementTag(r, "mnx"); // check that this is an mnx file

            //    // https://w3c.github.io/mnx/specification/common/#the-mnx-element

            //    A.ReadToXmlElementTag(r, "head", "score", "collection");
            //    bool headFound = false;
            //    while(r.Name == "head" || r.Name == "score" || r.Name == "collection")
            //    {
            //        if(r.NodeType != XmlNodeType.EndElement)
            //        {
            //            switch(r.Name)
            //            {
            //                case "head":
            //                    A.Assert(headFound == false);
            //                    Head = new Head(r); // not implemented yet
            //                    headFound = true;
            //                    break;
            //                case "score":
            //                    Score = new Score(r);
            //                    break;
            //                case "collection":
            //                    Collection = new Collection(r); // not implemented yet
            //                    break;

            //            }
            //        }
            //        A.ReadToXmlElementTag(r, "head", "score", "collection", "mnx");
            //    }
            //    A.Assert(r.Name == "mnx"); // end of mnx

            //    // Head is optional -- can be null here.
            //    // Score and Collection are alternatives
            //    A.Assert((Score != null && Collection == null) || (Score == null && Collection != null));
            //}
        }

        public override string ToString()
        {
            return FileName;
        }
    }

}

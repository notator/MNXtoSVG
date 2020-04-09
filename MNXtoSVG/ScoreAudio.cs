using System;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class ScoreAudio : IWritable
    {
        public ScoreAudio(XmlReader r)
        {
            G.Assert(r.Name == "score-audio");
            // https://w3c.github.io/mnx/specification/common/#elementdef-score-audio

            throw new NotImplementedException();

            //G.Assert(r.Name == "mnx-common");

            //while(r.Name == "head" || r.Name == "score")
            //{
            //    if(r.NodeType != XmlNodeType.EndElement)
            //    {
            //        switch(r.Name)
            //        {
            //            case "head":
            //                GetHead(r);
            //                break;
            //            case "score":
            //                MNXCommonScore = new MNXScore(r);
            //                break;
            //        }
            //    }
            //    G.ReadToXmlElementTag(r, "head", "score", "mnx");
            //}
            //G.Assert(r.Name == "mnx"); // end of mnx

        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
using MNXtoSVG.Globals;
using System;
using System.Xml;

namespace MNXtoSVG
{
    internal class ScoreAudio
    {
        // https://w3c.github.io/mnx/specification/common/#elementdef-score-audio

        public ScoreAudio(XmlReader r)
        {
            G.Assert(r.Name == "score-audio");

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
    }
}
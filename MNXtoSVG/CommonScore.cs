using MNXtoSVG.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNXtoSVG
{
    internal class CommonScore
    {
        // https://w3c.github.io/mnx/specification/common/#the-mnx-common-element

        public CommonScore(XmlReader r)
        {
            G.Assert(r.Name == "mnx-common");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "profile":
                        { 
                            switch(r.Value)
                            {
                                case "standard":
                                    this.Profile = G.MNXCommonProfile.standard;
                                    break;
                                default:
                                    throw new ApplicationException("Unknown profile");
                            }
                        }
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }
                                                                    
            G.ReadToXmlElementTag(r, "global", "part", "score-audio");

            while(r.Name == "global" || r.Name == "part" || r.Name == "score-audio")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "global":
                            Globals.Add(new Global(r));
                            break;
                        case "part":
                            Parts.Add(new Part(r));
                            break;
                        case "score-audio":
                            ScoreAudios.Add(new ScoreAudio(r));
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "global", "part", "score-audio", "mnx-common");
            }
            G.Assert(r.Name == "mnx-common"); // end of "mnx-common"

            G.Assert(Profile != G.MNXCommonProfile.undefined);
            G.Assert(Globals.Count > 0);
            G.Assert(Parts.Count > 0);
            G.Assert(ScoreAudios.Count >= 0);
        }

        public readonly G.MNXCommonProfile Profile = G.MNXCommonProfile.undefined;
        public readonly List<Global> Globals = new List<Global>();
        public readonly List<Part> Parts = new List<Part>();
        public readonly List<ScoreAudio> ScoreAudios = new List<ScoreAudio>();
    }
}
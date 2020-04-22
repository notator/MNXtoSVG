using System;
using System.Collections.Generic;
using System.Xml;
using MNX.AGlobals;

namespace MNX.Common
{
    public class MNXCommon
    {
        internal readonly List<Global> Globals = new List<Global>();
        internal readonly List<Part> Parts = new List<Part>();
        internal readonly List<ScoreAudio> ScoreAudios = new List<ScoreAudio>();

        public MNXCommon(XmlReader r)
        {
            A.Assert(r.Name == "mnx-common");
            // https://w3c.github.io/mnx/specification/common/#the-mnx-common-element

            A.Profile = null;

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
                                    A.Profile = MNXProfile.MNXCommonStandard;
                                    break;
                                default:
                                    A.ThrowError("Error: unknown profile");
                                    break;
                            }
                        }
                        break;
                    default:
                        A.ThrowError("Error: unknown attribute");
                        break;
                }
            }

            A.ReadToXmlElementTag(r, "global", "part", "score-audio");

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
                A.ReadToXmlElementTag(r, "global", "part", "score-audio", "mnx-common");
            }

            AdjustForGraceNotes();

            A.Assert(r.Name == "mnx-common"); // end of "mnx-common"

            A.Assert(Globals.Count > 0);
            A.Assert(Parts.Count > 0);
            A.Assert(ScoreAudios.Count >= 0);
        }

        private void AdjustForGraceNotes()
        {
            for(var partIndex = 0; partIndex < Parts.Count; partIndex++)
            {
                List<Measure> measures = Parts[partIndex].Measures;
                for(var measureIndex = 0; measureIndex < measures.Count; measureIndex++)
                {
                    Measure measure = measures[measureIndex];
                    measure.AdjustForGraceNotes();
                }
            }
        }
    }
}
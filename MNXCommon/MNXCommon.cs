using System;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    public class MNXCommon
    {
        public readonly Global Global = null;
        public readonly List<Part> Parts = new List<Part>();
        public readonly List<ScoreAudio> ScoreAudios = new List<ScoreAudio>();
        public readonly int NumberOfMeasures;

        public List<List<int>> VoicesPerStaffPerPart
        {
            get
            {
                var voicesPerStaffPerPart = new List<List<int>>();
                foreach(var part in Parts)
                {
                    voicesPerStaffPerPart.Add(part.VoicesPerStaff);
                }
                return voicesPerStaffPerPart;
            }
        }

        public MNXCommon(XmlReader r)
        {
            M.Assert(r.Name == "mnx-common");
            // https://w3c.github.io/mnx/specification/common/#the-mnx-common-element

            M.Profile = null;

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
                                    M.Profile = MNXProfile.MNXCommonStandard;
                                    break;
                                default:
                                    M.ThrowError("Error: unknown profile");
                                    break;
                            }
                        }
                        break;
                    default:
                        M.ThrowError("Error: unknown attribute");
                        break;
                }
            }

            M.ReadToXmlElementTag(r, "global", "part", "score-audio");

            while(r.Name == "global" || r.Name == "part" || r.Name == "score-audio")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "global":
                            Global = new Global(r);
                            break;
                        case "part":
                            Parts.Add(new Part(r));
                            break;
                        case "score-audio":
                            ScoreAudios.Add(new ScoreAudio(r));
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "global", "part", "score-audio", "mnx-common");
            }

            NumberOfMeasures = Global.Measures.Count;

            AdjustForGraceNotes();

            M.Assert(r.Name == "mnx-common"); // end of "mnx-common"

            M.Assert(Global != null);
            M.Assert(Parts.Count > 0);
            M.Assert(ScoreAudios.Count >= 0);
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
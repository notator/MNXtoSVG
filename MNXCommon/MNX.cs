using MNX.Globals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-mnx-element
    public class MNX
    {
        public readonly Global Global = null;
        public readonly List<Part> Parts = new List<Part>();
        public readonly List<ScoreAudio> ScoreAudios = new List<ScoreAudio>();
        public readonly int NumberOfMeasures;
        internal readonly string FileName;

        public MNX(string mnxPath)
        {
            FileName = Path.GetFileNameWithoutExtension(mnxPath);

            using(XmlReader r = XmlReader.Create(mnxPath))
            {
                M.ReadToXmlElementTag(r, "mnx"); // check that this is an mnx file
                // https://w3c.github.io/mnx/specification/common/#the-mnx-common-element

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
                                Parts.Add(new Part(r, Global.GlobalMeasures));
                                break;
                            case "score-audio":
                                ScoreAudios.Add(new ScoreAudio(r));
                                break;
                        }
                    }
                    M.ReadToXmlElementTag(r, "global", "part", "score-audio", "mnx");
                }

                NumberOfMeasures = Global.GlobalMeasures.Count;

                AdjustForGraceNotes();

                M.Assert(r.Name == "mnx"); // end of "mnx"

                M.Assert(Global != null);
                M.Assert(Parts.Count > 0);
                M.Assert(ScoreAudios.Count >= 0);
            }
        }

        /// <summary>
        /// Returns partIndex, measureIndex, sequenceIndex, eventIndex
        /// </summary>
        //public Tuple<int, int, int, int> EventAddress(string eventID)
        public (int partIndex, int measureIndex, int sequenceIndex, int eventIndex) EventAddress(string eventID)
        {
            var partIndex = -1;
            var measureIndex = -1;
            var sequenceIndex = -1;
            var eventIndex = -1;
            var found = false;

            for(var pIndex = 0; pIndex < Parts.Count; ++pIndex)
            {
                var part = Parts[pIndex];
                for(var mIndex = 0; mIndex < part.Measures.Count; ++mIndex)
                {
                    var measure = part.Measures[mIndex];
                    for(var sIndex = 0; sIndex < measure.Sequences.Count; ++sIndex)
                    {
                        var sequence = measure.Sequences[sIndex];
                        for(var eIndex = 0; eIndex < sequence.Events.Count; ++eIndex)
                        {
                            var evt = sequence.Events[eIndex];
                            if(eventID.Equals(evt.ID))
                            {
                                partIndex = pIndex;
                                measureIndex = mIndex;
                                sequenceIndex = sIndex;
                                eventIndex = eIndex;
                                break;                            
                            }
                        }
                        if(found) break;
                    }
                    if(found) break;
                }
                if(found) break;
            }

            return (partIndex, measureIndex, sequenceIndex, eventIndex);
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


        public override string ToString()
        {
            return FileName;
        }
    }
}

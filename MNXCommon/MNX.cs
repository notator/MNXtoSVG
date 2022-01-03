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
                        for(var eIndex = 0; eIndex < sequence.EventsGracesAndForwards.Count; ++eIndex)
                        {
                            var evt = sequence.EventsGracesAndForwards[eIndex];
                            if(evt is Event e && eventID.Equals(e.ID)) // Forward objects have no ID, and are ignored here.
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

        /// <summary>
        /// Grace objects currently have their correct ticksPosInScore, but ticksDuration = 0.
        /// </summary>
        private void AdjustForGraceNotes()
        {
            if(SetAllTicksDurationsForMakeTimeGraces())
            {
                ResetTicksPositionsinScore();
            }
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

        private void ResetTicksPositionsinScore()
        {
            foreach(Part part in Parts)
            {
                foreach(var measure in part.Measures)
                {
                    foreach(var sequence in measure.Sequences)
                    {
                        int ticksPosInScore = 0;
                        foreach(var component in sequence.Components)
                        {
                            if(component is IHasTicks t)
                            {
                                t.TicksPosInScore = ticksPosInScore;
                                ticksPosInScore += t.TicksDuration;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns false if there are no MakeTime graces in the file, otherwise true.
        /// </summary>
        /// <returns></returns>
        private bool SetAllTicksDurationsForMakeTimeGraces()
        {
            bool durationsHaveChanged = false;
            SortedDictionary<int, List<Grace>> ticksPosMakeTimeGraces = new SortedDictionary<int, List<Grace>>();
            foreach(Part part in Parts)
            {
                foreach(var measure in part.Measures)
                {
                    foreach(var sequence in measure.Sequences)
                    {
                        foreach(var component in sequence.Components)
                        {
                            if(component is Grace g && g.Type == GraceType.makeTime)
                            {
                                int ticksPos = g.TicksPosInScore;
                                if(ticksPosMakeTimeGraces.ContainsKey(ticksPos) == false)
                                {
                                    ticksPosMakeTimeGraces[ticksPos] = new List<Grace>();
                                }
                                ticksPosMakeTimeGraces[ticksPos].Add(g);  
                            }
                        }
                    }
                }
            }

            if(ticksPosMakeTimeGraces.Count > 0)
            {
                SortedDictionary<int, int> ticksPosTicksToAdd = new SortedDictionary<int, int>();
                foreach(var tickPos in ticksPosMakeTimeGraces.Keys)
                {
                    int maxTicksDuration = 0;
                    foreach(var grace in ticksPosMakeTimeGraces[tickPos])
                    {
                        //maxTicksDuration = (maxTicksDuration > grace.DefaultDuration) ? maxTicksDuration : grace.DefaultDuration;
                    }
                    foreach(var grace in ticksPosMakeTimeGraces[tickPos])
                    {
                        grace.TicksDuration = maxTicksDuration;
                    }
                    ticksPosTicksToAdd.Add(tickPos, maxTicksDuration);
                }

                foreach(var tickPos in ticksPosTicksToAdd.Keys)
                {
                    int ticksToAdd = ticksPosTicksToAdd[tickPos];

                    List<IHasTicks> objectsToStretch = new List<IHasTicks>();
                    foreach(Part part in Parts)
                    {
                        foreach(var measure in part.Measures)
                        {
                            foreach(var sequence in measure.Sequences)
                            {
                                foreach(var component in sequence.Components)
                                {
                                    if(component is IHasTicks hasSettableTickDuration)
                                    {
                                        objectsToStretch.Add(hasSettableTickDuration);
                                    }
                                }
                            }
                        }
                    }
                    foreach(var objectToStretch in objectsToStretch)
                    {
                        objectToStretch.TicksDuration += ticksToAdd;
                    }
                }

                durationsHaveChanged = true;
            }
            return durationsHaveChanged;
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

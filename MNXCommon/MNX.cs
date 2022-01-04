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

                AdjustForGraceNotes(); //M.Assert(value >= M.MinimumEventTicks);

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
        /// This function sets Grace.TickDuration values, adjusting Event and Forward TickDuration
        /// values accordingly.
        /// It then sets TickPosInScore for all Event and Forward objects, including those inside Grace and TupletDef objects.
        /// <para>Checked Preconditions: All Objects currently have TicksPosInScore=0.
        /// Grace objects currently have TickDuration=0.
        /// Event and Forward objects have TickDurations that sum to
        /// each Measure's DefaultTickDuration (as defined by its CurrentTimeSignature).
        /// </para>
        /// </summary>
        private void AdjustForGraceNotes()
        {
            AssertPreconditions();

            //SetAllTicksDurationsForMakeTimeGraces();
            
            //for(var partIndex = 0; partIndex < Parts.Count; partIndex++)
            //{
            //    List<Measure> measures = Parts[partIndex].Measures;
            //    for(var measureIndex = 0; measureIndex < measures.Count; measureIndex++)
            //    {
            //        Measure measure = measures[measureIndex];
            //        measure.AdjustForGraceNotes();
            //    }
            //}

            SetTicksPositionsInScore();
        }

        /// All Objects currently have TicksPosInScore=0.
        /// Grace objects currently have TickDuration=0.
        /// Event and Forward objects have TickDurations that sum to
        /// each Measure's TicksDuration (as defined by its CurrentTimeSignature).
        private void AssertPreconditions()
        {
            foreach(Part part in Parts)
            {
                for(int j = 0; j < part.Measures.Count; j++)
                {
                    var measure = part.Measures[j];
                    var measureTicksDuration = Global.GlobalMeasures[j].TicksDuration;
                    for(int i = 0; i < measure.Sequences.Count; i++)
                    {
                        var sequence = measure.Sequences[i];
                        int sequenceTicksSum = 0;
                        foreach(var component in sequence.Components)
                        {
                            if(component is IHasTicks iHasTicks)
                            {
                                M.Assert(iHasTicks.TicksPosInScore == 0);
                                
                                if(iHasTicks is Grace g)
                                {
                                    var efs = g.EventsGracesAndForwards;
                                    foreach(var ef in efs)
                                    {
                                        M.Assert(! (ef is Grace));
                                        M.Assert(ef.TicksPosInScore == 0);
                                        M.Assert(ef.TicksDuration == 0);
                                    }
                                }
                                else if(iHasTicks is TupletDef td)
                                {
                                    var efs = td.EventsGracesAndForwards;
                                    foreach(var ef in efs)
                                    {
                                        M.Assert(ef.TicksPosInScore == 0);
                                        sequenceTicksSum += ef.TicksDuration;
                                    }
                                }
                                else
                                {
                                    sequenceTicksSum += iHasTicks.TicksDuration;
                                }
                            }
                        }
                        M.Assert(measureTicksDuration == sequenceTicksSum);
                    }
                }
            }
        }

        private void SetTicksPositionsInScore()
        {
            foreach(Part part in Parts)
            {
                for(int j = 0; j < part.Measures.Count; j++)
                {
                    int ticksPosInScore = 0;
                    var measure = part.Measures[j];
                    var measureTicksDuration = Global.GlobalMeasures[j].TicksDuration;
                    foreach(var sequence in measure.Sequences)
                    {
                        var seqTicksDuration = 0;
                        foreach(var component in sequence.Components)
                        {
                            if(component is IHasTicks iHasTicks)
                            {
                                M.Assert(iHasTicks.TicksPosInScore == 0);

                                if(iHasTicks is Grace g)
                                {
                                    var efs = g.EventsGracesAndForwards;
                                    foreach(var ef in efs)
                                    {
                                        ef.TicksPosInScore = ticksPosInScore;
                                        ticksPosInScore += ef.TicksDuration;
                                        seqTicksDuration += ef.TicksDuration;
                                    }
                                }
                                else if(iHasTicks is TupletDef td)
                                {
                                    var efs = td.EventsGracesAndForwards;
                                    foreach(var ef in efs)
                                    {
                                        ef.TicksPosInScore = ticksPosInScore;
                                        ticksPosInScore += ef.TicksDuration;
                                        seqTicksDuration += ef.TicksDuration;
                                    }
                                }
                                else
                                {
                                    iHasTicks.TicksPosInScore = ticksPosInScore;
                                    ticksPosInScore += iHasTicks.TicksDuration;
                                    seqTicksDuration += iHasTicks.TicksDuration;
                                }
                            }
                        }
                        M.Assert(measureTicksDuration == seqTicksDuration);
                    }
                }
            }
        }

        private void SetAllTicksDurationsForMakeTimeGraces()
        {
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

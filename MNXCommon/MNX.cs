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

                AdjustTicksDurationsForGraceNotes();

                SetIEventTicksPositionsInScore();

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
                        for(var eIndex = 0; eIndex < sequence.IEventsAndGraces.Count; ++eIndex)
                        {
                            var evt = sequence.IEventsAndGraces[eIndex];
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
        /// This function sets Grace.TicksDuration values,
        /// adjusting Event, Forward, and GlobalMeasure TicksDuration values accordingly.
        /// <para>Checked Preconditions: All IEvents currently have TicksPosInScore=0.
        /// Grace objects currently have TickDuration=0.
        /// Event and Forward objects have TickDurations that sum to each Measure's
        /// DefaultTickDuration (as defined by its CurrentTimeSignature).
        /// </para>
        /// </summary>
        private void AdjustTicksDurationsForGraceNotes()
        {
            AssertPreconditions();

            var tickPosMakeTimeGraces = GetTickPosGraces(GraceType.makeTime);

            if(tickPosMakeTimeGraces.Count > 0)
            {
                SetAllTicksDurationsForMakeTimeGraces(tickPosMakeTimeGraces);
            }

            var tickPosStealingGraces = GetTickPosGraces(GraceType.stealPrevious);
            var tickPosStealFollowingGraces = GetTickPosGraces(GraceType.stealFollowing);
            foreach(var key in tickPosStealFollowingGraces.Keys)
            {
                if(tickPosStealingGraces.ContainsKey(key) == false)
                {
                    tickPosStealingGraces.Add(key, new List<Grace>());
                }

                var graceList = tickPosStealFollowingGraces[key];
                foreach(var grace in graceList)
                {
                    tickPosStealingGraces[key].Add(grace);
                }
            }

            if(tickPosStealingGraces.Count > 0)
            {
                SetTicksDurationsForStealingGraces(tickPosStealingGraces);
            }

            for(int i = 0; i < this.NumberOfMeasures; i++)
            {
                int previousTicksDuration = 0;
                foreach(var part in this.Parts)
                {
                    var sequences = part.Measures[i].Sequences;
                    previousTicksDuration = sequences[0].TicksDuration; // sums the TicksDurations of the content
                    for(int j = 1; j < sequences.Count; j++)
                    {
                        var thisTicksDuration = sequences[j].TicksDuration; // sums the TicksDurations of the content
                        M.Assert(thisTicksDuration == previousTicksDuration);
                        previousTicksDuration = thisTicksDuration;
                    }
                }
                Global.GlobalMeasures[i].TicksDuration = previousTicksDuration;
            }
        }

        /// All IEvents and Graces currently have TicksPosInScore=0.
        /// Grace objects currently have TickDuration=0.
        /// Event and Forward (=IEvent) objects have TickDurations that sum to their Measure's TicksDuration.
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
                            if(component is IHasTicksDuration iHasTicksDuration)
                            {
                                if(iHasTicksDuration is Grace g)
                                {
                                    var efs = g.IEventsAndGraces;
                                    foreach(var ef in efs)
                                    {
                                        M.Assert(!(ef is Grace));
                                        M.Assert(ef.TicksDuration == 0);
                                    }
                                }
                                else if(iHasTicksDuration is TupletDef td)
                                {
                                    var efs = td.IEventsAndGraces;
                                    foreach(var ef in efs)
                                    {
                                        sequenceTicksSum += ef.TicksDuration;
                                    }
                                }
                                else
                                {
                                    sequenceTicksSum += iHasTicksDuration.TicksDuration;
                                }
                            }
                        }
                        M.Assert(measureTicksDuration == sequenceTicksSum);
                    }
                }
            }
        }

        private SortedDictionary<int, List<Grace>> GetTickPosGraces(GraceType getGraceType)
        {
            SortedDictionary<int, List<Grace>> ticksPosGraces= new SortedDictionary<int, List<Grace>>();
            int nMeasures = Parts[0].Measures.Count;
            foreach(Part part in Parts)
            {
                int ticksPosInScore = 0;
                for(int measureIndex = 0; measureIndex < nMeasures; measureIndex++)
                {
                    var measure = part.Measures[measureIndex];

                    foreach(var sequence in measure.Sequences)
                    {
                        var iEventsAndGraces = sequence.IEventsAndGraces;
                        foreach(var iEventOrGrace in iEventsAndGraces)
                        {
                            if(iEventOrGrace is IEvent iEvent)
                            {
                                ticksPosInScore += iEvent.TicksDuration;
                            }
                            else if(iEventOrGrace is Grace g && g.Type == getGraceType)
                            {
                                if(ticksPosGraces.ContainsKey(ticksPosInScore) == false)
                                {
                                    ticksPosGraces[ticksPosInScore] = new List<Grace>();
                                }
                                ticksPosGraces[ticksPosInScore].Add(g);
                            }
                        }
                    }
                }
            }

            return ticksPosGraces;
        }

        /// <summary>
        /// Sets the TickDuration of the make-time Grace objects, and increments
        /// the TickDurations of IEvent and GlobalMeasure objects accordingly.
        /// Sequence
        /// </summary>
        private void SetAllTicksDurationsForMakeTimeGraces(SortedDictionary<int, List<Grace>> tickPositionMakeTimeGraces)
        {
            #region Assert Graces are all GraceType.makeTime
            M.Assert(tickPositionMakeTimeGraces.Count > 0);
            foreach(var key in tickPositionMakeTimeGraces.Keys)
            {
                var graces = tickPositionMakeTimeGraces[key];
                foreach(var grace in graces)
                {
                    M.Assert(grace.Type == GraceType.makeTime);
                }
            }
            #endregion

            SortedDictionary<int, int> ticksPosTicksToAdd = new SortedDictionary<int, int>();
            #region get ticksPosTicksToAdd
            foreach(var tickPos in tickPositionMakeTimeGraces.Keys)
            {
                int maxTicksDuration = 0;
                foreach(var makeTimeGrace in tickPositionMakeTimeGraces[tickPos])
                {
                    M.Assert(makeTimeGrace.TicksDuration == 0);
                    int defaultMakeTimeGraceDuration = makeTimeGrace.GetDefaultMakeTimeGraceTicksDuration();
                    maxTicksDuration = (maxTicksDuration > defaultMakeTimeGraceDuration) ? maxTicksDuration : defaultMakeTimeGraceDuration;
                }
                ticksPosTicksToAdd.Add(tickPos, maxTicksDuration);
            }
            #endregion

            int nMeasures = Parts[0].Measures.Count;
            List<int> addTickPositions = new List<int>();
            foreach(var tickPosAtWhichToAdd in ticksPosTicksToAdd.Keys)
            {
                addTickPositions.Add(tickPosAtWhichToAdd);
            }
            int maxTickPosAtWhichToAdd = addTickPositions[addTickPositions.Count - 1];

            #region  
            foreach(Part part in Parts)
            {
                int ticksPosInScore = 0;
                int addTicksIndex = 0;
                int addTickPosition = addTickPositions[addTicksIndex++];
                int ticksToAdd = ticksPosTicksToAdd[addTickPosition];

                for(int measureIndex = 0; measureIndex < nMeasures; measureIndex++)
                {
                    if(ticksPosInScore > maxTickPosAtWhichToAdd)
                    {
                        break;
                    }
                    var measure = part.Measures[measureIndex];
                    int measureTicksPos = ticksPosInScore;
                    int nSequences = measure.Sequences.Count;

                    for(int sequenceIndex = 0; sequenceIndex < nSequences; sequenceIndex++)
                    {
                        var sequence = measure.Sequences[sequenceIndex];
                        int localTicksPosInScore = measureTicksPos;
                        int localAdditions = 0;
                        var iEventsAndGraces = sequence.IEventsAndGraces;

                        foreach(var iEventOrGrace in iEventsAndGraces)
                        {
                            int oldTicksDuration = iEventOrGrace.TicksDuration;
                            bool incrementAddTickPosition = false;

                            if(iEventOrGrace is Grace g && g.Type == GraceType.makeTime)
                            {
                                M.Assert(g.TicksDuration == 0);
                                if(localTicksPosInScore == addTickPosition)
                                {
                                    g.TicksDuration = ticksToAdd;
                                    localAdditions += ticksToAdd;
                                    incrementAddTickPosition = true;
                                }
                            }
                            else if(iEventOrGrace is IEvent ie)
                            {
                                if(localTicksPosInScore >= addTickPosition && addTickPosition < (localTicksPosInScore + ie.TicksDuration))
                                {
                                    ie.TicksDuration += ticksToAdd;
                                    localAdditions += ticksToAdd;
                                    incrementAddTickPosition = true;
                                }
                            }

                            if(incrementAddTickPosition)
                            {
                                if(addTicksIndex < addTickPositions.Count - 1)
                                {
                                    addTickPosition = addTickPositions[addTicksIndex++];
                                    ticksToAdd = ticksPosTicksToAdd[addTickPosition];
                                }
                                else
                                {
                                    addTickPosition = int.MaxValue;
                                    ticksToAdd = 0;
                                }
                            }

                            localTicksPosInScore += oldTicksDuration;
                        }

                        int measureTicksDuration = localTicksPosInScore - measureTicksPos + localAdditions;
                        if(sequenceIndex == 0)
                        {
                            Global.GlobalMeasures[measureIndex].TicksDuration = measureTicksDuration;
                        }
                        else
                        {
                            M.Assert(Global.GlobalMeasures[measureIndex].TicksDuration == measureTicksDuration);
                        }

                        if(sequenceIndex == nSequences - 1)
                        {

                            ticksPosInScore = localTicksPosInScore;
                        }
                    }
                }
            }
            #endregion

        }
        private void SetTicksDurationsForStealingGraces(SortedDictionary<int, List<Grace>> tickPosStealingGraces)
        {
            void SetGraceDurationAndStealFrom(Grace grace, IEvent victim)
            {
                M.Assert(victim != null);
                int toSteal = (int)(victim.TicksDuration * M.GraceStealProportion);
                M.Assert(toSteal >= M.MinimumEventTicks);
                grace.TicksDuration = toSteal;
                victim.TicksDuration -= toSteal;
                M.Assert(victim.TicksDuration >= M.MinimumEventTicks);
            }

            foreach(Part part in Parts)
            {
                foreach(Measure measure in part.Measures)
                {
                    foreach(Sequence sequence in measure.Sequences)
                    {
                        var iEventsAndGraces = sequence.IEventsAndGraces;
                        for(int i = 0; i < iEventsAndGraces.Count; i++)
                        {
                            IEvent previous = null;
                            IEvent following = null;
                            if(iEventsAndGraces[i] is Grace grace && grace.Type != GraceType.makeTime)
                            {
                                if(grace.Type == GraceType.stealPrevious)
                                {
                                    if(i > 0)
                                    {
                                        previous = iEventsAndGraces[i - 1] as IEvent;
                                    }
                                    SetGraceDurationAndStealFrom(grace, previous);
                                }
                                else if(grace.Type == GraceType.stealFollowing)
                                {
                                    if(i < iEventsAndGraces.Count - 1)
                                    {
                                        following = iEventsAndGraces[i + 1] as IEvent;
                                    }
                                    SetGraceDurationAndStealFrom(grace, following);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the TickPositionInScore for all IEvents in the score.
        /// The IEvents are Event and Forward objects contained at the lowest level in (nested) TupletDefs and Grace objects.
        /// Grace objects do not nest, and only contain a flat list of Event objects. They don't contain Forward objects.
        /// </summary>
        private void SetIEventTicksPositionsInScore()
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
                            if(component is IHasTicksDuration iHasTicksDuration)
                            {
                                if(iHasTicksDuration is Grace g)
                                {
                                    var efs = g.IEventsAndGraces;
                                    foreach(var ef in efs)
                                    {
                                        if(ef is IEvent iEvent)
                                        {
                                            iEvent.TicksPosInScore = ticksPosInScore;
                                            ticksPosInScore += iEvent.TicksDuration;
                                            seqTicksDuration += iEvent.TicksDuration;
                                        }
                                    }
                                }
                                else if(iHasTicksDuration is TupletDef td)
                                {
                                    var efs = td.IEventsAndGraces; // copes with nesting TupletDefs.
                                    foreach(var ef in efs)
                                    {
                                        if(ef is IEvent iEvent)
                                        {
                                            iEvent.TicksPosInScore = ticksPosInScore;
                                            ticksPosInScore += iEvent.TicksDuration;
                                            seqTicksDuration += iEvent.TicksDuration;
                                        }
                                        else if(ef is Grace grace)
                                        {
                                            var gefgs = grace.IEventsAndGraces; // Grace objects don't nest, so only contain IEvents
                                            foreach(var gefg in gefgs)
                                            {
                                                if(gefg is IEvent ie)
                                                {
                                                    ie.TicksPosInScore = ticksPosInScore;
                                                    ticksPosInScore += ie.TicksDuration;
                                                    seqTicksDuration += ie.TicksDuration;
                                                }
                                            }                                           
                                        }
                                    }
                                }
                                else
                                {
                                    if(iHasTicksDuration is IEvent iEvent)
                                    {
                                        iEvent.TicksPosInScore = ticksPosInScore;
                                        ticksPosInScore += iEvent.TicksDuration;
                                        seqTicksDuration += iEvent.TicksDuration;
                                    }
                                }
                            }
                        }
                        M.Assert(measureTicksDuration == seqTicksDuration);
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

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

            AdjustPartsForGraceNotes(Parts);

            A.Assert(r.Name == "mnx-common"); // end of "mnx-common"

            A.Assert(Globals.Count > 0);
            A.Assert(Parts.Count > 0);
            A.Assert(ScoreAudios.Count >= 0);
        }

        private void AdjustPartsForGraceNotes(List<Part> parts)
        {
            for(var partIndex = 0; partIndex < parts.Count; partIndex++)
            {
                List<Measure> measures = parts[partIndex].Measures;
                for(var measureIndex = 0; measureIndex < measures.Count; measureIndex++)
                {
                    List<Sequence> sequences = measures[measureIndex].Sequences;
                    for(var sequenceIndex = 0; sequenceIndex < sequences.Count; sequenceIndex++)
                    {
                        List<ISequenceComponent> seqComponents = sequences[sequenceIndex].Seq;
                        for(var seqComponentIndex = 0; seqComponentIndex < seqComponents.Count; seqComponentIndex++)
                        {
                            if(seqComponents[seqComponentIndex] is Grace grace)
                            {
                                int stealableTicks = 0;
                                switch(grace.Type)
                                {
                                    case GraceType.stealPrevious:
                                        Event previousEvent = FindPreviousEvent(partIndex, measureIndex, sequenceIndex, seqComponentIndex);
                                        if(previousEvent == null)
                                        {
                                            A.ThrowError("Can't steal ticks from another grace or before the beginning of a part.");
                                        }
                                        stealableTicks = (previousEvent.Duration.Ticks + B.MinimumEventTicks);
                                        if(grace.Ticks > stealableTicks)
                                        {
                                            grace.Ticks = stealableTicks; 
                                        }
                                        previousEvent.Duration.Ticks -= grace.Ticks;
                                        break;
                                    case GraceType.stealFollowing:
                                        Event nextEvent = FindNextEvent(partIndex, measureIndex, sequenceIndex, seqComponentIndex);
                                        if(nextEvent == null)
                                        {
                                            A.ThrowError("Can't steal ticks from another grace or after the end of a part.");
                                        }
                                        stealableTicks = (nextEvent.Duration.Ticks + B.MinimumEventTicks);
                                        if(grace.Ticks > stealableTicks)
                                        {
                                            grace.Ticks = stealableTicks;
                                        }
                                        nextEvent.Duration.Ticks -= grace.Ticks;
                                        break;
                                    case GraceType.makeTime:
                                        MakeTime(grace.Ticks, partIndex, measureIndex, sequenceIndex, seqComponentIndex);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Event FindPreviousEvent(int partIndex, int measureIndex, int sequenceIndex, int seqComponentIndex)
        {
            List<ISequenceComponent> seq = Parts[partIndex].Measures[measureIndex].Sequences[sequenceIndex].Seq;
            int returnEventIndex = seqComponentIndex - 1;
            if(returnEventIndex < 0)
            {
                seq = GetPreviousSeq(partIndex, measureIndex, sequenceIndex);
                if(seq == null)
                {
                    return null;
                }
                returnEventIndex = seq.Count - 1;
            }

            Event returnEvent = null;
            ISequenceComponent previousSeqComponent = seq[returnEventIndex];
            while(!(previousSeqComponent is ITicks iTicks))
            {
                returnEventIndex--;
                if(returnEventIndex < 0)
                {
                    seq = GetPreviousSeq(partIndex, measureIndex, sequenceIndex);
                    if(seq == null)
                    {
                        return null;
                    }
                    returnEventIndex = seq.Count - 1;
                }
            }

            if(seq[returnEventIndex] is Tuplet)
            {
                Tuplet tuplet = seq[returnEventIndex] as Tuplet;
                while(tuplet.Seq[tuplet.Seq.Count - 1] is Tuplet)
                {
                    // nested tuplet
                    tuplet = tuplet.Seq[tuplet.Seq.Count - 1] as Tuplet;
                }
                int tupletComponentIndex = tuplet.Seq.Count - 1;
                while(!(tuplet.Seq[tupletComponentIndex] is ITicks))
                {
                    tupletComponentIndex--;
                }
                returnEvent = tuplet.Seq[tupletComponentIndex] as Event; // will be null if Grace
            }
            else
            {
                returnEvent = seq[returnEventIndex] as Event; // will be null if Grace
            }

            return returnEvent;
        }

        private List<ISequenceComponent> GetPreviousSeq(int partIndex, int measureIndex, int sequenceIndex)
        {
            measureIndex--;
            if(measureIndex < 0)
            {
                return null;
            }
            return Parts[partIndex].Measures[measureIndex].Sequences[sequenceIndex].Seq;
        }

        private Event FindNextEvent(int partIndex, int measureIndex, int sequenceIndex, int seqComponentIndex)
        {
            throw new NotImplementedException();
        }

        private List<ISequenceComponent> GetNextSeq(int partIndex, int measureIndex, int sequenceIndex)
        {
            measureIndex++;
            if(measureIndex >= Parts[partIndex].Measures.Count)
            {
                return null;
            }
            return Parts[partIndex].Measures[measureIndex].Sequences[sequenceIndex].Seq;
        }

        private void MakeTime(int ticks, int partIndex, int measureIndex, int sequenceIndex, int seqComponentIndex)
        {
            throw new NotImplementedException();
        }
    }
}
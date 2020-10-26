using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public class Measure : IHasTicks
    {
        /// <summary>
        /// If null, this value should be set when the whole score has been read
        /// see https://w3c.github.io/mnx/specification/common/#the-measure-element
        /// </summary>
        public int? Number = null;        

        /// <summary>
        /// Contrary to https://w3c.github.io/mnx/specification/common/#the-measure-element
        /// this (0-based) attribute is always set by the constructor.
        /// </summary>
        public readonly int Index = -1;
        public int TicksPosInScore
        {
            get
            {
                if(_ticksPosInScore == -1)
                {
                    throw new ApplicationException("A Global Measure has no TicksPosInScore");
                }
                else
                {
                    return _ticksPosInScore;
                }
            }
        }
        private readonly int _ticksPosInScore;

        public override string ToString() => $"Measure: Index={Index} TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration}";

        public readonly BarlineType? Barline = null; // default

        public readonly Directions Directions = null;
        public readonly List<Sequence> Sequences = new List<Sequence>();

        public int TicksDuration
        {
            get
            {
                int ticks = Sequences[0].TicksDuration;
                for(var i = 1; i < Sequences.Count; i++)
                {
                    M.Assert(Sequences[i].TicksDuration == ticks);
                }
                return ticks;
            }
        }

        public Measure(XmlReader r, int measureIndex, int ticksPosInScore, bool isGlobal)
        {
            M.Assert(r.Name == "measure");
            // https://w3c.github.io/mnx/specification/common/#the-measure-element

            if(r.IsEmptyElement)
            {
                if(isGlobal)
                {
                    return;
                }
                else
                {
                    M.ThrowError("Empty measure in part.");
                }
            }

            Index = measureIndex; // ji: 23.06.2020
            _ticksPosInScore = ticksPosInScore; // ji: 23.06.2020

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    // The optional MNX-Common "index" attribute is always ignored.
                    //case "index":
                    //    Index = Int32.Parse(r.Value);
                    //    M.Assert(Index > 0);
                    //    break;
                    case "number":
                        Number = Int32.Parse(r.Value);
                        M.Assert(Number > 0);
                        break;
                    case "barline":
                        Barline = GetBarlineType(r.Value);
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            M.ReadToXmlElementTag(r, "directions", "sequence");

            int sequenceIndex = 0;

            while(r.Name == "directions" || r.Name == "sequence")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "directions":
                            Directions = new Directions(r, ticksPosInScore, isGlobal);
                            break;
                        case "sequence":
                            if(isGlobal)
                            {
                                M.ThrowError("Error in input file.");
                            }
                            Sequence sequence = new Sequence(r, measureIndex, ticksPosInScore, sequenceIndex++, isGlobal);
                            Sequences.Add(sequence);
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "directions", "sequence", "measure");
            }
            M.Assert(r.Name == "measure"); // end of measure
        }

        private BarlineType GetBarlineType(string value)
        {
            BarlineType rval = BarlineType.regular;
            switch(value)
            {
                case "regular":
                    rval = BarlineType.regular;
                    break;
                case "dotted":
                    rval = BarlineType.dotted;
                    break;
                case "dashed":
                    rval = BarlineType.dashed;
                    break;
                case "heavy":
                    rval = BarlineType.heavy;
                    break;
                case "light-light":
                    rval = BarlineType.lightLight;
                    break;
                case "light-heavy":
                    rval = BarlineType.lightHeavy;
                    break;
                case "heavy-light":
                    rval = BarlineType.heavyLight;
                    break;
                case "heavy-heavy":
                    rval = BarlineType.heavyHeavy;
                    break;
                case "tick":
                    rval = BarlineType.tick;
                    break;
                case "short":
                    rval = BarlineType._short;
                    break;
                case "none":
                    rval = BarlineType.none;
                    break;
                default:
                    M.ThrowError("Error: unknown barline type");
                    break;
            }
            return rval;
        }

        public void AdjustForGraceNotes()
        {
            for(var sequenceIndex = 0; sequenceIndex < Sequences.Count; sequenceIndex++)
            {
                List<IHasTicks> eventsAndEventGroups = Sequences[sequenceIndex].EventsAndEventGroups;
                for(var eegIndex = 0; eegIndex < eventsAndEventGroups.Count; eegIndex++)
                {
                    if(eventsAndEventGroups[eegIndex] is Grace grace)
                    {
                        int graceIndex = eegIndex;
                        int stealableTicks = 0;
                        switch(grace.Type)
                        {
                            case GraceType.stealPrevious:
                                Event previousEvent = FindPreviousEvent(eventsAndEventGroups, graceIndex);
                                stealableTicks = (previousEvent.TicksDuration - M.MinimumEventTicks);
                                if(grace.TicksDuration > stealableTicks)
                                {
                                    grace.TicksDuration = stealableTicks;
                                }
                                previousEvent.TicksDuration -= grace.TicksDuration;
                                break;
                            case GraceType.stealFollowing:
                                Event nextEvent = FindNextEvent(eventsAndEventGroups, graceIndex);
                                stealableTicks = (nextEvent.TicksDuration - M.MinimumEventTicks);
                                if(grace.TicksDuration > stealableTicks)
                                {
                                    grace.TicksDuration = stealableTicks;
                                }
                                nextEvent.TicksDuration -= grace.TicksDuration;
                                break;
                            case GraceType.makeTime:
                                MakeTime(eventsAndEventGroups, grace);
                                break;
                        }
                    }
                }
            }
        }            

        private static Event FindPreviousEvent(List<IHasTicks> eventsAndEventGroups, int graceIndex)
        {
            if(graceIndex == 0)
            {
                M.ThrowError("Can't steal ticks from the previous measure.");
            }
            IHasTicks previousObject = eventsAndEventGroups[graceIndex - 1];
            if(previousObject is Grace)
            {
                M.ThrowError("Can't steal ticks from a Grace.");
            }
            Event previousEvent = null;
            if(previousObject is EventGroup eg)
            {
                List<Event> events = eg.Events;
                previousEvent = events[events.Count - 1];
            }
            else
            {
                previousEvent = previousObject as Event;
            }

            return previousEvent;
        }

        private static Event FindNextEvent(List<IHasTicks> eventsAndEventGroups, int graceIndex)
        {
            if(graceIndex == (eventsAndEventGroups.Count - 1))
            {
                M.ThrowError("Can't steal ticks from the next measure.");
            }
            IHasTicks nextObject = eventsAndEventGroups[graceIndex + 1];
            if(nextObject is Grace)
            {
                M.ThrowError("Can't steal ticks from a Grace.");
            }
            Event nextEvent = null;
            if(nextObject is EventGroup eg)
            {
                List<Event> events = eg.Events;
                nextEvent = events[0];
            }
            else
            {
                nextEvent = nextObject as Event;
            }

            return nextEvent;
        }

        private void MakeTime(List<IHasTicks> eventsAndEventGroups, Grace grace)
        {
            int graceTicksPostion = 0;
            foreach(var obj in eventsAndEventGroups)
            {
                if(obj == grace)
                {
                    break;
                }
                graceTicksPostion += obj.TicksDuration;
            }
            foreach(var sequence in this.Sequences)
            {
                int ticksPos = 0;
                List<IHasTicks> eegs = sequence.EventsAndEventGroups;
                int eegIndex = 0;
                int insertTicksPos = 0;
                for(var index = 0; index < eegs.Count; index++)
                {
                    if(ticksPos >= graceTicksPostion)
                    {
                        eegIndex = index;
                        break;
                    }
                    insertTicksPos = ticksPos;
                    ticksPos += eegs[index].TicksDuration;              
                }
                var eeg = eegs[eegIndex];
                if(eeg is EventGroup eg)
                {
                    List<Event> events = eg.Events;
                    for(var i = 0; i < events.Count; i++)
                    {
                        if(insertTicksPos >= graceTicksPostion || i == (events.Count - 1))
                        {
                            events[i].TicksDuration += grace.TicksDuration;
                            break;
                        }
                    }
                }
                else ((Event)eeg).TicksDuration += grace.TicksDuration;                
            }
        }
    }
}
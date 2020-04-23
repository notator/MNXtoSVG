using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal class Measure : IHasTicks
    {
        /// <summary>
        /// If null, this value should be set when the whole score has been read
        /// see https://w3c.github.io/mnx/specification/common/#the-measure-element
        /// </summary>
        public int? Number = null;
        /// <summary>
        /// If null, this value should be set when the whole score has been read
        /// see https://w3c.github.io/mnx/specification/common/#the-measure-element
        /// </summary>
        public int? Index = null;
        public readonly BarlineType? Barline = null; // default

        public readonly Directions Directions = null;
        public readonly List<Sequence> Sequences = new List<Sequence>();

        public int Ticks
        {
            get
            {
                int ticks = Sequences[0].Ticks;
                for(var i = 1; i < Sequences.Count; i++)
                {
                    M.Assert(Sequences[i].Ticks == ticks);
                }
                return ticks;
            }
        }

        public Measure(XmlReader r, bool isGlobal)
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

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "index":
                        Index = Int32.Parse(r.Value);
                        M.Assert(Index > 0);
                        break;
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

            while(r.Name == "directions" || r.Name == "sequence")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "directions":
                            Directions = new Directions(r, isGlobal);
                            break;
                        case "sequence":
                            if(isGlobal)
                            {
                                M.ThrowError("Error in input file.");
                            }
                            Sequences.Add(new Sequence(r, isGlobal));
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

        internal void AdjustForGraceNotes()
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
                                stealableTicks = (previousEvent.Ticks - B.MinimumEventTicks);
                                if(grace.Ticks > stealableTicks)
                                {
                                    grace.Ticks = stealableTicks;
                                }
                                previousEvent.Ticks -= grace.Ticks;
                                break;
                            case GraceType.stealFollowing:
                                Event nextEvent = FindNextEvent(eventsAndEventGroups, graceIndex);
                                stealableTicks = (nextEvent.Ticks - B.MinimumEventTicks);
                                if(grace.Ticks > stealableTicks)
                                {
                                    grace.Ticks = stealableTicks;
                                }
                                nextEvent.Ticks -= grace.Ticks;
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
                graceTicksPostion += obj.Ticks;
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
                    ticksPos += eegs[index].Ticks;              
                }
                var eeg = eegs[eegIndex];
                if(eeg is EventGroup eg)
                {
                    List<Event> events = eg.Events;
                    for(var i = 0; i < events.Count; i++)
                    {
                        if(insertTicksPos >= graceTicksPostion || i == (events.Count - 1))
                        {
                            events[i].Ticks += grace.Ticks;
                            break;
                        }
                    }
                }
                else ((Event)eeg).Ticks += grace.Ticks;                
            }
        }
    }
}
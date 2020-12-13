using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public class GlobalMeasure : IHasTicks
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
        public int Index { get; private set; } = -1;
        public int TicksDuration { get; private set; } = -1;
        public int TicksPosInScore { get; private set; } = -1;

        public override string ToString() => $"GlobalMeasure: Index={Index} TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration}";

        public readonly BarlineType? Barline = null; // default

        public readonly GlobalDirections GlobalDirections = null;

        public GlobalMeasure(XmlReader r, int measureIndex, TimeSignature currentTimeSig, int ticksPosInScore)
        {
            M.Assert(r.Name == "measure");
            // https://w3c.github.io/mnx/specification/common/#the-measure-element

            Index = measureIndex; // ji: 23.06.2020
            TicksDuration = (currentTimeSig != null) ? currentTimeSig.TicksDuration : -1; // overridden below if the time sig changes.
            TicksPosInScore = ticksPosInScore; // ji: 13.12.2020

            if(!r.IsEmptyElement)
            {
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

                M.ReadToXmlElementTag(r, "directions");

                while(r.Name == "directions")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "directions":
                                GlobalDirections = new GlobalDirections(r, currentTimeSig, ticksPosInScore);
                                currentTimeSig = (GlobalDirections.TimeSignature != null) ? GlobalDirections.TimeSignature : currentTimeSig;
                                TicksDuration = currentTimeSig.TicksDuration;
                                break;
                        }
                    }
                    M.ReadToXmlElementTag(r, "directions", "measure");
                }
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
    }
}
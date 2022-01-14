using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-grace-element
    /// </summary>
    public class Grace : EventGroup, IHasTicksDuration, ISequenceComponent
    {
        public readonly GraceType Type = GraceType.stealPrevious; // spec says this is the default.
        public readonly bool? Slash = null;
        public override string ToString() => $"Grace: TicksDuration={TicksDuration} TicksPosInScore={TicksPosInScore} Type={Type} MsPosInScore={MsPosInScore} MsDuration={MsDuration}";

        /// <summary>
        /// Grace and Event implement Ticks.set so that grace can steal.
        /// </summary>
        public override int TicksDuration
        {
            get
            {
                // Graces don't nest, and dont contain Forward objects
                return base.TicksDuration;
            }
            set
            {
                int outerTicks = value;

                // Graces don't nest, and dont contain Forward objects
                List<IHasTicksDuration> events = IEventsAndGraces;
                List<int> innerTicks = new List<int>();
                foreach(Event e in events)
                {
                    int ticks = e.MNXDurationSymbol.GetDefaultTicks();
                    innerTicks.Add(ticks);
                }

                List<int> newTicks = M.IntDivisionSizes(outerTicks, innerTicks);

                for(var i = 0; i < newTicks.Count; i++)
                {
                    int ticks = newTicks[i];
                    M.Assert(ticks >= M.MinimumEventTicks);
                    events[i].TicksDuration = ticks;
                }
            }
        }

        /// <summary>
        /// Grace is not an IEvent. This is just for debugging (intellisense) purposes.
        /// </summary>
        public int TicksPosInScore
        {
            get
            {
                Event e = IEventsAndGraces[0] as Event;
                return e.TicksPosInScore;
            }
        }

        internal int GetDefaultMakeTimeGraceTicksDuration()
        {
            M.Assert(this.Type == GraceType.makeTime);
            int rval = 0;
            // Graces don't nest, and dont contain Forward objects
            List<IHasTicksDuration> events = IEventsAndGraces;
            foreach(Event e in events)
            {
                int eventTickSize = (int)(e.MNXDurationSymbol.GetDefaultTicks() * M.MakeTimeGraceDefaultTickSize);
                M.Assert(eventTickSize >= M.MinimumEventTicks);
                rval += eventTickSize;
            }
            return rval;
        }

        public Grace(XmlReader r)
        {            
            M.Assert(r.Name == "grace");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "type":
                        Type = GetGraceType(r.Value);
                        break;
                    case "slash":
                        Slash = (r.Value == "yes");
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            M.ReadToXmlElementTag(r, "event");

            while(r.Name == "event")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "event":
                            Event e = new Event(r, true);
                            e.TicksDuration = 0; // Set correctly when the complete file has been parsed.
                            Components.Add(e);
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "event", "grace");
            }

            M.Assert(IEventsAndGraces.Count > 0);
            M.Assert(r.Name == "grace"); // end of grace
        }

        private GraceType GetGraceType(string value)
        {
            GraceType rval = GraceType.stealPrevious; // default (spec)
            switch(value)
            {
                case "steal-previous":
                    rval = GraceType.stealPrevious;
                    break;
                case "steal-following":
                    rval = GraceType.stealFollowing;
                    break;
                case "make-time":
                    rval = GraceType.makeTime;
                    break;
                default:
                    M.ThrowError("Error: unknown grace type.");
                    break;
            }
            return rval;
        }
    }
}
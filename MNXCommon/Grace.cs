using MNX.AGlobals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-grace-element
    /// </summary>
    public class Grace : ITicks, ISequenceComponent
    {
        public readonly GraceType Type = GraceType.stealPrevious; // spec says this is the default.
        public readonly bool? Slash = null;

        public readonly List<Event> Events;

        public int Ticks
        {
            get
            {
                int rval = 0;
                foreach(var e in Events)
                {
                    rval += e.Duration.Ticks;
                }
                return rval;
            }
            set
            {
                List<int> tickss = new List<int>();
                foreach(var e in Events)
                {
                    tickss.Add(e.Duration.Ticks);
                }
                List<int> newTickss = B.GetInnerTicks(value, tickss);

                for(var i = 0; i < newTickss.Count; i++)
                {
                    Events[i].Duration.Ticks = newTickss[i];
                }
            }
        }

        public Grace(XmlReader r)
        {            
            A.Assert(r.Name == "grace");

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

            Events = GetGraceContent(r);

            A.Assert(r.Name == "grace"); // end of grace

        }

        private List<Event> GetGraceContent(XmlReader r)
        {
            List<ISequenceComponent> seq = B.GetSequenceContent(r, "grace", false);
            List<Event> rval = new List<Event>();
            foreach(var item in seq)
            {
                if(item is Event e)
                {
                    int nTicks = e.Duration.Ticks / 3;
                    e.Duration.Ticks = (nTicks < B.MinimumEventTicks) ? B.MinimumEventTicks : nTicks;
                    // Grace.Ticks returns the sum of these Ticks.
                    rval.Add(e);
                }
            }
            return rval;
        }

        private GraceType GetGraceType(string value)
        {
            GraceType rval = GraceType.stealPrevious; // default (spec)
            switch(value)
            {
                case "steal-following":
                    rval = GraceType.stealFollowing;
                    break;
                case "make-time":
                    rval = GraceType.makeTime;
                    break;
                default:
                    A.ThrowError("Error: unknown grace type.");
                    break;
            }
            return rval;
        }
    }
}
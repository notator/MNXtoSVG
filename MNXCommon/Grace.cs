﻿using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-grace-element
    /// </summary>
    internal class Grace : EventGroup, IHasTicks, ISeqComponent
    {
        public readonly GraceType Type = GraceType.stealPrevious; // spec says this is the default.
        public readonly bool? Slash = null;

        /// <summary>
        /// Grace and Event implement Ticks.set so that grace can steal.
        /// </summary>
        public override int Ticks
        {
            get
            {
                return base.Ticks; // returns the sum of the inner ticks.
            }
            set
            {
                int outerTicks = value;
                List<Event> events = Events;
                List<int> innerTicks = new List<int>();
                foreach(var ev in Events)
                {
                    innerTicks.Add(ev.Ticks);
                }

                List<int> newTicks = M.IntDivisionSizes(outerTicks, innerTicks);

                for(var i = 0; i < newTicks.Count; i++)
                {
                    events[i].Ticks = newTicks[i];
                }
            }
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

            Seq = GetSequenceContent(r, "grace", false);

            SetDefaultTicks(Seq);

            M.Assert(r.Name == "grace"); // end of grace

        }

        private void SetDefaultTicks(List<ISeqComponent> seq)
        {
            var eventList = Events;
            foreach(var e in eventList)
            {
                int nTicks = e.DSymbol.DefaultTicks / 3;
                e.DSymbol.Ticks = (nTicks < B.MinimumEventTicks) ? B.MinimumEventTicks : nTicks;
            }
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
                    M.ThrowError("Error: unknown grace type.");
                    break;
            }
            return rval;
        }
    }
}
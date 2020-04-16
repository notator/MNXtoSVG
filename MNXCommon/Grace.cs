using MNX.AGlobals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-grace-element
    /// </summary>
    internal class Grace : EventGroup, ITicks, ISeqComponent
    {
        public readonly GraceType Type = GraceType.stealPrevious; // spec says this is the default.
        public readonly bool? Slash = null;

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

            Seq = B.GetSequenceContent(r, "grace", false);

            SetDefaultTicks(Seq);

            A.Assert(r.Name == "grace"); // end of grace

        }

        private void SetDefaultTicks(List<ISeqComponent> seq)
        {
            var eventList = EventList;
            foreach(var e in eventList)
            {
                int nTicks = e.Duration.Ticks / 3;
                e.Duration.Ticks = (nTicks < B.MinimumEventTicks) ? B.MinimumEventTicks : nTicks;
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
                    A.ThrowError("Error: unknown grace type.");
                    break;
            }
            return rval;
        }
    }
}
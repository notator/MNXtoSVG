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

        public readonly List<ISequenceComponent> Seq;

        public int Ticks { get; set; } = 1;

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

            A.Assert(r.Name == "grace"); // end of grace

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
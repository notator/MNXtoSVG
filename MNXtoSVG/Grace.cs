using System;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-grace-element
    /// </summary>
    public class Grace : IWritable, ITicks, ITicksSequenceComponent
    {
        public readonly MNXCGraceType Type = MNXCGraceType.stealPrevious; // spec says this is the default.
        public readonly bool? Slash = null;

        public readonly List<IWritable> Seq;

        public int Ticks { get; set; } = 1;

        public Grace(XmlReader r)
        {            
            G.Assert(r.Name == "grace");

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

            Seq = G.GetSequenceContent(r, "grace", false);

            G.Assert(r.Name == "grace"); // end of grace

        }

        private MNXCGraceType GetGraceType(string value)
        {
            MNXCGraceType rval = MNXCGraceType.stealPrevious; // default (spec)
            switch(value)
            {
                case "steal-following":
                    rval = MNXCGraceType.stealFollowing;
                    break;
                case "make-time":
                    rval = MNXCGraceType.makeTime;
                    break;
                default:
                    G.ThrowError("Error: unknown grace type.");
                    break;
            }
            return rval;
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
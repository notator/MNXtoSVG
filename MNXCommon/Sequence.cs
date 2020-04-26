using System;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-sequence-element
    internal class Sequence : EventGroup, IHasTicks, IPartMeasureComponent
    {
        public readonly Orientation? Orient = null; // default
        public readonly uint? StaffIndex = null; // default
        public readonly string VoiceID = null; // default

        public Sequence(XmlReader r, bool isGlobal)
        {
            M.Assert(r.Name == "sequence");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "orient":
                        if(r.Value == "up")
                            Orient = Orientation.up;
                        else if(r.Value == "down")
                            Orient = Orientation.down;
                        break;
                    case "staff":
                        StaffIndex = UInt32.Parse(r.Value);
                        break;
                    case "voice":
                        VoiceID = r.Value;
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            SequenceComponents = GetSequenceComponents(r, "sequence", isGlobal);

            M.Assert(r.Name == "sequence");
        }

        /// <summary>
        /// If the ticksObject is not found, this function returns the current length of the sequence.
        /// </summary>
        /// <returns></returns>
        internal int TickPositionInSeq(IHasTicks ticksObject)
        {
            int rval = 0;
            foreach(var seqObj in SequenceComponents)
            {
                if(seqObj is IHasTicks tObj)
                {
                    if(tObj == ticksObject)
                    {
                        break;
                    }
                    rval += tObj.Ticks;
                }
            }

            return rval;
        }

        internal List<IUniqueDef> GetIUDs()
        {
            throw new NotImplementedException();
        }
    }
}
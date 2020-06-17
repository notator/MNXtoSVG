using System;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-sequence-element
    public class Sequence : EventGroup, IHasTicks, IPartMeasureComponent
    {
        public readonly Orientation? Orient = null; // default
        public readonly int? StaffIndex = null; // default
        public readonly string VoiceID = null; // default

        /// <summary>
        /// This value is set by the Part constructor, once the Part as a whole has been read.
        /// Sequences that have no VoiceID have IndexID == their top-bottom index value.
        /// Sequences that have a VoiceID have an IndexID alloctaed in bottom-to-top order (on a 'heap').
        /// </summary>
        public int IndexID;

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
                        StaffIndex = Int32.Parse(r.Value) - 1; //MNX indices start at 1, MNXtoSVG indices start at 0.
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

        public List<IUniqueDef> SetMsDurationsAndGetIUniqueDefs(double millisecondsPerTick)
        {
            List<Event> events = this.Events;
            SetMsDurationPerEvent(events, millisecondsPerTick);

            var rval = new List<IUniqueDef>();
            OctaveShift octaveShift = null;

            foreach(var seqObj in SequenceComponents)
            {
                if(seqObj is Directions d)
                {
                    if(d.Clef != null)
                    {
                        rval.Add(d.Clef as IUniqueDef);
                    }
                    if(d.KeySignature != null)
                    {
                        rval.Add(d.KeySignature as IUniqueDef);
                    }
                    if(d.TimeSignature != null)
                    {
                        rval.Add(d.TimeSignature as IUniqueDef);
                    }
                    if(d.OctaveShift != null)
                    {
                        octaveShift = d.OctaveShift; // set as Event or Grace attribute below
                    }
                }
                else if(seqObj is Beamed beamed)
                {
                    M.Assert(beamed.Events.Count > 1);
                    int maxIndex = beamed.Events.Count - 1;
                    for(var i = 0; i < beamed.Events.Count; i++)
                    {
                        var evt = beamed.Events[i];
                        if(i == 0)
                        {
                            evt.IsBeamStart = true;
                        }
                        else if( i == maxIndex)
                        {
                            evt.IsBeamEnd = true;
                        }

                        rval.Add(evt as IUniqueDef);
                    }
                }
                else if(seqObj is Event evt)
                {
                    evt.OctaveShift = octaveShift;
                    octaveShift = null;
                    rval.Add(evt as IUniqueDef);
                }
                else if(seqObj is Grace g)
                {
                    var graceComponents = g.SequenceComponents;
                    foreach(var graceCompt in graceComponents)
                    {
                        // Assuming that Grace groups can only contain Events and Directions...
                        if(graceCompt is Event graceEvt)
                        {
                            graceEvt.OctaveShift = octaveShift;
                            octaveShift = null;
                            rval.Add(graceEvt as IUniqueDef);
                        }
                        if(graceCompt is Directions graceDir)
                        {
                            if(graceDir.OctaveShift != null)
                            {
                                octaveShift = graceDir.OctaveShift;
                            }
                        }
                    }
                }
                else
                {
                    throw new ApplicationException("unhandled SequenceComponent type.");
                    //rval.Add(seqObj as IUniqueDef);
                }
            }
            return rval;
        }

        private void SetMsDurationPerEvent(List<Event> events, double millisecondsPerTick)
        {
            var tickPositions = new List<int>();
            int currentTickPosition = 0;
            foreach(var evt in Events)
            {
                tickPositions.Add(currentTickPosition);
                currentTickPosition += evt.Ticks;
            }
            tickPositions.Add(currentTickPosition);

            var msPositions = new List<int>();
            foreach(var tickPosition in tickPositions)
            {
                msPositions.Add((int)Math.Round(tickPosition * millisecondsPerTick));
            }

            for(var i = 1; i < msPositions.Count; i++)
            {
                int msDuration = msPositions[i] - msPositions[i - 1];
                events[i - 1].MsDuration = msDuration;
            }
        }
    }
}
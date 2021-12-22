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
        public readonly int Index = -1; // 0-based, always set by ctor.

        public override string ToString() => $"Sequence: TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration} MsPosInScore={MsPosInScore} MsDuration={MsDuration}";

        /// <summary>
        /// This value is set by the Part constructor, once the Part as a whole has been read.
        /// Sequences that have no VoiceID have IndexID == their top-bottom index value.
        /// Sequences that have a VoiceID have an IndexID alloctaed in bottom-to-top order (on a 'heap').
        /// </summary>
        public int IndexID;

        public int MsPositionInScore { get; private set; }

        public Sequence(XmlReader r, TimeSignature currentTimeSig, int measureindex, int ticksPosInScore, int sequenceIndex)
        {
            M.Assert(r.Name == "sequence");

            Index = sequenceIndex;
            TicksPosInScore = ticksPosInScore;

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

            M.ReadToXmlElementTag(r, "event", "tuplet", "grace", "directions", "beams", "forward");

            while(r.Name == "event" || r.Name == "tuplet" || r.Name == "grace" || r.Name == "directions" || 
                r.Name == "beams" || r.Name == "forward")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "event":
                            Event e = new Event(r, ticksPosInScore);
                            ticksPosInScore += e.TicksDuration;
                            SequenceComponents.Add(e);
                            break;
                        case "tuplet":
                            TupletDef tupletDef = new TupletDef(r, ticksPosInScore);
                            ticksPosInScore += tupletDef.TicksDuration;
                            SequenceComponents.Add(tupletDef);
                            break;
                        case "grace":
                            Grace grace = new Grace(r, ticksPosInScore);
                            ticksPosInScore += grace.TicksDuration;
                            SequenceComponents.Add(grace);
                            break;
                        case "directions":
                            SequenceComponents.Add(new SequenceDirections(r, currentTimeSig, ticksPosInScore));
                            break;
                        case "beams":
                            SequenceComponents.Add(new Beams(r, ticksPosInScore));
                            break;
                        case "forward":
                            Forward forward = new Forward(r, ticksPosInScore);
                            ticksPosInScore += forward.TicksDuration;
                            SequenceComponents.Add(forward);
                            break;
                    }
                }

                M.ReadToXmlElementTag(r, "event", "tuplet", "grace", "directions", "beams", "forward", "sequence");
            }

            M.Assert(Events.Count > 0);
            M.Assert(r.Name == "sequence"); // end of sequence content
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
                    rval += tObj.TicksDuration;
                }
            }

            return rval;
        }

        public List<IUniqueDef> SetMsDurationsAndGetIUniqueDefs(int seqMsPositionInScore, double millisecondsPerTick)
        {
            List<Event> events = this.Events;
            MsPositionInScore = seqMsPositionInScore;

            SetMsDurations(seqMsPositionInScore, events, millisecondsPerTick);

            var rval = new List<IUniqueDef>();
            OctaveShift octaveShift = null;

            foreach(var seqObj in SequenceComponents)
            {
                if(seqObj is PartDirections d)
                {
                    if(d.Clef != null)
                    {
                        rval.Add(d.Clef as IUniqueDef);
                    }
                    if(d.KeySignature != null)
                    {
                        rval.Add(d.KeySignature as IUniqueDef);
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
                    }
                }
                else if(seqObj is TupletDef t)
                {
                    octaveShift = GetTupletComponents(t, rval, octaveShift);
                }
                else
                {
                    throw new ApplicationException("unhandled SequenceComponent type.");
                    //rval.Add(seqObj as IUniqueDef);
                }
            }
            return rval;
        }

        /// <summary>
        /// recursive function (for nested tuplets)
        /// </summary>
        /// <param name="tupletDef"></param>
        /// <param name="iuds"></param>
        /// <param name="octaveShift"></param>
        /// <returns>current octave shift (can be null)</returns>
        private static OctaveShift GetTupletComponents(TupletDef tupletDef, List<IUniqueDef> iuds, OctaveShift octaveShift)
        {
            var tupletComponents = tupletDef.SequenceComponents;
            Event firstEvent = (Event)tupletComponents.Find(e => e is Event);
            if(firstEvent.TupletDefs == null)
            {
                firstEvent.TupletDefs = new List<TupletDef>();
            }
            firstEvent.TupletDefs.Add(tupletDef);
            foreach(var component in tupletComponents)
            {
                if(component is Event tupletEvt)
                {
                    tupletEvt.OctaveShift = octaveShift;
                    octaveShift = null;
                    iuds.Add(tupletEvt as IUniqueDef);
                }
                else if(component is TupletDef tplet)
                {
                    // recursive call
                    octaveShift = GetTupletComponents(tplet, iuds, octaveShift);
                }
            }

            return octaveShift;
        }

        private void SetMsDurations(int seqMsPositionInScore, List<Event> events, double millisecondsPerTick)
        {
            var tickPositions = new List<int>();
            int currentTickPosition = 0;
            foreach(var evt in Events)
            {
                tickPositions.Add(currentTickPosition);
                currentTickPosition += evt.TicksDuration;
            }
            tickPositions.Add(currentTickPosition);

            var msPositions = new List<int>();
            foreach(var tickPosition in tickPositions)
            {
                msPositions.Add((int)Math.Round(tickPosition * millisecondsPerTick));
            }

            int seqMsDuration = 0;
            int evtMsPositionInScore = seqMsPositionInScore;
            for(var i = 1; i < msPositions.Count; i++)
            {
                int msDuration = msPositions[i] - msPositions[i - 1];
                events[i - 1].MsDuration = msDuration;
                events[i - 1].MsPosInScore = evtMsPositionInScore;
                evtMsPositionInScore += msDuration;
                seqMsDuration += msDuration;
            }

            MsDuration = seqMsDuration;
        }
    }
}
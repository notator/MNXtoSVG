﻿using System;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-sequence-element
    public class Sequence : EventGroup, IPartMeasureComponent
    {
        public readonly Orientation? Orient = null; // default
        public readonly int? StaffIndex = null; // default
        public readonly string VoiceID = null; // default
        public readonly int Index = -1; // 0-based, always set by ctor.

        public readonly SequenceDirections Directions = new SequenceDirections(); // contains an empty Components list.

        public override string ToString() => $"Sequence: MsPosInScore={MsPosInScore} MsDuration={MsDuration}";

        /// <summary>
        /// This value is set by the Part constructor, once the Part as a whole has been read.
        /// Sequences that have no VoiceID have IndexID == their top-bottom index value.
        /// Sequences that have a VoiceID have an IndexID alloctaed in bottom-to-top order (on a 'heap').
        /// </summary>
        public int IndexID;

        public Sequence(XmlReader r, TimeSignature currentTimeSig, int measureindex, int sequenceIndex)
        {
            M.Assert(r.Name == "sequence");

            Index = sequenceIndex;

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
                            Event e = new Event(r);
                            Components.Add(e);
                            break;
                        case "tuplet":
                            TupletDef tupletDef = new TupletDef(r, true);
                            Components.Add(tupletDef);
                            break;
                        case "grace":
                            Grace grace = new Grace(r);
                            // Grace notes are initially given the normal TickDuration for their MNXDurationSymbol,
                            // so that the different grace duration classes have different, proportional sizes.
                            // All ticksPosInScore and ticksDuration values are updated for grace notes
                            // when the whole score has been read (in MNX.AdjustForGraceNotes())
                            Components.Add(grace);
                            break;
                        case "directions":
                            Directions = new SequenceDirections(r, currentTimeSig);
                            break;
                        case "beams":
                            Components.Add(new BeamBlocks(r));
                            break;
                        case "forward":
                            Forward forward = new Forward(r);
                            Components.Add(forward);
                            break;
                    }
                }

                M.ReadToXmlElementTag(r, "event", "tuplet", "grace", "directions", "beams", "forward", "sequence");
            }

            M.Assert(IEventsAndGraces.Count > 0);
            M.Assert(r.Name == "sequence"); // end of sequence content
        }

        public List<IUniqueDef> GetIUniqueDefs()
        {
            var rval = new List<IUniqueDef>();
            OctaveShift octaveShift = null;

            // SequenceComponents includes the SequenceDirectionComponents (see the Sequence constructor above)
            foreach(var seqObj in Components)
            {
                if(seqObj is SequenceDirections d)
                {
                    if(d.Clef != null)
                    {
                        rval.Add(d.Clef as IUniqueDef); // mid staff clef change
                    }
                    //if(d.Cresc != null)
                    //{
                    //    rval.Add(d.Cresc as IUniqueDef);
                    //}
                    //if(d.Dim != null)
                    //{
                    //    rval.Add(d.Cim as IUniqueDef);
                    //}
                    //if(d.Dynamic != null)
                    //{
                    //    rval.Add(d.Dynamic as IUniqueDef);
                    //}
                    //if(d.Expression != null)
                    //{
                    //    rval.Add(d.Expression as IUniqueDef);
                    //}
                    //if(d.Instruction != null)
                    //{
                    //    rval.Add(d.Instruction as IUniqueDef);
                    //}
                    if(d.OctaveShift != null)
                    {
                        rval.Add(d.OctaveShift as IUniqueDef);
                    }
                    //if(d.Wedge != null)
                    //{
                    //    rval.Add(d.Wedge as IUniqueDef);
                    //}
                }
                else if(seqObj is Event evt)
                {
                    evt.OctaveShift = octaveShift;
                    octaveShift = null;
                    rval.Add(evt as IUniqueDef);
                }
                else if(seqObj is Grace g)
                {
                    var graceComponents = g.Components;
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
                else if(seqObj is BeamBlocks beamBlocks)
                {
                    //foreach(var beamBlock in beamBlocks.Blocks)
                    //{
                    //    rval.Add(beamBlock as IUniqueDef);
                    //}
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
        private OctaveShift GetTupletComponents(TupletDef tupletDef, List<IUniqueDef> iuds, OctaveShift octaveShift)
        {
            var tupletComponents = tupletDef.Components;
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
                else if(component is Grace g)
                {
                    var graceComponents = g.Components;
                    foreach(var graceCompt in graceComponents)
                    {
                        // Assuming that Grace groups can only contain Events and Directions...
                        if(graceCompt is Event graceEvt)
                        {
                            graceEvt.OctaveShift = octaveShift;
                            octaveShift = null;
                            iuds.Add(graceEvt as IUniqueDef);
                        }
                    }
                }
                else if(component is TupletDef tplet)
                {
                    // recursive call
                    octaveShift = GetTupletComponents(tplet, iuds, octaveShift);
                }
            }

            return octaveShift;
        }

        public void SetMsDurations(int seqMsPositionInScore, double millisecondsPerTick)
        {
            MsPosInScore = seqMsPositionInScore;

            var tickPositions = new List<int>();
            int currentTickPosition = 0;
            foreach(var evt in IEventsAndGraces)
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

            int totalMsDuration = 0;
            int evtMsPositionInScore = MsPosInScore;
            List<IHasTicksDuration> iEventsAndGraces = IEventsAndGraces;
            for(var i = 1; i < msPositions.Count; i++)
            {
                IHasTicksDuration ihtd = iEventsAndGraces[i - 1];
                int msDuration = msPositions[i] - msPositions[i - 1];
                if(ihtd is Event e)
                {
                    e.MsDuration = msDuration;
                    e.MsPosInScore = evtMsPositionInScore;
                }
                else if(ihtd is Forward f)
                {
                    f.MsDuration = msDuration;
                    f.MsPosInScore = evtMsPositionInScore;
                }
                else if(ihtd is Grace g)
                {
                    g.MsDuration = msDuration;
                    g.MsPosInScore = evtMsPositionInScore;
                    List<IHasTicksDuration> graceEvents = g.IEventsAndGraces;
                    List<int> tickDurations = new List<int>();
                    foreach(var ge in graceEvents)
                    {
                        tickDurations.Add(ge.TicksDuration);
                    }
                    List<int> msDurations = M.IntDivisionSizes(msDuration, tickDurations);
                    int localMsPosInScore = g.MsPosInScore;
                    for(int j = 0; j < msDurations.Count; j++)
                    {
                        int msDur = msDurations[j];
                        graceEvents[j].MsPosInScore = localMsPosInScore;
                        graceEvents[j].MsDuration = msDur;
                        localMsPosInScore += msDur;
                    }
                }
                evtMsPositionInScore += msDuration;
                totalMsDuration += msDuration;
            }

            MsDuration = totalMsDuration;
        }
    }
}
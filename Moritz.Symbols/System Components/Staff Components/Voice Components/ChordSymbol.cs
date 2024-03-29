using System;
using System.Collections.Generic;
using System.Diagnostics;
using MNX.Common;
using MNX.Globals;
using Moritz.Xml;

namespace Moritz.Symbols
{
    public abstract class ChordSymbol : DurationSymbol
    {
        public ChordSymbol(Voice voice, int msDuration, int absMsPosition, MNX.Common.Event mnxEventDef, double fontSize)
            : base(voice, msDuration, absMsPosition, mnxEventDef.MNXDurationSymbol, fontSize)
        {
            // note that all chord symbols have a stem! 
            // Even cautionary, semibreves and breves need a stem direction in order to set chord Metrics correctly.
            Stem = new Stem(this);

            IsBeamStart = mnxEventDef.IsBeamStart;
            IsBeamRestart = mnxEventDef.IsBeamRestart;
            IsBeamEnd = mnxEventDef.IsBeamEnd;

            OctaveShift = mnxEventDef.OctaveShift;
            EndOctaveShift = mnxEventDef.EndOctaveShift;
            TicksPosInScore = mnxEventDef.TicksPosInScore;
            TicksDuration = mnxEventDef.TicksDuration;

            SlurDefs = mnxEventDef.SlurDefs; // can be null
            TupletDefs = mnxEventDef.TupletDefs; // can be null
            EventID = mnxEventDef.ID;

            // Beam is currently null. Create when necessary.
        }

        /// <summary>
        /// Old constructor, currently not used (03.05.2020), but retained for future inspection
        /// </summary>
        public ChordSymbol(Voice voice, int msDuration, int absMsPosition, int minimumCrotchetDurationMS, double fontSize)
            : base(voice, msDuration, absMsPosition, minimumCrotchetDurationMS, fontSize)
        {
            M.Assert(false); // 03.05.2020: don't use this constructor (to be inspected once work on midi info begins).

            // note that all chord symbols have a stem! 
            // Even cautionary, semibreves and breves need a stem direction in order to set chord Metrics correctly.
            Stem = new Stem(this);

            // Beam is currently null. Create when necessary.
        }

        public VerticalDir DefaultStemDirection(Clef clef)
        {
            M.Assert(this.HeadsTopDown.Count > 0);
            double gap = 32; // dummy value
            List<double> topDownHeadOriginYs = new List<double>();
            int lastMidiPitch = int.MaxValue;
            foreach(Head head in this.HeadsTopDown)
            {
                M.Assert(head.MidiPitch < lastMidiPitch);
                topDownHeadOriginYs.Add(head.GetOriginY(clef, gap));
            }

            double heightOfMiddleStaffLine = (this.Voice.Staff.NumberOfStafflines / 2) * gap;
            double halfHeight = 0;
            if(topDownHeadOriginYs.Count == 1)
                halfHeight = topDownHeadOriginYs[0];
            else
                halfHeight = (topDownHeadOriginYs[topDownHeadOriginYs.Count - 1] + topDownHeadOriginYs[0]) / 2;

            if(halfHeight <= heightOfMiddleStaffLine)
                return VerticalDir.down;
            else
                return VerticalDir.up;
        }

        /// <summary>
        /// In capella 2008, only single articulation or "staccato tenuto" are supported.
        /// </summary>
        public List<Articulation> Articulations = new List<Articulation>();

        #region composition

        #region private for SetPitches()
        /// <summary>
        /// Returns true if sharps are to be used to represent the midiPitches,
        /// or false if flats are to be used.
        /// </summary>
        /// <param name="midiPitches"></param>
        /// <returns></returns>
        internal bool UseSharps(List<byte> midiPitches, List<byte> midiVelocities)
        {
            for(int i = 0; i < midiPitches.Count; i++)
            {
                M.Assert(midiPitches[i] >= 0 && midiPitches[i] < 128);
            }

            bool useSharps = true;
            List<int> midiIntervals = new List<int>();
            for(int i = 1; i < midiPitches.Count; i++)
            {
                midiIntervals.Add(midiPitches[i] - midiPitches[i - 1]);
            }
            List<int> collapsedIntervals = new List<int>();
            foreach(int midiInterval in midiIntervals)
            {
                collapsedIntervals.Add(midiInterval % 12);
            }

            // look in the following order (I dont like augmented seconds at all)!
            int[] preferredIntervals = { 3, 7, 1, 2, 4, 5, 6, 8, 9, 10, 11 };
            bool? useSharpsOrNull = null;
            foreach(int interval in preferredIntervals)
            {
                int index = 0;
                if(collapsedIntervals.Contains(interval))
                {
                    foreach(int value in collapsedIntervals)
                    {
                        if(value == interval)
                            break;
                        index++;
                    }
                    // index is now both the index of the "most preferred" interval 
                    // and the index of the lower midiPitch of the "most preferred" interval.
                    Head head = null;
                    M.Assert(midiVelocities != null); // April 2020 (InputChordSymbols no longer exist)
                    if(midiVelocities != null)
                    {
                        head = new Head(null, midiPitches[index], midiVelocities[index], true);
                    }
                    else
                    {
                        //head = new Head(null, midiPitches[index], -1, true);  // a Head in an InputChordSymbol
                    }
                    // head is either natural or sharp.
                    int preferredInterval = collapsedIntervals[index];
                    useSharpsOrNull = GetUseSharps(head, preferredInterval);
                }
                if(useSharpsOrNull != null)
                {
                    useSharps = (bool)useSharpsOrNull;
                    break;
                }
            }
            return useSharps;
        }
        /// <summary>
        /// Head.Alteration is either 0 or 1 (natural or sharp).
        /// Returns the sharp/flat preference for representing the interval on this head,
        /// or null if there is no preference.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private bool? GetUseSharps(Head head, int interval)
        {
            M.Assert(interval > 0 && interval < 12);
            M.Assert(head.Alteration == 0 || head.Alteration == 1);
            bool? useSharpsOrNull = null;
            #region Head is A
            if(head.Pitch[0] == 'A')
            {
                if(head.Alteration == 0) // (A)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                            useSharpsOrNull = true;
                            break;
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (A#)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 7:
                        case 4:
                        case 2:
                            useSharpsOrNull = false;
                            break;
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }

                }
            }
            #endregion A
            #region Head is B
            else if(head.Pitch[0] == 'B')
            {
                if(head.Alteration == 0) // (B)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 7:
                        case 4:
                        case 2:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (B#)
                {
                    useSharpsOrNull = false;
                }
            }
            #endregion
            #region Head is C
            else if(head.Pitch[0] == 'C')
            {
                if(head.Alteration == 0) // (C)
                {
                    switch(interval)
                    {
                        case 10:
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (C#)
                {
                    switch(interval)
                    {
                        case 4:
                            useSharpsOrNull = false;
                            break;
                        case 10:
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is C
            #region Head is D
            else if(head.Pitch[0] == 'D')
            {
                if(head.Alteration == 0) // (D)
                {
                    switch(interval)
                    {
                        case 11:
                        case 4:
                            useSharpsOrNull = true;
                            break;
                        case 8:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (D#)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                        case 2:
                            useSharpsOrNull = false;
                            break;
                        case 8:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is D
            #region Head is E
            else if(head.Pitch[0] == 'E')
            {
                if(head.Alteration == 0) // (E)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                        case 2:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (E#)
                {
                    useSharpsOrNull = false;
                }
            }
            #endregion Head is E
            #region Head is F
            else if(head.Pitch[0] == 'F')
            {
                if(head.Alteration == 0) // (F)
                {
                    switch(interval)
                    {
                        case 10:
                        case 8:
                        case 5:
                        case 3:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (F#)
                {
                    switch(interval)
                    {
                        case 10:
                        case 8:
                        case 5:
                        case 3:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        case 11: // should really be G-flat, but I dont like G-flats!
                            //useSharpsOrNull = false;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is F
            #region Head is G
            else if(head.Pitch[0] == 'G')
            {
                if(head.Alteration == 0) // (G)
                {
                    switch(interval)
                    {
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = false;
                            break;
                        case 11:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
                else // Alteration == 1 (G#)
                {
                    switch(interval)
                    {
                        case 11:
                        case 9:
                        case 4:
                            useSharpsOrNull = false;
                            break;
                        case 8:
                        case 3:
                        case 1:
                            useSharpsOrNull = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion Head is G
            return useSharpsOrNull;
        }

        internal void FinalizeBeamBlock(double rightBarlineX)
        {
            M.Assert(this.BeamBlock != null);
            M.Assert(this.BeamBlockDef != null);

            BeamBlock.FinalizeBeamBlock(BeamBlockDef, rightBarlineX);
        }
        #endregion private

        /// <summary>
        /// This chordSymbol is in the lower of two voices on a staff. The argument is another synchronous chordSymbol
        /// at the same MsPosition on the same staff. Both chordSymbols have ChordMetrics, and the chord in the lower
        /// voice has been moved (either right or left) so that there are no collisions between noteheads.
        /// This function moves the accidentals in both chords horizontally, so that they are all on the left of both
        /// chords but as far to the right as possible without there being any collisions.
        /// Accidentals are positioned in top-bottom and right-left order.
        /// If two noteheads are at the same diatonic height, both accidentals will already exist and have forced display.
        /// Such accidentals are placed in the left-right order of the noteheads
        /// </summary>
        /// <param name="upperChord"></param>
        public void AdjustAccidentalsX(ChordSymbol upperChord)
        {
            this.ChordMetrics.AdjustAccidentalsForTwoChords(upperChord.ChordMetrics, M.PageFormat.StafflineStemStrokeWidthVBPX);
        }

        /// <summary>
        /// Returns the maximum (positive) horizontal distance by which this anchorage symbol overlaps
        /// (any characters in) the previous noteObjectMoment (which contains symbols from both voices
        /// in a 2-voice staff).
        /// This function is used by rests and barlines.It is overridden by chords.
        /// </summary>
        /// <param name="previousAS"></param>
        public override double OverlapWidth(NoteObjectMoment previousNOM)
        {
            double overlap = double.MinValue;
            double localOverlap = 0;
            foreach(Anchor previousAS in previousNOM.Anchors)
            {
				//if(this is Study2b2ChordSymbol)
				//	localOverlap = Metrics.OverlapWidth(previousAS);
				//else
                localOverlap = ChordMetrics.OverlapWidth(previousAS);

                overlap = overlap > localOverlap ? overlap : localOverlap;
            }
            return overlap;
        }
        #endregion composition

        #region display attributes
        /// <summary>
        /// up/down means that the Chord is notated in the staff above/below it's real staff.
        /// This may be used, when a voice changes from one staff to another.
        /// </summary>
        public VerticalDir NotationStaff = VerticalDir.none;
        #endregion display attributes

        /// <summary>
        /// Returns this.Metrics cast to ChordMetrics.
        /// Before accessing this property, this.Metrics must be assigned to an object of type ChordMetrics.
        /// </summary>
        internal ChordMetrics ChordMetrics
        {
            get
            {
				if(!(Metrics is ChordMetrics chordMetrics))
				{
					throw new ApplicationException();
				}
				return chordMetrics;
            }
        }

        public readonly bool IsBeamStart;
        public readonly bool IsBeamRestart;
        public readonly bool IsBeamEnd;

        public MNX.Common.BeamBlock BeamBlockDef { get; internal set; }
        public BeamBlock BeamBlock = null; // defaults

        public readonly OctaveShift OctaveShift;
        public readonly OctaveShift EndOctaveShift;
        public readonly int TicksPosInScore;
        public readonly int TicksDuration;
        public Stem Stem = null; // defaults
        
        public List<Head> HeadsTopDown = new List<Head>(); // Heads are in top-down order.
        public List<SlurDef> SlurDefs = null; // definitions from MNX file
        public readonly string EventID = null;  // ID from MNX file

        public int? MsDurationToNextBarline { get { return _msDurationToNextBarline; } }
        protected int? _msDurationToNextBarline = null;
    }
}

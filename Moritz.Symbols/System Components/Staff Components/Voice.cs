using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using Moritz.Spec;
using MNX.Globals;

namespace Moritz.Symbols
{
    /// <summary>
    ///  A sequence of noteObjects.
    /// </summary>
    public abstract class Voice
    {
        public Voice(Staff staff, Voice voice)
        {
            Staff = staff;
            StemDirection = voice.StemDirection;
            this.AppendNoteObjects(voice.NoteObjects);
        }

        public Voice(Staff staff)
        {
            Staff = staff;
        }

        /// <summary>
        /// Writes out an SVG Voice
        /// The following NoteObject types are only written if voiceIndex == 0:
        ///   Barline, Clef, SmallClef, KeySignature, TimeSignature.
        /// </summary>
        /// <param name="w"></param>
        public virtual void WriteSVG(SvgWriter w, int voiceIndex, List<CarryMsgs> carryMsgsPerChannel, bool graphicsOnly)
        {
            for(int i = 0; i < NoteObjects.Count; ++i)
            {
				NoteObject noteObject = NoteObjects[i];
				if(noteObject is Barline barline && voiceIndex == 0)
				{
					bool isLastNoteObject = (i == (NoteObjects.Count - 1));
					double top = Staff.Metrics.StafflinesTop;
					double bottom = Staff.Metrics.StafflinesBottom;
					if(barline.IsVisible)
					{
						barline.WriteSVG(w, top, bottom, isLastNoteObject);
					}
					barline.WriteDrawObjectsSVG(w); 
				}
				if(noteObject is CautionaryChordSymbol cautionaryChordSymbol)
				{
					cautionaryChordSymbol.WriteSVG(w);
				}
				if(noteObject is OutputChordSymbol outputChordSymbol)
				{
					M.Assert(carryMsgsPerChannel != null);
					outputChordSymbol.WriteSVG(w, this.MidiChannel, carryMsgsPerChannel[this.MidiChannel], graphicsOnly);
				}
				if(noteObject is OutputRestSymbol outputRestSymbol)
				{
					M.Assert(carryMsgsPerChannel != null);
					outputRestSymbol.WriteSVG(w, this.MidiChannel, carryMsgsPerChannel[this.MidiChannel], graphicsOnly);
				}
				if(noteObject is Clef clef && voiceIndex == 0)
				{
					if(clef.Metrics != null)
					{
						// if this is the first barline, the staff name and (maybe) barnumber will be written.
						ClefMetrics cm = clef.Metrics as ClefMetrics;
						clef.WriteSVG(w, cm.ClefID, cm.OriginX, cm.OriginY);
					}
				}
				if(noteObject is SmallClef smallClef && voiceIndex == 0)
				{
					if(smallClef.Metrics != null)
					{
						SmallClefMetrics scm = smallClef.Metrics as SmallClefMetrics;
						smallClef.WriteSVG(w, scm.ClefID, scm.OriginX, scm.OriginY);
					}
				}
                if(noteObject is KeySignature keySignature && voiceIndex == 0)
                {
                    keySignature.WriteSVG(w, keySignature.Fifths.ToString(), keySignature.Metrics.OriginX, keySignature.Metrics.OriginY);
                }
                if(noteObject is TimeSignature timeSignature && voiceIndex == 0)
                {
                    timeSignature.WriteSVG(w, timeSignature.Signature, timeSignature.Metrics.OriginX, timeSignature.Metrics.OriginY);
                }
			}
		}

		public bool ContainsAChordSymbol
		{
			get
			{
				bool containsAChordSymbol = false;
				foreach(NoteObject noteObject in NoteObjects)
				{
					if(noteObject is ChordSymbol)
					{
						containsAChordSymbol = true;
						break;
					}
				}
				return containsAChordSymbol;
			}
		}

        /// <summary>
        /// Returns the first barline to occur before any durationSymbols, or null if no such barline exists.
        /// </summary>
        public Barline InitialBarline
        {
            get
            {
                Barline initialBarline = null;
                foreach(NoteObject noteObject in NoteObjects)
                {
                    initialBarline = noteObject as Barline;
                    if(noteObject is DurationSymbol || noteObject is Barline)
                        break;
                }
                return initialBarline;
            }
        }

        public DurationSymbol FinalDurationSymbol
        {
            get
            {
                DurationSymbol finalDurationSymbol = null;
                for(int i = this.NoteObjects.Count - 1; i >= 0; i--)
                {
                    finalDurationSymbol = this.NoteObjects[i] as DurationSymbol;
                    if(finalDurationSymbol != null)
                        break;
                }
                return finalDurationSymbol;
            }
        }

        #region composition
        /// <summary>
        /// Replaces the DurationSymbol symbolToBeReplaced (which is in this Voice's NoteObjects)
        /// by the all the noteObjects. Sets each of the noteObjects' Voice to this. 
        /// </summary>
        public void Replace(DurationSymbol symbolToBeReplaced, List<NoteObject> noteObjects)
        {
            #region conditions
            M.Assert(symbolToBeReplaced != null && symbolToBeReplaced.Voice == this);
            #endregion conditions

            List<NoteObject> tempList = new List<NoteObject>(this.NoteObjects);
            this.NoteObjects.Clear();
            int i = 0;
            while(tempList.Count > i && tempList[i] != symbolToBeReplaced)
            {
                this.NoteObjects.Add(tempList[i]);
                i++;
            }
            foreach(NoteObject noteObject in noteObjects)
            {
                noteObject.Voice = this;
                this.NoteObjects.Add(noteObject);
            }
            // tempList[i] is the symbolToBeReplaced
            i++;
            while(tempList.Count > i)
            {
                this.NoteObjects.Add(tempList[i]);
                i++;
            }
            tempList = null;
        }
        /// <summary>
        /// Appends a clone of the noteObjects to this voice's NoteObjects
        /// (Sets each new noteObjects container to this.)
        /// </summary>
        /// <param name="noteObjects"></param>
        public void AppendNoteObjects(List<NoteObject> noteObjects)
        {
            foreach(NoteObject noteObject in noteObjects)
            {
                noteObject.Voice = this;
                NoteObjects.Add(noteObject);
            }
        }

        /// <summary>
        /// Sets Chord.Stem.Direction for each chord.
        /// Chords are beamed together, duration classes permitting, unless a rest or clef intervenes.
        /// If a barline intervenes, and beamsCrossBarlines is true, the chords are beamed together.
        /// If a barline intervenes, and beamsCrossBarlines is false, the beam is broken.
        /// </summary>
        public void SetChordStemDirectionsAndCreateBeamBlocks(PageFormat pageFormat)
        {
            List<ChordSymbol> chordsBeamedTogether = new List<ChordSymbol>();
            Clef currentClef = null;
            bool breakGroup = false;
            ChordSymbol lastChord = null;
            foreach(ChordSymbol cs in ChordSymbols)
                lastChord = cs;

            foreach(NoteObject noteObject in NoteObjects)
            {
				ChordSymbol chord = noteObject as ChordSymbol;
				RestSymbol rest = noteObject as RestSymbol;
                Clef clef = noteObject as Clef;
                Barline barline = noteObject as Barline;

				if(noteObject is CautionaryChordSymbol cautionaryChord)
					continue;

				if(chord != null)
                {
                    if(chord.DurationClass == DurationClass.cautionary
                    || chord.DurationClass == DurationClass.breve
                    || chord.DurationClass == DurationClass.semibreve
                    || chord.DurationClass == DurationClass.minim
                    || chord.DurationClass == DurationClass.crotchet)
                    {
                        if(currentClef != null)
                        {
                            if(this.StemDirection == VerticalDir.none)
                                chord.Stem.Direction = chord.DefaultStemDirection(currentClef);
                            else
                                chord.Stem.Direction = this.StemDirection;
                        }
                        breakGroup = true;
                    }
                    else
                    {
                        chordsBeamedTogether.Add(chord);
						// chord.Stem.BeamContinues is the value of
						// MidiChordDef.BeamContinues.
						// This value is true by default, but can be set
						// (in MidiChordDef) classes used by composition
						// algorithms.
						if(chord.Stem.BeamContinues)
                            breakGroup = false;
                        else
                            breakGroup = true;
                    }
                }

                if(chordsBeamedTogether.Count > 0)
                {
                    if(rest != null)
                    {
                        if(rest.LocalCautionaryChordDef == null)
                            breakGroup = true;
                    }

                    if(clef != null)
                        breakGroup = true;

                    if(barline != null && !pageFormat.BeamsCrossBarlines)
                        breakGroup = true;

                    if(chord == lastChord)
                        breakGroup = true;
                }

                if(chordsBeamedTogether.Count > 0 && breakGroup)
                {
                    if(currentClef != null)
                    {
                        if(chordsBeamedTogether.Count == 1)
                        {
                            if(this.StemDirection == VerticalDir.none)
                                chordsBeamedTogether[0].Stem.Direction = chordsBeamedTogether[0].DefaultStemDirection(currentClef);
                            else
                                chordsBeamedTogether[0].Stem.Direction = this.StemDirection;

                        }
                        else if(chordsBeamedTogether.Count > 1)
                        {
                            double beamThickness = pageFormat.BeamThickness;
                            double beamStrokeThickness = pageFormat.StafflineStemStrokeWidthVBPX;
                            chordsBeamedTogether[0].BeamBlock =
                                new BeamBlock(currentClef, chordsBeamedTogether, this.StemDirection, beamThickness, beamStrokeThickness);
                        }
                    }
                    chordsBeamedTogether.Clear();
                }

                if(clef != null)
                    currentClef = clef;
            }
        }

        /// <summary>
        /// The system has been justified horizontally, so all objects are at their final horizontal positions.
        /// The outer tips of stems which are inside BeamBlocks have been set to the beamBlock's DefaultStemTipY value.
        /// This function
        ///  1. creates the contained beams, and sets the final coordinates of their corners.
        ///  2. resets the contained Stem.Metrics (by creating and re-allocating new ones)
        ///  3. moves objects which are outside the stem tips vertically by the same amount as the stem tips are moved.
        /// </summary>
        public void FinalizeBeamBlocks()
        {
            HashSet<BeamBlock> beamBlocks = FindBeamBlocks();
            foreach(BeamBlock beamBlock in beamBlocks)
            {
                beamBlock.FinalizeBeamBlock();
            }
        }

        public void RemoveBeamBlockBeams()
        {
            HashSet<BeamBlock> beamBlocks = FindBeamBlocks();
            foreach(BeamBlock beamBlock in beamBlocks)
            {
                beamBlock.Beams.Clear();
            }
        }

        private HashSet<BeamBlock> FindBeamBlocks()
        {
            HashSet<BeamBlock> beamBlocks = new HashSet<BeamBlock>();
            foreach(ChordSymbol chord in ChordSymbols)
            {
                if(chord.BeamBlock != null)
                    beamBlocks.Add(chord.BeamBlock);
            }
            return beamBlocks;
        }

        #region Enumerators
        public IEnumerable AnchorageSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
					if(noteObject is AnchorageSymbol iHasDrawObjects)
						yield return iHasDrawObjects;
				}
            }
        }
        public IEnumerable DurationSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
					if(noteObject is DurationSymbol durationSymbol)
						yield return durationSymbol;
				}
            }
        }
        public IEnumerable ChordSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
					if(noteObject is ChordSymbol chordSymbol)
						yield return chordSymbol;
				}
            }
        }
        public IEnumerable RestSymbols
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
					if(noteObject is RestSymbol restSymbol)
						yield return restSymbol;
				}
            }
        }

        #endregion Enumerators

        #endregion composition

        #region fields loaded from .capx files
        #region attribute fields
        public VerticalDir StemDirection = VerticalDir.none;
        #endregion
        #region element fields
        public List<NoteObject> NoteObjects { get { return _noteObjects; } }
        private readonly List<NoteObject> _noteObjects = new List<NoteObject>();
        #endregion
        #endregion
        #region moritz-specific fields
        public Staff Staff; // container
        /// <summary>
        /// The first duration symbol in this (short) voice.
        /// </summary>
        public DurationSymbol FirstDurationSymbol
        {
            get
            {
                if(_firstDurationSymbol == null)
                {
                    foreach(NoteObject noteObject in NoteObjects)
                    {
                        _firstDurationSymbol = noteObject as DurationSymbol;
                        if(_firstDurationSymbol != null)
                            break;
                    }
                }
                return _firstDurationSymbol;
            }
            set
            {
                _firstDurationSymbol = value;
            }
        }
		#endregion

		private int _midiChannel = int.MaxValue;
		/// <summary>
		/// A MidiChannel attribute is always written for every OutputVoice in the first system in a score.
		/// No other OutputVoice MidiChannels are written.
		/// </summary>
		public int MidiChannel
		{ 
			get { return _midiChannel; } 
			set
			{
				M.Assert(value >= 0 && value <= 15);
				_midiChannel = value;
			}
		}

        public VoiceDef VoiceDef = null;

        private DurationSymbol _firstDurationSymbol;
    }
}

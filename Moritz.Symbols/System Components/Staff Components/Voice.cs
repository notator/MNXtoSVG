using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Xml;
using Moritz.Spec;
using MNX.Globals;
using MNX.Common;
using System.Drawing;

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
            bool suppressEndOfScoreBarline = false;

            for(int i = 0; i < NoteObjects.Count; ++i)
            {
                NoteObject noteObject = NoteObjects[i];
                if(noteObject is Barline barline && voiceIndex == 0)
                {
                    bool isLastNoteObject = (i == (NoteObjects.Count - 1));
                    double top = Staff.Metrics.StafflinesTop;
                    double bottom = Staff.Metrics.StafflinesBottom;
                    if(barline.IsVisible && !suppressEndOfScoreBarline)
                    {
                        barline.WriteSVG(w, top, bottom, isLastNoteObject, true);
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
                if(noteObject is RepeatSymbol repeatSymbol && voiceIndex == 0)
                {
                    bool isLastNoteObject = (i == (NoteObjects.Count - 1));
                    double top = Staff.Metrics.StafflinesTop;
                    double bottom = Staff.Metrics.StafflinesBottom;
                    repeatSymbol.WriteSVG(w, top, bottom, isLastNoteObject, true);

                    suppressEndOfScoreBarline = (i == (NoteObjects.Count - 2));
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

            List<NoteObject> tempList = new List<NoteObject>(NoteObjects);
            this.NoteObjects.Clear();
            int i = 0;
            while(tempList.Count > i && tempList[i] != symbolToBeReplaced)
            {
                NoteObjects.Add(tempList[i]);
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
        /// BeamBlocks are created, beginning with a chord that has IsBeamStart == true, and ending with a chord that has IsBeamEnd == true.
        /// BeamBlocks only contain ChordSymbols, but these may be interspersed with other NoteObjects (barlines, clefs, rests, cautionaryChords etc...)
        /// An exception is thrown, if a BeamBlock is open at the end of the voice.
        /// </summary>
        public void SetChordStemDirectionsAndCreateBeamBlocks(PageFormat pageFormat)
        {
            List<List<OutputChordSymbol>> beamedGroups = GetBeamedGroups();

            Clef currentClef = null;
            int groupIndex = 0;
            List<OutputChordSymbol> beamedGroup = null;
            foreach(var noteObject in NoteObjects)
            {
                if(noteObject is OutputChordSymbol chord)
                {
                    if(chord.BeamBlockDef != null)
                    {
                        M.Assert(currentClef != null);
                        beamedGroup = beamedGroups[groupIndex++];
                        double beamThickness = pageFormat.BeamThickness;
                        double beamStrokeThickness = pageFormat.StafflineStemStrokeWidthVBPX;
                        chord.BeamBlock = new BeamBlock(currentClef, beamedGroup, this.StemDirection, beamThickness, beamStrokeThickness);
                    }
                    else if(chord.IsBeamEnd)
                    {
                        beamedGroup = null;
                    }
                    else if(beamedGroup == null)
                    {
                        M.Assert(currentClef != null);
                        if(this.StemDirection == VerticalDir.none)
                            chord.Stem.Direction = chord.DefaultStemDirection(currentClef);
                        else
                            chord.Stem.Direction = this.StemDirection;
                    }
                }

                if(noteObject is Clef clef)
                    currentClef = clef;
            }
        }

        private List<List<OutputChordSymbol>> GetBeamedGroups()
        {
            List<List<OutputChordSymbol>> beamedGroups = new List<List<OutputChordSymbol>>();


            bool inGroup = false;
            List<OutputChordSymbol> beamedGroup = null;
            for(var i = 0; i < NoteObjects.Count; i++)
            {
                var noteObject = NoteObjects[i];
                if(noteObject is OutputChordSymbol chordSymbol)
                {
                    if(chordSymbol.IsBeamStart)
                    {
                        M.Assert(inGroup == false);
                        inGroup = true;
                        beamedGroup = new List<OutputChordSymbol>
                        {
                            chordSymbol
                        };
                        beamedGroups.Add(beamedGroup);
                        chordSymbol.Stem.BeamContinues = true;
                    }
                    else if(chordSymbol.IsBeamEnd)
                    {
                        M.Assert(inGroup == true);
                        beamedGroup.Add(chordSymbol);
                        chordSymbol.Stem.BeamContinues = false;
                        inGroup = false;
                    }
                    else if(inGroup)
                    {
                        M.Assert(chordSymbol.DurationClass < DurationClass.crotchet);
                        beamedGroup.Add(chordSymbol);
                        chordSymbol.Stem.BeamContinues = true;
                    }
                }
            }

            M.Assert(inGroup == false); // Beamblocks may extend across Barlines, but not across Systems.

            return beamedGroups;
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
            HashSet<ChordSymbol> chordSymbolsThatStartBeamBlocks = FindChordSymbolsThatStartBeamBlocks();
            foreach(var chordSymbol in chordSymbolsThatStartBeamBlocks)
            {
                chordSymbol.FinalizeBeamBlock();
            }
        }

        public void RemoveBeamBlockBeams()
        {
            HashSet<ChordSymbol> chordSymbolsThatStartBeamBlocks = FindChordSymbolsThatStartBeamBlocks();
            foreach(var chordSymbol in chordSymbolsThatStartBeamBlocks)
            {
                chordSymbol.BeamBlock.Beams.Clear();
            }
        }

        private HashSet<ChordSymbol> FindChordSymbolsThatStartBeamBlocks()
        {
            HashSet<ChordSymbol> chordSymbolsThatStartBeamBlocks = new HashSet<ChordSymbol>();
            foreach(ChordSymbol chord in ChordSymbols)
            {
                if(chord.BeamBlockDef != null)
                    chordSymbolsThatStartBeamBlocks.Add(chord);
            }
            return chordSymbolsThatStartBeamBlocks;
        }

        #region Enumerators
        public IEnumerable Anchors
        {
            get
            {
                foreach(NoteObject noteObject in NoteObjects)
                {
                    if(noteObject is Anchor iHasDrawObjects)
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

        #region add slur templates
        /// <summary>
        /// All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
        /// Staves have not yet been moved from their original vertical position (so have their top staffline at y-coordinate = 0).
        /// first
        /// </summary>
        /// <param name="firstSlurInfos">List<(targetEventID, targetHeadID, slurIsOver, slurTemplateBeginY)></param>
        /// <param name="gap">The distance between two stafflines</param>
        /// <param name="slurLeftLimit">The start-x position of a slur that ends on the first chord in the voice</param>
        /// <param name="slurRightLimit">The end-x position of a slur that ends after the final barline in the voice</param>
        /// <returns>The first parameter, changed to contain new values.</returns>
        internal List<(string, string, bool, double)> AddSlurTemplates(List<(string, string, bool, double)> firstSlurInfos, double gap, double slurLeftLimit, double slurRightLimit)
        {
            double endAngle = Math.PI / 3; // 60°

            if(firstSlurInfos.Count > 0)
            {
                AddVoiceStartSlurTemplates(firstSlurInfos, endAngle, slurLeftLimit);
            }
            firstSlurInfos.Clear();


            for(var noteObjectIndex = 0; noteObjectIndex < NoteObjects.Count; noteObjectIndex++)
            {
                if(NoteObjects[noteObjectIndex] is OutputChordSymbol leftChord)
                {
                    if(leftChord.SlurDefs != null && leftChord.SlurDefs.Count > 0)
                    {
                        var headsTopDown = leftChord.HeadsTopDown;
                        var headsMetricsTopDown = ((ChordMetrics)leftChord.Metrics).HeadsMetricsTopDown;

                        foreach(var slurDef in leftChord.SlurDefs)
                        {
                            (HeadMetrics startNoteMetrics, HeadMetrics endNoteMetrics, string targetEventID, string targetHeadID) =
                                FindSlurHeadMetrics(headsTopDown, headsMetricsTopDown, slurDef, noteObjectIndex);
                            // endNote and targetHeadID are null if the target is not on this system.

                            bool isOver = (slurDef.Side == Orientation.up);

                            (double slurTemplateBeginX, double slurTemplateBeginY, double slurTemplateEndX, double slurTemplateEndY) =
                                GetSlurTemplateCoordinates(startNoteMetrics, endNoteMetrics, gap, isOver, endAngle, slurRightLimit);

                            leftChord.AddSlurTemplate(slurTemplateBeginX, slurTemplateBeginY, slurTemplateEndX, slurTemplateEndY, gap, isOver, endAngle);

                            if(endNoteMetrics == null)
                            {
                                // All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
                                // Staves have not yet been moved from their original vertical position (so have their top staffline at y-coordinate = 0).
                                M.Assert(Staff.Metrics.StafflinesTop == 0);
                                firstSlurInfos.Add((targetEventID, targetHeadID, slurDef.Side == Orientation.up, slurTemplateBeginY));
                            }
                        }
                    }
                }
            }

            return firstSlurInfos;
        }

        private void AddVoiceStartSlurTemplates(List<(string, string, bool, double)> firstSlurInfos, double endAngle, double slurTieLeftLimit)
        {
            var gap = M.PageFormat.GapVBPX;

            // All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
            // Staves have not yet been moved from their original vertical position (so have their top staffline at y-coordinate = 0).
            // This means that slurTemplateLeftY can be set below to the value of slurInfo.item4 set in this voice in the previous system.
            M.Assert(Staff.Metrics.StafflinesTop == 0);

            foreach(var slurTieInfo in firstSlurInfos)
            {
                var targetEventID = slurTieInfo.Item1;
                var targetHeadID = slurTieInfo.Item2;
                bool isOver = slurTieInfo.Item3;
                double slurTemplateLeftY = slurTieInfo.Item4;

                var targetChord = NoteObjects.Find(chord => chord is OutputChordSymbol outputChord && outputChord.EventID.Equals(targetEventID)) as OutputChordSymbol;
                M.Assert(targetChord != null, "The target chord must be in this voice. (Neither slurs nor ties can currently span more than one system break.)");

                var headsTopDown = targetChord.HeadsTopDown;
                var headsMetricsTopDown = ((ChordMetrics)targetChord.Metrics).HeadsMetricsTopDown;

                HeadMetrics headMetrics = FindTargetHeadMetrics(headsTopDown, headsMetricsTopDown, targetHeadID, isOver);

                (double slurTemplateEndX, double slurTemplateEndY) = GetStartSlurTieTemplateEndCoordinates(true, headMetrics, gap, isOver);
                targetChord.AddSlurTemplate(slurTieLeftLimit, slurTemplateLeftY, slurTemplateEndX, slurTemplateEndY, gap, isOver, endAngle);
            }
        }


        /// <summary>
        /// If slurDef.endNote is not in this Voice, the returned endHeadMetrics and targetHeadID will be null.
        /// </summary>
        /// <returns></returns>
        private (HeadMetrics startHeadMetrics, HeadMetrics endHeadMetrics, string targetEventID, string targetHeadID)
            FindSlurHeadMetrics(List<Head> startHeadsTopDown, IReadOnlyList<HeadMetrics> startHeadsMetricsTopDown, SlurDef slurDef, int noteObjectIndex)
        {
            HeadMetrics startHeadMetrics = null;
            Head endHead = null;
            HeadMetrics endHeadMetrics = null;
            string targetEventID = slurDef.TargetID;
            string targetHeadID = slurDef.EndNoteID; // can be null

            var startHeadID = slurDef.StartNoteID;
            if(startHeadID == null)
            {
                startHeadMetrics = (slurDef.Side == Orientation.up) ? startHeadsMetricsTopDown[0] : startHeadsMetricsTopDown[startHeadsMetricsTopDown.Count - 1];
            }
            else
            {
                var startHead = startHeadsTopDown.Find(head => head.ID.Equals(startHeadID));
                startHeadMetrics = startHeadsMetricsTopDown[startHeadsTopDown.FindIndex(head => head == startHead)];
            }

            var outputChordSymbol = FindNextChord(noteObjectIndex++, true);
            while(outputChordSymbol != null)
            {
                if(outputChordSymbol.EventID.Equals(targetEventID))
                {
                    var headsTopDown = outputChordSymbol.HeadsTopDown;
                    var headsMetricsTopDown = ((ChordMetrics)outputChordSymbol.Metrics).HeadsMetricsTopDown;
                    if(targetHeadID == null)
                    {
                        endHead = (slurDef.Side == Orientation.up) ? headsTopDown[0] : headsTopDown[headsTopDown.Count - 1];
                    }
                    else
                    {
                        endHead = headsTopDown.Find(head => head.ID.Equals(targetHeadID));
                    }
                    endHeadMetrics = headsMetricsTopDown[headsTopDown.FindIndex(head => head == endHead)];
                    break;
                }
                outputChordSymbol = FindNextChord(noteObjectIndex++, true);
            }

            return (startHeadMetrics, endHeadMetrics, targetEventID, targetHeadID);
        }

        /// <summary>
        /// Returns null or the next OutputChordSymbol in the voice.
        /// null is returned if there is no next OutputChordSymbol in the voice, or ignoreRests is false and
        /// an OutputRestSymbol occurs before the next OutputChordSymbol. 
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="noteObjectIndex"></param>
        /// <returns></returns>
        private OutputChordSymbol FindNextChord(int noteObjectIndex, bool ignoreRests)
        {
            OutputChordSymbol rval = null;
            for(var i = noteObjectIndex + 1; i < NoteObjects.Count; i++)
            {
                if(NoteObjects[i] is OutputRestSymbol && ignoreRests == false)
                {
                    break;
                }
                if(NoteObjects[i] is OutputChordSymbol ocs)
                {
                    rval = ocs;
                    break;
                }
            }
            return rval;
        }

        private HeadMetrics FindTargetHeadMetrics(List<Head> headsTopDown, IReadOnlyList<HeadMetrics> headsMetricsTopDown, string targetHeadID, bool isOver)
        {
            HeadMetrics headMetrics = null;
            if(targetHeadID == null)
            {
                headMetrics = (isOver) ? headsMetricsTopDown[0] : headsMetricsTopDown[headsMetricsTopDown.Count - 1];
            }
            else
            {
                var head = headsTopDown.Find(h => h.ID == targetHeadID);
                headMetrics = headsMetricsTopDown[headsTopDown.FindIndex(h => h == head)];
            }
            M.Assert(headMetrics != null);

            return headMetrics;
        }

        private (double slurBeginX, double slurBeginY, double slurEndX, double slurEndY)
            GetSlurTemplateCoordinates(HeadMetrics startHeadMetrics, HeadMetrics endHeadMetrics, double gap, bool isOver, double endAngle, double slurTieRightLimit)
        {
            // dx an dy will be wrt centre of notehead

            double dy = gap * 0.8;
            double dx = dy / Math.Tan(endAngle); // same angle as end control points in template

            var beginCentreX = ((startHeadMetrics.Right + startHeadMetrics.Left) / 2);
            var beginCentreY = ((startHeadMetrics.Top + startHeadMetrics.Bottom) / 2);
            var slurBeginX = beginCentreX + dx;
            var slurBeginY = (isOver) ? beginCentreY - dy : beginCentreY + dy;
            double slurEndX;
            double slurEndY;
            if(endHeadMetrics != null)
            {
                var endCentreX = ((endHeadMetrics.Right + endHeadMetrics.Left) / 2);
                var endCentreY = ((endHeadMetrics.Top + endHeadMetrics.Bottom) / 2);
                slurEndX = endCentreX - dx;
                slurEndY = (isOver) ? endCentreY - dy : endCentreY + dy;
            }
            else
            {
                slurEndX = slurTieRightLimit;
                slurEndY = slurBeginY;
            }

            return (slurBeginX, slurBeginY, slurEndX, slurEndY);
        }

        #endregion add slur templates

        #region add tie templates

        /// <summary>
        /// All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
        /// Staves have not yet been moved from their original vertical position (so have their top staffline at y-coordinate = 0).
        /// first
        /// </summary>
        /// <param name="firstTieInfos">List<(targetPitch, isOver)></param>
        /// <param name="gap">The distance between two stafflines</param>
        /// <param name="tieLeftLimit">The start-x position of a tie that ends on the first chord in the voice</param>
        /// <param name="tieRightLimit">The end-x position of a tie that ends after the final barline in the voice</param>
        /// <returns>The first parameter, changed to contain new values.</returns>
        internal List<(string, bool)> AddTieTemplates(List<(string, bool)> firstTieInfos, double gap, double tieLeftLimit, double tieRightLimit)
        {
            if(firstTieInfos.Count > 0)
            {
                AddVoiceStartTieTemplates(firstTieInfos, tieLeftLimit);
            }
            firstTieInfos.Clear();

            Clef clef = null;
            for(var noteObjectIndex = 0; noteObjectIndex < NoteObjects.Count; noteObjectIndex++)
            {
                if(NoteObjects[noteObjectIndex] is Clef)
                {
                    clef = NoteObjects[noteObjectIndex] as Clef;
                }
                else if(NoteObjects[noteObjectIndex] is OutputChordSymbol leftChord)
                {
                    var leftHeadsTopDown = leftChord.HeadsTopDown;
                    var leftHeadsMetricsTopDown = ((ChordMetrics)leftChord.Metrics).HeadsMetricsTopDown;                    
                    StemMetrics stemMetrics = ((ChordMetrics)leftChord.Metrics).StemMetrics;
                    VerticalDir leftStemDir = (stemMetrics == null) ? leftChord.DefaultStemDirection(clef): stemMetrics.VerticalDir;

                    for(var headIndex = 0; headIndex < leftHeadsTopDown.Count; ++headIndex)
                    {
                        var leftHead = leftHeadsTopDown[headIndex];
                        var leftHeadMetrics = leftHeadsMetricsTopDown[headIndex];
                        var tied = leftHead.Tied;
                        if(tied != null)
                        {
                            HeadMetrics rightHeadMetrics = null;
                            bool isOver = GetTieIsOver(tied, leftHeadsMetricsTopDown, leftStemDir, headIndex);
                            var nextChord = FindNextChord(noteObjectIndex, false);
                            if(nextChord == null)
                            {
                                // All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
                                // Staves have not yet been moved from their original vertical position
                                // (so leftHeadMetrics.top, .centreY and .bottom are valid in the next system too.
                                M.Assert(Staff.Metrics.StafflinesTop == 0);

                                firstTieInfos.Add((leftHead.Pitch, isOver));
                            }
                            else
                            {
                                rightHeadMetrics = FindRightHeadMetrics(leftHead.Pitch, nextChord);
                            }

                            // rightHeadMetrics is null if the tie is to next system (i.e. ends at tieRightLimit)
                            (double tieTemplateBeginX, double tieTemplateEndX, double templateY) =
                                GetTieTemplateCoordinates(leftHeadMetrics, rightHeadMetrics, gap, isOver, tieLeftLimit, tieRightLimit);

                            leftChord.AddTieTemplate(tieTemplateBeginX, tieTemplateEndX, templateY, gap, isOver);
                        }
                    }
                }
            }

            return firstTieInfos;
        }

        /// <summary>
        /// This algorithm currently ignores
        /// 1. chords with noteheads on both sides of the stem.
        /// 2. the height wrt staff of stemless, single note chords. (?)
        /// </summary>
        /// <param name="tied"></param>
        /// <param name="leftHeadsMetricsTopDown"></param>
        /// <param name="leftStemDir"></param>
        /// <param name="headIndex"></param>
        /// <returns></returns>
        private bool GetTieIsOver(Tied tied, IReadOnlyList<HeadMetrics> leftHeadsMetricsTopDown, VerticalDir leftStemDir, int headIndex)
        {
            bool isOver;

            if(tied.Side != null)
            {
                isOver = (tied.Side == Orientation.up) ;
            }
            else
            {
                if(leftStemDir == VerticalDir.none || (leftHeadsMetricsTopDown.Count > 1))
                {
                    isOver = (headIndex <= leftHeadsMetricsTopDown.Count / 2);
                }
                else // leftHeadsMetricsTopDown.Count == 1
                {
                    isOver = (leftStemDir == VerticalDir.down);
                }
            }

            return isOver;
        }

        private HeadMetrics FindRightHeadMetrics(string targetPitch, OutputChordSymbol nextChord)
        {
            HeadMetrics rval = null;

            var headsTopDown = nextChord.HeadsTopDown;
            var headMetricsTopDown = ((ChordMetrics)nextChord.Metrics).HeadsMetricsTopDown;

            for(int i = 0; i < headsTopDown.Count; ++i)
            {
                if(headsTopDown[i].Pitch.Equals(targetPitch))
                {
                    rval = headMetricsTopDown[i];
                    break;
                }
            }

            return rval;
        }

        private (double tieTemplateBeginX, double tieTemplateEndX, double templateY)
            GetTieTemplateCoordinates(HeadMetrics leftHeadMetrics, HeadMetrics rightHeadMetrics, double gap, bool isOver, double tieLeftlimit, double tieRightLimit)
        {
            M.Assert(leftHeadMetrics != null || rightHeadMetrics != null);

            double dx = gap * 0.7;
            double dy = (isOver) ? -dx : dx;

            double leftHeadCentreX = (leftHeadMetrics != null) ? (leftHeadMetrics.Left + leftHeadMetrics.Right) / 2 : 0;
            double leftHeadCentreY = (leftHeadMetrics != null) ? (leftHeadMetrics.Top + leftHeadMetrics.Bottom) / 2 : 0;
            double rightHeadCentreX = (rightHeadMetrics != null) ? (rightHeadMetrics.Left + rightHeadMetrics.Right) / 2 : 0;
            double rightHeadCentreY = (rightHeadMetrics != null) ? (rightHeadMetrics.Top + rightHeadMetrics.Bottom) / 2 : 0;

            double tieTemplateBeginX = (leftHeadMetrics == null) ? tieLeftlimit : leftHeadCentreX + dx;
            double tieTemplateEndX = (rightHeadMetrics == null) ? tieRightLimit : rightHeadCentreX - dx;
            double templateY = (leftHeadMetrics == null) ? rightHeadCentreY + dy : leftHeadCentreY + dy;

            return (tieTemplateBeginX, tieTemplateEndX, templateY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstTieInfos">List<(string targetPitch, Head leftHead, HeadMetrics leftHeadMetrics, bool tieIsOver)></param>
        /// <param name="tieLeftLimit"></param>
        private void AddVoiceStartTieTemplates(List<(string, bool)> firstTieInfos, double tieLeftLimit)
        {
            var gap = M.PageFormat.GapVBPX;

            // All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
            // Staves have not yet been moved from their original vertical position (so have their top staffline at y-coordinate = 0).
            // This means that slurTemplateLeftY can be set below to the value of slurInfo.item4 set in this voice in the previous system.
            M.Assert(Staff.Metrics.StafflinesTop == 0);

            foreach(var firstTieInfo in firstTieInfos)
            {
                string targetPitch = firstTieInfo.Item1;
                bool isOver = firstTieInfo.Item2;

                var targetChord = NoteObjects.Find(chord => chord is OutputChordSymbol) as OutputChordSymbol;
                M.Assert(targetChord != null, "The target chord must be in this voice. (Neither slurs nor ties can currently span more than one system break.)");

                HeadMetrics targetHeadMetrics = FindRightHeadMetrics(targetPitch, targetChord);

                (double tieTemplateEndX, double tieTemplateEndY) = GetStartSlurTieTemplateEndCoordinates(false, targetHeadMetrics, gap, isOver);
                targetChord.AddTieTemplate(tieLeftLimit, tieTemplateEndX, tieTemplateEndY, gap, isOver);
            }
        }

        #endregion add tie templates

        #region  functions used for both slurs and ties

        private (double endX, double endY) GetStartSlurTieTemplateEndCoordinates(bool isSlur, HeadMetrics headMetrics, double gap, bool isOver)
        {
            double dx;
            double dy;

            if(isSlur)
            {
                dy = gap * 0.75;
                dx = dy / 1.5; // The same angle as the control line (arcTan(3/2))
            }
            else // is tie
            {
                dx = gap * 0.65;
                dy = dx;
            }

            var endCentreX = ((headMetrics.Right + headMetrics.Left) / 2);
            var endCentreY = ((headMetrics.Top + headMetrics.Bottom) / 2);
            var endX = endCentreX - dx;
            var endY = (isOver) ? endCentreY - dy : endCentreY + dy;

            return (endX, endY);
        }

        #endregion functions used for both slurs and ties

        /// <summary>
        /// All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
        /// Staves have not yet been moved from their original vertical position (so have their top staffline at y-coordinate = 0).
        /// first
        /// </summary>
        internal void AddTuplets(Graphics graphics)
        {
            for(var noteObjectIndex = 0; noteObjectIndex < NoteObjects.Count; noteObjectIndex++)
            {
                NoteObject noteObject = NoteObjects[noteObjectIndex];
                if(noteObject is OutputChordSymbol || noteObject is OutputRestSymbol)
                {
                    DurationSymbol dSymbol = (DurationSymbol) noteObject;
                    if(dSymbol.TupletDefs != null)
                    {
                        foreach(TupletDef tupletDef in dSymbol.TupletDefs)
                        {
                            Tuplet tuplet = new Tuplet(graphics, tupletDef, NoteObjects, noteObjectIndex);

                            if(dSymbol.Metrics is ChordMetrics chordMetrics)
                            {
                                chordMetrics.AddTuplet(tuplet);
                            }
                            else if(dSymbol.Metrics is RestMetrics restMetrics)
                            {
                                restMetrics.AddTuplet(tuplet);
                            }
                            
                        }
                    }
                }
            }
        }
    }
}

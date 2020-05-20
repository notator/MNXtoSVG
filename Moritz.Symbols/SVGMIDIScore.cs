using System;
using System.Collections.Generic;
using Moritz.Spec;
using Moritz.Xml;
using MNX.Globals;
using MNX.Common;
using System.IO;

namespace Moritz.Symbols
{
	public class SVGMIDIScore : SvgScore
    {
        public SVGMIDIScore(string targetFolder, MNXCommon mnxCommon, Form1Data form1Data)
            : base(targetFolder, form1Data.FileNameWithoutExtension, form1Data.Options)
        {
            this.MetadataWithDate = new MetadataWithDate()
            {
                Title = form1Data.Metadata.Title,
                Author = form1Data.Metadata.Author,
                Keywords = form1Data.Metadata.Keywords,
                Comment = form1Data.Metadata.Comment,
                Date = M.NowString
            };

            PageFormat = new PageFormat(form1Data, mnxCommon.VoicesPerStaffPerPart);
            Notator = new Notator(PageFormat);

            List<Bar> bars = GetBars(mnxCommon);

            CheckBars(bars);

            CreateSystems(bars, form1Data);

            string filePath = null;
            if(form1Data.Options.WriteScrollScore)
            {
                PageFormat.BottomVBPX = GetNewBottomVBPX(Systems);
                PageFormat.BottomMarginPosVBPX = (int)(PageFormat.BottomVBPX - PageFormat.DefaultDistanceBetweenSystemsVBPX);
                filePath = SaveSVGScrollScore(!form1Data.Options.IncludeMIDIData, form1Data.Options.WritePage1Titles);
            }
            else
            {
                filePath = SaveMultiPageScore(!form1Data.Options.IncludeMIDIData, form1Data.Options.WritePage1Titles);
            }

            // Opens the score in the program which is set by the system to open .svg or .html files.
            global::System.Diagnostics.Process.Start(filePath);

        }

        private List<Bar> GetBars(MNXCommon mnxCommon)
        {
            var bars = new List<Bar>();
            List<List<IUniqueDef>> globalIUDsPerMeasure = mnxCommon.Global.GetGlobalIUDsPerMeasure();
            var midiChannelsPerStaff = PageFormat.MIDIChannelsPerStaff;

            var absSeqMsPosition = 0;
            for(var measureIndex = 0; measureIndex < globalIUDsPerMeasure.Count; measureIndex++)
            {
                var midiChannelIndexPerOutputVoice = new List<int>();
                List<Trk> trks = new List<Trk>();
                List<IUniqueDef> globalIUDs = globalIUDsPerMeasure[measureIndex];
                foreach(var part in mnxCommon.Parts)
                {
                    var measure = part.Measures[measureIndex];
                    var voicesPerStaff = part.VoicesPerStaff;
                    var nStaves = voicesPerStaff.Count;

                    for(var staffIndex = 0; staffIndex < nStaves; ++staffIndex)
                    {
                        var nVoices = voicesPerStaff[staffIndex];
                        for(var voiceIndex = 0; voiceIndex < nVoices; voiceIndex++)
                        {
                            Sequence sequence = measure.Sequences[voiceIndex];
                            List<IUniqueDef> seqIUDs = sequence.SetMsDurationsAndGetIUniqueDefs(PageFormat.MillisecondsPerTick);
                            if(voiceIndex == 0)
                            {
                                InsertGlobalIUDsInSeqIUDs(globalIUDs, seqIUDs);
                            }
                            var midiChannel = midiChannelsPerStaff[staffIndex][voiceIndex];
                            midiChannelIndexPerOutputVoice.Add(midiChannel);
                            Trk trk = new Trk(midiChannel, 0, seqIUDs);
                            trks.Add(trk);
                        }
                    }
                }
                Seq seq = new Seq(absSeqMsPosition, trks, midiChannelIndexPerOutputVoice);
                Bar bar = new Bar(seq);
                bars.Add(bar);
                absSeqMsPosition += seq.MsDuration;
            }

            return bars;
        }


        private void InsertGlobalIUDsInSeqIUDs(List<IUniqueDef> globalIUDs, List<IUniqueDef> seqIUDs)
        {
            if(globalIUDs.Find(obj => obj is MNX.Common.TimeSignature) is MNX.Common.TimeSignature timeSignature)
            {
                int insertIndex = (seqIUDs[0] is MNX.Common.Clef) ? 1 : 0;
                seqIUDs.Insert(insertIndex, timeSignature as IUniqueDef);
            }

            if(globalIUDs.Find(obj => obj is MNX.Common.KeySignature) is MNX.Common.KeySignature keySignature)
            {
                int insertIndex = (seqIUDs[0] is MNX.Common.Clef) ? 1 : 0;
                seqIUDs.Insert(insertIndex, keySignature as IUniqueDef);
            }
        }


        #region delete
        //private void GetVoiceDefs(MNXCommon mnxCommon, out List<VoiceDef> voiceDefs, out List<int> endBarlineMsPositionPerBar)
        //{
        //    List<List<IUniqueDef>> globalIUDsPerMeasure = mnxCommon.Global.GetGlobalIUDsPerMeasure();

        //    List<List<Trk>> Tracks = new List<List<Trk>>();
        //    foreach(var part in mnxCommon.Parts)
        //    {
        //        int nTracks = part.Measures[0].Sequences.Count;
        //        for(var i = 0; i < nTracks; i++)
        //        {
        //            List<Trk> track = new List<Trk>();
        //            for(int measureIndex = 0; measureIndex < part.Measures.Count; measureIndex++)
        //            {
        //                List<IUniqueDef> globalIUDs = globalIUDsPerMeasure[measureIndex];
        //                var measure = part.Measures[measureIndex];
        //                Sequence sequence = measure.Sequences[i];
        //                List<IUniqueDef> seqIUDs = sequence.SetMsDurationsAndGetIUniqueDefs(PageFormat.MillisecondsPerTick);
        //                InsertGlobalIUDsInIUDs(globalIUDs, seqIUDs);
        //                Trk newTrk = new Trk(currentMIDIChannel, 0, seqIUDs);
        //                track.Add(newTrk);
        //            }
        //            Tracks.Add(track);
        //            currentMIDIChannel++;
        //        }
        //    }

        //    endBarlineMsPositionPerBar = GetEndBarlineMsPositionPerBar(Tracks[0]);
        //    voiceDefs = GetVoiceDefs(Tracks);
        //}


        //private List<int> GetEndBarlineMsPositionPerBar(List<Trk> trks)
        //{
        //    List<int> rval = new List<int>();
        //    int currentPosition = 0;
        //    foreach(var trk in trks)
        //    {
        //        currentPosition += trk.MsDuration;
        //        rval.Add(currentPosition);
        //    }
        //    return rval;
        //}

        ///// <summary>
        ///// This function consumes its argumant.
        ///// </summary>
        ///// <param name="tracks"></param>
        ///// <returns></returns>
        //private List<VoiceDef> GetVoiceDefs(List<List<Trk>> tracks)
        //{
        //    var rval = new List<VoiceDef>();

        //    foreach(var trkList in tracks)
        //    {
        //        Trk trk = trkList[0];
        //        for(var i = 1; i < trkList.Count; i++)
        //        {
        //            trk.AddRange(trkList[i]);
        //        }
        //        rval.Add(trk);
        //    }
        //    return rval;
        //}

        ///************************************************************/

        ///// <summary>
        ///// Used by GetBars(...) below.
        ///// </summary>
        //private List<VoiceDef> _voiceDefs = null;

        ///// Converts the List of VoiceDef to a list of Bar. The VoiceDefs are Cloned, and the Clones are consumed.
        ///// Uses the argument barline msPositions as the EndBarlines of the returned bars (which don't contain barlines).
        ///// An exception is thrown if:
        /////    1) the first argument value is less than or equal to 0.
        /////    2) the argument contains duplicate msPositions.
        /////    3) the argument is not in ascending order.
        /////    4) a Trk.MsPositionReContainer is not 0.
        /////    5) an msPosition is not the endMsPosition of any IUniqueDef in the seq.
        //private List<Bar> GetBars(IReadOnlyList<VoiceDef> voiceDefs, IReadOnlyList<int> msPositionPerBar)
        //{
        //    _voiceDefs = new List<VoiceDef>();
        //    foreach(var voiceDef in voiceDefs)
        //    {
        //        _voiceDefs.Add(((Trk)voiceDef).Clone() as VoiceDef);
        //    }

        //    CheckBarlineMsPositions(msPositionPerBar);

        //    List<int> barMsDurations = new List<int>();
        //    int startMsPos = 0;
        //    for(int i = 0; i < msPositionPerBar.Count; i++)
        //    {
        //        int endMsPos = msPositionPerBar[i];
        //        barMsDurations.Add(endMsPos - startMsPos);
        //        startMsPos = endMsPos;
        //    }

        //    List<Bar> bars = new List<Bar>();
        //    int totalDurationBeforePop = voiceDefs[0].MsDuration;
        //    List<int> midiChannelPerOutputVoice = new List<int>();
        //    for(var i = 0; i < voiceDefs.Count; i++)
        //    {
        //        midiChannelPerOutputVoice.Add(i);
        //    }
        //    List<Trk> trks = new List<Trk>();
        //    foreach(var voiceDef in voiceDefs)
        //    {
        //        trks.Add(voiceDef as Trk);
        //    }

        //    Seq seq = new Seq(0, trks, midiChannelPerOutputVoice);
        //    Bar remainingBar = new Bar(seq);

        //    foreach(int barMsDuration in barMsDurations)
        //    {
        //        Tuple<Bar, Bar> rTuple = PopBar(remainingBar, barMsDuration);
        //        Bar poppedBar = rTuple.Item1;
        //        remainingBar = rTuple.Item2; // null after the last pop.

        //        M.Assert(poppedBar.MsDuration == barMsDuration);
        //        if(remainingBar != null)
        //        {
        //            M.Assert(poppedBar.MsDuration + remainingBar.MsDuration == totalDurationBeforePop);
        //            totalDurationBeforePop = remainingBar.MsDuration;
        //        }
        //        else
        //        {
        //            M.Assert(poppedBar.MsDuration == totalDurationBeforePop);
        //        }

        //        bars.Add(poppedBar);
        //    }

        //    return bars;
        //}
        

        ///// <summary>
        ///// Returns a Tuple in which Item1 is the popped bar, Item2 is the remaining part of the input bar.
        ///// The popped bar has a list of voiceDefs containing the IUniqueDefs that
        ///// begin within barDuration. These IUniqueDefs are removed from the current bar before returning it as Item2.
        ///// MidiRestDefs and MidiChordDefs are split as necessary, so that when this
        ///// function returns, both the popped bar and the current bar contain voiceDefs
        ///// having the same msDuration. i.e.: Both the popped bar and the remaining bar "add up".
        ///// </summary>
        ///// <param name ="bar">The bar fron which the bar is popped.</param>
        ///// <param name="barMsDuration">The duration of the popped bar.</param>
        ///// <returns>The popped bar</returns>
        //private Tuple<Bar, Bar> PopBar(Bar bar, int barMsDuration)
        //{
        //    M.Assert(barMsDuration > 0);

        //    if(barMsDuration == bar.MsDuration)
        //    {
        //        return new Tuple<Bar, Bar>(bar, null);
        //    }

        //    Bar poppedBar = new Bar();
        //    Bar remainingBar = new Bar();
        //    int thisMsDuration = _voiceDefs[0].MsDuration;

        //    VoiceDef poppedBarVoice;
        //    VoiceDef remainingBarVoice;
        //    foreach(VoiceDef voiceDef in bar.VoiceDefs)
        //    {
        //        poppedBarVoice = new Trk(voiceDef.MidiChannel) { Container = poppedBar };
        //        poppedBar.VoiceDefs.Add(poppedBarVoice);
        //        remainingBarVoice = new Trk(voiceDef.MidiChannel) { Container = remainingBar };
        //        remainingBar.VoiceDefs.Add(remainingBarVoice);

        //        foreach(IUniqueDef iud in voiceDef.UniqueDefs)
        //        {
        //            int iudMsDuration = iud.MsDuration;
        //            int iudStartPos = iud.MsPositionReFirstUD;
        //            int iudEndPos = iudStartPos + iudMsDuration;

        //            if(iudStartPos >= barMsDuration)
        //            {
        //                if(iud is ClefDef && iudStartPos == barMsDuration)
        //                {
        //                    poppedBarVoice.UniqueDefs.Add(iud);
        //                }
        //                else
        //                {
        //                    remainingBarVoice.UniqueDefs.Add(iud);
        //                }
        //            }
        //            else if(iudEndPos > barMsDuration)
        //            {
        //                int durationBeforeBarline = barMsDuration - iudStartPos;
        //                int durationAfterBarline = iudEndPos - barMsDuration;
        //                if(iud is MidiRestDef)
        //                {
        //                    // This is a rest. Split it.
        //                    MidiRestDef firstRestHalf = new MidiRestDef(iudStartPos, durationBeforeBarline);
        //                    poppedBarVoice.UniqueDefs.Add(firstRestHalf);

        //                    MidiRestDef secondRestHalf = new MidiRestDef(barMsDuration, durationAfterBarline);
        //                    remainingBarVoice.UniqueDefs.Add(secondRestHalf);
        //                }
        //                if(iud is CautionaryChordDef)
        //                {
        //                    M.Assert(false, "There shouldnt be any cautionary chords here.");
        //                    // This error can happen if an attempt is made to set barlines too close together,
        //                    // i.e. (I think) if an attempt is made to create a bar that contains nothing... 
        //                }
        //                else if(iud is MidiChordDef)
        //                {
        //                    IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
        //                    uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
        //                    poppedBarVoice.UniqueDefs.Add(uniqueChordDef);

        //                    M.Assert(remainingBarVoice.UniqueDefs.Count == 0);
        //                    CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
        //                    remainingBarVoice.UniqueDefs.Add(ccd);
        //                }
        //            }
        //            else
        //            {
        //                M.Assert(iudEndPos <= barMsDuration && iudStartPos >= 0);
        //                poppedBarVoice.UniqueDefs.Add(iud);
        //            }
        //        }
        //    }

        //    poppedBar.AbsMsPosition = remainingBar.AbsMsPosition;
        //    poppedBar.AssertConsistency();
        //    if(remainingBar != null)
        //    {
        //        remainingBar.AbsMsPosition += barMsDuration;
        //        remainingBar.SetMsPositionsReFirstUD();
        //        remainingBar.AssertConsistency();
        //    }

        //    return new Tuple<Bar, Bar>(poppedBar, remainingBar);
        //}

        ///// <summary>
        ///// An exception is thrown if:
        /////    1) the first argument value is less than or equal to 0.
        /////    2) the argument contains duplicate msPositions.
        /////    3) the argument is not in ascending order.
        /////    4) a VoiceDef.MsPositionReContainer is not 0.
        /////    5) if the bar contains InputVoiceDefs, an msPosition is not the endMsPosition of any IUniqueDef in the InputVoiceDefs
        /////       else if an msPosition is not the endMsPosition of any IUniqueDef in the Trks.
        ///// </summary>
        //private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReThisBar)
        //{
        //    M.Assert(barlineMsPositionsReThisBar[0] > 0, "The first msPosition must be greater than 0.");

        //    int msDuration = _voiceDefs[0].MsDuration;
        //    for(int i = 0; i < barlineMsPositionsReThisBar.Count; ++i)
        //    {
        //        int msPosition = barlineMsPositionsReThisBar[i];
        //        M.Assert(msPosition <= _voiceDefs[0].MsDuration);
        //        for(int j = i + 1; j < barlineMsPositionsReThisBar.Count; ++j)
        //        {
        //            M.Assert(msPosition != barlineMsPositionsReThisBar[j], "Error: Duplicate barline msPositions.");
        //        }
        //    }

        //    int currentMsPos = -1;
        //    foreach(int msPosition in barlineMsPositionsReThisBar)
        //    {
        //        M.Assert(msPosition > currentMsPos, "Value out of order.");
        //        currentMsPos = msPosition;
        //        bool found = false;
        //        for(int i = _voiceDefs.Count - 1; i >= 0; --i)
        //        {
        //            VoiceDef voiceDef = _voiceDefs[i];

        //            M.Assert(voiceDef.MsPositionReContainer == 0);

        //            foreach(IUniqueDef iud in voiceDef.UniqueDefs)
        //            {
        //                if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
        //                {
        //                    found = true;
        //                    break;
        //                }
        //            }
        //            if(found)
        //            {
        //                break;
        //            }
        //        }
        //        M.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef in a Trk.");
        //    }
        //}


        #endregion delete

        private int GetNewBottomVBPX(List<SvgSystem> Systems)
        {
            int frameHeight = PageFormat.TopMarginPage1VBPX + 20;
            foreach(SvgSystem system in Systems)
            {
                SystemMetrics sm = system.Metrics;
                frameHeight += (int)((sm.Bottom - sm.Top) + PageFormat.DefaultDistanceBetweenSystemsVBPX);
            }

            return frameHeight;
        }

        private void CreateSystems(List<Bar> bars, Form1Data form1Data)
        {
            // TODO (see SetScoreRegionsData() function already implemented below)
            //if(form1Data.RegionStartBarIndices != null)
            //{
            //    ScoreData = SetScoreRegionsData(bars, form1Data.RegionStartBarIndices);
            //} 

            /**********************************************************************/
            List<int> nVoicesPerStaff = GetNVoicesPerStaff(PageFormat.MIDIChannelsPerStaff);
            List<List<List<VoiceDef>>> voiceDefsPerStaffPerBar = GetVoiceDefsPerStaffPerBar(bars, nVoicesPerStaff);

            // If a staff has two voices (parallel sequences) then the first must be higher than the second.
            // The order is swapped here if necessary. Stem directions are set later, when real Staff objects exist.
            NormalizeTopToBottomVoiceOrder(voiceDefsPerStaffPerBar);

            // Ensure that all VoiceDefs shared by a Staff begin with the same Clef, KeySignature and TimeSignature.
            // Changes of Clef, KeySignature and TimeSignature over the course of the score are taken into account.
            // Small (cautionary) Clefs in lower voices have smallClef.IsVisible = false.
            InsertInitialIUDs(bars, voiceDefsPerStaffPerBar); // see Moritz: ComposableScore.cs line 63.

            CreateEmptySystems(voiceDefsPerStaffPerBar); // one system per bar

            if(PageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

                FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                /// The systems do not yet contain Metrics info.
                /// The systems are given Metrics inside the following function then justified internally,
                /// both horizontally and vertically.
                Notator.CreateMetricsAndJustifySystems(this.Systems);

            }

            CheckSystems(this.Systems);
        }

        private List<int> GetNVoicesPerStaff(IReadOnlyList<IReadOnlyList<int>> midiChannelsPerStaff)
        {
            List<int> nVoicesPerStaff = new List<int>();
            foreach(var list in midiChannelsPerStaff)
            {
                nVoicesPerStaff.Add(list.Count);
            }
            return nVoicesPerStaff;
        }

        private List<List<List<VoiceDef>>> GetVoiceDefsPerStaffPerBar(List<Bar> bars, List<int> nVoicesPerStaff)
        {
            var rval = new List<List<List<VoiceDef>>>();
            foreach(var bar in bars)
            {
                int voiceIndexInBar = 0;
                var voiceDefsPerStaff = new List<List<VoiceDef>>();
                for(var staffIndex = 0; staffIndex < nVoicesPerStaff.Count; staffIndex++)
                {
                    var staffVoiceDefs = new List<VoiceDef>();
                    var nStaffVoices = nVoicesPerStaff[staffIndex];
                    for(var staffVoiceIndex = 0; staffVoiceIndex < nStaffVoices; staffVoiceIndex++)
                    {
                        staffVoiceDefs.Add(bar.VoiceDefs[voiceIndexInBar++]);
                    }
                    voiceDefsPerStaff.Add(staffVoiceDefs);
                }
                rval.Add(voiceDefsPerStaff);
            }
            return rval;
        }

        /// <summary>
        /// If a staff has two voices (parallel sequences) then the first must be higher than the second.
        /// The order is swapped here if necessary. Stem directions are set later, when real Staff objects exist.
        /// </summary>
        /// <param name="voiceDefsPerStaffPerBar"></param>
        private void NormalizeTopToBottomVoiceOrder(List<List<List<VoiceDef>>> voiceDefsPerStaffPerBar)
        {
            foreach(var voiceDefsPerStaff in voiceDefsPerStaffPerBar)
            {
                foreach(var staffVoiceDefs in voiceDefsPerStaff)
                {
                    M.Assert(staffVoiceDefs.Count > 0 && staffVoiceDefs.Count < 3);
                    if(staffVoiceDefs.Count == 2)
                    {
                        double averagePitchV1 = GetAveragePitch(staffVoiceDefs[0]);
                        double averagePitchV2 = GetAveragePitch(staffVoiceDefs[1]);
                        if(averagePitchV1 < averagePitchV2)
                        {
                            var temp = staffVoiceDefs[0];
                            staffVoiceDefs[0] = staffVoiceDefs[1];
                            staffVoiceDefs[1] = temp;
                        }
                    }
                }
            }
        }

        private double GetAveragePitch(VoiceDef voiceDef)
        {
            double sum = 0;
            double nNotes = 0;
            foreach(IUniqueDef iud in voiceDef.UniqueDefs)
            {
                if(iud is Event e)
                {
                    foreach(var note in e.Notes)
                    {
                        var midiVal = M.MidiPitchDict[note.Pitch];
                        sum += midiVal;
                        nNotes++;
                    }
                }
            }

            return sum / nNotes;
        }

        // Ensure that all VoiceDefs shared by a Staff begin with the same Clef, KeySignature and TimeSignature.
        // Changes of Clef, KeySignature and TimeSignature over the course of the score are taken into account.
        // Small (cautionary) Clefs in lower voices have smallClef.IsVisible = false.
        private void InsertInitialIUDs(List<Bar> bars, List<List<List<VoiceDef>>> voiceDefsPerStaffPerBar)
        {
            List<List<IUniqueDef>> initialIUDsPerStaff = GetInitialIUDsPerStaff(voiceDefsPerStaffPerBar[0]);

            foreach(var voiceDefsPerStaff in voiceDefsPerStaffPerBar)
            {
                var nStaves = voiceDefsPerStaff.Count;
                for(var staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    var staffVoiceDefs = voiceDefsPerStaff[staffIndex];
                    InsertInitialIUDs(staffVoiceDefs, initialIUDsPerStaff[staffIndex]);
                    UpdateInitialIUDs(staffVoiceDefs[0], initialIUDsPerStaff[staffIndex]);
                }
            }
        }

        private List<List<IUniqueDef>> GetInitialIUDsPerStaff(List<List<VoiceDef>> voiceDefsPerStaff)
        {
            var initialIUDsPerStaff = new List<List<IUniqueDef>>();
            foreach(var staffVoiceDefs in voiceDefsPerStaff)
            {
                List<IUniqueDef> initialIUDs = new List<IUniqueDef>();
                foreach(var voiceDef in staffVoiceDefs)
                {
                    foreach(var iud in voiceDef.UniqueDefs)
                    {
                        if(!(iud is Event))
                        {
                            M.Assert(!initialIUDs.Contains(iud));
                            initialIUDs.Add(iud); // cloned later when inserted in the other voiceDef
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                AssertConsistency(initialIUDs);
                initialIUDsPerStaff.Add(initialIUDs);
            }
            return initialIUDsPerStaff;
        }

        /// <summary>
        /// Throws an exception if the following conditions are not met:
        /// The initialIUDs has a maximum Count of 3,
        /// The first IUniqueDef must be a Clef,
        /// The other IUniqueDefs may be 1 each of KeySignature and TimeSignature (in that order)
        /// </summary>
        private void AssertConsistency(List<IUniqueDef> initialIUDs)
        {
            M.Assert(initialIUDs.Count >= 1 && initialIUDs.Count < 4);
            M.Assert(initialIUDs[0] is MNX.Common.Clef);
            if(initialIUDs.Count > 1)
            {
                M.Assert(initialIUDs[1] is MNX.Common.KeySignature || initialIUDs[1] is MNX.Common.TimeSignature);
            }
            if(initialIUDs.Count > 2)
            {
                M.Assert(initialIUDs[1] is MNX.Common.KeySignature && initialIUDs[2] is MNX.Common.TimeSignature);
            }
        }

        private void InsertInitialIUDs(List<VoiceDef> staffVoiceDefs, List<IUniqueDef> initialIUDs)
        {
            var clef = initialIUDs[0];
            foreach(var voiceDef in staffVoiceDefs)
            {
                var voiceDefIUDs = voiceDef.UniqueDefs;
                if(voiceDefIUDs.Contains(clef))
                {
                    foreach(var iud1 in initialIUDs)
                    {
                        M.Assert(voiceDefIUDs.Contains(iud1));
                    }
                }
                else
                {
                    M.Assert(voiceDefIUDs[0] is Event);
                    for(int i = initialIUDs.Count - 1; i >= 0; i--)
                    {
                        voiceDefIUDs.Insert(0, (IUniqueDef)initialIUDs[i].Clone());
                    }
                }
            }
        }

        private void UpdateInitialIUDs(VoiceDef voiceDef, List<IUniqueDef> initialIUDs)
        {
            var initialTimeSig = initialIUDs.Find(obj => obj is MNX.Common.TimeSignature);
            if(initialTimeSig != null)
            {
                initialIUDs.Remove(initialTimeSig);
            }
            MNX.Common.Clef finalClef = null;
            MNX.Common.KeySignature finalKeySig = null;
            MNX.Common.TimeSignature finalTimeSig = null;
            foreach(var iud in voiceDef.UniqueDefs)
            {
                if(iud is MNX.Common.Clef lastClef)
                {
                    finalClef = lastClef;
                }
                else if(iud is MNX.Common.KeySignature lastKeySig)
                {
                    finalKeySig = lastKeySig;
                }
                else if(iud is MNX.Common.TimeSignature lastTimeSig)
                {
                    finalTimeSig = lastTimeSig;
                }
            }
            if(finalClef != initialIUDs[0])
            {
                initialIUDs[0] = finalClef;
            }
            if(finalKeySig != null)
            {
                if(initialIUDs.Count == 2 && initialIUDs[1] != finalKeySig)
                {
                    initialIUDs[1] = finalKeySig;
                }
                else if(initialIUDs.Count == 1)
                {
                    initialIUDs.Add(finalKeySig);
                }
            }
            if(finalTimeSig != null && finalTimeSig != initialTimeSig)
            {
                initialIUDs.Add(finalTimeSig);
            }
        }

        public ScoreData SetScoreRegionsData(List<Bar> bars, List<int> regionStartBarIndices)
        {
            List<(int index, int msPosition)> regionBorderlines = GetRegionBarlineIndexMsPosList(bars, regionStartBarIndices);

            /** start from Tombeau 1 ******************************
 
            // Each regionBorderline consists of a bar's index and its msPositionInScore.
            // The finalBarline is also included, so regionBorderlines.Count is 1 + RegionStartBarIndices.Count.

            RegionDef rd1 = new RegionDef("A", regionBorderlines[0], regionBorderlines[1]);
            RegionDef rd2 = new RegionDef("B", regionBorderlines[1], regionBorderlines[3]);
            RegionDef rd3 = new RegionDef("C", regionBorderlines[1], regionBorderlines[2]);
            RegionDef rd4 = new RegionDef("D", regionBorderlines[3], regionBorderlines[5]);
            RegionDef rd5 = new RegionDef("E", regionBorderlines[4], regionBorderlines[5]);

            List<RegionDef> regionDefs = new List<RegionDef>() { rd1, rd2, rd3, rd4, rd5 };

            RegionSequence regionSequence = new RegionSequence(regionDefs, "ABCADEA");
            
            ************************ end from tombeau 1 **/

            RegionSequence regionSequence = null;
            ScoreData scoreData = new ScoreData(regionSequence);

            return scoreData;
        }

        /// <summary>
        /// Returns a list of (index, msPosition) KeyValuePairs.
        /// These are the (index, msPosition) of the barlines at which regions begin, and the (index, msPosition) of the final barline.
        /// The first KeyValuePair is (0,0), the last is the (index, msPosition) for the final barline in the score.
        /// The number of entries in the returned list is therefore 1 + bars.Count.
        /// </summary>
        private List<(int index, int msPosition)> GetRegionBarlineIndexMsPosList(List<Bar> bars, List<int> regionStartBarIndices)
        {
            var rval = new List<(int index, int msPosition)>();

            int barlineMsPos = 0;
            int barsCount = bars.Count;
            for(int i = 0; i < barsCount; ++i)
            {
                if(regionStartBarIndices.Contains(i))
                {
                    rval.Add((index: i, msPosition: barlineMsPos));
                }
                barlineMsPos += bars[i].MsDuration;
            }
            rval.Add((index: barsCount, msPosition: barlineMsPos));

            return rval;
        }

		private void CheckBars(List<Bar> bars)
		{
            string errorString = null;
			if(bars.Count == 0)
				errorString = "The algorithm has not created any bars!";
			else
			{
				errorString = BasicChecks(bars);
			}
			if(string.IsNullOrEmpty(errorString))
			{
				errorString = CheckCCSettings(bars);
			}
			M.Assert(string.IsNullOrEmpty(errorString), errorString);
		}
		#region private to CheckBars(...)
		private string BasicChecks(List<Bar> bars)
        {
			string errorString = null;
			//List<int> visibleLowerVoiceIndices = new List<int>();
			//Dictionary<int, string> upperVoiceClefDict = GetUpperVoiceClefDict(bars[0], PageFormat, /*sets*/ visibleLowerVoiceIndices);

			for (int barIndex = 0; barIndex < bars.Count; ++barIndex)
			{
				Bar bar = bars[barIndex];
				IReadOnlyList<VoiceDef> voiceDefs = bar.VoiceDefs;
				string barNumber = (barIndex + 1).ToString();

				if(voiceDefs.Count == 0)
				{
					errorString = $"Bar {barNumber} contains no voices.";
					break;
				}
				if(!(voiceDefs[0] is Trk))
				{
					errorString = "The top (first) voice in every bar must be an output voice.";
					break;
				}

				for(int voiceIndex = 0; voiceIndex < voiceDefs.Count; ++voiceIndex)
				{
					VoiceDef voiceDef = voiceDefs[voiceIndex];
					string voiceNumber = (voiceIndex + 1).ToString();
					if(voiceDef.UniqueDefs.Count == 0)
					{
						errorString = $"Voice number {voiceNumber} in Bar {barNumber} has an empty UniqueDefs list.";
						break;
					}
				}

				errorString = CheckThatLowerVoicesHaveNoSmallClefs(voiceDefs);

				if(!string.IsNullOrEmpty(errorString))
					break;
			}
			return errorString;
		}

		private string CheckThatLowerVoicesHaveNoSmallClefs(IReadOnlyList<VoiceDef> voiceDefs)
		{
			string errorString = "";

			List<int> lowerVoiceIndices = GetLowerVoiceIndices();

			foreach(int lowerVoiceIndex in lowerVoiceIndices)
			{
				var uniqueDefs = voiceDefs[lowerVoiceIndex].UniqueDefs;
				foreach(IUniqueDef iud in uniqueDefs)
				{
					if(iud is ClefDef)
					{
						errorString = "Small Clefs may not be defined for lower voices on a staff.";
						break;
					}
				}
				if(!string.IsNullOrEmpty(errorString))
					break;
			}

			return errorString;
		}

		private List<int> GetLowerVoiceIndices()
		{
			List<int> lowerVoiceIndices = new List<int>();
			int voiceIndex = 0;
			
			IReadOnlyList<IReadOnlyList<int>> outputChPerStaff = PageFormat.MIDIChannelsPerStaff;

			for(int staffIndex = 0; staffIndex < outputChPerStaff.Count; ++staffIndex)
			{
				if(outputChPerStaff[staffIndex].Count > 1)
				{
					voiceIndex++;
					lowerVoiceIndices.Add(voiceIndex);
				}
				voiceIndex++;
			}

			return lowerVoiceIndices;
		}

		private int NOutputVoices(List<VoiceDef> bar1)
		{
			int nOutputVoices = 0;
			foreach(VoiceDef voiceDef in bar1)
			{
				if(voiceDef is Trk)
				{
					nOutputVoices++;
				}
			}
			return nOutputVoices;
		}

		/// <summary>
		/// Synchronous continuous controller settings (ccSettings) are not allowed.
		/// </summary>
		private string CheckCCSettings(List<Bar> bars)
		{
			string errorString = null;
			List<int> ccSettingsMsPositions = new List<int>();
			foreach(Bar bar in bars)
			{
				ccSettingsMsPositions.Clear();

				if(!string.IsNullOrEmpty(errorString))
				{
					break;
				}
			}

			return errorString;
		}

		#endregion

		/// <summary>
		/// Check that each output track index (top to bottom) is the same as its MidiChannel (error is fatal)
		/// </summary>
		/// <param name="systems"></param>
		private void CheckSystems(List<SvgSystem> systems)
		{
			var outputTrackMidiChannels = new List<int>();
			for(int systemIndex = 0; systemIndex < systems.Count; systemIndex++)
			{
				var staves = systems[systemIndex].Staves;
				outputTrackMidiChannels.Clear();
				for (int staffIndex = 0; staffIndex < staves.Count; staffIndex++)
				{
					var voices = staves[staffIndex].Voices;
					foreach(var voice in voices)
					{
						if(voice is OutputVoice)
						{
							outputTrackMidiChannels.Add(voice.MidiChannel);
						}
						else break;
					} 
				}
				for (int trackIndex = 0; trackIndex < outputTrackMidiChannels.Count; trackIndex++)
				{
					M.Assert(trackIndex == outputTrackMidiChannels[trackIndex], "Track index and MidiChannel must be identical.");
				}
			}
		}

        /// <summary>
        /// Creates one System per bar (=list of VoiceDefs) in the argument.
        /// The Systems are complete with staves and voices of the correct type:
        /// Each OutputStaff is allocated parallel (empty) OutputVoice fields.
        /// Each Voice has a VoiceDef field that is allocated to the corresponding
        /// VoiceDef from the argument.
        /// The OutputVoices have MIDIChannels arranged according to PageFormat.OutputMIDIChannelsPerStaff.
        /// OutputVoices are given a midi channel allocated from top to bottom in the printed score.
        /// </summary>
        private void CreateEmptySystems(List<List<List<VoiceDef>>> voiceDefsPerStaffPerBar)
        {
            var nSystems = voiceDefsPerStaffPerBar.Count;
            for(var systemIndex = 0; systemIndex < nSystems; systemIndex++)
            {
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
                var voiceDefsPerStaff = voiceDefsPerStaffPerBar[systemIndex];
                var nStaves = voiceDefsPerStaff.Count;
                for(var staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    string staffname = StaffName(systemIndex, staffIndex);
                    OutputStaff outputStaff = new OutputStaff(system, staffname, PageFormat.StafflinesPerStaff[staffIndex], PageFormat.GapVBPX, PageFormat.StafflineStemStrokeWidthVBPX);

                    var staffVoiceDefs = voiceDefsPerStaff[staffIndex];
                    var staffMidiChannels = PageFormat.MIDIChannelsPerStaff[staffIndex];
                    M.Assert(staffVoiceDefs.Count == staffMidiChannels.Count);
                    for(int ovIndex = 0; ovIndex < staffVoiceDefs.Count; ++ovIndex)
                    {
                        Trk trkDef = staffVoiceDefs[ovIndex] as Trk;
                        M.Assert(trkDef != null);
                        OutputVoice outputVoice = new OutputVoice(outputStaff, staffMidiChannels[ovIndex])
                        {
                            VoiceDef = staffVoiceDefs[ovIndex]
                        };
                        outputStaff.Voices.Add(outputVoice);
                    }
                    SetStemDirections(outputStaff);
                    system.Staves.Add(outputStaff);
                }
            }
		}

		private string StaffName(int systemIndex, int staffIndex)
        {
            if(systemIndex == 0)
            {
                return PageFormat.LongStaffNames[staffIndex];
            }
            else
            {
                return PageFormat.ShortStaffNames[staffIndex];
            }
        }

        private void SetStemDirections(Staff staff)
        {
            if(staff.Voices.Count == 1)
            {
                staff.Voices[0].StemDirection = VerticalDir.none;
            }
            else
            {
                M.Assert(staff.Voices.Count == 2);
                staff.Voices[0].StemDirection = VerticalDir.up;
                staff.Voices[1].StemDirection = VerticalDir.down;
            }
        }
    }
}

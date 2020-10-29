using MNX.Common;
using MNX.Globals;
using Moritz.Spec;
using Moritz.Xml;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Moritz.Symbols
{
    public class SVGMIDIScore : SvgScore
    {
        public SVGMIDIScore(string targetFolder, MNX.Common.MNX mnx, Form1Data form1Data)
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

            M.PageFormat = new PageFormat(form1Data, mnx.VoicesPerStaffPerPart);

            Notator = new Notator();

            List<Bar> bars = GetBars(mnx);

            CheckBars(bars);

            CreateSystems(bars, form1Data);

            string filePath = null;
            if(form1Data.Options.WriteScrollScore)
            {
                M.PageFormat.BottomVBPX = GetNewBottomVBPX(Systems);
                M.PageFormat.BottomMarginPosVBPX = (int)(M.PageFormat.BottomVBPX - M.PageFormat.DefaultDistanceBetweenSystemsVBPX);
                filePath = SaveSVGScrollScore(!form1Data.Options.IncludeMIDIData, form1Data.Options.WritePage1Titles);
            }
            else
            {
                filePath = SaveMultiPageScore(!form1Data.Options.IncludeMIDIData, form1Data.Options.WritePage1Titles);
            }

            // Opens the score in the program which is set by the system to open .svg or .html files.
            global::System.Diagnostics.Process.Start(filePath);

        }

        private List<Bar> GetBars(MNX.Common.MNX mnxCommon)
        {
            var bars = new List<Bar>();
            List<List<IUniqueDef>> globalDirectionsPerMeasure = mnxCommon.Global.GetGlobalDirectionsPerMeasure();
            var midiChannelsPerStaff = M.PageFormat.MIDIChannelsPerStaff;
            var nSystemStaves = midiChannelsPerStaff.Count;

            var seqMsPositionInScore = 0;
            for(var measureIndex = 0; measureIndex < globalDirectionsPerMeasure.Count; measureIndex++)
            {
                var midiChannelIndexPerOutputVoice = new List<int>();
                List<Trk> trks = new List<Trk>();
                List<IUniqueDef> globalDirections = globalDirectionsPerMeasure[measureIndex];
                var systemStaffIndex = 0;
                foreach(var part in mnxCommon.Parts)
                {
                    var measure = part.Measures[measureIndex];
                    List<IUniqueDef> measureDirections = GetMeasureDirections(measure.Directions);
                    var voicesPerStaff = part.VoicesPerStaff;
                    var nPartStaves = voicesPerStaff.Count;

                    for(var partStaffIndex = 0; partStaffIndex < nPartStaves; ++partStaffIndex)
                    {
                        var nVoices = voicesPerStaff[partStaffIndex];

                        for(var voiceIndex = 0; voiceIndex < nVoices; voiceIndex++)
                        {
                            Sequence sequence = measure.Sequences[voiceIndex];
                            List<IUniqueDef> seqIUDs = sequence.SetMsDurationsAndGetIUniqueDefs(seqMsPositionInScore, M.PageFormat.MillisecondsPerTick);

                            InsertDirectionsInSeqIUDs(seqIUDs, measureDirections, globalDirections);

                            var midiChannel = midiChannelsPerStaff[systemStaffIndex][voiceIndex];
                            midiChannelIndexPerOutputVoice.Add(midiChannel);
                            Trk trk = new Trk(midiChannel, 0, seqIUDs);
                            trks.Add(trk);
                        }
                        systemStaffIndex++;
                    }
                }
                Seq seq = new Seq(seqMsPositionInScore, trks, midiChannelIndexPerOutputVoice);
                Bar bar = new Bar(seq);
                bars.Add(bar);
                seqMsPositionInScore += seq.MsDuration;
            }

            AdjustNoteheadPitchesForOctaveShifts(bars);

            return bars;
        }

        /// <summary>
        /// Adjust all NoteheadPitches where OctaveShifts are active.
        /// For later OctaveShift bracket construction:
        /// 1. The original OctaveShift attribute remains in the first Event (or Grace) object.
        /// 2. The EndOctaveShift attribute in the final shifted Event is set to true (default is false). 
        /// </summary>
        /// <param name="bars"></param>
        private void AdjustNoteheadPitchesForOctaveShifts(List<Bar> bars)
        {
            List<List<VoiceDef>> barsPerVoice = new List<List<VoiceDef>>();

            #region get barsPerVoice
            foreach(Bar bar in bars)
            {
                for(var voiceIndex = 0; voiceIndex < bar.VoiceDefs.Count; voiceIndex++)
                {
                    var voiceDef = bar.VoiceDefs[voiceIndex];
                    if(voiceIndex > (barsPerVoice.Count - 1) )
                    {
                        barsPerVoice.Add(new List<VoiceDef>());
                    }
                    barsPerVoice[voiceIndex].Add(voiceDef);
                }
            }
            #endregion

            OctaveShift octaveShift = null;
            Tuple<int, int> endOctaveShiftPos = null;
            for(var voiceIndex = 0; voiceIndex < barsPerVoice.Count; voiceIndex++)
            {
                List<VoiceDef> voiceBars = barsPerVoice[voiceIndex];
                for(var barIndex = 0; barIndex < voiceBars.Count; barIndex++)
                {
                    int tickPositionInBar = 0;
                    var iuds = voiceBars[barIndex].UniqueDefs;
                    for(var iudIndex = 0; iudIndex < iuds.Count; iudIndex++)
                    {
                        if(iuds[iudIndex] is Event evt)
                        {
                            if(evt.OctaveShift != null)
                            {
                                octaveShift = evt.OctaveShift;
                                Tuple<int?, int> eosp = octaveShift.EndOctaveShiftPos;                                
                                if(eosp.Item1 == null)
                                {
                                    endOctaveShiftPos = new Tuple<int, int>(barIndex, eosp.Item2);
                                }
                                else
                                {
                                    endOctaveShiftPos = new Tuple<int, int>((int)eosp.Item1, eosp.Item2);
                                }
                            }
                            if(octaveShift != null)
                            {
                                evt.ShiftNoteheadPitches(octaveShift.Type);

                                if(endOctaveShiftPos.Item1 == barIndex && endOctaveShiftPos.Item2 == tickPositionInBar)
                                {
                                    evt.EndOctaveShift = octaveShift;
                                    octaveShift = null;
                                    endOctaveShiftPos = null;
                                }
                            }
                            tickPositionInBar += evt.TicksDuration;
                        }
                    }
                }
            }
        }

        private List<IUniqueDef> GetMeasureDirections(Directions measureDirections)
        {
            var rval = new List<IUniqueDef>();
            if(measureDirections != null)
            {
                if(measureDirections.Clef != null)
                {
                    rval.Add(measureDirections.Clef);
                }
                if(measureDirections.KeySignature != null)
                {
                    rval.Add(measureDirections.KeySignature);
                }
                if(measureDirections.TimeSignature != null)
                {
                    rval.Add(measureDirections.TimeSignature);
                }
                if(measureDirections.OctaveShift != null)
                {
                    rval.Add(measureDirections.OctaveShift);
                }
            }

            return rval;
        }

        private void InsertDirectionsInSeqIUDs(List<IUniqueDef> seqIUDs, List<IUniqueDef> measureDirections, List<IUniqueDef> globalDirections)
        {
            MNX.Common.Clef clef = Find<MNX.Common.Clef>(seqIUDs, measureDirections, globalDirections);
            MNX.Common.KeySignature keySignature = Find<MNX.Common.KeySignature>(seqIUDs, measureDirections, globalDirections);
            MNX.Common.TimeSignature timeSignature = Find<MNX.Common.TimeSignature>(seqIUDs, measureDirections, globalDirections);

            for(int i = 2; i >= 0; i--)
            {
                if(i < seqIUDs.Count)
                {
                    var iud = seqIUDs[i];
                    if(iud is MNX.Common.Clef || iud is MNX.Common.KeySignature || iud is MNX.Common.TimeSignature)
                    {
                        seqIUDs.RemoveAt(i);
                    }
                }
            }
            if(timeSignature != null)
            {
                seqIUDs.Insert(0, (IUniqueDef)timeSignature.Clone());
            }
            if(keySignature != null)
            {
                seqIUDs.Insert(0, (IUniqueDef)keySignature.Clone());
            }
            if(clef != null)
            {
                seqIUDs.Insert(0, (IUniqueDef)clef.Clone());
            }
        }

        private T Find<T>(List<IUniqueDef> seqIUDs, List<IUniqueDef> measureDirections, List<IUniqueDef> globalDirections)
        {
            T t = default(T);
            if(seqIUDs.Find(obj => obj is T) is T seqT)
            {
                t = seqT;
            }
            else if(measureDirections.Find(obj => obj is T) is T measureT)
            {
                t = measureT;
            }
            else if(globalDirections.Find(obj => obj is T) is T globalT)
            {
                t = globalT;
            }
            return t;
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
            int frameHeight = M.PageFormat.TopMarginPage1VBPX + 20;
            foreach(SvgSystem system in Systems)
            {
                SystemMetrics sm = system.Metrics;
                frameHeight += (int)((sm.Bottom - sm.Top) + M.PageFormat.DefaultDistanceBetweenSystemsVBPX);
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
            List<int> nVoicesPerStaff = GetNVoicesPerStaff(M.PageFormat.MIDIChannelsPerStaff);
            List<List<List<VoiceDef>>> voiceDefsPerStaffPerBar = GetVoiceDefsPerStaffPerBar(bars, nVoicesPerStaff);

            // If a staff has two voices (parallel sequences) then the first must be higher than the second.
            // The order is swapped here if necessary. Stem directions are set later, when real Staff objects exist.
            NormalizeTopToBottomVoiceOrder(voiceDefsPerStaffPerBar);

            // Ensure that all VoiceDefs shared by a Staff begin with the same Clef, KeySignature and TimeSignature.
            // Changes of Clef, KeySignature and TimeSignature over the course of the score are taken into account.
            // Small (cautionary) Clefs in lower voices have smallClef.IsVisible = false.
            InsertInitialIUDs(bars, voiceDefsPerStaffPerBar); // see Moritz: ComposableScore.cs line 63.

            CreateEmptySystems(voiceDefsPerStaffPerBar); // one system per bar

            if(M.PageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

                FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                using(Image image = new Bitmap(1, 1))
                {
                    using(Graphics graphics = Graphics.FromImage(image)) // used for measuring strings
                    {
                        /// The systems do not yet contain Metrics info.
                        /// The systems are given Metrics inside the following function then justified horizontally.
                        Notator.CreateMetricsAndJustifySystemsHorizontally(graphics, this.Systems);

                        CreateSlursAndTies(this.Systems, M.PageFormat.GapVBPX);

                        CreateExtendersAndJustifySystemsVertically(graphics);
                    }
                }
            }

            CheckSystems(this.Systems);
        }

        private void CreateExtendersAndJustifySystemsVertically(Graphics graphics)
        {
            #region initialise continuations to none (for first system).
            List<OctaveShift> continuingOctaveShiftExtender = new List<OctaveShift>();
            foreach(var staff in Systems[0].Staves)
            {
                continuingOctaveShiftExtender.Add(null);
            }
            #endregion
            foreach(var system in Systems)
            {
                for(var staffIndex = 0; staffIndex < system.Staves.Count; staffIndex++)
                {
                    var staff = system.Staves[staffIndex];

                    continuingOctaveShiftExtender[staffIndex] =
                        staff.CreateOctaveShiftExtenders(graphics, continuingOctaveShiftExtender[staffIndex]);

                    // do the same for AccelRitExtenders, PedalExtenders etc. here.

                    staff.MoveBarnumberAboveExtenders();
                }
                system.JustifyVertically(M.PageFormat.RightVBPX, M.PageFormat.GapVBPX);
            }
        }

        /// <summary>
        /// All the NoteObjects have Metrics, and have been moved to their correct left-right positions.
        /// </summary>
        /// <param name="systems"></param>
        private void CreateSlursAndTies(List<SvgSystem> systems, double gap)
        {
            var noteObjects = systems[0].Staves[0].Voices[0].NoteObjects;
            var slurTieLeftLimit = noteObjects.Find(obj => obj is Barline).Metrics.OriginX - M.PageFormat.GapVBPX;
            var slurTieRightLimit = noteObjects[noteObjects.Count - 1].Metrics.OriginX + M.PageFormat.GapVBPX;
            List<(string, bool)> headIDsSlurredToPreviousSystem = new List<(string, bool)>();
            List<string> headIDsTiedToPreviousSystem = new List<string>();

            foreach(var system in systems)
            {
                if(headIDsTiedToPreviousSystem.Count > 0)
                {
                    foreach(var staff in system.Staves)
                    {
                        foreach(var voice in staff.Voices)
                        {
                            Tie.TieFirstHeads(voice, headIDsTiedToPreviousSystem, slurTieLeftLimit);
                        }
                    }
                }
                M.Assert(headIDsTiedToPreviousSystem.Count == 0);

                if(headIDsSlurredToPreviousSystem.Count > 0)
                {
                    foreach(var staff in system.Staves)
                    {
                        foreach(var voice in staff.Voices)
                        {
                            AddSlurTemplatesToFirstHeads(voice, headIDsSlurredToPreviousSystem, slurTieLeftLimit);
                        }
                    }
                }
                M.Assert(headIDsSlurredToPreviousSystem.Count == 0);

                foreach(var staff in system.Staves)
                {
                    foreach(var voice in staff.Voices)
                    {
                        noteObjects = voice.NoteObjects;
                        
                        for(var noteObjectIndex = 0; noteObjectIndex < noteObjects.Count; noteObjectIndex++)
                        {
                            if(noteObjects[noteObjectIndex] is OutputChordSymbol leftChord)
                            {
                                if(leftChord.HeadsTopDown[0].Tied != null)
                                {
                                    OutputChordSymbol rightChord = FindNextChord(voice, noteObjectIndex); // returns null if there is no OutputChordSymbol to the right.

                                    // Each Tuple contains tieOriginX, tieOriginY, tieRightX, tieIsOver, tieTargetHeadID 
                                    List<Tuple<double, double, double, bool, string>> tiesData = Tie.GetTiesData(leftChord, rightChord, slurTieRightLimit);

                                    leftChord.AddTies(tiesData);

                                    if(tiesData[0].Item3 > slurTieRightLimit)
                                    {
                                        foreach(var tieData in tiesData)
                                        {
                                            headIDsTiedToPreviousSystem.Add(tieData.Item5);
                                        }
                                    }
                                }

                                if(leftChord.Slurs != null && leftChord.Slurs.Count > 0)
                                {
                                    var headsTopDown = leftChord.HeadsTopDown;

                                    foreach(var slurDef in leftChord.Slurs)
                                    {                                        
                                        (Head startNote, Head endNote, string targetEventID, string targetHeadID) = FindSlurHeads(headsTopDown, slurDef, voice, noteObjectIndex, slurTieRightLimit);
                                        // endNote and targetHeadID are null if the target is not on this system.

                                        (double slurBeginX, double slurBeginY, double slurEndX, double slurEndY, bool isOver) = GetSlurData(startNote, endNote, slurTieRightLimit);

                                        leftChord.AddSlurTemplate(slurBeginX, slurBeginY, slurEndX, slurEndY, isOver);

                                        if(endNote == null)
                                        {
                                            headIDsSlurredToPreviousSystem.Add((targetHeadID, isOver));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }                
            }
        }

        private void AddSlurTemplatesToFirstHeads(Voice voice, List<(string headID, bool isOver)> headIDsSlurredToPreviousSystem, double slurTieLeftLimit)
        {
            throw new NotImplementedException();
        }

        private (double slurBeginX, double slurBeginY, double slurEndX, double slurEndY, bool isOver) GetSlurData(Head startNote, Head endNote, double slurTieRightLimit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If slurDef.endNote is not in this Voice, the returned endHead and targetHeadID will be null.
        /// </summary>
        /// <returns></returns>
        private (Head startHead, Head endHead, string targetEventID, string targetHeadID) FindSlurHeads(List<Head> startHeadsTopDown, Slur slurDef, Voice voice, int noteObjectIndex, double systemsRightX)
        {
            Head startHead = null;
            Head endHead = null;
            string targetEventID = slurDef.TargetEventID;
            string targetHeadID = slurDef.EndNoteID; // can be null

            var startHeadID = slurDef.StartNoteID;
            if(startHeadID == null)
            {
                startHead = (slurDef.Orient == Orientation.up) ? startHeadsTopDown[0] : startHeadsTopDown[startHeadsTopDown.Count -1];
            }
            else
            {
                startHead = startHeadsTopDown.Find(head => head.ID.Equals(startHeadID));
            }            

            for(var noIndex = noteObjectIndex; noIndex <= voice.NoteObjects.Count; ++noIndex)
            {
                var outputChordSymbol = voice.NoteObjects[noIndex] as OutputChordSymbol;
                if(outputChordSymbol != null && outputChordSymbol.EventID.Equals(targetEventID))
                {

                }
            }

            return (startHead, endHead, targetEventID, targetHeadID);
        }

        /// <summary>
        /// returns null if there is no OutputChordSymbol to the right of the one at noteObjectIndex.
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="noteObjectIndex"></param>
        /// <returns></returns>
        private OutputChordSymbol FindNextChord(Voice voice, int noteObjectIndex)
        {
            OutputChordSymbol rval = null;
            var noteObjects = voice.NoteObjects;
            for(var i = noteObjectIndex + 1; i < noteObjects.Count; i++)
            {
                M.Assert(!(noteObjects[i] is OutputRestSymbol));
                if(noteObjects[i] is OutputChordSymbol ocs)
                {
                    rval = ocs;
                    break;
                }
            }
            return rval;
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
                        var midiVal = M.MidiPitchDict[note.SoundingPitch];
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
                List<IUniqueDef> unSortedInitialIUDs = new List<IUniqueDef>();
                for(var i = 0; i < 3; i++)
                {
                    foreach(var voiceDef in staffVoiceDefs)
                    {
                        var iud = voiceDef.UniqueDefs[i];
                        if(!(iud is Event))
                        {
                            if(!unSortedInitialIUDs.Contains(iud))
                            {
                                unSortedInitialIUDs.Add(iud); // cloned later when inserted in the other voiceDef
                            }
                        }
                    }
                }

                List<IUniqueDef> initialIUDs = new List<IUniqueDef>();
                IUniqueDef clef;
                if((clef = unSortedInitialIUDs.Find(obj => obj is MNX.Common.Clef)) != null)
                {
                    initialIUDs.Add(clef);
                }
                IUniqueDef keySig;
                if((keySig = unSortedInitialIUDs.Find(obj => obj is MNX.Common.KeySignature)) != null)
                {
                    initialIUDs.Add(keySig);
                }
                IUniqueDef timeSig;
                if((timeSig = unSortedInitialIUDs.Find(obj => obj is MNX.Common.TimeSignature)) != null)
                {
                    initialIUDs.Add(timeSig);
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
            MNX.Common.Clef initialClef = null;
            MNX.Common.KeySignature initialKeySig = null;
            MNX.Common.TimeSignature initialTimeSig = null;

            foreach(var voiceDef in staffVoiceDefs)
            {
                for(var i = 0; i < 3; i++)
                {
                    if(i < voiceDef.UniqueDefs.Count)
                    {
                        if(voiceDef.UniqueDefs[i] is MNX.Common.Clef clef)
                        {
                            initialClef = clef;
                        }
                        if(voiceDef.UniqueDefs[i] is MNX.Common.KeySignature keySig)
                        {
                            initialKeySig = keySig;
                        }
                        if(voiceDef.UniqueDefs[i] is MNX.Common.TimeSignature timeSig)
                        {
                            initialTimeSig = timeSig;
                        }
                    }
                }
            }
            foreach(IUniqueDef iud in initialIUDs)
            {
                if(iud is MNX.Common.Clef clef && initialClef == null)
                {
                    initialClef = (MNX.Common.Clef) clef;
                }
                if(iud is MNX.Common.KeySignature keySig && initialKeySig == null)
                {
                    initialKeySig = (MNX.Common.KeySignature) keySig;
                }
                if(iud is MNX.Common.TimeSignature timeSig && initialTimeSig == null)
                {
                    initialTimeSig = (MNX.Common.TimeSignature) timeSig;
                }
            }
            M.Assert(initialClef != null);

            foreach(var voiceDef in staffVoiceDefs)
            {
                for(int i = 2; i >= 0; i--)
                {
                    if(i < voiceDef.Count)
                    {
                        if(voiceDef[i] is MNX.Common.Clef || voiceDef[i] is MNX.Common.KeySignature || voiceDef[i] is MNX.Common.TimeSignature)
                        {
                            voiceDef.RemoveAt(i);
                        }
                    }
                }
                if(initialTimeSig != null)
                {
                    voiceDef.Insert(0, initialTimeSig);
                }
                if(initialKeySig != null)
                {
                    voiceDef.Insert(0, initialKeySig);
                }
                voiceDef.Insert(0, initialClef);
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
			
			IReadOnlyList<IReadOnlyList<int>> outputChPerStaff = M.PageFormat.MIDIChannelsPerStaff;

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
                    OutputStaff outputStaff = new OutputStaff(system, staffname, M.PageFormat.StafflinesPerStaff[staffIndex], M.PageFormat.GapVBPX, M.PageFormat.StafflineStemStrokeWidthVBPX);

                    var staffVoiceDefs = voiceDefsPerStaff[staffIndex];
                    var staffMidiChannels = M.PageFormat.MIDIChannelsPerStaff[staffIndex];
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
                return M.PageFormat.LongStaffNames[staffIndex];
            }
            else
            {
                return M.PageFormat.ShortStaffNames[staffIndex];
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

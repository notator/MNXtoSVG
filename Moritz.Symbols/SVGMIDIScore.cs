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

            GetVoiceDefs(mnxCommon, out List<VoiceDef> voiceDefs, out List<int> endBarlineMsPositionPerBar);

            List<Bar> bars = GetBars(voiceDefs, endBarlineMsPositionPerBar);

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

        private void GetVoiceDefs(MNXCommon mnxCommon, out List<VoiceDef> voiceDefs, out List<int> endBarlineMsPositionPerBar)
        {
            List<List<int>> midiChannelsPerStaff = new List<List<int>>();
            int currentMIDIChannel = 0;

            List<List<IUniqueDef>> globalIUDsPerMeasure = mnxCommon.Global.GetGlobalIUDsPerMeasure();

            List<List<Trk>> Tracks = new List<List<Trk>>();
            List<int> numberOfStavesPerPart = new List<int>();
            foreach(var part in mnxCommon.Parts)
            {
                int nTracks = part.Measures[0].Sequences.Count;
                List<int> midiChannelsPerPart = new List<int>();
                int numberOfStaves = 1;
                for(var i = 0; i < nTracks; i++)
                {
                    midiChannelsPerPart.Add(currentMIDIChannel);
                    List<Trk> track = new List<Trk>();
                    for(int measureIndex = 0; measureIndex < part.Measures.Count; measureIndex++)
                    {
                        List<IUniqueDef> globalIUDs = globalIUDsPerMeasure[measureIndex];
                        var measure = part.Measures[measureIndex];
                        Sequence sequence = measure.Sequences[i];
                        if(sequence.StaffIndex != null)
                        {
                            int newNStaves = (int)sequence.StaffIndex; // StaffIndex starts at 1 !
                            numberOfStaves = (newNStaves > numberOfStaves) ? newNStaves : numberOfStaves;
                        }
                        List<IUniqueDef> seqIUDs = sequence.SetMsDurationsAndGetIUniqueDefs(PageFormat.MillisecondsPerTick);
                        MeasureInsertGlobalIUDsInIUDs(globalIUDs, seqIUDs);
                        Trk newTrk = new Trk(currentMIDIChannel, 0, seqIUDs);
                        track.Add(newTrk);
                    }
                    Tracks.Add(track);
                    currentMIDIChannel++;
                }
                numberOfStavesPerPart.Add(numberOfStaves);
                midiChannelsPerStaff.Add(midiChannelsPerPart);
            }

            endBarlineMsPositionPerBar = GetEndBarlineMsPositionPerBar(Tracks[0]);
            voiceDefs = GetVoiceDefs(Tracks);
        }

        private void MeasureInsertGlobalIUDsInIUDs(List<IUniqueDef> globalIUDs, List<IUniqueDef> seqIUDs)
        {
            if(globalIUDs.Find(obj => obj is TimeSignature) is TimeSignature timeSignature)
            {
                int insertIndex = (seqIUDs[0] is Clef) ? 1 : 0;
                seqIUDs.Insert(insertIndex, timeSignature as IUniqueDef);
            }
            if(globalIUDs.Find(obj => obj is KeySignature) is KeySignature keySignature)
            {
                int insertIndex = (seqIUDs[0] is Clef) ? 1 : 0;
                seqIUDs.Insert(insertIndex, keySignature as IUniqueDef);
            }
        }

        private List<int> GetEndBarlineMsPositionPerBar(List<Trk> trks)
        {
            List<int> rval = new List<int>();
            int currentPosition = 0;
            foreach(var trk in trks)
            {
                currentPosition += trk.MsDuration;
                rval.Add(currentPosition);
            }
            return rval;
        }

        /// <summary>
        /// This function consumes its argumant.
        /// </summary>
        /// <param name="tracks"></param>
        /// <returns></returns>
        private List<VoiceDef> GetVoiceDefs(List<List<Trk>> tracks)
        {
            var rval = new List<VoiceDef>();

            foreach(var trkList in tracks)
            {
                Trk trk = trkList[0];
                for(var i = 1; i < trkList.Count; i++)
                {
                    trk.AddRange(trkList[i]);
                }
                rval.Add(trk);
            }
            return rval;
        }




















        /// <summary>
        /// Used by GetBars(...) below.
        /// </summary>
        private List<VoiceDef> _voiceDefs = null;
        /// Converts the List of VoiceDef to a list of Bar. The VoiceDefs are Cloned, and the Clones are consumed.
        /// Uses the argument barline msPositions as the EndBarlines of the returned bars (which don't contain barlines).
        /// An exception is thrown if:
        ///    1) the first argument value is less than or equal to 0.
        ///    2) the argument contains duplicate msPositions.
        ///    3) the argument is not in ascending order.
        ///    4) a Trk.MsPositionReContainer is not 0.
        ///    5) an msPosition is not the endMsPosition of any IUniqueDef in the seq.
        private List<Bar> GetBars(IReadOnlyList<VoiceDef> voiceDefs, IReadOnlyList<int> msPositionPerBar)
        {
            _voiceDefs = new List<VoiceDef>();
            foreach(var voiceDef in voiceDefs)
            {
                _voiceDefs.Add(((Trk)voiceDef).Clone() as VoiceDef);
            }

            CheckBarlineMsPositions(msPositionPerBar);

            List<int> barMsDurations = new List<int>();
            int startMsPos = 0;
            for(int i = 0; i < msPositionPerBar.Count; i++)
            {
                int endMsPos = msPositionPerBar[i];
                barMsDurations.Add(endMsPos - startMsPos);
                startMsPos = endMsPos;
            }

            List<Bar> bars = new List<Bar>();
            int totalDurationBeforePop = voiceDefs[0].MsDuration;
            List<int> midiChannelPerOutputVoice = new List<int>();
            for(var i = 0; i < voiceDefs.Count; i++)
            {
                midiChannelPerOutputVoice.Add(i);
            }
            List<Trk> trks = new List<Trk>();
            foreach(var voiceDef in voiceDefs)
            {
                trks.Add(voiceDef as Trk);
            }

            Seq seq = new Seq(0, trks, midiChannelPerOutputVoice);
            Bar remainingBar = new Bar(seq);

            foreach(int barMsDuration in barMsDurations)
            {
                Tuple<Bar, Bar> rTuple = PopBar(remainingBar, barMsDuration);
                Bar poppedBar = rTuple.Item1;
                remainingBar = rTuple.Item2; // null after the last pop.

                M.Assert(poppedBar.MsDuration == barMsDuration);
                if(remainingBar != null)
                {
                    M.Assert(poppedBar.MsDuration + remainingBar.MsDuration == totalDurationBeforePop);
                    totalDurationBeforePop = remainingBar.MsDuration;
                }
                else
                {
                    M.Assert(poppedBar.MsDuration == totalDurationBeforePop);
                }

                bars.Add(poppedBar);
            }

            return bars;
        }

        /// <summary>
        /// Returns a Tuple in which Item1 is the popped bar, Item2 is the remaining part of the input bar.
        /// The popped bar has a list of voiceDefs containing the IUniqueDefs that
        /// begin within barDuration. These IUniqueDefs are removed from the current bar before returning it as Item2.
        /// MidiRestDefs and MidiChordDefs are split as necessary, so that when this
        /// function returns, both the popped bar and the current bar contain voiceDefs
        /// having the same msDuration. i.e.: Both the popped bar and the remaining bar "add up".
        /// </summary>
        /// <param name ="bar">The bar fron which the bar is popped.</param>
        /// <param name="barMsDuration">The duration of the popped bar.</param>
        /// <returns>The popped bar</returns>
        private Tuple<Bar, Bar> PopBar(Bar bar, int barMsDuration)
        {
            M.Assert(barMsDuration > 0);

            if(barMsDuration == bar.MsDuration)
            {
                return new Tuple<Bar, Bar>(bar, null);
            }

            Bar poppedBar = new Bar();
            Bar remainingBar = new Bar();
            int thisMsDuration = _voiceDefs[0].MsDuration;

            VoiceDef poppedBarVoice;
            VoiceDef remainingBarVoice;
            foreach(VoiceDef voiceDef in bar.VoiceDefs)
            {
                poppedBarVoice = new Trk(voiceDef.MidiChannel) { Container = poppedBar };
                poppedBar.VoiceDefs.Add(poppedBarVoice);
                remainingBarVoice = new Trk(voiceDef.MidiChannel) { Container = remainingBar };
                remainingBar.VoiceDefs.Add(remainingBarVoice);

                foreach(IUniqueDef iud in voiceDef.UniqueDefs)
                {
                    int iudMsDuration = iud.MsDuration;
                    int iudStartPos = iud.MsPositionReFirstUD;
                    int iudEndPos = iudStartPos + iudMsDuration;

                    if(iudStartPos >= barMsDuration)
                    {
                        if(iud is ClefDef && iudStartPos == barMsDuration)
                        {
                            poppedBarVoice.UniqueDefs.Add(iud);
                        }
                        else
                        {
                            remainingBarVoice.UniqueDefs.Add(iud);
                        }
                    }
                    else if(iudEndPos > barMsDuration)
                    {
                        int durationBeforeBarline = barMsDuration - iudStartPos;
                        int durationAfterBarline = iudEndPos - barMsDuration;
                        if(iud is MidiRestDef)
                        {
                            // This is a rest. Split it.
                            MidiRestDef firstRestHalf = new MidiRestDef(iudStartPos, durationBeforeBarline);
                            poppedBarVoice.UniqueDefs.Add(firstRestHalf);

                            MidiRestDef secondRestHalf = new MidiRestDef(barMsDuration, durationAfterBarline);
                            remainingBarVoice.UniqueDefs.Add(secondRestHalf);
                        }
                        if(iud is CautionaryChordDef)
                        {
                            M.Assert(false, "There shouldnt be any cautionary chords here.");
                            // This error can happen if an attempt is made to set barlines too close together,
                            // i.e. (I think) if an attempt is made to create a bar that contains nothing... 
                        }
                        else if(iud is MidiChordDef)
                        {
                            IUniqueSplittableChordDef uniqueChordDef = iud as IUniqueSplittableChordDef;
                            uniqueChordDef.MsDurationToNextBarline = durationBeforeBarline;
                            poppedBarVoice.UniqueDefs.Add(uniqueChordDef);

                            M.Assert(remainingBarVoice.UniqueDefs.Count == 0);
                            CautionaryChordDef ccd = new CautionaryChordDef(uniqueChordDef, 0, durationAfterBarline);
                            remainingBarVoice.UniqueDefs.Add(ccd);
                        }
                    }
                    else
                    {
                        M.Assert(iudEndPos <= barMsDuration && iudStartPos >= 0);
                        poppedBarVoice.UniqueDefs.Add(iud);
                    }
                }
            }

            poppedBar.AbsMsPosition = remainingBar.AbsMsPosition;
            poppedBar.AssertConsistency();
            if(remainingBar != null)
            {
                remainingBar.AbsMsPosition += barMsDuration;
                remainingBar.SetMsPositionsReFirstUD();
                remainingBar.AssertConsistency();
            }

            return new Tuple<Bar, Bar>(poppedBar, remainingBar);
        }

        /// <summary>
        /// An exception is thrown if:
        ///    1) the first argument value is less than or equal to 0.
        ///    2) the argument contains duplicate msPositions.
        ///    3) the argument is not in ascending order.
        ///    4) a VoiceDef.MsPositionReContainer is not 0.
        ///    5) if the bar contains InputVoiceDefs, an msPosition is not the endMsPosition of any IUniqueDef in the InputVoiceDefs
        ///       else if an msPosition is not the endMsPosition of any IUniqueDef in the Trks.
        /// </summary>
        private void CheckBarlineMsPositions(IReadOnlyList<int> barlineMsPositionsReThisBar)
        {
            M.Assert(barlineMsPositionsReThisBar[0] > 0, "The first msPosition must be greater than 0.");

            int msDuration = _voiceDefs[0].MsDuration;
            for(int i = 0; i < barlineMsPositionsReThisBar.Count; ++i)
            {
                int msPosition = barlineMsPositionsReThisBar[i];
                M.Assert(msPosition <= _voiceDefs[0].MsDuration);
                for(int j = i + 1; j < barlineMsPositionsReThisBar.Count; ++j)
                {
                    M.Assert(msPosition != barlineMsPositionsReThisBar[j], "Error: Duplicate barline msPositions.");
                }
            }

            int currentMsPos = -1;
            foreach(int msPosition in barlineMsPositionsReThisBar)
            {
                M.Assert(msPosition > currentMsPos, "Value out of order.");
                currentMsPos = msPosition;
                bool found = false;
                for(int i = _voiceDefs.Count - 1; i >= 0; --i)
                {
                    VoiceDef voiceDef = _voiceDefs[i];

                    M.Assert(voiceDef.MsPositionReContainer == 0);

                    foreach(IUniqueDef iud in voiceDef.UniqueDefs)
                    {
                        if(msPosition == (iud.MsPositionReFirstUD + iud.MsDuration))
                        {
                            found = true;
                            break;
                        }
                    }
                    if(found)
                    {
                        break;
                    }
                }
                M.Assert(found, "Error: barline must be at the endMsPosition of at least one IUniqueDef in a Trk.");
            }
        }

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

            CreateEmptySystems(bars); // one system per bar

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

        /// <summary>
        /// Inserts a ClefDef at the beginning of each Trk in each bar, taking any cautionaryChordDefs into account.
        /// </summary>
        /// <param name="bars"></param>
        /// <param name="initialClefPerMIDIChannel">The clefs at the beginning of the score.</param>
        private void InsertInitialClefDefs(List<Bar> bars, List<string> initialClefPerMIDIChannel)
		{
			// bars can currently contain cautionary clefs, but no initial clefs
			List<string> currentClefs = new List<string>(initialClefPerMIDIChannel);
			int nBars = bars.Count;
			int nVoiceDefs = bars[0].VoiceDefs.Count;
			M.Assert(nVoiceDefs == initialClefPerMIDIChannel.Count); // VoiceDefs are Trks
			foreach (Bar bar in bars)
			{
				for (int i = 0; i < nVoiceDefs; ++i)
				{
					ClefDef initialClefDef = new ClefDef(currentClefs[i], 0); // msPos is set later in Notator.ConvertVoiceDefsToNoteObjects()
					bar.VoiceDefs[i].Insert(0, initialClefDef);
					List<IUniqueDef> iuds = bar.VoiceDefs[i].UniqueDefs;
					for (int j = 1; j < iuds.Count; ++j)
					{
						if (iuds[j] is ClefDef cautionaryClefDef)
						{
							currentClefs[i] = cautionaryClefDef.ClefType;
						}
					}
				}
			}
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
		public void CreateEmptySystems(List<Bar> bars)
        {
            foreach(Bar bar in bars)
            {
                SvgSystem system = new SvgSystem(this);
                this.Systems.Add(system);
            }

            CreateEmptyOutputStaves(bars);
		}

        private void CreateEmptyOutputStaves(List<Bar> bars)
        {
            int nStaves = PageFormat.MIDIChannelsPerStaff.Count;

			for(int systemIndex = 0; systemIndex < Systems.Count; systemIndex++)
            {
                SvgSystem system = Systems[systemIndex];
                IReadOnlyList<VoiceDef> voiceDefs = bars[systemIndex].VoiceDefs;

				#region create visible staves
				for(int staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    string staffname = StaffName(systemIndex, staffIndex);
                    OutputStaff outputStaff = new OutputStaff(system, staffname, PageFormat.StafflinesPerStaff[staffIndex], PageFormat.GapVBPX, PageFormat.StafflineStemStrokeWidthVBPX);

                    IReadOnlyList<int> outputVoiceIndices = PageFormat.MIDIChannelsPerStaff[staffIndex];
                    for(int ovIndex = 0; ovIndex < outputVoiceIndices.Count; ++ovIndex)
                    {
                        Trk trkDef = voiceDefs[outputVoiceIndices[ovIndex]] as Trk;
                        M.Assert(trkDef != null);
                        OutputVoice outputVoice = new OutputVoice(outputStaff, trkDef.MidiChannel)
						{
							VoiceDef = trkDef
						};
                        outputStaff.Voices.Add(outputVoice);
                    }
                    SetStemDirections(outputStaff);
                    system.Staves.Add(outputStaff);
                }
				#endregion
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

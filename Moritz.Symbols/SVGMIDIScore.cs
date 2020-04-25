using System;
using System.Collections.Generic;
using Moritz.Spec;
using Moritz.Xml;
using MNX.Globals;

namespace Moritz.Symbols
{
	public class SVGMIDIScore : SvgScore
    {
        public SVGMIDIScore(string targetFolder, List<Bar> bars, SVGData svgData)
            : base(targetFolder, svgData.metadataTitle)
        {
            CheckBars(bars);

            this.MetadataWithDate = new MetadataWithDate()
            {
                Title = svgData.metadataTitle,
                Author = svgData.metadataAuthor,
                Keywords = svgData.metadataKeywords,
                Comment = svgData.metadataComment,
                Date = M.NowString
            };

            this.MetadataWithDate.Date = M.NowString; // printed in info string at top of score.

            PageFormat = new PageFormat(svgData);

            CreateScore(bars, svgData);

            if(svgData.optionsWriteScrollScore)
            {
                PageFormat.BottomVBPX = GetNewBottomVBPX(Systems);
                PageFormat.BottomMarginPosVBPX = (int)(PageFormat.BottomVBPX - PageFormat.DefaultDistanceBetweenSystemsVBPX);
                SaveSingleSVGScore(!svgData.optionsIncludeMIDIData, svgData.optionsWritePage1Titles);
            }
            else
            {
                SaveMultiPageScore(!svgData.optionsIncludeMIDIData, svgData.optionsWritePage1Titles);
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

        private void CreateScore(List<Bar> bars, SVGData svgData)
        {
            // TODO (see SetScoreRegionsData() function already implemented below)
            //if(svgData.RegionStartBarIndices != null)
            //{
            //    ScoreData = SetScoreRegionsData(bars, svgData.RegionStartBarIndices);
            //}            

            InsertInitialClefDefs(bars, PageFormat.InitialClefPerMIDIChannel);

            CreateEmptySystems(bars); // one system per bar

            bool success = true;
            if(PageFormat.ChordSymbolType != "none") // set by AudioButtonsControl
            {
                Notator.ConvertVoiceDefsToNoteObjects(this.Systems);

                FinalizeSystemStructure(); // adds barlines, joins bars to create systems, etc.

                /// The systems do not yet contain Metrics info.
                /// The systems are given Metrics inside the following function then justified internally,
                /// both horizontally and vertically.
                Notator.CreateMetricsAndJustifySystems(this.Systems);
                success = CreatePages();
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
			M.Assert(nVoiceDefs == initialClefPerMIDIChannel.Count); // VoiceDefs are both Trks and InputVoiceDefs
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

			List<int> lowerVoiceIndices = GetOutputAndInputLowerVoiceIndices();

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

		private List<int> GetOutputAndInputLowerVoiceIndices()
		{
			List<int> lowerVoiceIndices = new List<int>();
			int voiceIndex = 0;
			
			List<List<byte>> outputChPerStaff = PageFormat.OutputMIDIChannelsPerStaff;

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
		/// Each InputStaff is allocated parallel (empty) InputVoice fields.
		/// Each OutputStaff is allocated parallel (empty) OutputVoice fields.
		/// Each Voice has a VoiceDef field that is allocated to the corresponding
		/// VoiceDef from the argument.
		/// The OutputVoices have MIDIChannels arranged according to PageFormat.OutputMIDIChannelsPerStaff.
		/// The InputVoices have MIDIChannels arranged according to PageFormat.InputMIDIChannelsPerStaff.
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
            int nStaves = PageFormat.OutputMIDIChannelsPerStaff.Count;

			for(int systemIndex = 0; systemIndex < Systems.Count; systemIndex++)
            {
                SvgSystem system = Systems[systemIndex];
                IReadOnlyList<VoiceDef> voiceDefs = bars[systemIndex].VoiceDefs;

				#region create visible staves
				for(int staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    string staffname = StaffName(systemIndex, staffIndex);
                    OutputStaff outputStaff = new OutputStaff(system, staffname, PageFormat.StafflinesPerStaff[staffIndex], PageFormat.GapVBPX, PageFormat.StafflineStemStrokeWidthVBPX);

                    List<byte> outputVoiceIndices = PageFormat.OutputMIDIChannelsPerStaff[staffIndex];
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

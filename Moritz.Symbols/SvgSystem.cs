using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Moritz.Xml;
using Moritz.Spec;
using MNX.Globals;

namespace Moritz.Symbols
{
    public class SvgSystem
    {
        /// <summary>
        /// Moritz defaults: tempo=30, InstrNotation=long, left indent=0, right indent = 0;
        /// </summary>
        /// <param name="score"></param>
        public SvgSystem(SvgScore score)
        {
            Score = score;
        }

        /// <summary>
        /// Writes out all the SVGSystem's staves. 
        /// </summary>
        /// <param name="w"></param>
        public void WriteSVG(SvgWriter w, int systemNumber, PageFormat pageFormat, List<CarryMsgs> carryMsgsPerChannel, bool graphicsOnly)
        {
            w.SvgStartGroup(this.Metrics.CSSObjectClass.ToString()); // "system"

			if(!graphicsOnly)
			{
				WriteLeftToRightElement(w);
			}

            for(int staffIndex = 0; staffIndex < Staves.Count; staffIndex++)
            {
                Staves[staffIndex].WriteSVG(w, systemNumber, staffIndex + 1, carryMsgsPerChannel, graphicsOnly);
            }

            if(Staves.Count > 1)
            {
                w.SvgStartGroup(CSSObjectClass.staffConnectors.ToString());
                WriteConnectors(w, systemNumber, pageFormat);
                w.SvgEndGroup(); // connectors
            }

            w.SvgEndGroup(); // system
        }

        private void WriteLeftToRightElement(SvgWriter w)
        {
            w.WriteStartElement("score", "leftToRight", null);
            w.WriteAttributeString("systemTop", M.DoubleToShortString(this.Metrics.NotesTop));
            w.WriteAttributeString("systemBottom", M.DoubleToShortString(this.Metrics.NotesBottom));
            w.WriteEndElement(); // leftToRight
        }

        private void WriteConnectors(SvgWriter w, int systemNumber, PageFormat pageFormat)
        {
            List<bool> barlineContinuesDownList = pageFormat.BarlineContinuesDownList;
            M.Assert(barlineContinuesDownList[barlineContinuesDownList.Count - 1] == false);
            Barline barline = null;
            bool isFirstBarline = true;

            for(int staffIndex = 0; staffIndex < Staves.Count; staffIndex++)
            {
                Staff staff = Staves[staffIndex];
                if(staff.Metrics != null)
                {
                    Voice voice = staff.Voices[0];
                    double barlinesTop = staff.Metrics.StafflinesTop;
                    double barlinesBottom = staff.Metrics.StafflinesBottom;

                    #region set barlinesTop, barlinesBottom
                    switch(staff.NumberOfStafflines)
                    {
                        case 1:
                            barlinesTop -= (staff.Gap * 1.5F);
                            barlinesBottom += (staff.Gap * 1.5F);
                            break;
                        case 2:
                        case 3:
                        case 4:
                            barlinesTop -= staff.Gap;
                            barlinesBottom += staff.Gap;
                            break;
                        default:
                            break;
                    }
                    #endregion set barlinesTop, barlinesBottom

                    #region draw barlines down from staves
                    if(staffIndex < Staves.Count - 1)
                    {
                        //TopEdge topEdge = new TopEdge(Staves[staffIndex + 1], 0, pageFormat.Right);
                        TopEdge topEdge = GetTopEdge(staffIndex + 1, pageFormat.RightVBPX);
                        if(topEdge != null)
                        {
                            BottomEdge bottomEdge = new BottomEdge(staff, 0, pageFormat.RightVBPX, pageFormat.GapVBPX);
                            isFirstBarline = true;

                            for(int i = 0; i < voice.NoteObjects.Count; ++i)
                            {
                                NoteObject noteObject = voice.NoteObjects[i];
                                barline = noteObject as Barline;
                                if(barline != null)
                                {
                                    // draw grouping barlines between staves
                                    if(barlineContinuesDownList[staffIndex] || isFirstBarline)
                                    {
                                        double top = bottomEdge.YatX(barline.Metrics.OriginX);
                                        double bottom = topEdge.YatX(barline.Metrics.OriginX);
                                        bool isLastNoteObject = (i == (voice.NoteObjects.Count - 1));
                                        barline.WriteSVG(w, top, bottom, isLastNoteObject, false);
                                        isFirstBarline = false;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }

        #region make graphics
        /// <summary>
        /// All the objects in this SvgSystem are given Metrics which are then moved to their
        /// final positions within the SvgSystem.
        /// When this function returns, all the contained, drawable objects have their final
        /// relative positions within the sysrtem. They are actually drawn when the SvgSystem
		/// has been moved to its final position on the page.
		/// If there are any remaining overlapping symbols in the system, they are reported in the
		/// returned list of Tuples (this list can be empty). The Tuple members are:
		///     item1: system number
		///     item2: the number of remaining overlaps (in top or lower voices)
		///     item3: "top" or "lower", saying in which voices the overlaps are.
        /// </summary>
        public List<Tuple<int, int, string>> MakeGraphics(Graphics graphics, int systemNumber, PageFormat pageFormat, double leftMargin)
        {
            if(Metrics == null)
                CreateMetrics(graphics, pageFormat, leftMargin);

			// All noteObject metrics are now on the left edge of the page.
			// Chords are aligned on the left edge of the page, with accidentals etc further to 
			// the left. If two standard chords are synchronous in two voices of the same staff,
			// and the noteheads would overlap, the lower chord will have been been moved slightly
			// left or right. The two chords are at their final positions relative to each other.
			// Barlines are currently aligned with their main line on the left edge of the page.
			// Barlines' DrawObjects are currently aligned on the left edge of the page with their
			// OriginY at 0 (barNumbers and staffNames are centred, regionStartTexts are left-aligned,
			// and regionEndTexts are right-aligned to the edge of the page). These drawObjects are 
			// are moved to their correct X- and Y- positions after all the DurationObjects and
			// Barlines have been moved to their final positions on the staff.

            List<NoteObjectMoment> moments = MomentSymbols(pageFormat.GapVBPX);

			SymbolSet symbolSet = Score.Notator.SymbolSet;

            // barlineandRepeatSymbolWidths:  Key is a moment's msPosition. Value is the distance between the left edge 
            // of the repeatSymbol or barline and the AlignmentX of the moment which immediately follows it.
            Dictionary<int, double> barlineAndRepeatWidths = GetBarlineAndRepeatSymbolWidths(moments, pageFormat);

			DistributeProportionally(moments, barlineAndRepeatWidths, pageFormat, leftMargin);

			// The moments have now been distributed proportionally within each bar, but no checking has
			// been done for overlapping noteObject Metrics.

			// SymbolSet is an abstract root class, and the functions called on symbolSet are virtual.
			// Usually they only do something when symbolSet is a StandardSymbolSet.
			symbolSet.AdjustRestsVertically(Staves);
			symbolSet.SetBeamedStemLengths(Staves); // see the comment next to the function

			List<Tuple<int, int, string>> overlapsInfoList = JustifyHorizontally(systemNumber, moments, barlineAndRepeatWidths, pageFormat.StafflineStemStrokeWidthVBPX);

			symbolSet.FinalizeBeamBlocks(Staves);
            symbolSet.AlignLyrics(Staves);
            SvgSystem nextSystem = null;
            if(systemNumber < this.Score.Systems.Count)
            {
                nextSystem = this.Score.Systems[systemNumber];
            }
            symbolSet.AddNoteheadExtenderLines(Staves, pageFormat.RightMarginPosVBPX, pageFormat.GapVBPX,
                pageFormat.NoteheadExtenderStrokeWidth, pageFormat.StafflineStemStrokeWidthVBPX, nextSystem);

            SetBarlineVisibility(pageFormat.BarlineContinuesDownList);

			SetBarlineFramedTextsMetricsPosition();
			AlignStaffnamesInLeftMargin(leftMargin, pageFormat.GapVBPX);
			ResetStaffMetricsBoundaries();

			return overlapsInfoList;
        }

        private double CreateMetrics(Graphics graphics, PageFormat pageFormat, double leftMarginPos)
        {           
            this.Metrics = new SystemMetrics();
            for(int staffIndex = 0; staffIndex < Staves.Count; ++staffIndex)
            {
                Staff staff = Staves[staffIndex];

                double staffHeight = staff.Gap * (staff.NumberOfStafflines - 1);
                staff.Metrics = new StaffMetrics(leftMarginPos, pageFormat.RightMarginPosVBPX, staffHeight);

                for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                {
                    Voice voice = staff.Voices[voiceIndex];

                    voice.SetChordStemDirectionsAndCreateBeamBlocks(pageFormat);

                    string currentClefType = null;
                    for(int nIndex = 0; nIndex < staff.Voices[voiceIndex].NoteObjects.Count; nIndex++)
                    {
                        NoteObject noteObject = staff.Voices[voiceIndex].NoteObjects[nIndex];                        
                        noteObject.Metrics = Score.Notator.SymbolSet.NoteObjectMetrics(graphics, noteObject, voice.StemDirection, staff.Gap, pageFormat, currentClefType);

						M.Assert(noteObject.Metrics != null);
						staff.Metrics.Add(noteObject.Metrics);
						if(noteObject is Barline barline)
						{
							barline.AddAncilliaryMetricsTo(staff.Metrics);
						}
                        if(noteObject is Clef clef)
                        {
                            currentClefType = clef.ClefType;
                        }
                    }
                }

                if(staff.Voices.Count > 1)
                {
                    M.Assert(Score.Notator.SymbolSet is StandardSymbolSet);
                    // Other symbol sets do not support multi voice staves.
                    staff.AdjustTwoPartChords();
                }

                staff.Metrics.Move(0f, pageFormat.DefaultDistanceBetweenStavesVBPX * staffIndex);
                this.Metrics.Add(staff.Metrics);
            }

            return (this.Metrics.Bottom - this.Metrics.Top);
        }

		/// <summary> 
		/// The horizontal Metrics of all the NoteObjects are set to their final display positions.
		/// Returns a list of Tuples, which is empty if there were no remaining overlapping symbols.
		/// If there are Tuples in the list, they contain:
		///     item1: system number
		///     item2: the number of remaining overlaps (in top or lower voices)
		///     item3: "top" or "lower", saying in which voices the overlaps are.
		/// </summary>
		/// <param name="pageFormat"></param>
		private List<Tuple<int, int, string>> JustifyHorizontally(int systemNumber, List<NoteObjectMoment> systemMoments,
            Dictionary<int, double> barlineAndRepeatWidths, double hairline)
        {
			var overlaps = new Dictionary<int, double>();
			var rval = new List<Tuple<int, int, string>>();

			HashSet<int> nonCompressibleSystemMomentPositions = new HashSet<int>();
            bool lowerVoiceMoved = false;
            do
            {
                overlaps = JustifyTopVoicesHorizontally(systemMoments, barlineAndRepeatWidths, nonCompressibleSystemMomentPositions, hairline);
				if(overlaps.Count > 0)
				{
					rval.Add(Tuple.Create(systemNumber, overlaps.Count, "top"));
				}

				if(LowerVoicesExist)
                {
                    lowerVoiceMoved = false;
                    overlaps = JustifyLowerVoicesHorizontally(ref lowerVoiceMoved, systemMoments, barlineAndRepeatWidths, nonCompressibleSystemMomentPositions, hairline);
					if(overlaps.Count > 0)
					{
						rval.Add(Tuple.Create(systemNumber, overlaps.Count, "lower"));
					}
				}

            } while(lowerVoiceMoved);

			return rval;
		}

		private Dictionary<int, double> JustifyTopVoicesHorizontally(List<NoteObjectMoment> systemMoments, Dictionary<int, double> barlineAndRepeatWidths, 
            HashSet<int> nonCompressibleSystemMomentPositions, double hairline)
        {
            Dictionary<int, double> overlaps = new Dictionary<int, double>();
            List<NoteObjectMoment> moments = null;
			bool allStavesRedistributed = true;
			bool staffRedistributed = false;
			do
            {
                foreach(Staff staff in this.Staves)
                {
                    moments = GetVoiceMoments(staff.Voices[0], systemMoments);
                    if(moments.Count > 1)
                        overlaps = GetOverlaps(moments, hairline);
                    if(overlaps.Count == 0)
                        continue;

                    staffRedistributed = RedistributeMoments(systemMoments, barlineAndRepeatWidths, nonCompressibleSystemMomentPositions, moments, overlaps);
					allStavesRedistributed = (staffRedistributed == false) ? false : allStavesRedistributed;
                }

            } while(overlaps.Count > 0 && allStavesRedistributed);

            return overlaps;
        }

        private bool LowerVoicesExist
        {
            get
            {
                bool lowerVoicesExist = false;
                foreach(Staff staff in this.Staves)
                {
                    if(staff.Voices.Count > 1)
                    {
                        lowerVoicesExist = true;
                        break;
                    }
                }
                return lowerVoicesExist;
            }
        }

		private Dictionary<int, double> JustifyLowerVoicesHorizontally(ref bool lowerVoiceMoved, 
            List<NoteObjectMoment> systemMoments, Dictionary<int, double> barlineAndRepeatWidths,
            HashSet<int> nonCompressibleSystemMomentPositions, double hairline)
        {
            Dictionary<int, double> overlaps = new Dictionary<int, double>();
            List<NoteObjectMoment> moments = null;
			bool redistributed = false;
			do
            {
                foreach(Staff staff in this.Staves)
                {
                    if(staff.Voices.Count > 1)
                    {
                        moments = GetVoiceMoments(staff.Voices[1], systemMoments);
                        if(moments.Count > 1)
                            overlaps = GetOverlaps(moments, hairline);
                        if(overlaps.Count == 0)
                        {
                            moments = GetStaffMoments(staff, systemMoments);
                            if(moments.Count > 1)
                                overlaps = GetOverlaps(moments, hairline);
                        }

                        if(overlaps.Count == 0)
                            continue;

                        lowerVoiceMoved = true;
						redistributed = RedistributeMoments(systemMoments, barlineAndRepeatWidths, nonCompressibleSystemMomentPositions, moments, overlaps);
                    }
                }

            } while(overlaps.Count > 0 && redistributed);

            return overlaps;
        }

        private List<NoteObjectMoment> GetVoiceMoments(Voice voice, List<NoteObjectMoment> systemMoments)
        {
            List<NoteObjectMoment> voiceMoments = new List<NoteObjectMoment>();
            NoteObjectMoment voiceNOM = null;
            foreach(NoteObjectMoment systemNOM in systemMoments)
            {
                voiceNOM = null;
                foreach(NoteObject noteObject in systemNOM.NoteObjects)
                {
                    if(noteObject.Voice == voice)
                    {
                        if(voiceNOM == null)
                        {
                            // noteObject in voice 1
                            voiceNOM = new NoteObjectMoment(systemNOM.AbsMsPosition);
                            voiceNOM.Add(noteObject);
                            voiceNOM.AlignmentX = systemNOM.AlignmentX;
                        }
                        else // noteObject in voice 2
                        {
                            voiceNOM.Add(noteObject);
                        }
                    }
                }
                if(voiceNOM != null)
                    voiceMoments.Add(voiceNOM);
            }
            return voiceMoments;
        }

        private List<NoteObjectMoment> GetStaffMoments(Staff staff, List<NoteObjectMoment> systemMoments)
        {
            List<NoteObjectMoment> staffMoments = new List<NoteObjectMoment>();
            NoteObjectMoment staffNOM = null;
            foreach(NoteObjectMoment systemNOM in systemMoments)
            {
                staffNOM = null;
                foreach(NoteObject noteObject in systemNOM.NoteObjects)
                {
                    if(noteObject.Voice.Staff == staff)
                    {
                        if(staffNOM == null)
                        {
                            // noteObject in voice 1
                            staffNOM = new NoteObjectMoment(systemNOM.AbsMsPosition);
                            staffNOM.Add(noteObject);
                            staffNOM.AlignmentX = systemNOM.AlignmentX;
                        }
                        else // noteObject in voice 2
                        {
                            staffNOM.Add(noteObject);
                        }
                    }
                }
                if(staffNOM != null)
                    staffMoments.Add(staffNOM);
            }
            return staffMoments;
        }

        /// <summary>
        /// the returned dictionary contains
        ///     key: the msPosition of a system moment
        ///     value: the distance between the left edge of the repeatSymbol or barline and the moment's AlignmentX.
        /// </summary>
        /// <param name="moments"></param>
        /// <param name="gap"></param>
        /// <returns></returns>
        private Dictionary<int, double> GetBarlineAndRepeatSymbolWidths(List<NoteObjectMoment> moments, PageFormat pageFormat)
        {
            Dictionary<int, double> barlineAndRepeatWidths = new Dictionary<int, double>();

            //Barline barline = null;
            //RepeatSymbol repeatSymbol = null;
            //int absMsPos = 0;
            for(int i = 1; i < moments.Count; i++)
            {
                var absMsPos = moments[i].AbsMsPosition;
                M.Assert(moments.Count > 1);
                var repeatSymbol = moments[i].RepeatSymbol;
                if(repeatSymbol != null && repeatSymbol.Metrics != null)
                {
                    var repeatSymbolWidth = moments[i].AlignmentX - repeatSymbol.Metrics.Left;
                    barlineAndRepeatWidths.Add(absMsPos, repeatSymbolWidth);
                }
                else
                {
                    var barline = moments[i].Barline;
                    if(barline != null && barline.Metrics != null)
                    {
                        var barlineWidth = moments[i].AlignmentX - barline.Metrics.Left;
                        barlineAndRepeatWidths.Add(absMsPos, barlineWidth);
                    }
                }
            }
            return barlineAndRepeatWidths;
        }
        /// <summary>
        /// The MomentSymbols are in order of msPosition.
        /// The contained symbols are in order of voice (top-bottom of this system).
        /// Barlines, Clefs and TimeSignatures, KeySignatures and RepeatSymbols are added to the NoteObjectMomentSymbol containing the following DurationSymbol.
        /// When this function returns, moments are in order of msPosition, and aligned internally at AlignmentX = 0;
        /// </summary>
        private List<NoteObjectMoment> MomentSymbols(double gap)
        {
            SortedDictionary<int, NoteObjectMoment> dict = new SortedDictionary<int, NoteObjectMoment>();
            Barline barline = null;
            Clef clef = null;
            TimeSignature timeSignature = null;
            KeySignature keySignature = null;
            RepeatSymbol repeatSymbol = null;

            foreach(Staff staff in Staves)
            {
                foreach(Voice voice in staff.Voices)
                {
                    int key = -1;
                    #region foreach noteObject
                    foreach(NoteObject noteObject in voice.NoteObjects)
                    {
						if(!(noteObject is DurationSymbol durationSymbol))
						{
							if(noteObject is Clef)
								clef = noteObject as Clef;
                            if(noteObject is Barline)
                                barline = noteObject as Barline;
                            if(noteObject is TimeSignature)
                                timeSignature = noteObject as TimeSignature;
                            if(noteObject is KeySignature)
                                keySignature = noteObject as KeySignature;
                            if(noteObject is RepeatSymbol)
                                repeatSymbol = noteObject as RepeatSymbol;
                        }
						else
						{
							key = durationSymbol.AbsMsPosition;

							if(!dict.ContainsKey(key))
							{
								dict.Add(key, new NoteObjectMoment(durationSymbol.AbsMsPosition));
							}

							if(clef != null)
							{
								dict[key].Add(clef);
								clef = null;
							}
                            if(keySignature != null)
                            {
                                dict[key].Add(keySignature);
                                keySignature = null;
                            }
                            if(barline != null)
                            {
                                dict[key].Add(barline);
                                barline = null;
                            }
                            if(timeSignature != null)
                            {
                                dict[key].Add(timeSignature);
                                timeSignature = null;
                            }
                            if(repeatSymbol != null)
                            {
                                dict[key].Add(repeatSymbol);
                                repeatSymbol = null;
                            }

                            dict[key].Add(durationSymbol);
						}
					}
                    #endregion

                    NoteObjectMoment endMoment;
                    if(!dict.ContainsKey(this.AbsEndMsPosition))
                    {
                        endMoment = new NoteObjectMoment(this.AbsEndMsPosition);
                        dict.Add(this.AbsEndMsPosition, endMoment);
                    }
                    else
                    {
                        endMoment = dict[AbsEndMsPosition];
                    }

                    if(clef != null) // final clef
                    {
                        endMoment.Add(clef);
                    }
                    if(repeatSymbol != null)
                    {
                        endMoment.Add(repeatSymbol);
                    }
                    if(barline != null) // final barline
                    {
                        endMoment.Add(barline);
                    }
                    if(timeSignature != null) // timeSig after final barline
                    {
                        endMoment.Add(timeSignature);
                    }
                }
            }

            List<NoteObjectMoment> momentSymbols = new List<NoteObjectMoment>();
            M.Assert(dict.Count > 0);
            foreach(int key in dict.Keys)
                momentSymbols.Add(dict[key]);

            foreach(NoteObjectMoment momentSymbol in momentSymbols)
            {
                momentSymbol.SetInternalXPositions(gap);
            }

            #region debug
            // moments are currently in order of msPosition.
            double prevMsPos = -1;
            foreach(NoteObjectMoment moment in momentSymbols)
            {
                M.Assert(moment.AbsMsPosition > prevMsPos);
                prevMsPos = moment.AbsMsPosition;
            }
            #endregion

            return momentSymbols;
        }

        /// <summary>
        /// When this function returns, the moments have been distributed proportionally within each bar.
        /// Symbols are at their correct positions, except that no checking has been done for overlapping noteObject Metrics.
        /// </summary>
        private void DistributeProportionally(List<NoteObjectMoment> moments, Dictionary<int, double> barlineAndRepeatWidths, PageFormat pageFormat,
            double leftMarginPos)
        {
            List<double> momentWidths = new List<double>();

            double momentWidth = 0;
            for(int i = 1; i < moments.Count; i++)
            {
                momentWidth = (moments[i].AbsMsPosition - moments[i - 1].AbsMsPosition) * 10000;
                momentWidths.Add(momentWidth);
            }
            momentWidths.Add(0F); // final barline

            double totalMomentWidths = 0;
            foreach(double width in momentWidths)
                totalMomentWidths += width;

            double totalBarlineAndRepeatSymbolWidths = 0;
            foreach(double width in barlineAndRepeatWidths.Values)
            {
                totalBarlineAndRepeatSymbolWidths += width;
            }

            double leftEdgeToFirstAlignment = moments[0].LeftEdgeToAlignment();

            double spreadWidth = pageFormat.RightMarginPosVBPX - leftMarginPos - leftEdgeToFirstAlignment - totalBarlineAndRepeatSymbolWidths;

            double factor = spreadWidth / totalMomentWidths;

            double currentPosition = leftMarginPos + leftEdgeToFirstAlignment;
            for(int i = 0; i < momentWidths.Count; i++)
            {
                if(barlineAndRepeatWidths.ContainsKey(moments[i].AbsMsPosition))
                {
                    currentPosition += barlineAndRepeatWidths[moments[i].AbsMsPosition];
                }
                moments[i].MoveToAlignmentX(currentPosition);
                currentPosition += momentWidths[i] * factor;
            }
        }
        /// <summary>
        /// Returns a dictionary containing moment msPositions and the overlapWidth they contain.
        /// The msPositions are of the moments whose width will have to be increased because they contain one or more
        /// anchorage symbols which overlap the next symbol on the same staff.
        /// </summary>
        private Dictionary<int, double> GetOverlaps(List<NoteObjectMoment> staffMoments, double hairline)
        {
            Dictionary<int, double> overlaps = new Dictionary<int, double>();

            NoteObjectMoment previousNOM = null;
            double overlapWidth = 0;
            int absMsPos = 0;
            int previousMsPos = 0;

            foreach(NoteObjectMoment nom in staffMoments)
            {
                if(previousNOM != null)
                {
                    foreach(NoteObject noteObject in nom.NoteObjects)
                    {
                        overlapWidth = noteObject.OverlapWidth(previousNOM);
                        if(overlapWidth >= 0)
                        {
                            absMsPos = nom.AbsMsPosition;
                            previousMsPos = previousNOM.AbsMsPosition;

                            overlapWidth += hairline;
                            if(overlaps.ContainsKey(previousMsPos))
                                overlaps[previousMsPos] = overlaps[previousMsPos] > overlapWidth ? overlaps[previousMsPos] : overlapWidth;
                            else
                                overlaps.Add(previousMsPos, overlapWidth);
                        }
                    }
                }
                previousNOM = nom;
            }

            return overlaps;
        }

        /// <summary>
        /// Redistributes the moments horizontally (regardless of any overlaps).
        /// </summary>
        private bool RedistributeMoments(List<NoteObjectMoment> systemMoments, Dictionary<int, double> barlineAndRepeatWidths,
            HashSet<int> nonCompressibleSystemMomentPositions,
            List<NoteObjectMoment> staffMoments, // the NoteObjectMoments used by the current staff (contains their msPositions)
            Dictionary<int, double> staffOverlaps) // msPosition, overlap.
        {
            M.Assert(systemMoments.Count > 1 && staffMoments.Count > 1);

            SetNonCompressible(nonCompressibleSystemMomentPositions, systemMoments, staffMoments, staffOverlaps);

            Dictionary<int, double> systemMomentWidthsWithoutBarlinesAndRepeats = GetExistingWidthsWithoutBarlinesAndRepeats(systemMoments, barlineAndRepeatWidths);

			/// The factor by which to compress all those moment widths which are to be compressed.
			double compressionFactor = CompressionFactor(systemMomentWidthsWithoutBarlinesAndRepeats, nonCompressibleSystemMomentPositions, staffOverlaps);

			/// If compressionFactor is less than or equal to 0, the moments will overlap
			/// in the resulting score (and a warning message will be displayed).
			if(compressionFactor > 0)
			{
				/// widthFactors contains the factor by which to multiply the existing width of each system moment.
				Dictionary<int, double> widthFactors =
					WidthFactors(systemMoments, staffMoments, staffOverlaps, barlineAndRepeatWidths, nonCompressibleSystemMomentPositions, compressionFactor);

				for(int i = 1; i < systemMoments.Count; ++i)
				{
					int sysMomentMsPos = systemMoments[i - 1].AbsMsPosition;
					double existingWidth = systemMomentWidthsWithoutBarlinesAndRepeats[sysMomentMsPos];
					double alignmentX = systemMoments[i - 1].AlignmentX + (existingWidth * widthFactors[sysMomentMsPos]);

					if(barlineAndRepeatWidths.ContainsKey(systemMoments[i].AbsMsPosition))
						alignmentX += barlineAndRepeatWidths[systemMoments[i].AbsMsPosition];

					systemMoments[i].MoveToAlignmentX(alignmentX);
				}
				return true;
			}
			else
			{
				return false;
			}
        }

        private Dictionary<int, double> GetExistingWidthsWithoutBarlinesAndRepeats(
            List<NoteObjectMoment> moments,
            Dictionary<int, double> barlineAndRepeatWidths)
        {
            double originalWidth = 0;
            Dictionary<int, double> originalWidthsWithoutBarlines = new Dictionary<int, double>();

            for(int i = 1; i < moments.Count; i++)
            {
                originalWidth = moments[i].AlignmentX - moments[i - 1].AlignmentX;
                if(barlineAndRepeatWidths.ContainsKey(moments[i].AbsMsPosition))
                {
                    originalWidth -= barlineAndRepeatWidths[moments[i].AbsMsPosition];
                }

                originalWidthsWithoutBarlines.Add(moments[i - 1].AbsMsPosition, originalWidth);
            }

            return originalWidthsWithoutBarlines;
        }

        /// <summary>
        /// systemMoments which are about to be widened, are set to be non-compressible.
        /// In other words, this function adds the MsPositions of all systemMoments in range of
        /// the staffMoments having staffOverlaps to the nonCompressibleSystemMomentPositions hash set.
        /// </summary>
        private void SetNonCompressible(HashSet<int> nonCompressibleSystemMomentPositions, 
            List<NoteObjectMoment> systemMoments,
            List<NoteObjectMoment> staffMoments, 
            Dictionary<int, double> staffOverlaps)
        {
            M.Assert(staffMoments.Count > 1);
            for(int stmIndex = 1; stmIndex < staffMoments.Count; ++stmIndex)
            {
                int prevMPos = staffMoments[stmIndex - 1].AbsMsPosition;
                int mPos = staffMoments[stmIndex].AbsMsPosition;
                if(staffOverlaps.ContainsKey(prevMPos))
                {
                    int startIndex = 0;
                    int endIndex = 0;
                    for(int i = 0; i < systemMoments.Count; ++i)
                    {
                        if(systemMoments[i].AbsMsPosition == prevMPos)
                        {
                            startIndex = i;
                        }
                        if(systemMoments[i].AbsMsPosition == mPos)
                        {
                            endIndex = i;
                            break;
                        }
                    }
                    for(int i = startIndex; i < endIndex; ++i)
                    {
                        nonCompressibleSystemMomentPositions.Add(systemMoments[i].AbsMsPosition);
                    }
                }
            }
        }

        /// <summary>
        /// The factor by which to compress those moment widths which are to be compressed.
		/// If the returned value is less than or equal to zero, it will not be used (see
		/// the calling function).
        /// </summary>
        private double CompressionFactor(Dictionary<int, double> systemMomentWidthsWithoutBarlines, HashSet<int> nonCompressibleSystemMomentPositions, Dictionary<int, double> staffOverlaps)
        {
            double totalCompressibleWidth = TotalCompressibleWidth(systemMomentWidthsWithoutBarlines, nonCompressibleSystemMomentPositions);
            double totalOverlaps = TotalOverlaps(staffOverlaps);
            double compressionFactor = (totalCompressibleWidth - totalOverlaps) / totalCompressibleWidth;

            return compressionFactor;
        }

        private double TotalCompressibleWidth(Dictionary<int, double> originalSystemMomentWidths,
                                            HashSet<int> nonCompressibleSystemMomentPositions)
        {
            double totalCompressibleWidth = 0;
            foreach(int msPos in originalSystemMomentWidths.Keys)
            {
                if(!nonCompressibleSystemMomentPositions.Contains(msPos))
                    totalCompressibleWidth += originalSystemMomentWidths[msPos];
            }
            return totalCompressibleWidth;
        }

        private double TotalOverlaps(Dictionary<int, double> staffOverlaps)
        {
            double total = 0;
            foreach(double overlap in staffOverlaps.Values)
            {
                total += overlap;
            }
            return total;
        }

        /// <summary>
        /// Returns a dictionary having an entry for each staff moment, whose key/value pairs are
        ///     key: the msPosition of a staffMoment
        ///     value: the factor by which the staffMoment will be expanded in order to remove a staffOverlap.
        /// Staff moments can span more than one systemMoment...
        /// </summary>
        private Dictionary<int, double> GetStaffExpansionFactors(
            Dictionary<int, double> staffMomentWidthsWithoutBarlines,
            Dictionary<int, double> staffOverlaps)
        {
            Dictionary<int, double> staffExpansionFactors = new Dictionary<int, double>();
            double originalWidth;
            double factor;
            foreach(int msPosition in staffMomentWidthsWithoutBarlines.Keys)
            {
                if(staffOverlaps.ContainsKey(msPosition))
                {
                    originalWidth = staffMomentWidthsWithoutBarlines[msPosition];
                    factor = (originalWidth + staffOverlaps[msPosition]) / originalWidth;
                    staffExpansionFactors.Add(msPosition, factor);
                }
            }

            return staffExpansionFactors;
        }

        /// <summary>
        /// Returns an [msPos, changeFactor] pair for each moment in the system.
        /// The change factor can be 1, compressionFactor, or taken from the staffExpansionFactors.
        /// </summary>
        private Dictionary<int, double> WidthFactors(
            List<NoteObjectMoment> systemMoments, 
            List<NoteObjectMoment> staffMoments,
            Dictionary<int, double> staffOverlaps,
            Dictionary<int, double> barlineAndRepeatWidths,
            HashSet<int> nonCompressibleSystemMomentPositions,
            double compressionFactor)
        {
            Dictionary<int, double> staffMomentWidthsWithoutBarlinesAndRepeats =
                            GetExistingWidthsWithoutBarlinesAndRepeats(staffMoments, barlineAndRepeatWidths);

            // contains an <msPos, expansionFactor> pair for each staffMoment that will be expanded.
            Dictionary<int, double> staffExpansionFactors =
                            GetStaffExpansionFactors(staffMomentWidthsWithoutBarlinesAndRepeats, staffOverlaps);

            List<int> systemMomentKeys = new List<int>();
            foreach(NoteObjectMoment nom in systemMoments)
            {
                systemMomentKeys.Add(nom.AbsMsPosition);
            }

            Dictionary<int, double> widthFactors = new Dictionary<int, double>();
            foreach(int msPos in systemMomentKeys)
            {
                if(nonCompressibleSystemMomentPositions.Contains(msPos))
                    widthFactors.Add(msPos, 1F); // default factor is 1 (moments which are nonCompressible)
                else
                    widthFactors.Add(msPos, compressionFactor);
            }
            // widthFactors now contains an entry for each system Moment
            // Now set the expansionFactors from the staff.

            double expFactor = 0;
            for(int i = 1; i < staffMoments.Count; ++i)
            {
                int startMsPos = staffMoments[i - 1].AbsMsPosition;
                int endMsPos = staffMoments[i].AbsMsPosition;
                if(staffExpansionFactors.ContainsKey(startMsPos))
                {
                    expFactor = staffExpansionFactors[startMsPos];
                    foreach(int msPos in systemMomentKeys)
                    {
                        if(msPos >= startMsPos && msPos < endMsPos)
                        {
                            widthFactors[msPos] = expFactor;
                        }
                        if(msPos >= endMsPos)
                            break;
                    }
                }
            }
            return widthFactors;
        }

		/// <summary>
		/// Each FramedTextsMetrics' OriginX and OriginY values are 0 when this function is called.
		/// First, align their OriginX with their barline's OriginX, then move them up
		/// until they either do not collide with other objects or are at their default
		/// (lowest) positions.
		/// Now, if they currently collide with some other NoteObject on the same staff,
		/// they are moved vertically away from the system until they dont.
		/// Barnumber boxes are moved outside region info boxes, if necessary.
		/// </summary>
		private void SetBarlineFramedTextsMetricsPosition()
		{
			List<NoteObject> voice0NoteObjects = this.Staves[0].Voices[0].NoteObjects;
			List<NoteObject> voice1NoteObjects = (this.Staves[0].Voices.Count > 1) ? this.Staves[0].Voices[1].NoteObjects : null;

			foreach(NoteObject noteObject in voice0NoteObjects)
			{
				if(noteObject is Barline barline)
				{
					barline.AlignFramedTextsXY(voice0NoteObjects);
					if(voice1NoteObjects != null)
					{
						barline.AlignFramedTextsXY(voice1NoteObjects);
					}
				}
			}

			MoveFramedRegionTextsY(voice0NoteObjects);
		}

		private void MoveFramedRegionTextsY(List<NoteObject> voice0NoteObjects)
		{
			List<FramedRegionInfoMetrics> fmt = new List<FramedRegionInfoMetrics>();

			foreach(NoteObject noteObject in voice0NoteObjects)
			{
				if(noteObject is StartRegionBarline startRegionBarline)
				{
					fmt.Add(startRegionBarline.FramedRegionStartTextMetrics);
				}
				else if(noteObject is EndAndStartRegionBarline endAndStartRegionBarline)
				{
					fmt.Add(endAndStartRegionBarline.FramedRegionEndTextMetrics);
					fmt.Add(endAndStartRegionBarline.FramedRegionStartTextMetrics);
				}
				else if(noteObject is EndRegionBarline endRegionBarline)
				{
					fmt.Add(endRegionBarline.FramedRegionEndTextMetrics);
				}
			}
			for(int i = 1; i < fmt.Count; i++)
			{
				fmt[i].MoveAbove(fmt[i - 1]);
			}
		}

		private void AlignStaffnamesInLeftMargin(double leftMarginPos, double gap)
        {
            foreach(Staff staff in Staves)
            {
                foreach(NoteObject noteObject in staff.Voices[0].NoteObjects)
                {
                    if(noteObject is Barline firstBarline)
                    {
                        StaffNameMetrics staffNameMetrics = firstBarline.StaffNameMetrics;
                        if(staffNameMetrics != null)
                        {
                            double alignX = leftMarginPos / 2;
                            double deltaX = alignX - staffNameMetrics.OriginX + gap;
                            staffNameMetrics.Move(deltaX, 0F);
                        }
                        break;
                    }
                }
            }
        }

        private void ResetStaffMetricsBoundaries()
        {
            foreach(Staff staff in Staves)
            {
                if(staff.Metrics != null)
                {
                    staff.Metrics.ResetBoundary();
                }
            }
        }

        private void SetBarlineVisibility(List<bool> barlineContinuesDownList)
        {
            // set the visibility of all but the last barline
            for(int staffIndex = 0; staffIndex < Staves.Count; ++staffIndex)
            {
                Staff staff = Staves[staffIndex];
                Voice voice = staff.Voices[0];
                List<NoteObject> noteObjects = voice.NoteObjects;
                for(int i = 0; i < noteObjects.Count; ++i)
                {
                    bool isSingleStaffGroup =
                        (((staffIndex == 0) || !barlineContinuesDownList[staffIndex - 1]) // there is no grouped staff above
                            && (!barlineContinuesDownList[staffIndex])); // there is no grouped staff below 

                    if((noteObjects[i] is CautionaryChordSymbol)
                    && !isSingleStaffGroup)
                    {                           
                        Barline barline = noteObjects[i - 1] as Barline;
                        M.Assert(barline != null);
                        barline.IsVisible = false;
                    }
                }
            }
            // set the visibility of the last barline on this system
            SvgScore score = this.Score;
            SvgSystem nextSystem = null;
            for(int sysindex = 0; sysindex < this.Score.Systems.Count - 1; ++sysindex)
            {
                if(this == this.Score.Systems[sysindex])
                {
                    nextSystem = this.Score.Systems[sysindex + 1];
                    break;
                }
            }
            if(nextSystem != null)
            {
                for(int staffIndex = 0; staffIndex < Staves.Count; ++staffIndex)
                {
                    List<NoteObject> noteObjects = Staves[staffIndex].Voices[0].NoteObjects;
					DurationSymbol durationSymbol = nextSystem.Staves[staffIndex].Voices[0].FirstDurationSymbol;
					if (noteObjects[noteObjects.Count - 1] is Barline barline
					&& (durationSymbol is CautionaryChordSymbol))
					{
						barline.IsVisible = false;
					}
				}
            }
        }
        public void JustifyVertically(double pageWidth, double gap)
        {
            double stafflinesTop = Staves[0].Metrics.StafflinesTop;
            if(stafflinesTop != 0)
            {
                for(int i = 0; i < Staves.Count; ++i)
                {
                    Staff staff = Staves[i];
                    if(staff.Metrics != null)
                    {
                        staff.Metrics.Move(0, stafflinesTop * -1);
                    }
                }
            }

            M.Assert(Staves[0].Metrics.StafflinesTop == 0);
            for(int i = 1; i < Staves.Count; ++i)
            {
                if(Staves[i].Metrics != null)
                {
                    //BottomEdge bottomEdge = new BottomEdge(Staves[i - 1], 0, pageWidth, gap);
                    BottomEdge bottomEdge = GetBottomEdge(i - 1, 0, pageWidth, gap);
                    if(bottomEdge != null)
                    {
                        TopEdge topEdge = new TopEdge(Staves[i], 0, pageWidth);
                        double separation = topEdge.DistanceToEdgeAbove(bottomEdge);
                        double dy = gap - separation;
                        // limit stafflineHeight to multiples of pageFormat.Gap so that stafflines
                        // are not displayed as thick grey lines.
                        dy = dy - (dy % gap) + gap; // the minimum space bewteen stafflines is gap pixels.
                        if(dy > 0F)
                        {
                            for(int j = i; j < Staves.Count; ++j)
                            {
                                if(Staves[j].Metrics != null)
                                {
                                    Staves[j].Metrics.Move(0, dy);
                                }
                            }
                            this.Metrics.StafflinesBottom += dy;
                        }
                    }
                }
            }

            this.Metrics = new SystemMetrics();
            foreach(Staff staff in Staves)
            {
                this.Metrics.Add(staff.Metrics);
            }
            M.Assert(this.Metrics.StafflinesTop == 0);
        }

        private BottomEdge GetBottomEdge(int upperStaffIndex, int topVisibleStaffIndex, double pageWidth, double gap)
        {
            BottomEdge bottomEdge = null;
            for(int i = upperStaffIndex; i >= topVisibleStaffIndex; --i)
            {
                Staff staff = Staves[i];
                if(staff.Metrics != null)
                {
                    bottomEdge = new BottomEdge(staff, 0, pageWidth, gap);
                    break;
                }
            }
            return bottomEdge;
        }

        private TopEdge GetTopEdge(int lowerStaffIndex, double pageWidth)
        {
            TopEdge topEdge = null;
            for(int i = lowerStaffIndex; i < Staves.Count; ++i)
            {
                Staff staff = Staves[i];
                if(staff.Metrics != null)
                {
                    topEdge = new TopEdge(staff, 0, pageWidth);
                    break;
                }
            }
            return topEdge;
        }
        #endregion

        public int AbsStartMsPosition;
        public int AbsEndMsPosition;

        public List<Staff> Staves = new List<Staff>();
        public SystemMetrics Metrics = null;

		public SvgScore Score; // the containing score

	}
}

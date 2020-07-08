using MNX.Globals;
using MNX.Common;
using Moritz.Spec;
using Moritz.Xml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Moritz.Symbols
{
	public class Notator
    {
        public Notator()
        {
            bool error = false;
            switch(M.PageFormat.ChordSymbolType)
            {
                case "standard":
                    SymbolSet = new StandardSymbolSet(false); // _coloredVelocities = false;
                    break;
                case "coloredVelocities":
                    SymbolSet = new StandardSymbolSet(true); // _coloredVelocities = true;
                    break;
                case "none":
                    SymbolSet = null;
                    break;
                default:
                    error = true;
                    break;
            }
            if(error)
                throw new ApplicationException("Cannot construct Notator!");
        }

        /// <summary>
        /// There is still one system per bar.
		/// Each VoiceDef begins with an MNXCommon.Clef (taking small clefs into account).
		/// An Exception will be thrown if a SmallClefDef is found on the lower voiceDef in a staff in the systems input.
		/// Small clefs (if there are any) are copied from the top to the bottom voice (if there is one) on each staff.
        /// Small clefs on lower voices on a staff have IsVisible set to false.
        /// </summary>
        /// <param name="systems"></param>
        public void ConvertVoiceDefsToNoteObjects(List<SvgSystem> systems)
        {
            byte[] currentChannelVelocities = new byte[systems[0].Staves.Count];
			var topVoiceSmallClefs = new List<SmallClef>();

			int systemAbsMsPos = 0;
            for(int systemIndex = 0; systemIndex < systems.Count; ++systemIndex)
            {
                SvgSystem system = systems[systemIndex];
                system.AbsStartMsPosition = systemAbsMsPos;
                int msPositionReVoiceDef = 0;
                for(int staffIndex = 0; staffIndex < system.Staves.Count; ++staffIndex)
                {
                    Staff staff = system.Staves[staffIndex];
                    msPositionReVoiceDef = 0;
					topVoiceSmallClefs.Clear();
					for(int voiceIndex = 0; voiceIndex < staff.Voices.Count; ++voiceIndex)
                    {
                        Voice voice = staff.Voices[voiceIndex];
                        voice.VoiceDef.AgglomerateRests();

                        msPositionReVoiceDef = 0;
						List<IUniqueDef> iuds = voice.VoiceDef.UniqueDefs;
						M.Assert(iuds[0] is ClefDef || iuds[0] is MNX.Common.Clef); /** <-------------- **/

						for (int iudIndex = 0; iudIndex < iuds.Count; ++ iudIndex)
                        {
							IUniqueDef iud = voice.VoiceDef.UniqueDefs[iudIndex];
                            int absMsPosition = systemAbsMsPos + msPositionReVoiceDef;

                            NoteObject noteObject =
                                SymbolSet.GetNoteObject(voice, absMsPosition, iud, iudIndex, ref currentChannelVelocities[staffIndex]);

							if(noteObject is SmallClef smallClef)
							{
								if(voiceIndex == 0)
								{
									if(staff.Voices.Count > 1)
									{
										topVoiceSmallClefs.Add(smallClef);
									}
								}
								else
								{
									throw new Exception("SmallClefs may not be defined for a lower voice. They will be copied from the top voice");
								}
							}

							if(iud is IUniqueSplittableChordDef iscd && iscd.MsDurationToNextBarline != null)
							{
								msPositionReVoiceDef += (int)iscd.MsDurationToNextBarline;
							}
							else
							{
								msPositionReVoiceDef += iud.MsDuration;
							}

							voice.NoteObjects.Add(noteObject);
                        }
                    }

                    if(staff.Voices.Count == 2)
                    {
						if(topVoiceSmallClefs.Count > 0)
						{
							AddSmallClefsToLowerVoice(staff.Voices[1], topVoiceSmallClefs);
						}

						if(SymbolSet is StandardSymbolSet standardSymbolSet)
							standardSymbolSet.ForceNaturalsInSynchronousChords(staff);
					}
                }
                systemAbsMsPos += msPositionReVoiceDef;
            }
        }

		private void AddSmallClefsToLowerVoice(Voice voice, List<SmallClef> clefsInTopStaff)
		{
			foreach(SmallClef smallClef in clefsInTopStaff)
			{
				InvisibleSmallClef invisibleSmallClef = new InvisibleSmallClef(voice, smallClef.ClefType, smallClef.AbsMsPosition)
				{
					IsVisible = false
				};
				InsertInvisibleClefChangeInNoteObjects(voice, invisibleSmallClef);
			}
		}

        private void InsertInvisibleClefChangeInNoteObjects(Voice voice, InvisibleSmallClef invisibleSmallClef)
        {
            int absMsPos = invisibleSmallClef.AbsMsPosition;
            List<DurationSymbol> durationSymbols = new List<DurationSymbol>();
            foreach(DurationSymbol durationSymbol in voice.DurationSymbols)
            {
                durationSymbols.Add(durationSymbol);
            }

			M.Assert(!(voice.NoteObjects[voice.NoteObjects.Count - 1] is Barline));
			M.Assert(durationSymbols.Count > 0);
			M.Assert(absMsPos > durationSymbols[0].AbsMsPosition);

            if(absMsPos > durationSymbols[durationSymbols.Count - 1].AbsMsPosition)
            {
                // the noteObjects do not yet have a final barline (see M.Assert() above)
                voice.NoteObjects.Add(invisibleSmallClef);
            }
            else
            {
                for(int i = durationSymbols.Count - 2; i >= 0; --i)
                {
                    if(durationSymbols[i].AbsMsPosition < absMsPos)
                    {
                        InsertBeforeDS(voice.NoteObjects, durationSymbols[i + 1], invisibleSmallClef);
                        break;
                    }
                }
            }
        }

        private void InsertBeforeDS(List<NoteObject> noteObjects, DurationSymbol insertBeforeDS, InvisibleSmallClef invisibleSmallClef)
        {
            for(int i = 0; i < noteObjects.Count; ++i)
            {
				if(noteObjects[i] is DurationSymbol durationSymbol && durationSymbol == insertBeforeDS)
				{
					noteObjects.Insert(i, invisibleSmallClef);
					break;
				}
			}
        }

        /// <summary>
        /// The current msPosition of a voice will be retrievable as currentMsPositionPerVoicePerStaff[staffIndex][voiceIndex].
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        private List<List<int>> InitializeCurrentMsPositionPerVoicePerStaff(SvgSystem system)
        {
            List<List<int>> currentMsPositionPerVoicePerStaff = new List<List<int>>();
            foreach(Staff staff in system.Staves)
            {
                List<int> currentVoiceMsPositions = new List<int>();
                currentMsPositionPerVoicePerStaff.Add(currentVoiceMsPositions);
                foreach(Voice voice in staff.Voices)
                {
                    currentVoiceMsPositions.Add(0);
                }
            }
            return currentMsPositionPerVoicePerStaff;
        }

        /// <summary>
        /// The systems do not yet contain Metrics info.
        /// Puts up a Warning Message Box if there are overlapping symbols after the score has been justified horizontally.
        /// </summary>
        public void CreateMetricsAndJustifySystems(List<SvgSystem> systems)
        {
			// set when there are overlaps...
			List<Tuple<int, int, string>> overlaps;
			List<Tuple<int, int, string>> allOverlaps = new List<Tuple<int, int, string>>();

			using(Image image = new Bitmap(1, 1))
            {
                using(Graphics graphics = Graphics.FromImage(image)) // used for measuring strings
                {
                    double system1LeftMarginPos = GetLeftMarginPos(systems[0], graphics, M.PageFormat);
                    double otherSystemsLeftMarginPos = 0;
                    if(systems.Count > 1)
                        otherSystemsLeftMarginPos = GetLeftMarginPos(systems[1], graphics, M.PageFormat);

                    for(int sysIndex = 0; sysIndex < systems.Count; ++sysIndex)
                    {
                        double leftMargin = (sysIndex == 0) ? system1LeftMarginPos : otherSystemsLeftMarginPos;
                        overlaps = systems[sysIndex].MakeGraphics(graphics, sysIndex + 1, M.PageFormat, leftMargin);
						foreach(Tuple<int, int, string> overlap in overlaps)
						{
							allOverlaps.Add(overlap);
						}
                    }
                }
            }
			if(allOverlaps.Count > 0)
			{
				WarnAboutOverlaps(allOverlaps);
			}
        }

		private void WarnAboutOverlaps(List<Tuple<int, int, string>> allOverlaps)
		{
			string msg1 = "There was not enough horizontal space for all the symbols in\n" +
						  "the following systems:\n";
			string msg2Spacer = "      ";
			string msg3 = "\n" +
						  "Possible solutions:\n" +
						  "    Reduce the number of bars in the system(s).\n" +
						  "    Set a smaller gap size for the score.";

			StringBuilder sb = new StringBuilder();
			sb.Append(msg1);
			foreach(Tuple<int, int, string> t in allOverlaps)
			{
				sb.Append($"{msg2Spacer}System number: {t.Item1} -- ({t.Item2} overlaps in {t.Item3} voices)\n");
			}
			sb.Append(msg3);

			MessageBox.Show(sb.ToString(), "Overlap(s) Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private double GetLeftMarginPos(SvgSystem system, Graphics graphics, PageFormat pageFormat)
        {
            double leftMarginPos = pageFormat.LeftMarginPosVBPX;
            double maxNameWidth = 0;
            foreach(Staff staff in system.Staves)
            {
                foreach(NoteObject noteObject in staff.Voices[0].NoteObjects)
                {
					if(noteObject is NormalBarline firstBarline)
					{
						foreach(DrawObject drawObject in firstBarline.DrawObjects)
						{
							if(drawObject is StaffNameText staffName)
							{
								M.Assert(staffName.TextInfo != null);

								TextMetrics staffNameMetrics = new TextMetrics(CSSObjectClass.staffName, graphics, staffName.TextInfo);
								double nameWidth = staffNameMetrics.Right - staffNameMetrics.Left;
								maxNameWidth = (maxNameWidth > nameWidth) ? maxNameWidth : nameWidth;
							}
						}
						break;
					}
				}
            }
            leftMarginPos = maxNameWidth + (pageFormat.GapVBPX * 2.0F);
            leftMarginPos = (leftMarginPos > pageFormat.LeftMarginPosVBPX) ? leftMarginPos : pageFormat.LeftMarginPosVBPX;

            return leftMarginPos;
        }

        #region properties

        public SymbolSet SymbolSet = null;

        #endregion

    }
}

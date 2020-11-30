using MNX.Globals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Moritz.Symbols
{
	/// <summary>
	/// A list of synchronous NoteObjects.
	/// </summary>
	public class NoteObjectMoment
    {
        public NoteObjectMoment(int absMsPosition)
        {
            _absMsPosition = absMsPosition;
        }

        /// <summary>
        /// Returns the distance between the leftmost left edge and this moment's alignment point.
        /// </summary>
        /// <returns></returns>
        public double LeftEdgeToAlignment()
        {
            double maxLeftEdgeToAlignmentX = double.MinValue;
            foreach(NoteObject noteObject in _noteObjects)
            {
                if(noteObject.Metrics != null) // is null if the noteobject is on an invisibleOutputStaff
                {
                    double leftEdgeToAlignmentX = AlignmentX - noteObject.Metrics.Left;
                    maxLeftEdgeToAlignmentX =
                        maxLeftEdgeToAlignmentX > leftEdgeToAlignmentX ? maxLeftEdgeToAlignmentX : leftEdgeToAlignmentX;
                }
            }
            return maxLeftEdgeToAlignmentX;
        }

        /// <summary>
        /// returns the first Barline in this NoteObjectMoment,
        /// or null if there is no barline.
        /// </summary>
        public Barline Barline
        {
            get
            {
                Barline barline = null;
                foreach(NoteObject noteObject in NoteObjects)
                {
                    if(noteObject is Barline)
                    {
                        barline = noteObject as Barline;
                        break;
                    }
                }
                return barline;
            }
        }

        /// <summary>
        /// returns a dictionary containing staff, rightEdge pairs.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Staff, double> StaffRights()
        {
            Dictionary<Staff, double> dict = new Dictionary<Staff, double>();
            foreach(NoteObject noteObject in _noteObjects)
            {
                Staff staff = noteObject.Voice.Staff;
                if(dict.ContainsKey(staff))
                {
                    if(dict[staff] < noteObject.Metrics.Right)
                        dict[staff] = noteObject.Metrics.Right;
                }
                else
                {
                    dict.Add(staff, noteObject.Metrics.Right);
                }
            }
            return dict;
        }

        public void MoveToAlignmentX(double alignmentX)
        {
            double deltaX = alignmentX - AlignmentX;
            foreach(NoteObject noteObject in _noteObjects)
            {
                if(noteObject.Metrics != null)
                    noteObject.Metrics.Move(deltaX, 0);
            }
            AlignmentX = alignmentX;
        }

        /// <summary>
        /// OutputChordSymbols all have Metrics.OriginX at AlignmentX == 0, and are not moved.
        /// OutputRestSymbols can have Metrics.OriginX at some other value, but they are not moved either.
        /// Other NoteObjects are moved left so that they don't overlap the object on their right.
        /// The left-right order is Clef-KeySignature-Barline-TimeSignature-DurationSymbol.
        ///                      Or Clef-KeySignature-TimeSignature-RepeatBeginBarline-DurationSymbol.
        /// The finalBarline has no DurationSymbols. Its noteObjects are right-aligned to 0.
        /// </summary>
        public void SetInternalXPositions(double gap)
        {
            M.Assert(AlignmentX == 0F);

            var timeSignatures = new List<NoteObject>();
            var keySignatures = new List<NoteObject>();
            var barlines = new List<NoteObject>();
            var clefs = new List<NoteObject>();

            bool momentContainsRepeatBeginBarline = false;

            double minLeft = double.MaxValue;
            #region get typed objectLists and minLeft
            foreach(var noteObject in _noteObjects)
            {
                if(noteObject is DurationSymbol ds)
                {
                    if(ds is OutputChordSymbol ocs)
                    {
                        M.Assert(ocs.Metrics.OriginX == 0);
                    }
                    minLeft = (minLeft < ds.Metrics.Left) ? minLeft : ds.Metrics.Left;
                }
                else if(noteObject is TimeSignature ts)
                {
                    timeSignatures.Add(ts);
                }
                else if(noteObject is KeySignature ks)
                {
                    keySignatures.Add(ks);
                }
                else if(noteObject is Barline b)
                {
                    barlines.Add(b);
                }
                else if(noteObject is Clef c)
                {
                    clefs.Add(c);
                }
            }

            if(minLeft < double.MaxValue) // this moment contains one or more DurationSymbols
            {
                if(timeSignatures.Count > 0)
                {
                    minLeft -= 0; // padding between DurationSymbol and TimeSig
                }
                else if(clefs.Count == 0)
                {
                    // put the keysig next to the timeSig or durationSymbol
                    if(keySignatures.Count > 0)
                    {
                        minLeft -= gap / 3; // padding between DurationSymbol and KeySig
                    }
                    else if(barlines.Count > 0)
                    {
                        minLeft -= gap * 1; // padding between DurationSymbol and Barline
                    }
                }
                else
                {
                    if(barlines.Count > 0)
                    {
                        minLeft -= gap * 1; // padding between DurationSymbol and Barline
                    }
                    if(keySignatures.Count > 0)
                    {
                        minLeft -= 0; // padding between DurationSymbol and KeySig
                    }

                    else if(clefs.Count > 0)
                    {
                        minLeft -= gap / 2; // padding between DurationSymbol and Clef
                    }

                }
            }
            else // finalBarline
            {
                minLeft = 0;
            }
            #endregion

            if(barlines.Count > 0 && barlines[0] is RepeatBeginBarline)
            {
                momentContainsRepeatBeginBarline = true;
                minLeft -= gap / 2; // padding between Chord and RepeatBeginBarline
                Move(barlines, ref minLeft);
                minLeft += gap / 2; // padding between RepeatBeginBarline and symbol on left.
            }

            if(timeSignatures.Count > 0)
            {
                Move(timeSignatures, ref minLeft);

                if(clefs.Count == 0 && keySignatures.Count > 0)
                {
                    minLeft -= 0; // padding between TimeSignature and KeySignature
                }
                if(barlines.Count > 0)
                {
                    if(momentContainsRepeatBeginBarline)
                    {
                        minLeft += gap / 4; // padding between TimeSignature and Clef
                    }
                    else
                    {
                        minLeft -= gap / 3; // padding between TimeSignature and Barline
                    }
                }
            }

            if(keySignatures.Count > 0)
            {
                if(clefs.Count > 0)
                {
                    // Put clef and keySig together:
                    // left to right order is Clef, KeySignature, Barline, TimeSignature
                    if(barlines.Count > 0)
                    {
                        Move(barlines, ref minLeft);
                        // padding between barline and KeySignature or Clef is 0.
                    }
                    Move(keySignatures, ref minLeft);
                    // padding between KeySignature and Clef is 0.
                }
                else
                {
                    // Put keySig right of barline:
                    // left to right order is Clef, Barline, KeySignature, TimeSignature
                    Move(keySignatures, ref minLeft);
                    if(barlines.Count > 0)
                    {
                        minLeft -= gap / 2; // padding between keySignature and Barline
                        Move(barlines, ref minLeft);
                        // padding between Barline and Clef is 0.
                    }
                }
            }
            else if(barlines.Count > 0 && ! (barlines[0] is RepeatBeginBarline))
            {
                Move(barlines, ref minLeft);
                // padding between Barline and Clef is 0.
            }

            foreach(var clef in clefs)
            {
                clef.Metrics.Move(minLeft - clef.Metrics.Right, 0);
            }
        }

        private static void Move(List<NoteObject> noteObjects, ref double minLeft)
        {
            double localLeft = double.MaxValue;
            foreach(var noteObject in noteObjects)
            {
                noteObject.Metrics.Move(minLeft - noteObject.Metrics.Right, 0);
                localLeft = (localLeft < noteObject.Metrics.Left) ? localLeft : noteObject.Metrics.Left;
            }
            minLeft = localLeft;
        }

        public void ShowWarning_ControlsMustBeInTopVoice(DurationSymbol durationSymbol)
        {
            ShowVoiceWarning(durationSymbol, "control symbol");
        }
        public void ShowWarning_DynamicsMustBeInTopVoice(DurationSymbol durationSymbol)
        {
            ShowVoiceWarning(durationSymbol, "dynamic");
        }
        public void ShowVoiceWarning(DurationSymbol durationSymbol, string type)
        {
            int staffIndex = 0;
            int voiceIndex = 0;
            Staff staff = durationSymbol.Voice.Staff;
            foreach(Staff testStaff in staff.SVGSystem.Staves)
            {
                if(staff == testStaff)
                    break;
                staffIndex++;
            }
            foreach(Voice voice in staff.Voices)
            {
                if(voice == durationSymbol.Voice)
                    break;
                voiceIndex++;
            }
            string msg = "Found a " + type + " at\n" +
                        "    millisecond position " + durationSymbol.AbsMsPosition + "\n" +
                        "    staff index " + staffIndex.ToString() + "\n" +
                        "    voice index " + voiceIndex.ToString() + "\n\n" +
                        "Controls which are not attached to the top voice\n" +
                        "in a staff will be ignored.";
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void Add(NoteObject noteObject)
        {
            if(noteObject is DurationSymbol durationSymbol)
            {
                M.Assert(_absMsPosition == durationSymbol.AbsMsPosition);
            }
            _noteObjects.Add(noteObject);
        }

        public IEnumerable AnchorageSymbols
        {
            get
            {
                foreach(NoteObject noteObject in _noteObjects)
                {
					if(noteObject is AnchorageSymbol anchorageSymbol)
						yield return anchorageSymbol;
				}
            }
        }

        public IEnumerable ChordSymbols
        {
            get
            {
                foreach(ChordSymbol chordSymbol in _noteObjects)
                    yield return chordSymbol;
            }
        }

        public double AlignmentX = 0;

        /// <summary>
        /// The logical position in milliseconds from the beginning of the score.
        /// </summary>
        public int AbsMsPosition { get { return _absMsPosition; } }
        private readonly int _absMsPosition = -1;

        public List<NoteObject> NoteObjects { get { return _noteObjects; } }
        private List<NoteObject> _noteObjects = new List<NoteObject>();
    }
}

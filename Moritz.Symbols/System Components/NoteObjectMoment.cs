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
        /// Moves NoteObjects in this NoteObjectMoment to their correct positions with respect to the aligning DurationSymbol.
        /// The duration symbol remains where it is with Metrics.OriginX at AlignmentX == 0.
        /// </summary>
        public void SetInternalXPositions(double gap)
        {
            M.Assert(AlignmentX == 0F);

            DurationSymbol ds = (DurationSymbol)_noteObjects.Find(obj => obj is DurationSymbol);
            TimeSignature ts = (TimeSignature)_noteObjects.Find(obj => obj is TimeSignature);
            KeySignature ks = (KeySignature)_noteObjects.Find(obj => obj is KeySignature);
            Barline b = (Barline)_noteObjects.Find(obj => obj is Barline);
            Clef c = (Clef)_noteObjects.Find(obj => obj is Clef);
            SmallClef sc = (SmallClef)_noteObjects.Find(obj => obj is SmallClef);

            double leftEdge = 0;
            if(ds != null)
            {
                leftEdge = ds.Metrics.Left;
                if(ts != null)
                {
                    ts.Metrics.Move(leftEdge - ts.Metrics.Right, 0);
                    leftEdge = ts.Metrics.Left;
                }
                if(ks != null)
                {
                    ks.Metrics.Move(leftEdge - ks.Metrics.Right, 0);
                    leftEdge = ks.Metrics.Left - (gap * 0.4);
                }
                if(b != null)
                {
                    if(ts != null && ks == null)
                    {
                        leftEdge -= (gap * 0.5);
                    }
                    else if(ts == null && ks == null)
                    {
                        leftEdge = (ds is OutputRestSymbol) ? leftEdge -= (gap * 0.4) : leftEdge -= (gap * 0.8);
                    }
                    b.Metrics.Move(leftEdge - b.Metrics.Right, 0);
                    leftEdge = b.Metrics.Left;
                }
                if(c != null)
                {
                    c.Metrics.Move(leftEdge - c.Metrics.Right, 0);
                    leftEdge = c.Metrics.Left;
                }
                if(sc != null)
                {
                    sc.Metrics.Move(leftEdge - sc.Metrics.Right, 0);
                    leftEdge = sc.Metrics.Left;
                }
            }
            else // final barline
            {
                if(ts != null)
                {
                    ts.Metrics.Move(0 - ts.Metrics.Right, 0);
                    leftEdge = ts.Metrics.Left - (gap * 0.5);
                }
                if(b != null)
                {
                    b.Metrics.Move(leftEdge - b.Metrics.OriginX, 0);
                    leftEdge = b.Metrics.Left - (gap * 0.5);
                }
                if(ks != null)
                {
                    ks.Metrics.Move(leftEdge - ks.Metrics.Right, 0);
                    leftEdge = ks.Metrics.Left;
                }
                if(sc != null)
                {
                    sc.Metrics.Move(leftEdge - sc.Metrics.Right, 0);
                    leftEdge = sc.Metrics.Left;
                }
            }
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

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

            double minBarlineOriginX = double.MaxValue;
            for(var i = 0; i < _noteObjects.Count; i++)
            {
                if(_noteObjects[i] is Barline b && b.Metrics != null)
                {
                    if(b.Metrics.OriginX < minBarlineOriginX)
                    {
                        minBarlineOriginX = b.Metrics.OriginX;
                    }
                }
			}
            for(int index = 0; index < _noteObjects.Count; index++)
            {
                if(_noteObjects[index] is Barline barline && barline.Metrics != null)
                {
                    if(index > 0)
                    {
                        if(_noteObjects[index - 1] is KeySignature keySignature)
                        {
                            keySignature.Metrics.Move(minBarlineOriginX - keySignature.Metrics.Right, 0);
                            if(_noteObjects[index - 2] is Clef kClef)
                            {
                                kClef.Metrics.Move(keySignature.Metrics.Left - kClef.Metrics.Right, 0);
                            }
                        }
                        else if(_noteObjects[index - 1] is Clef clef)
                        {
                            clef.Metrics.Move(minBarlineOriginX - clef.Metrics.Right, 0);
                        }
                    }
                    barline.Metrics.Move(minBarlineOriginX - barline.Metrics.OriginX, 0);
                }

                if(_noteObjects[index] is ChordSymbol chordSymbol && chordSymbol.Metrics != null)
                {
                    if(index > 0)
                    {
                        double dx = 0;
                        var prevNoteObject = _noteObjects[index - 1];

                        if(prevNoteObject is SmallClef smallClef)
                        {
                            smallClef.Metrics.Move(chordSymbol.Metrics.Left - smallClef.Metrics.Right + gap, 0);
                        }
                        if(prevNoteObject is Barline)
                        {
                            dx = -(gap / 2);
                            foreach(var noteObj in _noteObjects)
                            {
                                if(!(noteObj is ChordSymbol))
                                {
                                    noteObj.Metrics.Move(dx, 0);
                                }
                            }
                        }
                    }
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

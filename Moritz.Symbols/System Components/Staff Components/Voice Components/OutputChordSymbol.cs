using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using MNX.Globals;
using Moritz.Spec;
using Moritz.Xml;
using System;
using MNX.Common;

namespace Moritz.Symbols
{
    public class OutputChordSymbol : ChordSymbol
    {
        public OutputChordSymbol(Voice voice, MNX.Common.Event mnxEventDef, int absMsPosition, PageFormat pageFormat)
            : base(voice, mnxEventDef.MsDuration, absMsPosition, mnxEventDef, pageFormat.MusicFontHeight, true)
        {
            SetHeads(mnxEventDef);
        }

        private void SetHeads(Event mnxEventDef)
        {
            foreach(var note in mnxEventDef.Notes)
            {
                Head head = new Head(this, note);
                HeadsTopDown.Add(head);
            }
            HeadsTopDown.Sort((a, b) => (M.MidiPitchDict[b.Pitch] - M.MidiPitchDict[a.Pitch]));
        }

        /// <summary>
        /// Old constructor, currently not used (03.05.2020), but retained for future inspection
        /// </summary>
        public OutputChordSymbol(Voice voice, MidiChordDef umcd, int absMsPosition, PageFormat pageFormat)
            : base(voice, umcd.MsDuration, absMsPosition, pageFormat.MinimumCrotchetDuration, pageFormat.MusicFontHeight, umcd.BeamContinues)
        {
            M.Assert(false); // 03.05.2020: don't use this constructor (to be inspected once work on midi info begins).

            _midiChordDef = umcd;

            _msDurationToNextBarline = umcd.MsDurationToNextBarline;

            SetNoteheadPitchesAndVelocities(umcd.NotatedMidiPitches, umcd.NotatedMidiVelocities);

            if(! String.IsNullOrEmpty(umcd.OrnamentText))
            {
				string ornamentString = null;
				if(Char.IsDigit(umcd.OrnamentText[0]))
				{
					// if umcd.OrnamentText is null or empty, there will be no ornamentString DrawObject
					ornamentString = String.Concat('~', umcd.OrnamentText);
				}
				else
				{
					ornamentString = umcd.OrnamentText;
				}
				OrnamentText ornamentText = new OrnamentText(this, ornamentString, pageFormat.OrnamentFontHeight);
				DrawObjects.Add(ornamentText);
			}

            if(umcd.Lyric != null)
            {
				LyricText lyric = new LyricText(this, umcd.Lyric, FontHeight);
                DrawObjects.Add(lyric);
            }
        }

        /// <summary>
        /// used by CautionaryOutputChordSymbol
        /// </summary>
        public OutputChordSymbol(Voice voice, int msDuration, int absMsPosition, int minimumCrotchetDurationMS, double fontSize)
            : base(voice, msDuration, absMsPosition, minimumCrotchetDurationMS, fontSize, false)
        {

        }

        /// <summary>
        /// This function uses a sophisticated algorithm to decide whether flats or sharps are to be used to
        /// represent the chord. Chords can have naturals and either sharps or flats (but not both).
        /// The display of naturals is forced if the same notehead height also exists with a sharp or flat.
        /// (The display of other accidentals are always forced in the Head constructor.)
        /// Exceptions: This function throws an exception if
        ///     1) any of the input midiPitches is out of midi range (0..127).
        ///     2) the midiPitches are not in ascending order
        ///     3) the midiPitches are not unique.
        /// The midiPitches argument must be in order of size (ascending), but Heads are created in top-down order.
        /// </summary>
        /// <param name="midiPitches"></param>
        public void SetNoteheadPitchesAndVelocities(List<byte> midiPitches, List<byte> midiVelocities)
        {
            #region check inputs
             M.Assert(midiPitches.Count == midiVelocities.Count);
            int previousPitch = -1;
            foreach(int midiPitch in midiPitches)
            {
                M.Assert(midiPitch >= 0 && midiPitch <= 127, "midiPitch out of range.");
                M.Assert(midiPitch > previousPitch, "midiPitches must be unique and in ascending order.");
                previousPitch = midiPitch;
            }
            foreach(int midiVelocity in midiVelocities)
            {
                M.Assert(midiVelocity >= 0 && midiVelocity <= 127, "midiVelocity out of range.");
            }
            #endregion
            this.HeadsTopDown.Clear();
            bool useSharp = UseSharps(midiPitches, midiVelocities); // returns false if flats are to be used
            for(int i = midiPitches.Count - 1; i >= 0; --i)
            {
                Head head = new Head(this, midiPitches[i], midiVelocities[i], useSharp);
                this.HeadsTopDown.Add(head);
            }
            for(int i = 0; i < midiPitches.Count; i++)
            {
                if(this.HeadsTopDown[i].Alteration == 0)
                {
                    this.HeadsTopDown[i].DisplayAccidental = DisplayAccidental.suppress;
                }
            }
            for(int i = 1; i < midiPitches.Count; i++)
            {
                if(this.HeadsTopDown[i].Pitch == this.HeadsTopDown[i - 1].Pitch)
                {
                    this.HeadsTopDown[i - 1].DisplayAccidental = DisplayAccidental.force;
                    this.HeadsTopDown[i].DisplayAccidental = DisplayAccidental.force;
                }
            }
        }

        internal void SetNoteheadColorClasses()
        {
            foreach(Head head in HeadsTopDown)
            {
                M.Assert(head.MidiVelocity >= 0 && head.MidiVelocity <= 127);

                int velocity = head.MidiVelocity;
                if(velocity > M.MaxMidiVelocity[M.Dynamic.ff])
                {
                    head.ColorClass = CSSColorClass.fffColor;    
                }
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.f])
                {
                    head.ColorClass = CSSColorClass.ffColor;
                }
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.mf])
                {
                    head.ColorClass = CSSColorClass.fColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.mp])
                {
                    head.ColorClass = CSSColorClass.mfColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.p])
                {
                    head.ColorClass = CSSColorClass.mpColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.pp])
                {
                    head.ColorClass = CSSColorClass.pColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.ppp])
                {
                    head.ColorClass = CSSColorClass.ppColor;
				}
                else if(velocity > M.MaxMidiVelocity[M.Dynamic.pppp])
                {
                    head.ColorClass = CSSColorClass.pppColor;
				}
                else // > 0 
                {
                    head.ColorClass = CSSColorClass.ppppColor;
				}
            }
        }

		/// <summary>
		/// This function should never be used. Use the other WriteSVG() instead.
		/// </summary>
		/// <param name="w"></param>
		public override void WriteSVG(SvgWriter w)
		{
			M.Assert(false, "This function should never be called.");
		}

		public void WriteSVG(SvgWriter w, int channel, CarryMsgs carryMsgs, bool graphicsOnly)
        {
            if(ChordMetrics.BeamBlock != null)
                ChordMetrics.BeamBlock.WriteSVG(w);

			w.SvgStartGroup(CSSObjectClass.chord.ToString()); // "chord"
			if(!graphicsOnly)
			{
                w.WriteAttributeString("score", "alignment", null, ChordMetrics.OriginX.ToString(M.En_USNumberFormat));

                _midiChordDef.WriteSVG(w, channel, carryMsgs);
            }

            w.SvgStartGroup(CSSObjectClass.graphics.ToString());
            ChordMetrics.WriteSVG(w);
            w.SvgEndGroup();

            w.SvgEndGroup(); // "chord"
        }

        internal void AddSlurTemplate(double slurBeginX, double slurBeginY, double slurEndX, double slurEndY, double gap, bool isOver, double endAngle)
        {
            // The SVG scale is such that there is no problem using integers here.

            double dyControl = gap * 3;
            double dxControl = dyControl / Math.Tan(endAngle);
            
            double x1 = slurBeginX;
            double y1 = slurBeginY;
            double x4 = slurEndX;
            double y4 = slurEndY;

            SlurTemplate slurTemplate = null;
            double shortSlurMaxWidth = gap * 20; // 5 staff heights
            double shortSlurMinWidth = gap * 9.5; // 4.4 is shortTieMaxWidth
            double width = x4 - x1;
            if(width <= shortSlurMaxWidth)
            {
                if(width <= shortSlurMinWidth)
                {
                    double factor = width / shortSlurMinWidth;
                    dxControl *= factor;
                    dyControl *= factor;
                }
                // short (=two-point) slur template
                // standard Bezier points
                var p1 = new Point((int)x1, (int)y1);
                var p2 = (isOver) ? new Point((int)(x1 + dxControl), (int)(y1 - dyControl)) : new Point((int)(x1 + dxControl), (int)(y1 + dyControl));
                var p4 = new Point((int)x4, (int)y4);
                var p3 = (isOver) ? new Point((int)(x4 - dxControl), (int)(y4 - dyControl)) : new Point((int)(x4 - dxControl), (int)(y4 + dyControl));

                slurTemplate = new SlurTemplate(p1, p2, p3, p4, gap, isOver);
            }
            else
            {
                // long (=three-point) slur template
                var p1 = new Point((int)x1, (int)y1);
                var c1 = (isOver) ? new Point((int)(x1 + dxControl), (int)(y1 - dyControl)) : new Point((int)(x1 + dxControl), (int)(y1 + dyControl));
                var p3 = new Point((int)x4, (int)y4);
                var c3 = (isOver) ? new Point((int)(x4 - dxControl), (int)(y4 - dyControl)) : new Point((int)(x4 - dxControl), (int)(y4 + dyControl));
                var p2 = new Point((int)((x1 + x4) / 2), (int)((y1 + c1.Y) / 2));
                var c2 = new Point((p1.X + p2.X) / 2, p2.Y);

                slurTemplate = new SlurTemplate(p1, c1, c2, p2, c3, p3, gap, isOver);
            }

            ChordMetrics.SlurTemplates.Add(slurTemplate); // So that the slurTemplate will be written to SVG.
            ChordMetrics.AddSlurTieMetrics((SlurTieMetrics)slurTemplate.Metrics); // So that the tie will be moved vertically with the system.
        }

        /// <summary>
        /// Tie templates have similar point and control point structures as a slurs, except that
        /// 1. they are both horizontal and symmetric.
        /// 2. the maximum width of a short tie is twice the width of a tie hook.
        /// 3. long slurs have three points on the slur, long ties have four points on the tie.
        /// </summary>
        /// <param name="tieTemplateBeginX"></param>
        /// <param name="tieBeginY"></param>
        /// <param name="tieTemplateEndX"></param>
        /// <param name="tieTemplateY"></param>
        /// <param name="gap"></param>
        /// <param name="isOver"></param>
        internal void AddTieTemplate(double tieTemplateBeginX, double tieTemplateEndX, double tieTemplateY, double gap, bool isOver)
        {
            // The SVG scale is such that there should be no problem using integers here.
            TieTemplate tieTemplate = null;

            double p1x = tieTemplateBeginX;
            double p1y = tieTemplateY;
            double p2x = tieTemplateEndX;
            double p2y = tieTemplateY;

            double shortTieMaxWidth = gap * 5;
            double veryShortTieMaxWidth = gap * 3.45;
            double width = p2x - p1x;

            double deltaControl; // the x and y distance between an end-point on the template and its related control point.

            if(width <= shortTieMaxWidth)
            {
                if(width <= veryShortTieMaxWidth)
                {                    
                    deltaControl = width / 3;
                    // widen the tie
                    width = (width < gap) ? gap : width; // just to guard against divide by zero error in next line
                    double deltaWidth = (veryShortTieMaxWidth - width) * 0.35 * (veryShortTieMaxWidth / width);
                    p1x -= deltaWidth;
                    p2x += deltaWidth;
                }
                else
                {
                    deltaControl = gap * 1.15;
                }

                var p1 = new Point((int)p1x, (int)p1y);
                var c1 = (isOver) ? new Point((int)(p1x + deltaControl), (int)(p1y - deltaControl)) : new Point((int)(p1x + deltaControl), (int)(p1y + deltaControl));
                var p2 = new Point((int)p2x, (int)p2y);
                var c2 = (isOver) ? new Point((int)(p2x - deltaControl), (int)(p2y - deltaControl)) : new Point((int)(p2x - deltaControl), (int)(p2y + deltaControl));

                tieTemplate = new ShortTieTemplate(p1, c1, c2, p2, gap, isOver);
            }
            else
            {
                deltaControl = (gap * 0.85);

                // long tie template
                var p1 = new Point((int)p1x, (int)p1y);
                var c1 = (isOver) ? new Point((int)(p1x + deltaControl), (int)(p1y - deltaControl)) : new Point((int)(p1x + deltaControl), (int)(p1y + deltaControl));
                var tc = new Point(c1.X + (int)(deltaControl / 2), c1.Y);
                var tp = new Point((int)(p1x + (p2x - p1x) / 2), c1.Y);                
                var c2 = (isOver) ? new Point((int)(p2x - deltaControl), (int)(p2y - deltaControl)) : new Point((int)(p2x - deltaControl), (int)(p2y + deltaControl));
                var p2 = new Point((int)p2x, (int)p2y);

                tieTemplate = new LongTieTemplate(p1, c1, tc, tp, c2, p2, gap, isOver);
            }

            ChordMetrics.TieTemplates.Add(tieTemplate); // So that the slurTemplate will be written to SVG.
            ChordMetrics.AddSlurTieMetrics((SlurTieMetrics)tieTemplate.Metrics); // So that the tie will be moved vertically with the system.
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("outputChord  ");
            sb.Append(InfoString);
            return sb.ToString();
        }

        public MidiChordDef MidiChordDef { get { return _midiChordDef; } }
        protected MidiChordDef _midiChordDef = null;
    }
}

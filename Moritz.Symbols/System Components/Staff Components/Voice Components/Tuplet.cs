using System.Collections.Generic;
using System.Drawing;

using MNX.Common;
using MNX.Globals;

using Moritz.Xml;

namespace Moritz.Symbols
{

    /// <summary>
    /// Tuplets (and their Metrics) are created *after* the noteheads they span have moved to their final
    /// left-right positions on the system, but before the system has moved to its final vertical position.
    /// Tuplet Metrics are therefore only ever moved vertically.
    /// </summary>
    internal class Tuplet : DrawObject
    {
        /// <summary>
        /// noteObjects[noteObjectIndex] is the first OutputChordSymbol or OutputRestSymbol in the tuplet, and is the
        /// noteObject to which the tuplet is attached,
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="tupletDef"></param>
        /// <param name="noteObjects"></param>
        /// <param name="noteObjectIndex"></param>
        public Tuplet(Graphics graphics, TupletDef tupletDef, List<NoteObject> noteObjects, int noteObjectIndex)
            : base(noteObjects[noteObjectIndex])
        {
            List<NoteObject> tupletChordsAndRests = GetTupletChordsAndRests(noteObjects, noteObjectIndex, tupletDef);

            var gap = M.PageFormat.GapVBPX;
            var textInfo = GetTupletTextInfo(tupletDef.InnerDuration, tupletDef.OuterDuration);

            Metrics = new TextMetrics(CSSObjectClass.tupletText, graphics, textInfo);

            var textHeight = (Metrics.Bottom - Metrics.Top);
            var textWidth = (Metrics.Right - Metrics.Left);
            bool isOver = (tupletDef.Orient == Orientation.up);
            double textXAlignment;
            if(tupletChordsAndRests.Count > 2)
            {
                int alignedIndex = (int)(tupletChordsAndRests.Count) / 2;
                Metrics metrics = tupletChordsAndRests[alignedIndex].Metrics;
                if(metrics is ChordMetrics cMetrics)
                {
                    textXAlignment = cMetrics.OriginX - textWidth / 4;
                }
                else
                {
                    textXAlignment = ((metrics.Right - metrics.Left) / 2) + metrics.Left;
                }
            }
            else
            {
                M.Assert(tupletChordsAndRests.Count == 2);
                Metrics metrics1 = tupletChordsAndRests[0].Metrics;
                Metrics metrics2 = tupletChordsAndRests[1].Metrics;

                textXAlignment = ((metrics1.Left + metrics2.Right) / 2) - textWidth / 4;
            }

            //textYAlignment = (isOver) ? metrics.Top - gap - (textHeight / 2) : metrics.Bottom + gap + (textHeight / 2);
            double textYAlignment = (isOver) ? double.MaxValue : double.MinValue; ;
            foreach(NoteObject noteObject in tupletChordsAndRests)
            {
                var metrics = noteObject.Metrics;
                if(isOver)
                {
                    textYAlignment = (metrics.Top < textYAlignment) ? metrics.Top : textYAlignment;
                }
                else
                {
                    textYAlignment = (metrics.Bottom > textYAlignment) ? metrics.Bottom : textYAlignment;
                }
            }

            #region move vertically off the staff if necessary
            StaffMetrics staffMetrics = tupletChordsAndRests[0].Voice.Staff.Metrics;
            if(isOver)
            {
                double topMax = staffMetrics.StafflinesTop - gap - (textHeight / 2);
                if(textYAlignment > topMax)
                {
                    textYAlignment = topMax;
                }
            }
            else
            {
                double topMin = staffMetrics.StafflinesBottom + gap + (textHeight / 2);
                if(textYAlignment < topMin)
                {
                    textYAlignment = topMin;
                }
            }
            #endregion

            Metrics.Move(textXAlignment, textYAlignment + (textHeight / 2));

            // set auto correctly later -- depends on beaming
            if(tupletDef.Bracket == TupletBracketDisplay.yes || tupletDef.Bracket == TupletBracketDisplay.auto)
            {
                double bracketHoriz = textYAlignment;
                double bracketLeft = tupletChordsAndRests[0].Metrics.Left - M.PageFormat.StafflineStemStrokeWidthVBPX;
                double bracketRight = tupletChordsAndRests[tupletChordsAndRests.Count - 1].Metrics.Right + M.PageFormat.StafflineStemStrokeWidthVBPX;
                double bracketHeight = gap * 0.75;

                _textAndBracketMetrics = new TupletMetrics((TextMetrics)Metrics, bracketHoriz, bracketLeft, bracketRight, bracketHeight, isOver);
            }
        }

        private List<NoteObject> GetTupletChordsAndRests(List<NoteObject> noteObjects, int noteObjectIndex, TupletDef tupletDef)
        {
            List<NoteObject> chordsAndRests = new List<NoteObject>();
            int nEvents = tupletDef.IEventsAndGraces.FindAll(e => e is Event).Count; // TODO: Tuplets can now nest and include Grace notes

            for(int i = noteObjectIndex; i < noteObjects.Count; ++i)
            {
                if(noteObjects[i] is OutputChordSymbol || noteObjects[i] is OutputRestSymbol)
                {
                    chordsAndRests.Add(noteObjects[i]);
                    if(chordsAndRests.Count == nEvents)
                    {
                        break;
                    }
                }
            }

            return chordsAndRests;
        }

        private TextInfo GetTupletTextInfo(MNXDurationSymbol innerDuration, MNXDurationSymbol outerDuration)
        {
            M.Assert(innerDuration.DurationSymbolTyp == outerDuration.DurationSymbolTyp);
            int outMult = (int)outerDuration.Multiple;
            string text;
            switch(outMult)
            {
                case 2:
                case 4:
                case 8:
                case 16:
                    {
                        text = ((int)innerDuration.Multiple).ToString();
                        break;
                    }
                default:
                    {
                        text = ((int)innerDuration.Multiple).ToString() + ":" + outMult.ToString();
                        break;
                    }
            }

            //text = ((int)innerDuration.Multiple).ToString() + ":" + outMult.ToString();

            return new TextInfo(text, "Open Sans Condensed", M.PageFormat.TupletFontHeight, SVGFontWeight.bold, SVGFontStyle.italic, TextHorizAlign.center);
        }


        private readonly TupletMetrics _textAndBracketMetrics = null;

        internal virtual void Move(double dy)
        { 
            if(_textAndBracketMetrics != null)
            {
                _textAndBracketMetrics.Move(0, dy); // moves both text and brackets
            }
            else
            {
                Metrics.Move(0, dy); // only moves the text
            }
        }

        public override void WriteSVG(SvgWriter w)
        {
            if(_textAndBracketMetrics != null)
            {
                _textAndBracketMetrics.WriteSVG(w); // writes both text and brackets
            }
            else
            {
                Metrics.WriteSVG(w); // only writes the text
            }
        }
    }
}
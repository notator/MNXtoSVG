using Moritz.Xml;
using MNX.Globals;
using System.Drawing;
using System;

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
        /// A tuplet consisting only of the text (no brackets)
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="text"></param>
        /// <param name="isOver"></param>
        internal Tuplet(NoteObject container, TextMetrics textMetrics)
            : base(container)
        {
            Metrics = textMetrics;
        }

        internal void AddBrackets(TextMetrics textMetrics, double bracketHoriz, double bracketLeft, double bracketRight, double bracketHeight, bool isOver)
        {
            _textAndBracketMetrics = new TupletMetrics(textMetrics, bracketHoriz, bracketLeft, bracketRight, bracketHeight, isOver);
        }


        private TupletMetrics _textAndBracketMetrics = null;

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
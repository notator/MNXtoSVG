
using Moritz.Xml;

namespace Moritz.Symbols
{
	public class Stem
	{
        public Stem(ChordSymbol chordSymbol)
        {
            Chord = chordSymbol;
        }

        public readonly ChordSymbol Chord;

		public VerticalDir Direction = VerticalDir.none;
        public bool Draw = true; // set to false for cautionary chords
	}
}

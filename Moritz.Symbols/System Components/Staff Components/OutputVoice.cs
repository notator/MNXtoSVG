using Moritz.Spec;
using Moritz.Xml;
using System.Collections.Generic;

namespace Moritz.Symbols
{
    public class OutputVoice : Voice
    {
        public OutputVoice(OutputStaff outputStaff, int midiChannel)
            : base(outputStaff)
        {
            MidiChannel = midiChannel;
        }

		public override void WriteSVG(SvgWriter w, int voiceIndex, List<CarryMsgs> carryMsgsPerChannel, bool graphicsOnly)
        {
			w.SvgStartGroup(CSSObjectClass.voice.ToString());

            base.WriteSVG(w, voiceIndex, carryMsgsPerChannel, graphicsOnly);
            w.SvgEndGroup(); // outputVoice
        }
    }
}

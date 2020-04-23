using System;
using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-score-audio
    internal class ScoreAudio
    {
        public ScoreAudio(XmlReader r)
        {
            M.Assert(r.Name == "score-audio");            

            throw new NotImplementedException();
        }
    }
}
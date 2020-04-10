using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Note : IWritable
    {
        public Note(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#elementdef-note
            G.Assert(r.Name == "note");

        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
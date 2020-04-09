using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Directions : IWritable
    {
        public Directions(XmlReader r)
        {
            G.Assert(r.Name == "directions");
            // https://w3c.github.io/mnx/specification/common/#elementdef-directions
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
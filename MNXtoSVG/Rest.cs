using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Rest : IWritable
    {
        public Rest(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#elementdef-rest
            G.Assert(r.Name == "rest");

        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
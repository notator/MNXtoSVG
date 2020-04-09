using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Collection : IWritable
    {        
        public Collection(XmlReader r)
        {
            G.Assert(r.Name == "collection");
            // https://w3c.github.io/mnx/specification/common/#elementdef-collection

            throw new NotImplementedException();
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
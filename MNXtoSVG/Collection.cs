using MNXtoSVG.Globals;
using System;
using System.Xml;

namespace MNXtoSVG
{
    internal class Collection
    {
        // https://w3c.github.io/mnx/specification/common/#elementdef-collection
        public Collection(XmlReader r)
        {
            G.Assert(r.Name == "collection");

            throw new NotImplementedException();
        }

    }
}
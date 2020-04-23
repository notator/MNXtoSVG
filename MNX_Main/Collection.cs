using System;
using System.Xml;
using MNX.Globals;

namespace MNX_Main
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-collection
    internal class Collection
    {        
        public Collection(XmlReader r)
        {
            M.Assert(r.Name == "collection");           

            throw new NotImplementedException();
        }
    }
}
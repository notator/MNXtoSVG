using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Event : IWritable
    {
        public Event(XmlReader r)
        {
            G.Assert(r.Name == "event");
            // https://w3c.github.io/mnx/specification/common/#the-event-element

        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}

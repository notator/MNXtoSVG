using MNX.AGlobals;
using System;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-rest
    public class Rest : ITicks, IEventComponent
    {
        public Rest(XmlReader r)
        {
            A.Assert(r.Name == "rest");
        }

        public int Ticks => throw new NotImplementedException();

    }
}
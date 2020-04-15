using MNX.AGlobals;
using System;
using System.Xml;

namespace MNX.Common
{
    public class Rest : ISequenceComponent, ITicks, IEventComponent
    {
        public Rest(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#elementdef-rest
            A.Assert(r.Name == "rest");
        }

        public int Ticks => throw new NotImplementedException();

    }
}
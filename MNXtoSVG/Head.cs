using MNXtoSVG.Globals;
using System;
using System.Xml;

namespace MNXtoSVG
{
    internal class Head
    {
        // https://w3c.github.io/mnx/specification/common/#the-score-element
        public Head(XmlReader r)
        {
            G.Assert(r.Name == "head");

            throw new NotImplementedException();

        }

    }
}
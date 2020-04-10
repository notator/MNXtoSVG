using System;
using System.Xml;

namespace MNXtoSVG
{
    internal abstract class DirectionClass
    {
        public readonly DirectionAttributes DirectionAttributes = new DirectionAttributes();

        internal virtual bool SetAttribute(XmlReader r)
        {
            bool rval = DirectionAttributes.SetAttribute(r);

            return rval;
        }
    }

    internal abstract class SpanClass : DirectionClass
    {
        public readonly SpanAttributes SpanAttributes = new SpanAttributes();

        internal override bool SetAttribute(XmlReader r)
        {
            bool rval = base.SetAttribute(r);
            if(rval == false)
            {
                rval = SpanAttributes.SetAttribute(r);
            }

            return rval;
        }
    }
}
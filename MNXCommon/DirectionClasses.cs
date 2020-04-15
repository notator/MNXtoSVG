using System.Xml;

namespace MNX.Common
{
    public abstract class Direction
    {
        public readonly DirectionAttributes DirectionAttributes = new DirectionAttributes();

        internal virtual bool SetAttribute(XmlReader r)
        {
            bool rval = DirectionAttributes.SetAttribute(r);

            return rval;
        }
    }

    public abstract class Span : Direction
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
using System.Xml;

namespace MNX.Common
{
    public class SpanAttributes
    {
        public string End { get; private set; }

        public SpanAttributes()
        {
            End = null;
        }

        internal bool SetAttribute(XmlReader r)
        {
            if(r.Name == "end")
            {
                End = r.Value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
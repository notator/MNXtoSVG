using MNX.AGlobals;
using System.Xml;

namespace MNX.Common
{
    public class Tie : Span
    {
        public readonly string Target = null;
        public readonly MNXC_PositionInMeasure Location = null;

        public Tie(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-tied-element
            A.Assert(r.Name == "tied");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "target":
                        Target = r.Value;
                        break;
                    case "location":
                        Location = new MNXC_PositionInMeasure(r.Value);
                        break;
                }
            }
        }
    }
}
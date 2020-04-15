using System.Xml;
using MNX.AGlobals;

namespace MNX.Common
{
    internal class TimeSignature : IDirectionsComponent
    {
        public readonly string Signature;
        public readonly string Measure;

        public TimeSignature(XmlReader r)
        {
            A.Assert(r.Name == "time");

            // https://w3c.github.io/mnx/specification/common/#the-time-element
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "signature":
                        Signature = r.Value;
                        break;
                    case "measure":
                        Measure = r.Value;
                        break;
                    default:
                        A.ThrowError("Unknown time attribute.");
                        break;
                }
            }

            // r.Name is now the name of the last time attribute that has been read.
        }
    }
}
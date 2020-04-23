using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    internal class TimeSignature : IGlobalDirectionsComponent
    {
        public readonly string Signature;
        public readonly string Measure;

        public TimeSignature(XmlReader r)
        {
            M.Assert(r.Name == "time");

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
                        M.ThrowError("Unknown time attribute.");
                        break;
                }
            }

            // r.Name is now the name of the last time attribute that has been read.
        }
    }
}
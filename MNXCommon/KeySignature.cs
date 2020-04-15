using MNX.AGlobals;
using System.Xml;

namespace MNX.Common
{
    internal class KeySignature : IDirectionsComponent
    {
        public readonly int Fifths = 0; // default

        public KeySignature(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-key-element
            A.Assert(r.Name == "key");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "fifths":
                        int.TryParse(r.Value, out Fifths);
                        break;
                    default:
                        A.ThrowError("Unknown key attribute.");
                        break;
                }
            }

            // r.Name is now the name of the last key attribute that has been read.
        }
    }
}
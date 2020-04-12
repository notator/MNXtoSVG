using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class KeySignature : IWritable
    {
        public readonly int Fifths = 0; // default

        public KeySignature(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-key-element
            G.Assert(r.Name == "key");

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
                        G.ThrowError("Unknown key attribute.");
                        break;
                }
            }

            // r.Name is now the name of the last key attribute that has been read.
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
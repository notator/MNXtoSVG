using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class TimeSignature : IWritable, IWritableSequenceComponent
    {
        public readonly string Signature;
        public readonly string Measure;

        public TimeSignature(XmlReader r)
        {
            G.Assert(r.Name == "time");

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
                        G.ThrowError("Unknown time attribute.");
                        break;
                }
            }

            // r.Name is now the name of the last time attribute that has been read.
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
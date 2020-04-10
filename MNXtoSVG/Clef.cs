using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Clef : DirectionClass, IWritable
    {
        public readonly int Line = 0; // 0 means uninitialised. Line must start at 1 (the bottom line of the staff)
        public readonly int Octave = 0; // Default. Octave can be set to any positive or negative integer.
        public readonly G.MNXClefSign Sign = G.MNXClefSign.undefined;

        public Clef(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-clef-element
            G.Assert(r.Name == "clef");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "line":
                        int.TryParse(r.Value, out Line);
                        G.Assert(Line > 0);
                        break;
                    case "sign":
                        Sign = GetMNXClefSign(r.Value);
                        break;
                    case "octave":
                        int.TryParse(r.Value, out Octave);
                        break;
                    default:
                        if(base.SetAttribute(r) == false)
                        {
                            G.ThrowError("Unknown clef attribute.");
                        }
                        break;
                }
            }

            G.Assert(Sign != G.MNXClefSign.undefined && Line > 0);

            // r.Name is now the name of the last clef attribute that has been read.
        }

        private G.MNXClefSign GetMNXClefSign(string value)
        {
            G.MNXClefSign rval = G.MNXClefSign.undefined;

            switch(value)
            {
                case "G":
                    rval = G.MNXClefSign.G;
                    break;
                case "F":
                    rval = G.MNXClefSign.F;
                    break;
                case "C":
                    rval = G.MNXClefSign.C;
                    break;
                case "percussion":
                    rval = G.MNXClefSign.percussion;
                    break;
                case "jianpu":
                    rval = G.MNXClefSign.jianpu;
                    break;
                case "none":
                    rval = G.MNXClefSign.none;
                    break;
            }
            
            return rval;
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
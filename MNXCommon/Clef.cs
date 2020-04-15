﻿using MNX.AGlobals;
using System.Xml;

namespace MNX.Common
{
    internal class Clef : Direction, ISequenceComponent
    {
        public readonly int Line = 0; // 0 means uninitialised. Line must start at 1 (the bottom line of the staff)
        public readonly int Octave = 0; // Default. Octave can be set to any positive or negative integer.
        public readonly ClefType? Sign = null;

        public Clef(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-clef-element
            A.Assert(r.Name == "clef");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "line":
                        int.TryParse(r.Value, out Line);
                        A.Assert(Line > 0);
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
                            A.ThrowError("Unknown clef attribute.");
                        }
                        break;
                }
            }

            A.Assert(Sign != null && Line > 0);

            // r.Name is now the name of the last clef attribute that has been read.
        }

        private ClefType GetMNXClefSign(string value)
        {
            ClefType rval = ClefType.G;

            switch(value)
            {
                case "G":
                    rval = ClefType.G;
                    break;
                case "F":
                    rval = ClefType.F;
                    break;
                case "C":
                    rval = ClefType.C;
                    break;
                case "percussion":
                    rval = ClefType.percussion;
                    break;
                case "jianpu":
                    rval = ClefType.jianpu;
                    break;
                case "none":
                    rval = ClefType.none;
                    break;
                default:
                    A.ThrowError("Error: unknown clef sign");
                    break;
            }
            
            return rval;
        }
    }
}
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class OctaveShift : SpanClass, IWritable
    {
        public readonly MNXOctaveShiftType? Type = null;

        public OctaveShift(XmlReader r)
            : base()
        {
            // https://w3c.github.io/mnx/specification/common/#the-octave-shift-element
            G.Assert(r.Name == "octave-shift");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "type":
                        Type = GetOctaveShiftType(r.Value);
                        break;
                    default:
                        if(base.SetAttribute(r) == false)
                        {
                            G.ThrowError("Error: Unknown attribute name.");
                        }
                        break;
                }
            }

            // r.Name is now the name of the last octave-shift attribute that has been read.
        }

        private MNXOctaveShiftType GetOctaveShiftType(string value)
        {
            MNXOctaveShiftType rval = MNXOctaveShiftType.up1Oct;

            switch(value)
            {
                case "-8":
                    rval = MNXOctaveShiftType.down1Oct;
                    break;
                case "8":
                    rval = MNXOctaveShiftType.up1Oct;
                    break;
                case "-15":
                    rval = MNXOctaveShiftType.down2Oct;
                    break;
                case "15":
                    rval = MNXOctaveShiftType.up2Oct;
                    break;
                case "-22":
                    rval = MNXOctaveShiftType.down3Oct;
                    break;
                case "22":
                    rval = MNXOctaveShiftType.up3Oct;
                    break;
                default:
                    G.ThrowError("Error: unknown octave shift type");
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
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class OctaveShift : SpanClass, IWritable
    {
        public readonly G.MNXOctaveShiftType Type = G.MNXOctaveShiftType.undefined; // default

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

        private G.MNXOctaveShiftType GetOctaveShiftType(string value)
        {
            G.MNXOctaveShiftType rval = G.MNXOctaveShiftType.undefined;

            switch(value)
            {
                case "-8":
                    rval = G.MNXOctaveShiftType.down1Oct;
                    break;
                case "8":
                    rval = G.MNXOctaveShiftType.up1Oct;
                    break;
                case "-15":
                    rval = G.MNXOctaveShiftType.down2Oct;
                    break;
                case "15":
                    rval = G.MNXOctaveShiftType.up2Oct;
                    break;
                case "-22":
                    rval = G.MNXOctaveShiftType.down3Oct;
                    break;
                case "22":
                    rval = G.MNXOctaveShiftType.up3Oct;
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
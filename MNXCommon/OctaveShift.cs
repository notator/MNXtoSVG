using System.Xml;
using MNX.AGlobals;

namespace MNX.Common
{
    internal class OctaveShift : Span
    {
        public readonly OctaveShiftType? Type = null;

        public OctaveShift(XmlReader r)
            : base()
        {
            // https://w3c.github.io/mnx/specification/common/#the-octave-shift-element
            A.Assert(r.Name == "octave-shift");

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
                            A.ThrowError("Error: Unknown attribute name.");
                        }
                        break;
                }
            }

            // r.Name is now the name of the last octave-shift attribute that has been read.
        }

        private OctaveShiftType GetOctaveShiftType(string value)
        {
            OctaveShiftType rval = OctaveShiftType.up1Oct;

            switch(value)
            {
                case "-8":
                    rval = OctaveShiftType.down1Oct;
                    break;
                case "8":
                    rval = OctaveShiftType.up1Oct;
                    break;
                case "-15":
                    rval = OctaveShiftType.down2Oct;
                    break;
                case "15":
                    rval = OctaveShiftType.up2Oct;
                    break;
                case "-22":
                    rval = OctaveShiftType.down3Oct;
                    break;
                case "22":
                    rval = OctaveShiftType.up3Oct;
                    break;
                default:
                    A.ThrowError("Error: unknown octave shift type");
                    break;
            }

            return rval;
        }
    }
}
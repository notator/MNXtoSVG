using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-octave-shift-element
    internal class OctaveShift : Span
    {
        // Instruction attributes
        public override PositionInMeasure Location { get; }
        public override int StaffIndex { get; }
        public override Orientation? Orient { get; }
        // Span attributes
        public override string Target { get; }
        public override PositionInMeasure End { get; }

        public readonly OctaveShiftType? Type = null;

        #region IUniqueDef
        public override string ToString() => $"OctaveShift: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";
        #endregion IUniqueDef

        public OctaveShift(XmlReader r)
            : base()
        {            
            M.Assert(r.Name == "octave-shift");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "type":
                        Type = GetOctaveShiftType(r.Value);
                        break;
                    // Span attribute
                    case "target":
                        Target = r.Value;
                        break;
                    case "end":
                        End = new PositionInMeasure(r.Value);
                        break;
                    // Instruction attributes
                    case "location":
                        Location = new PositionInMeasure(r.Value);
                        break;
                    case "staff-index":
                        int staffIndex = 0;
                        int.TryParse(r.Value, out staffIndex);
                        StaffIndex = staffIndex;
                        break;
                    case "orient":
                        switch(r.Value)
                        {
                            case "up":
                                Orient = Orientation.up;
                                break;
                            case "down":
                                Orient = Orientation.down;
                                break;
                        }
                        break;
                    default:
                        M.ThrowError("Error: Unknown attribute name.");
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
                    M.ThrowError("Error: unknown octave shift type");
                    break;
            }

            return rval;
        }
    }
}
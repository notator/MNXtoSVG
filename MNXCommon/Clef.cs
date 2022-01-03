using MNX.Globals;
using Moritz.Spec;
using System.Xml;

namespace MNX.Common
{
    public class Clef : PartDirectionsComponent, ISequenceDirectionsComponent, IUniqueDef
    {
        // Instruction attributes
        public override PositionInMeasure Location { get; }
        public override int StaffIndex { get; }
        public override Orientation? Orient { get; }
        // Other attributes
        public readonly int Line = 0; // 0 means uninitialised. Line must start at 1 (the bottom line of the staff)
        public readonly int Octave = 0; // Default. Octave can be set to any positive or negative integer.
        public readonly ClefType? Sign = null;

        #region IUniqueDef
        public override string ToString() => $"Clef: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";
        #endregion IUniqueDef

        public Clef(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-clef-element
            M.Assert(r.Name == "clef");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "line":
                        int.TryParse(r.Value, out Line);
                        M.Assert(Line > 0);
                        break;
                    case "sign":
                        Sign = GetMNXClefSign(r.Value);
                        break;
                    case "octave":
                        int.TryParse(r.Value, out Octave);
                        break;
                    // Instruction attributes
                    case "location":
                        Location = new PositionInMeasure(r.Value);
                        break;
                    case "staff-index":
                        int staffIndex;
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
                        M.ThrowError("Unknown clef attribute.");
                        break;
                }
            }

            M.Assert(Sign != null && Line > 0);

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
                    M.ThrowError("Error: unknown clef sign");
                    break;
            }
            
            return rval;
        }
    }
}
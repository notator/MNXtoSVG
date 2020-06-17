using System;
using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-octave-shift-element
    public class OctaveShift : Span, ISeqComponent
    {
        // Instruction attributes
        public override PositionInMeasure Location { get; }
        public override int StaffIndex { get; }
        public override Orientation? Orient { get; }
        // Span attributes
        public override string Target { get; }
        public override PositionInMeasure End { get; }
        /// <summary>
        /// returns measureIndex (base 0), tickPositionInMeasure
        /// if(measureIndex is null) the tickPosition is in the current measure.
        /// </summary>
        public Tuple<int?, int> EndOctaveShiftPos
        {
            get
            {
                Tuple<int?, int> rval = null;
                
                if(End.MeasureNumber == null)
                {
                    rval = new Tuple<int?, int>(null, End.TickPositionInMeasure);
                }
                else
                {
                    rval = new Tuple<int?, int>((int)End.MeasureNumber - 1, End.TickPositionInMeasure);
                }
                return rval;
            }
        }

        public OctaveShiftType Type { get { return (OctaveShiftType)_octaveShiftType; } }
        private OctaveShiftType? _octaveShiftType = null;

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
                        _octaveShiftType = GetOctaveShiftType(r.Value);
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

            M.Assert(_octaveShiftType != null);

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
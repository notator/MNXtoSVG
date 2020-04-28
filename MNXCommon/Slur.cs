using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    public class Slur : Span, IEventComponent
    {
        // Instruction attributes
        public override PositionInMeasure Location { get; }
        public override int StaffIndex { get; }
        public override Orientation? Orient { get; }
        // Span attributes
        public override string Target { get; }
        public override PositionInMeasure End { get; }

        // Other attributes
        public readonly string StartNote = null; // an ID
        public readonly string EndNote = null; // an ID
        public readonly LineType LineType = LineType.solid;
        public readonly Orientation? Side = null;
        public readonly Orientation? SideEnd = null;

        public Slur(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-slur-element
            M.Assert(r.Name == "slur");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "start-note":
                        StartNote = r.Value;
                        break;
                    case "end-note":
                        EndNote = r.Value;
                        break;
                    case "line-type":
                        LineType = GetLineType(r.Value);
                        break;
                    case "side":
                        if(r.Value == "up")
                            Side = Orientation.up;
                        else if(r.Value == "down")
                            Side = Orientation.down;
                        break;
                    case "side-end":
                        if(r.Value == "up")
                            SideEnd = Orientation.up;
                        else if(r.Value == "down")
                            SideEnd = Orientation.down;
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
                }
            }
        }

        private LineType GetLineType(string value)
        {
            LineType rval = LineType.solid; // default
            switch(value)
            {
                case "solid":
                    rval = LineType.solid;
                    break;
                case "dashed":
                    rval = LineType.dashed;
                    break;
                case "dotted":
                    rval = LineType.dotted;
                    break;
                default:
                    M.ThrowError("Error: unknown line type");
                    break;
            }
            return rval;
        }
    }
}
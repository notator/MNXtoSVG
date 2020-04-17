using MNX.AGlobals;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-tied-element
    internal class Tied : Span, INoteComponent
    {
        // Instruction attributes
        public override PositionInMeasure Location { get; }
        public override int StaffIndex { get; }
        public override Orientation? Orient { get; }
        // Span attribute
        public override string Target { get; }
        public override PositionInMeasure End { get; }

        public Tied(XmlReader r)
        {            
            A.Assert(r.Name == "tied");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
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
    }
}
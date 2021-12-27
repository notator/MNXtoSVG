using MNX.Globals;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-tied-element
    public class Tied : Span, INoteComponent
    {
        // DirectionComponent attributes
        public override PositionInMeasure Location { get; }
        public override int StaffIndex { get; }
        public override Orientation? Orient { get; }
        // Span attribute
        public override string TargetID { get; }
        public override PositionInMeasure End { get; }
        public override int TicksPosInScore { get { return _ticksPosInScore; } }
        private readonly int _ticksPosInScore;

        // New attribute (like Slur) -- ji 04.11.2020
        public readonly Orientation? Side = null;

        #region IUniqueDef
        public override string ToString() => $"Tied: Target={TargetID} MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";
        #endregion IUniqueDef

        public Tied(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "tied");
            _ticksPosInScore = ticksPosInScore;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    // Span attribute
                    case "target":
                        TargetID = r.Value;
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
                    case "side":
                        switch(r.Value)
                        {
                            case "up":
                                Side = Orientation.up;
                                break;
                            case "down":
                                Side = Orientation.down;
                                break;
                        }
                        break;
                }
            }
        }
    }
}
using System.Xml;
using MNX.AGlobals;

namespace MNX.Common
{
    public class Slur : ISequenceComponent, IEventComponent
    {
        public readonly string Target = null; // an ID
        public readonly MNXC_PositionInMeasure Location = null;
        public readonly string StartNote = null; // an ID
        public readonly string EndNote = null; // an ID
        public readonly LineType LineType = LineType.solid;
        public readonly Orientation? Side = null;
        public readonly Orientation? SideEnd = null;

        public Slur(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-slur-element
            A.Assert(r.Name == "slur");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "target":
                        Target = r.Value;
                        break;
                    case "location":
                        Location = new MNXC_PositionInMeasure(r.Value);
                        break;
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
                    A.ThrowError("Error: unknown line type");
                    break;
            }
            return rval;
        }
    }
}
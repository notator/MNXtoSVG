using MNX.Globals;
using Moritz.Spec;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-key-element
    public class KeySignature : PartDirectionsComponent, IGlobalDirectionsComponent, IUniqueDef
    {
        // Instruction attributes
        public override PositionInMeasure Location => new PositionInMeasure("0");
        public override int StaffIndex
        {
            get
            {
                // KeySignature.StaffIndex should never be retrieved.
                M.Assert(false, "Application Error.");
                return -1; 
            }
        }
        public override Orientation? Orient { get { return null; } }
        // other attributes
        public readonly int Fifths = 0; // default

        #region IUniqueDef
        public override string ToString() => $"KeySignature: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";
        #endregion IUniqueDef

        public KeySignature(XmlReader r)
        {
            M.Assert(r.Name == "key");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "fifths":
                        int.TryParse(r.Value, out Fifths);
                        break;
                    default:
                        M.ThrowError("Unknown key attribute.");
                        break;
                }
            }

            // r.Name is now the name of the last key attribute that has been read.
        }
    }


}
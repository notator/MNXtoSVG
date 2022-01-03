using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public class Jump : IGlobalDirectionsComponent, IUniqueDef
    {
        public readonly PositionInMeasure PositionInMeasure;
        public readonly JumpType JumpType;

        #region IUniqueDef
        public override string ToString() => $"Type: {JumpType} MsPositionReFirstIUD={MsPositionReFirstUD}";
        /// <summary>
        /// (?) See IUniqueDef Interface definition. (?)
        /// </summary>
        public object Clone()
        {
            return this;
        }
        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = 0;
        }

        public int MsDuration { get { return 0; } set { M.Assert(false, "Application Error."); } }

        public int MsPositionReFirstUD
        {
            get
            {
                M.Assert(_msPositionReFirstIUD >= 0);
                return _msPositionReFirstIUD;
            }
            set
            {
                M.Assert(value >= 0);
                _msPositionReFirstIUD = value;
            }
        }

        private int _msPositionReFirstIUD = 0;

        #endregion IUniqueDef

        public Jump(XmlReader r)
        {
            M.Assert(r.Name == "jump");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "location":
                        {
                            PositionInMeasure = new PositionInMeasure(r.Value);
                            break;
                        }
                    case "type":
                        switch(r.Value)
                        {
                            case "segno":
                                JumpType = JumpType.segno;
                                break;
                            case "dsalfine":
                                JumpType = JumpType.dsalfine;
                                break;
                            default:
                                JumpType = JumpType.unknown;
                                break;
                        }                        
                        break;
                    default:
                        M.ThrowError("Unknown jump attribute.");
                        break;
                }
            }
            // r.Name is now the name of the last jump attribute that has been read.
        }
    }
}
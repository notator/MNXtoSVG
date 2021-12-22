using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public class Segno : IGlobalDirectionsComponent, IUniqueDef
    {
        public readonly PositionInMeasure PositionInMeasure;
        public readonly string SMuFLGlyphName = "none"; // optional attribute
        private readonly int TicksPosInScore;

        #region IUniqueDef
        public override string ToString() => $"SMuFLGlyphName: {SMuFLGlyphName} TicksPosInScore={TicksPosInScore} MsPositionReFirstIUD={MsPositionReFirstUD}";
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

        public int TicksDuration { get; }

        private int _msPositionReFirstIUD = 0;

        #endregion IUniqueDef

        public Segno(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "segno");
            TicksPosInScore = ticksPosInScore;
            TicksDuration = 0;

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
                    case "glyph":
                        SMuFLGlyphName = r.Value;                        
                        break;
                    default:
                        M.ThrowError("Unknown segno attribute.");
                        break;
                }
            }
            // r.Name is now the name of the last jump attribute that has been read.
        }
    }
}
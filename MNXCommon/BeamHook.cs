using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public class BeamHook : IUniqueDef
    {
        private readonly int TicksPosInScore;

        public readonly string EventID;
        public readonly BeamHookDirection BeamHookDirection;
        public readonly int Depth;

        #region IUniqueDef
        public override string ToString() => $"Depth={Depth} EventID={EventID} BeamHookDirection={BeamHookDirection}";
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

        public BeamHook(XmlReader r, int ticksPosInScore, int topLevelDepth)
        {
            M.Assert(r.Name == "beam-hook");
            TicksPosInScore = ticksPosInScore;
            TicksDuration = 0;
            Depth = r.Depth - topLevelDepth;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "event":
                        {
                            EventID = r.Value.Trim();
                            break;
                        }
                    case "direction":
                        {
                            switch(r.Value.Trim())
                            {
                                case "left":
                                    BeamHookDirection = BeamHookDirection.left;
                                    break;
                                case "right":
                                    BeamHookDirection = BeamHookDirection.right;
                                    break;
                            }
                            break;
                        }
                    default:
                        M.ThrowError("Unknown beam-hook attribute.");
                        break;
                }
            }

            r.MoveToElement();
            M.Assert(r.Name == "beam-hook");
        }
    }
}
using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#common-direction-attributes
    /// </summary>
    public abstract class PartDirectionsComponent : IPartDirectionsComponent
    {
        /// <summary>
        /// https://w3c.github.io/mnx/specification/common/#measure-location
        /// Here are some instances of the measure location syntax from the spec:
        /// 0.25   -> one quarter note after the start of a containing measure 
        /// 3/8    -> three eighth notes after the start of a containing measure 
        /// 4:0.25 -> one quarter note after the start of the measure with index 4 
        /// 4:1/4  -> the same as the preceding example
        /// #event235 -> the same metrical position as the event whose element ID is event235
        /// </summary>
        public abstract PositionInMeasure Location { get; }
        public abstract int StaffIndex { get; }
        public abstract Orientation? Orient { get; }

        #region IUniqueDef
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
    }

    /// <summary>
    /// A Span is an object like a Slur, Tie or OctaveShift.
    /// </summary>
    public abstract class Span : PartDirectionsComponent
    {
        #region IUniqueDef
        public override string ToString() => $"Span: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";
        #endregion IUniqueDef

        public abstract string TargetID { get; }
        public abstract PositionInMeasure End { get; }
    }
}
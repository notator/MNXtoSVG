using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#common-direction-attributes
    /// Contrary to the spec (which I find very confusing), I am calling this
    /// object type an Instruction, rather than a "Direction".
    /// The spec (and I) use "Directions" for the high level container class.
    /// An Instruction is an object like a Clef or KeySignature.
    /// A Span is an object like a Slur, Tie or OctaveShift.
    /// </summary>
    internal abstract class Instruction
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
    }

    internal abstract class Span : Instruction
    {
        public abstract string Target { get; }
        public abstract PositionInMeasure End { get; }
    }
}
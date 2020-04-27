

using MNX.Globals;

namespace Moritz.Spec
{
    ///<summary>
    /// IUniqueDef is implemented by all objects that can be added to a VoiceDefs.UniqueDefs list.
    /// The Moritz objects must implement a deep Clone() so that VoiceDefs.DeepClone() can be implemented.
    /// VoiceDefs is used for composition. When complete, the UniqueDefs list is transferred to
    /// Voice before the definitions are converted to the objects themselves.
    /// The MNX object Clone() functions simply return this, because they are already unique. 
    /// Currently this interface is implemented by the following Moritz objects (11.9.2014):
    ///     MidiChordDef
    ///     MidiRestDef
    ///     CautionaryChordDef
    ///     ClefDef
    /// and the MNX objects (27.04.2020):
    ///   ISeqComponents:
    ///     Tuplet,
    ///     Beamed,
    ///     Grace,
    ///     Event,
    ///     Forward
    ///   Directions
    ///     Directions
    ///   IDirectionsComponents:
    ///     Clef,
    ///     KeySignature
    ///</summary>
    public interface IUniqueDef : System.ICloneable
    {
        string ToString();

        /// <summary>
        /// Returns a deep clone (a unique object)
        /// </summary>
        new object Clone();

        /// <summary>
        /// This function is problematic.
        /// It needs to be replaced by one that takes the whole duration
        /// of the accel or rit into account.
        /// </summary>
        /// <param name="factor"></param>
        void AdjustMsDuration(double factor);

        int MsDuration { get; set; }
        int MsPositionReFirstUD { get; set; }
    }
}

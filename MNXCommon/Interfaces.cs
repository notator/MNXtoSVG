
namespace MNX.Common
{
    /// <summary>
    /// Directions.
    /// </summary>
    public interface IGlobalMeasureComponent
    {
    }

    /// <summary>
    /// TimeSignature, Clef, KeySignature, OctaveShift.
    /// </summary>
    public interface IDirectionsComponent
    {
    }

    /// <summary>
    /// Directions, Sequence.
    /// </summary>
    public interface IPartMeasureComponent
    {
    }

    /// <summary>
    /// MNX Objects: Directions, Tuplet, Grace, Event, Forward  
    /// </summary>
    public interface ISeqComponent
    {
    }

    /// <summary>
    /// Rest, Slur (Tied is INoteComponent)
    /// </summary>
    public interface IEventComponent
    {
    }

    /// <summary>
    /// Tied 
    /// </summary>
    public interface INoteComponent
    {

    }

    /// <summary>
    /// Measure, Sequence, Tuplet, Beam, Grace, Event, Forward
    /// (Rest is an implementation detail of Event.) 
    /// </summary>
    public interface IHasTicks
    {
        int TicksDuration { get; }
        int TicksPosInScore { get; }
    }
}


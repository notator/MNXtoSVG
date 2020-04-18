


namespace MNX.Common
{
    /// <summary>
    /// Directions.
    /// </summary>
    public interface IGlobalMeasureComponent
    {
    }

    /// <summary>
    /// TimeSignature.
    /// </summary>
    public interface IGlobalDirectionsComponent
    {
    }

    /// <summary>
    /// Directions, Sequence.
    /// </summary>
    public interface IPartMeasureComponent
    {
    }

    /// <summary>
    /// Directions,
    /// Tuplet, Beamed, Grace, Event, Forward
    /// Clef 
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
        int Ticks { get; }
    }

    /// <summary>
    /// Can be Global or Part Directions.
    /// Currently implemented by KeySignature
    /// </summary>
    internal interface IDirectionsComponent
    {
    }
}


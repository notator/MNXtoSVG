
using System.Collections.Generic;

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
    /// Tuplet, Beamed, Grace, Event
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
    /// Currently implemented by Measure, Sequence, Tuplet, Beam, Grace, Event, Rest
    /// </summary>
    public interface ITicks
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


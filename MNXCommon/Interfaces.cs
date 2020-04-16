
using System.Collections.Generic;

namespace MNX.Common
{
    /// <summary>
    /// The Measure is in a global Directions
    /// Currently implemented by TimeSignature.
    /// </summary>
    public interface IGlobalMeasureComponent
    {
    }

    /// <summary>
    /// The Measure is in a Part
    /// Currently implemented by Directions, Sequence.
    /// </summary>
    public interface IPartMeasureComponent
    {
    }

    /// <summary>
    /// Currently implemented by
    /// Directions,
    /// Tuplet, Beamed, Grace, Event, Rest
    /// plus Clef, Slur. 
    /// </summary>
    public interface ISeqComponent
    {
    }

    /// <summary>
    /// Currently implemented by Tied 
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
    /// Sequence, Tuplet, Beam, Grace
    /// </summary>
    public interface IEventList
    {
        List<Event> EventList { get; }
    }

    /// <summary>
    /// Note, Rest, Slur (Tie is a Span)
    /// All IEventComponents are also IWritable
    /// </summary>
    public interface IEventComponent
    {
    }

    /// <summary>
    /// Can be Global or Part Directions.
    /// Currently implemented by KeySignature
    /// </summary>
    internal interface IDirectionsComponent
    {
    }
}


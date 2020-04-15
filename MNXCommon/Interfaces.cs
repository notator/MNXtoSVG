
namespace MNX.Common
{
    /// <summary>
    /// Currently implemented by Measure, Sequence, Tuplet, Event, Grace, Rest
    /// </summary>
    public interface ITicks
    {
        int Ticks { get; }
    }

    /// <summary>
    /// Currently implemented by KeySignature, TimeSignature...
    /// </summary>
    public interface IDirectionsComponent
    {
    }

    /// <summary>
    /// Currently implemented by Tuplet, Event, Grace, Rest
    /// plus Clef, TimeSignature, Tied, Slur, Rest.
    /// </summary>
    public interface ISequenceComponent
    {
    }

    /// <summary>
    /// Note, Rest, Slur (Tie is a Span)
    /// All IEventComponents are also IWritable
    /// </summary>
    public interface IEventComponent
    {
    }
}


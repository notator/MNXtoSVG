

using System.Xml;

namespace MNXtoSVG
{
    /// <summary>
    /// Currently implemented by all IWritableSequenceComponents plus
    ///   higher level objects: Score, MNX_Common, Part, Measure, Sequence
    ///   lower level objects: Note, Accidental
    /// </summary>
    public interface IWritable
    {
        void WriteSVG(XmlWriter w);
    }

    /// <summary>
    /// Currently implemented by ITicksSequenceComponents plus Clef, TimeSignature, Tied, Slur, Rest.
    /// </summary>
    public interface IWritableSequenceComponent
    {
        void WriteSVG(XmlWriter w);
    }

    /// <summary>
    /// Currently implemented by all ITicksSequenceComponents plus Measure, Sequence
    /// </summary>
    public interface ITicks
    {
        int Ticks { get; }
    }

    /// <summary>
    /// Currently implemented by Tuplet, Event, Grace, Rest
    /// </summary>
    public interface ITicksSequenceComponent
    {
        int Ticks { get; }
    }

    /// <summary>
    /// Note, Rest, Slur (Tie is a Span)
    /// All IEventComponents are also IWritable
    /// </summary>
    public interface IEventComponent
    {

    }
}



namespace MNX.Common
{
    /// <summary>
    /// GlobalDirections.
    /// </summary>
    public interface IGlobalMeasureComponent
    {
    }

    /// <summary>
    /// PartDirections, Sequence.
    /// </summary>
    public interface IPartMeasureComponent
    {
    }

    /// <summary>
    /// Beams, SequenceDirections, Event, Forward, Grace, Tuplet
    /// </summary>
    public interface ISequenceComponent
    {
    }

    /// <summary>
    /// Ending, Fine, Jump, Key, Repeat, Segno, Tempo, Time
    /// </summary>
    public interface IGlobalDirectionsComponent
    {
    }

    /// <summary>
    /// Clef, Key
    /// </summary>
    public interface IPartDirectionsComponent
    {
    }

    /// <summary>
    /// Clef, Cresc, Dim, Dynamics, Expression, Instruction, OctaveShift, Wedge
    /// </summary>
    public interface ISequenceDirectionsComponent
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
    /// Sequence, Grace, Event, Forward, Tuplet
    /// (Rest is an implementation detail of Event.) 
    /// </summary>
    public interface IHasTicks
    {
        /// <summary>
        /// Measure and Sequence calculate the returned duration dynamically.
        /// </summary>
        int TicksDuration { get; }
        int TicksPosInScore { get; }
    }

    /// <summary>
    /// Grace, Event, Forward, Tuplet
    /// (Rest is an implementation detail of Event.) 
    /// </summary>
    public interface IHasSettableTicksDuration
    {
        int TicksDuration { get; set; }
        int TicksPosInScore { get; set; }
    }
}


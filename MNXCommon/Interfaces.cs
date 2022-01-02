
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
        int TicksPosInScore { get; set; }
    }

    /// <summary>
    /// Ending, Fine, Jump, Key, Repeat, Segno, Tempo, Time
    /// </summary>
    public interface IGlobalDirectionsComponent
    {
        int TicksPosInScore { get; set; }
    }

    /// <summary>
    /// Clef, Key
    /// </summary>
    public interface IPartDirectionsComponent
    {
        int TicksPosInScore { get; set; }
    }

    /// <summary>
    /// Clef, Cresc, Dim, Dynamics, Expression, Instruction, OctaveShift, Wedge
    /// </summary>
    public interface ISequenceDirectionsComponent
    {
        int TicksPosInScore { get; set; }
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

    /// <summary>
    /// Grace, Event, Forward, Tuplet
    /// (Rest is an implementation detail of Event.) 
    /// </summary>
    public interface IHasSettableTickDuration
    {
        int TicksDuration { get; set; }
        int TicksPosInScore { get; }
    }
}


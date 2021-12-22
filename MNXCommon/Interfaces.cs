
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
    /// Measure, Sequence, Tuplet, Beam, Grace, Event, Forward
    /// (Rest is an implementation detail of Event.) 
    /// </summary>
    public interface IHasTicks
    {
        int TicksDuration { get; }
        int TicksPosInScore { get; }
    }
}


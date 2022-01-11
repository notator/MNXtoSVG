
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
    /// GlobalMeasure, Sequence, Grace, Event, Forward, Tuplet
    /// (Rest is an implementation detail of Event.) 
    /// </summary>
    public interface IHasTicksDuration
    {
        /// <summary>
        /// Sequence, Grace and Tuplet override get: calculate the returned duration dynamically,
        /// Sequence throws an exception if set is called.
        /// Grace and Tuplet are EventGroups. They both override set to
        /// set the TicksDuration of their individual Components.
        /// </summary>
        int TicksDuration { get; set; }
    }

    /// <summary>
    /// Event, Forward (Rest is an implementation detail of Event.) 
    /// </summary>
    public interface IEvent
    {
        int TicksDuration { get; set; }
        int TicksPosInScore { get; set; }
    }

    /// <summary>
    /// Beam, BeamHook
    /// </summary>
    public interface IBeamBlockComponent
    {
        int Depth { get; }
    }
}


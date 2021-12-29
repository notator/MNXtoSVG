using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-tuplet-element
    /// </summary>
    public class TupletDef : EventGroup, IHasTicks, ISequenceComponent
    {
        /// Compulsory attributes:
        #region MNX file attributes
        /// duration with respect to containing element (from MNX outer) 
        /// <summary>
        /// Note that OuterDuration is the logical duration in the score,
        /// but OuterDuration.Ticks may change if Grace objects steal ticks
        /// from the first or last event.
        /// </para>
        /// </summary>
        public readonly MNXDurationSymbol OuterDuration = null;
        // duration of the enclosed sequence content (from MNX inner)
        public readonly MNXDurationSymbol InnerDuration = null;

        // Optional attributes:
        // Orientation of this tuplet. (The spec says default is app-specific.) 
        public readonly Orientation Orient = Orientation.up; // app-specific default

        // (1-based) staff index of this tuplet. The spec says that the default is app-specific,
        // and that "The topmost staff in a part has a staff index of 1; staves below the topmost staff
        // are identified with successively increasing indices."
        // https://w3c.github.io/mnx/specification/common/#common-parts-staves
        public readonly int Staff = 1; // (app-specific?) default

        // Control over the display of the tuplet ratio numbers
        public readonly TupletNumberDisplay ShowNumber = TupletNumberDisplay.inner; // spec default
        // Control over the display of the tuplet ratio note values
        public readonly TupletNumberDisplay ShowValue = TupletNumberDisplay.none; // spec default
        // Control over the display of brackets -> Bracket
        public readonly TupletBracketDisplay Bracket = TupletBracketDisplay.auto; // spec default
        #endregion MNX file attributes

        public override string ToString() => $"Tuplet: TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration} MsPosInScore={MsPosInScore} MsDuration={MsDuration}";

        public TupletDef(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "tuplet");

            TicksPosInScore = ticksPosInScore;

            C.CurrentTupletLevel = 1;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "outer":
                        OuterDuration = new MNXDurationSymbol(r.Value, C.CurrentTupletLevel);
                        break;
                    case "inner":
                        InnerDuration = new MNXDurationSymbol(r.Value, C.CurrentTupletLevel);
                        break;
                    case "orient":
                        Orient = GetMNXOrientation(r.Value);
                        break;
                    case "staff":
                        int staff;
                        int.TryParse(r.Value, out staff);
                        if(staff > 0)
                        {
                            Staff = staff;
                        }
                        break;
                    case "show-number":
                        ShowNumber = GetTupletNumberDisplay(r.Value);
                        break;
                    case "show-value":
                        ShowValue = GetTupletNumberDisplay(r.Value);
                        break;
                    case "bracket":
                        Bracket = GetTupletBracketDisplay(r.Value);
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            M.ReadToXmlElementTag(r, "event", "grace", "forward");

            while(r.Name == "event" || r.Name == "grace" || r.Name == "forward" )
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "event":
                            Event e = new Event(r, ticksPosInScore);
                            ticksPosInScore += e.TicksDuration;
                            Components.Add(e);
                            break;
                        case "grace":
                            Grace grace = new Grace(r, ticksPosInScore);
                            ticksPosInScore += grace.TicksDuration;
                            Components.Add(grace);
                            break;
                        case "forward":
                            Forward forward = new Forward(r, ticksPosInScore);
                            ticksPosInScore += forward.TicksDuration;
                            Components.Add(forward);
                            break;
                    }
                }

                M.ReadToXmlElementTag(r, "event", "grace", "forward", "tuplet");
            }

            M.Assert(Events.Count > 0);
            M.Assert(r.Name == "tuplet"); // end of (nested) tuplet content

            int outerTicks = this.OuterDuration.GetDefaultTicks();
            this.OuterDuration.TicksDuration = outerTicks;
            SetTicksInContent(outerTicks);

            C.CurrentTupletLevel = 0;
        }

        private void SetTicksInContent(int outerTicks)
        {
            int GetBasicTicks(IHasTicks component)
            {
                int rval = 0;
                if(component is Event e)
                {
                    MNXDurationSymbol defaultDuration = e.MNXDurationSymbol;
                    MNXDurationSymbol ticksOverride = e.TicksOverride;
                    rval = (ticksOverride == null) ? defaultDuration.GetDefaultTicks() : ticksOverride.GetDefaultTicks();
                }
                else if(component is Forward f)
                {
                    rval = f.TicksDuration;
                }
                return rval;
            }

            void SetGraceTicksDuration(Grace g, int graceIndex, List<int> localInnerTicks)
            {
                switch(g.Type)
                {
                    case GraceType.stealPrevious:
                    {
                        M.Assert(graceIndex > 0, "Error in MNX file.");
                        g.TicksDuration = localInnerTicks[graceIndex - 1] / 3;
                        break;
                    }
                    case GraceType.stealFollowing:
                    {
                        M.Assert(graceIndex < localInnerTicks.Count, "Error in MNX file.");
                        g.TicksDuration = localInnerTicks[graceIndex] / 3;
                        break;
                    }
                    case GraceType.makeTime:
                    {
                        M.Assert(graceIndex < localInnerTicks.Count, "Error in MNX file.");
                        g.TicksDuration = localInnerTicks[graceIndex] / 3;
                        break;
                    }
                }
            }

            List<int> eventAndForwardBasicTicks = new List<int>();

            // Get 1. the outer ticks of each Event and Forward component.
            foreach(IHasTicks component in Components )
            {
                if(component is Event || component is Forward)
                {
                    eventAndForwardBasicTicks.Add(GetBasicTicks(component));
                }
            }

            List<int> innerTicks = M.IntDivisionSizes(outerTicks, eventAndForwardBasicTicks);

            for(int i = Components.Count - 1; i >= 0; i--)
            {
                if(Components[i] is Grace g)
                {
                    innerTicks.Insert(i, 0); // dummy for Grace TicksDuration
                }
            }

            for(int i = 0; i < Components.Count; i++)
            {
                if(Components[i] is Grace g)
                {
                    SetGraceTicksDuration(g, i, innerTicks);
                }
            }

            // All ticksPosInScore values are updated again for grace notes
            // when the whole score has been read (in MNX.AdjustForGraceNotes())

            int ticksPosInScore = this.TicksPosInScore;

            for(int i = 0; i < Components.Count; i++)
            {
                if(Components[i] is Event e)
                {
                    e.TicksDuration = innerTicks[i];
                    e.TicksPosInScore = ticksPosInScore;
                    ticksPosInScore += e.TicksDuration;
                }
                if(Components[i] is Grace g)
                {
                    g.TicksDuration = innerTicks[i];
                    g.TicksPosInScore = ticksPosInScore;
                    ticksPosInScore += g.TicksDuration;
                }
                if(Components[i] is Forward f)
                {
                    f.TicksDuration = innerTicks[i];
                    f.TicksPosInScore = ticksPosInScore;
                    ticksPosInScore += f.TicksDuration;
                }
            }
        }



        private Orientation GetMNXOrientation(string value)
        {
            Orientation rval = Orientation.up;
            switch(value)
            {
                case "up":
                    rval = Orientation.up;
                    break;
                case "down":
                    rval = Orientation.down;
                    break;
                default:
                    M.ThrowError("Error: unknown orientation");
                    break;
            }
            return rval;
        }

        private TupletNumberDisplay GetTupletNumberDisplay(string value)
        {
            TupletNumberDisplay rval = TupletNumberDisplay.inner; // default
            switch(value)
            {
                case "both":
                    rval = TupletNumberDisplay.both;
                    break;
                case "none":
                    rval = TupletNumberDisplay.none;
                    break;
                default:
                    M.ThrowError("Error: unknown tuplet number display type.");
                    break;
            }
            return rval;
        }
        private TupletBracketDisplay GetTupletBracketDisplay(string value)
        {
            TupletBracketDisplay rval = TupletBracketDisplay.auto; // default
            switch(value)
            {
                case "yes":
                    rval = TupletBracketDisplay.yes;
                    break;
                case "no":
                    rval = TupletBracketDisplay.no;
                    break;
                default:
                    M.ThrowError("Error: unknown tuplet bracket display type.");
                    break;
            }
            return rval;
        }
    }
}

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

        public int Depth;

        public override string ToString() => $"Tuplet: TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration} MsPosInScore={MsPosInScore} MsDuration={MsDuration}";

        public TupletDef(XmlReader r, int ticksPosInScore, int depth = 1)
        {
            M.Assert(r.Name == "tuplet");

            _ticksPosInScore = ticksPosInScore;

            C.CurrentTupletLevel = depth;
            Depth = depth;

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

            M.ReadToXmlElementTag(r, "event", "tuplet", "grace", "forward");

            while(r.Name == "event" || r.Name == "tuplet" || r.Name == "grace" || r.Name == "forward")
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
                        case "tuplet":
                            TupletDef tupletDef = new TupletDef(r, ticksPosInScore, depth + 1);
                            ticksPosInScore += tupletDef.TicksDuration;
                            Components.Add(tupletDef);
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

                if(r.Name == "tuplet" && r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                M.ReadToXmlElementTag(r, "event", "grace", "forward", "tuplet");
            }

            M.Assert(Events.Count > 0);
            M.Assert(r.Name == "tuplet"); // end of (nested) tuplet content

            if(C.CurrentTupletLevel == 1)
            {
                int outerTicks = this.OuterDuration.GetDefaultTicks();
                this.OuterDuration.Ticks = outerTicks;
                SetTicksInContent(outerTicks, this.Depth);
            }

            C.CurrentTupletLevel--;
        }

        /// <summary>
        /// This function is called recursively for nested tuplets. 
        /// </summary>
        private void SetTicksInContent(int outerTicks, int localTupletLevel)
        {
            List<int> ticksInside = new List<int>();
            List<ISequenceComponent> components = new List<ISequenceComponent>();
            int stealGraceTicks = 0;

            void GetObject(ISequenceComponent component)
            {
                if(component is Event e)
                {
                    M.Assert(e.TupletLevel == localTupletLevel);

                    MNXDurationSymbol defaultDuration = e.MNXDurationSymbol;
                    MNXDurationSymbol ticksOverride = e.TicksOverride;
                    int basicTicks = 0;
                    if(ticksOverride != null)
                    {
                        basicTicks = ticksOverride.GetDefaultTicks();                            
                    }
                    else
                    {
                        basicTicks = defaultDuration.GetDefaultTicks();
                    }
                    basicTicks -= stealGraceTicks;
                    stealGraceTicks = 0;
                    components.Add(e);
                    ticksInside.Add(basicTicks);
                }
                else if(component is TupletDef t)
                {
                    // a nested tuplet
                    M.Assert(t.Depth == localTupletLevel);
                   
                    MNXDurationSymbol d = t.OuterDuration;
                    int basicTicks = d.GetDefaultTicks();
                    components.Add(t);
                    ticksInside.Add(basicTicks);
                }
                else if(component is Grace g)
                {
                    ticksInside.Add(g.TicksDuration);
                }
            }

            // Get 1. the outer ticks of Events and Tuplets at this tuplet level
            // and 2. a list of nested tuplets
            foreach(var component in Components)
            {
                GetObject(component);
            }

            List<int> innerTicks = M.IntDivisionSizes(outerTicks, ticksInside);

            for(var i = 0; i < components.Count; i++)
            {
                ISequenceComponent component = components[i];
                int ticks = innerTicks[i];
                if(component is TupletDef tuplet)
                {
                    tuplet.OuterDuration.Ticks = ticks;
                    tuplet.SetTicksInContent(tuplet.OuterDuration.Ticks, tuplet.Depth + 1);
                }
                else
                {
                    Event evnt = component as Event;
                    evnt.TicksDuration = ticks;
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

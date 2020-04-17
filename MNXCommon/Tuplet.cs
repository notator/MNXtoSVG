using MNX.AGlobals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-tuplet-element
    /// </summary>
    internal class Tuplet : EventGroup, ITicks, ISeqComponent
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
        public readonly DurationSymbol OuterDuration = null;
        // duration of the enclosed sequence content (from MNX inner)
        public readonly DurationSymbol InnerDuration = null;

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

        #region Runtime property
        public readonly int TupletLevel;
        #endregion Runtime property

        public Tuplet(XmlReader r)
        {
            TupletLevel = B.CurrentTupletLevel; // top level tuplet has tuplet level 0

            B.CurrentTupletLevel++;

            A.Assert(r.Name == "tuplet");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "outer":
                        OuterDuration = new DurationSymbol(r.Value, B.CurrentTupletLevel);
                        break;
                    case "inner":
                        InnerDuration = new DurationSymbol(r.Value, B.CurrentTupletLevel);
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

            Seq = GetSequenceContent(r, "tuplet", false);

            if(B.CurrentTupletLevel == 1)
            {
                int outerTicks = this.OuterDuration.GetDefaultTicks();
                this.OuterDuration.Ticks = outerTicks;
                SetTicksInContent(outerTicks, this.TupletLevel + 1);
            }

            A.Assert(r.Name == "tuplet"); // end of (nested) tuplet

            B.CurrentTupletLevel--;
        }

        /// <summary>
        /// This function is called recursively for nested tuplets. 
        /// </summary>
        private void SetTicksInContent(int outerTicks, int localTupletLevel)
        {
            List<int> ticksInside = new List<int>();
            List<ISeqComponent> components = new List<ISeqComponent>();
            int stealGraceTicks = 0;

            void GetObject(ISeqComponent component)
            {
                if(component is Event e)
                {
                    A.Assert(e.TupletLevel == localTupletLevel);

                    DurationSymbol defaultDuration = e.DSymbol;
                    DurationSymbol ticksOverride = e.TicksOverride;
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
                else if(component is Tuplet t)
                {
                    // a nested tuplet
                    A.Assert(t.TupletLevel == localTupletLevel);
                   
                    DurationSymbol d = t.OuterDuration;
                    int basicTicks = d.GetDefaultTicks();
                    components.Add(t);
                    ticksInside.Add(basicTicks);
                }
                else if(component is Grace g)
                {
                    ticksInside.Add(g.Ticks);
                }
            }

            // Get 1. the outer ticks of Events and Tuplets at this tuplet level (which may be inside a "beamed")
            // and 2. a list of nested tuplets
            foreach(var s in Seq)
            {
                if(s is Beamed beamed)
                {
                    for(var i = 0; i < beamed.Seq.Count; i++)
                    {
                        if(beamed.Seq[i] is ISeqComponent component)
                        {
                            GetObject(component);
                        }
                    }
                }
                else if(s is ISeqComponent component)
                {
                    GetObject(component);
                }
            }

            List<int> innerTicks = B.GetInnerTicks(outerTicks, ticksInside);

            for(var i = 0; i < components.Count; i++)
            {
                ISeqComponent component = components[i];
                int ticks = innerTicks[i];
                if(component is Tuplet tuplet)
                {
                    tuplet.OuterDuration.Ticks = ticks;
                    tuplet.SetTicksInContent(tuplet.OuterDuration.DefaultTicks, tuplet.TupletLevel + 1);
                }
                else
                {
                    Event evnt = component as Event;
                    evnt.DSymbol.Ticks = ticks;
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
                    A.ThrowError("Error: unknown orientation");
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
                    A.ThrowError("Error: unknown tuplet number display type.");
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
                    A.ThrowError("Error: unknown tuplet bracket display type.");
                    break;
            }
            return rval;
        }
    }
}

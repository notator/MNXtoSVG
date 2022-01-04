using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-tuplet-element
    /// </summary>
    public class TupletDef : EventGroup, ISequenceComponent
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

        public override int TicksDuration
        {
            get
            {
                return OuterDuration.TicksDuration;
            }
            set
            {
                // this function is used when setting tuplet event ticks when dealing with nested tuplets.
                M.Assert(value >= M.MinimumEventTicks);
                OuterDuration.TicksDuration = value;
            }
        }

        public override string ToString() => $"Tuplet: TicksPosInScore={TicksPosInScore} TicksDuration={TicksDuration} MsPosInScore={MsPosInScore} MsDuration={MsDuration}";

        private bool _isTopLevel;

        public TupletDef(XmlReader r, bool isTopLevel)
        {
            M.Assert(r.Name == "tuplet");

            TicksPosInScore = 0;

            _isTopLevel = isTopLevel;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "outer":
                        OuterDuration = new MNXDurationSymbol(r.Value);
                        break;
                    case "inner":
                        InnerDuration = new MNXDurationSymbol(r.Value);
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

            M.ReadToXmlElementTag(r, "event", "grace", "forward", "tuplet");

            while(r.Name == "event" || r.Name == "grace" || r.Name == "forward" || r.Name == "tuplet")
            {
                if(r.Name == "tuplet" && r.NodeType == XmlNodeType.EndElement)
                {
                    break; //  pop 1 level
                }

                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "event":
                            Event e = new Event(r);
                            Components.Add(e);
                            break;
                        case "grace":
                            Grace grace = new Grace(r);
                            Components.Add(grace);
                            break;
                        case "forward":
                            Forward forward = new Forward(r);
                            Components.Add(forward);
                            break;
                        case "tuplet":
                            TupletDef tuplet = new TupletDef(r, false);
                            Components.Add(tuplet);
                            break;
                    }
                }

                M.ReadToXmlElementTag(r, "event", "grace", "forward", "tuplet");
            }

            M.Assert(EventsGracesAndForwards.Count > 0);
            M.Assert(r.Name == "tuplet"); // end of (nested) tuplet content

            if(_isTopLevel)
            {
                int outerTicks = this.OuterDuration.GetDefaultTicks();
                this.OuterDuration.TicksDuration = outerTicks;
                SetTicksDurationsInContentIgnoringGraces(outerTicks);
            }
        }

        /// <summary>
        /// This function is called recursively.
        /// TicksPosInScore is not set. Grace groups are ignored.
        /// TicksPosInScore and TicksDurations are updated for grace notes
        /// when the whole score has been read (in MNX.AdjustForGraceNotes()).
        /// </summary>
        /// <param name="outerTicks"></param>
        private void SetTicksDurationsInContentIgnoringGraces(int outerTicks)
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
                else if(component is TupletDef t)
                {
                    rval = t.OuterDuration.GetDefaultTicks();
                }
                return rval;
            }

            List<int> eventForwardTupletBasicTicks = new List<int>();
            List<IHasTicks> eventForwardTuplets = new List<IHasTicks>();

            // Get the default outer ticks of each contained Event, Forward and TupletDef component.
            foreach(IHasTicks component in Components )
            {
                if(component is Event || component is Forward || component is TupletDef)
                {
                    eventForwardTupletBasicTicks.Add(GetBasicTicks(component));
                    eventForwardTuplets.Add(component);
                }
            }

            List<int> innerTicks = M.IntDivisionSizes(outerTicks, eventForwardTupletBasicTicks);

            // Set the default outer ticks of each contained Event, Forward and TupletDef component.
            for(int i = 0; i < eventForwardTuplets.Count; i++)
            {
                IHasTicks eventForwardTuplet = eventForwardTuplets[i];
                int ticks = innerTicks[i];
                if(eventForwardTuplet is Event e)
                {
                    e.TicksDuration = innerTicks[i];
                }
                if(eventForwardTuplet is Forward f)
                {
                    f.TicksDuration = innerTicks[i];
                }
                if(eventForwardTuplet is TupletDef t)
                {
                    t.TicksDuration = innerTicks[i];
                    t.SetTicksDurationsInContentIgnoringGraces(t.TicksDuration); // recursive call
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

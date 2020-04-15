using MNX.AGlobals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-tuplet-element
    /// </summary>
    public class Tuplet : ITicks, ISequenceComponent
    {
        #region MNX file attributes
        // Compulsory attributes:
        // duration with respect to containing element (from MNX outer)
        public readonly Duration Duration = null;
        // duration of the enclosed sequence content (from MNX inner)
        public readonly Duration InnerDuration = null;

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

        #region Runtime properties
        public readonly List<ISequenceComponent> Seq;

        public readonly int TupletLevel;

        public int Ticks
        {
            get
            {
                return Duration.Ticks;
            }
        }

        #endregion Runtime properties

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
                        Duration = new Duration(r.Value, B.CurrentTupletLevel);
                        break;
                    case "inner":
                        InnerDuration = new Duration(r.Value, B.CurrentTupletLevel);
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

            Seq = B.GetSequenceContent(r, "tuplet", false);

            if(B.CurrentTupletLevel == 1)
            {
                int outerTicks = this.Duration.GetBasicTicks();
                this.Duration.Ticks = outerTicks;
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
            List<ISequenceComponent> components = new List<ISequenceComponent>();

            void GetObject(ISequenceComponent component)
            {
                if(component is Event e)
                {
                    A.Assert(e.TupletLevel == localTupletLevel);

                    Duration defaultDuration = e.Duration;
                    Duration ticksOverride = e.TicksOverride;
                    int basicTicks = 0;
                    if(ticksOverride != null)
                    {
                        basicTicks = ticksOverride.GetBasicTicks();                            
                    }
                    else
                    {
                        basicTicks = defaultDuration.GetBasicTicks();
                    }
                    components.Add(e);
                    ticksInside.Add(basicTicks);
                }
                else if(component is Tuplet t)
                {
                    // a nested tuplet
                    A.Assert(t.TupletLevel == localTupletLevel);
                   
                    Duration d = t.Duration;
                    int basicTicks = d.GetBasicTicks();
                    components.Add(t);
                    ticksInside.Add(basicTicks);
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
                        if(beamed.Seq[i] is ISequenceComponent component)
                        {
                            GetObject(component);
                        }
                    }
                }
                else if(s is ISequenceComponent component)
                {
                    GetObject(component);
                }
            }

            List<int> innerTicks = GetInnerTicks(outerTicks, ticksInside);

            for(var i = 0; i < components.Count; i++)
            {
                ISequenceComponent component = components[i];
                int ticks = innerTicks[i];
                if(component is Tuplet tuplet)
                {
                    tuplet.Duration.Ticks = ticks;
                    tuplet.SetTicksInContent(tuplet.Duration.Ticks, tuplet.TupletLevel + 1);
                }
                else
                {
                    Event evnt = component as Event;
                    evnt.Duration.Ticks = ticks;
                }
            }
        }

        /// <summary>
        /// This code was lifted from Moritz.Globals.IntDivisionSizes(total, relativeSizes).
        /// The function divides total into relativeSizes.Count parts, returning a List whose:
        ///     * Count is relativeSizes.Count.
        ///     * sum is exactly equal to total
        ///     * members have the relative sizes (as nearly as possible) to the values in the relativeSizes argument. 
        /// </summary>
        private List<int> GetInnerTicks(int total, List<int> relativeSizes)
        {
            int divisor = relativeSizes.Count;
            int sumRelative = 0;
            for(int i = 0; i < divisor; ++i)
            {
                sumRelative += relativeSizes[i];
            }
            float factor = ((float)total / (float)sumRelative);
            float fPos = 0;
            List<int> intPositions = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                intPositions.Add((int)(Math.Floor(fPos)));
                fPos += (relativeSizes[i] * factor);
            }
            intPositions.Add((int)Math.Floor(fPos));

            List<int> intDivisionSizes = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                int intDuration = (int)(intPositions[i + 1] - intPositions[i]);
                intDivisionSizes.Add(intDuration);
            }

            int intSum = 0;
            foreach(int i in intDivisionSizes)
            {
                //A.Assert(i >= 0);
                if(i < 0)
                {
                    throw new ApplicationException();
                }
                intSum += i;
            }
            A.Assert(intSum <= total);
            if(intSum < total)
            {
                int lastDuration = intDivisionSizes[intDivisionSizes.Count - 1];
                lastDuration += (total - intSum);
                intDivisionSizes.RemoveAt(intDivisionSizes.Count - 1);
                intDivisionSizes.Add(lastDuration);
            }
            return intDivisionSizes;
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

using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Tuplet : IWritable
    {
        /* Attributes:
         * outer - duration with respect to containing element
         * inner - duration of the enclosed sequence content
         * orient - optional orientation of this tuplet
         * staff - optional staff index of this tuplet
         * show-number - optional control over the display of the tuplet ratio numbers
         * show-value - optional control over the display of the tuplet ratio note values
         * bracket - optional control over the display of brackets
         */
        public readonly MNXC_Duration OuterDuration = null;
        public readonly MNXC_Duration InnerDuration = null;
        public readonly MNXOrientation Orient = MNXOrientation.undefined;
        public readonly int? Staff = null;
        public readonly MNXCTupletNumberDisplay ShowNumber = MNXCTupletNumberDisplay.inner; // default
        public readonly MNXCTupletNumberDisplay ShowValue = MNXCTupletNumberDisplay.none; // default
        public readonly MNXCTupletBracketDisplay Bracket = MNXCTupletBracketDisplay.auto; // default

        public readonly List<IWritable> Seq;

        public readonly int TupletLevel;

        public Tuplet(XmlReader r)
        {
            TupletLevel = G.CurrentTupletLevel; // top level tuplet has tuplet level 0

            G.CurrentTupletLevel++;

            // https://w3c.github.io/mnx/specification/common/#the-tuplet-element
            G.Assert(r.Name == "tuplet");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "outer":
                        OuterDuration = new MNXC_Duration(r.Value, G.CurrentTupletLevel);
                        break;
                    case "inner":
                        InnerDuration = new MNXC_Duration(r.Value, G.CurrentTupletLevel);
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

            Seq = G.GetSequenceContent(r, "tuplet", false);

            SetTicks();

            G.Assert(r.Name == "tuplet"); // end of (nested) tuplet

            G.CurrentTupletLevel--;
        }

        private void SetTicks()
        {
            int outerTicks = this.OuterDuration.GetBasicTicks();

            int localTupletLevel = this.TupletLevel + 1;

            List<int> ticksInside = new List<int>();
            List<MNXC_Duration> durationObjects = new List<MNXC_Duration>();
            List<Tuplet> nestedTuplets = new List<Tuplet>();

            void GetObject(IWritable s)
            {
                if(s is Event e)
                {
                    if(e.TupletLevel == localTupletLevel)
                    {
                        MNXC_Duration d = e.Duration;
                        MNXC_Duration p = e.PerformedDuration;
                        int basicTicks = 0;
                        if(p != null)
                        {
                            basicTicks = p.GetBasicTicks();
                            durationObjects.Add(p);
                        }
                        else
                        {
                            basicTicks = d.GetBasicTicks();
                            durationObjects.Add(d);
                        }        
                        ticksInside.Add(basicTicks);
                    }
                }
                if(s is Tuplet t)
                {
                    // a nested tuplet
                    if(t.TupletLevel == localTupletLevel)
                    {
                        MNXC_Duration d = t.OuterDuration;
                        int basicTicks = d.GetBasicTicks();
                        durationObjects.Add(d);
                        ticksInside.Add(basicTicks);
                        nestedTuplets.Add(t);
                    }
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
                        GetObject(beamed.Seq[i]);
                    }
                }
                else
                {
                    GetObject(s);
                }
            }

            List<int> innerTicks = GetInnerTicks(outerTicks, ticksInside);

            for(var i = 0; i < durationObjects.Count; i++)
            {
                MNXC_Duration d = durationObjects[i];
                int ticks = innerTicks[i];
                d.SetTicks(ticks, localTupletLevel);
            }

            foreach(var tuplet in nestedTuplets)
            {
                tuplet.SetTicks();
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
                //G.Assert(i >= 0);
                if(i < 0)
                {
                    throw new ApplicationException();
                }
                intSum += i;
            }
            G.Assert(intSum <= total);
            if(intSum < total)
            {
                int lastDuration = intDivisionSizes[intDivisionSizes.Count - 1];
                lastDuration += (total - intSum);
                intDivisionSizes.RemoveAt(intDivisionSizes.Count - 1);
                intDivisionSizes.Add(lastDuration);
            }
            return intDivisionSizes;
        }

        private MNXOrientation GetMNXOrientation(string value)
        {
            MNXOrientation rval = MNXOrientation.undefined;
            switch(value)
            {
                case "up":
                    rval = MNXOrientation.up;
                    break;
                case "down":
                    rval = MNXOrientation.down;
                    break;
            }
            return rval;
        }

        private MNXCTupletNumberDisplay GetTupletNumberDisplay(string value)
        {
            MNXCTupletNumberDisplay rval = MNXCTupletNumberDisplay.inner; // default
            switch(value)
            {
                case "both":
                    rval = MNXCTupletNumberDisplay.both;
                    break;
                case "none":
                    rval = MNXCTupletNumberDisplay.none;
                    break;
                default:
                    break;
            }
            return rval;
        }
        private MNXCTupletBracketDisplay GetTupletBracketDisplay(string value)
        {
            MNXCTupletBracketDisplay rval = MNXCTupletBracketDisplay.auto; // default
            switch(value)
            {
                case "yes":
                    rval = MNXCTupletBracketDisplay.yes;
                    break;
                case "no":
                    rval = MNXCTupletBracketDisplay.no;
                    break;
                default:
                    break;
            }
            return rval;
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}

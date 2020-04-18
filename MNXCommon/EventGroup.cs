using MNX.AGlobals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal abstract class EventGroup : IHasTicks
    {
        public List<ISeqComponent> Seq = null;

        public List<IHasTicks> EventsAndEventGroups
        {
            get
            {
                List<IHasTicks> eventsAndEventGroups = new List<IHasTicks>();
                foreach(var item in Seq)
                {
                    if(item is IHasTicks iht)
                    {
                        eventsAndEventGroups.Add(iht);
                    }
                }
                return eventsAndEventGroups;
            }
        }

        /// <summary>
        /// Returns a flat sequence of Events constructed from
        /// any contained Events and EventGroups. 
        /// </summary>
        public List<Event> Events
        {
            get
            {
                List<Event> rval = new List<Event>();
                foreach(var item in Seq)
                {
                    if(item is EventGroup eg)
                    {
                        var eventList = eg.Events;
                        foreach(var e in eventList)
                        {
                            rval.Add(e);
                        }
                    }
                    else if(item is Event e)
                    {
                        rval.Add(e);
                    }
                }
                return rval;
            }
        }

        public virtual int Ticks
        {
            get
            {
                var eventList = Events;
                int rval = 0;
                foreach(var e in eventList)
                {
                    rval += e.Ticks;
                }
                return rval;
            }
            set
            {
                // EventGroup Ticks is virtual so that it can be overriden by Grace.
                // Grace.Ticks implements Ticks.set, so that it can flexiby steal Ticks from Event.
                // Event also implements Ticks.set.
                A.ThrowError("Application Error: This function should never be called.");       
            }
        }

        /// <summary>
        /// This function is called after getting the class specific attributes
        /// The XmlReader is currently pointing to the last attribute read or to
        /// the beginning of the containing (sequence-like) element.
        /// See https://w3c.github.io/mnx/specification/common/#elementdef-sequence
        /// The spec says:
        /// "directions occurring within sequence content must omit this ("location") attribute as their
        /// location is determined during the procedure of sequencing the content."
        /// </summary>
        protected static List<ISeqComponent> GetSequenceContent(XmlReader r, string caller, bool isGlobal)
        {
            /// local function, called below.
            /// The spec says:
            /// "directions occurring within sequence content (i.e.when isGlobal is false) must omit
            /// this ("location") attribute as their location is determined during the procedure of
            /// sequencing the content."
            /// If found, write a message to the console, explaining that such data is ignored.
            void CheckDirectionContent(List<ISeqComponent> seq)
            {
                bool global = isGlobal; // isGlobal is from the outer scope                
            }

            List<ISeqComponent> content = new List<ISeqComponent>();

            // Read to the first element inside the caller element.
            // These are all the elements that can occur inside sequence-like elements. (Some of them nest.)
            A.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "forward");

            while(r.Name == "directions" || r.Name == "event" || r.Name == "grace"
                || r.Name == "beamed" || r.Name == "tuplet" || r.Name == "forward" || r.Name == "sequence")
            {
                if(r.Name == caller && r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "directions":
                            content.Add(new Directions(r, isGlobal));
                            break;
                        case "event":
                            Event e = new Event(r);
                            content.Add(e);
                            break;
                        case "grace":
                            Grace g = new Grace(r);
                            content.Add(g);
                            break;
                        case "beamed":
                            content.Add(new Beamed(r));
                            break;
                        case "tuplet":
                            content.Add(new Tuplet(r));
                            break;
                        case "forward":
                            content.Add(new Forward(r));
                            break;
                    }
                }

                A.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "forward", "sequence");
            }

            CheckDirectionContent(content);

            A.Assert(r.Name == caller); // end of sequence content

            return content;
        }

        /// <summary>
        /// This code is the same as in Moritz.Globals.IntDivisionSizes(total, relativeSizes).
        /// The function divides total into relativeSizes.Count parts, returning a List whose:
        ///     * Count is relativeSizes.Count.
        ///     * sum is exactly equal to total
        ///     * members have the relative sizes (as nearly as possible) to the values in the relativeSizes argument. 
        /// </summary>
        protected static List<int> GetJustifiedInnerTicks(int total, List<int> relativeSizes)
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
    }
}

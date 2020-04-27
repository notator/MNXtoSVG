﻿using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal abstract class EventGroup : IHasTicks
    {
        #region IUniqueDef
        /// <summary>
        /// (?) See IUniqueDef Interface definition. (?)
        /// </summary>
        public object Clone()
        {
            return this;
        }
        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = (int)(MsDuration * factor);
            M.Assert(MsDuration > 0, "An EventGroup's MsDuration may not be set to zero!");
        }

        public int MsDuration
        {
            get
            {
                return _msDuration;
            }
            set
            {
                M.Assert(value > 0);
                _msDuration = value;
            }
        }
        private int _msDuration;

        public int MsPositionReFirstUD
        {
            get
            {
                M.Assert(_msPositionReFirstIUD >= 0);
                return _msPositionReFirstIUD;
            }
            set
            {
                M.Assert(value >= 0);
                _msPositionReFirstIUD = value;
            }
        }
        private int _msPositionReFirstIUD = 0;

        #endregion IUniqueDef

        public List<ISeqComponent> SequenceComponents = null;

        public List<IHasTicks> EventsAndEventGroups
        {
            get
            {
                List<IHasTicks> eventsAndEventGroups = new List<IHasTicks>();
                foreach(var item in SequenceComponents)
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
                foreach(var item in SequenceComponents)
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
                M.ThrowError("Application Error: This function should never be called.");       
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
        protected static List<ISeqComponent> GetSequenceComponents(XmlReader r, string caller, bool isGlobal)
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
            M.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "forward");

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

                M.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "forward", "sequence");
            }

            CheckDirectionContent(content);

            M.Assert(r.Name == caller); // end of sequence content

            return content;
        }
    }
}

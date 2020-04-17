using MNX.AGlobals;
using System.Collections.Generic;

namespace MNX.Common
{
    internal abstract class EventGroup : ITicks
    {
        public List<ISeqComponent> Seq = null;

        public List<ITicks> EventsAndEventGroups
        {
            get
            {
                List<ITicks> eventsAndEventGroups = new List<ITicks>();
                foreach(var item in Seq)
                {
                    if(item is ITicks it)
                    {
                        eventsAndEventGroups.Add(it);
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
    }
}

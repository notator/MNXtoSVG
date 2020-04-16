using System.Collections.Generic;

namespace MNX.Common
{
    internal abstract class EventGroup
    {
        public List<ISeqComponent> Seq = null;

        /// <summary>
        /// Returns a flat sequence of Events constructed from
        /// any contained Events and EventGroups. 
        /// </summary>
        public List<Event> EventList
        {
            get
            {
                List<Event> rval = new List<Event>();
                foreach(var item in Seq)
                {
                    if(item is EventGroup eg)
                    {
                        var eventList = eg.EventList;
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

        public int Ticks
        {
            get
            {
                var eventList = EventList;
                int rval = 0;
                foreach(var e in eventList)
                {
                    rval += e.Duration.Ticks;
                }
                return rval;
            }
            set
            {
                int totalTicks = value;
                var eventList = EventList;
                List<int> tickss = new List<int>();
                foreach(var e in eventList)
                {
                    tickss.Add(e.Duration.Ticks);
                }
                List<int> newTickss = B.GetInnerTicks(totalTicks, tickss);

                for(var i = 0; i < newTickss.Count; i++)
                {
                    eventList[i].Duration.Ticks = newTickss[i];
                }
            }
        }
    }
}

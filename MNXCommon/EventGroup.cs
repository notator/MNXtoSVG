using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public abstract class EventGroup : IHasTicks
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
        public int MsPosInScore = -1;

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

        public List<ISequenceComponent> Components = new List<ISequenceComponent>();

        public List<IHasTicks> EventsAndEventGroups
        {
            get
            {
                List<IHasTicks> eventsAndEventGroups = new List<IHasTicks>();
                foreach(var item in Components)
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
                foreach(var item in Components)
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

        public virtual int TicksDuration
        {
            get
            {
                var eventList = Events;
                int rval = 0;
                foreach(var e in eventList)
                {
                    rval += e.TicksDuration;
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
        public int TicksPosInScore { get; set; }
    }
}

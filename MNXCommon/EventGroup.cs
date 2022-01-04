using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public abstract class EventGroup : IHasTicksDuration
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

        /// <summary>
        /// Returns a flat sequence of Event, Grace and Forward objects.
        /// (The Grace objects are still complete EventGroups)
        /// </summary>
        public List<IHasTicksDuration> EventsGracesAndForwards
        {
            get
            {
                List<IHasTicksDuration> GetEventsGracesOrForwards(EventGroup eventGroup)
                {
                    List<IHasTicksDuration> localRval = new List<IHasTicksDuration>();
                    foreach(var item in eventGroup.Components)
                    {
                        if(item is EventGroup eg && !(eg is Grace))
                        {
                            localRval.AddRange(GetEventsGracesOrForwards(eg)); // recursive call
                        }
                        else if(item is Event e)
                        {
                            localRval.Add(e);
                        }
                        else if(item is Forward f)
                        {
                            localRval.Add(f);
                        }
                        else if(item is Grace g)
                        {
                            localRval.Add(g);
                        }
                    }

                    return localRval;
                }
                List<IHasTicksDuration> rval = new List<IHasTicksDuration>();
                foreach(var item in Components)
                {
                    if(item is EventGroup eg && !(eg is Grace))
                    {
                        var eventList = GetEventsGracesOrForwards(eg);
                        rval.AddRange(eventList);
                    }
                    else if(item is Event e)
                    {
                        rval.Add(e);
                    }
                    else if(item is Forward f)
                    {
                        rval.Add(f);
                    }
                    else if(item is Grace g)
                    {
                        rval.Add(g);
                    }
                }
                return rval;
            }
        }

        public virtual int TicksDuration
        {
            get
            {
                var eventList = EventsGracesAndForwards;
                int rval = 0;
                foreach(var e in eventList)
                {
                    rval += e.TicksDuration;
                }
                return rval;
            }
            set
            {
                // EventGroup TicksDuration is virtual so that it can be overriden by Grace and Tuplet.
                // Grace.TicksDuration implements Ticks.set, so that it can flexiby steal Ticks from Event.
                // Event and Forward (which are not EventGroups) also implements TicksDuration.set.
                // This default implementation is
                M.ThrowError("Application Error: This function should never be called.");
            }
        }
    }
}

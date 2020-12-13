﻿using MNX.Globals;
using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public class Global
    {
        public List<string> PartIDs = null;
        public List<GlobalMeasure> GlobalMeasures = new List<GlobalMeasure>();

        public Global(XmlReader r)
        {
            M.Assert(r.Name == "global");
            // https://w3c.github.io/mnx/specification/common/#the-global-element

            TimeSignature currentTimeSig = null;
            int currentTicksPosInScore = 0;

            // can have a "parts" attribute
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "parts":
                        var partIDs = r.Value.Split(' ');
                        PartIDs = new List<string>(partIDs);
                        break;
                }
            }

            M.ReadToXmlElementTag(r, "measure");

            int measureIndex = 0;
            while(r.Name == "measure")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    GlobalMeasure globalMeasure = new GlobalMeasure(r, measureIndex++, currentTimeSig, currentTicksPosInScore);
                    currentTimeSig = (globalMeasure.GlobalDirections.TimeSignature != null) ? globalMeasure.GlobalDirections.TimeSignature : currentTimeSig;
                    currentTicksPosInScore += currentTimeSig.TicksDuration;
                    GlobalMeasures.Add(globalMeasure);
                }
                M.ReadToXmlElementTag(r, "measure", "global");
            }
            M.Assert(r.Name == "global"); // end of global

            M.Assert(GlobalMeasures.Count > 0);
        }

        public List<List<IUniqueDef>> GetGlobalIUDsPerMeasure()
        {
            var rval = new List<List<IUniqueDef>>();
            for(var measureIndex = 0; measureIndex < GlobalMeasures.Count; measureIndex++)
            {
                List<IUniqueDef> measureList = new List<IUniqueDef>();
                GlobalDirections globalDirections = GlobalMeasures[measureIndex].GlobalDirections;
                if(globalDirections != null)
                {
                    if(globalDirections.KeySignature != null)
                    {
                        measureList.Add(globalDirections.KeySignature);
                    }
                    if(globalDirections.TimeSignature != null)
                    {
                        measureList.Add(globalDirections.TimeSignature);
                    }
                    if(globalDirections.OctaveShift != null)
                    {
                        measureList.Add(globalDirections.OctaveShift);
                    }
                }
                rval.Add(measureList);
            }
            return rval;
        }

        /// <summary>
        /// Returns a list of Repeat objects per measure.
        /// Each list contains repeat objects in order of their ticks position.
        /// </summary>
        /// <returns></returns>
        public List<List<Repeat>> GetRepeatSymbolsPerMeasure()
        {
            var rval = new List<List<Repeat>>();
            for(var measureIndex = 0; measureIndex < GlobalMeasures.Count; measureIndex++)
            {
                var measureList = new List<Repeat>();
                rval.Add(measureList);
                var directions = GlobalMeasures[measureIndex].GlobalDirections;
                if(directions != null)
                {
                    SortedList<Repeat,int> sortedRepeats = directions.Repeats;
                    if(sortedRepeats != null)
                    {
                        foreach(var repeat in sortedRepeats)
                        {
                            measureList.Add(repeat.Key);
                        }
                    }
                }
            }
            return rval;
        }
    }
}
using MNX.Globals;
using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public class Global
    {
        public List<string> PartIDs = null;
        public List<Measure> Measures = new List<Measure>();

        public Global(XmlReader r)
        {
            M.Assert(r.Name == "global");
            // https://w3c.github.io/mnx/specification/common/#the-global-element

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
                    Measure measure = new Measure(r, measureIndex++, -1, true);
                    Measures.Add(measure);
                }
                M.ReadToXmlElementTag(r, "measure", "global");
            }
            M.Assert(r.Name == "global"); // end of global

            M.Assert(Measures.Count > 0);
        }

        public List<List<IUniqueDef>> GetGlobalIUDsPerMeasure()
        {
            var rval = new List<List<IUniqueDef>>();
            for(var measureIndex = 0; measureIndex < Measures.Count; measureIndex++)
            {
                List<IUniqueDef> measureList = new List<IUniqueDef>();
                Directions directions = Measures[measureIndex].Directions;
                if(directions != null)
                {
                    if(directions.KeySignature != null)
                    {
                        measureList.Add(directions.KeySignature);
                    }
                    if(directions.TimeSignature != null)
                    {
                        measureList.Add(directions.TimeSignature);
                    }
                    if(directions.OctaveShift != null)
                    {
                        measureList.Add(directions.OctaveShift);
                    }
                }
                rval.Add(measureList);
            }
            return rval;
        }

        /// <summary>
        /// Item1 in each Tuple is RepeatBeginBarline (true or false)
        /// Item2 in each Tuple is RepeatEndBarline (true or false) 
        /// </summary>
        /// <returns></returns>
        public List<Tuple<bool, bool>> GetGlobalRepeatBarlineTypesPerMeasure()
        {
            var rval = new List<Tuple<bool, bool>>();
            for(var measureIndex = 0; measureIndex < Measures.Count; measureIndex++)
            {
                Directions directions = Measures[measureIndex].Directions;
                Tuple<bool, bool> measureData = new Tuple<bool, bool>(directions.RepeatBeginBarline, directions.RepeatEndBarline);
                rval.Add(measureData);
            }
            return rval;
        }
    }
}
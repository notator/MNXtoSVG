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
        public List<GlobalMeasure> GlobalMeasures = new List<GlobalMeasure>();

        public Global(XmlReader r)
        {
            M.Assert(r.Name == "global");
            // https://w3c.github.io/mnx/specification/common/#the-global-element

            TimeSignature currentTimeSig = null;

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

            M.ReadToXmlElementTag(r, "measure-global");

            int measureIndex = 0;
            while(r.Name == "measure-global")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    GlobalMeasure globalMeasure = new GlobalMeasure(r, measureIndex++, currentTimeSig);
                    currentTimeSig = globalMeasure.GlobalDirections.CurrentTimeSignature;
                    GlobalMeasures.Add(globalMeasure);
                }
                M.ReadToXmlElementTag(r, "measure-global", "global");
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
                    var components = globalDirections.Components;
                    KeySignature keySignature = components.Find(x => x is KeySignature) as KeySignature;
                    if(keySignature != null)
                    {
                        measureList.Add(keySignature);
                    }
                    TimeSignature timeSignature = components.Find(x => x is TimeSignature) as TimeSignature;
                    if(timeSignature != null)
                    {
                        measureList.Add(timeSignature);
                    }
                    RepeatBegin repeatBegin = components.Find(x => x is RepeatBegin) as RepeatBegin;
                    if(repeatBegin != null)
                    {
                        measureList.Add(repeatBegin);
                    }
                    RepeatEnd repeatEnd = components.Find(x => x is RepeatEnd) as RepeatEnd;
                    if(repeatEnd != null)
                    {
                        measureList.Add(repeatEnd);
                    }
                    Segno segno = components.Find(x => x is Segno) as Segno;
                    if(segno != null)
                    {
                        measureList.Add(segno);
                    }
                    Jump jump = components.Find(x => x is Jump) as Jump;
                    if(jump != null)
                    {
                        measureList.Add(jump);
                    }
                    Fine fine = components.Find(x => x is Fine) as Fine;
                    if(fine != null)
                    {
                        measureList.Add(fine);
                    }
                }
                rval.Add(measureList);
            }
            return rval;
        }
    }
}
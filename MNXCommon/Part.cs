using System;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    public class Part
    {
        public readonly string PartName;
        public readonly string PartAbbreviation;
        public readonly string InstrumentSound;
        public List<Measure> Measures = new List<Measure>();

        public List<int> VoicesPerStaff
        {
            get
            {
                var nStaves = 1; // minimum
                foreach(var measure in Measures)
                {
                    foreach(var sequence in measure.Sequences)
                    {
                        if(sequence.StaffIndex != null)
                        {
                            int staffNumber = ((int)sequence.StaffIndex) + 1;
                            nStaves = (nStaves > staffNumber) ? nStaves : staffNumber;
                        }
                    }
                }
                List<int> voicesPerStaff = new List<int>();
                for(var staffIndex = 0; staffIndex < nStaves; staffIndex++)
                {
                    int maxStaffVoices = 0;
                    foreach(var measure in Measures)
                    {
                        int nMeasureStaffVoices = 0;
                        foreach(var sequence in measure.Sequences)
                        {
                            if(staffIndex == 0 || sequence.StaffIndex == null)
                            {
                                nMeasureStaffVoices++;
                            }
                            else
                            {
                                nMeasureStaffVoices++;
                            }
                        }
                        maxStaffVoices = (maxStaffVoices > nMeasureStaffVoices) ? maxStaffVoices : nMeasureStaffVoices;
                    }

                    M.Assert(maxStaffVoices <= 2, "MNXtoSVG does not support more than two voices per staff.");

                    voicesPerStaff.Add(maxStaffVoices);
                }

                return voicesPerStaff;
            }
        }

        public Part(XmlReader r)
        {
            M.Assert(r.Name == "part");
            // https://w3c.github.io/mnx/specification/common/#the-part-element

            M.ReadToXmlElementTag(r, "part-name", "part-abbreviation", "instrument-sound", "measure");

            int measureIndex = 0;
            int ticksPosInScore = 0;
            while(r.Name == "part-name" || r.Name == "part-abbreviation" || r.Name == "instrument-sound" || r.Name == "measure")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "part-name":
                            PartName = r.ReadElementContentAsString();
                            break;
                        case "part-abbreviation":
                            PartAbbreviation = r.ReadElementContentAsString();
                            break;
                        case "instrument-sound":
                            InstrumentSound = r.ReadElementContentAsString();
                            break;
                        case "measure":
                            Measure measure = new Measure(r, measureIndex++, ticksPosInScore, false);
                            ticksPosInScore += measure.TicksDuration;
                            Measures.Add(measure);
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "part-name", "part-abbreviation", "instrument-sound", "measure", "part");
            }
            M.Assert(r.Name == "part"); // end of part

            SetOctaveShiftExtendersEndTicksPos(Measures);
        }

        private void SetOctaveShiftExtendersEndTicksPos(List<Measure> measures)
        {
            for(var measureIndex = 0; measureIndex < measures.Count; measureIndex++)
            {
                var measure = measures[measureIndex];
                foreach(var sequence in measure.Sequences)
                {
                    foreach(var components in sequence.SequenceComponents)
                    {
                        if(components is Directions directions && directions.OctaveShift != null)
                        {
                            var octaveShift = directions.OctaveShift;
                            var item1 = octaveShift.EndOctaveShiftPos.Item1;
                            int endMeasureIndex = (item1 == null) ? measureIndex : (int)item1;
                            var tickPosInMeasure = octaveShift.EndOctaveShiftPos.Item2;
                            octaveShift.EndTicksPosInScore = measures[endMeasureIndex].TicksPosInScore + tickPosInMeasure;
                        }
                    }
                }
            }
        }
    }
}
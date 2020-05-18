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
                            Measures.Add(new Measure(r, false));
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "part-name", "part-abbreviation", "instrument-sound", "measure", "part");
            }
            M.Assert(r.Name == "part"); // end of part
        }
    }
}
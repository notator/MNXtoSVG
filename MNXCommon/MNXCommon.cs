using System;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;
using Moritz.Spec;
using Moritz.Symbols;

namespace MNX.Common
{
    public class MNXCommon
    {
        internal readonly List<Global> Globals = new List<Global>();
        internal readonly List<Part> Parts = new List<Part>();
        internal readonly List<ScoreAudio> ScoreAudios = new List<ScoreAudio>();

        public MNXCommon(XmlReader r)
        {
            M.Assert(r.Name == "mnx-common");
            // https://w3c.github.io/mnx/specification/common/#the-mnx-common-element

            M.Profile = null;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "profile":
                        { 
                            switch(r.Value)
                            {
                                case "standard":
                                    M.Profile = MNXProfile.MNXCommonStandard;
                                    break;
                                default:
                                    M.ThrowError("Error: unknown profile");
                                    break;
                            }
                        }
                        break;
                    default:
                        M.ThrowError("Error: unknown attribute");
                        break;
                }
            }

            M.ReadToXmlElementTag(r, "global", "part", "score-audio");

            while(r.Name == "global" || r.Name == "part" || r.Name == "score-audio")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "global":
                            Globals.Add(new Global(r));
                            break;
                        case "part":
                            Parts.Add(new Part(r));
                            break;
                        case "score-audio":
                            ScoreAudios.Add(new ScoreAudio(r));
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "global", "part", "score-audio", "mnx-common");
            }

            AdjustForGraceNotes();

            M.Assert(r.Name == "mnx-common"); // end of "mnx-common"

            M.Assert(Globals.Count > 0);
            M.Assert(Parts.Count > 0);
            M.Assert(ScoreAudios.Count >= 0);
        }

        public MNXCommonData GetMNXCommonData()
        {
            List<VoiceDef> voiceDefs = new List<VoiceDef>();
            List<List<int>> midiChannelsPerStaff = new List<List<int>>();
            List<int> msPositionPerBar = new List<int>();

            int currentMIDIChannel = 0;

            List<List<Trk>> Tracks = new List<List<Trk>>();
            foreach(var part in Parts)
            {
                int nTracks = part.Measures[0].Sequences.Count;
                List<int> midiChannelsPerPart = new List<int>();
                for(var i = 0; i < nTracks; i++)
                {
                    midiChannelsPerPart.Add(currentMIDIChannel);
                    List<Trk> track = new List<Trk>();
                    foreach(var measure in part.Measures)
                    {
                        List<IUniqueDef> iuds = measure.Sequences[i].GetIUDs();
                        Trk newTrk = new Trk(currentMIDIChannel, 0, iuds);
                        track.Add(newTrk);
                    }
                    currentMIDIChannel++;
                }
                midiChannelsPerStaff.Add(midiChannelsPerPart);
            }

            msPositionPerBar = GetMsPositionPerBar(Tracks[0]);
            voiceDefs = GetVoiceDefs(Tracks);

            MNXCommonData mnxCommonData = new MNXCommonData()
            {
                VoiceDefs = voiceDefs,
                MidiChannelsPerStaff = midiChannelsPerStaff,
                MsPositionPerBar = msPositionPerBar
            };

            return mnxCommonData;
        }

        private List<int> GetMsPositionPerBar(List<Trk> trks)
        {
            List<int> rval = new List<int>();
            int currentPosition = 0;
            foreach(var trk in trks)
            {
                rval.Add(currentPosition);
                currentPosition += trk.MsDuration;
            }
            return rval;
        }

        /// <summary>
        /// This function consumes its argumant.
        /// </summary>
        /// <param name="tracks"></param>
        /// <returns></returns>
        private List<VoiceDef> GetVoiceDefs(List<List<Trk>> tracks)
        {
            var rval = new List<VoiceDef>();

            foreach(var trkList in tracks)
            {
                Trk trk = trkList[0];
                for(var i = 1; i < trkList.Count; i++)
                {
                    trk.AddRange(trkList[i]);
                }
                rval.Add(trk);
            }
            return rval;
        }

        private void AdjustForGraceNotes()
        {
            for(var partIndex = 0; partIndex < Parts.Count; partIndex++)
            {
                List<Measure> measures = Parts[partIndex].Measures;
                for(var measureIndex = 0; measureIndex < measures.Count; measureIndex++)
                {
                    Measure measure = measures[measureIndex];
                    measure.AdjustForGraceNotes();
                }
            }
        }
    }
}
using MNX.Globals;
using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-event-element
    /// </summary>
    public class Beams : ISequenceComponent
    {
        public readonly List<Beam> ContainedBeams = new List<Beam>();
        public readonly List<BeamHook> ContainedBeamHooks = new List<BeamHook>();
        public int TicksPosInScore { get { return _ticksPosInScore; } }
        private readonly int _ticksPosInScore;

        public Beams(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "beams");
            
            _ticksPosInScore = ticksPosInScore;

            M.ReadToXmlElementTag(r, "beam");

            int topLevelDepth = r.Depth;

            while(r.Name == "beam" || r.Name == "beam-hook")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "beam":
                            {
                                ContainedBeams.Add(new Beam(r, ticksPosInScore, topLevelDepth));
                                break;
                            }
                        case "beam-hook":
                            {
                                ContainedBeamHooks.Add(new BeamHook(r, ticksPosInScore, topLevelDepth));
                                break;
                            }
                    }                    
                }
                M.ReadToXmlElementTag(r, "beam", "beam-hook", "beams");
            }
            M.Assert(r.Name == "beams"); // end of beams
        }
    }
}

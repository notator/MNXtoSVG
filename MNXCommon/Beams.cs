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

        public Beams(XmlReader r)
        {
            M.Assert(r.Name == "beams");

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
                                ContainedBeams.Add(new Beam(r, topLevelDepth));
                                break;
                            }
                        case "beam-hook":
                            {
                                ContainedBeamHooks.Add(new BeamHook(r, topLevelDepth));
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

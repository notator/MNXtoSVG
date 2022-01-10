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
    public class BeamBlocks : ISequenceComponent
    {
        public readonly List<BeamBlock> Blocks = new List<BeamBlock>();

        public BeamBlocks(XmlReader r)
        {
            M.Assert(r.Name == "beams");

            M.ReadToXmlElementTag(r, "beam");

            BeamBlock currentBeamBlock = null;

            int topLevelDepth = r.Depth;

            while(r.Name == "beam" || r.Name == "beam-hook")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "beam":
                            {
                                if(r.Depth == topLevelDepth)
                                {
                                    currentBeamBlock = new BeamBlock();
                                    Blocks.Add(currentBeamBlock);
                                }
                                currentBeamBlock.ContainedBeams.Add(new Beam(r, topLevelDepth));
                                break;
                            }
                        case "beam-hook":
                            {
                                currentBeamBlock.ContainedBeamHooks.Add(new BeamHook(r, topLevelDepth));
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

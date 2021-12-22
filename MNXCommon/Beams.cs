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

        public Beams(XmlReader r, int ticksPosInScore)
        {
            M.Assert(r.Name == "beams");

            M.ReadToXmlElementTag(r, "beam");

            while(r.Name == "beam")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "beam":
                            ContainedBeams.Add(new Beam(r, ticksPosInScore));
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "beam", "beams");
            }
            M.Assert(r.Name == "beams"); // end of beams
        }
    }
}

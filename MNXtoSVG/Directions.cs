using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Directions : IWritable
    {
        // These are just the elements used in the first set of examples.
        // Other elements need to be added later.
        public readonly Time Time;
        public readonly Clef Clef;
        public readonly Key Key;
        public readonly OctaveShift OctaveShift;

        public Directions(XmlReader r, bool isGlobal)
        {
            G.Assert(r.Name == "directions");
            // https://w3c.github.io/mnx/specification/common/#elementdef-directions

            // These are just the elements used in the first set of examples.
            // Other elements need to be added later.
            G.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift");

            while(r.Name == "time" || r.Name == "clef" || r.Name == "key" || r.Name == "octave-shift")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "time":
                            // https://w3c.github.io/mnx/specification/common/#the-time-element
                            if(G.MNXProfile == G.MNXProfileEnum.MNXCommonStandard && isGlobal == false)
                            {
                                G.ThrowError("Error: the time element must be global in standard mnx-common.");
                            }
                            Time = new Time(r);
                            break;
                        case "clef":
                            Clef = new Clef(r);
                            break;
                        case "key":
                            // https://w3c.github.io/mnx/specification/common/#the-key-element
                            Key = new Key(r);
                            break;
                        case "octave-shift":
                            OctaveShift = new OctaveShift(r);
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift", "directions");
            }
            G.Assert(r.Name == "directions"); // end of "directions"
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
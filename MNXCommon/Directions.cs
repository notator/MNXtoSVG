using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-directions
    internal class Directions : IGlobalMeasureComponent, IPartMeasureComponent, ISeqComponent
    {
        // These are just the elements used in the first set of examples.
        // Other elements need to be added later.
        public readonly TimeSignature TimeSignature;
        public readonly Clef Clef;
        public readonly KeySignature KeySignature;
        public readonly OctaveShift OctaveShift;

        public Directions(XmlReader r, bool isGlobal)
        {
            A.Assert(r.Name == "directions");

            // These are just the elements used in the first set of examples.
            // Other elements need to be added later.
            A.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift");

            while(r.Name == "time" || r.Name == "clef" || r.Name == "key" || r.Name == "octave-shift")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "time":
                            // https://w3c.github.io/mnx/specification/common/#the-time-element
                            if(A.Profile == MNXProfile.MNXCommonStandard && isGlobal == false)
                            {
                                A.ThrowError("Error: the time element must be global in standard mnx-common.");
                            }
                            TimeSignature = new TimeSignature(r);
                            break;
                        case "clef":
                            Clef = new Clef(r);
                            break;
                        case "key":
                            // https://w3c.github.io/mnx/specification/common/#the-key-element
                            KeySignature = new KeySignature(r);
                            break;
                        case "octave-shift":
                            OctaveShift = new OctaveShift(r);
                            break;
                    }
                }
                A.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift", "directions");
            }
            A.Assert(r.Name == "directions"); // end of "directions"
        }
    }
}
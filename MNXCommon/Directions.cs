using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNX.Globals;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-directions
    public class Directions : IGlobalMeasureComponent, IPartMeasureComponent, ISeqComponent
    {
        // These are just the elements used in the first set of examples.
        // Other elements need to be added later.
        public readonly TimeSignature TimeSignature;
        public readonly Clef Clef;
        public readonly KeySignature KeySignature;
        public readonly OctaveShift OctaveShift;

        #region IUniqueDef
        public override string ToString() => $"Directions: MsPositionReFirstIUD={MsPositionReFirstUD} MsDuration={MsDuration}";

        /// <summary>
        /// (?) See IUniqueDef Interface definition. (?)
        /// </summary>
        public object Clone()
        {
            return this;
        }
        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor)
        {
            MsDuration = 0;
        }

        public int MsDuration { get { return 0; } set { M.Assert(false, "Application Error."); } }

        public int MsPositionReFirstUD
        {
            get
            {
                M.Assert(_msPositionReFirstIUD >= 0);
                return _msPositionReFirstIUD;
            }
            set
            {
                M.Assert(value >= 0);
                _msPositionReFirstIUD = value;
            }
        }
        private int _msPositionReFirstIUD = 0;

        #endregion IUniqueDef

        public Directions(XmlReader r, bool isGlobal)
        {
            M.Assert(r.Name == "directions");

            // These are just the elements used in the first set of examples.
            // Other elements need to be added later.
            M.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift");

            while(r.Name == "time" || r.Name == "clef" || r.Name == "key" || r.Name == "octave-shift")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "time":
                            // https://w3c.github.io/mnx/specification/common/#the-time-element
                            if(M.Profile == MNXProfile.MNXCommonStandard && isGlobal == false)
                            {
                                M.ThrowError("Error: the time element must be global in standard mnx-common.");
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
                M.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift", "directions");
            }
            M.Assert(r.Name == "directions"); // end of "directions"
        }
    }
}
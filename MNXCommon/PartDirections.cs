using MNX.Globals;

using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-directions
    public class PartDirections : IPartMeasureComponent
    {
        // These are just the elements used in the first set of examples.
        // Other elements may need to be added later.
        public readonly Clef Clef;
        public readonly KeySignature KeySignature;

        public readonly List<IPartDirectionsComponent> Components = new List<IPartDirectionsComponent>();

        #region IUniqueDef
        public override string ToString() => $"PartDirections";

        /// <summary>
        /// (?) See IUniqueDef Interface definition. (?)
        /// </summary>
        public object Clone()
        {
            return this;
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

        public PartDirections(XmlReader r)
        {
            M.Assert(r.Name == "directions-part");

            // These are just the elements used in the first set of examples.
            // Other elements need to be added later.
            M.ReadToXmlElementTag(r, "clef", "key");

            while(r.Name == "clef" || r.Name == "key")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "clef":
                            Clef = new Clef(r);
                            Components.Add(Clef);
                            break;
                        case "key":
                            // https://w3c.github.io/mnx/specification/common/#the-key-element
                            KeySignature = new KeySignature(r);
                            Components.Add(KeySignature);
                            break;

                    }
                }
                M.ReadToXmlElementTag(r, "clef", "key", "directions-part");
            }

            M.Assert(r.Name == "directions-part"); // end of "directions-part"
        }
    }
}
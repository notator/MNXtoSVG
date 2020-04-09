using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Directions : IWritable
    {
        /// <summary>
        /// https://w3c.github.io/mnx/specification/common/#measure-location
        /// This string is currently just the value in the file.
        /// Here are some instances of the measure location syntax from the spec:
        /// 0.25   -> one quarter note after the start of a containing measure 
        /// 3/8    -> three eighth notes after the start of a containing measure 
        /// 4:0.25 -> one quarter note after the start of the measure with index 4 
        /// 4:1/4  -> the same as the preceding example
        /// #event235 -> the same metrical position as the event whose element ID is event235
        /// 
        /// This value is *writable*: see https://w3c.github.io/mnx/specification/common/#sequence-the-content
        /// "directions occurring within sequence content must omit this attribute as their
        /// location is determined during the procedure of sequencing the content."
        /// </summary>
        public string Location = null;
        public readonly int? StaffIndex = null;
        public readonly G.MNXOrientation Orientation = G.MNXOrientation.undefined;

        // These are just the elements used in the first set of examples.
        // Other elements need to be added later.
        public readonly Time Time;
        public readonly object Clef;
        public readonly Key Key;

        public Directions(XmlReader r, string parentElement, string grandParentElement)
        {
            G.Assert(r.Name == "directions");
            // https://w3c.github.io/mnx/specification/common/#elementdef-directions

            if(parentElement == "directions")
            {
                Location = "0"; // default value in <directions><directions> (can be overridden below)
            }

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    // https://w3c.github.io/mnx/specification/common/#common-direction-attributes
                    case "location":
                        // https://w3c.github.io/mnx/specification/common/#measure-location
                        if(parentElement == "sequence")
                        {
                            G.ThrowError("Error: location attribute in <sequence><directions> element.");
                        }
                        Location = r.Value;
                        break;
                    case "staff":
                        // https://w3c.github.io/mnx/specification/common/#staff-index
                        StaffIndex = int.Parse(r.Value);
                        G.Assert(StaffIndex >= 1); // 1 based
                        break;
                    case "orient":
                        if(r.Value == "up")
                            Orientation = G.MNXOrientation.up;
                        else if(r.Value == "down")
                            Orientation = G.MNXOrientation.down;
                        break;
                    default:
                        G.ThrowError("Unknown directions attribute.");
                        break;

                }
            }

            // These are just the elements used in the first set of examples.
            // Other elements need to be added later.
            G.ReadToXmlElementTag(r, "time", "clef", "key", "octave-shift");

            while(r.Name == "time" || r.Name == "clef" || r.Name == "key")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "time":
                            // https://w3c.github.io/mnx/specification/common/#the-time-element
                            if(grandParentElement != "global")
                            {
                                G.ThrowError("Error: the time element must be global in standard mnx-common.");
                            }
                            Time = new Time(r);
                            break;
                        case "clef":
                            Clef = new Clef(r);
                            break;
                        case "key":
                            Key = new Key(r);
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "time", "clef", "key", "directions");
            }
            G.Assert(r.Name == "directions"); // end of "directions"
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
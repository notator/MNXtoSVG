using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class DirectionAttributes
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
        public int StaffIndex { get; private set; }
        public MNXOrientation Orientation { get; private set; }

        public DirectionAttributes()
        {
            Location = null;
            StaffIndex = -1;
            Orientation = MNXOrientation.undefined;
        }

        internal bool SetAttribute(XmlReader r)
        {
            bool rval = false;

            switch(r.Name)
            {
                // https://w3c.github.io/mnx/specification/common/#common-direction-attributes
                case "location":
                    // https://w3c.github.io/mnx/specification/common/#measure-location
                    Location = r.Value;
                    rval = true;
                    break;
                case "staff":
                    // https://w3c.github.io/mnx/specification/common/#staff-index
                    StaffIndex = int.Parse(r.Value);
                    G.Assert(StaffIndex >= 1); // 1 based
                    rval = true;
                    break;
                case "orient":
                    if(r.Value == "up")
                        Orientation = MNXOrientation.up;
                    else if(r.Value == "down")
                        Orientation = MNXOrientation.down;
                    rval = true;
                    break;
            }

            return rval;
        }
    }
}
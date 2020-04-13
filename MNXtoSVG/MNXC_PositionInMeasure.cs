using MNXtoSVG.Globals;
using System.Text;

namespace MNXtoSVG
{
    public class MNXC_PositionInMeasure
    {
        /*
         * Here are some instances of the measure location syntax:
         * 0.25
         * one quarter note after the start of a containing measure
         * 
         * 3/8
         * three eighth notes after the start of a containing measure
         * 
         * 4:0.25
         * one quarter note after the start of the measure with index 4
         * 
         * 4:1/4
         * the same as the preceding example
         * 
         * #event235
         * the same metrical position as the event whose element ID is event235
         */

        public readonly MNXC_Duration Position = null;
        public readonly int? MeasureNumber = null;
        public readonly string ID = null; // currently without the leading '#' (okay?)
        public readonly MNXC_ShortTieOrSlur? Short = null;

        /// <summary>
        /// The value argument is the MNXC measure location string
        /// ("0.25", "3/8", "4:0.25", "4:1/4", "#event235" etc.)
        /// I assume that the argument value can be "incoming" and "outgoing"
        /// in addition to the values described at
        /// https://w3c.github.io/mnx/specification/common/#measure-location
        /// See https://w3c.github.io/mnx/specification/common/#the-tied-element
        /// I'd prefer not to allow the decimal representations of position.
        /// These just duplicate the other options, and create unnecessary work
        /// for parsers, so I'm currently ignoring them.
        /// </summary>
        public MNXC_PositionInMeasure(string value)
        {
            StringBuilder sbValue = new StringBuilder(value);
            int colonPos = -1;

            if(value[0] == '#')
            {
                ID = value.Substring(1); // no '#' (okay?)
            }
            else if((colonPos = value.IndexOf(':')) < 0)
            {
                switch(value)
                {
                    case "incoming":
                        Short = MNXC_ShortTieOrSlur.incoming;
                        break;
                    case "outgoing":
                        Short = MNXC_ShortTieOrSlur.outgoing;
                        break;
                    default:
                        if(value.IndexOf('.') >= 0)
                            Position = new MNXC_Duration(value, G.CurrentTupletLevel);
                        break;
                }
            }
            else
            {
                char[] separator = { ':' };
                string[] mStrs = value.Split(separator, System.StringSplitOptions.None);
                int.TryParse(mStrs[0], out int measureNumber);
                MeasureNumber = measureNumber;
                Position = new MNXC_Duration(mStrs[1], G.CurrentTupletLevel);
            }
        }
    }
}
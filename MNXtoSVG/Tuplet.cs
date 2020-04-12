using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Tuplet : IWritable
    {
        /* Attributes:
         * outer - duration with respect to containing element
         * inner - duration of the enclosed sequence content
         * orient - optional orientation of this tuplet
         * staff - optional staff index of this tuplet
         * show-number - optional control over the display of the tuplet ratio numbers
         * show-value - optional control over the display of the tuplet ratio note values
         * bracket - optional control over the display of brackets
         */
        public readonly MNXC_Duration Outer = null;
        public readonly MNXC_Duration Inner = null;
        public readonly MNXOrientation Orient = MNXOrientation.undefined;
        public readonly int? Staff = null;
        public readonly MNXCTupletNumberDisplay ShowNumber = MNXCTupletNumberDisplay.inner; // default
        public readonly MNXCTupletNumberDisplay ShowValue = MNXCTupletNumberDisplay.none; // default
        public readonly MNXCTupletBracketDisplay Bracket = MNXCTupletBracketDisplay.auto; // default

        public readonly List<IWritable> Seq;

        public Tuplet(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-tuplet-element
            G.Assert(r.Name == "tuplet");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "outer":
                        Outer = new MNXC_Duration(r.Value);
                        break;
                    case "inner":
                        Inner = new MNXC_Duration(r.Value);
                        break;
                    case "orient":
                        Orient = GetMNXOrientation(r.Value);
                        break;
                    case "staff":
                        int staff;
                        int.TryParse(r.Value, out staff);
                        if(staff > 0)
                        {
                            Staff = staff;
                        }
                        break;
                    case "show-number":
                        ShowNumber = GetTupletNumberDisplay(r.Value);
                        break;
                    case "show-value":
                        ShowValue = GetTupletNumberDisplay(r.Value);
                        break;
                    case "bracket":
                        Bracket = GetTupletBracketDisplay(r.Value);
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            Seq = G.GetSequenceContent(r, "tuplet", false);

            G.Assert(r.Name == "tuplet"); // end of (nested) tuplet
        }

        private MNXOrientation GetMNXOrientation(string value)
        {
            MNXOrientation rval = MNXOrientation.undefined;
            switch(value)
            {
                case "up":
                    rval = MNXOrientation.up;
                    break;
                case "down":
                    rval = MNXOrientation.down;
                    break;
            }
            return rval;
        }

        private MNXCTupletNumberDisplay GetTupletNumberDisplay(string value)
        {
            MNXCTupletNumberDisplay rval = MNXCTupletNumberDisplay.inner; // default
            switch(value)
            {
                case "both":
                    rval = MNXCTupletNumberDisplay.both;
                    break;
                case "none":
                    rval = MNXCTupletNumberDisplay.none;
                    break;
                default:
                    break;
            }
            return rval;
        }
        private MNXCTupletBracketDisplay GetTupletBracketDisplay(string value)
        {
            MNXCTupletBracketDisplay rval = MNXCTupletBracketDisplay.auto; // default
            switch(value)
            {
                case "yes":
                    rval = MNXCTupletBracketDisplay.yes;
                    break;
                case "no":
                    rval = MNXCTupletBracketDisplay.no;
                    break;
                default:
                    break;
            }
            return rval;
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}

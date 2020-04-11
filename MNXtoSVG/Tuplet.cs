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
        public readonly G.MNXOrientation Orient = G.MNXOrientation.undefined;
        public readonly int? Staff = null;
        public readonly G.MNXCTupletNumberDisplay ShowNumber = G.MNXCTupletNumberDisplay.inner; // default
        public readonly G.MNXCTupletNumberDisplay ShowValue = G.MNXCTupletNumberDisplay.none; // default
        public readonly G.MNXCTupletBracketDisplay Bracket = G.MNXCTupletBracketDisplay.auto; // default

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

        private G.MNXOrientation GetMNXOrientation(string value)
        {
            G.MNXOrientation rval = G.MNXOrientation.undefined;
            switch(value)
            {
                case "up":
                    rval = G.MNXOrientation.up;
                    break;
                case "down":
                    rval = G.MNXOrientation.down;
                    break;
            }
            return rval;
        }

        private G.MNXCTupletNumberDisplay GetTupletNumberDisplay(string value)
        {
            G.MNXCTupletNumberDisplay rval = G.MNXCTupletNumberDisplay.inner; // default
            switch(value)
            {
                case "both":
                    rval = G.MNXCTupletNumberDisplay.both;
                    break;
                case "none":
                    rval = G.MNXCTupletNumberDisplay.none;
                    break;
                default:
                    break;
            }
            return rval;
        }
        private G.MNXCTupletBracketDisplay GetTupletBracketDisplay(string value)
        {
            G.MNXCTupletBracketDisplay rval = G.MNXCTupletBracketDisplay.auto; // default
            switch(value)
            {
                case "yes":
                    rval = G.MNXCTupletBracketDisplay.yes;
                    break;
                case "no":
                    rval = G.MNXCTupletBracketDisplay.no;
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

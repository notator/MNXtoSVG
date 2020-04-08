using MNXtoSVG.Globals;
using System;
using System.Xml;

namespace MNXtoSVG
{
    internal class Directions
    {
        // https://w3c.github.io/mnx/specification/common/#elementdef-directions

        public Directions(XmlReader r)
        {
            G.Assert(r.Name == "directions");

            //    int count = r.AttributeCount;
            //    for(int i = 0; i < count; i++)
            //    {
            //        r.MoveToAttribute(i);
            //        switch(r.Name)
            //        {
            //            case "index":
            //                break;
            //            case "number":
            //                break;
            //            case "barline":
            //                {
            //                    switch(r.Value)
            //                    {
            //                        // default is G.MNXBarlineType.undefined (see below)
            //                        case "regular":
            //                            this.BarlineType = G.MNXBarlineType.regular;
            //                            break;
            //                        case "dotted":
            //                            this.BarlineType = G.MNXBarlineType.dotted;
            //                            break;
            //                        case "dashed":
            //                            this.BarlineType = G.MNXBarlineType.dashed;
            //                            break;
            //                        case "heavy":
            //                            this.BarlineType = G.MNXBarlineType.heavy;
            //                            break;
            //                        case "light-light":
            //                            this.BarlineType = G.MNXBarlineType.lightLight;
            //                            break;
            //                        case "light-heavy":
            //                            this.BarlineType = G.MNXBarlineType.lightHeavy;
            //                            break;
            //                        case "heavy-light":
            //                            this.BarlineType = G.MNXBarlineType.heavyLight;
            //                            break;
            //                        case "heavy-heavy":
            //                            this.BarlineType = G.MNXBarlineType.heavyHeavy;
            //                            break;
            //                        case "tick":
            //                            this.BarlineType = G.MNXBarlineType.tick;
            //                            break;
            //                        case "short":
            //                            this.BarlineType = G.MNXBarlineType._short;
            //                            break;
            //                        case "none":
            //                            this.BarlineType = G.MNXBarlineType.none;
            //                            break;
            //                    }
            //                }
            //                break;
            //            default:
            //                throw new ApplicationException("Unknown attribute");
            //        }
            //    }
            //
        }

        //public readonly G.MNXBarlineType BarlineType = G.MNXBarlineType.undefined; // default
    }
}
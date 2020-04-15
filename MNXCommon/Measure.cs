using MNX.AGlobals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal class Measure : ITicks
    {
        /// <summary>
        /// If null, this value should be set when the whole score has been read
        /// see https://w3c.github.io/mnx/specification/common/#the-measure-element
        /// </summary>
        public int? Number = null;
        /// <summary>
        /// If null, this value should be set when the whole score has been read
        /// see https://w3c.github.io/mnx/specification/common/#the-measure-element
        /// </summary>
        public int? Index = null;
        public readonly BarlineType? Barline = null; // default

        public readonly Directions Directions = null;
        public readonly List<Sequence> Sequences = new List<Sequence>();

        public int Ticks
        {
            get
            {
                int ticks = Sequences[0].Ticks;
                for(var i = 1; i < Sequences.Count; i++)
                {
                    A.Assert(Sequences[i].Ticks == ticks);
                }
                return ticks;
            }
        }

        public Measure(XmlReader r, bool isGlobal)
        {
            A.Assert(r.Name == "measure");
            // https://w3c.github.io/mnx/specification/common/#the-measure-element

            if(r.IsEmptyElement)
            {
                if(isGlobal)
                {
                    return;
                }
                else
                {
                    A.ThrowError("Empty measure in part.");
                }
            }

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "index":
                        Index = Int32.Parse(r.Value);
                        A.Assert(Index > 0);
                        break;
                    case "number":
                        Number = Int32.Parse(r.Value);
                        A.Assert(Number > 0);
                        break;
                    case "barline":
                        Barline = GetBarlineType(r.Value);
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            A.ReadToXmlElementTag(r, "directions", "sequence");

            while(r.Name == "directions" || r.Name == "sequence")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "directions":
                            Directions = new Directions(r, isGlobal);
                            break;
                        case "sequence":
                            if(isGlobal)
                            {
                                A.ThrowError("Error in input file.");
                            }
                            Sequences.Add(new Sequence(r, isGlobal));
                            break;
                    }
                }
                A.ReadToXmlElementTag(r, "directions", "sequence", "measure");
            }
            A.Assert(r.Name == "measure"); // end of measure
        }

        private BarlineType GetBarlineType(string value)
        {
            BarlineType rval = BarlineType.regular;
            switch(value)
            {
                case "regular":
                    rval = BarlineType.regular;
                    break;
                case "dotted":
                    rval = BarlineType.dotted;
                    break;
                case "dashed":
                    rval = BarlineType.dashed;
                    break;
                case "heavy":
                    rval = BarlineType.heavy;
                    break;
                case "light-light":
                    rval = BarlineType.lightLight;
                    break;
                case "light-heavy":
                    rval = BarlineType.lightHeavy;
                    break;
                case "heavy-light":
                    rval = BarlineType.heavyLight;
                    break;
                case "heavy-heavy":
                    rval = BarlineType.heavyHeavy;
                    break;
                case "tick":
                    rval = BarlineType.tick;
                    break;
                case "short":
                    rval = BarlineType._short;
                    break;
                case "none":
                    rval = BarlineType.none;
                    break;
                default:
                    A.ThrowError("Error: unknown barline type");
                    break;
            }
            return rval;
        }
    }
}
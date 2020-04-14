using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Measure : IWritable , ITicks
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
        public readonly MNXBarlineType? BarlineType = null; // default

        public readonly Directions Directions = null;
        public readonly List<Sequence> Sequences = new List<Sequence>();

        public int Ticks
        {
            get
            {
                int ticks = Sequences[0].Ticks;
                for(var i = 1; i < Sequences.Count; i++)
                {
                    G.Assert(Sequences[i].Ticks == ticks);
                }
                return ticks;
            }
        }

        public Measure(XmlReader r, bool isGlobal)
        {
            G.Assert(r.Name == "measure");
            // https://w3c.github.io/mnx/specification/common/#the-measure-element

            if(r.IsEmptyElement)
            {
                if(isGlobal)
                {
                    return;
                }
                else
                {
                    G.ThrowError("Empty measure in part.");
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
                        G.Assert(Index > 0);
                        break;
                    case "number":
                        Number = Int32.Parse(r.Value);
                        G.Assert(Number > 0);
                        break;
                    case "barline":
                        BarlineType = GetBarlineType(r.Value);
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            G.ReadToXmlElementTag(r, "directions", "sequence");

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
                                G.ThrowError("Error in input file.");
                            }
                            Sequences.Add(new Sequence(r, isGlobal));
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "directions", "sequence", "measure");
            }
            G.Assert(r.Name == "measure"); // end of measure
        }

        private MNXBarlineType GetBarlineType(string value)
        {
            MNXBarlineType rval = MNXBarlineType.regular;
            switch(value)
            {
                case "regular":
                    rval = MNXBarlineType.regular;
                    break;
                case "dotted":
                    rval = MNXBarlineType.dotted;
                    break;
                case "dashed":
                    rval = MNXBarlineType.dashed;
                    break;
                case "heavy":
                    rval = MNXBarlineType.heavy;
                    break;
                case "light-light":
                    rval = MNXBarlineType.lightLight;
                    break;
                case "light-heavy":
                    rval = MNXBarlineType.lightHeavy;
                    break;
                case "heavy-light":
                    rval = MNXBarlineType.heavyLight;
                    break;
                case "heavy-heavy":
                    rval = MNXBarlineType.heavyHeavy;
                    break;
                case "tick":
                    rval = MNXBarlineType.tick;
                    break;
                case "short":
                    rval = MNXBarlineType._short;
                    break;
                case "none":
                    rval = MNXBarlineType.none;
                    break;
                default:
                    G.ThrowError("Error: unknown barline type");
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
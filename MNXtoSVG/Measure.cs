using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Measure : IWritable
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
        public readonly G.MNXBarlineType BarlineType = G.MNXBarlineType.undefined; // default

        public readonly Directions GlobalDirections = null;
        public readonly Directions PartDirections = null;
        public readonly List<Sequence> Sequences = new List<Sequence>();

        public Measure(XmlReader r, string parentElement)
        {
            G.Assert(r.Name == "measure");
            // https://w3c.github.io/mnx/specification/common/#the-measure-element

            if(r.IsEmptyElement)
            {
                if(parentElement == "global")
                {
                    return;
                }
                else
                {
                    G.ThrowError("Empty measure in sequence.");
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
                        {
                            switch(r.Value)
                            {
                                // default is G.MNXBarlineType.undefined (see below)
                                case "regular":
                                    this.BarlineType = G.MNXBarlineType.regular;
                                    break;
                                case "dotted":
                                    this.BarlineType = G.MNXBarlineType.dotted;
                                    break;
                                case "dashed":
                                    this.BarlineType = G.MNXBarlineType.dashed;
                                    break;
                                case "heavy":
                                    this.BarlineType = G.MNXBarlineType.heavy;
                                    break;
                                case "light-light":
                                    this.BarlineType = G.MNXBarlineType.lightLight;
                                    break;
                                case "light-heavy":
                                    this.BarlineType = G.MNXBarlineType.lightHeavy;
                                    break;
                                case "heavy-light":
                                    this.BarlineType = G.MNXBarlineType.heavyLight;
                                    break;
                                case "heavy-heavy":
                                    this.BarlineType = G.MNXBarlineType.heavyHeavy;
                                    break;
                                case "tick":
                                    this.BarlineType = G.MNXBarlineType.tick;
                                    break;
                                case "short":
                                    this.BarlineType = G.MNXBarlineType._short;
                                    break;
                                case "none":
                                    this.BarlineType = G.MNXBarlineType.none;
                                    break;
                            }
                        }
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
                            if(parentElement == "global")
                            {
                                GlobalDirections = new Directions(r);
                            }
                            else if(parentElement == "part")
                            {
                                PartDirections = new Directions(r);
                            }
                            break;
                        case "sequence":
                            if(parentElement == "global")
                            {
                                G.ThrowError("Error in input file.");
                            }
                            Sequences.Add(new Sequence(r));
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "directions", "sequence", "measure");
            }
            G.Assert(r.Name == "measure"); // end of measure
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
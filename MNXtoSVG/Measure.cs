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

        public readonly Directions Directions = null;
        public readonly List<Sequence> Sequences = new List<Sequence>();

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
                            Sequences.Add(new Sequence(r, "sequence", isGlobal));
                            break;
                    }
                }
                G.ReadToXmlElementTag(r, "directions", "sequence", "measure");
            }
            G.Assert(r.Name == "measure"); // end of measure
        }

        private G.MNXBarlineType GetBarlineType(string value)
        {
            G.MNXBarlineType rval = G.MNXBarlineType.undefined;
            switch(value)
            {
                // default is G.MNXBarlineType.undefined (see below)
                case "regular":
                    rval = G.MNXBarlineType.regular;
                    break;
                case "dotted":
                    rval = G.MNXBarlineType.dotted;
                    break;
                case "dashed":
                    rval = G.MNXBarlineType.dashed;
                    break;
                case "heavy":
                    rval = G.MNXBarlineType.heavy;
                    break;
                case "light-light":
                    rval = G.MNXBarlineType.lightLight;
                    break;
                case "light-heavy":
                    rval = G.MNXBarlineType.lightHeavy;
                    break;
                case "heavy-light":
                    rval = G.MNXBarlineType.heavyLight;
                    break;
                case "heavy-heavy":
                    rval = G.MNXBarlineType.heavyHeavy;
                    break;
                case "tick":
                    rval = G.MNXBarlineType.tick;
                    break;
                case "short":
                    rval = G.MNXBarlineType._short;
                    break;
                case "none":
                    rval = G.MNXBarlineType.none;
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
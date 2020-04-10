using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Note : IWritable
    {
        // Compulsory Attribute
        public readonly string Pitch = null; // the musical pitch of this note
        // Optional Attributes
        public readonly string ID = null; // the ID of this note
        public readonly int? Staff = null; // an optional staff index for this note
        public readonly G.MNXCommonAccidental? Accidental = null; // an optional accidental for this note
        public readonly string Value = null; // an optional note value for this note

        // Optional Content
        public readonly Tied Tied = null;
        // Content (not defined in spec yet)
        // 6.5.1. The notehead element
        // 6.5.2. The fret element
        // 6.5.3. The string element

        public Note(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#elementdef-note
            G.Assert(r.Name == "note");

            bool noteElementIsEmpty = r.IsEmptyElement; // used below

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "id":
                        ID = r.Value;
                        break;
                    case "pitch":
                        Pitch = r.Value;
                        break;
                    case "staff":
                        int s;
                        int.TryParse(r.Value, out s);
                        if(s > 0)
                        {
                            Staff = s;
                        }
                        break;
                    case "accidental":
                        Accidental = GetAccidental(r.Value);
                        break;
                    case "value":
                        Value = r.Value;
                        break;
                }
            }

            if(noteElementIsEmpty == false)
            {
                G.ReadToXmlElementTag(r, "tied", "notehead", "fret", "string");

                while(r.Name == "tied" || r.Name == "notehead" || r.Name == "fret" || r.Name == "string")
                {
                    if(r.NodeType != XmlNodeType.EndElement)
                    {
                        switch(r.Name)
                        {
                            case "tied":
                                Tied = new Tied(r);
                                break;
                            case "notehead":
                                G.ThrowError("Error: Not implemented yet.");
                                break;
                            case "fret":
                                G.ThrowError("Error: Not implemented yet.");
                                break;
                            case "string":
                                G.ThrowError("Error: Not implemented yet.");
                                break;
                        }
                    }
                    G.ReadToXmlElementTag(r, "tied", "notehead", "fret", "string", "note");
                }
                G.Assert(r.Name == "note"); // end of note
            }
            //else
            //{
            //    // r is still pointing at the last attribute
            //    G.Assert(r.Name == "id" || r.Name == "pitch" || r.Name == "staff" || r.Name == "accidental" || r.Name == "value");
            //}
        }

        private G.MNXCommonAccidental? GetAccidental(string value)
        {
            G.MNXCommonAccidental? rval = null;

            switch(value)
            {
                case "auto":
                    rval = G.MNXCommonAccidental.auto;
                    break;
                case "sharp":
                    rval = G.MNXCommonAccidental.sharp;
                    break;
                case "natural":
                    rval = G.MNXCommonAccidental.natural;
                    break;
                case "flat":
                    rval = G.MNXCommonAccidental.flat;
                    break;
                case "double-sharp":
                    rval = G.MNXCommonAccidental.doubleSharp;
                    break;
                case "sharp-sharp":
                    rval = G.MNXCommonAccidental.sharpSharp;
                    break;
                case "flat-flat":
                    rval = G.MNXCommonAccidental.flatFlat;
                    break;
                case "natural-sharp":
                    rval = G.MNXCommonAccidental.naturalSharp;
                    break;
                case "natural-flat":
                    rval = G.MNXCommonAccidental.naturalFlat;
                    break;
                case "quarter-flat":
                    rval = G.MNXCommonAccidental.quarterFlat;
                    break;
                case "quarter-sharp":
                    rval = G.MNXCommonAccidental.quarterSharp;
                    break;
                case "three-quarters-flat":
                    rval = G.MNXCommonAccidental.threeQuartersFlat;
                    break;
                case "three-quarters-sharp":
                    rval = G.MNXCommonAccidental.threeQuartersSharp;
                    break;
                case "sharp-down":
                    rval = G.MNXCommonAccidental.sharpDown;
                    break;
                case "sharp-up":
                    rval = G.MNXCommonAccidental.sharpUp;
                    break;
                case "natural-down":
                    rval = G.MNXCommonAccidental.naturalDown;
                    break;
                case "natural-up":
                    rval = G.MNXCommonAccidental.naturalUp;
                    break;
                case "flat-down":
                    rval = G.MNXCommonAccidental.flatDown;
                    break;
                case "flat-up":
                    rval = G.MNXCommonAccidental.flatUp;
                    break;
                case "triple-sharp":
                    rval = G.MNXCommonAccidental.tripleSharp;
                    break;
                case "triple-flat":
                    rval = G.MNXCommonAccidental.tripleFlat;
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
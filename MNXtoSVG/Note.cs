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
        public readonly MNXCommonAccidental? Accidental = null; // an optional accidental for this note
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

        private MNXCommonAccidental? GetAccidental(string value)
        {
            MNXCommonAccidental? rval = null;

            switch(value)
            {
                case "auto":
                    rval = MNXCommonAccidental.auto;
                    break;
                case "sharp":
                    rval = MNXCommonAccidental.sharp;
                    break;
                case "natural":
                    rval = MNXCommonAccidental.natural;
                    break;
                case "flat":
                    rval = MNXCommonAccidental.flat;
                    break;
                case "double-sharp":
                    rval = MNXCommonAccidental.doubleSharp;
                    break;
                case "sharp-sharp":
                    rval = MNXCommonAccidental.sharpSharp;
                    break;
                case "flat-flat":
                    rval = MNXCommonAccidental.flatFlat;
                    break;
                case "natural-sharp":
                    rval = MNXCommonAccidental.naturalSharp;
                    break;
                case "natural-flat":
                    rval = MNXCommonAccidental.naturalFlat;
                    break;
                case "quarter-flat":
                    rval = MNXCommonAccidental.quarterFlat;
                    break;
                case "quarter-sharp":
                    rval = MNXCommonAccidental.quarterSharp;
                    break;
                case "three-quarters-flat":
                    rval = MNXCommonAccidental.threeQuartersFlat;
                    break;
                case "three-quarters-sharp":
                    rval = MNXCommonAccidental.threeQuartersSharp;
                    break;
                case "sharp-down":
                    rval = MNXCommonAccidental.sharpDown;
                    break;
                case "sharp-up":
                    rval = MNXCommonAccidental.sharpUp;
                    break;
                case "natural-down":
                    rval = MNXCommonAccidental.naturalDown;
                    break;
                case "natural-up":
                    rval = MNXCommonAccidental.naturalUp;
                    break;
                case "flat-down":
                    rval = MNXCommonAccidental.flatDown;
                    break;
                case "flat-up":
                    rval = MNXCommonAccidental.flatUp;
                    break;
                case "triple-sharp":
                    rval = MNXCommonAccidental.tripleSharp;
                    break;
                case "triple-flat":
                    rval = MNXCommonAccidental.tripleFlat;
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
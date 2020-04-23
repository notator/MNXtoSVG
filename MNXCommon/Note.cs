using MNX.Globals;
using System.Xml;

namespace MNX.Common
{
    internal class Note : IEventComponent
    {
        // Compulsory Attribute
        public readonly string Pitch = null; // the musical pitch of this note
        // Optional Attributes
        public readonly string ID = null; // the ID of this note

        // optional staff index of this note (also Tuplet, Rest, Event)
        // (1-based) staff index of this tuplet. The spec says that the default is app-specific,
        // and that "The topmost staff in a part has a staff index of 1; staves below the topmost staff
        // are identified with successively increasing indices."
        public readonly int Staff = 1; // app-specific default
        public readonly Accidental Accidental = null; // an optional accidental for this note
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
            M.Assert(r.Name == "note");

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
                        Accidental = new Accidental(r.Value);
                        break;
                    case "value":
                        Value = r.Value;
                        break;
                }
            }

            if(noteElementIsEmpty == false)
            {
                M.ReadToXmlElementTag(r, "tied", "notehead", "fret", "string");

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
                                M.ThrowError("Error: Not implemented yet.");
                                break;
                            case "fret":
                                M.ThrowError("Error: Not implemented yet.");
                                break;
                            case "string":
                                M.ThrowError("Error: Not implemented yet.");
                                break;
                        }
                    }
                    M.ReadToXmlElementTag(r, "tied", "notehead", "fret", "string", "note");
                }
                M.Assert(r.Name == "note"); // end of note
            }
            //else
            //{
            //    // r is still pointing at the last attribute
            //    A.Assert(r.Name == "id" || r.Name == "pitch" || r.Name == "staff" || r.Name == "accidental" || r.Name == "value");
            //}
        }
    }
}
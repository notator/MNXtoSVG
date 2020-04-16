using MNX.AGlobals;
using System;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-rest
    // Rest is a *field* in Event.
    public class Rest
    {
        public readonly string Pitch;

        public Rest(XmlReader r)
        {
            A.Assert(r.Name == "rest");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "pitch":
                        Pitch = r.Value;
                        break;
                }
            }
            A.ReadToXmlElementTag(r, "rest");

            A.Assert(r.Name == "rest"); // end of rest
        }
    }
}
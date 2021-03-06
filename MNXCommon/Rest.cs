﻿using MNX.Globals;
using System;
using System.Xml;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#elementdef-rest
    // Rest is a *field* in Event.
    public class Rest
    {
        public readonly string Pitch = null;

        public Rest(XmlReader r)
        {
            M.Assert(r.Name == "rest");

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
            
            // r now points either to the last attribute read
            // or to the empty rest.
        }
    }
}
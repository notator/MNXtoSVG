﻿
using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal class Beamed : EventGroup, ISeqComponent
    {
        public readonly MNXDurationSymbol Duration = null;
        public readonly string Continue = null;
        public readonly string ID = null;

        public Beamed(XmlReader r)
        {
            M.Assert(r.Name == "beamed");
            // https://w3c.github.io/mnx/specification/common/#the-beamed-element

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "value":
                        Duration = new MNXDurationSymbol(r.Value, C.CurrentTupletLevel);
                        break;
                    case "continue":
                        Continue = r.Value;
                        break;
                    case "id":
                        ID = r.Value;
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            SequenceComponents = GetSequenceComponents(r, "beamed", false);

            M.Assert(r.Name == "beamed"); // end of (nested) beamed
        }
    }
}

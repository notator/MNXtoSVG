using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class SpanAttributes
    {
        public string End { get; private set; }

        public SpanAttributes()
        {
            End = null;
        }

        internal bool SetAttribute(XmlReader r)
        {
            if(r.Name == "end")
            {
                End = r.Value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
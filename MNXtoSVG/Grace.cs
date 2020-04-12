﻿using System;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Grace : IWritable
    {
        public readonly G.MNXCGraceType Type = G.MNXCGraceType.stealPrevious; // spec says this is the default.
        public readonly bool? Slash = null;

        public readonly List<IWritable> Seq;

        public Grace(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-grace-element
            G.Assert(r.Name == "grace");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "type":
                        Type = GetType(r.Value);
                        break;
                    case "slash":
                        Slash = (r.Value == "yes");
                        break;
                    default:
                        throw new ApplicationException("Unknown attribute");
                }
            }

            Seq = G.GetSequenceContent(r, "grace", false);

            G.Assert(r.Name == "grace"); // end of grace

        }

        private G.MNXCGraceType GetType(string value)
        {
            G.MNXCGraceType rval = G.MNXCGraceType.stealPrevious; // spec says this is the default.
            switch(value)
            {
                case "steal-following":
                    rval = G.MNXCGraceType.stealFollowing;
                    break;
                case "make-time":
                    rval = G.MNXCGraceType.makeTime;
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
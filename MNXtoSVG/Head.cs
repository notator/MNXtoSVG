﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    internal class Head : IWritable
    {        
        public Head(XmlReader r)
        {
            G.Assert(r.Name == "head");
            // https://w3c.github.io/mnx/specification/common/#the-score-element

            throw new NotImplementedException();

        }

        public void WriteSVG(XmlWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
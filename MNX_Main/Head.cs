﻿using MNX.AGlobals;
using System;
using System.Xml;

namespace MNX_Main
{
    internal class Head
    {        
        public Head(XmlReader r)
        {
            A.Assert(r.Name == "head");
            // https://w3c.github.io/mnx/specification/common/#the-score-element

            throw new NotImplementedException();
        }
    }
}
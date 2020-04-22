using MNX.AGlobals;
using MNX.Common;
using System;
using System.Xml;

namespace MNX_Main
{
    internal class Score
    {
        public readonly string SourceFilePath = null;
        public readonly MNXCommon MNXCommon;

        public Score(XmlReader r)
        {
            A.Assert(r.Name == "score");
            // https://w3c.github.io/mnx/specification/common/#the-score-element
            // can have a "src" attribute
            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "src":
                        SourceFilePath = r.Value;
                        break;
                }
            }

            // Other score types need to be added and constructed here.
            A.ReadToXmlElementTag(r, "mnx-common");

            while(r.Name == "mnx-common")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "mnx-common":
                            MNXCommon = new MNXCommon(r);
                            break;
                    }
                }
                A.ReadToXmlElementTag(r, "mnx-common", "score");
            }
            A.Assert(r.Name == "score"); // end of event        
        }
    }
}
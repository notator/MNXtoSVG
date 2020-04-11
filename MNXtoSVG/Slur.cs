using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public class Slur : IWritable
    {
        public readonly string Target = null; // an ID
        public readonly MNXC_PositionInMeasure Location = null;
        public readonly string StartNote = null; // an ID
        public readonly string EndNote = null; // an ID
        public readonly G.MNXLineType LineType = G.MNXLineType.solid;
        public readonly G.MNXOrientation Side = G.MNXOrientation.undefined;
        public readonly G.MNXOrientation SideEnd = G.MNXOrientation.undefined;

        public Slur(XmlReader r)
        {
            // https://w3c.github.io/mnx/specification/common/#the-slur-element
            G.Assert(r.Name == "slur");

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "target":
                        Target = r.Value;
                        break;
                    case "location":
                        Location = new MNXC_PositionInMeasure(r.Value);
                        break;
                    case "start-note":
                        StartNote = r.Value;
                        break;
                    case "end-note":
                        EndNote = r.Value;
                        break;
                    case "line-type":
                        LineType = GetLineType(r.Value);
                        break;
                    case "side":
                        if(r.Value == "up")
                            Side = G.MNXOrientation.up;
                        else if(r.Value == "down")
                            Side = G.MNXOrientation.down;
                        break;
                    case "side-end":
                        if(r.Value == "up")
                            SideEnd = G.MNXOrientation.up;
                        else if(r.Value == "down")
                            SideEnd = G.MNXOrientation.down;
                        break;
                }
            }
        }

        private G.MNXLineType GetLineType(string value)
        {
            G.MNXLineType rval = G.MNXLineType.solid; // default
            switch(value)
            {
                case "solid":
                    rval = G.MNXLineType.solid;
                    break;
                case "dashed":
                    rval = G.MNXLineType.dashed;
                    break;
                case "dotted":
                    rval = G.MNXLineType.dotted;
                    break;
            }
            return rval;
        }

        public void WriteSVG(XmlWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
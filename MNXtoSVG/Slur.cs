using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using MNXtoSVG.Globals;

namespace MNXtoSVG
{
    public enum MNXLineType
    {
        solid, // always default
        dashed,
        dotted
    }

    public class Slur : IWritable
    {
        public readonly string Target = null; // an ID
        public readonly MNXC_PositionInMeasure Location = null;
        public readonly string StartNote = null; // an ID
        public readonly string EndNote = null; // an ID
        public readonly MNXLineType LineType = MNXLineType.solid;
        public readonly MNXOrientation Side = MNXOrientation.undefined;
        public readonly MNXOrientation SideEnd = MNXOrientation.undefined;

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
                            Side = MNXOrientation.up;
                        else if(r.Value == "down")
                            Side = MNXOrientation.down;
                        break;
                    case "side-end":
                        if(r.Value == "up")
                            SideEnd = MNXOrientation.up;
                        else if(r.Value == "down")
                            SideEnd = MNXOrientation.down;
                        break;
                }
            }
        }

        private MNXLineType GetLineType(string value)
        {
            MNXLineType rval = MNXLineType.solid; // default
            switch(value)
            {
                case "solid":
                    rval = MNXLineType.solid;
                    break;
                case "dashed":
                    rval = MNXLineType.dashed;
                    break;
                case "dotted":
                    rval = MNXLineType.dotted;
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
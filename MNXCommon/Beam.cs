using MNX.Globals;

using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public class Beam
    {
        public List<string> EventIDs = new List<string>();
        public List<Beam> Beams = new List<Beam>();
        public List<BeamHook> BeamHooks = new List<BeamHook>();

        public readonly int TicksPosInScore;

        #region Runtime property
        public readonly int BeamLevel;
        #endregion Runtime property

        public Beam(XmlReader r, int ticksPosInScore)
        {
            BeamLevel = C.CurrentBeamLevel; // top level beam has beam level 0
            M.Assert(r.Name == "beam");

            C.CurrentBeamLevel++;

            TicksPosInScore = ticksPosInScore;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "events":
                        string[] events = r.Value.Split(' ', (char)System.StringSplitOptions.RemoveEmptyEntries);
                        EventIDs = new List<string>(events);
                        break;
                }
            }

            // extend the contained elements as necessary..
            M.ReadToXmlElementTag(r, "beam", "beam-hook");

            while(r.Name == "beam" || r.Name == "beam-hook")
            {
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "beam":
                            Beams.Add(new Beam(r, ticksPosInScore));
                            break;
                        case "beam-hook":
                            BeamHooks.Add(new BeamHook(r, ticksPosInScore));
                            break;
                    }
                }
                M.ReadToXmlElementTag(r, "beam", "beam-hook");
            }

            M.Assert(r.Name == "beam"); // end of (nested) beam content

            //if(C.CurrentBeamLevel == 1)
            //{
            //    int outerTicks = this.OuterDuration.GetDefaultTicks();
            //    this.OuterDuration.Ticks = outerTicks;
            //    SetTicksInContent(outerTicks, this.TupletLevel + 1);
            //}

            C.CurrentBeamLevel--;
        }
    }
}
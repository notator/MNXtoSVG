using MNX.Globals;

using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MNX.Common
{
    public class Beam : IBeamBlockComponent
    {
        public List<string> EventIDs = new List<string>();
        public int Depth { get; }

        public override string ToString()
        {
            StringBuilder idsSB = new StringBuilder();
            foreach(string id in EventIDs)
            {
                idsSB.Append(id);
                idsSB.Append(" ");
            }
            idsSB.Length--;

            return $"Depth={Depth} EventIDs={idsSB}";
        }

        public Beam(List<string> eventIDs, int depth)
        {
            foreach(var id in eventIDs)
            {
                EventIDs.Add(id);
            }
            
            Depth = depth;
        }

        public Beam(XmlReader r, int topLevelDepth)
        {
            M.Assert(r.Name == "beam");

            Depth = r.Depth - topLevelDepth;

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

            r.MoveToElement();
            M.Assert(r.Name == "beam");
        }
    }
}
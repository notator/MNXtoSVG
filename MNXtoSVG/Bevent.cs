using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MNXtoSVG
{
    public abstract class Bevent
    {
    }

    public class Event : Bevent
    {
        public Event(XmlReader r)
        {

        }
    }

    public class Beamed : Bevent
    {
        public Beamed(XmlReader r)
        {

        }
    }
}

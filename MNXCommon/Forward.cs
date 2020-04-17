using MNX.AGlobals;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    /// <summary>
    /// https://w3c.github.io/mnx/specification/common/#the-event-element
    /// </summary>
    internal class Forward : IHasTicks, ISeqComponent
    {
        #region MNX file attributes
        // Compulsory Attribute         
        //   the notated metrical duration of this event  ( /2, /4, /8 etc)
        public readonly DurationSymbol DSymbol = null;
        #endregion  MNX file attributes

        #region runtime properties
        // TupletLevel is used when setting Duration.Ticks. It has the following meaning:
        //    0: The Forward is not contained in a Tuplet.
        //    1: The Forward is contained in a Tuplet.
        //    2: The Forward is contained in a Tuplet nested in a Tuplet.
        //    etc. (This app copes with arbitrarily nested tuplets.)
        public readonly int TupletLevel;
        public int Ticks { get { return DSymbol.DefaultTicks; } }

        #endregion runtime properties

        public Forward(XmlReader r)
        {
            TupletLevel = B.CurrentTupletLevel;

            A.Assert(r.Name == "forward");

            r.MoveToAttribute("duration");
            DSymbol = new DurationSymbol(r.Value, B.CurrentTupletLevel);

            A.ReadToXmlElementTag(r, "forward");

            A.Assert(r.Name == "forward"); // end of forward

        }
    }
}

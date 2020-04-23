using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal static class B
    {
        /// <summary>
        /// The minimum number of Ticks in an ITicks object.
        /// </summary>
        internal static readonly int MinimumEventTicks = 4;
        /// <summary>
        /// Used by the parser. The value 0 means that there are no tuplets currently active.
        /// This value is incremented at the beginning of a Tuplet constructor, and decremented when it ends. 
        /// </summary>
        internal static int CurrentTupletLevel = 0;

        internal readonly static int[] DurationSymbolTicks =
        {
            8192, // noteDoubleWhole_breve
            4096, // noteWhole_semibreve
            2048, // noteHalf_minim
            1024, // noteQuarter_crotchet
            512,  // note8th_1flag_quaver
            256,  // note16th_2flags_semiquaver
            128,  // note32nd_3flags_demisemiquaver
            64,   // note64th_4flags
            32,   // note128th_5flags
            16,   // note256th_6flags
            8,    // note512th_7flags
            4     // note1024th_8flags
        };
    }
}

using MNX.Globals;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    internal static class C
    {
        /// <summary>
        /// Used while parsing.
        /// The value 0 means that there are no tuplets currently active.
        /// This value is incremented at the beginning of a Tuplet constructor,
        /// and decremented when it ends. 
        /// </summary>
        internal static int CurrentTupletLevel = 0;

        /// <summary>
        /// Used while parsing.
        /// The value 0 means that there are no beams currently active.
        /// This value is incremented at the beginning of a Beam constructor,
        /// and decremented when it ends. 
        /// </summary>
        internal static int CurrentBeamLevel = 0;
    }
}

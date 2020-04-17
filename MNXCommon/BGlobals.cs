using MNX.AGlobals;
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

        /// <summary>
        /// This code is the same as in Moritz.Globals.IntDivisionSizes(total, relativeSizes).
        /// The function divides total into relativeSizes.Count parts, returning a List whose:
        ///     * Count is relativeSizes.Count.
        ///     * sum is exactly equal to total
        ///     * members have the relative sizes (as nearly as possible) to the values in the relativeSizes argument. 
        /// </summary>
        internal static List<int> GetInnerTicks(int total, List<int> relativeSizes)
        {
            int divisor = relativeSizes.Count;
            int sumRelative = 0;
            for(int i = 0; i < divisor; ++i)
            {
                sumRelative += relativeSizes[i];
            }
            float factor = ((float)total / (float)sumRelative);
            float fPos = 0;
            List<int> intPositions = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                intPositions.Add((int)(Math.Floor(fPos)));
                fPos += (relativeSizes[i] * factor);
            }
            intPositions.Add((int)Math.Floor(fPos));

            List<int> intDivisionSizes = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                int intDuration = (int)(intPositions[i + 1] - intPositions[i]);
                intDivisionSizes.Add(intDuration);
            }

            int intSum = 0;
            foreach(int i in intDivisionSizes)
            {
                //A.Assert(i >= 0);
                if(i < 0)
                {
                    throw new ApplicationException();
                }
                intSum += i;
            }
            A.Assert(intSum <= total);
            if(intSum < total)
            {
                int lastDuration = intDivisionSizes[intDivisionSizes.Count - 1];
                lastDuration += (total - intSum);
                intDivisionSizes.RemoveAt(intDivisionSizes.Count - 1);
                intDivisionSizes.Add(lastDuration);
            }
            return intDivisionSizes;
        }

        /// <summary>
        /// If the ticksObject is not found, this function returns the current length of the sequence.
        /// </summary>
        /// <returns></returns>
        internal static int TickPositionInSequence(Sequence sequence, ITicks ticksObject)
        {
            int rval = 0;
            foreach(var seqObj in sequence.Seq)
            {
                if(seqObj is ITicks tObj)
                {
                    if(tObj == ticksObject)
                    {
                        break;
                    }
                    rval += tObj.Ticks;
                }
            }

            return rval;
        }
    }
}

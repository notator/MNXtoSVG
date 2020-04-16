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
        internal static readonly int MinimumEventTicks = 5;
        /// <summary>
        /// Used by the parser. The value 0 means that there are no tuplets currently active.
        /// This value is incremented at the beginning of a Tuplet constructor, and decremented when it ends. 
        /// </summary>
        internal static int CurrentTupletLevel = 0;

        /// <summary>
        /// This function is called after getting the class specific attributes
        /// The XmlReader is currently pointing to the last attribute read or to
        /// the beginning of the containing (sequence-like) element.
        /// See https://w3c.github.io/mnx/specification/common/#elementdef-sequence
        /// The spec says:
        /// "directions occurring within sequence content must omit this ("location") attribute as their
        /// location is determined during the procedure of sequencing the content."
        /// </summary>
        internal static List<ISeqComponent> GetSequenceContent(XmlReader r, string caller, bool isGlobal)
        {
            /// local function, called below.
            /// The spec says:
            /// "directions occurring within sequence content (i.e.when isGlobal is false) must omit
            /// this ("location") attribute as their location is determined during the procedure of
            /// sequencing the content."
            /// If found, write a message to the console, explaining that such data is ignored.
            void CheckDirectionContent(List<ISeqComponent> seq)
            {
                bool global = isGlobal; // isGlobal is from the outer scope                
            }

            List<ISeqComponent> content = new List<ISeqComponent>();

            // Read to the first element inside the caller element.
            // These are all the elements that can occur inside sequence-like elements. (Some of them nest.)
            A.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "sequence");

            while(r.Name == "directions" || r.Name == "event" || r.Name == "grace"
                || r.Name == "beamed" || r.Name == "tuplet" || r.Name == "sequence")
            {
                if(r.Name == caller && r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if(r.NodeType != XmlNodeType.EndElement)
                {
                    switch(r.Name)
                    {
                        case "directions":
                            content.Add(new Directions(r, isGlobal));
                            break;
                        case "event":
                            Event e = new Event(r);
                            content.Add(e);
                            break;
                        case "grace":
                            Grace g = new Grace(r);
                            content.Add(g);
                            break;
                        case "beamed":
                            content.Add(new Beamed(r));
                            break;
                        case "tuplet":
                            content.Add(new Tuplet(r));
                            break;
                        case "sequence":
                            content.Add(new Sequence(r, isGlobal));
                            break;
                    }
                }

                A.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "sequence");
            }

            CheckDirectionContent(content);

            A.Assert(r.Name == caller); // end of sequence content

            return content;
        }

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

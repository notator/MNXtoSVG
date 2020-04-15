using MNX.AGlobals;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public static class B
    {
        /// <summary>
        /// Used by the parser. The value 0 means that there are no tuplets currently active.
        /// This value is incremented at the beginning of a Tuplet constructor, and decremented when it ends. 
        /// </summary>
        public static int CurrentTupletLevel = 0;

        /// <summary>
        /// This function is called after getting the class specific attributes
        /// The XmlReader is currently pointing to the last attribute read or to
        /// the beginning of the containing (sequence-like) element.
        /// See https://w3c.github.io/mnx/specification/common/#elementdef-sequence
        /// The spec says:
        /// "directions occurring within sequence content must omit this ("location") attribute as their
        /// location is determined during the procedure of sequencing the content."
        /// </summary>
        public static List<ISequenceComponent> GetSequenceContent(XmlReader r, string caller, bool isGlobal)
        {
            /// local function, called below.
            /// The spec says:
            /// "directions occurring within sequence content (i.e.when isGlobal is false) must omit
            /// this ("location") attribute as their location is determined during the procedure of
            /// sequencing the content."
            /// If found, write a message to the console, explaining that such data is ignored.
            void CheckDirectionContent(List<ISequenceComponent> seq)
            {
                bool global = isGlobal; // isGlobal is from the outer scope                
            }

            List<ISequenceComponent> content = new List<ISequenceComponent>();

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
                            content.Add(new Event(r));
                            break;
                        case "grace":
                            content.Add(new Grace(r));
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

        public readonly static int[] DurationSymbolTicks =
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

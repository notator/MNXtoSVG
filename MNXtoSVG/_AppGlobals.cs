using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MNXtoSVG.Globals
{
    public static class G
    {
        public static MNXProfileEnum MNXProfile = MNXProfileEnum.undefined;

        public enum MNXCTupletNumberDisplay
        {
            inner, // ShowNumber default 
            both,
            none // ShowValue default
        }

        public enum MNXCTupletBracketDisplay
        {
            auto, // default 
            yes,
            no
        }

        public enum MNXCGraceType
        {
            stealPrevious,
            stealFollowing,
            makeTime
        }

        public enum MNXProfileEnum
        {
            undefined,
            MNXCommonStandard
        }

        public enum MNXOctaveShiftType
        {
            undefined,
            down1Oct, // 8va (notes are rendered down one octave)
            up1Oct,   // 8vb (notes are rendered up one octave)
            down2Oct, // 15ma(notes are rendered down two octaves)
            up2Oct,   // 15mb(notes are rendered up two octaves)
            down3Oct, // 22ma(notes are rendered down three octaves)
            up3Oct    // 22mb(notes are rendered up three octaves)
        }

        /// <summary>
        /// ji -- April 2020: Should three repeat barline types be defined as well?
        ///     repeat-begin,
        ///     repeat-end,
        ///     repeat-end-begin
        /// </summary>
        public enum MNXBarlineType
        {
            undefined,
            regular,
            dotted,
            dashed,
            heavy,
            lightLight,
            lightHeavy,
            heavyLight,
            heavyHeavy,
            tick,
            _short,
            none,
        }

        public enum MNXOrientation
        {
            undefined,
            up,
            down
        }

        public enum MNXLineType
        {
            solid, // always default
            dashed,
            dotted
        }

        public enum MNXClefSign
        {
            undefined, // ji
            G, // G (treble) clef
            F, // F(bass) clef
            C, // C clef
            percussion, // Percussion clef
            jianpu, // Jianpu clef ?? not mnx-common...
            tab, // not in MNX Spec... but in MusicXML spec: https://usermanuals.musicxml.com/MusicXML/Content/ST-MusicXML-clef-sign.htm
            none // The spec asks: Is the none value from MusicXML needed? Why?
        }

        /// <summary>
        /// Avaiable CWMN accidentals copied from MusicXML. (Not all the accidentals there are for CWMN.)
        /// See https://usermanuals.musicxml.com/MusicXML/Content/ST-MusicXML-accidental-value.htm
        /// </summary>
        public enum MNXCommonAccidental
        {
            auto, // from spec
            sharp,
            natural,
            flat,
            doubleSharp,
            sharpSharp,
            flatFlat,
            naturalSharp,
            naturalFlat,
            quarterFlat,
            quarterSharp,
            threeQuartersFlat,
            threeQuartersSharp,
            sharpDown,
            sharpUp,
            naturalDown,
            naturalUp,
            flatDown,
            flatUp,
            tripleSharp,
            tripleFlat
        }

        /// <summary>
        /// Adapted from CapXML Utilities.
        /// Reads to the next start or end tag having a name which is in the parameter list.
        /// When this function returns, XmlReader.Name is the name of the start or end tag found.
        /// If none of the names in the parameter list is found, an exception is thrown with a diagnostic message.
        /// </summary>
        /// <param name="r">XmlReader</param>
        /// <param name="possibleElements">Any number of strings, separated by commas</param>
        public static void ReadToXmlElementTag(XmlReader r, params string[] possibleElements)
        {
            List<string> elementNames = new List<string>(possibleElements);
            do
            {
                r.Read();
            } while(!elementNames.Contains(r.Name) && !r.EOF);

            if(r.EOF)
            {
                StringBuilder msg = new StringBuilder("Error reading Xml file:\n"
                    + "None of the following elements could be found:\n");
                foreach(string s in elementNames)
                    msg.Append(s.ToString() + "\n");

                MessageBox.Show(msg.ToString(), "Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                throw new ApplicationException(msg.ToString());
            }
        }

        public static void ThrowError(string errorDescription,
                    [CallerLineNumber] int lineNumber = 0,
                    [CallerMemberName] string caller = null,
                    [CallerFilePath] string path = null)
        {
            string infoStr = errorDescription + "\n" +
                Path.GetFileName(path) +
                "\nline number:" + lineNumber +
                "\n(method: " + caller + ")";

            throw new ApplicationException(infoStr);
        }

        /// <summary>
        /// Throws an ApplicationException if condition is false,
        /// and displays a custom message.
        /// </summary>
        /// <param name="v"></param>
        public static void Assert(bool condition, string message)
        {
            if(condition == false)
            {
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Throws an ApplicationException if condition is false
        /// </summary>
        /// <param name="v"></param>
        public static void Assert(bool condition)
        {
            if(condition == false)
            {
                throw new ApplicationException($"Condition failed.");
            }
        }

        /// <summary>
        /// This function is called after getting the class specific attributes
        /// The XmlReader is pointing to the last attribute read or to the beginning
        /// of the containing (sequence-like) element.
        /// </summary>
        public static List<IWritable> GetSequenceContent(XmlReader r, string caller, bool isGlobal)
        {
            List<IWritable> content = new List<IWritable>();
            // https://w3c.github.io/mnx/specification/common/#elementdef-sequence

            G.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "sequence");

            while(r.Name == "directions" || r.Name == "event" || r.Name == "grace" || r.Name == "beamed" || r.Name == "tuplet" || r.Name == "sequence")
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

                G.ReadToXmlElementTag(r, "directions", "event", "grace", "beamed", "tuplet", "sequence");
            }

            //CheckDirections(Seq, isGlobal);

            G.Assert(r.Name == caller); // end of sequence content

            return content;
        }

        //private void CheckDirections(List<IWritable> seq, bool isGlobal)
        //{
        //    // check the constraints on the contained directions here!
        //    // maybe silently correct any errors.
        //}


    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace MNX.Globals 
{
    /// <summary>
    /// MNX Globals
    /// </summary>
    public static class M
    {
        #region MNX application constants
        private static string GetMNX_MainFolder()
        {
            string directory = Directory.GetCurrentDirectory();

            string directoryName = Path.GetFileName(directory);
            while(directoryName != "MNX_Main")
            {
                var startIndex = directory.IndexOf(directoryName) - 1;
                directory = directory.Remove(startIndex);
                directoryName = Path.GetFileName(directory);
            }

            return directory;
        }
        private static readonly string mnxMain_Folder = GetMNX_MainFolder();

        public static readonly string OptionsForWriteAll_Path = mnxMain_Folder + @"\OptionsForWriteAll.f1d";
        // contains all MNX input files (not just mnx-common)
        public static readonly string MNX_in_Folder = mnxMain_Folder + @"\MNX_in\mnx\";
        // contains page formatting data parallel to the files in the MNX_in_Folder.
        public static readonly string Form1Data_Folder = mnxMain_Folder + @"\MNX_in\form1Data\";
        // contains the output SVG.
        public static readonly string SVG_out_Folder = mnxMain_Folder + @"\SVG_out\";

        //Creates and initializes the CultureInfo which uses the international sort.
        public static CultureInfo ci = new CultureInfo("en-US", false);
        public static NumberFormatInfo En_USNumberFormat = ci.NumberFormat;

        #region ticks

        /// <summary>
        /// The minimum number of Ticks in an ITicks object.
        /// </summary>
        public static readonly int MinimumEventTicks = 4;

        // This value could be raised by any power of 2.
        public static int TicksPerCrotchet = 1024;
        // this value is used while creating an SVG file.
        public static double MillisecondsPerTick = 0;

        public readonly static int[] DurationSymbolTicks = GetDurationSymbolTicks();

        private static int[] GetDurationSymbolTicks()
        {
            int[] tickss = new int[12];
            tickss[0] = M.TicksPerCrotchet * 8;  // noteDoubleWhole_breve
            tickss[1] = M.TicksPerCrotchet * 4;  // noteWhole_semibreve
            tickss[2] = M.TicksPerCrotchet * 2;  // noteHalf_minim
            tickss[3] = M.TicksPerCrotchet;      // noteQuarter_crotchet
            tickss[4] = M.TicksPerCrotchet / 2;  // note8th_1flag_quaver
            tickss[5] = M.TicksPerCrotchet / 4;  // note16th_2flags_semiquaver
            tickss[6] = M.TicksPerCrotchet / 8;  // note32nd_3flags_demisemiquaver
            tickss[7] = M.TicksPerCrotchet / 16; // note64th_4flags
            tickss[8] = M.TicksPerCrotchet / 32; // note128th_5flags
            tickss[9] = M.TicksPerCrotchet / 64; // note256th_6flags
            tickss[10] = M.TicksPerCrotchet / 128; // note512th_7flags
            tickss[11] = M.TicksPerCrotchet / 256; // note1024th_8flags

            if(tickss[11] < MinimumEventTicks)
            {
                throw new ApplicationException("TicksPerCrotchet was too small.");
            }

            return tickss;
        }

        #endregion ticks

        #endregion MNX application constants

        // Set for the score currently being constructed.
        public static MNXProfile? Profile = null;

        #region Form1

        /// <summary>
        /// The current date. (Written to XML files.)
        /// Cloned from Moritz
        /// </summary>
        public static string NowString
        {
            get
            {
                CultureInfo ci = new CultureInfo("en-US", false);
                return DateTime.Today.ToString("dddd dd.MM.yyyy", ci.DateTimeFormat) + ", " + DateTime.Now.ToLongTimeString();
            }
        }

        public static void SetTextBoxErrorColorIfNotOkay(TextBox textBox, bool okay)
        {
            if(okay)
            {
                textBox.BackColor = Color.White;
            }
            else
            {
                textBox.BackColor = M.TextBoxErrorColor;
            }
        }

        public static Color TextBoxErrorColor = Color.FromArgb(255, 220, 220);
        //public static Color GreenButtonColor = Color.FromArgb(215, 225, 215);
        //public static Color LightGreenButtonColor = Color.FromArgb(205, 240, 205);

        public static bool HasError(TextBox textBox)
        {
            return textBox.BackColor == M.TextBoxErrorColor;
        }

        public static void CheckTextBoxIsUInt(TextBox textBox)
        {
            bool okay = true;
            textBox.Text.Trim();
            try
            {
                uint i = uint.Parse(textBox.Text);
            }
            catch
            {
                okay = false;
            }

            SetTextBoxErrorColorIfNotOkay(textBox, okay);
        }

        public static void CheckTextBoxIsUnsignedDouble(TextBox textBox)
        {
            double d = 0;
            bool okay = true;
            textBox.Text.Trim();
            try
            {
                d = double.Parse(textBox.Text);
            }
            catch
            {
                okay = false;
            }

            if(d <= 0)
            {
                okay = false;
            }

            SetTextBoxErrorColorIfNotOkay(textBox, okay);
        }

        public static void CheckSystemStartBarsUnsignedIntList(TextBox textBox)
        {
            List<int> checkedUnsignedInts = null;
            bool okay = false;
            if(textBox.Text.Length > 0)
            {
                checkedUnsignedInts = GetCheckedUnsignedInts(textBox, int.MaxValue, 0, int.MaxValue);
                if(checkedUnsignedInts != null)
                {
                    if(CheckStartBarConditions(checkedUnsignedInts))
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach(int integer in checkedUnsignedInts)
                        {
                            string intString = integer.ToString();
                            sb.Append(",  " + intString);
                        }
                        sb.Remove(0, 3);
                        textBox.Text = sb.ToString();
                        okay = true;
                    }
                }
            }

            SetTextBoxErrorColorIfNotOkay(textBox, okay);
        }

        private static bool CheckStartBarConditions(List<int> unsignedInts)
        {
            bool rval = true;
            if(unsignedInts[0] != 1)
            {
                rval = false;
            }
            else
            {
                for(var i = 1; i < unsignedInts.Count; i++)
                {
                    if(unsignedInts[i - 1] >= unsignedInts[i])
                    {
                        rval = false;
                        break;
                    }
                }
            }
            return rval;
        }

        /// <summary>
        /// Returns null if
        ///     textBox.Text is empty, or
        ///     textBox.Text contains anything other than numbers, commas and whitespace or
        ///     count is not equal to uint.MaxValue and there are not count values or
        ///     the values are outside the given range.
        /// </summary>
        private static List<int> GetCheckedUnsignedInts(TextBox textBox, uint count, int minVal, int maxVal)
        {
            List<int> rval = null;
            List<string> strings = new List<string>();
            bool okay = true;
            if(textBox.Text.Length > 0)
            {
                foreach(Char c in textBox.Text)
                {
                    if(!(Char.IsDigit(c) || c == ',' || Char.IsWhiteSpace(c)))
                        okay = false;
                }
                if(okay)
                {
                    char[] delimiters = { ',', ' ' };
                    rval = StringToIntList(textBox.Text, delimiters);
                }
            }

            if(rval == null || rval.Count == 0)
                return null;
            else return rval;
        }

        /// <summary>
        /// Returne false if
        ///     intList == null
        ///     count != uint.MaxValue && intList.Count != count
        ///     or any value is less than minVal, 
        ///     or any value is greater than maxval.
        /// </summary>
        private static bool CheckIntList(List<int> intList, uint count, int minVal, int maxVal)
        {
            bool OK = true;
            if(intList == null || (count != uint.MaxValue && intList.Count != count))
                OK = false;
            else
            {
                foreach(int value in intList)
                {
                    if(value < minVal || value > maxVal)
                    {
                        OK = false;
                        break;
                    }
                }
            }
            return OK;
        }

        public static void SetToWhite(TextBox textBox)
        {
            if(textBox != null)
            {
                textBox.ForeColor = Color.Black;
                textBox.BackColor = Color.White;
            }
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

        /// <summary>
        /// Converts a string containing unsigned integers separated by the delimiters in arg2 (' ' and ',')
        /// to the corresponding list of integers.
        /// Returns null if the string contains anything other than the delimiters and positive integers greater than 0. 
        /// Copied (changed a bit) from Moritz...
        /// </summary>
        public static List<int> StringToIntList(string s, char[] delimiters)
        {
            List<int> rval = new List<int>();
            string[] integers = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach(string integer in integers)
            {
                string iString = integer.Trim();
                if(!string.IsNullOrEmpty(iString))
                {
                    int i = int.Parse(iString);
                    if( i <= 0)
                    {
                        rval = null;
                        break;
                    }
                    rval.Add(i);
                }
            }

            return rval;
        }
        #endregion Form1

        #region Error handling
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

        #endregion Error handling

        #region Assertions

        /// <summary>
        /// Fails if the argument is outside the range 1..127.
        /// </summary>
        public static void AssertIsVelocityValue(int velocity)
        {
            M.Assert(velocity >= 1 && velocity <= 127);
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
        /// Used by AssertRange0_11(int) and AssertRange0_127(int)
        /// </summary>
        private static void AssertRange0_(int maxVal, int value)
        {
            if(value < 0 || value > maxVal)
            {
                throw new ApplicationException($"Value must be in range 0..{maxVal}");
            }
        }
        /// <summary>
        /// Used by AssertRange0_11(byte) and AssertRange0_127(byte)
        /// </summary>
        private static void AssertRange0_(int maxVal, byte value)
        {
            if(value < 0 || value > maxVal)
            {
                throw new ApplicationException($"Value must be in range 0..{maxVal}");
            }
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(int value)
        {
            AssertRange0_(11, value);
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(byte value)
        {
            AssertRange0_(11, value);
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(int value)
        {
            AssertRange0_(127, value);
        }
        /// <summary>
        /// Throws an ApplicationException if the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(byte value)
        {
            AssertRange0_(127, value);
        }
        /// <summary>
        /// Used by AssertRange0_11(int collection) and AssertRange0_127(int collection) 
        /// </summary>
        private static void AssertRange0_(int maxVal, ICollection<int> collection)
        {
            foreach(int value in collection)
            {
                if(value < 0 || value > maxVal)
                {
                    throw new ApplicationException($"Value must be in range 0..{maxVal}");
                }
            }
        }
        /// <summary>
        /// Used by AssertRange0_11(byte collection) and AssertRange0_127(byte collection) 
        /// </summary>
        private static void AssertRange0_(int maxVal, ICollection<byte> collection)
        {
            foreach(int value in collection)
            {
                if(value < 0 || value > maxVal)
                {
                    throw new ApplicationException($"Value must be in range 0..{maxVal}");
                }
            }
        }
        /// <summary>
        /// Throws an ApplicationException if an int in the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(ICollection<int> collection)
        {
            AssertRange0_(11, collection);
        }
        /// <summary>
        /// Throws an ApplicationException if a byte in the argument is not in range 0..11. 
        /// </summary>
        public static void AssertRange0_11(ICollection<byte> collection)
        {
            AssertRange0_(11, collection);
        }
        /// <summary>
        /// Throws an ApplicationException if an int in the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(ICollection<int> collection)
        {
            AssertRange0_(127, collection);
        }
        /// <summary>
        /// Throws an ApplicationException if a byte in the argument is not in range 0..127. 
        /// </summary>
        public static void AssertRange0_127(ICollection<byte> collection)
        {
            AssertRange0_(127, collection);
        }

        #endregion

        #region IntDivision
        /// <summary>
        /// This function divides total into divisor parts, returning a List of ints whose:
        ///     * Count is equal to divisor.
        ///     * sum is exactly equal to total
        ///     * members are as equal as possible. 
        /// </summary>
        public static List<int> IntDivisionSizes(int total, int divisor)
        {
            List<int> relativeSizes = new List<int>();
            for(int i = 0; i < divisor; ++i)
            {
                relativeSizes.Add(1);
            }
            return IntDivisionSizes(total, relativeSizes);

        }

        public static List<int> IntDivisionSizes(int total, List<int> relativeSizes)
        {
            int divisor = relativeSizes.Count;
            int sumRelative = 0;
            for(int i = 0; i < divisor; ++i)
            {
                sumRelative += relativeSizes[i];
            }
            double factor = ((double)total / (double)sumRelative);
            double fPos = 0;
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
            M.Assert(intSum <= total);
            if(intSum < total)
            {
                int lastDuration = intDivisionSizes[intDivisionSizes.Count - 1];
                lastDuration += (total - intSum);
                intDivisionSizes.RemoveAt(intDivisionSizes.Count - 1);
                intDivisionSizes.Add(lastDuration);
            }
            return intDivisionSizes;
        }
        #endregion

        #region MIDI
        #region MIDI Constants
        /// <summary>
        /// Commands
        /// </summary>
        public static readonly int CMD_NOTE_OFF_0x80 = 0x80;
        public static readonly int CMD_NOTE_ON_0x90 = 0x90;
        public static readonly int CMD_AFTERTOUCH_0xA0 = 0xA0;
        public static readonly int CMD_CONTROL_CHANGE_0xB0 = 0xB0;
        public static readonly int CMD_PATCH_CHANGE_0xC0 = 0xC0;
        public static readonly int CMD_CHANNEL_PRESSURE_0xD0 = 0xD0;
        public static readonly int CMD_PITCH_WHEEL_0xE0 = 0xE0;

        /// <summary>
        /// Control Numbers (These are just the ones Moritz uses.)
        /// </summary>
        public static readonly int CTL_BANK_CHANGE_0 = 0;
        public static readonly int CTL_MODWHEEL_1 = 1;
        public static readonly int CTL_PAN_10 = 10;
        public static readonly int CTL_EXPRESSION_11 = 11;
        public static readonly int CTL_REGISTEREDPARAMETER_COARSE_101 = 101;
        public static readonly int CTL_REGISTEREDPARAMETER_FINE_100 = 100;
        public static readonly int CTL_DATAENTRY_COARSE_6 = 6;
        public static readonly int CTL_DATAENTRY_FINE_38 = 38;
        public static readonly int SELECT_PITCHBEND_RANGE_0 = 0;
        public static readonly int DEFAULT_NOTEOFF_VELOCITY_64 = 64;
        /// <summary>
        /// The following values are (supposed to be) set by AllControllersOff.
        /// </summary>
        public static readonly byte DEFAULT_BANKAndPATCH_0 = 0;
        public static readonly byte DEFAULT_VOLUME_100 = 100;
        public static readonly byte DEFAULT_EXPRESSION_127 = 127;
        public static readonly byte DEFAULT_PITCHWHEELDEVIATION_2 = 2;
        public static readonly byte DEFAULT_PITCHWHEEL_64 = 64;
        public static readonly byte DEFAULT_PAN_64 = 64;
        public static readonly byte DEFAULT_MODWHEEL_0 = 0;
        /// <summary>
        /// Constants that are set when palette fields are empty
        /// </summary>
        public static readonly bool DEFAULT_HAS_CHORDOFF_true = true;
        public static readonly bool DEFAULT_CHORDREPEATS_false = false;
        public static readonly int DEFAULT_ORNAMENT_MINIMUMDURATION_1 = 1;
        #endregion MIDI Contstants

        /// <summary>
        /// Returns the value argument as a byte, coerced to the range [0..127] 
        /// </summary>
        public static byte SetRange0_127(int value)
        {
            int rval;

            if(value > 127)
                rval = 127;
            else if(value < 0)
                rval = 0;
            else
                rval = value;

            return (byte)rval;
        }

        /// <summary>
        /// Returns the argument as a byte coerced to the range 1..127.
        /// </summary>
        public static byte VelocityValue(int velocity)
        {
            velocity = (velocity >= 1) ? velocity : 1;
            velocity = (velocity <= 127) ? velocity : 127;
            return (byte)velocity;
        }

        private static void SetMidiPitchDict()
        {
            string[] alphabet = { "C", "D", "E", "F", "G", "A", "B" };
            int midiPitch = 0;
            for(int octave = 0; octave < 11; octave++)
            {
                foreach(string letter in alphabet)
                {
                    switch(letter)
                    {
                        case "A":
                            midiPitch = 9;
                            break;
                        case "B":
                            midiPitch = 11;
                            break;
                        case "C":
                            midiPitch = 0;
                            break;
                        case "D":
                            midiPitch = 2;
                            break;
                        case "E":
                            midiPitch = 4;
                            break;
                        case "F":
                            midiPitch = 5;
                            break;
                        case "G":
                            midiPitch = 7;
                            break;
                    }

                    midiPitch += (octave * 12); // C5 (middle letter) is 60
                    if(midiPitch >= 0 && midiPitch <= 119)
                    {
                        string pitchString = letter + octave.ToString();
                        MidiPitchDict.Add(pitchString, midiPitch);
                    }
                    else if(midiPitch > 119 && midiPitch < 128)
                    {
                        string pitchString = letter + ":";
                        MidiPitchDict.Add(pitchString, midiPitch);
                    }
                }
            }
        }

        /// <summary>
        /// capella pitch strings and their equivalent midi pitch numbers.
        /// the strings are absolute diatonic pitch. middle C (c'): "C5"
        /// Chromatic pitches are found using the head.alteration field (-2..+2)
        /// </summary>
        static public Dictionary<string, int> MidiPitchDict = new Dictionary<string, int>();

        /// <summary>
        /// The current time measured in milliseconds.
        /// </summary>
        public static int NowMilliseconds
        {
            get
            {
                return (int)DateTime.Now.Ticks / 10000;
            }
        }

        /// <summary>
        /// Note that Moritz does not use M.Dynamic.ffff even though it is defined in CLicht.
        /// </summary>
        public enum Dynamic
        {
            none, pppp, ppp, pp, p, mp, mf, f, ff, fff
        }

        /// <summary>
        /// The key is one of the following strings: "fff", "ff", "f", "mf", "mp", "p", "pp", "ppp", "pppp".
        /// The value is used to determine Moritz' transcription of velocity -> dynamic symbol.
        /// Note that Moritz does not use M.Dynamic.ffff even though it is defined in CLicht.
        /// </summary>
        public static Dictionary<M.Dynamic, byte> MaxMidiVelocity = new Dictionary<M.Dynamic, byte>()
        {
            // March 2016:  equal steps between 15 (max pppp) and 127 (max fff)
            { M.Dynamic.fff, 127},
            { M.Dynamic.ff, 113},
            { M.Dynamic.f, 99},
            { M.Dynamic.mf, 85},
            { M.Dynamic.mp, 71},
            { M.Dynamic.p, 57},
            { M.Dynamic.pp, 43},
            { M.Dynamic.ppp, 29},
            { M.Dynamic.pppp, 15}
        };

        /// <summary>
        /// The key is one of the following strings: "fff", "ff", "f", "mf", "mp", "p", "pp", "ppp", "pppp".
        /// The value is a string containing the equivalent CLicht character.
        /// Note that Moritz does not use M.Dynamic.ffff even though it is defined in CLicht.
        /// </summary>
        public static Dictionary<M.Dynamic, string> CLichtDynamicsCharacters = new Dictionary<M.Dynamic, string>()
        {
            { M.Dynamic.fff, "Ï"},
            { M.Dynamic.ff, "ƒ"},
            { M.Dynamic.f, "f"},
            { M.Dynamic.mf, "F"},
            { M.Dynamic.mp, "P"},
            { M.Dynamic.p, "p"},
            { M.Dynamic.pp, "π"},
            { M.Dynamic.ppp, "∏"},
            { M.Dynamic.pppp, "Ø"}
        };

        #endregion MIDI

        #region XML
        public static void CreateDirectoryIfItDoesNotExist(string directoryPath)
        {
            if(!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                while(!Directory.Exists(directoryPath))
                    Thread.Sleep(100);
            }
        }
        /// <summary>
        /// Converts the value to a string, using as few decimal places as possible (maximum 4) and a '.' decimal point where necessary.
        /// Use this whenever writing an attribute to SVG.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DoubleToShortString(double value)
        {
            return ((float)value).ToString("0.####", En_USNumberFormat);
        }

        /// <summary>
        /// Returns the name of an enum field as a string, or (if the field has a Description Attribute)
        /// its Description attribute.
        /// </summary>
        /// <example>
        /// If enum Language is defined as follows:
        /// 
        /// 	public enum Language
        /// 	{
        ///										Basic,
        /// 		[Description("Kernigan")]   C,
        /// 		[Description("Stroustrup")]	CPP,
        /// 		[Description("Gosling")]	Java,
        /// 		[Description("Helzberg")]	CSharp
        /// 	}
        /// 
        ///		string languageDescription = GetEnumDescription(Language.Basic);
        /// sets languageDescription to "Basic"
        /// 
        ///		string languageDescription = GetEnumDescription(Language.CPP);
        /// sets languageDescription to "Stroustrup"
        /// </example>
        /// <param name="field">A field of any enum</param>
        /// <returns>
        /// If the enum field has no Description attribute, the field's name as a string.
        /// If the enum field has a Description attribute, the value of that attribute.
        /// </returns>
        public static string GetEnumDescription(Enum field)
        {
            FieldInfo fieldInfo = field.GetType().GetField(field.ToString());
            DescriptionAttribute[] attribs =
                (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attribs.Length == 0 ? field.ToString() : attribs[0].Description);
        }
        #endregion
    }
}

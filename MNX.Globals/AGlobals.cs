using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace MNX.AGlobals 
{
    public static class A
    {
        #region application constants
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
        private static readonly string mnxMainFolder = GetMNX_MainFolder();
        // contains all MNX input files (not just mnx-common)
        public static readonly string MNX_in_Folder = mnxMainFolder + @"\MNX_in\mnx\";
        // contains page formatting data parallel to the files in the MNX_in_Folder.
        public static readonly string SVGData_Folder = mnxMainFolder + @"\MNX_in\svgData\";
        // contains the output SVG.
        public static readonly string SVG_out_Folder = mnxMainFolder + @"\SVG_out\";

        //Creates and initializes the CultureInfo which uses the international sort.
        public static CultureInfo ci = new CultureInfo("en-US", false);
        public static NumberFormatInfo En_USNumberFormat = ci.NumberFormat;
        #endregion

        // These are set for the score currently being constructed.
        public static MNXProfile? Profile = null;

        #region Cloned Moritz functions
        #region Form1
        /// <summary>
        /// Cloned from Moritz
        /// </summary>
        /// <param name="directoryPath"></param>
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
                textBox.BackColor = A.TextBoxErrorColor;
            }
        }

        public static Color TextBoxErrorColor = Color.FromArgb(255, 220, 220);
        //public static Color GreenButtonColor = Color.FromArgb(215, 225, 215);
        //public static Color LightGreenButtonColor = Color.FromArgb(205, 240, 205);

        public static bool HasError(TextBox textBox)
        {
            return textBox.BackColor == A.TextBoxErrorColor;
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
        #endregion Error handling
        #endregion Cloned Moritz functions
    }
}

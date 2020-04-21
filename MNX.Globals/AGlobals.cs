﻿using System;
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
        public static SVGData SVGData;

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

        //public delegate void SetDialogStateDelegate(TextBox textBox, bool success);
        ///// <summary>
        ///// If the textBox is disabled, this function does nothing.
        ///// SetDialogState sets the text box to an error state (usually by setting its background colour to pink) if:
        /////     textBox.Text is empty, or
        /////     textBox.Text contains anything other than numbers, commas and whitespace or
        /////     count != uint.MaxValue and there are not count values or the values are outside the given range.
        ///// Cloned from Moritz
        ///// </summary>
        //public static void LeaveIntRangeTextBox(TextBox textBox, bool canBeEmpty, uint count, int minVal, int maxVal,
        //                                            SetDialogStateDelegate SetDialogState)
        //{
        //    if(textBox.Enabled)
        //    {
        //        if(textBox.Text.Length > 0)
        //        {
        //            List<string> checkedIntStrings = GetCheckedIntStrings(textBox, count, minVal, maxVal);
        //            if(checkedIntStrings != null)
        //            {
        //                StringBuilder sb = new StringBuilder();
        //                foreach(string intString in checkedIntStrings)
        //                {
        //                    sb.Append(",  " + intString);
        //                }
        //                sb.Remove(0, 3);
        //                textBox.Text = sb.ToString();
        //                SetDialogState(textBox, true);
        //            }
        //            else
        //            {
        //                SetDialogState(textBox, false);
        //            }
        //        }
        //        else
        //        {
        //            if(canBeEmpty)
        //                SetDialogState(textBox, true);
        //            else
        //                SetDialogState(textBox, false);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Returns null if
        /////     textBox.Text is empty, or
        /////     textBox.Text contains anything other than numbers, commas and whitespace or
        /////     count is not equal to uint.MaxValue and there are not count values or
        /////     the values are outside the given range.
        ///// Cloned from Moritz
        ///// </summary>
        //private static List<string> GetCheckedIntStrings(TextBox textBox, uint count, int minVal, int maxVal)
        //{
        //    List<string> strings = new List<string>();
        //    bool okay = true;
        //    if(textBox.Text.Length > 0)
        //    {
        //        foreach(Char c in textBox.Text)
        //        {
        //            if(!(Char.IsDigit(c) || c == ',' || Char.IsWhiteSpace(c)))
        //                okay = false;
        //        }
        //        if(okay)
        //        {
        //            try
        //            {
        //                List<int> ints = StringToIntList(textBox.Text, ',');

        //                if(CheckIntList(ints, count, minVal, maxVal))
        //                {
        //                    foreach(int i in ints)
        //                        strings.Add(i.ToString(A.En_USNumberFormat));
        //                }
        //            }
        //            catch
        //            {
        //                // This can happen if StringToIntList(...) throws an exception
        //                // -- which can happen if two numbers are separated by whitespace but no comma!)
        //            }
        //        }
        //    }

        //    if(strings.Count > 0)
        //        return strings;
        //    else return null;
        //}

        ///// <summary>
        ///// Returne false if
        /////     intList == null
        /////     count != uint.MaxValue && intList.Count != count
        /////     or any value is less than minVal, 
        /////     or any value is greater than maxval.
        ///// Cloned from Moritz
        ///// </summary>
        //private static bool CheckIntList(List<int> intList, uint count, int minVal, int maxVal)
        //{
        //    bool OK = true;
        //    if(intList == null || (count != uint.MaxValue && intList.Count != count))
        //        OK = false;
        //    else
        //    {
        //        foreach(int value in intList)
        //        {
        //            if(value < minVal || value > maxVal)
        //            {
        //                OK = false;
        //                break;
        //            }
        //        }
        //    }
        //    return OK;
        //}

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
        /// Copied from Moritz...
        /// Converts a string containing integers separated by whitespace and the character in arg2
        /// to the corresponding list of integers.
        /// Throws an exception if the string contains anything other than 
        /// positive or negative integers, the separator or white space. 
        /// </summary>
        public static List<int> StringToIntList(string s, char separator)
        {
            List<int> rval = new List<int>();
            char[] delimiter = { separator };
            string[] integers = s.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                foreach(string integer in integers)
                {
                    string i = integer.Trim();
                    if(!string.IsNullOrEmpty(i))
                    {
                        rval.Add(int.Parse(i));
                    }
                }
            }
            catch
            {
                throw new ApplicationException("Error in AGlobals.StringToIntList()");
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

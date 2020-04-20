using System;
using System.Collections.Generic;
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

        // used when writing doubles as text (with decimal point)
        public static readonly CultureInfo en_US_CultureInfo = CultureInfo.CreateSpecificCulture("en-US");
        #endregion

        // These are set for the score currently being constructed.
        public static MNXProfile? Profile = null;
        public static SVGData SVGData;

        /// <summary>
        /// Copied from Moritz
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
        /// Copied from Moritz
        /// </summary>
        public static string NowString
        {
            get
            {
                CultureInfo ci = new CultureInfo("en-US", false);
                return DateTime.Today.ToString("dddd dd.MM.yyyy", ci.DateTimeFormat) + ", " + DateTime.Now.ToLongTimeString();
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
    }
}

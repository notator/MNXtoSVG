using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MNX.AGlobals 
{
    public static class A
    {
        public static MNXProfile? Profile = null;

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
    }
}

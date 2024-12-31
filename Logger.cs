using System;
using System.Diagnostics;
using System.IO;

namespace FunWebsiteThing
{
    /*
     * Methods: Write
     * Properties/Misc: Log
     */
    /// <summary>
    /// This class handles logging for various classes throughout the program, including a stacktrace output in case we have an error. 
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Our log file is located in the program's folder
        /// </summary>
        public static string Log = "./FWT.log";
        /// <summary>
        /// Write: Writes a message to our Wasspord.log, usually important info such as errors, warnings, or debug info I'd appreciate if an issue arises.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messagetype"></param>
        public static void Write(string message, string messagetype = "LOG")
        {
            string[] flavortexts = { "The problem probably lies in", "The stack is as follows:" };
            string Time = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt");
            StackTrace st = new StackTrace(); // Create a stack trace
            StackFrame parentsf = st.GetFrame(1); // this is the parent of the method call
            StackFrame grandparentsf = st.GetFrame(2); // this is the grandparent (parent's parent) of the method call
            using (StreamWriter writer = new StreamWriter(Log, true))
            {
                writer.WriteLine("(" + Time + ") [" + messagetype + "]: " + message);
                if (messagetype == "ERROR" || messagetype == "DEBUG") // if error, let's help out by giving the stack trace
                {
                    string stack = grandparentsf.GetMethod().Name + " -> " + parentsf.GetMethod().Name; // this is a string that says Grandparent -> Parent
                    writer.WriteLine("(" + Time + ") [" + messagetype + "]: " + messagetype == "ERROR" ? flavortexts[0] : flavortexts[1] + " " + stack + ".");
                }
                writer.Close();
            }
        }
    }
}

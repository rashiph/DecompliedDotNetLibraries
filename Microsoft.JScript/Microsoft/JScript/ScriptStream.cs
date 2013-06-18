namespace Microsoft.JScript
{
    using System;
    using System.IO;

    public class ScriptStream
    {
        public static TextWriter Error = Console.Error;
        public static TextWriter Out = Console.Out;

        public static void PrintStackTrace()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception exception)
            {
                PrintStackTrace(exception);
            }
        }

        public static void PrintStackTrace(Exception e)
        {
            Out.WriteLine(e.StackTrace);
            Out.Flush();
        }

        public static void Write(string str)
        {
            Out.Write(str);
        }

        public static void WriteLine(string str)
        {
            Out.WriteLine(str);
        }
    }
}


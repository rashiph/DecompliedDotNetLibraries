namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization.Formatters;

    internal static class Util
    {
        internal static StreamWriter writer;

        [Conditional("_LOGGING")]
        internal static void Log(string message)
        {
        }

        [Conditional("_LOGGING")]
        internal static void LogInput(ref TextReader input)
        {
            if (InternalRM.SoapCheckEnabled())
            {
                string s = input.ReadToEnd();
                input = new StringReader(s);
            }
        }

        [Conditional("_LOGGING")]
        internal static void LogString(string strbuffer)
        {
        }
    }
}


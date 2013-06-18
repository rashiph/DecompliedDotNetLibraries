namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Diagnostics;

    internal static class Util
    {
        [Conditional("SER_LOGGING")]
        internal static void NVTrace(string name, object value)
        {
        }

        [Conditional("SER_LOGGING")]
        internal static void NVTrace(string name, string value)
        {
        }

        [Conditional("_LOGGING")]
        internal static void NVTraceI(string name, object value)
        {
        }

        [Conditional("_LOGGING")]
        internal static void NVTraceI(string name, string value)
        {
        }

        internal static string PString(string value)
        {
            if (value == null)
            {
                return "";
            }
            return value;
        }
    }
}


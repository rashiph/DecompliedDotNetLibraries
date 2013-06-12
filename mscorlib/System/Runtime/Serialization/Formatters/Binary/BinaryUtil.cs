namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;

    internal static class BinaryUtil
    {
        [Conditional("_LOGGING")]
        public static void NVTraceI(string name, object value)
        {
            BCLDebug.CheckEnabled("BINARY");
        }

        [Conditional("_LOGGING")]
        public static void NVTraceI(string name, string value)
        {
            BCLDebug.CheckEnabled("BINARY");
        }
    }
}


namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true), SecurityCritical]
    public sealed class InternalRM
    {
        [Conditional("_LOGGING")]
        public static void InfoSoap(params object[] messages)
        {
        }

        public static bool SoapCheckEnabled()
        {
            return BCLDebug.CheckEnabled("SOAP");
        }
    }
}


namespace System.Linq.Parallel
{
    using System;
    using System.Diagnostics;

    internal static class TraceHelpers
    {
        internal static void NotYetImplemented()
        {
            NotYetImplemented(false, "NYI");
        }

        internal static void NotYetImplemented(string message)
        {
            NotYetImplemented(false, "NYI: " + message);
        }

        internal static void NotYetImplemented(bool assertCondition, string message)
        {
            if (!assertCondition)
            {
                throw new NotImplementedException();
            }
        }

        [Conditional("PFXTRACE")]
        internal static void SetVerbose()
        {
        }

        [Conditional("PFXTRACE")]
        internal static void TraceError(string msg, params object[] args)
        {
        }

        [Conditional("PFXTRACE")]
        internal static void TraceInfo(string msg, params object[] args)
        {
        }

        [Conditional("PFXTRACE")]
        internal static void TraceWarning(string msg, params object[] args)
        {
        }
    }
}


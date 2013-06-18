namespace System.Runtime.Caching
{
    using System;
    using System.Diagnostics;
    using System.Security;

    [SecuritySafeCritical]
    internal static class Dbg
    {
        internal const string DATE_FORMAT = "yyyy/MM/dd HH:mm:ss.ffff";
        internal const string TAG_ALL = "*";
        internal const string TAG_EXTERNAL = "External";
        internal const string TAG_INTERNAL = "Internal";
        internal const string TIME_FORMAT = "HH:mm:ss:ffff";

        [Conditional("DBG")]
        internal static void AlwaysValidate(string tagName)
        {
        }

        [Conditional("DBG")]
        internal static void Assert(bool assertion)
        {
        }

        [Conditional("DBG")]
        internal static void Assert(bool assertion, string message)
        {
        }

        [Conditional("DBG")]
        internal static void Break()
        {
        }

        [Conditional("DBG")]
        internal static void CheckValid(bool assertion, string message)
        {
        }

        [Conditional("DBG")]
        internal static void Dump(string tagName, object obj)
        {
        }

        [Conditional("DBG")]
        internal static void Fail(string message)
        {
        }

        internal static string FormatLocalDate(DateTime localTime)
        {
            return string.Empty;
        }

        internal static bool IsTagEnabled(string tagName)
        {
            return false;
        }

        internal static bool IsTagPresent(string tagName)
        {
            return false;
        }

        [Conditional("DBG")]
        internal static void Trace(string tagName, Exception e)
        {
        }

        [Conditional("DBG")]
        internal static void Trace(string tagName, string message)
        {
        }

        [Conditional("DBG")]
        internal static void Trace(string tagName, string message, bool includePrefix)
        {
        }

        [Conditional("DBG")]
        internal static void Trace(string tagName, string message, Exception e)
        {
        }

        [Conditional("DBG")]
        internal static void Trace(string tagName, string message, Exception e, bool includePrefix)
        {
        }

        [Conditional("DBG")]
        public static void TraceException(string tagName, Exception e)
        {
        }

        [Conditional("DBG")]
        internal static void Validate(object obj)
        {
        }

        [Conditional("DBG")]
        internal static void Validate(string tagName, object obj)
        {
        }
    }
}


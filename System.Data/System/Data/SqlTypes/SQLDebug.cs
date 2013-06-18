namespace System.Data.SqlTypes
{
    using System;
    using System.Diagnostics;

    internal sealed class SQLDebug
    {
        private SQLDebug()
        {
        }

        [Conditional("DEBUG")]
        internal static void Check(bool condition)
        {
        }

        [Conditional("DEBUG")]
        internal static void Check(bool condition, string conditionString)
        {
        }

        [Conditional("DEBUG")]
        internal static void Check(bool condition, string conditionString, string message)
        {
        }
    }
}


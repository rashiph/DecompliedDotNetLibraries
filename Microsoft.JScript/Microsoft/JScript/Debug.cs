namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;

    internal static class Debug
    {
        [Conditional("ASSERTION")]
        public static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new AssertException("Assertion fired");
            }
        }

        [Conditional("ASSERTION")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new AssertException(message);
            }
        }

        [Conditional("ASSERTION")]
        public static void NotImplemented(string message)
        {
            throw new AssertException("Method Not Yet Implemented");
        }

        [Conditional("ASSERTION")]
        public static void PostCondition(bool condition)
        {
            if (!condition)
            {
                throw new PostConditionException("PostCondition missed");
            }
        }

        [Conditional("ASSERTION")]
        public static void PostCondition(bool condition, string message)
        {
            if (!condition)
            {
                throw new PostConditionException(message);
            }
        }

        [Conditional("ASSERTION")]
        public static void PreCondition(bool condition)
        {
            if (!condition)
            {
                throw new PreConditionException("PreCondition missed");
            }
        }

        [Conditional("ASSERTION")]
        public static void PreCondition(bool condition, string message)
        {
            if (!condition)
            {
                throw new PreConditionException(message);
            }
        }

        [Conditional("LOGGING")]
        public static void Print(string str)
        {
            ScriptStream.Out.WriteLine(str);
        }

        [Conditional("LOGGING")]
        internal static void PrintLine(string message)
        {
            ScriptStream.Out.WriteLine(message);
        }

        [Conditional("LOGGING")]
        public static void PrintStack()
        {
            ScriptStream.PrintStackTrace();
        }

        [Conditional("LOGGING")]
        public static void PrintStack(Exception e)
        {
            ScriptStream.PrintStackTrace(e);
        }
    }
}


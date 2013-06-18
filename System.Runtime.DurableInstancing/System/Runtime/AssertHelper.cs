namespace System.Runtime
{
    using System;
    using System.Diagnostics;

    internal static class AssertHelper
    {
        internal static void FireAssert(string message)
        {
            try
            {
            }
            finally
            {
                Debug.Assert(false, message);
            }
        }
    }
}


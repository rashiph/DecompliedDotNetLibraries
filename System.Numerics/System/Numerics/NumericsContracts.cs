namespace System.Numerics
{
    using System;
    using System.Diagnostics;

    internal static class NumericsContracts
    {
        [Conditional("DEBUG")]
        public static void Check(bool f)
        {
        }

        [Conditional("DEBUG")]
        public static void Fail(string str)
        {
        }
    }
}


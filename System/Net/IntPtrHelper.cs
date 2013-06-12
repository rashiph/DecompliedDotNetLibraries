namespace System.Net
{
    using System;

    internal static class IntPtrHelper
    {
        internal static IntPtr Add(IntPtr a, int b)
        {
            return (IntPtr) (((long) a) + b);
        }

        internal static long Subtract(IntPtr a, IntPtr b)
        {
            return (((long) a) - ((long) b));
        }
    }
}


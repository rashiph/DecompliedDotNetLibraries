namespace System.IdentityModel
{
    using System;

    internal static class IntPtrHelper
    {
        private const string KERNEL32 = "kernel32.dll";

        internal static IntPtr Add(IntPtr a, int b)
        {
            return (IntPtr) (((long) a) + b);
        }
    }
}


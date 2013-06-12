namespace System.Linq.Parallel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Wrapper<T>
    {
        internal T Value;
        internal Wrapper(T value)
        {
            this.Value = value;
        }
    }
}


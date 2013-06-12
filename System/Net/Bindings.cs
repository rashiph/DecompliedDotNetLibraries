namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Bindings
    {
        internal int BindingsLength;
        internal IntPtr pBindings;
    }
}


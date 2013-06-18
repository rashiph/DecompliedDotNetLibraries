namespace System.Windows.Forms.Layout
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    internal struct NullLayoutTransaction : IDisposable
    {
        public void Dispose()
        {
        }
    }
}


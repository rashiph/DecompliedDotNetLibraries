namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ForceTokenStabilization]
    internal struct NativeVariant
    {
        private ushort vt;
        private ushort wReserved1;
        private ushort wReserved2;
        private ushort wReserved3;
        private IntPtr data1;
        private IntPtr data2;
    }
}


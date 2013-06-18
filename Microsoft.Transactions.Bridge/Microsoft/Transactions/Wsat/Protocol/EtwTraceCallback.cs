namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal unsafe delegate uint EtwTraceCallback([In] uint requestCode, [In] IntPtr requestContext, [In] IntPtr bufferSize, [In] byte* buffer);
}


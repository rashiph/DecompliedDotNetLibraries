namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate int WaitDelegate(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);
}


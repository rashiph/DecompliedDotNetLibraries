namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public delegate void WaitOrTimerCallback(object state, bool timedOut);
}


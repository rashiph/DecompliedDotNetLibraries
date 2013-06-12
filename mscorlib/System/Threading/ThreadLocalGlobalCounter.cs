namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ThreadLocalGlobalCounter
    {
        internal static int MAXIMUM_GLOBAL_COUNT = (ThreadLocal<int>.MAXIMUM_TYPES_LENGTH * 4);
        internal static volatile int s_fastPathCount;
    }
}


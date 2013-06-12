namespace System.Threading
{
    using System;

    [Serializable]
    internal enum StackCrawlMark
    {
        LookForMe,
        LookForMyCaller,
        LookForMyCallersCaller,
        LookForThread
    }
}


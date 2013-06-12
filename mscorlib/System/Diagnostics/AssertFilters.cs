namespace System.Diagnostics
{
    using System;

    [Serializable]
    internal enum AssertFilters
    {
        FailDebug,
        FailIgnore,
        FailTerminate,
        FailContinueFilter
    }
}


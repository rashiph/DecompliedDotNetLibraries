namespace System.Data.Common
{
    using System;

    public enum GroupByBehavior
    {
        Unknown,
        NotSupported,
        Unrelated,
        MustContainAll,
        ExactMatch
    }
}


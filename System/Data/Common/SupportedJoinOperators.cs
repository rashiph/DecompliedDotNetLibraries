namespace System.Data.Common
{
    using System;

    [Flags]
    public enum SupportedJoinOperators
    {
        FullOuter = 8,
        Inner = 1,
        LeftOuter = 2,
        None = 0,
        RightOuter = 4
    }
}


namespace System.Linq.Expressions
{
    using System;

    internal enum AnalyzeTypeIsResult
    {
        KnownFalse,
        KnownTrue,
        KnownAssignable,
        Unknown
    }
}


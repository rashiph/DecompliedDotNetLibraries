namespace System.Linq.Parallel
{
    using System;

    [Flags]
    internal enum QueryAggregationOptions
    {
        None,
        Associative,
        Commutative,
        AssociativeCommutative
    }
}


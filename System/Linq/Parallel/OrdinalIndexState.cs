namespace System.Linq.Parallel
{
    using System;

    internal enum OrdinalIndexState : byte
    {
        Correct = 1,
        Increasing = 2,
        Indexible = 0,
        Shuffled = 3
    }
}


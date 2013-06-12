namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;

    internal interface IMergeHelper<TInputOutput>
    {
        void Execute();
        IEnumerator<TInputOutput> GetEnumerator();
        TInputOutput[] GetResultsAsArray();
    }
}


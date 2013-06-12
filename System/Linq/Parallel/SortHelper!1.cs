namespace System.Linq.Parallel
{
    using System;

    internal abstract class SortHelper<TInputOutput>
    {
        protected SortHelper()
        {
        }

        internal abstract TInputOutput[] Sort();
    }
}


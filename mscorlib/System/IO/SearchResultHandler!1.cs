namespace System.IO
{
    using System;
    using System.Security;

    internal abstract class SearchResultHandler<TSource>
    {
        protected SearchResultHandler()
        {
        }

        [SecurityCritical]
        internal abstract TSource CreateObject(SearchResult result);
        [SecurityCritical]
        internal abstract bool IsResultIncluded(SearchResult result);
    }
}


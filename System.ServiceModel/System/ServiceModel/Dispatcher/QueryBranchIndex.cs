namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class QueryBranchIndex
    {
        protected QueryBranchIndex()
        {
        }

        internal abstract void CollectXPathFilters(ICollection<MessageFilter> filters);
        internal abstract void Match(int valIndex, ref Value val, QueryBranchResultSet results);
        internal abstract void Remove(object key);
        internal abstract void Trim();

        internal abstract int Count { get; }

        internal abstract QueryBranch this[object key] { get; set; }
    }
}


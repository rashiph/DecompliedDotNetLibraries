namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class QueryBranchResultSet
    {
        internal static SortComparer comparer = new SortComparer();
        private QueryBranchResultSet next;
        private QueryBuffer<QueryBranchResult> results;

        internal QueryBranchResultSet() : this(2)
        {
        }

        internal QueryBranchResultSet(int capacity)
        {
            this.results = new QueryBuffer<QueryBranchResult>(capacity);
        }

        internal void Add(QueryBranch branch, int valIndex)
        {
            this.results.Add(new QueryBranchResult(branch, valIndex));
        }

        internal void Clear()
        {
            this.results.count = 0;
        }

        internal void Sort()
        {
            this.results.Sort(comparer);
        }

        internal int Count
        {
            get
            {
                return this.results.count;
            }
        }

        internal QueryBranchResult this[int index]
        {
            get
            {
                return this.results[index];
            }
        }

        internal QueryBranchResultSet Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal class SortComparer : IComparer<QueryBranchResult>
        {
            public int Compare(QueryBranchResult x, QueryBranchResult y)
            {
                return (x.branch.id - y.branch.id);
            }

            public bool Equals(QueryBranchResult x, QueryBranchResult y)
            {
                return (x.branch.id == y.branch.id);
            }

            public int GetHashCode(QueryBranchResult obj)
            {
                return obj.branch.id;
            }
        }
    }
}


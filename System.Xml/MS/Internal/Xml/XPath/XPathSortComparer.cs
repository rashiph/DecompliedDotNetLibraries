namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal sealed class XPathSortComparer : IComparer<SortKey>
    {
        private IComparer[] comparers;
        private Query[] expressions;
        private const int minSize = 3;
        private int numSorts;

        public XPathSortComparer() : this(3)
        {
        }

        public XPathSortComparer(int size)
        {
            if (size <= 0)
            {
                size = 3;
            }
            this.expressions = new Query[size];
            this.comparers = new IComparer[size];
        }

        public void AddSort(Query evalQuery, IComparer comparer)
        {
            if (this.numSorts == this.expressions.Length)
            {
                Query[] queryArray = new Query[this.numSorts * 2];
                IComparer[] comparerArray = new IComparer[this.numSorts * 2];
                for (int i = 0; i < this.numSorts; i++)
                {
                    queryArray[i] = this.expressions[i];
                    comparerArray[i] = this.comparers[i];
                }
                this.expressions = queryArray;
                this.comparers = comparerArray;
            }
            if ((evalQuery.StaticType == XPathResultType.NodeSet) || (evalQuery.StaticType == XPathResultType.Any))
            {
                evalQuery = new StringFunctions(Function.FunctionType.FuncString, new Query[] { evalQuery });
            }
            this.expressions[this.numSorts] = evalQuery;
            this.comparers[this.numSorts] = comparer;
            this.numSorts++;
        }

        internal XPathSortComparer Clone()
        {
            XPathSortComparer comparer = new XPathSortComparer(this.numSorts);
            for (int i = 0; i < this.numSorts; i++)
            {
                comparer.comparers[i] = this.comparers[i];
                comparer.expressions[i] = (Query) this.expressions[i].Clone();
            }
            comparer.numSorts = this.numSorts;
            return comparer;
        }

        public Query Expression(int i)
        {
            return this.expressions[i];
        }

        int IComparer<SortKey>.Compare(SortKey x, SortKey y)
        {
            int num = 0;
            for (int i = 0; i < x.NumKeys; i++)
            {
                num = this.comparers[i].Compare(x[i], y[i]);
                if (num != 0)
                {
                    return num;
                }
            }
            return (x.OriginalPosition - y.OriginalPosition);
        }

        public int NumSorts
        {
            get
            {
                return this.numSorts;
            }
        }
    }
}


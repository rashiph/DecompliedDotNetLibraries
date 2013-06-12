namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ArrayMergeHelper<TInputOutput> : IMergeHelper<TInputOutput>
    {
        private TInputOutput[] m_outputArray;
        private QueryResults<TInputOutput> m_queryResults;
        private QuerySettings m_settings;

        public ArrayMergeHelper(QuerySettings settings, QueryResults<TInputOutput> queryResults)
        {
            this.m_settings = settings;
            this.m_queryResults = queryResults;
            int count = this.m_queryResults.Count;
            this.m_outputArray = new TInputOutput[count];
        }

        public void Execute()
        {
            new QueryExecutionOption<int>(QueryOperator<int>.AsQueryOperator(ParallelEnumerable.Range(0, this.m_queryResults.Count)), this.m_settings).ForAll<int>(new Action<int>(this.ToArrayElement));
        }

        public IEnumerator<TInputOutput> GetEnumerator()
        {
            return ((IEnumerable<TInputOutput>) this.GetResultsAsArray()).GetEnumerator();
        }

        public TInputOutput[] GetResultsAsArray()
        {
            return this.m_outputArray;
        }

        private void ToArrayElement(int index)
        {
            this.m_outputArray[index] = this.m_queryResults[index];
        }
    }
}


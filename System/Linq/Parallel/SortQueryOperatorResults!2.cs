namespace System.Linq.Parallel
{
    using System;

    internal class SortQueryOperatorResults<TInputOutput, TSortKey> : QueryResults<TInputOutput>
    {
        protected QueryResults<TInputOutput> m_childQueryResults;
        private SortQueryOperator<TInputOutput, TSortKey> m_op;
        private bool m_preferStriping;
        private QuerySettings m_settings;

        internal SortQueryOperatorResults(QueryResults<TInputOutput> childQueryResults, SortQueryOperator<TInputOutput, TSortKey> op, QuerySettings settings, bool preferStriping)
        {
            this.m_childQueryResults = childQueryResults;
            this.m_op = op;
            this.m_settings = settings;
            this.m_preferStriping = preferStriping;
        }

        internal override void GivePartitionedStream(IPartitionedStreamRecipient<TInputOutput> recipient)
        {
            this.m_childQueryResults.GivePartitionedStream(new ChildResultsRecipient<TInputOutput, TSortKey>(recipient, this.m_op, this.m_settings));
        }

        internal override bool IsIndexible
        {
            get
            {
                return false;
            }
        }

        private class ChildResultsRecipient : IPartitionedStreamRecipient<TInputOutput>
        {
            private SortQueryOperator<TInputOutput, TSortKey> m_op;
            private IPartitionedStreamRecipient<TInputOutput> m_outputRecipient;
            private QuerySettings m_settings;

            internal ChildResultsRecipient(IPartitionedStreamRecipient<TInputOutput> outputRecipient, SortQueryOperator<TInputOutput, TSortKey> op, QuerySettings settings)
            {
                this.m_outputRecipient = outputRecipient;
                this.m_op = op;
                this.m_settings = settings;
            }

            public void Receive<TKey>(PartitionedStream<TInputOutput, TKey> childPartitionedStream)
            {
                this.m_op.WrapPartitionedStream<TKey>(childPartitionedStream, this.m_outputRecipient, false, this.m_settings);
            }
        }
    }
}


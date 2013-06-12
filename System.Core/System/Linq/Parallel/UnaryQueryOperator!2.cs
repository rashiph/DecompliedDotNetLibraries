namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class UnaryQueryOperator<TInput, TOutput> : QueryOperator<TOutput>
    {
        private readonly QueryOperator<TInput> m_child;
        private System.Linq.Parallel.OrdinalIndexState m_indexState;

        internal UnaryQueryOperator(IEnumerable<TInput> child) : this(QueryOperator<TInput>.AsQueryOperator(child))
        {
        }

        private UnaryQueryOperator(QueryOperator<TInput> child) : this(child, child.OutputOrdered, child.SpecifiedQuerySettings)
        {
        }

        internal UnaryQueryOperator(IEnumerable<TInput> child, bool outputOrdered) : this(QueryOperator<TInput>.AsQueryOperator(child), outputOrdered)
        {
        }

        internal UnaryQueryOperator(QueryOperator<TInput> child, bool outputOrdered) : this(child, outputOrdered, child.SpecifiedQuerySettings)
        {
        }

        private UnaryQueryOperator(QueryOperator<TInput> child, bool outputOrdered, QuerySettings settings) : base(outputOrdered, settings)
        {
            this.m_indexState = System.Linq.Parallel.OrdinalIndexState.Shuffled;
            this.m_child = child;
        }

        protected void SetOrdinalIndexState(System.Linq.Parallel.OrdinalIndexState indexState)
        {
            this.m_indexState = indexState;
        }

        internal abstract void WrapPartitionedStream<TKey>(PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TOutput> recipient, bool preferStriping, QuerySettings settings);

        internal QueryOperator<TInput> Child
        {
            get
            {
                return this.m_child;
            }
        }

        internal sealed override System.Linq.Parallel.OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return this.m_indexState;
            }
        }

        internal class UnaryQueryOperatorResults : QueryResults<TOutput>
        {
            protected QueryResults<TInput> m_childQueryResults;
            private UnaryQueryOperator<TInput, TOutput> m_op;
            private bool m_preferStriping;
            private QuerySettings m_settings;

            internal UnaryQueryOperatorResults(QueryResults<TInput> childQueryResults, UnaryQueryOperator<TInput, TOutput> op, QuerySettings settings, bool preferStriping)
            {
                this.m_childQueryResults = childQueryResults;
                this.m_op = op;
                this.m_settings = settings;
                this.m_preferStriping = preferStriping;
            }

            internal override void GivePartitionedStream(IPartitionedStreamRecipient<TOutput> recipient)
            {
                if ((((ParallelExecutionMode) this.m_settings.ExecutionMode.Value) == ParallelExecutionMode.Default) && this.m_op.LimitsParallelism)
                {
                    IEnumerable<TOutput> source = this.m_op.AsSequentialQuery(this.m_settings.CancellationState.ExternalCancellationToken);
                    PartitionedStream<TOutput, int> partitionedStream = ExchangeUtilities.PartitionDataSource<TOutput>(source, this.m_settings.DegreeOfParallelism.Value, this.m_preferStriping);
                    recipient.Receive<int>(partitionedStream);
                }
                else if (this.IsIndexible)
                {
                    PartitionedStream<TOutput, int> stream2 = ExchangeUtilities.PartitionDataSource<TOutput>(this, this.m_settings.DegreeOfParallelism.Value, this.m_preferStriping);
                    recipient.Receive<int>(stream2);
                }
                else
                {
                    this.m_childQueryResults.GivePartitionedStream(new ChildResultsRecipient<TInput, TOutput>(recipient, this.m_op, this.m_preferStriping, this.m_settings));
                }
            }

            private class ChildResultsRecipient : IPartitionedStreamRecipient<TInput>
            {
                private UnaryQueryOperator<TInput, TOutput> m_op;
                private IPartitionedStreamRecipient<TOutput> m_outputRecipient;
                private bool m_preferStriping;
                private QuerySettings m_settings;

                internal ChildResultsRecipient(IPartitionedStreamRecipient<TOutput> outputRecipient, UnaryQueryOperator<TInput, TOutput> op, bool preferStriping, QuerySettings settings)
                {
                    this.m_outputRecipient = outputRecipient;
                    this.m_op = op;
                    this.m_preferStriping = preferStriping;
                    this.m_settings = settings;
                }

                public void Receive<TKey>(PartitionedStream<TInput, TKey> inputStream)
                {
                    this.m_op.WrapPartitionedStream<TKey>(inputStream, this.m_outputRecipient, this.m_preferStriping, this.m_settings);
                }
            }
        }
    }
}


namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class BinaryQueryOperator<TLeftInput, TRightInput, TOutput> : QueryOperator<TOutput>
    {
        private System.Linq.Parallel.OrdinalIndexState m_indexState;
        private readonly QueryOperator<TLeftInput> m_leftChild;
        private readonly QueryOperator<TRightInput> m_rightChild;

        internal BinaryQueryOperator(QueryOperator<TLeftInput> leftChild, QueryOperator<TRightInput> rightChild) : base(false, leftChild.SpecifiedQuerySettings.Merge(rightChild.SpecifiedQuerySettings))
        {
            this.m_indexState = System.Linq.Parallel.OrdinalIndexState.Shuffled;
            this.m_leftChild = leftChild;
            this.m_rightChild = rightChild;
        }

        internal BinaryQueryOperator(ParallelQuery<TLeftInput> leftChild, ParallelQuery<TRightInput> rightChild) : this(QueryOperator<TLeftInput>.AsQueryOperator(leftChild), QueryOperator<TRightInput>.AsQueryOperator(rightChild))
        {
        }

        protected void SetOrdinalIndex(System.Linq.Parallel.OrdinalIndexState indexState)
        {
            this.m_indexState = indexState;
        }

        public abstract void WrapPartitionedStream<TLeftKey, TRightKey>(PartitionedStream<TLeftInput, TLeftKey> leftPartitionedStream, PartitionedStream<TRightInput, TRightKey> rightPartitionedStream, IPartitionedStreamRecipient<TOutput> outputRecipient, bool preferStriping, QuerySettings settings);

        internal QueryOperator<TLeftInput> LeftChild
        {
            get
            {
                return this.m_leftChild;
            }
        }

        internal sealed override System.Linq.Parallel.OrdinalIndexState OrdinalIndexState
        {
            get
            {
                return this.m_indexState;
            }
        }

        internal QueryOperator<TRightInput> RightChild
        {
            get
            {
                return this.m_rightChild;
            }
        }

        internal class BinaryQueryOperatorResults : QueryResults<TOutput>
        {
            protected QueryResults<TLeftInput> m_leftChildQueryResults;
            private BinaryQueryOperator<TLeftInput, TRightInput, TOutput> m_op;
            private bool m_preferStriping;
            protected QueryResults<TRightInput> m_rightChildQueryResults;
            private QuerySettings m_settings;

            internal BinaryQueryOperatorResults(QueryResults<TLeftInput> leftChildQueryResults, QueryResults<TRightInput> rightChildQueryResults, BinaryQueryOperator<TLeftInput, TRightInput, TOutput> op, QuerySettings settings, bool preferStriping)
            {
                this.m_leftChildQueryResults = leftChildQueryResults;
                this.m_rightChildQueryResults = rightChildQueryResults;
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
                    this.m_leftChildQueryResults.GivePartitionedStream(new LeftChildResultsRecipient<TLeftInput, TRightInput, TOutput>(recipient, (BinaryQueryOperator<TLeftInput, TRightInput, TOutput>.BinaryQueryOperatorResults) this, this.m_preferStriping, this.m_settings));
                }
            }

            private class LeftChildResultsRecipient : IPartitionedStreamRecipient<TLeftInput>
            {
                private IPartitionedStreamRecipient<TOutput> m_outputRecipient;
                private bool m_preferStriping;
                private BinaryQueryOperator<TLeftInput, TRightInput, TOutput>.BinaryQueryOperatorResults m_results;
                private QuerySettings m_settings;

                internal LeftChildResultsRecipient(IPartitionedStreamRecipient<TOutput> outputRecipient, BinaryQueryOperator<TLeftInput, TRightInput, TOutput>.BinaryQueryOperatorResults results, bool preferStriping, QuerySettings settings)
                {
                    this.m_outputRecipient = outputRecipient;
                    this.m_results = results;
                    this.m_preferStriping = preferStriping;
                    this.m_settings = settings;
                }

                public void Receive<TLeftKey>(PartitionedStream<TLeftInput, TLeftKey> source)
                {
                    BinaryQueryOperator<TLeftInput, TRightInput, TOutput>.BinaryQueryOperatorResults.RightChildResultsRecipient<TLeftKey> recipient = new BinaryQueryOperator<TLeftInput, TRightInput, TOutput>.BinaryQueryOperatorResults.RightChildResultsRecipient<TLeftKey>(this.m_outputRecipient, this.m_results.m_op, source, this.m_preferStriping, this.m_settings);
                    this.m_results.m_rightChildQueryResults.GivePartitionedStream(recipient);
                }
            }

            private class RightChildResultsRecipient<TLeftKey> : IPartitionedStreamRecipient<TRightInput>
            {
                private PartitionedStream<TLeftInput, TLeftKey> m_leftPartitionedStream;
                private BinaryQueryOperator<TLeftInput, TRightInput, TOutput> m_op;
                private IPartitionedStreamRecipient<TOutput> m_outputRecipient;
                private bool m_preferStriping;
                private QuerySettings m_settings;

                internal RightChildResultsRecipient(IPartitionedStreamRecipient<TOutput> outputRecipient, BinaryQueryOperator<TLeftInput, TRightInput, TOutput> op, PartitionedStream<TLeftInput, TLeftKey> leftPartitionedStream, bool preferStriping, QuerySettings settings)
                {
                    this.m_outputRecipient = outputRecipient;
                    this.m_op = op;
                    this.m_preferStriping = preferStriping;
                    this.m_leftPartitionedStream = leftPartitionedStream;
                    this.m_settings = settings;
                }

                public void Receive<TRightKey>(PartitionedStream<TRightInput, TRightKey> rightPartitionedStream)
                {
                    this.m_op.WrapPartitionedStream<TLeftKey, TRightKey>(this.m_leftPartitionedStream, rightPartitionedStream, this.m_outputRecipient, this.m_preferStriping, this.m_settings);
                }
            }
        }
    }
}


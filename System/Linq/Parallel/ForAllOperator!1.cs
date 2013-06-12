namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ForAllOperator<TInput> : UnaryQueryOperator<TInput, TInput>
    {
        private readonly Action<TInput> m_elementAction;

        internal ForAllOperator(IEnumerable<TInput> child, Action<TInput> elementAction) : base(child)
        {
            this.m_elementAction = elementAction;
        }

        internal override IEnumerable<TInput> AsSequentialQuery(CancellationToken token)
        {
            throw new InvalidOperationException();
        }

        internal override QueryResults<TInput> Open(QuerySettings settings, bool preferStriping)
        {
            return new UnaryQueryOperator<TInput, TInput>.UnaryQueryOperatorResults(base.Child.Open(settings, preferStriping), this, settings, preferStriping);
        }

        internal void RunSynchronously()
        {
            Shared<bool> topLevelDisposedFlag = new Shared<bool>(false);
            CancellationTokenSource topLevelCancellationTokenSource = new CancellationTokenSource();
            QuerySettings querySettings = base.SpecifiedQuerySettings.WithPerExecutionSettings(topLevelCancellationTokenSource, topLevelDisposedFlag).WithDefaults();
            QueryLifecycle.LogicalQueryExecutionBegin(querySettings.QueryId);
            base.GetOpenedEnumerator(3, true, true, querySettings);
            querySettings.CleanStateAtQueryEnd();
            QueryLifecycle.LogicalQueryExecutionEnd(querySettings.QueryId);
        }

        internal override void WrapPartitionedStream<TKey>(PartitionedStream<TInput, TKey> inputStream, IPartitionedStreamRecipient<TInput> recipient, bool preferStriping, QuerySettings settings)
        {
            int partitionCount = inputStream.PartitionCount;
            PartitionedStream<TInput, int> partitionedStream = new PartitionedStream<TInput, int>(partitionCount, Util.GetDefaultComparer<int>(), OrdinalIndexState.Correct);
            for (int i = 0; i < partitionCount; i++)
            {
                partitionedStream[i] = new ForAllEnumerator<TInput, TKey>(inputStream[i], this.m_elementAction, settings.CancellationState.MergedCancellationToken);
            }
            recipient.Receive<int>(partitionedStream);
        }

        internal override bool LimitsParallelism
        {
            get
            {
                return false;
            }
        }

        private class ForAllEnumerator<TKey> : QueryOperatorEnumerator<TInput, int>
        {
            private CancellationToken m_cancellationToken;
            private readonly Action<TInput> m_elementAction;
            private readonly QueryOperatorEnumerator<TInput, TKey> m_source;

            internal ForAllEnumerator(QueryOperatorEnumerator<TInput, TKey> source, Action<TInput> elementAction, CancellationToken cancellationToken)
            {
                this.m_source = source;
                this.m_elementAction = elementAction;
                this.m_cancellationToken = cancellationToken;
            }

            protected override void Dispose(bool disposing)
            {
                this.m_source.Dispose();
            }

            internal override bool MoveNext(ref TInput currentElement, ref int currentKey)
            {
                TInput local = default(TInput);
                TKey local2 = default(TKey);
                int num = 0;
                while (this.m_source.MoveNext(ref local, ref local2))
                {
                    if ((num++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    this.m_elementAction(local);
                }
                return false;
            }
        }
    }
}


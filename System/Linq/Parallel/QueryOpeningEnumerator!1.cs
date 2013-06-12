namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class QueryOpeningEnumerator<TOutput> : IEnumerator<TOutput>, IDisposable, IEnumerator
    {
        private bool m_hasQueryOpeningFailed;
        private readonly ParallelMergeOptions? m_mergeOptions;
        private int m_moveNextIteration;
        private IEnumerator<TOutput> m_openedQueryEnumerator;
        private readonly QueryOperator<TOutput> m_queryOperator;
        private QuerySettings m_querySettings;
        private readonly bool m_suppressOrderPreservation;
        private readonly CancellationTokenSource m_topLevelCancellationTokenSource;
        private readonly Shared<bool> m_topLevelDisposedFlag;

        internal QueryOpeningEnumerator(QueryOperator<TOutput> queryOperator, ParallelMergeOptions? mergeOptions, bool suppressOrderPreservation)
        {
            this.m_topLevelDisposedFlag = new Shared<bool>(false);
            this.m_topLevelCancellationTokenSource = new CancellationTokenSource();
            this.m_queryOperator = queryOperator;
            this.m_mergeOptions = mergeOptions;
            this.m_suppressOrderPreservation = suppressOrderPreservation;
        }

        public void Dispose()
        {
            this.m_topLevelDisposedFlag.Value = true;
            this.m_topLevelCancellationTokenSource.Cancel();
            if (this.m_openedQueryEnumerator != null)
            {
                this.m_openedQueryEnumerator.Dispose();
                this.m_querySettings.CleanStateAtQueryEnd();
            }
            QueryLifecycle.LogicalQueryExecutionEnd(this.m_querySettings.QueryId);
        }

        public bool MoveNext()
        {
            if (this.m_topLevelDisposedFlag.Value)
            {
                throw new ObjectDisposedException("enumerator", System.Linq.SR.GetString("PLINQ_DisposeRequested"));
            }
            if (this.m_openedQueryEnumerator == null)
            {
                this.OpenQuery();
            }
            bool flag = this.m_openedQueryEnumerator.MoveNext();
            if ((this.m_moveNextIteration & 0x3f) == 0)
            {
                CancellationState.ThrowWithStandardMessageIfCanceled(this.m_querySettings.CancellationState.ExternalCancellationToken);
            }
            this.m_moveNextIteration++;
            return flag;
        }

        private void OpenQuery()
        {
            if (this.m_hasQueryOpeningFailed)
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("PLINQ_EnumerationPreviouslyFailed"));
            }
            try
            {
                this.m_querySettings = this.m_queryOperator.SpecifiedQuerySettings.WithPerExecutionSettings(this.m_topLevelCancellationTokenSource, this.m_topLevelDisposedFlag).WithDefaults();
                QueryLifecycle.LogicalQueryExecutionBegin(this.m_querySettings.QueryId);
                this.m_openedQueryEnumerator = this.m_queryOperator.GetOpenedEnumerator(this.m_mergeOptions, this.m_suppressOrderPreservation, false, this.m_querySettings);
                CancellationState.ThrowWithStandardMessageIfCanceled(this.m_querySettings.CancellationState.ExternalCancellationToken);
            }
            catch
            {
                this.m_hasQueryOpeningFailed = true;
                throw;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public TOutput Current
        {
            get
            {
                if (this.m_openedQueryEnumerator == null)
                {
                    throw new InvalidOperationException(System.Linq.SR.GetString("PLINQ_CommonEnumerator_Current_NotStarted"));
                }
                return this.m_openedQueryEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }
    }
}


namespace System.Linq.Parallel
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    [StructLayout(LayoutKind.Sequential)]
    internal struct QuerySettings
    {
        private System.Threading.Tasks.TaskScheduler m_taskScheduler;
        private int? m_degreeOfParallelism;
        private System.Linq.Parallel.CancellationState m_cancellationState;
        private ParallelExecutionMode? m_executionMode;
        private ParallelMergeOptions? m_mergeOptions;
        private int m_queryId;
        internal System.Linq.Parallel.CancellationState CancellationState
        {
            get
            {
                return this.m_cancellationState;
            }
            set
            {
                this.m_cancellationState = value;
            }
        }
        internal System.Threading.Tasks.TaskScheduler TaskScheduler
        {
            get
            {
                return this.m_taskScheduler;
            }
            set
            {
                this.m_taskScheduler = value;
            }
        }
        internal int? DegreeOfParallelism
        {
            get
            {
                return this.m_degreeOfParallelism;
            }
            set
            {
                this.m_degreeOfParallelism = value;
            }
        }
        internal ParallelExecutionMode? ExecutionMode
        {
            get
            {
                return this.m_executionMode;
            }
            set
            {
                this.m_executionMode = value;
            }
        }
        internal ParallelMergeOptions? MergeOptions
        {
            get
            {
                return this.m_mergeOptions;
            }
            set
            {
                this.m_mergeOptions = value;
            }
        }
        internal int QueryId
        {
            get
            {
                return this.m_queryId;
            }
        }
        internal QuerySettings(System.Threading.Tasks.TaskScheduler taskScheduler, int? degreeOfParallelism, CancellationToken externalCancellationToken, ParallelExecutionMode? executionMode, ParallelMergeOptions? mergeOptions)
        {
            this.m_taskScheduler = taskScheduler;
            this.m_degreeOfParallelism = degreeOfParallelism;
            this.m_cancellationState = new System.Linq.Parallel.CancellationState(externalCancellationToken);
            this.m_executionMode = executionMode;
            this.m_mergeOptions = mergeOptions;
            this.m_queryId = -1;
        }

        internal QuerySettings Merge(QuerySettings settings2)
        {
            if ((this.TaskScheduler != null) && (settings2.TaskScheduler != null))
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_DuplicateTaskScheduler"));
            }
            if (this.DegreeOfParallelism.HasValue && settings2.DegreeOfParallelism.HasValue)
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_DuplicateDOP"));
            }
            if (this.CancellationState.ExternalCancellationToken.CanBeCanceled && settings2.CancellationState.ExternalCancellationToken.CanBeCanceled)
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_DuplicateWithCancellation"));
            }
            if (this.ExecutionMode.HasValue && settings2.ExecutionMode.HasValue)
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_DuplicateExecutionMode"));
            }
            if (this.MergeOptions.HasValue && settings2.MergeOptions.HasValue)
            {
                throw new InvalidOperationException(System.Linq.SR.GetString("ParallelQuery_DuplicateMergeOptions"));
            }
            System.Threading.Tasks.TaskScheduler taskScheduler = (this.TaskScheduler == null) ? settings2.TaskScheduler : this.TaskScheduler;
            int? degreeOfParallelism = this.DegreeOfParallelism.HasValue ? this.DegreeOfParallelism : settings2.DegreeOfParallelism;
            CancellationToken externalCancellationToken = this.CancellationState.ExternalCancellationToken.CanBeCanceled ? this.CancellationState.ExternalCancellationToken : settings2.CancellationState.ExternalCancellationToken;
            ParallelExecutionMode? executionMode = this.ExecutionMode.HasValue ? this.ExecutionMode : settings2.ExecutionMode;
            return new QuerySettings(taskScheduler, degreeOfParallelism, externalCancellationToken, executionMode, this.MergeOptions.HasValue ? this.MergeOptions : settings2.MergeOptions);
        }

        internal QuerySettings WithPerExecutionSettings()
        {
            return this.WithPerExecutionSettings(new CancellationTokenSource(), new System.Linq.Parallel.Shared<bool>(false));
        }

        internal QuerySettings WithPerExecutionSettings(CancellationTokenSource topLevelCancellationTokenSource, System.Linq.Parallel.Shared<bool> topLevelDisposedFlag)
        {
            QuerySettings settings = new QuerySettings(this.TaskScheduler, this.DegreeOfParallelism, this.CancellationState.ExternalCancellationToken, this.ExecutionMode, this.MergeOptions);
            settings.CancellationState.InternalCancellationTokenSource = topLevelCancellationTokenSource;
            settings.CancellationState.MergedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(settings.CancellationState.InternalCancellationTokenSource.Token, settings.CancellationState.ExternalCancellationToken);
            settings.CancellationState.TopLevelDisposedFlag = topLevelDisposedFlag;
            settings.m_queryId = PlinqEtwProvider.NextQueryId();
            return settings;
        }

        internal QuerySettings WithDefaults()
        {
            QuerySettings settings = this;
            if (settings.TaskScheduler == null)
            {
                settings.TaskScheduler = System.Threading.Tasks.TaskScheduler.Default;
            }
            if (!settings.DegreeOfParallelism.HasValue)
            {
                settings.DegreeOfParallelism = new int?(Scheduling.GetDefaultDegreeOfParallelism());
            }
            if (!settings.ExecutionMode.HasValue)
            {
                settings.ExecutionMode = 0;
            }
            if (!settings.MergeOptions.HasValue)
            {
                settings.MergeOptions = 0;
            }
            if (((ParallelMergeOptions) settings.MergeOptions) == ParallelMergeOptions.Default)
            {
                settings.MergeOptions = 2;
            }
            return settings;
        }

        internal static QuerySettings Empty
        {
            get
            {
                return new QuerySettings(null, null, new CancellationToken(), null, null);
            }
        }
        public void CleanStateAtQueryEnd()
        {
            this.m_cancellationState.MergedCancellationTokenSource.Dispose();
        }
    }
}


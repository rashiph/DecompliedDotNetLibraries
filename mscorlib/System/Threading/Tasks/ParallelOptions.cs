namespace System.Threading.Tasks
{
    using System;
    using System.Threading;

    public class ParallelOptions
    {
        private System.Threading.CancellationToken m_cancellationToken = System.Threading.CancellationToken.None;
        private int m_maxDegreeOfParallelism = -1;
        private System.Threading.Tasks.TaskScheduler m_scheduler = System.Threading.Tasks.TaskScheduler.Default;

        public System.Threading.CancellationToken CancellationToken
        {
            get
            {
                return this.m_cancellationToken;
            }
            set
            {
                this.m_cancellationToken = value;
            }
        }

        internal int EffectiveMaxConcurrencyLevel
        {
            get
            {
                int maxDegreeOfParallelism = this.MaxDegreeOfParallelism;
                int maximumConcurrencyLevel = this.EffectiveTaskScheduler.MaximumConcurrencyLevel;
                if ((maximumConcurrencyLevel > 0) && (maximumConcurrencyLevel != 0x7fffffff))
                {
                    maxDegreeOfParallelism = (maxDegreeOfParallelism == -1) ? maximumConcurrencyLevel : Math.Min(maximumConcurrencyLevel, maxDegreeOfParallelism);
                }
                return maxDegreeOfParallelism;
            }
        }

        internal System.Threading.Tasks.TaskScheduler EffectiveTaskScheduler
        {
            get
            {
                if (this.m_scheduler == null)
                {
                    return System.Threading.Tasks.TaskScheduler.Current;
                }
                return this.m_scheduler;
            }
        }

        public int MaxDegreeOfParallelism
        {
            get
            {
                return this.m_maxDegreeOfParallelism;
            }
            set
            {
                if ((value == 0) || (value < -1))
                {
                    throw new ArgumentOutOfRangeException("MaxDegreeOfParallelism");
                }
                this.m_maxDegreeOfParallelism = value;
            }
        }

        public System.Threading.Tasks.TaskScheduler TaskScheduler
        {
            get
            {
                return this.m_scheduler;
            }
            set
            {
                this.m_scheduler = value;
            }
        }
    }
}


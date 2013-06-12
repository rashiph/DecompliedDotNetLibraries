namespace System.Linq.Parallel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal abstract class MergeEnumerator<TInputOutput> : IEnumerator<TInputOutput>, IDisposable, IEnumerator
    {
        protected QueryTaskGroupState m_taskGroupState;

        protected MergeEnumerator(QueryTaskGroupState taskGroupState)
        {
            this.m_taskGroupState = taskGroupState;
        }

        public virtual void Dispose()
        {
            if (!this.m_taskGroupState.IsAlreadyEnded)
            {
                this.m_taskGroupState.QueryEnd(true);
            }
        }

        public abstract bool MoveNext();
        public virtual void Reset()
        {
        }

        public abstract TInputOutput Current { get; }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }
    }
}


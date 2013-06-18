namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;
    using System.Threading;

    internal class DbRetry
    {
        private const short _defaultMaxRetries = 20;
        private const int _defaultRetrySleep = 0x7d0;
        private bool _enableRetries;
        private short _maxRetries;
        private int _retrySleep;
        private const short _spinCount = 3;

        protected DbRetry()
        {
            this._maxRetries = 20;
            this._retrySleep = 0x7d0;
        }

        internal DbRetry(bool enableRetries)
        {
            this._maxRetries = 20;
            this._retrySleep = 0x7d0;
            this._enableRetries = enableRetries;
        }

        internal bool CanRetry(short retryCount)
        {
            if (!this._enableRetries)
            {
                return false;
            }
            return (retryCount < this._maxRetries);
        }

        internal void RetrySleep(short retryCount)
        {
            if (retryCount > 3)
            {
                int millisecondsTimeout = this._retrySleep * retryCount;
                Thread.Sleep(millisecondsTimeout);
            }
        }

        internal bool TryDoRetry(ref short retryCount)
        {
            short num;
            retryCount = (short) ((num = retryCount) + 1);
            if (this.CanRetry(num))
            {
                this.RetrySleep(retryCount);
                return true;
            }
            return false;
        }

        internal short MaxRetries
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._maxRetries;
            }
        }
    }
}


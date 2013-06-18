namespace System.Runtime.Caching
{
    using System;
    using System.Runtime.Caching.Resources;
    using System.Threading;

    public abstract class ChangeMonitor : IDisposable
    {
        private SafeBitVector32 _flags;
        private OnChangedCallback _onChangedCallback;
        private object _onChangedState = NOT_SET;
        private const int CHANGED = 2;
        private const int DISPOSED = 8;
        private const int INITIALIZED = 1;
        private const int INVOKED = 4;
        private static readonly object NOT_SET = new object();

        protected ChangeMonitor()
        {
        }

        public void Dispose()
        {
            this.OnChangedHelper(null);
            if (!this._flags[1])
            {
                throw new InvalidOperationException(R.Init_not_complete);
            }
            this.DisposeHelper();
        }

        protected abstract void Dispose(bool disposing);
        private void DisposeHelper()
        {
            if (this._flags[1] && this._flags.ChangeValue(8, true))
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected void InitializationComplete()
        {
            this._flags[1] = true;
            if (this._flags[2])
            {
                this.Dispose();
            }
        }

        public void NotifyOnChanged(OnChangedCallback onChangedCallback)
        {
            if (onChangedCallback == null)
            {
                throw new ArgumentNullException("onChangedCallback");
            }
            if (Interlocked.CompareExchange<OnChangedCallback>(ref this._onChangedCallback, onChangedCallback, null) != null)
            {
                throw new InvalidOperationException(R.Method_already_invoked);
            }
            if (this._flags[2])
            {
                this.OnChanged(null);
            }
        }

        protected void OnChanged(object state)
        {
            this.OnChangedHelper(state);
            if (this._flags[1])
            {
                this.DisposeHelper();
            }
        }

        private void OnChangedHelper(object state)
        {
            this._flags[2] = true;
            Interlocked.CompareExchange(ref this._onChangedState, state, NOT_SET);
            OnChangedCallback callback = this._onChangedCallback;
            if ((callback != null) && this._flags.ChangeValue(4, true))
            {
                callback(this._onChangedState);
            }
        }

        public bool HasChanged
        {
            get
            {
                return this._flags[2];
            }
        }

        public bool IsDisposed
        {
            get
            {
                return this._flags[8];
            }
        }

        public abstract string UniqueId { get; }
    }
}


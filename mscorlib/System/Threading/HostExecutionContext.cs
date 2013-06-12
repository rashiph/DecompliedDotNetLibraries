namespace System.Threading
{
    using System;
    using System.Security;

    public class HostExecutionContext : IDisposable
    {
        private object state;

        public HostExecutionContext()
        {
        }

        public HostExecutionContext(object state)
        {
            this.state = state;
        }

        [SecuritySafeCritical]
        public virtual HostExecutionContext CreateCopy()
        {
            if (this.state is IUnknownSafeHandle)
            {
                ((IUnknownSafeHandle) this.state).Clone();
            }
            return new HostExecutionContext(this.state);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
        }

        protected internal object State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }
    }
}


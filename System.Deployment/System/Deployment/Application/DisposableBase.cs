namespace System.Deployment.Application
{
    using System;

    internal abstract class DisposableBase : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.DisposeManagedResources();
                }
                this.DisposeUnmanagedResources();
            }
            this._disposed = true;
        }

        protected virtual void DisposeManagedResources()
        {
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        ~DisposableBase()
        {
            this.Dispose(false);
        }
    }
}


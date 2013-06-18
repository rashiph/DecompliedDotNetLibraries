namespace System.Web.Configuration
{
    using System;
    using System.Web;

    internal sealed class ImpersonateTokenRef : IDisposable
    {
        private IntPtr _handle;

        internal ImpersonateTokenRef(IntPtr token)
        {
            this._handle = token;
        }

        ~ImpersonateTokenRef()
        {
            if (this._handle != IntPtr.Zero)
            {
                UnsafeNativeMethods.CloseHandle(this._handle);
                this._handle = IntPtr.Zero;
            }
        }

        void IDisposable.Dispose()
        {
            if (this._handle != IntPtr.Zero)
            {
                UnsafeNativeMethods.CloseHandle(this._handle);
                this._handle = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }

        internal IntPtr Handle
        {
            get
            {
                return this._handle;
            }
        }
    }
}


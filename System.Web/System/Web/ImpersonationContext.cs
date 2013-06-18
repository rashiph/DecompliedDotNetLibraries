namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;

    internal class ImpersonationContext : IDisposable
    {
        private bool _impersonating;
        private bool _reverted;
        private HandleRef _savedToken;

        internal ImpersonationContext()
        {
        }

        internal ImpersonationContext(IntPtr token)
        {
            this.ImpersonateToken(new HandleRef(this, token));
        }

        private void Dispose(bool disposing)
        {
            if (this._savedToken.Handle != IntPtr.Zero)
            {
                try
                {
                }
                finally
                {
                    UnsafeNativeMethods.CloseHandle(this._savedToken.Handle);
                    this._savedToken = new HandleRef(this, IntPtr.Zero);
                }
            }
        }

        ~ImpersonationContext()
        {
            this.Dispose(false);
        }

        private static IntPtr GetCurrentToken()
        {
            IntPtr zero = IntPtr.Zero;
            if ((UnsafeNativeMethods.OpenThreadToken(UnsafeNativeMethods.GetCurrentThread(), 0x2000c, true, ref zero) == 0) && (Marshal.GetLastWin32Error() != 0x3f0))
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_impersonate"));
            }
            return zero;
        }

        protected void ImpersonateToken(HandleRef token)
        {
            try
            {
                this._savedToken = new HandleRef(this, GetCurrentToken());
                if ((this._savedToken.Handle != IntPtr.Zero) && (UnsafeNativeMethods.RevertToSelf() != 0))
                {
                    this._reverted = true;
                }
                if (token.Handle != IntPtr.Zero)
                {
                    if (UnsafeNativeMethods.SetThreadToken(IntPtr.Zero, token.Handle) == 0)
                    {
                        throw new HttpException(System.Web.SR.GetString("Cannot_impersonate"));
                    }
                    this._impersonating = true;
                }
            }
            catch
            {
                this.RestoreImpersonation();
                throw;
            }
        }

        private void RestoreImpersonation()
        {
            if (this._impersonating)
            {
                UnsafeNativeMethods.RevertToSelf();
                this._impersonating = false;
            }
            if (this._savedToken.Handle != IntPtr.Zero)
            {
                if (this._reverted && (UnsafeNativeMethods.SetThreadToken(IntPtr.Zero, this._savedToken.Handle) == 0))
                {
                    throw new HttpException(System.Web.SR.GetString("Cannot_impersonate"));
                }
                this._reverted = false;
            }
        }

        void IDisposable.Dispose()
        {
            this.Undo();
        }

        internal void Undo()
        {
            this.RestoreImpersonation();
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static bool CurrentThreadTokenExists
        {
            get
            {
                bool flag = false;
                try
                {
                }
                finally
                {
                    IntPtr currentToken = GetCurrentToken();
                    if (currentToken != IntPtr.Zero)
                    {
                        flag = true;
                        UnsafeNativeMethods.CloseHandle(currentToken);
                    }
                }
                return flag;
            }
        }
    }
}


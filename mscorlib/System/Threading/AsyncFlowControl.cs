namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    public struct AsyncFlowControl : IDisposable
    {
        private bool useEC;
        private ExecutionContext _ec;
        private SecurityContext _sc;
        private Thread _thread;
        internal void Setup(SecurityContextDisableFlow flags)
        {
            this.useEC = false;
            this._sc = Thread.CurrentThread.ExecutionContext.SecurityContext;
            this._sc._disableFlow = flags;
            this._thread = Thread.CurrentThread;
        }

        internal void Setup()
        {
            this.useEC = true;
            this._ec = Thread.CurrentThread.ExecutionContext;
            this._ec.isFlowSuppressed = true;
            this._thread = Thread.CurrentThread;
        }

        public void Dispose()
        {
            this.Undo();
        }

        public void Undo()
        {
            if (this._thread == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseAFCMultiple"));
            }
            if (this._thread != Thread.CurrentThread)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseAFCOtherThread"));
            }
            if (this.useEC)
            {
                if (Thread.CurrentThread.ExecutionContext != this._ec)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AsyncFlowCtrlCtxMismatch"));
                }
                ExecutionContext.RestoreFlow();
            }
            else
            {
                if (Thread.CurrentThread.ExecutionContext.SecurityContext != this._sc)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AsyncFlowCtrlCtxMismatch"));
                }
                SecurityContext.RestoreFlow();
            }
            this._thread = null;
        }

        public override int GetHashCode()
        {
            if (this._thread != null)
            {
                return this._thread.GetHashCode();
            }
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ((obj is AsyncFlowControl) && this.Equals((AsyncFlowControl) obj));
        }

        public bool Equals(AsyncFlowControl obj)
        {
            return ((((obj.useEC == this.useEC) && (obj._ec == this._ec)) && (obj._sc == this._sc)) && (obj._thread == this._thread));
        }

        public static bool operator ==(AsyncFlowControl a, AsyncFlowControl b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(AsyncFlowControl a, AsyncFlowControl b)
        {
            return !(a == b);
        }
    }
}


namespace System.Threading
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SynchronizationContextSwitcher : IDisposable
    {
        internal SynchronizationContext savedSC;
        internal SynchronizationContext currSC;
        internal ExecutionContext _ec;
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is SynchronizationContextSwitcher))
            {
                return false;
            }
            SynchronizationContextSwitcher switcher = (SynchronizationContextSwitcher) obj;
            return (((this.savedSC == switcher.savedSC) && (this.currSC == switcher.currSC)) && (this._ec == switcher._ec));
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(SynchronizationContextSwitcher c1, SynchronizationContextSwitcher c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(SynchronizationContextSwitcher c1, SynchronizationContextSwitcher c2)
        {
            return !c1.Equals(c2);
        }

        public void Dispose()
        {
            this.Undo();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), HandleProcessCorruptedStateExceptions, SecuritySafeCritical]
        internal bool UndoNoThrow()
        {
            if (this._ec != null)
            {
                try
                {
                    this.Undo();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public void Undo()
        {
            if (this._ec != null)
            {
                ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
                if (this._ec != executionContextNoCreate)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
                }
                if (this.currSC != this._ec.SynchronizationContext)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
                }
                executionContextNoCreate.SynchronizationContext = this.savedSC;
                this._ec = null;
            }
        }
    }
}


namespace System.Threading
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExecutionContextSwitcher : IDisposable
    {
        internal ExecutionContext prevEC;
        internal ExecutionContext currEC;
        internal SecurityContextSwitcher scsw;
        internal SynchronizationContextSwitcher sysw;
        internal object hecsw;
        internal Thread thread;
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is ExecutionContextSwitcher))
            {
                return false;
            }
            ExecutionContextSwitcher switcher = (ExecutionContextSwitcher) obj;
            return (((((this.prevEC == switcher.prevEC) && (this.currEC == switcher.currEC)) && ((this.scsw == switcher.scsw) && (this.sysw == switcher.sysw))) && (this.hecsw == switcher.hecsw)) && (this.thread == switcher.thread));
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(ExecutionContextSwitcher c1, ExecutionContextSwitcher c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(ExecutionContextSwitcher c1, ExecutionContextSwitcher c2)
        {
            return !c1.Equals(c2);
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Undo();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical, HandleProcessCorruptedStateExceptions]
        internal bool UndoNoThrow()
        {
            try
            {
                this.Undo();
            }
            catch
            {
                return false;
            }
            return true;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public void Undo()
        {
            if (this.thread != null)
            {
                if (this.thread != Thread.CurrentThread)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
                }
                if (this.currEC != Thread.CurrentThread.GetExecutionContextNoCreate())
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
                }
                this.scsw.Undo();
                try
                {
                    HostExecutionContextSwitcher.Undo(this.hecsw);
                }
                finally
                {
                    this.sysw.Undo();
                }
                Thread.CurrentThread.SetExecutionContext(this.prevEC);
                this.thread = null;
            }
        }
    }
}


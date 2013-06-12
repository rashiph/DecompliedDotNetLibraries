namespace System.Security
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityContextSwitcher : IDisposable
    {
        internal SecurityContext prevSC;
        internal SecurityContext currSC;
        internal ExecutionContext currEC;
        internal CompressedStackSwitcher cssw;
        internal WindowsImpersonationContext wic;
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is SecurityContextSwitcher))
            {
                return false;
            }
            SecurityContextSwitcher switcher = (SecurityContextSwitcher) obj;
            return ((((this.prevSC == switcher.prevSC) && (this.currSC == switcher.currSC)) && ((this.currEC == switcher.currEC) && (this.cssw == switcher.cssw))) && (this.wic == switcher.wic));
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(SecurityContextSwitcher c1, SecurityContextSwitcher c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(SecurityContextSwitcher c1, SecurityContextSwitcher c2)
        {
            return !c1.Equals(c2);
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Undo();
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), HandleProcessCorruptedStateExceptions]
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

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), HandleProcessCorruptedStateExceptions]
        public void Undo()
        {
            if (this.currEC != null)
            {
                if (this.currEC != Thread.CurrentThread.GetExecutionContextNoCreate())
                {
                    Environment.FailFast(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
                }
                if (this.currSC != this.currEC.SecurityContext)
                {
                    Environment.FailFast(Environment.GetResourceString("InvalidOperation_SwitcherCtxMismatch"));
                }
                this.currEC.SecurityContext = this.prevSC;
                this.currEC = null;
                bool flag = true;
                try
                {
                    if (this.wic != null)
                    {
                        flag &= this.wic.UndoNoThrow();
                    }
                }
                catch
                {
                    flag &= this.cssw.UndoNoThrow();
                    Environment.FailFast(Environment.GetResourceString("ExecutionContext_UndoFailed"));
                }
                if (!(flag & this.cssw.UndoNoThrow()))
                {
                    Environment.FailFast(Environment.GetResourceString("ExecutionContext_UndoFailed"));
                }
            }
        }
    }
}


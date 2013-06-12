namespace System.Threading
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CompressedStackSwitcher : IDisposable
    {
        internal CompressedStack curr_CS;
        internal CompressedStack prev_CS;
        internal IntPtr prev_ADStack;
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CompressedStackSwitcher))
            {
                return false;
            }
            CompressedStackSwitcher switcher = (CompressedStackSwitcher) obj;
            return (((this.curr_CS == switcher.curr_CS) && (this.prev_CS == switcher.prev_CS)) && (this.prev_ADStack == switcher.prev_ADStack));
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(CompressedStackSwitcher c1, CompressedStackSwitcher c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(CompressedStackSwitcher c1, CompressedStackSwitcher c2)
        {
            return !c1.Equals(c2);
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Undo();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), HandleProcessCorruptedStateExceptions, SecurityCritical]
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
            if ((this.curr_CS != null) || (this.prev_CS != null))
            {
                if (this.prev_ADStack != IntPtr.Zero)
                {
                    CompressedStack.RestoreAppDomainStack(this.prev_ADStack);
                }
                CompressedStack.SetCompressedStackThread(this.prev_CS);
                this.prev_CS = null;
                this.curr_CS = null;
                this.prev_ADStack = IntPtr.Zero;
            }
        }
    }
}


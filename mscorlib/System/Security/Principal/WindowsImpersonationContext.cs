namespace System.Security.Principal
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class WindowsImpersonationContext : IDisposable
    {
        private FrameSecurityDescriptor m_fsd;
        [SecurityCritical]
        private SafeTokenHandle m_safeTokenHandle;
        private WindowsIdentity m_wi;

        [SecurityCritical]
        private WindowsImpersonationContext()
        {
            this.m_safeTokenHandle = SafeTokenHandle.InvalidHandle;
        }

        [SecurityCritical]
        internal WindowsImpersonationContext(SafeTokenHandle safeTokenHandle, WindowsIdentity wi, bool isImpersonating, FrameSecurityDescriptor fsd)
        {
            this.m_safeTokenHandle = SafeTokenHandle.InvalidHandle;
            if (safeTokenHandle.IsInvalid)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
            }
            if (isImpersonating)
            {
                if (!Win32Native.DuplicateHandle(Win32Native.GetCurrentProcess(), safeTokenHandle, Win32Native.GetCurrentProcess(), ref this.m_safeTokenHandle, 0, true, 2))
                {
                    throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
                }
                this.m_wi = wi;
            }
            this.m_fsd = fsd;
        }

        [ComVisible(false), SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
        }

        [SecuritySafeCritical, ComVisible(false)]
        protected virtual void Dispose(bool disposing)
        {
            if ((disposing && (this.m_safeTokenHandle != null)) && !this.m_safeTokenHandle.IsClosed)
            {
                this.Undo();
                this.m_safeTokenHandle.Dispose();
            }
        }

        [SecuritySafeCritical]
        public void Undo()
        {
            int errorCode = 0;
            if (this.m_safeTokenHandle.IsInvalid)
            {
                errorCode = Win32.RevertToSelf();
                if (errorCode < 0)
                {
                    Environment.FailFast(Win32Native.GetMessage(errorCode));
                }
            }
            else
            {
                errorCode = Win32.RevertToSelf();
                if (errorCode < 0)
                {
                    Environment.FailFast(Win32Native.GetMessage(errorCode));
                }
                errorCode = Win32.ImpersonateLoggedOnUser(this.m_safeTokenHandle);
                if (errorCode < 0)
                {
                    throw new SecurityException(Win32Native.GetMessage(errorCode));
                }
            }
            WindowsIdentity.UpdateThreadWI(this.m_wi);
            if (this.m_fsd != null)
            {
                this.m_fsd.SetTokenHandles(null, null);
            }
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal bool UndoNoThrow()
        {
            bool flag = false;
            try
            {
                int errorCode = 0;
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    errorCode = Win32.RevertToSelf();
                    if (errorCode < 0)
                    {
                        Environment.FailFast(Win32Native.GetMessage(errorCode));
                    }
                }
                else
                {
                    errorCode = Win32.RevertToSelf();
                    if (errorCode >= 0)
                    {
                        errorCode = Win32.ImpersonateLoggedOnUser(this.m_safeTokenHandle);
                    }
                    else
                    {
                        Environment.FailFast(Win32Native.GetMessage(errorCode));
                    }
                }
                flag = errorCode >= 0;
                if (this.m_fsd != null)
                {
                    this.m_fsd.SetTokenHandles(null, null);
                }
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
    }
}


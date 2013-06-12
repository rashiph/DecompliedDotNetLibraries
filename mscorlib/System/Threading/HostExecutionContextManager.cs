namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    public class HostExecutionContextManager
    {
        private static bool _fIsHosted;
        private static bool _fIsHostedChecked;
        private static HostExecutionContextManager _hostExecutionContextManager;

        [SecuritySafeCritical]
        public virtual HostExecutionContext Capture()
        {
            HostExecutionContext context = null;
            if (CheckIfHosted())
            {
                IUnknownSafeHandle state = new IUnknownSafeHandle();
                context = new HostExecutionContext(state);
                CaptureHostSecurityContext(state);
            }
            return context;
        }

        [SecurityCritical]
        internal static HostExecutionContext CaptureHostExecutionContext()
        {
            HostExecutionContext context = null;
            HostExecutionContextManager currentHostExecutionContextManager = GetCurrentHostExecutionContextManager();
            if (currentHostExecutionContextManager != null)
            {
                context = currentHostExecutionContextManager.Capture();
            }
            return context;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int CaptureHostSecurityContext(SafeHandle capturedContext);
        [SecurityCritical]
        internal static bool CheckIfHosted()
        {
            if (!_fIsHostedChecked)
            {
                _fIsHosted = HostSecurityManagerPresent();
                _fIsHostedChecked = true;
            }
            return _fIsHosted;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int CloneHostSecurityContext(SafeHandle context, SafeHandle clonedContext);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        internal static HostExecutionContextManager GetCurrentHostExecutionContextManager()
        {
            if (AppDomainManager.CurrentAppDomainManager != null)
            {
                return AppDomainManager.CurrentAppDomainManager.HostExecutionContextManager;
            }
            return null;
        }

        internal static HostExecutionContextManager GetInternalHostExecutionContextManager()
        {
            if (_hostExecutionContextManager == null)
            {
                _hostExecutionContextManager = new HostExecutionContextManager();
            }
            return _hostExecutionContextManager;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool HostSecurityManagerPresent();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int ReleaseHostSecurityContext(IntPtr context);
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public virtual void Revert(object previousState)
        {
            HostExecutionContextSwitcher switcher = previousState as HostExecutionContextSwitcher;
            if (switcher == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotOverrideSetWithoutRevert"));
            }
            ExecutionContext executionContext = Thread.CurrentThread.ExecutionContext;
            if (executionContext != switcher.executionContext)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
            }
            switcher.executionContext = null;
            if (executionContext.HostExecutionContext != switcher.currentHostContext)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
            }
            HostExecutionContext previousHostContext = switcher.previousHostContext;
            if ((CheckIfHosted() && (previousHostContext != null)) && (previousHostContext.State is IUnknownSafeHandle))
            {
                IUnknownSafeHandle state = (IUnknownSafeHandle) previousHostContext.State;
                SetHostSecurityContext(state, false, null);
            }
            executionContext.HostExecutionContext = previousHostContext;
        }

        [SecurityCritical]
        public virtual object SetHostExecutionContext(HostExecutionContext hostExecutionContext)
        {
            if (hostExecutionContext == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotNewCaptureContext"));
            }
            HostExecutionContextSwitcher switcher = new HostExecutionContextSwitcher();
            ExecutionContext executionContext = Thread.CurrentThread.ExecutionContext;
            switcher.executionContext = executionContext;
            switcher.currentHostContext = hostExecutionContext;
            switcher.previousHostContext = null;
            if (CheckIfHosted() && (hostExecutionContext.State is IUnknownSafeHandle))
            {
                IUnknownSafeHandle state = new IUnknownSafeHandle();
                switcher.previousHostContext = new HostExecutionContext(state);
                IUnknownSafeHandle context = (IUnknownSafeHandle) hostExecutionContext.State;
                SetHostSecurityContext(context, true, state);
            }
            executionContext.HostExecutionContext = hostExecutionContext;
            return switcher;
        }

        [SecurityCritical]
        internal static object SetHostExecutionContextInternal(HostExecutionContext hostContext)
        {
            HostExecutionContextManager currentHostExecutionContextManager = GetCurrentHostExecutionContextManager();
            object obj2 = null;
            if (currentHostExecutionContextManager != null)
            {
                obj2 = currentHostExecutionContextManager.SetHostExecutionContext(hostContext);
            }
            return obj2;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern int SetHostSecurityContext(SafeHandle context, bool fReturnPrevious, SafeHandle prevContext);
    }
}


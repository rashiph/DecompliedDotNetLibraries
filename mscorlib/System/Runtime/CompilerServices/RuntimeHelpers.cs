namespace System.Runtime.CompilerServices
{
    using System;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public static class RuntimeHelpers
    {
        private static TryCode s_EnterMonitor = new TryCode(RuntimeHelpers.EnterMonitorAndTryCode);
        private static CleanupCode s_ExitMonitor = new CleanupCode(RuntimeHelpers.ExitMonitorOnBackout);

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void _CompileMethod(IRuntimeMethodInfo method);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _PrepareMethod(IRuntimeMethodInfo method, IntPtr* pInstantiation, int cInstantiation);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern void _RunClassConstructor(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern void _RunModuleConstructor(RuntimeModule module);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern void EnsureSufficientExecutionStack();
        [SecuritySafeCritical]
        private static void EnterMonitorAndTryCode(object helper)
        {
            ExecuteWithLockHelper helper2 = (ExecuteWithLockHelper) helper;
            Monitor.Enter(helper2.m_lockObject, ref helper2.m_tookLock);
            helper2.m_userCode(helper2.m_userState);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern bool Equals(object o1, object o2);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), PrePrepareMethod]
        internal static void ExecuteBackoutCodeHelper(object backoutCode, object userData, bool exceptionThrown)
        {
            ((CleanupCode) backoutCode)(userData, exceptionThrown);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void ExecuteCodeWithGuaranteedCleanup(TryCode code, CleanupCode backoutCode, object userData);
        [SecurityCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        internal static void ExecuteCodeWithLock(object lockObject, TryCode code, object userState)
        {
            ExecuteWithLockHelper userData = new ExecuteWithLockHelper(lockObject, code, userState);
            ExecuteCodeWithGuaranteedCleanup(s_EnterMonitor, s_ExitMonitor, userData);
        }

        [PrePrepareMethod]
        private static void ExitMonitorOnBackout(object helper, bool exceptionThrown)
        {
            ExecuteWithLockHelper helper2 = (ExecuteWithLockHelper) helper;
            if (helper2.m_tookLock)
            {
                Monitor.Exit(helper2.m_lockObject);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern int GetHashCode(object o);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern object GetObjectValue(object obj);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern void InitializeArray(Array array, RuntimeFieldHandle fldHandle);
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void PrepareConstrainedRegions()
        {
            ProbeForSufficientStack();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void PrepareConstrainedRegionsNoOP()
        {
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void PrepareContractedDelegate(Delegate d);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern void PrepareDelegate(Delegate d);
        [SecurityCritical]
        public static void PrepareMethod(RuntimeMethodHandle method)
        {
            _PrepareMethod(method.GetMethodInfo(), null, 0);
        }

        [SecurityCritical]
        public static unsafe void PrepareMethod(RuntimeMethodHandle method, RuntimeTypeHandle[] instantiation)
        {
            int num;
            fixed (IntPtr* ptrRef = RuntimeTypeHandle.CopyRuntimeTypeHandles(instantiation, out num))
            {
                _PrepareMethod(method.GetMethodInfo(), ptrRef, num);
                GC.KeepAlive(instantiation);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        public static extern void ProbeForSufficientStack();
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void RunClassConstructor(RuntimeTypeHandle type)
        {
            _RunClassConstructor(type.GetRuntimeType());
        }

        public static void RunModuleConstructor(ModuleHandle module)
        {
            _RunModuleConstructor(module.GetRuntimeModule());
        }

        public static int OffsetToStringData
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return 8;
            }
        }

        public delegate void CleanupCode(object userData, bool exceptionThrown);

        private class ExecuteWithLockHelper
        {
            internal object m_lockObject;
            internal bool m_tookLock;
            internal RuntimeHelpers.TryCode m_userCode;
            internal object m_userState;

            internal ExecuteWithLockHelper(object lockObject, RuntimeHelpers.TryCode userCode, object userState)
            {
                this.m_lockObject = lockObject;
                this.m_userCode = userCode;
                this.m_userState = userState;
            }
        }

        public delegate void TryCode(object userData);
    }
}


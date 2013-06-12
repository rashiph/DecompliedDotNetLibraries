namespace System.Threading
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public sealed class CompressedStack : ISerializable
    {
        internal static RuntimeHelpers.CleanupCode cleanupCode;
        [SecurityCritical]
        private SafeCompressedStackHandle m_csHandle;
        private PermissionListSet m_pls;
        internal static RuntimeHelpers.TryCode tryCode;

        [SecurityCritical]
        internal CompressedStack(SafeCompressedStackHandle csHandle)
        {
            this.m_csHandle = csHandle;
        }

        private CompressedStack(SerializationInfo info, StreamingContext context)
        {
            this.m_pls = (PermissionListSet) info.GetValue("PLS", typeof(PermissionListSet));
        }

        [SecurityCritical]
        private CompressedStack(SafeCompressedStackHandle csHandle, PermissionListSet pls)
        {
            this.m_csHandle = csHandle;
            this.m_pls = pls;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static CompressedStack Capture()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return GetCompressedStack(ref lookForMyCaller);
        }

        [SecurityCritical]
        internal bool CheckDemand(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh)
        {
            this.CompleteConstruction(null);
            if (this.PLS == null)
            {
                return false;
            }
            return this.PLS.CheckDemand(demand, permToken, rmh);
        }

        [SecurityCritical]
        internal bool CheckSetDemand(PermissionSet pset, RuntimeMethodHandleInternal rmh)
        {
            this.CompleteConstruction(null);
            if (this.PLS == null)
            {
                return false;
            }
            return this.PLS.CheckSetDemand(pset, rmh);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal void CompleteConstruction(CompressedStack innerCS)
        {
            if (this.PLS == null)
            {
                PermissionListSet set = PermissionListSet.CreateCompressedState(this, innerCS);
                lock (this)
                {
                    if (this.PLS == null)
                    {
                        this.m_pls = set;
                    }
                }
            }
        }

        [ComVisible(false), SecuritySafeCritical]
        public CompressedStack CreateCopy()
        {
            return new CompressedStack(this.m_csHandle, this.m_pls);
        }

        [SecurityCritical]
        internal void DemandFlagsOrGrantSet(int flags, PermissionSet grantSet)
        {
            this.CompleteConstruction(null);
            if (this.PLS != null)
            {
                this.PLS.DemandFlagsOrGrantSet(flags, grantSet);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern void DestroyDCSList(SafeCompressedStackHandle compressedStack);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void DestroyDelayedCompressedStack(IntPtr unmanagedCompressedStack);
        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        public static CompressedStack GetCompressedStack()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return GetCompressedStack(ref lookForMyCaller);
        }

        [SecurityCritical]
        internal static CompressedStack GetCompressedStack(ref StackCrawlMark stackMark)
        {
            CompressedStack innerCS = null;
            if (CodeAccessSecurityEngine.QuickCheckForAllDemands())
            {
                return new CompressedStack(null);
            }
            if (CodeAccessSecurityEngine.AllDomainsHomogeneousWithNoStackModifiers())
            {
                return new CompressedStack(GetDelayedCompressedStack(ref stackMark, false)) { m_pls = PermissionListSet.CreateCompressedState_HG() };
            }
            CompressedStack stack = new CompressedStack(null);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                stack.CompressedStackHandle = GetDelayedCompressedStack(ref stackMark, true);
                if ((stack.CompressedStackHandle != null) && IsImmediateCompletionCandidate(stack.CompressedStackHandle, out innerCS))
                {
                    try
                    {
                        stack.CompleteConstruction(innerCS);
                    }
                    finally
                    {
                        DestroyDCSList(stack.CompressedStackHandle);
                    }
                }
            }
            return stack;
        }

        internal static CompressedStack GetCompressedStackThread()
        {
            ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
            if ((executionContextNoCreate != null) && (executionContextNoCreate.SecurityContext != null))
            {
                return executionContextNoCreate.SecurityContext.CompressedStack;
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetDCSCount(SafeCompressedStackHandle compressedStack);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static extern SafeCompressedStackHandle GetDelayedCompressedStack(ref StackCrawlMark stackMark, bool walkStack);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern DomainCompressedStack GetDomainCompressedStack(SafeCompressedStackHandle compressedStack, int index);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void GetHomogeneousPLS(PermissionListSet hgPLS);
        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.CompleteConstruction(null);
            info.AddValue("PLS", this.m_pls);
        }

        [SecurityCritical]
        internal void GetZoneAndOrigin(ArrayList zoneList, ArrayList originList, PermissionToken zoneToken, PermissionToken originToken)
        {
            this.CompleteConstruction(null);
            if (this.PLS != null)
            {
                this.PLS.GetZoneAndOrigin(zoneList, originList, zoneToken, originToken);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern bool IsImmediateCompletionCandidate(SafeCompressedStackHandle compressedStack, out CompressedStack innerCS);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        internal static void RestoreAppDomainStack(IntPtr appDomainStack)
        {
            Thread.CurrentThread.RestoreAppDomainStack(appDomainStack);
        }

        [SecurityCritical]
        public static void Run(CompressedStack compressedStack, ContextCallback callback, object state)
        {
            if (compressedStack == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamNull"), "compressedStack");
            }
            if (cleanupCode == null)
            {
                tryCode = new RuntimeHelpers.TryCode(CompressedStack.runTryCode);
                cleanupCode = new RuntimeHelpers.CleanupCode(CompressedStack.runFinallyCode);
            }
            CompressedStackRunData userData = new CompressedStackRunData(compressedStack, callback, state);
            RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(tryCode, cleanupCode, userData);
        }

        [PrePrepareMethod, SecurityCritical]
        internal static void runFinallyCode(object userData, bool exceptionThrown)
        {
            CompressedStackRunData data = (CompressedStackRunData) userData;
            data.cssw.Undo();
        }

        [SecurityCritical]
        internal static void runTryCode(object userData)
        {
            CompressedStackRunData data = (CompressedStackRunData) userData;
            data.cssw = SetCompressedStack(data.cs, GetCompressedStackThread());
            data.callBack(data.state);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static IntPtr SetAppDomainStack(CompressedStack cs)
        {
            return Thread.CurrentThread.SetAppDomainStack((cs == null) ? null : cs.CompressedStackHandle);
        }

        [SecurityCritical, HandleProcessCorruptedStateExceptions]
        internal static CompressedStackSwitcher SetCompressedStack(CompressedStack cs, CompressedStack prevCS)
        {
            CompressedStackSwitcher switcher = new CompressedStackSwitcher();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    SetCompressedStackThread(cs);
                    switcher.prev_CS = prevCS;
                    switcher.curr_CS = cs;
                    switcher.prev_ADStack = SetAppDomainStack(cs);
                }
            }
            catch
            {
                switcher.UndoNoThrow();
                throw;
            }
            return switcher;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static void SetCompressedStackThread(CompressedStack cs)
        {
            ExecutionContext executionContext = Thread.CurrentThread.ExecutionContext;
            if (executionContext.SecurityContext != null)
            {
                executionContext.SecurityContext.CompressedStack = cs;
            }
            else if (cs != null)
            {
                SecurityContext context2 = new SecurityContext {
                    CompressedStack = cs
                };
                executionContext.SecurityContext = context2;
            }
        }

        internal SafeCompressedStackHandle CompressedStackHandle
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
            get
            {
                return this.m_csHandle;
            }
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
            private set
            {
                this.m_csHandle = value;
            }
        }

        internal PermissionListSet PLS
        {
            get
            {
                return this.m_pls;
            }
        }

        internal class CompressedStackRunData
        {
            internal ContextCallback callBack;
            internal CompressedStack cs;
            internal CompressedStackSwitcher cssw;
            internal object state;

            internal CompressedStackRunData(CompressedStack cs, ContextCallback cb, object state)
            {
                this.cs = cs;
                this.callBack = cb;
                this.state = state;
                this.cssw = new CompressedStackSwitcher();
            }
        }
    }
}


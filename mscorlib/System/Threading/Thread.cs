namespace System.Threading
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    [ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_Thread)), ComVisible(true)]
    public sealed class Thread : CriticalFinalizerObject, _Thread
    {
        private IntPtr DONT_USE_InternalThread;
        private Context m_Context;
        private CultureInfo m_CurrentCulture;
        private CultureInfo m_CurrentUICulture;
        private Delegate m_Delegate;
        private System.Threading.ExecutionContext m_ExecutionContext;
        private int m_ManagedThreadId;
        private string m_Name;
        private int m_Priority;
        private object m_ThreadStartArg;
        [ThreadStatic]
        private static LocalDataStoreHolder s_LocalDataStore;
        private static LocalDataStoreMgr s_LocalDataStoreMgr;
        private const int STATICS_BUCKET_SIZE = 0x20;

        [SecuritySafeCritical]
        public Thread(ParameterizedThreadStart start)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            this.SetStartHelper(start, 0);
        }

        [SecuritySafeCritical]
        public Thread(ThreadStart start)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            this.SetStartHelper(start, 0);
        }

        [SecuritySafeCritical]
        public Thread(ParameterizedThreadStart start, int maxStackSize)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            if (0 > maxStackSize)
            {
                throw new ArgumentOutOfRangeException("maxStackSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.SetStartHelper(start, maxStackSize);
        }

        [SecuritySafeCritical]
        public Thread(ThreadStart start, int maxStackSize)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            if (0 > maxStackSize)
            {
                throw new ArgumentOutOfRangeException("maxStackSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.SetStartHelper(start, maxStackSize);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public void Abort()
        {
            this.AbortInternal();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public void Abort(object stateInfo)
        {
            this.AbortReason = stateInfo;
            this.AbortInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void AbortInternal();
        [HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        public static LocalDataStoreSlot AllocateDataSlot()
        {
            return LocalDataStoreManager.AllocateDataSlot();
        }

        [HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        public static LocalDataStoreSlot AllocateNamedDataSlot(string name)
        {
            return LocalDataStoreManager.AllocateNamedDataSlot(name);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public static extern void BeginCriticalRegion();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern void BeginThreadAffinity();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern void ClearAbortReason();
        private static object CompleteCrossContextCallback(InternalCrossContextDelegate ftnToCall, object[] args)
        {
            return ftnToCall(args);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public extern void DisableComObjectEagerCleanup();
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public static extern void EndCriticalRegion();
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        public static extern void EndThreadAffinity();
        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        ~Thread()
        {
            this.InternalFinalize();
        }

        [HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        public static void FreeNamedDataSlot(string name)
        {
            LocalDataStoreManager.FreeNamedDataSlot(name);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern object GetAbortReason();
        [SecuritySafeCritical]
        public System.Threading.ApartmentState GetApartmentState()
        {
            return (System.Threading.ApartmentState) this.GetApartmentStateNative();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern int GetApartmentStateNative();
        [SecurityCritical, Obsolete("Thread.GetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
        public CompressedStack GetCompressedStack()
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ThreadAPIsNotSupported"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern Context GetContextInternal(IntPtr id);
        [SecurityCritical]
        internal Context GetCurrentContextInternal()
        {
            if (this.m_Context == null)
            {
                this.m_Context = Context.DefaultContext;
            }
            return this.m_Context;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static extern Thread GetCurrentThreadNative();
        [HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        public static object GetData(LocalDataStoreSlot slot)
        {
            LocalDataStoreHolder holder = s_LocalDataStore;
            if (holder == null)
            {
                LocalDataStoreManager.ValidateSlot(slot);
                return null;
            }
            return holder.Store.GetData(slot);
        }

        [SecuritySafeCritical]
        public static AppDomain GetDomain()
        {
            if (CurrentThread.m_Context != null)
            {
                return CurrentThread.m_Context.AppDomain;
            }
            AppDomain fastDomainInternal = GetFastDomainInternal();
            if (fastDomainInternal == null)
            {
                fastDomainInternal = GetDomainInternal();
            }
            return fastDomainInternal;
        }

        [SecuritySafeCritical]
        public static int GetDomainID()
        {
            return GetDomain().GetId();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern AppDomain GetDomainInternal();
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal System.Threading.ExecutionContext GetExecutionContextNoCreate()
        {
            return this.m_ExecutionContext;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern AppDomain GetFastDomainInternal();
        [ComVisible(false)]
        public override int GetHashCode()
        {
            return this.m_ManagedThreadId;
        }

        internal IllogicalCallContext GetIllogicalCallContext()
        {
            return this.ExecutionContext.IllogicalCallContext;
        }

        [SecurityCritical, HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        internal LogicalCallContext GetLogicalCallContext()
        {
            return this.ExecutionContext.LogicalCallContext;
        }

        [HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        public static LocalDataStoreSlot GetNamedDataSlot(string name)
        {
            return LocalDataStoreManager.GetNamedDataSlot(name);
        }

        internal ThreadHandle GetNativeHandle()
        {
            IntPtr pThread = this.DONT_USE_InternalThread;
            if (pThread.IsNull())
            {
                throw new ArgumentException(null, Environment.GetResourceString("Argument_InvalidHandle"));
            }
            return new ThreadHandle(pThread);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern int GetPriorityNative();
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern ulong GetProcessDefaultStackSize();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern int GetThreadStateNative();
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void InformThreadNameChange(ThreadHandle t, string name, int len);
        [SecurityCritical]
        internal object InternalCrossContextCallback(Context ctx, InternalCrossContextDelegate ftnToCall, object[] args)
        {
            return this.InternalCrossContextCallback(ctx, ctx.InternalContextID, 0, ftnToCall, args);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern object InternalCrossContextCallback(Context ctx, IntPtr ctxID, int appDomainID, InternalCrossContextDelegate ftnToCall, object[] args);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        private extern void InternalFinalize();
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization, SecurityCritical]
        internal static extern IntPtr InternalGetCurrentThread();
        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public void Interrupt()
        {
            this.InterruptInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void InterruptInternal();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool IsAliveNative();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool IsBackgroundNative();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool IsThreadpoolThreadNative();
        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public void Join()
        {
            this.JoinInternal();
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public bool Join(int millisecondsTimeout)
        {
            return this.JoinInternal(millisecondsTimeout);
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public bool Join(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return this.Join((int) totalMilliseconds);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        private extern void JoinInternal();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        private extern bool JoinInternal(int millisecondsTimeout);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern void MemoryBarrier();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool nativeGetSafeCulture(Thread t, int appDomainId, bool isUI, ref CultureInfo safeCulture);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool nativeSetThreadUILocale(string locale);
        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public static void ResetAbort()
        {
            Thread currentThread = CurrentThread;
            if ((currentThread.ThreadState & System.Threading.ThreadState.AbortRequested) == System.Threading.ThreadState.Running)
            {
                throw new ThreadStateException(Environment.GetResourceString("ThreadState_NoAbortRequested"));
            }
            currentThread.ResetAbortNative();
            currentThread.ClearAbortReason();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void ResetAbortNative();
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        internal extern void RestoreAppDomainStack(IntPtr appDomainStack);
        [Obsolete("Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202", false), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public void Resume()
        {
            this.ResumeInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void ResumeInternal();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern void SetAbortReason(object o);
        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, SelfAffectingThreading=true)]
        public void SetApartmentState(System.Threading.ApartmentState state)
        {
            if (!this.SetApartmentStateHelper(state, true))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ApartmentStateSwitchFailed"));
            }
        }

        [SecurityCritical]
        private bool SetApartmentStateHelper(System.Threading.ApartmentState state, bool fireMDAOnMismatch)
        {
            System.Threading.ApartmentState state2 = (System.Threading.ApartmentState) this.SetApartmentStateNative((int) state, fireMDAOnMismatch);
            if (((state != System.Threading.ApartmentState.Unknown) || (state2 != System.Threading.ApartmentState.MTA)) && (state2 != state))
            {
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern int SetApartmentStateNative(int state, bool fireMDAOnMismatch);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        internal extern IntPtr SetAppDomainStack(SafeCompressedStackHandle csHandle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void SetBackgroundNative(bool isBackground);
        [SecurityCritical, Obsolete("Thread.SetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
        public void SetCompressedStack(CompressedStack stack)
        {
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ThreadAPIsNotSupported"));
        }

        [HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        public static void SetData(LocalDataStoreSlot slot, object data)
        {
            LocalDataStoreHolder holder = s_LocalDataStore;
            if (holder == null)
            {
                holder = LocalDataStoreManager.CreateLocalDataStore();
                s_LocalDataStore = holder;
            }
            holder.Store.SetData(slot, data);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void SetExecutionContext(System.Threading.ExecutionContext value)
        {
            this.m_ExecutionContext = value;
            if (value != null)
            {
                this.m_ExecutionContext.Thread = this;
            }
        }

        [SecurityCritical, HostProtection(SecurityAction.LinkDemand, SharedState=true, ExternalThreading=true)]
        internal LogicalCallContext SetLogicalCallContext(LogicalCallContext callCtx)
        {
            LogicalCallContext logicalCallContext = this.ExecutionContext.LogicalCallContext;
            this.ExecutionContext.LogicalCallContext = callCtx;
            return logicalCallContext;
        }

        [SecurityCritical]
        private void SetPrincipalInternal(IPrincipal principal)
        {
            this.GetLogicalCallContext().SecurityData.Principal = principal;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void SetPriorityNative(int priority);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void SetStart(Delegate start, int maxStackSize);
        [SecurityCritical]
        private void SetStartHelper(Delegate start, int maxStackSize)
        {
            ulong processDefaultStackSize = GetProcessDefaultStackSize();
            if (((ulong) maxStackSize) > processDefaultStackSize)
            {
                try
                {
                    CodeAccessPermission.Demand(PermissionType.FullTrust);
                }
                catch (SecurityException)
                {
                    maxStackSize = (int) Math.Min(processDefaultStackSize, 0x7fffffffL);
                }
            }
            ThreadHelper helper = new ThreadHelper(start);
            if (start is ThreadStart)
            {
                this.SetStart(new ThreadStart(helper.ThreadStart), maxStackSize);
            }
            else
            {
                this.SetStart(new ParameterizedThreadStart(helper.ThreadStart), maxStackSize);
            }
        }

        [SecuritySafeCritical]
        public static void Sleep(int millisecondsTimeout)
        {
            SleepInternal(millisecondsTimeout);
        }

        public static void Sleep(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            Sleep((int) totalMilliseconds);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void SleepInternal(int millisecondsTimeout);
        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public static void SpinWait(int iterations)
        {
            SpinWaitInternal(iterations);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        private static extern void SpinWaitInternal(int iterations);
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public void Start()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.Start(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public void Start(object parameter)
        {
            if (this.m_Delegate is ThreadStart)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ThreadWrongThreadStart"));
            }
            this.m_ThreadStartArg = parameter;
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            this.Start(ref lookForMyCaller);
        }

        [SecuritySafeCritical]
        private void Start(ref StackCrawlMark stackMark)
        {
            this.StartupSetApartmentStateInternal();
            if (this.m_Delegate != null)
            {
                ThreadHelper target = (ThreadHelper) this.m_Delegate.Target;
                System.Threading.ExecutionContext ec = System.Threading.ExecutionContext.Capture(ref stackMark, System.Threading.ExecutionContext.CaptureOptions.IgnoreSyncCtx);
                target.SetExecutionContextHelper(ec);
            }
            IPrincipal principal = CallContext.Principal;
            this.StartInternal(principal, ref stackMark);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void StartInternal(IPrincipal principal, ref StackCrawlMark stackMark);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void StartupSetApartmentStateInternal();
        [SecuritySafeCritical, Obsolete("Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202", false), SecurityPermission(SecurityAction.Demand, ControlThread=true), SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public void Suspend()
        {
            this.SuspendInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void SuspendInternal();
        void _Thread.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _Thread.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _Thread.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _Thread.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, SelfAffectingThreading=true)]
        public bool TrySetApartmentState(System.Threading.ApartmentState state)
        {
            return this.SetApartmentStateHelper(state, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static byte VolatileRead(ref byte address)
        {
            byte num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static double VolatileRead(ref double address)
        {
            double num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static short VolatileRead(ref short address)
        {
            short num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int VolatileRead(ref int address)
        {
            int num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long VolatileRead(ref long address)
        {
            long num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IntPtr VolatileRead(ref IntPtr address)
        {
            IntPtr ptr = address;
            MemoryBarrier();
            return ptr;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static object VolatileRead(ref object address)
        {
            object obj2 = address;
            MemoryBarrier();
            return obj2;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static sbyte VolatileRead(ref sbyte address)
        {
            sbyte num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static float VolatileRead(ref float address)
        {
            float num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static ushort VolatileRead(ref ushort address)
        {
            ushort num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static uint VolatileRead(ref uint address)
        {
            uint num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static ulong VolatileRead(ref ulong address)
        {
            ulong num = address;
            MemoryBarrier();
            return num;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static UIntPtr VolatileRead(ref UIntPtr address)
        {
            UIntPtr ptr = address;
            MemoryBarrier();
            return ptr;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref byte address, byte value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref double address, double value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref short address, short value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref int address, int value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref long address, long value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref IntPtr address, IntPtr value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref object address, object value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static void VolatileWrite(ref sbyte address, sbyte value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void VolatileWrite(ref float address, float value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static void VolatileWrite(ref ushort address, ushort value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static void VolatileWrite(ref uint address, uint value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static void VolatileWrite(ref ulong address, ulong value)
        {
            MemoryBarrier();
            address = value;
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false)]
        public static void VolatileWrite(ref UIntPtr address, UIntPtr value)
        {
            MemoryBarrier();
            address = value;
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        public static bool Yield()
        {
            return YieldInternal();
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
        private static extern bool YieldInternal();

        internal object AbortReason
        {
            [SecurityCritical]
            get
            {
                object abortReason = null;
                try
                {
                    abortReason = this.GetAbortReason();
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ExceptionStateCrossAppDomain"), exception);
                }
                return abortReason;
            }
            [SecurityCritical]
            set
            {
                this.SetAbortReason(value);
            }
        }

        [Obsolete("The ApartmentState property has been deprecated.  Use GetApartmentState, SetApartmentState or TrySetApartmentState instead.", false)]
        public System.Threading.ApartmentState ApartmentState
        {
            [SecuritySafeCritical]
            get
            {
                return (System.Threading.ApartmentState) this.GetApartmentStateNative();
            }
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, Synchronization=true, SelfAffectingThreading=true)]
            set
            {
                this.SetApartmentStateNative((int) value, true);
            }
        }

        public static Context CurrentContext
        {
            [SecurityCritical]
            get
            {
                return CurrentThread.GetCurrentContextInternal();
            }
        }

        public CultureInfo CurrentCulture
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_CurrentCulture != null)
                {
                    CultureInfo safeCulture = null;
                    if (nativeGetSafeCulture(this, GetDomainID(), false, ref safeCulture) && (safeCulture != null))
                    {
                        return safeCulture;
                    }
                }
                return CultureInfo.UserDefaultCulture;
            }
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                CultureInfo.nativeSetThreadLocale(value.SortName);
                value.StartCrossDomainTracking();
                this.m_CurrentCulture = value;
            }
        }

        public static IPrincipal CurrentPrincipal
        {
            [SecuritySafeCritical]
            get
            {
                lock (CurrentThread)
                {
                    IPrincipal threadPrincipal = CallContext.Principal;
                    if (threadPrincipal == null)
                    {
                        threadPrincipal = GetDomain().GetThreadPrincipal();
                        CallContext.Principal = threadPrincipal;
                    }
                    return threadPrincipal;
                }
            }
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
            set
            {
                CallContext.Principal = value;
            }
        }

        public static Thread CurrentThread
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            get
            {
                return GetCurrentThreadNative();
            }
        }

        public CultureInfo CurrentUICulture
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_CurrentUICulture != null)
                {
                    CultureInfo safeCulture = null;
                    if (nativeGetSafeCulture(this, GetDomainID(), true, ref safeCulture) && (safeCulture != null))
                    {
                        return safeCulture;
                    }
                }
                return CultureInfo.UserDefaultUICulture;
            }
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                CultureInfo.VerifyCultureName(value, true);
                if (!nativeSetThreadUILocale(value.SortName))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidResourceCultureName", new object[] { value.Name }));
                }
                value.StartCrossDomainTracking();
                this.m_CurrentUICulture = value;
            }
        }

        public System.Threading.ExecutionContext ExecutionContext
        {
            [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            get
            {
                if ((this.m_ExecutionContext == null) && (this == CurrentThread))
                {
                    this.m_ExecutionContext = new System.Threading.ExecutionContext();
                    this.m_ExecutionContext.Thread = this;
                }
                return this.m_ExecutionContext;
            }
        }

        public bool IsAlive
        {
            [SecuritySafeCritical]
            get
            {
                return this.IsAliveNative();
            }
        }

        public bool IsBackground
        {
            [SecuritySafeCritical]
            get
            {
                return this.IsBackgroundNative();
            }
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, SelfAffectingThreading=true)]
            set
            {
                this.SetBackgroundNative(value);
            }
        }

        public bool IsThreadPoolThread
        {
            [SecuritySafeCritical]
            get
            {
                return this.IsThreadpoolThreadNative();
            }
        }

        private static LocalDataStoreMgr LocalDataStoreManager
        {
            get
            {
                if (s_LocalDataStoreMgr == null)
                {
                    Interlocked.CompareExchange<LocalDataStoreMgr>(ref s_LocalDataStoreMgr, new LocalDataStoreMgr(), null);
                }
                return s_LocalDataStoreMgr;
            }
        }

        public int ManagedThreadId { [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)] get; }

        public string Name
        {
            get
            {
                return this.m_Name;
            }
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
            set
            {
                lock (this)
                {
                    if (this.m_Name != null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WriteOnce"));
                    }
                    this.m_Name = value;
                    InformThreadNameChange(this.GetNativeHandle(), value, (value != null) ? value.Length : 0);
                }
            }
        }

        public ThreadPriority Priority
        {
            [SecuritySafeCritical]
            get
            {
                return (ThreadPriority) this.GetPriorityNative();
            }
            [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, SelfAffectingThreading=true)]
            set
            {
                this.SetPriorityNative((int) value);
            }
        }

        public System.Threading.ThreadState ThreadState
        {
            [SecuritySafeCritical]
            get
            {
                return (System.Threading.ThreadState) this.GetThreadStateNative();
            }
        }
    }
}


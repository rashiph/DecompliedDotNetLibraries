namespace System.Threading
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public sealed class Mutex : WaitHandle
    {
        private const string c_ReservedMutexName = @"Global\CLR_RESERVED_MUTEX_NAME";
        private static bool dummyBool;
        private static Mutex s_ReservedMutex = null;

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
        public Mutex() : this(false, null, out dummyBool)
        {
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private Mutex(SafeWaitHandle handle)
        {
            base.SetHandleInternal(handle);
            handle.SetAsMutex();
            base.hasThreadAffinity = true;
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public Mutex(bool initiallyOwned) : this(initiallyOwned, null, out dummyBool)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        public Mutex(bool initiallyOwned, string name) : this(initiallyOwned, name, out dummyBool)
        {
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public Mutex(bool initiallyOwned, string name, out bool createdNew) : this(initiallyOwned, name, out createdNew, null)
        {
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public unsafe Mutex(bool initiallyOwned, string name, out bool createdNew, MutexSecurity mutexSecurity)
        {
            if ((name != null) && (260 < name.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", new object[] { name }));
            }
            Win32Native.SECURITY_ATTRIBUTES structure = null;
            if (mutexSecurity != null)
            {
                structure = new Win32Native.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                byte[] securityDescriptorBinaryForm = mutexSecurity.GetSecurityDescriptorBinaryForm();
                byte* pDest = stackalloc byte[(IntPtr) securityDescriptorBinaryForm.Length];
                Buffer.memcpy(securityDescriptorBinaryForm, 0, pDest, 0, securityDescriptorBinaryForm.Length);
                structure.pSecurityDescriptor = pDest;
            }
            RuntimeHelpers.CleanupCode backoutCode = new RuntimeHelpers.CleanupCode(this.MutexCleanupCode);
            MutexCleanupInfo cleanupInfo = new MutexCleanupInfo(null, false);
            MutexTryCodeHelper helper = new MutexTryCodeHelper(initiallyOwned, cleanupInfo, name, structure, this);
            RuntimeHelpers.TryCode code = new RuntimeHelpers.TryCode(helper.MutexTryCode);
            RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(code, backoutCode, cleanupInfo);
            createdNew = helper.m_newMutex;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityPermission(SecurityAction.Assert, ControlPrincipal=true)]
        internal static unsafe void AcquireReservedMutex(ref bool bHandleObtained)
        {
            SafeWaitHandle handle = null;
            bHandleObtained = false;
            if (Environment.IsW2k3)
            {
                if (s_ReservedMutex == null)
                {
                    Win32Native.SECURITY_ATTRIBUTES security_attributes;
                    MutexSecurity security = new MutexSecurity();
                    SecurityIdentifier identity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    security.AddAccessRule(new MutexAccessRule(identity, MutexRights.FullControl, AccessControlType.Allow));
                    security_attributes = new Win32Native.SECURITY_ATTRIBUTES {
                        nLength = Marshal.SizeOf(security_attributes)
                    };
                    byte[] securityDescriptorBinaryForm = security.GetSecurityDescriptorBinaryForm();
                    byte* pDest = stackalloc byte[(IntPtr) securityDescriptorBinaryForm.Length];
                    Buffer.memcpy(securityDescriptorBinaryForm, 0, pDest, 0, securityDescriptorBinaryForm.Length);
                    security_attributes.pSecurityDescriptor = pDest;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        handle = Win32Native.CreateMutex(security_attributes, false, @"Global\CLR_RESERVED_MUTEX_NAME");
                        handle.SetAsReservedMutex();
                    }
                    int errorCode = Marshal.GetLastWin32Error();
                    if (handle.IsInvalid)
                    {
                        handle.SetHandleAsInvalid();
                        __Error.WinIOError(errorCode, @"Global\CLR_RESERVED_MUTEX_NAME");
                    }
                    Mutex mutex = new Mutex(handle);
                    Interlocked.CompareExchange<Mutex>(ref s_ReservedMutex, mutex, null);
                }
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    try
                    {
                        s_ReservedMutex.WaitOne();
                        bHandleObtained = true;
                    }
                    catch (AbandonedMutexException)
                    {
                        bHandleObtained = true;
                    }
                }
            }
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static int CreateMutexHandle(bool initiallyOwned, string name, Win32Native.SECURITY_ATTRIBUTES securityAttribute, out SafeWaitHandle mutexHandle)
        {
            bool bHandleObtained = false;
            bool flag2 = false;
        Label_0004:
            mutexHandle = Win32Native.CreateMutex(securityAttribute, initiallyOwned, name);
            int num = Marshal.GetLastWin32Error();
            if (mutexHandle.IsInvalid && (num == 5))
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    try
                    {
                    }
                    finally
                    {
                        Thread.BeginThreadAffinity();
                        flag2 = true;
                    }
                    AcquireReservedMutex(ref bHandleObtained);
                    mutexHandle = Win32Native.OpenMutex(0x100001, false, name);
                    if (!mutexHandle.IsInvalid)
                    {
                        num = 0xb7;
                    }
                    else
                    {
                        num = Marshal.GetLastWin32Error();
                    }
                }
                finally
                {
                    if (bHandleObtained)
                    {
                        ReleaseReservedMutex();
                    }
                    if (flag2)
                    {
                        Thread.EndThreadAffinity();
                    }
                }
                switch (num)
                {
                    case 2:
                        goto Label_0004;

                    case 0:
                        return 0xb7;
                }
            }
            return num;
        }

        [SecuritySafeCritical]
        public MutexSecurity GetAccessControl()
        {
            return new MutexSecurity(base.safeWaitHandle, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        [SecurityCritical, PrePrepareMethod]
        private void MutexCleanupCode(object userData, bool exceptionThrown)
        {
            MutexCleanupInfo info = (MutexCleanupInfo) userData;
            if (!base.hasThreadAffinity)
            {
                if ((info.mutexHandle != null) && !info.mutexHandle.IsInvalid)
                {
                    if (info.inCriticalRegion)
                    {
                        Win32Native.ReleaseMutex(info.mutexHandle);
                    }
                    info.mutexHandle.Dispose();
                }
                if (info.inCriticalRegion)
                {
                    Thread.EndCriticalRegion();
                    Thread.EndThreadAffinity();
                }
            }
        }

        [SecurityCritical]
        public static Mutex OpenExisting(string name)
        {
            return OpenExisting(name, MutexRights.Synchronize | MutexRights.Modify);
        }

        [SecurityCritical]
        public static Mutex OpenExisting(string name, MutexRights rights)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", Environment.GetResourceString("ArgumentNull_WithParamName"));
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (260 < name.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WaitHandleNameTooLong", new object[] { name }));
            }
            SafeWaitHandle handle = Win32Native.OpenMutex((int) rights, false, name);
            int errorCode = 0;
            if (handle.IsInvalid)
            {
                errorCode = Marshal.GetLastWin32Error();
                if ((2 == errorCode) || (0x7b == errorCode))
                {
                    throw new WaitHandleCannotBeOpenedException();
                }
                if (((name != null) && (name.Length != 0)) && (6 == errorCode))
                {
                    throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { name }));
                }
                __Error.WinIOError(errorCode, name);
            }
            return new Mutex(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
        public void ReleaseMutex()
        {
            if (!Win32Native.ReleaseMutex(base.safeWaitHandle))
            {
                throw new ApplicationException(Environment.GetResourceString("Arg_SynchronizationLockException"));
            }
            Thread.EndCriticalRegion();
            Thread.EndThreadAffinity();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static void ReleaseReservedMutex()
        {
            if (Environment.IsW2k3)
            {
                s_ReservedMutex.ReleaseMutex();
            }
        }

        [SecuritySafeCritical]
        public void SetAccessControl(MutexSecurity mutexSecurity)
        {
            if (mutexSecurity == null)
            {
                throw new ArgumentNullException("mutexSecurity");
            }
            mutexSecurity.Persist(base.safeWaitHandle);
        }

        internal class MutexCleanupInfo
        {
            internal bool inCriticalRegion;
            [SecurityCritical]
            internal SafeWaitHandle mutexHandle;

            [SecurityCritical]
            internal MutexCleanupInfo(SafeWaitHandle mutexHandle, bool inCriticalRegion)
            {
                this.mutexHandle = mutexHandle;
                this.inCriticalRegion = inCriticalRegion;
            }
        }

        internal class MutexTryCodeHelper
        {
            private Mutex.MutexCleanupInfo m_cleanupInfo;
            private bool m_initiallyOwned;
            private Mutex m_mutex;
            private string m_name;
            internal bool m_newMutex;
            [SecurityCritical]
            private Win32Native.SECURITY_ATTRIBUTES m_secAttrs;

            [PrePrepareMethod, SecurityCritical]
            internal MutexTryCodeHelper(bool initiallyOwned, Mutex.MutexCleanupInfo cleanupInfo, string name, Win32Native.SECURITY_ATTRIBUTES secAttrs, Mutex mutex)
            {
                this.m_initiallyOwned = initiallyOwned;
                this.m_cleanupInfo = cleanupInfo;
                this.m_name = name;
                this.m_secAttrs = secAttrs;
                this.m_mutex = mutex;
            }

            [SecurityCritical, PrePrepareMethod]
            internal void MutexTryCode(object userData)
            {
                SafeWaitHandle mutexHandle = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (this.m_initiallyOwned)
                    {
                        this.m_cleanupInfo.inCriticalRegion = true;
                        Thread.BeginThreadAffinity();
                        Thread.BeginCriticalRegion();
                    }
                }
                int errorCode = 0;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    errorCode = Mutex.CreateMutexHandle(this.m_initiallyOwned, this.m_name, this.m_secAttrs, out mutexHandle);
                }
                if (mutexHandle.IsInvalid)
                {
                    mutexHandle.SetHandleAsInvalid();
                    if (((this.m_name != null) && (this.m_name.Length != 0)) && (6 == errorCode))
                    {
                        throw new WaitHandleCannotBeOpenedException(Environment.GetResourceString("Threading.WaitHandleCannotBeOpenedException_InvalidHandle", new object[] { this.m_name }));
                    }
                    __Error.WinIOError(errorCode, this.m_name);
                }
                this.m_newMutex = errorCode != 0xb7;
                this.m_mutex.SetHandleInternal(mutexHandle);
                mutexHandle.SetAsMutex();
                this.m_mutex.hasThreadAffinity = true;
            }
        }
    }
}


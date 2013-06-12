namespace System.Security.AccessControl
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.Threading;

    internal sealed class Privilege
    {
        public const string AssignPrimaryToken = "SeAssignPrimaryTokenPrivilege";
        public const string Audit = "SeAuditPrivilege";
        public const string Backup = "SeBackupPrivilege";
        public const string ChangeNotify = "SeChangeNotifyPrivilege";
        public const string CreateGlobal = "SeCreateGlobalPrivilege";
        public const string CreatePageFile = "SeCreatePagefilePrivilege";
        public const string CreatePermanent = "SeCreatePermanentPrivilege";
        public const string CreateToken = "SeCreateTokenPrivilege";
        private readonly Thread currentThread = Thread.CurrentThread;
        public const string Debug = "SeDebugPrivilege";
        public const string EnableDelegation = "SeEnableDelegationPrivilege";
        public const string Impersonate = "SeImpersonatePrivilege";
        public const string IncreaseBasePriority = "SeIncreaseBasePriorityPrivilege";
        public const string IncreaseQuota = "SeIncreaseQuotaPrivilege";
        private bool initialState;
        public const string LoadDriver = "SeLoadDriverPrivilege";
        public const string LockMemory = "SeLockMemoryPrivilege";
        [SecurityCritical]
        private Win32Native.LUID luid;
        private static Hashtable luids = new Hashtable();
        public const string MachineAccount = "SeMachineAccountPrivilege";
        public const string ManageVolume = "SeManageVolumePrivilege";
        private bool needToRevert;
        private static ReaderWriterLock privilegeLock = new ReaderWriterLock();
        private static Hashtable privileges = new Hashtable();
        public const string ProfileSingleProcess = "SeProfileSingleProcessPrivilege";
        public const string RemoteShutdown = "SeRemoteShutdownPrivilege";
        public const string ReserveProcessor = "SeReserveProcessorPrivilege";
        public const string Restore = "SeRestorePrivilege";
        public const string Security = "SeSecurityPrivilege";
        public const string Shutdown = "SeShutdownPrivilege";
        private bool stateWasChanged;
        public const string SyncAgent = "SeSyncAgentPrivilege";
        public const string SystemEnvironment = "SeSystemEnvironmentPrivilege";
        public const string SystemProfile = "SeSystemProfilePrivilege";
        public const string SystemTime = "SeSystemtimePrivilege";
        public const string TakeOwnership = "SeTakeOwnershipPrivilege";
        private TlsContents tlsContents;
        private static LocalDataStoreSlot tlsSlot = Thread.AllocateDataSlot();
        public const string TrustedComputingBase = "SeTcbPrivilege";
        public const string TrustedCredentialManagerAccess = "SeTrustedCredManAccessPrivilege";
        public const string Undock = "SeUndockPrivilege";
        public const string UnsolicitedInput = "SeUnsolicitedInputPrivilege";

        [SecurityCritical]
        public Privilege(string privilegeName)
        {
            if (privilegeName == null)
            {
                throw new ArgumentNullException("privilegeName");
            }
            this.luid = LuidFromPrivilege(privilegeName);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public void Enable()
        {
            this.ToggleState(true);
        }

        [SecuritySafeCritical]
        ~Privilege()
        {
            if (this.needToRevert)
            {
                this.Revert();
            }
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static Win32Native.LUID LuidFromPrivilege(string privilege)
        {
            Win32Native.LUID luid;
            luid.LowPart = 0;
            luid.HighPart = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                privilegeLock.AcquireReaderLock(-1);
                if (luids.Contains(privilege))
                {
                    luid = (Win32Native.LUID) luids[privilege];
                    privilegeLock.ReleaseReaderLock();
                    return luid;
                }
                privilegeLock.ReleaseReaderLock();
                if (!Win32Native.LookupPrivilegeValue(null, privilege, ref luid))
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 8:
                            throw new OutOfMemoryException();

                        case 5:
                            throw new UnauthorizedAccessException();

                        case 0x521:
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPrivilegeName", new object[] { privilege }));
                    }
                    throw new InvalidOperationException();
                }
                privilegeLock.AcquireWriterLock(-1);
            }
            finally
            {
                if (privilegeLock.IsReaderLockHeld)
                {
                    privilegeLock.ReleaseReaderLock();
                }
                if (privilegeLock.IsWriterLockHeld)
                {
                    if (!luids.Contains(privilege))
                    {
                        luids[privilege] = luid;
                        privileges[luid] = privilege;
                    }
                    privilegeLock.ReleaseWriterLock();
                }
            }
            return luid;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Reset()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this.stateWasChanged = false;
                this.initialState = false;
                this.needToRevert = false;
                if ((this.tlsContents != null) && (this.tlsContents.DecrementReferenceCount() == 0))
                {
                    this.tlsContents = null;
                    Thread.SetData(tlsSlot, null);
                }
            }
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public void Revert()
        {
            int num = 0;
            if (!this.currentThread.Equals(Thread.CurrentThread))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustBeSameThread"));
            }
            if (this.NeedToRevert)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    bool flag = true;
                    try
                    {
                        if (this.stateWasChanged && ((this.tlsContents.ReferenceCountValue > 1) || !this.tlsContents.IsImpersonating))
                        {
                            Win32Native.TOKEN_PRIVILEGE newState = new Win32Native.TOKEN_PRIVILEGE {
                                PrivilegeCount = 1
                            };
                            newState.Privilege.Luid = this.luid;
                            newState.Privilege.Attributes = this.initialState ? 2 : 0;
                            Win32Native.TOKEN_PRIVILEGE structure = new Win32Native.TOKEN_PRIVILEGE();
                            uint returnLength = 0;
                            int introduced5 = Marshal.SizeOf(structure);
                            if (!Win32Native.AdjustTokenPrivileges(this.tlsContents.ThreadHandle, false, ref newState, (uint) introduced5, ref structure, ref returnLength))
                            {
                                num = Marshal.GetLastWin32Error();
                                flag = false;
                            }
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.Reset();
                        }
                    }
                }
                switch (num)
                {
                    case 8:
                        throw new OutOfMemoryException();

                    case 5:
                        throw new UnauthorizedAccessException();
                }
                if (num != 0)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        private void ToggleState(bool enable)
        {
            int num = 0;
            if (!this.currentThread.Equals(Thread.CurrentThread))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustBeSameThread"));
            }
            if (this.needToRevert)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustRevertPrivilege"));
            }
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                try
                {
                    this.tlsContents = Thread.GetData(tlsSlot) as TlsContents;
                    if (this.tlsContents == null)
                    {
                        this.tlsContents = new TlsContents();
                        Thread.SetData(tlsSlot, this.tlsContents);
                    }
                    else
                    {
                        this.tlsContents.IncrementReferenceCount();
                    }
                    Win32Native.TOKEN_PRIVILEGE newState = new Win32Native.TOKEN_PRIVILEGE {
                        PrivilegeCount = 1
                    };
                    newState.Privilege.Luid = this.luid;
                    newState.Privilege.Attributes = enable ? 2 : 0;
                    Win32Native.TOKEN_PRIVILEGE structure = new Win32Native.TOKEN_PRIVILEGE();
                    uint returnLength = 0;
                    int introduced4 = Marshal.SizeOf(structure);
                    if (!Win32Native.AdjustTokenPrivileges(this.tlsContents.ThreadHandle, false, ref newState, (uint) introduced4, ref structure, ref returnLength))
                    {
                        num = Marshal.GetLastWin32Error();
                    }
                    else if (0x514 == Marshal.GetLastWin32Error())
                    {
                        num = 0x514;
                    }
                    else
                    {
                        this.initialState = (structure.Privilege.Attributes & 2) != 0;
                        this.stateWasChanged = this.initialState != enable;
                        this.needToRevert = this.tlsContents.IsImpersonating || this.stateWasChanged;
                    }
                }
                finally
                {
                    if (!this.needToRevert)
                    {
                        this.Reset();
                    }
                }
            }
            switch (num)
            {
                case 0x514:
                    throw new PrivilegeNotHeldException(privileges[this.luid] as string);

                case 8:
                    throw new OutOfMemoryException();

                case 5:
                case 0x543:
                    throw new UnauthorizedAccessException();
            }
            if (num != 0)
            {
                throw new InvalidOperationException();
            }
        }

        public bool NeedToRevert
        {
            get
            {
                return this.needToRevert;
            }
        }

        private sealed class TlsContents : IDisposable
        {
            private bool disposed;
            private bool isImpersonating;
            [SecurityCritical]
            private static SafeTokenHandle processHandle = new SafeTokenHandle(IntPtr.Zero);
            private int referenceCount = 1;
            private static readonly object syncRoot = new object();
            [SecurityCritical]
            private SafeTokenHandle threadHandle = new SafeTokenHandle(IntPtr.Zero);

            [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            public TlsContents()
            {
                int num = 0;
                int num2 = 0;
                bool flag = true;
                if (processHandle.IsInvalid)
                {
                    lock (syncRoot)
                    {
                        if (processHandle.IsInvalid)
                        {
                            SafeTokenHandle handle;
                            if (!Win32Native.OpenProcessToken(Win32Native.GetCurrentProcess(), TokenAccessLevels.Duplicate, out handle))
                            {
                                num2 = Marshal.GetLastWin32Error();
                                flag = false;
                            }
                            processHandle = handle;
                        }
                    }
                }
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    try
                    {
                        SafeTokenHandle threadHandle = this.threadHandle;
                        num = System.Security.Principal.Win32.OpenThreadToken(TokenAccessLevels.AdjustPrivileges | TokenAccessLevels.Query, WinSecurityContext.Process, out this.threadHandle);
                        num &= 0x7ff8ffff;
                        if (num != 0)
                        {
                            if (flag)
                            {
                                this.threadHandle = threadHandle;
                                if (num != 0x3f0)
                                {
                                    flag = false;
                                }
                                if (flag)
                                {
                                    num = 0;
                                    if (!Win32Native.DuplicateTokenEx(processHandle, TokenAccessLevels.AdjustPrivileges | TokenAccessLevels.Query | TokenAccessLevels.Impersonate, IntPtr.Zero, Win32Native.SECURITY_IMPERSONATION_LEVEL.Impersonation, System.Security.Principal.TokenType.TokenImpersonation, ref this.threadHandle))
                                    {
                                        num = Marshal.GetLastWin32Error();
                                        flag = false;
                                    }
                                }
                                if (flag)
                                {
                                    num = System.Security.Principal.Win32.SetThreadToken(this.threadHandle);
                                    num &= 0x7ff8ffff;
                                    if (num != 0)
                                    {
                                        flag = false;
                                    }
                                }
                                if (flag)
                                {
                                    this.isImpersonating = true;
                                }
                            }
                            else
                            {
                                num = num2;
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.Dispose();
                        }
                    }
                }
                switch (num)
                {
                    case 8:
                        throw new OutOfMemoryException();

                    case 5:
                    case 0x543:
                        throw new UnauthorizedAccessException();
                }
                if (num != 0)
                {
                    throw new InvalidOperationException();
                }
            }

            [SecurityCritical]
            public int DecrementReferenceCount()
            {
                int num = --this.referenceCount;
                if (num == 0)
                {
                    this.Dispose();
                }
                return num;
            }

            [SecuritySafeCritical]
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            [SecurityCritical]
            private void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    if (disposing && (this.threadHandle != null))
                    {
                        this.threadHandle.Dispose();
                        this.threadHandle = null;
                    }
                    if (this.isImpersonating)
                    {
                        System.Security.Principal.Win32.RevertToSelf();
                    }
                    this.disposed = true;
                }
            }

            [SecuritySafeCritical]
            ~TlsContents()
            {
                if (!this.disposed)
                {
                    this.Dispose(false);
                }
            }

            public void IncrementReferenceCount()
            {
                this.referenceCount++;
            }

            public bool IsImpersonating
            {
                get
                {
                    return this.isImpersonating;
                }
            }

            public int ReferenceCountValue
            {
                get
                {
                    return this.referenceCount;
                }
            }

            public SafeTokenHandle ThreadHandle
            {
                [SecurityCritical]
                get
                {
                    return this.threadHandle;
                }
            }
        }
    }
}


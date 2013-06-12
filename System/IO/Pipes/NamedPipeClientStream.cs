namespace System.IO.Pipes
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class NamedPipeClientStream : PipeStream
    {
        private int m_access;
        private TokenImpersonationLevel m_impersonationLevel;
        private HandleInheritability m_inheritability;
        private string m_normalizedPipePath;
        private PipeOptions m_pipeOptions;

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(string pipeName) : this(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(string serverName, string pipeName) : this(serverName, pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction) : this(serverName, pipeName, direction, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
        {
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle) : base(direction, 0)
        {
            if (safePipeHandle == null)
            {
                throw new ArgumentNullException("safePipeHandle");
            }
            if (safePipeHandle.IsInvalid)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidHandle"), "safePipeHandle");
            }
            if (Microsoft.Win32.UnsafeNativeMethods.GetFileType(safePipeHandle) != 3)
            {
                throw new IOException(System.SR.GetString("IO_IO_InvalidPipeHandle"));
            }
            base.InitializeHandle(safePipeHandle, true, isAsync);
            if (isConnected)
            {
                base.State = PipeState.Connected;
            }
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options) : this(serverName, pipeName, direction, options, TokenImpersonationLevel.None, HandleInheritability.None)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel) : this(serverName, pipeName, direction, options, impersonationLevel, HandleInheritability.None)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(string serverName, string pipeName, PipeAccessRights desiredAccessRights, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability) : base(DirectionFromRights(desiredAccessRights), 0)
        {
            if (pipeName == null)
            {
                throw new ArgumentNullException("pipeName");
            }
            if (serverName == null)
            {
                throw new ArgumentNullException("serverName", System.SR.GetString("ArgumentNull_ServerName"));
            }
            if (pipeName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NeedNonemptyPipeName"));
            }
            if (serverName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_EmptyServerName"));
            }
            if ((options & ~(PipeOptions.Asynchronous | PipeOptions.WriteThrough)) != PipeOptions.None)
            {
                throw new ArgumentOutOfRangeException("options", System.SR.GetString("ArgumentOutOfRange_OptionsInvalid"));
            }
            if ((impersonationLevel < TokenImpersonationLevel.None) || (impersonationLevel > TokenImpersonationLevel.Delegation))
            {
                throw new ArgumentOutOfRangeException("impersonationLevel", System.SR.GetString("ArgumentOutOfRange_ImpersonationInvalid"));
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability", System.SR.GetString("ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable"));
            }
            if ((desiredAccessRights & ~(PipeAccessRights.AccessSystemSecurity | PipeAccessRights.FullControl)) != 0)
            {
                throw new ArgumentOutOfRangeException("desiredAccessRights", System.SR.GetString("ArgumentOutOfRange_InvalidPipeAccessRights"));
            }
            this.m_normalizedPipePath = Path.GetFullPath(@"\\" + serverName + @"\pipe\" + pipeName);
            if (string.Compare(this.m_normalizedPipePath, @"\\.\pipe\anonymous", StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new ArgumentOutOfRangeException("pipeName", System.SR.GetString("ArgumentOutOfRange_AnonymousReserved"));
            }
            this.m_inheritability = inheritability;
            this.m_impersonationLevel = impersonationLevel;
            this.m_pipeOptions = options;
            this.m_access = (int) desiredAccessRights;
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability) : base(direction, 0)
        {
            if (pipeName == null)
            {
                throw new ArgumentNullException("pipeName");
            }
            if (serverName == null)
            {
                throw new ArgumentNullException("serverName", System.SR.GetString("ArgumentNull_ServerName"));
            }
            if (pipeName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NeedNonemptyPipeName"));
            }
            if (serverName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_EmptyServerName"));
            }
            if ((options & ~(PipeOptions.Asynchronous | PipeOptions.WriteThrough)) != PipeOptions.None)
            {
                throw new ArgumentOutOfRangeException("options", System.SR.GetString("ArgumentOutOfRange_OptionsInvalid"));
            }
            if ((impersonationLevel < TokenImpersonationLevel.None) || (impersonationLevel > TokenImpersonationLevel.Delegation))
            {
                throw new ArgumentOutOfRangeException("impersonationLevel", System.SR.GetString("ArgumentOutOfRange_ImpersonationInvalid"));
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability", System.SR.GetString("ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable"));
            }
            this.m_normalizedPipePath = Path.GetFullPath(@"\\" + serverName + @"\pipe\" + pipeName);
            if (string.Compare(this.m_normalizedPipePath, @"\\.\pipe\anonymous", StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new ArgumentOutOfRangeException("pipeName", System.SR.GetString("ArgumentOutOfRange_AnonymousReserved"));
            }
            this.m_inheritability = inheritability;
            this.m_impersonationLevel = impersonationLevel;
            this.m_pipeOptions = options;
            if ((PipeDirection.In & direction) != ((PipeDirection) 0))
            {
                this.m_access |= -2147483648;
            }
            if ((PipeDirection.Out & direction) != ((PipeDirection) 0))
            {
                this.m_access |= 0x40000000;
            }
        }

        private void CheckConnectOperationsClient()
        {
            if (base.State == PipeState.Connected)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeAlreadyConnected"));
            }
            if (base.State == PipeState.Closed)
            {
                System.IO.__Error.PipeNotOpen();
            }
        }

        [SecurityCritical]
        protected internal override void CheckPipePropertyOperations()
        {
            base.CheckPipePropertyOperations();
            if (base.State == PipeState.WaitingToConnect)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeNotYetConnected"));
            }
            if (base.State == PipeState.Broken)
            {
                throw new IOException(System.SR.GetString("IO_IO_PipeBroken"));
            }
        }

        public void Connect()
        {
            this.Connect(-1);
        }

        [SecurityCritical]
        public void Connect(int timeout)
        {
            this.CheckConnectOperationsClient();
            if ((timeout < 0) && (timeout != -1))
            {
                throw new ArgumentOutOfRangeException("timeout", System.SR.GetString("ArgumentOutOfRange_InvalidTimeout"));
            }
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(this.m_inheritability);
            int pipeOptions = (int) this.m_pipeOptions;
            if (this.m_impersonationLevel != TokenImpersonationLevel.None)
            {
                pipeOptions |= 0x100000;
                pipeOptions |= ((int) (this.m_impersonationLevel - 1)) << 0x10;
            }
            int tickCount = Environment.TickCount;
            int num3 = 0;
        Label_005C:
            if (!Microsoft.Win32.UnsafeNativeMethods.WaitNamedPipe(this.m_normalizedPipePath, timeout - num3))
            {
                int errorCode = Marshal.GetLastWin32Error();
                switch (errorCode)
                {
                    case 2:
                        goto Label_00EF;

                    case 0:
                        goto Label_0105;
                }
                System.IO.__Error.WinIOError(errorCode, string.Empty);
            }
            SafePipeHandle handle = Microsoft.Win32.UnsafeNativeMethods.CreateNamedPipeClient(this.m_normalizedPipePath, this.m_access, FileShare.None, secAttrs, FileMode.Open, pipeOptions, Microsoft.Win32.UnsafeNativeMethods.NULL);
            if (handle.IsInvalid)
            {
                int num5 = Marshal.GetLastWin32Error();
                if (num5 == 0xe7)
                {
                    goto Label_00EF;
                }
                System.IO.__Error.WinIOError(num5, string.Empty);
            }
            base.InitializeHandle(handle, false, (this.m_pipeOptions & PipeOptions.Asynchronous) != PipeOptions.None);
            base.State = PipeState.Connected;
            return;
        Label_00EF:
            if ((timeout == -1) || ((num3 = Environment.TickCount - tickCount) < timeout))
            {
                goto Label_005C;
            }
        Label_0105:
            throw new TimeoutException();
        }

        private static PipeDirection DirectionFromRights(PipeAccessRights rights)
        {
            PipeDirection direction = (PipeDirection) 0;
            if ((rights & PipeAccessRights.ReadData) != 0)
            {
                direction |= PipeDirection.In;
            }
            if ((rights & PipeAccessRights.WriteData) != 0)
            {
                direction |= PipeDirection.Out;
            }
            return direction;
        }

        ~NamedPipeClientStream()
        {
            this.Dispose(false);
        }

        public int NumberOfServerInstances
        {
            [SecurityCritical]
            get
            {
                int num;
                this.CheckPipePropertyOperations();
                if (!Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeHandleState(base.InternalHandle, Microsoft.Win32.UnsafeNativeMethods.NULL, out num, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, 0))
                {
                    base.WinIOError(Marshal.GetLastWin32Error());
                }
                return num;
            }
        }
    }
}


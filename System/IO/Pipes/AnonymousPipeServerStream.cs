namespace System.IO.Pipes
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class AnonymousPipeServerStream : PipeStream
    {
        private SafePipeHandle m_clientHandle;
        private bool m_clientHandleExposed;

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeServerStream() : this(PipeDirection.Out, HandleInheritability.None, 0, null)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction) : this(direction, HandleInheritability.None, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability) : this(direction, inheritability, 0)
        {
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction, SafePipeHandle serverSafePipeHandle, SafePipeHandle clientSafePipeHandle) : base(direction, 0)
        {
            if (direction == PipeDirection.InOut)
            {
                throw new NotSupportedException(System.SR.GetString("NotSupported_AnonymousPipeUnidirectional"));
            }
            if (serverSafePipeHandle == null)
            {
                throw new ArgumentNullException("serverSafePipeHandle");
            }
            if (clientSafePipeHandle == null)
            {
                throw new ArgumentNullException("clientSafePipeHandle");
            }
            if (serverSafePipeHandle.IsInvalid)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidHandle"), "serverSafePipeHandle");
            }
            if (clientSafePipeHandle.IsInvalid)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidHandle"), "clientSafePipeHandle");
            }
            if (Microsoft.Win32.UnsafeNativeMethods.GetFileType(serverSafePipeHandle) != 3)
            {
                throw new IOException(System.SR.GetString("IO_IO_InvalidPipeHandle"));
            }
            if (Microsoft.Win32.UnsafeNativeMethods.GetFileType(clientSafePipeHandle) != 3)
            {
                throw new IOException(System.SR.GetString("IO_IO_InvalidPipeHandle"));
            }
            base.InitializeHandle(serverSafePipeHandle, true, false);
            this.m_clientHandle = clientSafePipeHandle;
            this.m_clientHandleExposed = true;
            base.State = PipeState.Connected;
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability, int bufferSize) : base(direction, bufferSize)
        {
            if (direction == PipeDirection.InOut)
            {
                throw new NotSupportedException(System.SR.GetString("NotSupported_AnonymousPipeUnidirectional"));
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability", System.SR.GetString("ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable"));
            }
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(inheritability);
            this.Create(direction, secAttrs, bufferSize);
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity) : base(direction, bufferSize)
        {
            object obj2;
            if (direction == PipeDirection.InOut)
            {
                throw new NotSupportedException(System.SR.GetString("NotSupported_AnonymousPipeUnidirectional"));
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability", System.SR.GetString("ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable"));
            }
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(inheritability, pipeSecurity, out obj2);
            try
            {
                this.Create(direction, secAttrs, bufferSize);
            }
            finally
            {
                if (obj2 != null)
                {
                    ((GCHandle) obj2).Free();
                }
            }
        }

        [SecurityCritical]
        private void Create(PipeDirection direction, Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs, int bufferSize)
        {
            bool flag;
            SafePipeHandle handle;
            SafePipeHandle handle2;
            if (direction == PipeDirection.In)
            {
                flag = Microsoft.Win32.UnsafeNativeMethods.CreatePipe(out handle, out this.m_clientHandle, secAttrs, bufferSize);
            }
            else
            {
                flag = Microsoft.Win32.UnsafeNativeMethods.CreatePipe(out this.m_clientHandle, out handle, secAttrs, bufferSize);
            }
            if (!flag)
            {
                System.IO.__Error.WinIOError(Marshal.GetLastWin32Error(), string.Empty);
            }
            if (!Microsoft.Win32.UnsafeNativeMethods.DuplicateHandle(Microsoft.Win32.UnsafeNativeMethods.GetCurrentProcess(), handle, Microsoft.Win32.UnsafeNativeMethods.GetCurrentProcess(), out handle2, 0, false, 2))
            {
                System.IO.__Error.WinIOError(Marshal.GetLastWin32Error(), string.Empty);
            }
            handle.Dispose();
            base.InitializeHandle(handle2, false, false);
            base.State = PipeState.Connected;
        }

        [SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((!this.m_clientHandleExposed && (this.m_clientHandle != null)) && !this.m_clientHandle.IsClosed)
                {
                    this.m_clientHandle.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [SecurityCritical]
        public void DisposeLocalCopyOfClientHandle()
        {
            if ((this.m_clientHandle != null) && !this.m_clientHandle.IsClosed)
            {
                this.m_clientHandle.Dispose();
            }
        }

        ~AnonymousPipeServerStream()
        {
            this.Dispose(false);
        }

        [SecurityCritical]
        public string GetClientHandleAsString()
        {
            this.m_clientHandleExposed = true;
            return this.m_clientHandle.DangerousGetHandle().ToString();
        }

        public SafePipeHandle ClientSafePipeHandle
        {
            [SecurityCritical]
            get
            {
                this.m_clientHandleExposed = true;
                return this.m_clientHandle;
            }
        }

        public override PipeTransmissionMode ReadMode
        {
            [SecurityCritical]
            set
            {
                this.CheckPipePropertyOperations();
                if ((value < PipeTransmissionMode.Byte) || (value > PipeTransmissionMode.Message))
                {
                    throw new ArgumentOutOfRangeException("value", System.SR.GetString("ArgumentOutOfRange_TransmissionModeByteOrMsg"));
                }
                if (value == PipeTransmissionMode.Message)
                {
                    throw new NotSupportedException(System.SR.GetString("NotSupported_AnonymousPipeMessagesNotSupported"));
                }
            }
        }

        public override PipeTransmissionMode TransmissionMode
        {
            [SecurityCritical]
            get
            {
                return PipeTransmissionMode.Byte;
            }
        }
    }
}


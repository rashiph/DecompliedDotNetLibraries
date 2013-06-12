namespace System.IO.Pipes
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class AnonymousPipeClientStream : PipeStream
    {
        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeClientStream(string pipeHandleAsString) : this(PipeDirection.In, pipeHandleAsString)
        {
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeClientStream(PipeDirection direction, SafePipeHandle safePipeHandle) : base(direction, 0)
        {
            if (direction == PipeDirection.InOut)
            {
                throw new NotSupportedException(System.SR.GetString("NotSupported_AnonymousPipeUnidirectional"));
            }
            if (safePipeHandle == null)
            {
                throw new ArgumentNullException("safePipeHandle");
            }
            if (safePipeHandle.IsInvalid)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidHandle"), "safePipeHandle");
            }
            this.Init(direction, safePipeHandle);
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public AnonymousPipeClientStream(PipeDirection direction, string pipeHandleAsString) : base(direction, 0)
        {
            if (direction == PipeDirection.InOut)
            {
                throw new NotSupportedException(System.SR.GetString("NotSupported_AnonymousPipeUnidirectional"));
            }
            if (pipeHandleAsString == null)
            {
                throw new ArgumentNullException("pipeHandleAsString");
            }
            long result = 0L;
            if (!long.TryParse(pipeHandleAsString, out result))
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidHandle"), "pipeHandleAsString");
            }
            SafePipeHandle safePipeHandle = new SafePipeHandle((IntPtr) result, true);
            if (safePipeHandle.IsInvalid)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidHandle"), "pipeHandleAsString");
            }
            this.Init(direction, safePipeHandle);
        }

        ~AnonymousPipeClientStream()
        {
            this.Dispose(false);
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private void Init(PipeDirection direction, SafePipeHandle safePipeHandle)
        {
            if (Microsoft.Win32.UnsafeNativeMethods.GetFileType(safePipeHandle) != 3)
            {
                throw new IOException(System.SR.GetString("IO_IO_InvalidPipeHandle"));
            }
            base.InitializeHandle(safePipeHandle, true, false);
            base.State = PipeState.Connected;
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


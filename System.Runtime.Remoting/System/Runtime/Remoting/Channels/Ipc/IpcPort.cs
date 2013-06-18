namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    internal class IpcPort : IDisposable
    {
        private bool _cacheable;
        private PipeHandle _handle;
        private string _portName;
        private const string authenticatedUserSidSddlForm = "S-1-5-11";
        private static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(IpcPort.AsyncFSCallback);
        private bool isDisposed;
        private const string networkSidSddlForm = "S-1-5-2";
        private const string prefix = @"\\.\pipe\";
        private static CommonSecurityDescriptor s_securityDescriptor = CreateSecurityDescriptor(null);

        private IpcPort(string portName, PipeHandle handle)
        {
            this._portName = portName;
            this._handle = handle;
            this._cacheable = true;
            ThreadPool.BindHandle(this._handle.Handle);
        }

        private static unsafe void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            PipeAsyncResult asyncResult = (PipeAsyncResult) Overlapped.Unpack(pOverlapped).AsyncResult;
            asyncResult._numBytes = (int) numBytes;
            if (errorCode == 0x6dL)
            {
                errorCode = 0;
            }
            asyncResult._errorCode = (int) errorCode;
            asyncResult._userCallback(asyncResult);
        }

        internal unsafe IAsyncResult BeginRead(byte[] data, int offset, int size, AsyncCallback callback, object state)
        {
            bool flag;
            PipeAsyncResult ar = new PipeAsyncResult(callback);
            NativeOverlapped* lpOverlapped = new Overlapped(0, 0, IntPtr.Zero, ar).UnsafePack(IOCallback, data);
            ar._overlapped = lpOverlapped;
            fixed (byte* numRef = data)
            {
                flag = NativePipe.ReadFile(this._handle, numRef + offset, size, IntPtr.Zero, lpOverlapped);
            }
            if (!flag)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 0x6dL)
                {
                    ar.CallUserCallback();
                    return ar;
                }
                if (errorCode != 0x3e5L)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_ReadFailure"), new object[] { GetMessage(errorCode) }));
                }
            }
            return ar;
        }

        internal static IpcPort Connect(string portName, bool secure, TokenImpersonationLevel impersonationLevel, int timeout)
        {
            string lpFileName = @"\\.\pipe\" + portName;
            uint num = 0x100000;
            if (secure)
            {
                switch (impersonationLevel)
                {
                    case TokenImpersonationLevel.None:
                        num = 0x100000;
                        break;

                    case TokenImpersonationLevel.Identification:
                        num = 0x110000;
                        break;

                    case TokenImpersonationLevel.Impersonation:
                        num = 0x120000;
                        break;

                    case TokenImpersonationLevel.Delegation:
                        num = 0x130000;
                        break;
                }
            }
            while (true)
            {
                PipeHandle handle = NativePipe.CreateFile(lpFileName, 0xc0000000, 3, IntPtr.Zero, 3, 0x40000080 | num, IntPtr.Zero);
                if (handle.Handle.ToInt32() != -1)
                {
                    return new IpcPort(portName, handle);
                }
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 0xe7L)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_ConnectIpcFailed"), new object[] { GetMessage(errorCode) }));
                }
                if (!NativePipe.WaitNamedPipe(lpFileName, timeout))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_Busy"), new object[] { GetMessage(errorCode) }));
                }
            }
        }

        internal static IpcPort Create(string portName, CommonSecurityDescriptor securityDescriptor, bool exclusive)
        {
            SECURITY_ATTRIBUTES security_attributes;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new NotSupportedException(CoreChannel.GetResourceString("Remoting_Ipc_Win9x"));
            }
            PipeHandle handle = null;
            string lpName = @"\\.\pipe\" + portName;
            security_attributes = new SECURITY_ATTRIBUTES {
                nLength = Marshal.SizeOf(security_attributes)
            };
            byte[] binaryForm = null;
            if (securityDescriptor == null)
            {
                securityDescriptor = s_securityDescriptor;
            }
            binaryForm = new byte[securityDescriptor.BinaryLength];
            securityDescriptor.GetBinaryForm(binaryForm, 0);
            GCHandle handle2 = GCHandle.Alloc(binaryForm, GCHandleType.Pinned);
            security_attributes.lpSecurityDescriptor = Marshal.UnsafeAddrOfPinnedArrayElement(binaryForm, 0);
            handle = NativePipe.CreateNamedPipe(lpName, (uint) (0x40000003 | (exclusive ? 0x80000 : 0)), 0, 0xff, 0x2000, 0x2000, uint.MaxValue, security_attributes);
            handle2.Free();
            if (handle.Handle.ToInt32() == -1)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_CreateIpcFailed"), new object[] { GetMessage(errorCode) }));
            }
            return new IpcPort(portName, handle);
        }

        internal static CommonSecurityDescriptor CreateSecurityDescriptor(SecurityIdentifier userSid)
        {
            SecurityIdentifier sid = new SecurityIdentifier("S-1-5-2");
            DiscretionaryAcl discretionaryAcl = new DiscretionaryAcl(false, false, 1);
            discretionaryAcl.AddAccess(AccessControlType.Deny, sid, -1, InheritanceFlags.None, PropagationFlags.None);
            if (userSid != null)
            {
                discretionaryAcl.AddAccess(AccessControlType.Allow, userSid, -1, InheritanceFlags.None, PropagationFlags.None);
            }
            discretionaryAcl.AddAccess(AccessControlType.Allow, WindowsIdentity.GetCurrent().User, -1, InheritanceFlags.None, PropagationFlags.None);
            return new CommonSecurityDescriptor(false, false, ControlFlags.DiscretionaryAclPresent | ControlFlags.GroupDefaulted | ControlFlags.OwnerDefaulted, null, null, null, discretionaryAcl);
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this._handle.Close();
                this.isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        internal unsafe int EndRead(IAsyncResult iar)
        {
            PipeAsyncResult result = iar as PipeAsyncResult;
            NativeOverlapped* nativeOverlappedPtr = result._overlapped;
            if (nativeOverlappedPtr != null)
            {
                Overlapped.Free(nativeOverlappedPtr);
            }
            if (result._errorCode != 0)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_ReadFailure"), new object[] { GetMessage(result._errorCode) }));
            }
            return result._numBytes;
        }

        ~IpcPort()
        {
            this.Dispose();
        }

        internal static string GetMessage(int errorCode)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (NativePipe.FormatMessage(0x3200, NativePipe.NULL, errorCode, 0, lpBuffer, lpBuffer.Capacity, NativePipe.NULL) != 0)
            {
                return lpBuffer.ToString();
            }
            return string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_UnknownError_Num"), new object[] { errorCode.ToString(CultureInfo.InvariantCulture) });
        }

        internal void ImpersonateClient()
        {
            if (!NativePipe.ImpersonateNamedPipeClient(this._handle))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_ImpersonationFailed"), new object[] { GetMessage(errorCode) }));
            }
        }

        internal unsafe int Read(byte[] data, int offset, int length)
        {
            bool flag = false;
            int lpNumberOfBytesRead = 0;
            fixed (byte* numRef = data)
            {
                flag = NativePipe.ReadFile(this._handle, numRef + offset, length, ref lpNumberOfBytesRead, IntPtr.Zero);
            }
            if (!flag)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_ReadFailure"), new object[] { GetMessage(errorCode) }));
            }
            return lpNumberOfBytesRead;
        }

        public bool WaitForConnect()
        {
            if (!NativePipe.ConnectNamedPipe(this._handle, null))
            {
                return (Marshal.GetLastWin32Error() == 0x217L);
            }
            return true;
        }

        internal unsafe void Write(byte[] data, int offset, int size)
        {
            int lpNumberOfBytesWritten = 0;
            bool flag = false;
            fixed (byte* numRef = data)
            {
                flag = NativePipe.WriteFile(this._handle, numRef + offset, size, ref lpNumberOfBytesWritten, IntPtr.Zero);
            }
            if (!flag)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, CoreChannel.GetResourceString("Remoting_Ipc_WriteFailure"), new object[] { GetMessage(errorCode) }));
            }
        }

        internal bool Cacheable
        {
            get
            {
                return this._cacheable;
            }
            set
            {
                this._cacheable = value;
            }
        }

        public bool IsDisposed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isDisposed;
            }
        }

        internal string Name
        {
            get
            {
                return this._portName;
            }
        }
    }
}


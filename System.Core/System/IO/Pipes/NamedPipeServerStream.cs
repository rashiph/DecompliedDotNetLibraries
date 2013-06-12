namespace System.IO.Pipes
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class NamedPipeServerStream : PipeStream
    {
        private static RuntimeHelpers.CleanupCode cleanupCode = new RuntimeHelpers.CleanupCode(NamedPipeServerStream.RevertImpersonationOnBackout);
        public const int MaxAllowedServerInstances = -1;
        private static int s_maxUsernameLength = 20;
        private static RuntimeHelpers.TryCode tryCode = new RuntimeHelpers.TryCode(NamedPipeServerStream.ImpersonateAndTryCode);
        private static readonly IOCompletionCallback WaitForConnectionCallback = new IOCompletionCallback(NamedPipeServerStream.AsyncWaitForConnectionCallback);

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName) : this(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, null, HandleInheritability.None, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName, PipeDirection direction) : this(pipeName, direction, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, null, HandleInheritability.None, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances) : this(pipeName, direction, maxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, null, HandleInheritability.None, 0)
        {
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle) : base(direction, PipeTransmissionMode.Byte, 0)
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
        public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode) : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, PipeOptions.None, 0, 0, null, HandleInheritability.None, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options) : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, 0, 0, null, HandleInheritability.None, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize) : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, null, HandleInheritability.None, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity) : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, HandleInheritability.None, 0)
        {
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability) : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, inheritability, 0)
        {
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights) : base(direction, transmissionMode, outBufferSize)
        {
            if (pipeName == null)
            {
                throw new ArgumentNullException("pipeName");
            }
            if (pipeName.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NeedNonemptyPipeName"));
            }
            if ((options & ~(PipeOptions.Asynchronous | PipeOptions.WriteThrough)) != PipeOptions.None)
            {
                throw new ArgumentOutOfRangeException("options", System.SR.GetString("ArgumentOutOfRange_OptionsInvalid"));
            }
            if (inBufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("inBufferSize", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (((maxNumberOfServerInstances < 1) || (maxNumberOfServerInstances > 0xfe)) && (maxNumberOfServerInstances != -1))
            {
                throw new ArgumentOutOfRangeException("maxNumberOfServerInstances", System.SR.GetString("ArgumentOutOfRange_MaxNumServerInstances"));
            }
            if ((inheritability < HandleInheritability.None) || (inheritability > HandleInheritability.Inheritable))
            {
                throw new ArgumentOutOfRangeException("inheritability", System.SR.GetString("ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable"));
            }
            if ((additionalAccessRights & ~(PipeAccessRights.AccessSystemSecurity | PipeAccessRights.TakeOwnership | PipeAccessRights.ChangePermissions)) != 0)
            {
                throw new ArgumentOutOfRangeException("additionalAccessRights", System.SR.GetString("ArgumentOutOfRange_AdditionalAccessLimited"));
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("PlatformNotSupported_NamedPipeServers"));
            }
            string fullPath = Path.GetFullPath(@"\\.\pipe\" + pipeName);
            if (string.Compare(fullPath, @"\\.\pipe\anonymous", StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new ArgumentOutOfRangeException("pipeName", System.SR.GetString("ArgumentOutOfRange_AnonymousReserved"));
            }
            object pinningHandle = null;
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs(inheritability, pipeSecurity, out pinningHandle);
            try
            {
                this.Create(fullPath, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, additionalAccessRights, secAttrs);
            }
            finally
            {
                if (pinningHandle != null)
                {
                    ((GCHandle) pinningHandle).Free();
                }
            }
        }

        [SecurityCritical]
        private static unsafe void AsyncWaitForConnectionCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            PipeAsyncResult asyncResult = (PipeAsyncResult) Overlapped.Unpack(pOverlapped).AsyncResult;
            if (errorCode == 0x217)
            {
                errorCode = 0;
            }
            asyncResult._errorCode = (int) errorCode;
            asyncResult._completedSynchronously = false;
            asyncResult._isComplete = true;
            ManualResetEvent event2 = asyncResult._waitHandle;
            if ((event2 != null) && !event2.Set())
            {
                System.IO.__Error.WinIOError();
            }
            AsyncCallback callback = asyncResult._userCallback;
            if (callback != null)
            {
                callback(asyncResult);
            }
        }

        [SecurityCritical, HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public unsafe IAsyncResult BeginWaitForConnection(AsyncCallback callback, object state)
        {
            this.CheckConnectOperationsServer();
            if (!base.IsAsync)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeNotAsync"));
            }
            PipeAsyncResult ar = new PipeAsyncResult {
                _handle = base.InternalHandle,
                _userCallback = callback,
                _userStateObject = state
            };
            ManualResetEvent event2 = new ManualResetEvent(false);
            ar._waitHandle = event2;
            NativeOverlapped* overlapped = new Overlapped(0, 0, IntPtr.Zero, ar).Pack(WaitForConnectionCallback, null);
            ar._overlapped = overlapped;
            if (!Microsoft.Win32.UnsafeNativeMethods.ConnectNamedPipe(base.InternalHandle, overlapped))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 0x217)
                {
                    overlapped->InternalLow = IntPtr.Zero;
                    if (base.State == PipeState.Connected)
                    {
                        throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeAlreadyConnected"));
                    }
                    ar.CallUserCallback();
                    return ar;
                }
                if (errorCode != 0x3e5)
                {
                    System.IO.__Error.WinIOError(errorCode, string.Empty);
                }
            }
            return ar;
        }

        [SecurityCritical]
        private void CheckConnectOperationsServer()
        {
            if (base.InternalHandle == null)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
            }
            if (base.State == PipeState.Closed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            if (base.InternalHandle.IsClosed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            if (base.State == PipeState.Broken)
            {
                throw new IOException(System.SR.GetString("IO_IO_PipeBroken"));
            }
        }

        [SecurityCritical]
        private void CheckDisconnectOperations()
        {
            if (base.State == PipeState.WaitingToConnect)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeNotYetConnected"));
            }
            if (base.State == PipeState.Disconnected)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeAlreadyDisconnected"));
            }
            if (base.InternalHandle == null)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
            }
            if (base.State == PipeState.Closed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            if (base.InternalHandle.IsClosed)
            {
                System.IO.__Error.PipeNotOpen();
            }
        }

        [SecurityCritical]
        private void Create(string fullPipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeAccessRights rights, Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES secAttrs)
        {
            int openMode = (int) (((direction | ((maxNumberOfServerInstances == 1) ? ((PipeDirection) 0x80000) : ((PipeDirection) 0))) | ((PipeDirection) ((int) options))) | ((PipeDirection) ((int) rights)));
            int pipeMode = (((int) transmissionMode) << 2) | (((int) transmissionMode) << 1);
            if (maxNumberOfServerInstances == -1)
            {
                maxNumberOfServerInstances = 0xff;
            }
            SafePipeHandle handle = Microsoft.Win32.UnsafeNativeMethods.CreateNamedPipe(fullPipeName, openMode, pipeMode, maxNumberOfServerInstances, outBufferSize, inBufferSize, 0, secAttrs);
            if (handle.IsInvalid)
            {
                System.IO.__Error.WinIOError(Marshal.GetLastWin32Error(), string.Empty);
            }
            base.InitializeHandle(handle, false, (options & PipeOptions.Asynchronous) != PipeOptions.None);
        }

        [SecurityCritical]
        public void Disconnect()
        {
            this.CheckDisconnectOperations();
            if (!Microsoft.Win32.UnsafeNativeMethods.DisconnectNamedPipe(base.InternalHandle))
            {
                System.IO.__Error.WinIOError(Marshal.GetLastWin32Error(), string.Empty);
            }
            base.State = PipeState.Disconnected;
        }

        [SecurityCritical]
        public unsafe void EndWaitForConnection(IAsyncResult asyncResult)
        {
            this.CheckConnectOperationsServer();
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!base.IsAsync)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeNotAsync"));
            }
            PipeAsyncResult result = asyncResult as PipeAsyncResult;
            if (result == null)
            {
                System.IO.__Error.WrongAsyncResult();
            }
            if (1 == Interlocked.CompareExchange(ref result._EndXxxCalled, 1, 0))
            {
                System.IO.__Error.EndWaitForConnectionCalledTwice();
            }
            WaitHandle handle = result._waitHandle;
            if (handle != null)
            {
                try
                {
                    handle.WaitOne();
                }
                finally
                {
                    handle.Close();
                }
            }
            NativeOverlapped* nativeOverlappedPtr = result._overlapped;
            if (nativeOverlappedPtr != null)
            {
                Overlapped.Free(nativeOverlappedPtr);
            }
            if (result._errorCode != 0)
            {
                System.IO.__Error.WinIOError(result._errorCode, string.Empty);
            }
            base.State = PipeState.Connected;
        }

        ~NamedPipeServerStream()
        {
            this.Dispose(false);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public string GetImpersonationUserName()
        {
            base.CheckWriteOperations();
            StringBuilder lpUserName = new StringBuilder(0x202);
            if (!Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeHandleState(base.InternalHandle, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, lpUserName, lpUserName.Capacity))
            {
                base.WinIOError(Marshal.GetLastWin32Error());
            }
            return lpUserName.ToString();
        }

        [SecurityCritical]
        private static void ImpersonateAndTryCode(object helper)
        {
            ExecuteHelper helper2 = (ExecuteHelper) helper;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (Microsoft.Win32.UnsafeNativeMethods.ImpersonateNamedPipeClient(helper2.m_handle))
                {
                    helper2.m_mustRevert = true;
                }
                else
                {
                    helper2.m_impersonateErrorCode = Marshal.GetLastWin32Error();
                }
            }
            if (helper2.m_mustRevert)
            {
                helper2.m_userCode();
            }
        }

        [PrePrepareMethod, SecurityCritical]
        private static void RevertImpersonationOnBackout(object helper, bool exceptionThrown)
        {
            ExecuteHelper helper2 = (ExecuteHelper) helper;
            if (helper2.m_mustRevert && !Microsoft.Win32.UnsafeNativeMethods.RevertToSelf())
            {
                helper2.m_revertImpersonateErrorCode = Marshal.GetLastWin32Error();
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public void RunAsClient(PipeStreamImpersonationWorker impersonationWorker)
        {
            base.CheckWriteOperations();
            ExecuteHelper userData = new ExecuteHelper(impersonationWorker, base.InternalHandle);
            RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(tryCode, cleanupCode, userData);
            if (userData.m_impersonateErrorCode != 0)
            {
                base.WinIOError(userData.m_impersonateErrorCode);
            }
            else if (userData.m_revertImpersonateErrorCode != 0)
            {
                base.WinIOError(userData.m_revertImpersonateErrorCode);
            }
        }

        [SecurityCritical]
        public void WaitForConnection()
        {
            this.CheckConnectOperationsServer();
            if (base.IsAsync)
            {
                IAsyncResult asyncResult = this.BeginWaitForConnection(null, null);
                this.EndWaitForConnection(asyncResult);
            }
            else
            {
                if (!Microsoft.Win32.UnsafeNativeMethods.ConnectNamedPipe(base.InternalHandle, Microsoft.Win32.UnsafeNativeMethods.NULL))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if (errorCode != 0x217)
                    {
                        System.IO.__Error.WinIOError(errorCode, string.Empty);
                    }
                    if ((errorCode == 0x217) && (base.State == PipeState.Connected))
                    {
                        throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeAlreadyConnected"));
                    }
                }
                base.State = PipeState.Connected;
            }
        }

        internal class ExecuteHelper
        {
            internal SafePipeHandle m_handle;
            internal int m_impersonateErrorCode;
            internal bool m_mustRevert;
            internal int m_revertImpersonateErrorCode;
            internal PipeStreamImpersonationWorker m_userCode;

            [SecurityCritical]
            internal ExecuteHelper(PipeStreamImpersonationWorker userCode, SafePipeHandle handle)
            {
                this.m_userCode = userCode;
                this.m_handle = handle;
            }
        }
    }
}


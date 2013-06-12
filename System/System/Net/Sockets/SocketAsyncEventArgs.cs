namespace System.Net.Sockets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    public class SocketAsyncEventArgs : EventArgs, IDisposable
    {
        private const int Configuring = -1;
        private const int Disposed = 2;
        private const int Free = 0;
        private const int InProgress = 1;
        internal int m_AcceptAddressBufferCount;
        internal byte[] m_AcceptBuffer;
        internal Socket m_AcceptSocket;
        internal byte[] m_Buffer;
        internal IList<ArraySegment<byte>> m_BufferList;
        private bool m_BufferListChanged;
        private int m_BytesTransferred;
        private bool m_CompletedChanged;
        private SocketAsyncOperation m_CompletedOperation;
        private Exception m_ConnectByNameError;
        private Socket m_ConnectSocket;
        private ExecutionContext m_Context;
        private ExecutionContext m_ContextCopy;
        private byte[] m_ControlBuffer;
        private GCHandle m_ControlBufferGCHandle;
        internal int m_Count;
        private Socket m_CurrentSocket;
        private bool m_DisconnectReuseSocket;
        private bool m_DisposeCalled;
        private ContextCallback m_ExecutionCallback;
        private MultipleConnectAsync m_MultipleConnect;
        private object[] m_ObjectsToPin;
        internal int m_Offset;
        private int m_Operating;
        private Overlapped m_Overlapped;
        private byte[] m_PinnedAcceptBuffer;
        private byte[] m_PinnedSingleBuffer;
        private int m_PinnedSingleBufferCount;
        private int m_PinnedSingleBufferOffset;
        private SocketAddress m_PinnedSocketAddress;
        private PinState m_PinState;
        internal IntPtr m_PtrAcceptBuffer;
        internal IntPtr m_PtrControlBuffer;
        internal SafeNativeOverlapped m_PtrNativeOverlapped;
        internal IntPtr m_PtrSendPacketsDescriptor;
        internal IntPtr m_PtrSingleBuffer;
        internal IntPtr m_PtrSocketAddressBuffer;
        internal IntPtr m_PtrSocketAddressBufferSize;
        internal IntPtr m_PtrWSAMessageBuffer;
        private IntPtr m_PtrWSARecvMsgWSABufferArray;
        private IPPacketInformation m_ReceiveMessageFromPacketInfo;
        private EndPoint m_RemoteEndPoint;
        private UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElement[] m_SendPacketsDescriptor;
        internal SendPacketsElement[] m_SendPacketsElements;
        internal int m_SendPacketsElementsBufferCount;
        internal int m_SendPacketsElementsFileCount;
        private SendPacketsElement[] m_SendPacketsElementsInternal;
        internal SafeHandle[] m_SendPacketsFileHandles;
        internal FileStream[] m_SendPacketsFileStreams;
        internal TransmitFileOptions m_SendPacketsFlags;
        internal int m_SendPacketsSendSize;
        internal SocketAddress m_SocketAddress;
        private GCHandle m_SocketAddressGCHandle;
        private System.Net.Sockets.SocketError m_SocketError;
        internal System.Net.Sockets.SocketFlags m_SocketFlags;
        private object m_UserToken;
        internal WSABuffer m_WSABuffer;
        internal WSABuffer[] m_WSABufferArray;
        private byte[] m_WSAMessageBuffer;
        private GCHandle m_WSAMessageBufferGCHandle;
        private WSABuffer[] m_WSARecvMsgWSABufferArray;
        private GCHandle m_WSARecvMsgWSABufferArrayGCHandle;
        internal static readonly int s_ControlDataIPv6Size = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6));
        internal static readonly int s_ControlDataSize = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.ControlData));
        private static bool s_LoggingEnabled = Logging.On;
        internal static readonly int s_WSAMsgSize = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.WSAMsg));

        public event EventHandler<SocketAsyncEventArgs> Completed
        {
            add
            {
                this.m_Completed += value;
                this.m_CompletedChanged = true;
            }
            remove
            {
                this.m_Completed -= value;
                this.m_CompletedChanged = true;
            }
        }

        private event EventHandler<SocketAsyncEventArgs> m_Completed;

        public SocketAsyncEventArgs()
        {
            if (!ComNetOS.IsPostWin2K)
            {
                throw new NotSupportedException(SR.GetString("WinXPRequired"));
            }
            this.m_ExecutionCallback = new ContextCallback(this.ExecutionCallback);
            this.m_SendPacketsSendSize = -1;
        }

        internal void CancelConnectAsync()
        {
            if ((this.m_Operating == 1) && (this.m_CompletedOperation == SocketAsyncOperation.Connect))
            {
                if (this.m_MultipleConnect != null)
                {
                    this.m_MultipleConnect.Cancel();
                }
                else
                {
                    this.m_CurrentSocket.Close();
                }
            }
        }

        private void CheckPinMultipleBuffers()
        {
            if (this.m_BufferList == null)
            {
                if (this.m_PinState == PinState.MultipleBuffer)
                {
                    this.FreeOverlapped(false);
                }
            }
            else if ((this.m_PinState != PinState.MultipleBuffer) || this.m_BufferListChanged)
            {
                this.m_BufferListChanged = false;
                this.FreeOverlapped(false);
                try
                {
                    this.SetupOverlappedMultiple();
                }
                catch (Exception)
                {
                    this.FreeOverlapped(false);
                    throw;
                }
            }
        }

        private void CheckPinNoBuffer()
        {
            if (this.m_PinState == PinState.None)
            {
                this.SetupOverlappedSingle(true);
            }
        }

        private void CheckPinSendPackets()
        {
            if (this.m_PinState != PinState.None)
            {
                this.FreeOverlapped(false);
            }
            this.SetupOverlappedSendPackets();
        }

        private void CheckPinSingleBuffer(bool pinUsersBuffer)
        {
            if (pinUsersBuffer)
            {
                if (this.m_Buffer != null)
                {
                    if ((this.m_PinState != PinState.SingleBuffer) || (this.m_PinnedSingleBuffer != this.m_Buffer))
                    {
                        this.FreeOverlapped(false);
                        this.SetupOverlappedSingle(true);
                    }
                    else
                    {
                        if (this.m_Offset != this.m_PinnedSingleBufferOffset)
                        {
                            this.m_PinnedSingleBufferOffset = this.m_Offset;
                            this.m_PtrSingleBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_Buffer, this.m_Offset);
                            this.m_WSABuffer.Pointer = this.m_PtrSingleBuffer;
                        }
                        if (this.m_Count != this.m_PinnedSingleBufferCount)
                        {
                            this.m_PinnedSingleBufferCount = this.m_Count;
                            this.m_WSABuffer.Length = this.m_Count;
                        }
                    }
                }
                else if (this.m_PinState == PinState.SingleBuffer)
                {
                    this.FreeOverlapped(false);
                }
            }
            else if ((this.m_PinState != PinState.SingleAcceptBuffer) || (this.m_PinnedSingleBuffer != this.m_AcceptBuffer))
            {
                this.FreeOverlapped(false);
                this.SetupOverlappedSingle(false);
            }
        }

        internal void Complete()
        {
            this.m_Operating = 0;
            if (this.m_DisposeCalled)
            {
                this.Dispose();
            }
        }

        private unsafe void CompletionPortCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            System.Net.Sockets.SocketFlags none = System.Net.Sockets.SocketFlags.None;
            System.Net.Sockets.SocketError socketError = (System.Net.Sockets.SocketError) errorCode;
            switch (socketError)
            {
                case System.Net.Sockets.SocketError.Success:
                    this.FinishOperationSuccess(socketError, (int) numBytes, none);
                    return;

                case System.Net.Sockets.SocketError.OperationAborted:
                    break;

                default:
                    if (this.m_CurrentSocket.CleanedUp)
                    {
                        socketError = System.Net.Sockets.SocketError.OperationAborted;
                    }
                    else
                    {
                        try
                        {
                            UnsafeNclNativeMethods.OSSOCK.WSAGetOverlappedResult(this.m_CurrentSocket.SafeHandle, this.m_PtrNativeOverlapped, out numBytes, false, out none);
                            socketError = (System.Net.Sockets.SocketError) Marshal.GetLastWin32Error();
                        }
                        catch
                        {
                            socketError = System.Net.Sockets.SocketError.OperationAborted;
                        }
                    }
                    break;
            }
            this.FinishOperationAsyncFailure(socketError, (int) numBytes, none);
        }

        public void Dispose()
        {
            this.m_DisposeCalled = true;
            if (Interlocked.CompareExchange(ref this.m_Operating, 2, 0) == 0)
            {
                this.FreeOverlapped(false);
                GC.SuppressFinalize(this);
            }
        }

        private void ExecutionCallback(object ignored)
        {
            this.OnCompleted(this);
        }

        ~SocketAsyncEventArgs()
        {
            this.FreeOverlapped(true);
        }

        internal void FinishConnectByNameSyncFailure(Exception exception, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.SetResults(exception, bytesTransferred, flags);
            if (this.m_CurrentSocket != null)
            {
                this.m_CurrentSocket.UpdateStatusAfterSocketError(this.m_SocketError);
            }
            this.Complete();
        }

        internal void FinishOperationAsyncFailure(Exception exception, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.SetResults(exception, bytesTransferred, flags);
            if (this.m_CurrentSocket != null)
            {
                this.m_CurrentSocket.UpdateStatusAfterSocketError(this.m_SocketError);
            }
            this.Complete();
            if (this.m_Context == null)
            {
                this.OnCompleted(this);
            }
            else
            {
                ExecutionContext.Run(this.m_ContextCopy, this.m_ExecutionCallback, null);
            }
        }

        internal void FinishOperationAsyncFailure(System.Net.Sockets.SocketError socketError, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.SetResults(socketError, bytesTransferred, flags);
            if (this.m_CurrentSocket != null)
            {
                this.m_CurrentSocket.UpdateStatusAfterSocketError(socketError);
            }
            this.Complete();
            if (this.m_Context == null)
            {
                this.OnCompleted(this);
            }
            else
            {
                ExecutionContext.Run(this.m_ContextCopy, this.m_ExecutionCallback, null);
            }
        }

        internal unsafe void FinishOperationSuccess(System.Net.Sockets.SocketError socketError, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.SetResults(socketError, bytesTransferred, flags);
            switch (this.m_CompletedOperation)
            {
                case SocketAsyncOperation.Accept:
                {
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogBuffer(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, false);
                        }
                    }
                    SocketAddress socketAddress = this.m_CurrentSocket.m_RightEndPoint.Serialize();
                    try
                    {
                        IntPtr ptr;
                        int num;
                        IntPtr ptr2;
                        this.m_CurrentSocket.GetAcceptExSockaddrs((this.m_PtrSingleBuffer != IntPtr.Zero) ? this.m_PtrSingleBuffer : this.m_PtrAcceptBuffer, (this.m_Count != 0) ? (this.m_Count - this.m_AcceptAddressBufferCount) : 0, this.m_AcceptAddressBufferCount / 2, this.m_AcceptAddressBufferCount / 2, out ptr, out num, out ptr2, out socketAddress.m_Size);
                        Marshal.Copy(ptr2, socketAddress.m_Buffer, 0, socketAddress.m_Size);
                        IntPtr handle = this.m_CurrentSocket.SafeHandle.DangerousGetHandle();
                        socketError = UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_AcceptSocket.SafeHandle, SocketOptionLevel.Socket, SocketOptionName.UpdateAcceptContext, ref handle, Marshal.SizeOf(handle));
                        if (socketError == System.Net.Sockets.SocketError.SocketError)
                        {
                            socketError = (System.Net.Sockets.SocketError) Marshal.GetLastWin32Error();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        socketError = System.Net.Sockets.SocketError.OperationAborted;
                    }
                    if (socketError == System.Net.Sockets.SocketError.Success)
                    {
                        this.m_AcceptSocket = this.m_CurrentSocket.UpdateAcceptSocket(this.m_AcceptSocket, this.m_CurrentSocket.m_RightEndPoint.Create(socketAddress), false);
                        if (s_LoggingEnabled)
                        {
                            Logging.PrintInfo(Logging.Sockets, this.m_AcceptSocket, SR.GetString("net_log_socket_accepted", new object[] { this.m_AcceptSocket.RemoteEndPoint, this.m_AcceptSocket.LocalEndPoint }));
                        }
                    }
                    else
                    {
                        this.SetResults(socketError, bytesTransferred, System.Net.Sockets.SocketFlags.None);
                        this.m_AcceptSocket = null;
                    }
                    break;
                }
                case SocketAsyncOperation.Connect:
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogBuffer(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, true);
                        }
                    }
                    try
                    {
                        socketError = UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_CurrentSocket.SafeHandle, SocketOptionLevel.Socket, SocketOptionName.UpdateConnectContext, (byte[]) null, 0);
                        if (socketError == System.Net.Sockets.SocketError.SocketError)
                        {
                            socketError = (System.Net.Sockets.SocketError) Marshal.GetLastWin32Error();
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        socketError = System.Net.Sockets.SocketError.OperationAborted;
                    }
                    if (socketError == System.Net.Sockets.SocketError.Success)
                    {
                        if (s_LoggingEnabled)
                        {
                            Logging.PrintInfo(Logging.Sockets, this.m_CurrentSocket, SR.GetString("net_log_socket_connected", new object[] { this.m_CurrentSocket.LocalEndPoint, this.m_CurrentSocket.RemoteEndPoint }));
                        }
                        this.m_CurrentSocket.SetToConnected();
                        this.m_ConnectSocket = this.m_CurrentSocket;
                    }
                    break;

                case SocketAsyncOperation.Disconnect:
                    this.m_CurrentSocket.SetToDisconnected();
                    this.m_CurrentSocket.m_RemoteEndPoint = null;
                    break;

                case SocketAsyncOperation.Receive:
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogBuffer(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, false);
                        }
                    }
                    break;

                case SocketAsyncOperation.ReceiveFrom:
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogBuffer(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, false);
                        }
                    }
                    this.m_SocketAddress.SetSize(this.m_PtrSocketAddressBufferSize);
                    if (!this.m_RemoteEndPoint.Serialize().Equals(this.m_SocketAddress))
                    {
                        try
                        {
                            this.m_RemoteEndPoint = this.m_RemoteEndPoint.Create(this.m_SocketAddress);
                        }
                        catch
                        {
                        }
                    }
                    break;

                case SocketAsyncOperation.ReceiveMessageFrom:
                {
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogBuffer(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, false);
                        }
                    }
                    this.m_SocketAddress.SetSize(this.m_PtrSocketAddressBufferSize);
                    if (!this.m_RemoteEndPoint.Serialize().Equals(this.m_SocketAddress))
                    {
                        try
                        {
                            this.m_RemoteEndPoint = this.m_RemoteEndPoint.Create(this.m_SocketAddress);
                        }
                        catch
                        {
                        }
                    }
                    IPAddress address3 = null;
                    UnsafeNclNativeMethods.OSSOCK.WSAMsg* msgPtr = (UnsafeNclNativeMethods.OSSOCK.WSAMsg*) Marshal.UnsafeAddrOfPinnedArrayElement(this.m_WSAMessageBuffer, 0);
                    if (this.m_ControlBuffer.Length == s_ControlDataSize)
                    {
                        UnsafeNclNativeMethods.OSSOCK.ControlData data = (UnsafeNclNativeMethods.OSSOCK.ControlData) Marshal.PtrToStructure(msgPtr->controlBuffer.Pointer, typeof(UnsafeNclNativeMethods.OSSOCK.ControlData));
                        if (data.length != UIntPtr.Zero)
                        {
                            address3 = new IPAddress((long) data.address);
                        }
                        this.m_ReceiveMessageFromPacketInfo = new IPPacketInformation((address3 != null) ? address3 : IPAddress.None, (int) data.index);
                    }
                    else if (this.m_ControlBuffer.Length == s_ControlDataIPv6Size)
                    {
                        UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6 pv = (UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6) Marshal.PtrToStructure(msgPtr->controlBuffer.Pointer, typeof(UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6));
                        if (pv.length != UIntPtr.Zero)
                        {
                            address3 = new IPAddress(pv.address);
                        }
                        this.m_ReceiveMessageFromPacketInfo = new IPPacketInformation((address3 != null) ? address3 : IPAddress.IPv6None, (int) pv.index);
                    }
                    else
                    {
                        this.m_ReceiveMessageFromPacketInfo = new IPPacketInformation();
                    }
                    break;
                }
                case SocketAsyncOperation.Send:
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogBuffer(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, true);
                        }
                    }
                    break;

                case SocketAsyncOperation.SendPackets:
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogSendPacketsBuffers(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, true);
                        }
                    }
                    if (this.m_SendPacketsFileStreams != null)
                    {
                        for (int i = 0; i < this.m_SendPacketsElementsFileCount; i++)
                        {
                            this.m_SendPacketsFileHandles[i] = null;
                            if (this.m_SendPacketsFileStreams[i] != null)
                            {
                                this.m_SendPacketsFileStreams[i].Close();
                                this.m_SendPacketsFileStreams[i] = null;
                            }
                        }
                    }
                    this.m_SendPacketsFileStreams = null;
                    this.m_SendPacketsFileHandles = null;
                    break;

                case SocketAsyncOperation.SendTo:
                    if (bytesTransferred > 0)
                    {
                        if (s_LoggingEnabled)
                        {
                            this.LogBuffer(bytesTransferred);
                        }
                        if (Socket.s_PerfCountersEnabled)
                        {
                            this.UpdatePerfCounters(bytesTransferred, true);
                        }
                    }
                    break;
            }
            if (socketError != System.Net.Sockets.SocketError.Success)
            {
                this.SetResults(socketError, bytesTransferred, flags);
                this.m_CurrentSocket.UpdateStatusAfterSocketError(socketError);
            }
            this.Complete();
            if (this.m_ContextCopy == null)
            {
                this.OnCompleted(this);
            }
            else
            {
                ExecutionContext.Run(this.m_ContextCopy, this.m_ExecutionCallback, null);
            }
        }

        internal void FinishOperationSyncFailure(System.Net.Sockets.SocketError socketError, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.SetResults(socketError, bytesTransferred, flags);
            if (this.m_CurrentSocket != null)
            {
                this.m_CurrentSocket.UpdateStatusAfterSocketError(socketError);
            }
            this.Complete();
        }

        internal void FinishWrapperConnectSuccess(Socket connectSocket, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.SetResults(System.Net.Sockets.SocketError.Success, bytesTransferred, flags);
            this.m_CurrentSocket = connectSocket;
            this.m_ConnectSocket = connectSocket;
            this.Complete();
            if (this.m_ContextCopy == null)
            {
                this.OnCompleted(this);
            }
            else
            {
                ExecutionContext.Run(this.m_ContextCopy, this.m_ExecutionCallback, null);
            }
        }

        private void FreeOverlapped(bool checkForShutdown)
        {
            if (!checkForShutdown || !NclUtilities.HasShutdownStarted)
            {
                if ((this.m_PtrNativeOverlapped != null) && !this.m_PtrNativeOverlapped.IsInvalid)
                {
                    this.m_PtrNativeOverlapped.Dispose();
                    this.m_PtrNativeOverlapped = null;
                    this.m_Overlapped = null;
                    this.m_PinState = PinState.None;
                    this.m_PinnedAcceptBuffer = null;
                    this.m_PinnedSingleBuffer = null;
                    this.m_PinnedSingleBufferOffset = 0;
                    this.m_PinnedSingleBufferCount = 0;
                }
                if (this.m_SocketAddressGCHandle.IsAllocated)
                {
                    this.m_SocketAddressGCHandle.Free();
                }
                if (this.m_WSAMessageBufferGCHandle.IsAllocated)
                {
                    this.m_WSAMessageBufferGCHandle.Free();
                }
                if (this.m_WSARecvMsgWSABufferArrayGCHandle.IsAllocated)
                {
                    this.m_WSARecvMsgWSABufferArrayGCHandle.Free();
                }
                if (this.m_ControlBufferGCHandle.IsAllocated)
                {
                    this.m_ControlBufferGCHandle.Free();
                }
            }
        }

        internal void LogBuffer(int size)
        {
            WSABuffer[] wSABufferArray;
            int num;
            switch (this.m_PinState)
            {
                case PinState.SingleAcceptBuffer:
                    Logging.Dump(Logging.Sockets, this.m_CurrentSocket, "FinishOperation(" + this.m_CompletedOperation + "Async)", this.m_AcceptBuffer, 0, size);
                    return;

                case PinState.SingleBuffer:
                    Logging.Dump(Logging.Sockets, this.m_CurrentSocket, "FinishOperation(" + this.m_CompletedOperation + "Async)", this.m_Buffer, this.m_Offset, size);
                    return;

                case PinState.MultipleBuffer:
                    wSABufferArray = this.m_WSABufferArray;
                    num = 0;
                    break;

                default:
                    return;
            }
            while (num < wSABufferArray.Length)
            {
                WSABuffer buffer = wSABufferArray[num];
                Logging.Dump(Logging.Sockets, this.m_CurrentSocket, "FinishOperation(" + this.m_CompletedOperation + "Async)", buffer.Pointer, Math.Min(buffer.Length, size));
                if ((size -= buffer.Length) <= 0)
                {
                    return;
                }
                num++;
            }
        }

        internal void LogSendPacketsBuffers(int size)
        {
            foreach (SendPacketsElement element in this.m_SendPacketsElementsInternal)
            {
                if (element != null)
                {
                    if ((element.m_Buffer != null) && (element.m_Count > 0))
                    {
                        Logging.Dump(Logging.Sockets, this.m_CurrentSocket, "FinishOperation(" + this.m_CompletedOperation + "Async)Buffer", element.m_Buffer, element.m_Offset, Math.Min(element.m_Count, size));
                    }
                    else if ((element.m_FilePath != null) && (element.m_FilePath.Length != 0))
                    {
                        Logging.PrintInfo(Logging.Sockets, this.m_CurrentSocket, "FinishOperation(" + this.m_CompletedOperation + "Async)", "Not logging data from file: " + element.m_FilePath);
                    }
                }
            }
        }

        protected virtual void OnCompleted(SocketAsyncEventArgs e)
        {
            EventHandler<SocketAsyncEventArgs> completed = this.m_Completed;
            if (completed != null)
            {
                completed(e.m_CurrentSocket, e);
            }
        }

        private void PinSocketAddressBuffer()
        {
            if (this.m_PinnedSocketAddress != this.m_SocketAddress)
            {
                if (this.m_SocketAddressGCHandle.IsAllocated)
                {
                    this.m_SocketAddressGCHandle.Free();
                }
                this.m_SocketAddressGCHandle = GCHandle.Alloc(this.m_SocketAddress.m_Buffer, GCHandleType.Pinned);
                this.m_SocketAddress.CopyAddressSizeIntoBuffer();
                this.m_PtrSocketAddressBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_SocketAddress.m_Buffer, 0);
                this.m_PtrSocketAddressBufferSize = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_SocketAddress.m_Buffer, this.m_SocketAddress.GetAddressSizeOffset());
                this.m_PinnedSocketAddress = this.m_SocketAddress;
            }
        }

        public void SetBuffer(int offset, int count)
        {
            this.SetBufferInternal(this.m_Buffer, offset, count);
        }

        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            this.SetBufferInternal(buffer, offset, count);
        }

        private void SetBufferInternal(byte[] buffer, int offset, int count)
        {
            this.StartConfiguring();
            try
            {
                if (buffer == null)
                {
                    this.m_Buffer = null;
                    this.m_Offset = 0;
                    this.m_Count = 0;
                }
                else
                {
                    if (this.m_BufferList != null)
                    {
                        throw new ArgumentException(SR.GetString("net_ambiguousbuffers", new object[] { "BufferList" }));
                    }
                    if ((offset < 0) || (offset > buffer.Length))
                    {
                        throw new ArgumentOutOfRangeException("offset");
                    }
                    if ((count < 0) || (count > (buffer.Length - offset)))
                    {
                        throw new ArgumentOutOfRangeException("count");
                    }
                    this.m_Buffer = buffer;
                    this.m_Offset = offset;
                    this.m_Count = count;
                }
                this.CheckPinSingleBuffer(true);
            }
            finally
            {
                this.Complete();
            }
        }

        internal void SetResults(Exception exception, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.m_ConnectByNameError = exception;
            this.m_BytesTransferred = bytesTransferred;
            this.m_SocketFlags = flags;
            if (exception == null)
            {
                this.m_SocketError = System.Net.Sockets.SocketError.Success;
            }
            else
            {
                SocketException exception2 = exception as SocketException;
                if (exception2 != null)
                {
                    this.m_SocketError = exception2.SocketErrorCode;
                }
                else
                {
                    this.m_SocketError = System.Net.Sockets.SocketError.SocketError;
                }
            }
        }

        internal void SetResults(System.Net.Sockets.SocketError socketError, int bytesTransferred, System.Net.Sockets.SocketFlags flags)
        {
            this.m_SocketError = socketError;
            this.m_ConnectByNameError = null;
            this.m_BytesTransferred = bytesTransferred;
            this.m_SocketFlags = flags;
        }

        private void SetupOverlappedMultiple()
        {
            this.m_Overlapped = new Overlapped();
            ArraySegment<byte>[] array = new ArraySegment<byte>[this.m_BufferList.Count];
            this.m_BufferList.CopyTo(array, 0);
            if ((this.m_ObjectsToPin == null) || (this.m_ObjectsToPin.Length != array.Length))
            {
                this.m_ObjectsToPin = new object[array.Length];
            }
            for (int i = 0; i < array.Length; i++)
            {
                this.m_ObjectsToPin[i] = array[i].Array;
            }
            if ((this.m_WSABufferArray == null) || (this.m_WSABufferArray.Length != array.Length))
            {
                this.m_WSABufferArray = new WSABuffer[array.Length];
            }
            this.m_PtrNativeOverlapped = new SafeNativeOverlapped(this.m_Overlapped.UnsafePack(new IOCompletionCallback(this.CompletionPortCallback), this.m_ObjectsToPin));
            for (int j = 0; j < array.Length; j++)
            {
                ArraySegment<byte> segment = array[j];
                ValidationHelper.ValidateSegment(segment);
                this.m_WSABufferArray[j].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(segment.Array, segment.Offset);
                this.m_WSABufferArray[j].Length = segment.Count;
            }
            this.m_PinState = PinState.MultipleBuffer;
        }

        private void SetupOverlappedSendPackets()
        {
            this.m_Overlapped = new Overlapped();
            this.m_SendPacketsDescriptor = new UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElement[this.m_SendPacketsElementsFileCount + this.m_SendPacketsElementsBufferCount];
            if ((this.m_ObjectsToPin == null) || (this.m_ObjectsToPin.Length != (this.m_SendPacketsElementsBufferCount + 1)))
            {
                this.m_ObjectsToPin = new object[this.m_SendPacketsElementsBufferCount + 1];
            }
            this.m_ObjectsToPin[0] = this.m_SendPacketsDescriptor;
            int index = 1;
            foreach (SendPacketsElement element in this.m_SendPacketsElementsInternal)
            {
                if ((element.m_Buffer != null) && (element.m_Count > 0))
                {
                    this.m_ObjectsToPin[index] = element.m_Buffer;
                    index++;
                }
            }
            this.m_PtrNativeOverlapped = new SafeNativeOverlapped(this.m_Overlapped.UnsafePack(new IOCompletionCallback(this.CompletionPortCallback), this.m_ObjectsToPin));
            this.m_PtrSendPacketsDescriptor = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_SendPacketsDescriptor, 0);
            int num2 = 0;
            int num3 = 0;
            foreach (SendPacketsElement element2 in this.m_SendPacketsElementsInternal)
            {
                if (element2 != null)
                {
                    if ((element2.m_Buffer != null) && (element2.m_Count > 0))
                    {
                        this.m_SendPacketsDescriptor[num2].buffer = Marshal.UnsafeAddrOfPinnedArrayElement(element2.m_Buffer, element2.m_Offset);
                        this.m_SendPacketsDescriptor[num2].length = (uint) element2.m_Count;
                        this.m_SendPacketsDescriptor[num2].flags = element2.m_Flags;
                        num2++;
                    }
                    else if ((element2.m_FilePath != null) && (element2.m_FilePath.Length != 0))
                    {
                        this.m_SendPacketsDescriptor[num2].fileHandle = this.m_SendPacketsFileHandles[num3].DangerousGetHandle();
                        this.m_SendPacketsDescriptor[num2].fileOffset = element2.m_Offset;
                        this.m_SendPacketsDescriptor[num2].length = (uint) element2.m_Count;
                        this.m_SendPacketsDescriptor[num2].flags = element2.m_Flags;
                        num3++;
                        num2++;
                    }
                }
            }
            this.m_PinState = PinState.SendPackets;
        }

        private void SetupOverlappedSingle(bool pinSingleBuffer)
        {
            this.m_Overlapped = new Overlapped();
            if (pinSingleBuffer)
            {
                if (this.m_Buffer != null)
                {
                    this.m_PtrNativeOverlapped = new SafeNativeOverlapped(this.m_Overlapped.UnsafePack(new IOCompletionCallback(this.CompletionPortCallback), this.m_Buffer));
                    this.m_PinnedSingleBuffer = this.m_Buffer;
                    this.m_PinnedSingleBufferOffset = this.m_Offset;
                    this.m_PinnedSingleBufferCount = this.m_Count;
                    this.m_PtrSingleBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_Buffer, this.m_Offset);
                    this.m_PtrAcceptBuffer = IntPtr.Zero;
                    this.m_WSABuffer.Pointer = this.m_PtrSingleBuffer;
                    this.m_WSABuffer.Length = this.m_Count;
                    this.m_PinState = PinState.SingleBuffer;
                }
                else
                {
                    this.m_PtrNativeOverlapped = new SafeNativeOverlapped(this.m_Overlapped.UnsafePack(new IOCompletionCallback(this.CompletionPortCallback), null));
                    this.m_PinnedSingleBuffer = null;
                    this.m_PinnedSingleBufferOffset = 0;
                    this.m_PinnedSingleBufferCount = 0;
                    this.m_PtrSingleBuffer = IntPtr.Zero;
                    this.m_PtrAcceptBuffer = IntPtr.Zero;
                    this.m_WSABuffer.Pointer = this.m_PtrSingleBuffer;
                    this.m_WSABuffer.Length = this.m_Count;
                    this.m_PinState = PinState.NoBuffer;
                }
            }
            else
            {
                this.m_PtrNativeOverlapped = new SafeNativeOverlapped(this.m_Overlapped.UnsafePack(new IOCompletionCallback(this.CompletionPortCallback), this.m_AcceptBuffer));
                this.m_PinnedAcceptBuffer = this.m_AcceptBuffer;
                this.m_PtrAcceptBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_AcceptBuffer, 0);
                this.m_PtrSingleBuffer = IntPtr.Zero;
                this.m_PinState = PinState.SingleAcceptBuffer;
            }
        }

        private void StartConfiguring()
        {
            switch (Interlocked.CompareExchange(ref this.m_Operating, -1, 0))
            {
                case 1:
                case -1:
                    throw new InvalidOperationException(SR.GetString("net_socketopinprogress"));

                case 2:
                    throw new ObjectDisposedException(base.GetType().FullName);
            }
        }

        internal void StartOperationAccept()
        {
            this.m_CompletedOperation = SocketAsyncOperation.Accept;
            this.m_AcceptAddressBufferCount = 2 * (this.m_CurrentSocket.m_RightEndPoint.Serialize().Size + 0x10);
            if (this.m_Buffer != null)
            {
                if (this.m_Count < this.m_AcceptAddressBufferCount)
                {
                    throw new ArgumentException(SR.GetString("net_buffercounttoosmall", new object[] { "Count" }));
                }
            }
            else
            {
                if ((this.m_AcceptBuffer == null) || (this.m_AcceptBuffer.Length < this.m_AcceptAddressBufferCount))
                {
                    this.m_AcceptBuffer = new byte[this.m_AcceptAddressBufferCount];
                }
                this.CheckPinSingleBuffer(false);
            }
        }

        internal void StartOperationCommon(Socket socket)
        {
            if (Interlocked.CompareExchange(ref this.m_Operating, 1, 0) != 0)
            {
                if (this.m_DisposeCalled)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
                throw new InvalidOperationException(SR.GetString("net_socketopinprogress"));
            }
            if (ExecutionContext.IsFlowSuppressed())
            {
                this.m_Context = null;
                this.m_ContextCopy = null;
            }
            else
            {
                if (this.m_CompletedChanged || (socket != this.m_CurrentSocket))
                {
                    this.m_CompletedChanged = false;
                    this.m_Context = null;
                    this.m_ContextCopy = null;
                }
                if (this.m_Context == null)
                {
                    this.m_Context = ExecutionContext.Capture();
                }
                if (this.m_Context != null)
                {
                    this.m_ContextCopy = this.m_Context.CreateCopy();
                }
            }
            this.m_CurrentSocket = socket;
        }

        internal void StartOperationConnect()
        {
            this.m_CompletedOperation = SocketAsyncOperation.Connect;
            this.m_MultipleConnect = null;
            this.m_ConnectSocket = null;
            this.PinSocketAddressBuffer();
            this.CheckPinNoBuffer();
        }

        internal void StartOperationDisconnect()
        {
            this.m_CompletedOperation = SocketAsyncOperation.Disconnect;
            this.CheckPinNoBuffer();
        }

        internal void StartOperationReceive()
        {
            this.m_CompletedOperation = SocketAsyncOperation.Receive;
        }

        internal void StartOperationReceiveFrom()
        {
            this.m_CompletedOperation = SocketAsyncOperation.ReceiveFrom;
            this.PinSocketAddressBuffer();
        }

        internal unsafe void StartOperationReceiveMessageFrom()
        {
            this.m_CompletedOperation = SocketAsyncOperation.ReceiveMessageFrom;
            this.PinSocketAddressBuffer();
            if (this.m_WSAMessageBuffer == null)
            {
                this.m_WSAMessageBuffer = new byte[s_WSAMsgSize];
                this.m_WSAMessageBufferGCHandle = GCHandle.Alloc(this.m_WSAMessageBuffer, GCHandleType.Pinned);
                this.m_PtrWSAMessageBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_WSAMessageBuffer, 0);
            }
            bool flag = this.m_CurrentSocket.AddressFamily == AddressFamily.InterNetwork;
            bool flag2 = this.m_CurrentSocket.AddressFamily == AddressFamily.InterNetworkV6;
            if (flag && ((this.m_ControlBuffer == null) || (this.m_ControlBuffer.Length != s_ControlDataSize)))
            {
                if (this.m_ControlBufferGCHandle.IsAllocated)
                {
                    this.m_ControlBufferGCHandle.Free();
                }
                this.m_ControlBuffer = new byte[s_ControlDataSize];
            }
            else if (flag2 && ((this.m_ControlBuffer == null) || (this.m_ControlBuffer.Length != s_ControlDataIPv6Size)))
            {
                if (this.m_ControlBufferGCHandle.IsAllocated)
                {
                    this.m_ControlBufferGCHandle.Free();
                }
                this.m_ControlBuffer = new byte[s_ControlDataIPv6Size];
            }
            if (!this.m_ControlBufferGCHandle.IsAllocated)
            {
                this.m_ControlBufferGCHandle = GCHandle.Alloc(this.m_ControlBuffer, GCHandleType.Pinned);
                this.m_PtrControlBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_ControlBuffer, 0);
            }
            if (this.m_Buffer != null)
            {
                if (this.m_WSARecvMsgWSABufferArray == null)
                {
                    this.m_WSARecvMsgWSABufferArray = new WSABuffer[1];
                }
                this.m_WSARecvMsgWSABufferArray[0].Pointer = this.m_PtrSingleBuffer;
                this.m_WSARecvMsgWSABufferArray[0].Length = this.m_Count;
                this.m_WSARecvMsgWSABufferArrayGCHandle = GCHandle.Alloc(this.m_WSARecvMsgWSABufferArray, GCHandleType.Pinned);
                this.m_PtrWSARecvMsgWSABufferArray = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_WSARecvMsgWSABufferArray, 0);
            }
            else
            {
                this.m_WSARecvMsgWSABufferArrayGCHandle = GCHandle.Alloc(this.m_WSABufferArray, GCHandleType.Pinned);
                this.m_PtrWSARecvMsgWSABufferArray = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_WSABufferArray, 0);
            }
            UnsafeNclNativeMethods.OSSOCK.WSAMsg* ptrWSAMessageBuffer = (UnsafeNclNativeMethods.OSSOCK.WSAMsg*) this.m_PtrWSAMessageBuffer;
            ptrWSAMessageBuffer->socketAddress = this.m_PtrSocketAddressBuffer;
            ptrWSAMessageBuffer->addressLength = (uint) this.m_SocketAddress.Size;
            ptrWSAMessageBuffer->buffers = this.m_PtrWSARecvMsgWSABufferArray;
            if (this.m_Buffer != null)
            {
                ptrWSAMessageBuffer->count = 1;
            }
            else
            {
                ptrWSAMessageBuffer->count = (uint) this.m_WSABufferArray.Length;
            }
            if (this.m_ControlBuffer != null)
            {
                ptrWSAMessageBuffer->controlBuffer.Pointer = this.m_PtrControlBuffer;
                ptrWSAMessageBuffer->controlBuffer.Length = this.m_ControlBuffer.Length;
            }
            ptrWSAMessageBuffer->flags = this.m_SocketFlags;
        }

        internal void StartOperationSend()
        {
            this.m_CompletedOperation = SocketAsyncOperation.Send;
        }

        internal void StartOperationSendPackets()
        {
            this.m_CompletedOperation = SocketAsyncOperation.SendPackets;
            if (this.m_SendPacketsElements != null)
            {
                this.m_SendPacketsElementsInternal = (SendPacketsElement[]) this.m_SendPacketsElements.Clone();
            }
            this.m_SendPacketsElementsFileCount = 0;
            this.m_SendPacketsElementsBufferCount = 0;
            foreach (SendPacketsElement element in this.m_SendPacketsElementsInternal)
            {
                if (element != null)
                {
                    if ((element.m_FilePath != null) && (element.m_FilePath.Length > 0))
                    {
                        this.m_SendPacketsElementsFileCount++;
                    }
                    if (element.m_Buffer != null)
                    {
                        this.m_SendPacketsElementsBufferCount++;
                    }
                }
            }
            if (this.m_SendPacketsElementsFileCount > 0)
            {
                this.m_SendPacketsFileStreams = new FileStream[this.m_SendPacketsElementsFileCount];
                this.m_SendPacketsFileHandles = new SafeHandle[this.m_SendPacketsElementsFileCount];
                int index = 0;
                foreach (SendPacketsElement element2 in this.m_SendPacketsElementsInternal)
                {
                    if (((element2 != null) && (element2.m_FilePath != null)) && (element2.m_FilePath.Length > 0))
                    {
                        Exception exception = null;
                        try
                        {
                            this.m_SendPacketsFileStreams[index] = new FileStream(element2.m_FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                        }
                        if (exception != null)
                        {
                            for (int i = 0; i < this.m_SendPacketsElementsFileCount; i++)
                            {
                                this.m_SendPacketsFileHandles[i] = null;
                                if (this.m_SendPacketsFileStreams[i] != null)
                                {
                                    this.m_SendPacketsFileStreams[i].Close();
                                    this.m_SendPacketsFileStreams[i] = null;
                                }
                            }
                            throw exception;
                        }
                        ExceptionHelper.UnmanagedPermission.Assert();
                        try
                        {
                            this.m_SendPacketsFileHandles[index] = this.m_SendPacketsFileStreams[index].SafeFileHandle;
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        index++;
                    }
                }
            }
            this.CheckPinSendPackets();
        }

        internal void StartOperationSendTo()
        {
            this.m_CompletedOperation = SocketAsyncOperation.SendTo;
            this.PinSocketAddressBuffer();
        }

        internal void StartOperationWrapperConnect(MultipleConnectAsync args)
        {
            this.m_CompletedOperation = SocketAsyncOperation.Connect;
            this.m_MultipleConnect = args;
            this.m_ConnectSocket = null;
        }

        internal void UpdatePerfCounters(int size, bool sendOp)
        {
            if (sendOp)
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesSent, (long) size);
                if (this.m_CurrentSocket.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsSent);
                }
            }
            else
            {
                NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketBytesReceived, (long) size);
                if (this.m_CurrentSocket.Transport == TransportType.Udp)
                {
                    NetworkingPerfCounters.Instance.Increment(NetworkingPerfCounterName.SocketDatagramsReceived);
                }
            }
        }

        public Socket AcceptSocket
        {
            get
            {
                return this.m_AcceptSocket;
            }
            set
            {
                this.m_AcceptSocket = value;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return this.m_Buffer;
            }
        }

        public IList<ArraySegment<byte>> BufferList
        {
            get
            {
                return this.m_BufferList;
            }
            set
            {
                this.StartConfiguring();
                try
                {
                    if ((value != null) && (this.m_Buffer != null))
                    {
                        throw new ArgumentException(SR.GetString("net_ambiguousbuffers", new object[] { "Buffer" }));
                    }
                    this.m_BufferList = value;
                    this.m_BufferListChanged = true;
                    this.CheckPinMultipleBuffers();
                }
                finally
                {
                    this.Complete();
                }
            }
        }

        public int BytesTransferred
        {
            get
            {
                return this.m_BytesTransferred;
            }
        }

        public Exception ConnectByNameError
        {
            get
            {
                return this.m_ConnectByNameError;
            }
        }

        public Socket ConnectSocket
        {
            get
            {
                return this.m_ConnectSocket;
            }
        }

        public int Count
        {
            get
            {
                return this.m_Count;
            }
        }

        public bool DisconnectReuseSocket
        {
            get
            {
                return this.m_DisconnectReuseSocket;
            }
            set
            {
                this.m_DisconnectReuseSocket = value;
            }
        }

        public SocketAsyncOperation LastOperation
        {
            get
            {
                return this.m_CompletedOperation;
            }
        }

        public int Offset
        {
            get
            {
                return this.m_Offset;
            }
        }

        public IPPacketInformation ReceiveMessageFromPacketInfo
        {
            get
            {
                return this.m_ReceiveMessageFromPacketInfo;
            }
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.m_RemoteEndPoint;
            }
            set
            {
                this.m_RemoteEndPoint = value;
            }
        }

        public SendPacketsElement[] SendPacketsElements
        {
            get
            {
                return this.m_SendPacketsElements;
            }
            set
            {
                this.StartConfiguring();
                try
                {
                    this.m_SendPacketsElements = value;
                    this.m_SendPacketsElementsInternal = null;
                }
                finally
                {
                    this.Complete();
                }
            }
        }

        public TransmitFileOptions SendPacketsFlags
        {
            get
            {
                return this.m_SendPacketsFlags;
            }
            set
            {
                this.m_SendPacketsFlags = value;
            }
        }

        public int SendPacketsSendSize
        {
            get
            {
                return this.m_SendPacketsSendSize;
            }
            set
            {
                this.m_SendPacketsSendSize = value;
            }
        }

        public System.Net.Sockets.SocketError SocketError
        {
            get
            {
                return this.m_SocketError;
            }
            set
            {
                this.m_SocketError = value;
            }
        }

        public System.Net.Sockets.SocketFlags SocketFlags
        {
            get
            {
                return this.m_SocketFlags;
            }
            set
            {
                this.m_SocketFlags = value;
            }
        }

        public object UserToken
        {
            get
            {
                return this.m_UserToken;
            }
            set
            {
                this.m_UserToken = value;
            }
        }

        private enum PinState
        {
            None,
            NoBuffer,
            SingleAcceptBuffer,
            SingleBuffer,
            MultipleBuffer,
            SendPackets
        }
    }
}


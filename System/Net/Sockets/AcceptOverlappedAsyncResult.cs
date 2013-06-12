namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class AcceptOverlappedAsyncResult : BaseOverlappedAsyncResult
    {
        private Socket m_AcceptSocket;
        private int m_AddressBufferLength;
        private byte[] m_Buffer;
        private Socket m_ListenSocket;
        private int m_LocalBytesTransferred;

        internal AcceptOverlappedAsyncResult(Socket listenSocket, object asyncState, AsyncCallback asyncCallback) : base(listenSocket, asyncState, asyncCallback)
        {
            this.m_ListenSocket = listenSocket;
        }

        private void LogBuffer(long size)
        {
            IntPtr bufferPtr = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_Buffer, 0);
            if (bufferPtr != IntPtr.Zero)
            {
                if (size > -1L)
                {
                    Logging.Dump(Logging.Sockets, this.m_ListenSocket, "PostCompletion", bufferPtr, (int) Math.Min(size, (long) this.m_Buffer.Length));
                }
                else
                {
                    Logging.Dump(Logging.Sockets, this.m_ListenSocket, "PostCompletion", bufferPtr, this.m_Buffer.Length);
                }
            }
        }

        internal override object PostCompletion(int numBytes)
        {
            SocketError errorCode = (SocketError) base.ErrorCode;
            SocketAddress socketAddress = null;
            if (errorCode == SocketError.Success)
            {
                this.m_LocalBytesTransferred = numBytes;
                if (Logging.On)
                {
                    this.LogBuffer((long) numBytes);
                }
                socketAddress = this.m_ListenSocket.m_RightEndPoint.Serialize();
                try
                {
                    IntPtr ptr;
                    int num;
                    IntPtr ptr2;
                    this.m_ListenSocket.GetAcceptExSockaddrs(Marshal.UnsafeAddrOfPinnedArrayElement(this.m_Buffer, 0), this.m_Buffer.Length - (this.m_AddressBufferLength * 2), this.m_AddressBufferLength, this.m_AddressBufferLength, out ptr, out num, out ptr2, out socketAddress.m_Size);
                    Marshal.Copy(ptr2, socketAddress.m_Buffer, 0, socketAddress.m_Size);
                    IntPtr handle = this.m_ListenSocket.SafeHandle.DangerousGetHandle();
                    errorCode = UnsafeNclNativeMethods.OSSOCK.setsockopt(this.m_AcceptSocket.SafeHandle, SocketOptionLevel.Socket, SocketOptionName.UpdateAcceptContext, ref handle, Marshal.SizeOf(handle));
                    if (errorCode == SocketError.SocketError)
                    {
                        errorCode = (SocketError) Marshal.GetLastWin32Error();
                    }
                }
                catch (ObjectDisposedException)
                {
                    errorCode = SocketError.OperationAborted;
                }
                base.ErrorCode = (int) errorCode;
            }
            if (errorCode == SocketError.Success)
            {
                return this.m_ListenSocket.UpdateAcceptSocket(this.m_AcceptSocket, this.m_ListenSocket.m_RightEndPoint.Create(socketAddress), false);
            }
            return null;
        }

        internal void SetUnmanagedStructures(byte[] buffer, int addressBufferLength)
        {
            base.SetUnmanagedStructures(buffer);
            this.m_AddressBufferLength = addressBufferLength;
            this.m_Buffer = buffer;
        }

        internal Socket AcceptSocket
        {
            set
            {
                this.m_AcceptSocket = value;
            }
        }

        internal byte[] Buffer
        {
            get
            {
                return this.m_Buffer;
            }
        }

        internal int BytesTransferred
        {
            get
            {
                return this.m_LocalBytesTransferred;
            }
        }
    }
}


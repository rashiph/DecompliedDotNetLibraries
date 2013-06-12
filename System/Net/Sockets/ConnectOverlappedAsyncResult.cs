namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class ConnectOverlappedAsyncResult : BaseOverlappedAsyncResult
    {
        private EndPoint m_EndPoint;

        internal ConnectOverlappedAsyncResult(Socket socket, EndPoint endPoint, object asyncState, AsyncCallback asyncCallback) : base(socket, asyncState, asyncCallback)
        {
            this.m_EndPoint = endPoint;
        }

        internal override object PostCompletion(int numBytes)
        {
            SocketError errorCode = (SocketError) base.ErrorCode;
            Socket asyncObject = (Socket) base.AsyncObject;
            if (errorCode == SocketError.Success)
            {
                try
                {
                    errorCode = UnsafeNclNativeMethods.OSSOCK.setsockopt(asyncObject.SafeHandle, SocketOptionLevel.Socket, SocketOptionName.UpdateConnectContext, (byte[]) null, 0);
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
                asyncObject.SetToConnected();
                return asyncObject;
            }
            return null;
        }

        internal EndPoint RemoteEndPoint
        {
            get
            {
                return this.m_EndPoint;
            }
        }
    }
}


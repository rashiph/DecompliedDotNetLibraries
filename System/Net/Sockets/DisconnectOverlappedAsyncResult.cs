namespace System.Net.Sockets
{
    using System;

    internal class DisconnectOverlappedAsyncResult : BaseOverlappedAsyncResult
    {
        internal DisconnectOverlappedAsyncResult(Socket socket, object asyncState, AsyncCallback asyncCallback) : base(socket, asyncState, asyncCallback)
        {
        }

        internal override object PostCompletion(int numBytes)
        {
            if (base.ErrorCode == 0)
            {
                Socket asyncObject = (Socket) base.AsyncObject;
                asyncObject.SetToDisconnected();
                asyncObject.m_RemoteEndPoint = null;
            }
            return base.PostCompletion(numBytes);
        }
    }
}


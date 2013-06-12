namespace System.Net.Sockets
{
    using System;
    using System.Net;

    internal class ConnectAsyncResult : ContextAwareResult
    {
        private EndPoint m_EndPoint;

        internal ConnectAsyncResult(object myObject, EndPoint endPoint, object myState, AsyncCallback myCallBack) : base(myObject, myState, myCallBack)
        {
            this.m_EndPoint = endPoint;
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


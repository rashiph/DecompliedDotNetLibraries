namespace System.Net.Sockets
{
    using System;
    using System.Net;

    internal class AcceptAsyncResult : ContextAwareResult
    {
        internal AcceptAsyncResult(object myObject, object myState, AsyncCallback myCallBack) : base(myObject, myState, myCallBack)
        {
        }
    }
}


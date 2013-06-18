namespace System.Web
{
    using System;

    internal interface IHttpResponseElement
    {
        byte[] GetBytes();
        long GetSize();
        void Send(HttpWorkerRequest wr);
    }
}


namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate IPEndPoint BindIPEndPoint(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount);
}


namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void HttpContinueDelegate(int StatusCode, WebHeaderCollection httpHeaders);
}


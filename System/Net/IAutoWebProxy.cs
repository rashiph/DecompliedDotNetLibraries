namespace System.Net
{
    using System;

    internal interface IAutoWebProxy : IWebProxy
    {
        ProxyChain GetProxies(Uri destination);
    }
}


namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal interface IWebProxyFinder : IDisposable
    {
        void Abort();
        bool GetProxies(Uri destination, out IList<string> proxyList);
        void Reset();

        bool IsValid { get; }
    }
}


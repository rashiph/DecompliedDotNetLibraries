namespace System.Runtime.Remoting.Proxies
{
    using System;

    [Flags]
    internal enum RealProxyFlags
    {
        None,
        RemotingProxy,
        Initialized
    }
}


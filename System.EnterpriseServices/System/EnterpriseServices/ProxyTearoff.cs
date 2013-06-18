namespace System.EnterpriseServices
{
    using System;

    internal abstract class ProxyTearoff
    {
        internal ProxyTearoff()
        {
        }

        internal abstract void Init(ServicedComponentProxy scp);
        internal abstract void SetCanBePooled(bool fCanBePooled);
    }
}


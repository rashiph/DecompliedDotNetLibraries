namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IApplicationTrustManager : ISecurityEncodable
    {
        ApplicationTrust DetermineApplicationTrust(ActivationContext activationContext, TrustManagerContext context);
    }
}


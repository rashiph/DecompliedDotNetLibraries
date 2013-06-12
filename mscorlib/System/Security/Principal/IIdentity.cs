namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IIdentity
    {
        string AuthenticationType { get; }

        bool IsAuthenticated { get; }

        string Name { get; }
    }
}


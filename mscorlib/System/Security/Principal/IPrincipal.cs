namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IPrincipal
    {
        bool IsInRole(string role);

        IIdentity Identity { get; }
    }
}


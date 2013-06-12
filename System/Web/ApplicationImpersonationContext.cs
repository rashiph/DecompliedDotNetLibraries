namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web.Hosting;

    internal sealed class ApplicationImpersonationContext : ImpersonationContext
    {
        internal ApplicationImpersonationContext()
        {
            base.ImpersonateToken(new HandleRef(this, HostingEnvironment.ApplicationIdentityToken));
        }
    }
}


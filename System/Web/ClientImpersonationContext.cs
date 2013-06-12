namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web.Hosting;

    internal sealed class ClientImpersonationContext : ImpersonationContext
    {
        internal ClientImpersonationContext(HttpContext context)
        {
            this.Start(context, true);
        }

        internal ClientImpersonationContext(HttpContext context, bool throwOnError)
        {
            this.Start(context, throwOnError);
        }

        private void Start(HttpContext context, bool throwOnError)
        {
            IntPtr zero = IntPtr.Zero;
            try
            {
                if (context != null)
                {
                    zero = context.ImpersonationToken;
                }
                else
                {
                    zero = HostingEnvironment.ApplicationIdentityToken;
                }
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }
            }
            if (zero != IntPtr.Zero)
            {
                base.ImpersonateToken(new HandleRef(this, zero));
            }
        }
    }
}


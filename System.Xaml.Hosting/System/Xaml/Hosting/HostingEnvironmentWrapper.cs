namespace System.Xaml.Hosting
{
    using System;
    using System.Web.Hosting;

    internal static class HostingEnvironmentWrapper
    {
        public static IDisposable UnsafeImpersonate()
        {
            return HostingEnvironment.Impersonate();
        }
    }
}


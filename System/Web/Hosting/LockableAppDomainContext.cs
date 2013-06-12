namespace System.Web.Hosting
{
    using System;
    using System.Runtime.CompilerServices;

    internal class LockableAppDomainContext
    {
        internal LockableAppDomainContext()
        {
        }

        internal HostingEnvironment HostEnv { get; set; }

        internal string PreloadContext { get; set; }

        internal bool RetryingPreload { get; set; }
    }
}


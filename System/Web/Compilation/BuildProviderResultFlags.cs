namespace System.Web.Compilation
{
    using System;

    [Flags]
    public enum BuildProviderResultFlags
    {
        Default,
        ShutdownAppDomainOnChange
    }
}


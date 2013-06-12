namespace System.Web.Compilation
{
    using System;

    [Flags]
    public enum BuildProviderAppliesTo
    {
        All = 7,
        Code = 2,
        Resources = 4,
        Web = 1
    }
}


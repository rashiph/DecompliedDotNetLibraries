namespace Microsoft.Build.Shared
{
    using System;

    [Flags]
    internal enum PartialComparisonFlags
    {
        Culture = 4,
        Default = 15,
        PublicKeyToken = 8,
        SimpleName = 1,
        Version = 2
    }
}


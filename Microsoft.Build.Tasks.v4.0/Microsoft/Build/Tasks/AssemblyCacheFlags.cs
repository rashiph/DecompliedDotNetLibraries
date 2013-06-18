namespace Microsoft.Build.Tasks
{
    using System;

    [Flags]
    internal enum AssemblyCacheFlags
    {
        DOWNLOAD = 4,
        GAC = 2,
        ZAP = 1
    }
}


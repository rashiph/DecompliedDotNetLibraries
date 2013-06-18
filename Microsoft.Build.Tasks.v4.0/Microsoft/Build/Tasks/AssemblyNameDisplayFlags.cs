namespace Microsoft.Build.Tasks
{
    using System;

    [Flags]
    internal enum AssemblyNameDisplayFlags
    {
        ALL = 0xa7,
        CULTURE = 2,
        PROCESSORARCHITECTURE = 0x20,
        PUBLIC_KEY_TOKEN = 4,
        RETARGETABLE = 0x80,
        VERSION = 1
    }
}


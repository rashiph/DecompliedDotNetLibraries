namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct UnificationVersion
    {
        internal string referenceFullPath;
        internal Version version;
        internal UnificationReason reason;
    }
}


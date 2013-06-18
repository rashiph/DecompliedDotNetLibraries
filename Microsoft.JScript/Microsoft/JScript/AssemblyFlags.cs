namespace Microsoft.JScript
{
    using System;

    internal enum AssemblyFlags
    {
        CompatibilityMask = 0x70,
        DisableJITcompileOptimizer = 0x4000,
        EnableJITcompileTracking = 0x8000,
        NonSideBySideAppDomain = 0x10,
        NonSideBySideMachine = 0x30,
        NonSideBySideProcess = 0x20,
        PublicKey = 1,
        SideBySideCompatible = 0
    }
}


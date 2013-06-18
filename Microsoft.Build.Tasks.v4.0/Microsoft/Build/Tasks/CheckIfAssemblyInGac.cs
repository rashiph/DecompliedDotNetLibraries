namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal delegate bool CheckIfAssemblyInGac(AssemblyNameExtension assemblyName, ProcessorArchitecture targetProcessorArchitecture, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVersion, Microsoft.Build.Shared.FileExists fileExists);
}


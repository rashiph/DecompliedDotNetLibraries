namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal delegate void GetAssemblyMetadata(string path, out AssemblyNameExtension[] dependencies, out string[] scatterFiles);
}


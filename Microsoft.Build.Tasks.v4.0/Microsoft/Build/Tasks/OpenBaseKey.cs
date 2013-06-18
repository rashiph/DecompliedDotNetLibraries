namespace Microsoft.Build.Tasks
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;

    internal delegate RegistryKey OpenBaseKey(RegistryHive hive, RegistryView view);
}


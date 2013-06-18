namespace Microsoft.Build.Tasks
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.CompilerServices;

    internal delegate string GetRegistrySubKeyDefaultValue(RegistryKey baseKey, string subKey);
}


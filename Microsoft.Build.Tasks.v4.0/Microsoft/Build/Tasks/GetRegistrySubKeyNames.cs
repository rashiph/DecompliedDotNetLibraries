namespace Microsoft.Build.Tasks
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;

    internal delegate IEnumerable GetRegistrySubKeyNames(RegistryKey baseKey, string subKey);
}


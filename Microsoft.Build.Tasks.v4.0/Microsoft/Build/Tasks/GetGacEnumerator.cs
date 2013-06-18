namespace Microsoft.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal delegate IEnumerable<AssemblyNameExtension> GetGacEnumerator(string strongName);
}


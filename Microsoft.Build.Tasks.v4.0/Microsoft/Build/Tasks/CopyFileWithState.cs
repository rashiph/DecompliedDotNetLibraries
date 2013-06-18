namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate bool CopyFileWithState(Microsoft.Build.Tasks.FileState source, Microsoft.Build.Tasks.FileState destination);
}


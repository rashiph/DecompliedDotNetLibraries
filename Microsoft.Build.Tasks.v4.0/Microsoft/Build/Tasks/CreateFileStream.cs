namespace Microsoft.Build.Tasks
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal delegate Stream CreateFileStream(string path, FileMode mode, FileAccess access);
}


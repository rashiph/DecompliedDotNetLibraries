namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Reflection;

    internal class ComReferenceWrapperInfo
    {
        internal Assembly assembly;
        internal AssemblyNameExtension originalPiaName;
        internal string path;

        internal ComReferenceWrapperInfo()
        {
        }
    }
}


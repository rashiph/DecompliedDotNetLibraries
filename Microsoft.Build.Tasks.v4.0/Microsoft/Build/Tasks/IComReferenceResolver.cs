namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal interface IComReferenceResolver
    {
        bool ResolveComAssemblyReference(string assemblyName, out string assemblyPath);
        bool ResolveComClassicReference(System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr, string outputDirectory, string wrapperType, string refName, out ComReferenceWrapperInfo wrapperInfo);
        bool ResolveNetAssemblyReference(string assemblyName, out string assemblyPath);
    }
}


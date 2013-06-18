namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Compiler;

    [ComVisible(true), Guid("0E6DF9D7-B4B5-4af7-9647-FC335CCE393F")]
    public interface ITypeProviderCreator
    {
        ITypeProvider GetTypeProvider(object obj);
        Assembly GetLocalAssembly(object obj);
        Assembly GetTransientAssembly(AssemblyName assemblyName);
        ITypeResolutionService GetTypeResolutionService(object obj);
    }
}


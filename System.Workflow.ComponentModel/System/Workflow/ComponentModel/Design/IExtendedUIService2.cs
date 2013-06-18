namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Reflection;

    public interface IExtendedUIService2
    {
        Assembly GetReflectionAssembly(AssemblyName assemblyName);
        Type GetRuntimeType(Type reflectionType);
        long GetTargetFrameworkVersion();
        bool IsSupportedType(Type type);
    }
}


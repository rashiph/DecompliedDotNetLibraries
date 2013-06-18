namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface ITypeProvider
    {
        event EventHandler TypeLoadErrorsChanged;

        event EventHandler TypesChanged;

        Type GetType(string name);
        Type GetType(string name, bool throwOnError);
        Type[] GetTypes();

        Assembly LocalAssembly { get; }

        ICollection<Assembly> ReferencedAssemblies { get; }

        IDictionary<object, Exception> TypeLoadErrors { get; }
    }
}


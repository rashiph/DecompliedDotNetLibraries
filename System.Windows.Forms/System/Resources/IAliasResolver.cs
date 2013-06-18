namespace System.Resources
{
    using System;
    using System.Reflection;

    internal interface IAliasResolver
    {
        void PushAlias(string alias, AssemblyName name);
        AssemblyName ResolveAlias(string alias);
    }
}


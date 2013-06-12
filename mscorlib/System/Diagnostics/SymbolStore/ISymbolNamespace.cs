namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISymbolNamespace
    {
        ISymbolNamespace[] GetNamespaces();
        ISymbolVariable[] GetVariables();

        string Name { get; }
    }
}


namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISymbolScope
    {
        ISymbolScope[] GetChildren();
        ISymbolVariable[] GetLocals();
        ISymbolNamespace[] GetNamespaces();

        int EndOffset { get; }

        ISymbolMethod Method { get; }

        ISymbolScope Parent { get; }

        int StartOffset { get; }
    }
}


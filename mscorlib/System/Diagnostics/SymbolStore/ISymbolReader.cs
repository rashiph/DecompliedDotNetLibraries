namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISymbolReader
    {
        ISymbolDocument GetDocument(string url, Guid language, Guid languageVendor, Guid documentType);
        ISymbolDocument[] GetDocuments();
        ISymbolVariable[] GetGlobalVariables();
        ISymbolMethod GetMethod(SymbolToken method);
        ISymbolMethod GetMethod(SymbolToken method, int version);
        ISymbolMethod GetMethodFromDocumentPosition(ISymbolDocument document, int line, int column);
        ISymbolNamespace[] GetNamespaces();
        byte[] GetSymAttribute(SymbolToken parent, string name);
        ISymbolVariable[] GetVariables(SymbolToken parent);

        SymbolToken UserEntryPoint { get; }
    }
}


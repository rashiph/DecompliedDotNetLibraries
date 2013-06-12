namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISymbolDocument
    {
        int FindClosestLine(int line);
        byte[] GetCheckSum();
        byte[] GetSourceRange(int startLine, int startColumn, int endLine, int endColumn);

        Guid CheckSumAlgorithmId { get; }

        Guid DocumentType { get; }

        bool HasEmbeddedSource { get; }

        Guid Language { get; }

        Guid LanguageVendor { get; }

        int SourceLength { get; }

        string URL { get; }
    }
}


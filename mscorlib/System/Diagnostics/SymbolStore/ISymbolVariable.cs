namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ISymbolVariable
    {
        byte[] GetSignature();

        int AddressField1 { get; }

        int AddressField2 { get; }

        int AddressField3 { get; }

        SymAddressKind AddressKind { get; }

        object Attributes { get; }

        int EndOffset { get; }

        string Name { get; }

        int StartOffset { get; }
    }
}


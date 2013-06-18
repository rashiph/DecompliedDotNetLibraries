namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISymUnmanagedDocument
    {
        void GetURL(uint urlLength, out uint actualUrlLength, [In, Out, MarshalAs(UnmanagedType.LPWStr)] string url);
        void GetDocumentType();
        void GetLanguage();
        void GetLanguageVendor();
        void GetCheckSumAlgorithmId();
        void GetCheckSum();
        void FindClosestLine();
        void HasEmbeddedSource();
        void GetSourceLength();
        void GetSourceRange();
    }
}


namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, CoClass(typeof(CorSymReader_SxS)), Guid("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISymUnmanagedReader
    {
        void GetDocument();
        void GetDocuments();
        void GetUserEntryPoint();
        ISymUnmanagedMethod GetMethod(uint methodDef);
        void GetMethodByVersion();
        void GetVariables();
        void GetGlobalVariables();
        void GetMethodFromDocumentPosition();
        void GetSymAttribute();
        void GetNamespaces();
        void Initialize([In, MarshalAs(UnmanagedType.IUnknown)] object metaDataImport, [In, MarshalAs(UnmanagedType.LPWStr)] string pdbPath, [In, MarshalAs(UnmanagedType.LPWStr)] string searchPath, [In, MarshalAs(UnmanagedType.IUnknown)] object stream);
        void UpdateSymbolStore();
        void ReplaceSymbolStore();
        void GetSymbolStoreFileName();
        void GetMethodsFromDocumentPosition();
        void GetDocumentVersion();
        void GetMethodVersion();
    }
}


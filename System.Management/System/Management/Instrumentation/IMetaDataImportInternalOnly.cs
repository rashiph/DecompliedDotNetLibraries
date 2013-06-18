namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [ComImport, Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeLibType(TypeLibTypeFlags.FRestricted)]
    internal interface IMetaDataImportInternalOnly
    {
        void f1();
        void f2();
        void f3();
        void f4();
        void f5();
        void f6();
        void f7();
        void GetScopeProps([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szName, [In] uint cchName, out uint pchName, out Guid pmvid);
    }
}


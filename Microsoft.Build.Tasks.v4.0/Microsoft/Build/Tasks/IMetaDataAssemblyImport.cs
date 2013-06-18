namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("EE62470B-E94B-424e-9B7C-2F00C9249F93"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMetaDataAssemblyImport
    {
        void GetAssemblyProps(uint mdAsm, out IntPtr pPublicKeyPtr, out uint ucbPublicKeyPtr, out uint uHashAlg, [MarshalAs(UnmanagedType.LPArray)] char[] strName, uint cchNameIn, out uint cchNameRequired, IntPtr amdInfo, out uint dwFlags);
        void GetAssemblyRefProps(uint mdAsmRef, out IntPtr ppbPublicKeyOrToken, out uint pcbPublicKeyOrToken, [MarshalAs(UnmanagedType.LPArray)] char[] strName, uint cchNameIn, out uint pchNameOut, IntPtr amdInfo, out IntPtr ppbHashValue, out uint pcbHashValue, out uint pdwAssemblyRefFlags);
        void GetFileProps([In] uint mdFile, [MarshalAs(UnmanagedType.LPArray)] char[] strName, uint cchName, out uint cchNameRequired, out IntPtr bHashData, out uint cchHashBytes, out uint dwFileFlags);
        void GetExportedTypeProps();
        void GetManifestResourceProps();
        void EnumAssemblyRefs([In, Out] ref IntPtr phEnum, [Out, MarshalAs(UnmanagedType.LPArray)] uint[] asmRefs, uint asmRefCount, out uint iFetched);
        void EnumFiles([In, Out] ref IntPtr phEnum, [Out, MarshalAs(UnmanagedType.LPArray)] uint[] fileRefs, uint fileRefCount, out uint iFetched);
        void EnumExportedTypes();
        void EnumManifestResources();
        void GetAssemblyFromScope(out uint mdAsm);
        void FindExportedTypeByName();
        void FindManifestResourceByName();
        [PreserveSig]
        void CloseEnum([In] IntPtr phEnum);
        void FindAssembliesByName();
    }
}


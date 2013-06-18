namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Workflow.Interop;

    internal static class MetaDataReader
    {
        internal static IEnumerable GetTypeRefNames(string assemblyLocation)
        {
            IMetaDataDispenser iteratorVariable0 = new MetaDataDispenser() as IMetaDataDispenser;
            if (iteratorVariable0 == null)
            {
                throw new InvalidOperationException(string.Format(SR.GetString("Error_MetaDataInterfaceMissing"), assemblyLocation, "IMetaDataDispenser"));
            }
            Guid riid = new Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44");
            object unknown = null;
            System.Workflow.Interop.NativeMethods.ThrowOnFailure(iteratorVariable0.OpenScope(assemblyLocation, 0, ref riid, out unknown));
            IMetaDataImport iteratorVariable3 = unknown as IMetaDataImport;
            if (iteratorVariable3 == null)
            {
                throw new InvalidOperationException(string.Format(SR.GetString("Error_MetaDataInterfaceMissing"), assemblyLocation, "IMetaDataImport"));
            }
            IntPtr enumHandle = new IntPtr();
            uint[] rTypeRefs = new uint[20];
            uint typeRefs = 0;
            do
            {
                System.Workflow.Interop.NativeMethods.ThrowOnFailure(iteratorVariable3.EnumTypeRefs(ref enumHandle, rTypeRefs, (uint) rTypeRefs.Length, ref typeRefs));
                for (int j = 0; j < typeRefs; j++)
                {
                    uint iteratorVariable10;
                    uint iteratorVariable9;
                    IntPtr zero = IntPtr.Zero;
                    System.Workflow.Interop.NativeMethods.ThrowOnFailure(iteratorVariable3.GetTypeRefProps(rTypeRefs[j], out iteratorVariable10, zero, 0, out iteratorVariable9));
                    if (iteratorVariable9 > 0)
                    {
                        string iteratorVariable11 = string.Empty;
                        zero = Marshal.AllocCoTaskMem((int) (2 * (iteratorVariable9 + 1)));
                        try
                        {
                            System.Workflow.Interop.NativeMethods.ThrowOnFailure(iteratorVariable3.GetTypeRefProps(rTypeRefs[j], out iteratorVariable10, zero, iteratorVariable9, out iteratorVariable9));
                        }
                        finally
                        {
                            iteratorVariable11 = Marshal.PtrToStringUni(zero);
                            Marshal.FreeCoTaskMem(zero);
                        }
                        IMetaDataAssemblyImport iteratorVariable12 = unknown as IMetaDataAssemblyImport;
                        if (iteratorVariable12 == null)
                        {
                            throw new InvalidOperationException(string.Format(SR.GetString("Error_MetaDataInterfaceMissing"), assemblyLocation, "IMetaDataAssemblyImport"));
                        }
                        if (TokenTypeFromToken(iteratorVariable10) == MetadataTokenType.AssemblyRef)
                        {
                            uint iteratorVariable20;
                            uint iteratorVariable19;
                            uint iteratorVariable17;
                            uint iteratorVariable15;
                            AssemblyMetadata iteratorVariable13;
                            IntPtr publicKeyOrToken = IntPtr.Zero;
                            IntPtr assemblyName = IntPtr.Zero;
                            IntPtr hashValueBlob = IntPtr.Zero;
                            System.Workflow.Interop.NativeMethods.ThrowOnFailure(iteratorVariable12.GetAssemblyRefProps(iteratorVariable10, out publicKeyOrToken, out iteratorVariable15, assemblyName, 0, out iteratorVariable17, out iteratorVariable13, out hashValueBlob, out iteratorVariable19, out iteratorVariable20));
                            if (iteratorVariable17 > 0)
                            {
                                assemblyName = Marshal.AllocCoTaskMem((int) (2 * (iteratorVariable17 + 1)));
                            }
                            if (iteratorVariable13.localeSize > 0)
                            {
                                iteratorVariable13.locale = Marshal.AllocCoTaskMem((int) (2 * (iteratorVariable13.localeSize + 1)));
                            }
                            if ((iteratorVariable17 > 0) || (iteratorVariable13.localeSize > 0))
                            {
                                System.Workflow.Interop.NativeMethods.ThrowOnFailure(iteratorVariable12.GetAssemblyRefProps(iteratorVariable10, out publicKeyOrToken, out iteratorVariable15, assemblyName, iteratorVariable17, out iteratorVariable17, out iteratorVariable13, out hashValueBlob, out iteratorVariable19, out iteratorVariable20));
                            }
                            string iteratorVariable21 = string.Empty;
                            for (int k = 0; k < iteratorVariable15; k++)
                            {
                                iteratorVariable21 = iteratorVariable21 + string.Format("{0}", Marshal.ReadByte(publicKeyOrToken, k).ToString("x2"));
                            }
                            yield return string.Format("{0}, {1}, Version={2}.{3}.{4}.{5}, Culture={6}, PublicKeyToken={7}", new object[] { iteratorVariable11, Marshal.PtrToStringUni(assemblyName), iteratorVariable13.majorVersion, iteratorVariable13.minorVersion, iteratorVariable13.buildNumber, iteratorVariable13.revisionNumber, string.IsNullOrEmpty(Marshal.PtrToStringUni(iteratorVariable13.locale)) ? "neutral" : Marshal.PtrToStringUni(iteratorVariable13.locale), string.IsNullOrEmpty(iteratorVariable21) ? "null" : iteratorVariable21 });
                        }
                    }
                }
            }
            while (typeRefs > 0);
        }

        private static MetadataTokenType TokenTypeFromToken(uint token)
        {
            return (((MetadataTokenType) token) & ((MetadataTokenType) (-16777216)));
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct AssemblyMetadata
        {
            public ushort majorVersion;
            public ushort minorVersion;
            public ushort buildNumber;
            public ushort revisionNumber;
            public IntPtr locale;
            public uint localeSize;
            public IntPtr processorIds;
            public uint processorIdCount;
            public IntPtr osInfo;
            public uint osInfoCount;
        }

        private static class Guids
        {
            public const string CLSID_MetaDataDispenser = "E5CB7A31-7512-11d2-89CE-0080C792E5D8";
            public const string IID_IMetaDataAssemblyImport = "EE62470B-E94B-424e-9B7C-2F00C9249F93";
            public const string IID_IMetaDataDispenser = "809C652E-7396-11d2-9771-00A0C9B4D50C";
            public const string IID_IMetaDataImport = "7DAC8207-D3AE-4c75-9B67-92801A497D44";
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("EE62470B-E94B-424e-9B7C-2F00C9249F93")]
        private interface IMetaDataAssemblyImport
        {
            void GetAssemblyProps();
            int GetAssemblyRefProps([In] uint assemblyRefToken, out IntPtr publicKeyOrToken, out uint sizePublicKeyOrToken, IntPtr assemblyName, [In] uint assemblyNameBufferSize, out uint assemblyNameSize, out MetaDataReader.AssemblyMetadata assemblyMetaData, out IntPtr hashValueBlob, out uint hashValueSize, out uint assemblyRefFlags);
        }

        [ComImport, Guid("809C652E-7396-11d2-9771-00A0C9B4D50C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMetaDataDispenser
        {
            void DefineScope();
            int OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] string scopeName, uint openFlags, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object unknown);
            void OpenScopeOnMemory();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
        private interface IMetaDataImport
        {
            void CloseEnum([In] IntPtr enumHandle);
            void CountEnum();
            void ResetEnum();
            void EnumTypeDefs();
            void EnumInterfaceImpls();
            int EnumTypeRefs([In, Out] ref IntPtr enumHandle, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] uint[] rTypeRefs, uint cMax, ref uint typeRefs);
            void FindTypeDefByName();
            void GetScopeProps();
            void GetModuleFromScope();
            void GetTypeDefProps();
            void GetInterfaceImplProps();
            int GetTypeRefProps([In] uint typeRefToken, out uint resolutionScopeToken, IntPtr typeRefName, uint nameLength, out uint actualLength);
        }

        [ComImport, Guid("E5CB7A31-7512-11d2-89CE-0080C792E5D8")]
        private class MetaDataDispenser
        {
        }

        private enum MetadataTokenType
        {
            AssemblyRef = 0x23000000,
            ModuleRef = 0x1a000000
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OsInfo
        {
            private uint osPlatformId;
            private uint osMajorVersion;
            private uint osMinorVersion;
        }
    }
}


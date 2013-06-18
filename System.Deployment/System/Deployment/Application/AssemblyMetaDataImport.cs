namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class AssemblyMetaDataImport : DisposableBase
    {
        private AssemblyReference[] _asmRefs;
        private IMetaDataAssemblyImport _assemblyImport;
        private static Guid _importerGuid = new Guid(((GuidAttribute) Attribute.GetCustomAttribute(typeof(IMetaDataImport), typeof(GuidAttribute), false)).Value);
        private IMetaDataDispenser _metaDispenser = ((IMetaDataDispenser) new CorMetaDataDispenser());
        private AssemblyModule[] _modules;
        private AssemblyName _name;
        private const int GENMAN_ENUM_TOKEN_BUF_SIZE = 0x10;
        private const int GENMAN_LOCALE_BUF_SIZE = 0x40;
        private const int GENMAN_STRING_BUF_SIZE = 0x400;

        public AssemblyMetaDataImport(string sourceFile)
        {
            this._assemblyImport = (IMetaDataAssemblyImport) this._metaDispenser.OpenScope(sourceFile, 0, ref _importerGuid);
        }

        private IntPtr AllocAsmMeta()
        {
            ASSEMBLYMETADATA assemblymetadata;
            assemblymetadata.usMajorVersion = assemblymetadata.usMinorVersion = assemblymetadata.usBuildNumber = (ushort) (assemblymetadata.usRevisionNumber = 0);
            assemblymetadata.cOses = assemblymetadata.cProcessors = 0;
            assemblymetadata.rOses = assemblymetadata.rpProcessors = IntPtr.Zero;
            assemblymetadata.rpLocale = Marshal.AllocCoTaskMem(0x80);
            assemblymetadata.cchLocale = 0x40;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ASSEMBLYMETADATA)));
            Marshal.StructureToPtr(assemblymetadata, ptr, false);
            return ptr;
        }

        private AssemblyName ConstructAssemblyName(IntPtr asmMetaPtr, char[] asmNameBuf, uint asmNameLength, IntPtr pubKeyPtr, uint pubKeyBytes, uint flags)
        {
            ASSEMBLYMETADATA assemblymetadata = (ASSEMBLYMETADATA) Marshal.PtrToStructure(asmMetaPtr, typeof(ASSEMBLYMETADATA));
            AssemblyName name = new AssemblyName {
                Name = new string(asmNameBuf, 0, ((int) asmNameLength) - 1),
                Version = new Version(assemblymetadata.usMajorVersion, assemblymetadata.usMinorVersion, assemblymetadata.usBuildNumber, assemblymetadata.usRevisionNumber)
            };
            string str = Marshal.PtrToStringUni(assemblymetadata.rpLocale);
            name.CultureInfo = new CultureInfo(str);
            if (pubKeyBytes > 0)
            {
                byte[] destination = new byte[pubKeyBytes];
                Marshal.Copy(pubKeyPtr, destination, 0, (int) pubKeyBytes);
                if ((flags & 1) != 0)
                {
                    name.SetPublicKey(destination);
                    return name;
                }
                name.SetPublicKeyToken(destination);
            }
            return name;
        }

        protected override void DisposeUnmanagedResources()
        {
            if (this._assemblyImport != null)
            {
                Marshal.ReleaseComObject(this._assemblyImport);
            }
            if (this._metaDispenser != null)
            {
                Marshal.ReleaseComObject(this._metaDispenser);
            }
        }

        private void FreeAsmMeta(IntPtr asmMetaPtr)
        {
            if (asmMetaPtr != IntPtr.Zero)
            {
                ASSEMBLYMETADATA assemblymetadata = (ASSEMBLYMETADATA) Marshal.PtrToStructure(asmMetaPtr, typeof(ASSEMBLYMETADATA));
                Marshal.FreeCoTaskMem(assemblymetadata.rpLocale);
                Marshal.DestroyStructure(asmMetaPtr, typeof(ASSEMBLYMETADATA));
                Marshal.FreeCoTaskMem(asmMetaPtr);
            }
        }

        private AssemblyModule[] ImportAssemblyFiles()
        {
            ArrayList list = new ArrayList();
            IntPtr zero = IntPtr.Zero;
            uint[] fileRefs = new uint[0x10];
            char[] strName = new char[0x400];
            try
            {
                uint num;
                do
                {
                    this._assemblyImport.EnumFiles(ref zero, fileRefs, (uint) fileRefs.Length, out num);
                    for (uint i = 0; i < num; i++)
                    {
                        IntPtr ptr2;
                        uint num3;
                        uint num4;
                        uint num5;
                        this._assemblyImport.GetFileProps(fileRefs[i], strName, (uint) strName.Length, out num3, out ptr2, out num4, out num5);
                        byte[] destination = new byte[num4];
                        Marshal.Copy(ptr2, destination, 0, (int) num4);
                        list.Add(new AssemblyModule(new string(strName, 0, ((int) num3) - 1), destination));
                    }
                }
                while (num > 0);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    this._assemblyImport.CloseEnum(zero);
                }
            }
            return (AssemblyModule[]) list.ToArray(typeof(AssemblyModule));
        }

        private AssemblyReference[] ImportAssemblyReferences()
        {
            ArrayList list = new ArrayList();
            IntPtr zero = IntPtr.Zero;
            uint[] asmRefs = new uint[0x10];
            try
            {
                uint num;
                do
                {
                    this._assemblyImport.EnumAssemblyRefs(ref zero, asmRefs, (uint) asmRefs.Length, out num);
                    for (uint i = 0; i < num; i++)
                    {
                        IntPtr ptr2;
                        IntPtr ptr3;
                        uint num3;
                        uint num4;
                        uint num5;
                        uint num6;
                        this._assemblyImport.GetAssemblyRefProps(asmRefs[i], out ptr3, out num4, null, 0, out num5, IntPtr.Zero, out ptr2, out num3, out num6);
                        char[] strName = new char[num5 + 1];
                        IntPtr amdInfo = IntPtr.Zero;
                        try
                        {
                            amdInfo = this.AllocAsmMeta();
                            this._assemblyImport.GetAssemblyRefProps(asmRefs[i], out ptr3, out num4, strName, (uint) strName.Length, out num5, amdInfo, out ptr2, out num3, out num6);
                            AssemblyName name = this.ConstructAssemblyName(amdInfo, strName, num5, ptr3, num4, num6);
                            list.Add(new AssemblyReference(name));
                        }
                        finally
                        {
                            this.FreeAsmMeta(amdInfo);
                        }
                    }
                }
                while (num > 0);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    this._assemblyImport.CloseEnum(zero);
                }
            }
            return (AssemblyReference[]) list.ToArray(typeof(AssemblyReference));
        }

        private AssemblyName ImportIdentity()
        {
            uint num;
            IntPtr ptr;
            uint num2;
            uint num3;
            uint num4;
            uint num5;
            AssemblyName name;
            this._assemblyImport.GetAssemblyFromScope(out num);
            this._assemblyImport.GetAssemblyProps(num, out ptr, out num2, out num3, null, 0, out num4, IntPtr.Zero, out num5);
            char[] strName = new char[num4 + 1];
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = this.AllocAsmMeta();
                this._assemblyImport.GetAssemblyProps(num, out ptr, out num2, out num3, strName, (uint) strName.Length, out num4, zero, out num5);
                name = this.ConstructAssemblyName(zero, strName, num4, ptr, num2, num5);
            }
            finally
            {
                this.FreeAsmMeta(zero);
            }
            return name;
        }

        public AssemblyModule[] Files
        {
            get
            {
                if (this._modules == null)
                {
                    lock (this)
                    {
                        if (this._modules == null)
                        {
                            this._modules = this.ImportAssemblyFiles();
                        }
                    }
                }
                return this._modules;
            }
        }

        public AssemblyName Name
        {
            get
            {
                if (this._name == null)
                {
                    lock (this)
                    {
                        if (this._name == null)
                        {
                            this._name = this.ImportIdentity();
                        }
                    }
                }
                return this._name;
            }
        }

        public AssemblyReference[] References
        {
            get
            {
                if (this._asmRefs == null)
                {
                    lock (this)
                    {
                        if (this._asmRefs == null)
                        {
                            this._asmRefs = this.ImportAssemblyReferences();
                        }
                    }
                }
                return this._asmRefs;
            }
        }
    }
}


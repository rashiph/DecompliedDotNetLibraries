namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class AssemblyInformation : DisposableBase
    {
        private AssemblyNameExtension[] assemblyDependencies;
        private string[] assemblyFiles;
        private IMetaDataAssemblyImport assemblyImport;
        private const int GENMAN_ENUM_TOKEN_BUF_SIZE = 0x10;
        private const int GENMAN_LOCALE_BUF_SIZE = 0x40;
        private const int GENMAN_STRING_BUF_SIZE = 0x400;
        private static Guid importerGuid = new Guid(((GuidAttribute) Attribute.GetCustomAttribute(typeof(IMetaDataImport), typeof(GuidAttribute), false)).Value);
        private IMetaDataDispenser metadataDispenser;

        internal AssemblyInformation(string sourceFile)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(sourceFile, "sourceFile");
            this.metadataDispenser = (IMetaDataDispenser) new CorMetaDataDispenser();
            this.assemblyImport = (IMetaDataAssemblyImport) this.metadataDispenser.OpenScope(sourceFile, 0, ref importerGuid);
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

        private AssemblyNameExtension ConstructAssemblyName(IntPtr asmMetaPtr, char[] asmNameBuf, uint asmNameLength, IntPtr pubKeyPtr, uint pubKeyBytes, uint flags)
        {
            ASSEMBLYMETADATA assemblymetadata = (ASSEMBLYMETADATA) Marshal.PtrToStructure(asmMetaPtr, typeof(ASSEMBLYMETADATA));
            AssemblyName assemblyName = new AssemblyName {
                Name = new string(asmNameBuf, 0, ((int) asmNameLength) - 1),
                Version = new Version(assemblymetadata.usMajorVersion, assemblymetadata.usMinorVersion, assemblymetadata.usBuildNumber, assemblymetadata.usRevisionNumber)
            };
            string name = Marshal.PtrToStringUni(assemblymetadata.rpLocale);
            if (name.Length > 0)
            {
                assemblyName.CultureInfo = CultureInfo.CreateSpecificCulture(name);
            }
            else
            {
                assemblyName.CultureInfo = CultureInfo.CreateSpecificCulture(string.Empty);
            }
            byte[] destination = new byte[pubKeyBytes];
            Marshal.Copy(pubKeyPtr, destination, 0, (int) pubKeyBytes);
            if ((flags & 1) != 0)
            {
                assemblyName.SetPublicKey(destination);
            }
            else
            {
                assemblyName.SetPublicKeyToken(destination);
            }
            assemblyName.Flags = (AssemblyNameFlags) flags;
            return new AssemblyNameExtension(assemblyName);
        }

        protected override void DisposeUnmanagedResources()
        {
            if (this.assemblyImport != null)
            {
                Marshal.ReleaseComObject(this.assemblyImport);
            }
            if (this.metadataDispenser != null)
            {
                Marshal.ReleaseComObject(this.metadataDispenser);
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

        internal static void GetAssemblyMetadata(string path, out AssemblyNameExtension[] dependencies, out string[] scatterFiles)
        {
            AssemblyInformation information = null;
            using (information = new AssemblyInformation(path))
            {
                dependencies = information.Dependencies;
                scatterFiles = information.Files;
            }
        }

        internal static string GetRuntimeVersion(string path)
        {
            StringBuilder szBuffer = null;
            uint num = 0;
            uint dwLength = 0;
            int capacity = 11;
            do
            {
                szBuffer = new StringBuilder(capacity);
                num = Microsoft.Build.Tasks.NativeMethods.GetFileVersion(path, szBuffer, capacity, out dwLength);
                capacity *= 2;
            }
            while (num == 0x8007007a);
            if ((num == 0) && (szBuffer != null))
            {
                return szBuffer.ToString();
            }
            return string.Empty;
        }

        private AssemblyNameExtension[] ImportAssemblyDependencies()
        {
            ArrayList list = new ArrayList();
            IntPtr zero = IntPtr.Zero;
            uint[] asmRefs = new uint[0x10];
            try
            {
                uint num;
                do
                {
                    this.assemblyImport.EnumAssemblyRefs(ref zero, asmRefs, (uint) asmRefs.Length, out num);
                    for (uint i = 0; i < num; i++)
                    {
                        IntPtr ptr2;
                        IntPtr ptr3;
                        uint num3;
                        uint num4;
                        uint num5;
                        uint num6;
                        this.assemblyImport.GetAssemblyRefProps(asmRefs[i], out ptr3, out num4, null, 0, out num5, IntPtr.Zero, out ptr2, out num3, out num6);
                        char[] strName = new char[num5 + 1];
                        IntPtr amdInfo = IntPtr.Zero;
                        try
                        {
                            amdInfo = this.AllocAsmMeta();
                            this.assemblyImport.GetAssemblyRefProps(asmRefs[i], out ptr3, out num4, strName, (uint) strName.Length, out num5, amdInfo, out ptr2, out num3, out num6);
                            AssemblyNameExtension extension = this.ConstructAssemblyName(amdInfo, strName, num5, ptr3, num4, num6);
                            list.Add(extension);
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
                    this.assemblyImport.CloseEnum(zero);
                }
            }
            return (AssemblyNameExtension[]) list.ToArray(typeof(AssemblyNameExtension));
        }

        private string[] ImportFiles()
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
                    this.assemblyImport.EnumFiles(ref zero, fileRefs, (uint) fileRefs.Length, out num);
                    for (uint i = 0; i < num; i++)
                    {
                        IntPtr ptr2;
                        uint num3;
                        uint num4;
                        uint num5;
                        this.assemblyImport.GetFileProps(fileRefs[i], strName, (uint) strName.Length, out num3, out ptr2, out num4, out num5);
                        string str = new string(strName, 0, ((int) num3) - 1);
                        list.Add(str);
                    }
                }
                while (num > 0);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    this.assemblyImport.CloseEnum(zero);
                }
            }
            return (string[]) list.ToArray(typeof(string));
        }

        public AssemblyNameExtension[] Dependencies
        {
            get
            {
                if (this.assemblyDependencies == null)
                {
                    lock (this)
                    {
                        if (this.assemblyDependencies == null)
                        {
                            this.assemblyDependencies = this.ImportAssemblyDependencies();
                        }
                    }
                }
                return this.assemblyDependencies;
            }
        }

        public string[] Files
        {
            get
            {
                if (this.assemblyFiles == null)
                {
                    lock (this)
                    {
                        if (this.assemblyFiles == null)
                        {
                            this.assemblyFiles = this.ImportFiles();
                        }
                    }
                }
                return this.assemblyFiles;
            }
        }
    }
}


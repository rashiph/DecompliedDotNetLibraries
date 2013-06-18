namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Build.Tasks;
    using System;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;

    internal class MetadataReader : IDisposable
    {
        private IMetaDataAssemblyImport _assemblyImport;
        private StringDictionary _attributes;
        private static Guid _importerGuid = GetGuidOfType(typeof(IMetaDataImport));
        private IMetaDataDispenser _metaDispenser;
        private readonly string _path;
        private static Guid _refidGuid = GetGuidOfType(typeof(IReferenceIdentity));

        private MetadataReader(string path)
        {
            object obj2;
            this._path = path;
            this._metaDispenser = (IMetaDataDispenser) new CorMetaDataDispenser();
            if (this._metaDispenser.OpenScope(path, 0, ref _importerGuid, out obj2) == 0)
            {
                this._assemblyImport = (IMetaDataAssemblyImport) obj2;
            }
        }

        public void Close()
        {
            if (this._assemblyImport != null)
            {
                Marshal.ReleaseComObject(this._assemblyImport);
            }
            if (this._metaDispenser != null)
            {
                Marshal.ReleaseComObject(this._metaDispenser);
            }
            this._attributes = null;
            this._metaDispenser = null;
            this._assemblyImport = null;
        }

        public static MetadataReader Create(string path)
        {
            MetadataReader reader = new MetadataReader(path);
            if (reader._assemblyImport != null)
            {
                return reader;
            }
            return null;
        }

        private static Guid GetGuidOfType(Type type)
        {
            GuidAttribute attribute = (GuidAttribute) Attribute.GetCustomAttribute(type, typeof(GuidAttribute), false);
            return new Guid(attribute.Value);
        }

        public bool HasAssemblyAttribute(string name)
        {
            uint num;
            this._assemblyImport.GetAssemblyFromScope(out num);
            IMetaDataImport2 import = (IMetaDataImport2) this._assemblyImport;
            IntPtr zero = IntPtr.Zero;
            uint pcbData = 0;
            import.GetCustomAttributeByName(num, name.ToCharArray(), out zero, out pcbData);
            return (pcbData != 0);
        }

        private void ImportAttributes()
        {
            IReferenceIdentity assemblyIdentityFromFile = (IReferenceIdentity) Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.GetAssemblyIdentityFromFile(this._path, ref _refidGuid);
            string attribute = assemblyIdentityFromFile.GetAttribute(null, "name");
            string str2 = assemblyIdentityFromFile.GetAttribute(null, "version");
            string a = assemblyIdentityFromFile.GetAttribute(null, "publicKeyToken");
            if (string.Equals(a, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                a = string.Empty;
            }
            else if (!string.IsNullOrEmpty(a))
            {
                a = a.ToUpperInvariant();
            }
            string str4 = assemblyIdentityFromFile.GetAttribute(null, "culture");
            string str5 = assemblyIdentityFromFile.GetAttribute(null, "processorArchitecture");
            if (!string.IsNullOrEmpty(str5))
            {
                str5 = str5.ToLowerInvariant();
            }
            this._attributes = new StringDictionary();
            this._attributes.Add("Name", attribute);
            this._attributes.Add("Version", str2);
            this._attributes.Add("PublicKeyToken", a);
            this._attributes.Add("Culture", str4);
            this._attributes.Add("ProcessorArchitecture", str5);
            assemblyIdentityFromFile = null;
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        private StringDictionary Attributes
        {
            get
            {
                if (this._attributes == null)
                {
                    lock (this)
                    {
                        if (this._attributes == null)
                        {
                            this.ImportAttributes();
                        }
                    }
                }
                return this._attributes;
            }
        }

        public string Culture
        {
            get
            {
                return this.Attributes["Culture"];
            }
        }

        public string Name
        {
            get
            {
                return this.Attributes["Name"];
            }
        }

        public string ProcessorArchitecture
        {
            get
            {
                return this.Attributes["ProcessorArchitecture"];
            }
        }

        public string PublicKeyToken
        {
            get
            {
                return this.Attributes["PublicKeyToken"];
            }
        }

        public string Version
        {
            get
            {
                return this.Attributes["Version"];
            }
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FRestricted), Guid("809c652e-7396-11d2-9771-00a0c9b4d50c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMetaDataDispenser
        {
            int DefineScope();
            [PreserveSig]
            int OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] string szScope, [In] uint dwOpenFlags, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object obj);
            int OpenScopeOnMemory();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("6eaf5ace-7917-4f3c-b129-e046a9704766")]
        private interface IReferenceIdentity
        {
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetAttribute([In, MarshalAs(UnmanagedType.LPWStr)] string Namespace, [In, MarshalAs(UnmanagedType.LPWStr)] string Name);
            void SetAttribute();
            void EnumAttributes();
            void Clone();
        }
    }
}


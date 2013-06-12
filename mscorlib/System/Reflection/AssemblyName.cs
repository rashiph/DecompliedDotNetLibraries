namespace System.Reflection
{
    using System;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;

    [Serializable, ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_AssemblyName)), ComVisible(true)]
    public sealed class AssemblyName : _AssemblyName, ICloneable, ISerializable, IDeserializationCallback
    {
        private string _CodeBase;
        private System.Globalization.CultureInfo _CultureInfo;
        private AssemblyNameFlags _Flags;
        private AssemblyHashAlgorithm _HashAlgorithm;
        private AssemblyHashAlgorithm _HashAlgorithmForControl;
        private byte[] _HashForControl;
        private string _Name;
        private byte[] _PublicKey;
        private byte[] _PublicKeyToken;
        private StrongNameKeyPair _StrongNameKeyPair;
        private System.Version _Version;
        private AssemblyVersionCompatibility _VersionCompatibility;
        private SerializationInfo m_siInfo;

        public AssemblyName()
        {
            this._HashAlgorithm = AssemblyHashAlgorithm.None;
            this._VersionCompatibility = AssemblyVersionCompatibility.SameMachine;
            this._Flags = AssemblyNameFlags.None;
        }

        [SecuritySafeCritical]
        public AssemblyName(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            if ((assemblyName.Length == 0) || (assemblyName[0] == '\0'))
            {
                throw new ArgumentException(Environment.GetResourceString("Format_StringZeroLength"));
            }
            this._Name = assemblyName;
            this.nInit();
        }

        internal AssemblyName(SerializationInfo info, StreamingContext context)
        {
            this.m_siInfo = info;
        }

        internal static System.Reflection.ProcessorArchitecture CalculateProcArchIndex(PortableExecutableKinds pek, ImageFileMachine ifm, AssemblyNameFlags flags)
        {
            if ((flags & 240) != 0x70)
            {
                if ((pek & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
                {
                    ImageFileMachine machine = ifm;
                    if (machine == ImageFileMachine.I386)
                    {
                        if ((pek & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
                        {
                            return System.Reflection.ProcessorArchitecture.MSIL;
                        }
                    }
                    else
                    {
                        if (machine == ImageFileMachine.IA64)
                        {
                            return System.Reflection.ProcessorArchitecture.IA64;
                        }
                        if (machine == ImageFileMachine.AMD64)
                        {
                            return System.Reflection.ProcessorArchitecture.Amd64;
                        }
                    }
                }
                else if (ifm == ImageFileMachine.I386)
                {
                    if (((pek & PortableExecutableKinds.Required32Bit) != PortableExecutableKinds.Required32Bit) && ((pek & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly))
                    {
                        return System.Reflection.ProcessorArchitecture.MSIL;
                    }
                    return System.Reflection.ProcessorArchitecture.X86;
                }
            }
            return System.Reflection.ProcessorArchitecture.None;
        }

        public object Clone()
        {
            AssemblyName name = new AssemblyName();
            name.Init(this._Name, this._PublicKey, this._PublicKeyToken, this._Version, this._CultureInfo, this._HashAlgorithm, this._VersionCompatibility, this._CodeBase, this._Flags, this._StrongNameKeyPair);
            name._HashForControl = this._HashForControl;
            name._HashAlgorithmForControl = this._HashAlgorithmForControl;
            return name;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string EscapeCodeBase(string codeBase);
        [SecuritySafeCritical]
        public static AssemblyName GetAssemblyName(string assemblyFile)
        {
            if (assemblyFile == null)
            {
                throw new ArgumentNullException("assemblyFile");
            }
            string fullPathInternal = Path.GetFullPathInternal(assemblyFile);
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullPathInternal).Demand();
            return nGetFileInformation(fullPathInternal);
        }

        internal string GetNameWithPublicKey()
        {
            byte[] publicKey = this.GetPublicKey();
            return (this.Name + ", PublicKey=" + Hex.EncodeHexString(publicKey));
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("_Name", this._Name);
            info.AddValue("_PublicKey", this._PublicKey, typeof(byte[]));
            info.AddValue("_PublicKeyToken", this._PublicKeyToken, typeof(byte[]));
            info.AddValue("_CultureInfo", (this._CultureInfo == null) ? -1 : this._CultureInfo.LCID);
            info.AddValue("_CodeBase", this._CodeBase);
            info.AddValue("_Version", this._Version);
            info.AddValue("_HashAlgorithm", this._HashAlgorithm, typeof(AssemblyHashAlgorithm));
            info.AddValue("_HashAlgorithmForControl", this._HashAlgorithmForControl, typeof(AssemblyHashAlgorithm));
            info.AddValue("_StrongNameKeyPair", this._StrongNameKeyPair, typeof(StrongNameKeyPair));
            info.AddValue("_VersionCompatibility", this._VersionCompatibility, typeof(AssemblyVersionCompatibility));
            info.AddValue("_Flags", this._Flags, typeof(AssemblyNameFlags));
            info.AddValue("_HashForControl", this._HashForControl, typeof(byte[]));
        }

        public byte[] GetPublicKey()
        {
            return this._PublicKey;
        }

        [SecuritySafeCritical]
        public byte[] GetPublicKeyToken()
        {
            if (this._PublicKeyToken == null)
            {
                this._PublicKeyToken = this.nGetPublicKeyToken();
            }
            return this._PublicKeyToken;
        }

        internal void Init(string name, byte[] publicKey, byte[] publicKeyToken, System.Version version, System.Globalization.CultureInfo cultureInfo, AssemblyHashAlgorithm hashAlgorithm, AssemblyVersionCompatibility versionCompatibility, string codeBase, AssemblyNameFlags flags, StrongNameKeyPair keyPair)
        {
            this._Name = name;
            if (publicKey != null)
            {
                this._PublicKey = new byte[publicKey.Length];
                Array.Copy(publicKey, this._PublicKey, publicKey.Length);
            }
            if (publicKeyToken != null)
            {
                this._PublicKeyToken = new byte[publicKeyToken.Length];
                Array.Copy(publicKeyToken, this._PublicKeyToken, publicKeyToken.Length);
            }
            if (version != null)
            {
                this._Version = (System.Version) version.Clone();
            }
            this._CultureInfo = cultureInfo;
            this._HashAlgorithm = hashAlgorithm;
            this._VersionCompatibility = versionCompatibility;
            this._CodeBase = codeBase;
            this._Flags = flags;
            this._StrongNameKeyPair = keyPair;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern AssemblyName nGetFileInformation(string s);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern byte[] nGetPublicKeyToken();
        [SecurityCritical]
        internal void nInit()
        {
            RuntimeAssembly assembly = null;
            this.nInit(out assembly, false, false);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern int nInit(out RuntimeAssembly assembly, bool forIntrospection, bool raiseResolveEvent);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern string nToString();
        public void OnDeserialization(object sender)
        {
            if (this.m_siInfo != null)
            {
                this._Name = this.m_siInfo.GetString("_Name");
                this._PublicKey = (byte[]) this.m_siInfo.GetValue("_PublicKey", typeof(byte[]));
                this._PublicKeyToken = (byte[]) this.m_siInfo.GetValue("_PublicKeyToken", typeof(byte[]));
                int culture = this.m_siInfo.GetInt32("_CultureInfo");
                if (culture != -1)
                {
                    this._CultureInfo = new System.Globalization.CultureInfo(culture);
                }
                this._CodeBase = this.m_siInfo.GetString("_CodeBase");
                this._Version = (System.Version) this.m_siInfo.GetValue("_Version", typeof(System.Version));
                this._HashAlgorithm = (AssemblyHashAlgorithm) this.m_siInfo.GetValue("_HashAlgorithm", typeof(AssemblyHashAlgorithm));
                this._StrongNameKeyPair = (StrongNameKeyPair) this.m_siInfo.GetValue("_StrongNameKeyPair", typeof(StrongNameKeyPair));
                this._VersionCompatibility = (AssemblyVersionCompatibility) this.m_siInfo.GetValue("_VersionCompatibility", typeof(AssemblyVersionCompatibility));
                this._Flags = (AssemblyNameFlags) this.m_siInfo.GetValue("_Flags", typeof(AssemblyNameFlags));
                try
                {
                    this._HashAlgorithmForControl = (AssemblyHashAlgorithm) this.m_siInfo.GetValue("_HashAlgorithmForControl", typeof(AssemblyHashAlgorithm));
                    this._HashForControl = (byte[]) this.m_siInfo.GetValue("_HashForControl", typeof(byte[]));
                }
                catch (SerializationException)
                {
                    this._HashAlgorithmForControl = AssemblyHashAlgorithm.None;
                    this._HashForControl = null;
                }
                this.m_siInfo = null;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern bool ReferenceMatchesDefinition(AssemblyName reference, AssemblyName definition);
        internal void SetHashControl(byte[] hash, AssemblyHashAlgorithm hashAlgorithm)
        {
            this._HashForControl = hash;
            this._HashAlgorithmForControl = hashAlgorithm;
        }

        internal void SetProcArchIndex(PortableExecutableKinds pek, ImageFileMachine ifm)
        {
            this.ProcessorArchitecture = CalculateProcArchIndex(pek, ifm, this._Flags);
        }

        public void SetPublicKey(byte[] publicKey)
        {
            this._PublicKey = publicKey;
            if (publicKey == null)
            {
                this._Flags &= ~AssemblyNameFlags.PublicKey;
            }
            else
            {
                this._Flags |= AssemblyNameFlags.PublicKey;
            }
        }

        public void SetPublicKeyToken(byte[] publicKeyToken)
        {
            this._PublicKeyToken = publicKeyToken;
        }

        void _AssemblyName.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _AssemblyName.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _AssemblyName.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _AssemblyName.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string fullName = this.FullName;
            if (fullName == null)
            {
                return base.ToString();
            }
            return fullName;
        }

        public string CodeBase
        {
            get
            {
                return this._CodeBase;
            }
            set
            {
                this._CodeBase = value;
            }
        }

        public System.Globalization.CultureInfo CultureInfo
        {
            get
            {
                return this._CultureInfo;
            }
            set
            {
                this._CultureInfo = value;
            }
        }

        public string EscapedCodeBase
        {
            [SecuritySafeCritical]
            get
            {
                if (this._CodeBase == null)
                {
                    return null;
                }
                return EscapeCodeBase(this._CodeBase);
            }
        }

        public AssemblyNameFlags Flags
        {
            get
            {
                return (this._Flags & -241);
            }
            set
            {
                this._Flags &= 240;
                this._Flags |= value & -241;
            }
        }

        public string FullName
        {
            [SecuritySafeCritical]
            get
            {
                return this.nToString();
            }
        }

        public AssemblyHashAlgorithm HashAlgorithm
        {
            get
            {
                return this._HashAlgorithm;
            }
            set
            {
                this._HashAlgorithm = value;
            }
        }

        public StrongNameKeyPair KeyPair
        {
            get
            {
                return this._StrongNameKeyPair;
            }
            set
            {
                this._StrongNameKeyPair = value;
            }
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }
        }

        public System.Reflection.ProcessorArchitecture ProcessorArchitecture
        {
            get
            {
                int num = ((int) (this._Flags & 0x70)) >> 4;
                if (num > 4)
                {
                    num = 0;
                }
                return (System.Reflection.ProcessorArchitecture) num;
            }
            set
            {
                int num = ((int) value) & 7;
                if (num <= 4)
                {
                    this._Flags = (AssemblyNameFlags) ((int) (((long) this._Flags) & 0xffffff0fL));
                    this._Flags |= num << 4;
                }
            }
        }

        public System.Version Version
        {
            get
            {
                return this._Version;
            }
            set
            {
                this._Version = value;
            }
        }

        public AssemblyVersionCompatibility VersionCompatibility
        {
            get
            {
                return this._VersionCompatibility;
            }
            set
            {
                this._VersionCompatibility = value;
            }
        }
    }
}


namespace System.Reflection
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.IO;
    using System.Reflection.Cache;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Util;
    using System.Text;
    using System.Threading;

    [Serializable, ForceTokenStabilization]
    internal class RuntimeAssembly : Assembly, ICustomQueryInterface
    {
        private const uint COR_E_LOADING_REFERENCE_ASSEMBLY = 0x80131058;
        [ForceTokenStabilization]
        private IntPtr m_assembly;
        private InternalCache m_cachedData;
        private object m_syncRoot;
        private const string s_localFilePrefix = "file:";

        private event ModuleResolveEventHandler _ModuleResolve;

        public event ModuleResolveEventHandler ModuleResolve
        {
            [SecurityCritical] add
            {
                this._ModuleResolve += value;
            }
            [SecurityCritical] remove
            {
                this._ModuleResolve -= value;
            }
        }

        internal RuntimeAssembly()
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern RuntimeAssembly _nLoad(AssemblyName fileName, string codeBase, System.Security.Policy.Evidence assemblySecurity, RuntimeAssembly locationHint, ref StackCrawlMark stackMark, bool throwOnFileNotFound, bool forIntrospection, bool suppressSecurityChecks);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool AptcaCheck(RuntimeAssembly targetAssembly, RuntimeAssembly sourceAssembly);
        private static IPermission CreateWebPermission(string codeBase)
        {
            Assembly assembly = Assembly.Load("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type enumType = assembly.GetType("System.Net.NetworkAccess", true);
            IPermission permission = null;
            if (enumType.IsEnum && enumType.IsVisible)
            {
                object[] args = new object[2];
                args[0] = (Enum) Enum.Parse(enumType, "Connect", true);
                if (args[0] != null)
                {
                    args[1] = codeBase;
                    enumType = assembly.GetType("System.Net.WebPermission", true);
                    if (enumType.IsVisible)
                    {
                        permission = (IPermission) Activator.CreateInstance(enumType, args);
                    }
                }
            }
            if (permission == null)
            {
                throw new InvalidOperationException();
            }
            return permission;
        }

        private static bool CulturesEqual(CultureInfo refCI, CultureInfo defCI)
        {
            bool flag = defCI.Equals(CultureInfo.InvariantCulture);
            if ((refCI == null) || refCI.Equals(CultureInfo.InvariantCulture))
            {
                return flag;
            }
            return (!flag && defCI.Equals(refCI));
        }

        [SecurityCritical]
        private static void DemandPermission(string codeBase, bool havePath, int demandFlag)
        {
            FileIOPermissionAccess pathDiscovery = FileIOPermissionAccess.PathDiscovery;
            switch (demandFlag)
            {
                case 1:
                    pathDiscovery = FileIOPermissionAccess.Read;
                    break;

                case 2:
                    pathDiscovery = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read;
                    break;

                case 3:
                    CreateWebPermission(AssemblyName.EscapeCodeBase(codeBase)).Demand();
                    return;
            }
            if (!havePath)
            {
                codeBase = new URLString(codeBase, true).GetFileName();
            }
            codeBase = Path.GetFullPathInternal(codeBase);
            new FileIOPermission(pathDiscovery, codeBase).Demand();
        }

        [SecurityCritical]
        private static AssemblyName EnumerateCache(AssemblyName partialName)
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            partialName.Version = null;
            ArrayList alAssems = new ArrayList();
            Fusion.ReadCache(alAssems, partialName.FullName, 2);
            IEnumerator enumerator = alAssems.GetEnumerator();
            AssemblyName name = null;
            CultureInfo cultureInfo = partialName.CultureInfo;
            while (enumerator.MoveNext())
            {
                AssemblyName name2 = new AssemblyName((string) enumerator.Current);
                if (CulturesEqual(cultureInfo, name2.CultureInfo))
                {
                    if (name == null)
                    {
                        name = name2;
                    }
                    else if (name2.Version > name.Version)
                    {
                        name = name2;
                    }
                }
            }
            return name;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool FCallIsDynamic(RuntimeAssembly assembly);
        [SecurityCritical]
        internal string GetCodeBase(bool copiedName)
        {
            string s = null;
            GetCodeBase(this.GetNativeHandle(), copiedName, JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetCodeBase(RuntimeAssembly assembly, bool copiedName, StringHandleOnStack retString);
        public override object[] GetCustomAttributes(bool inherit)
        {
            return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
            }
            return CustomAttribute.GetCustomAttributes(this, underlyingSystemType);
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return CustomAttributeData.GetCustomAttributesInternal(this);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetEntryPoint(RuntimeAssembly assembly, ObjectHandleOnStack retMethod);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetEvidence(RuntimeAssembly assembly, ObjectHandleOnStack retEvidence);
        [SecurityCritical]
        internal static Assembly GetExecutingAssembly(ref StackCrawlMark stackMark)
        {
            RuntimeAssembly o = null;
            GetExecutingAssembly(JitHelpers.GetStackCrawlMarkHandle(ref stackMark), JitHelpers.GetObjectHandleOnStack<RuntimeAssembly>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetExecutingAssembly(StackCrawlMarkHandle stackMark, ObjectHandleOnStack retAssembly);
        [SecuritySafeCritical]
        public override Type[] GetExportedTypes()
        {
            Type[] o = null;
            GetExportedTypes(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<Type[]>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetExportedTypes(RuntimeAssembly assembly, ObjectHandleOnStack retTypes);
        [SecuritySafeCritical]
        public override FileStream GetFile(string name)
        {
            RuntimeModule module = (RuntimeModule) this.GetModule(name);
            if (module == null)
            {
                return null;
            }
            return new FileStream(module.GetFullyQualifiedName(), FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        [SecuritySafeCritical]
        public override FileStream[] GetFiles(bool getResourceModules)
        {
            Module[] modules = this.GetModules(getResourceModules);
            int length = modules.Length;
            FileStream[] streamArray = new FileStream[length];
            for (int i = 0; i < length; i++)
            {
                streamArray[i] = new FileStream(((RuntimeModule) modules[i]).GetFullyQualifiedName(), FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            return streamArray;
        }

        [SecurityCritical]
        private AssemblyNameFlags GetFlags()
        {
            return GetFlags(this.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern AssemblyNameFlags GetFlags(RuntimeAssembly assembly);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void GetForwardedTypes(RuntimeAssembly assembly, ObjectHandleOnStack retTypes);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetFullName(RuntimeAssembly assembly, StringHandleOnStack retString);
        [SecurityCritical]
        internal void GetGrantSet(out System.Security.PermissionSet newGrant, out System.Security.PermissionSet newDenied)
        {
            System.Security.PermissionSet o = null;
            System.Security.PermissionSet set2 = null;
            GetGrantSet(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<System.Security.PermissionSet>(ref o), JitHelpers.GetObjectHandleOnStack<System.Security.PermissionSet>(ref set2));
            newGrant = o;
            newDenied = set2;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetGrantSet(RuntimeAssembly assembly, ObjectHandleOnStack granted, ObjectHandleOnStack denied);
        [SecurityCritical]
        private AssemblyHashAlgorithm GetHashAlgorithm()
        {
            return GetHashAlgorithm(this.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern AssemblyHashAlgorithm GetHashAlgorithm(RuntimeAssembly assembly);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern long GetHostContext(RuntimeAssembly assembly);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetImageRuntimeVersion(RuntimeAssembly assembly, StringHandleOnStack retString);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool GetIsStrongNameVerified(RuntimeAssembly assembly);
        public override Module[] GetLoadedModules(bool getResourceModules)
        {
            return this.GetModulesInternal(false, getResourceModules);
        }

        [SecurityCritical]
        internal CultureInfo GetLocale()
        {
            string s = null;
            GetLocale(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s));
            if (s == null)
            {
                return CultureInfo.InvariantCulture;
            }
            return new CultureInfo(s);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetLocale(RuntimeAssembly assembly, StringHandleOnStack retString);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetLocation(RuntimeAssembly assembly, StringHandleOnStack retString);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern RuntimeModule GetManifestModule(RuntimeAssembly assembly);
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
        {
            RuntimeAssembly o = null;
            string s = null;
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            int num = GetManifestResourceInfo(this.GetNativeHandle(), resourceName, JitHelpers.GetObjectHandleOnStack<RuntimeAssembly>(ref o), JitHelpers.GetStringHandleOnStack(ref s), JitHelpers.GetStackCrawlMarkHandle(ref lookForMyCaller));
            if (num == -1)
            {
                return null;
            }
            return new ManifestResourceInfo(o, s, (ResourceLocation) num);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetManifestResourceInfo(RuntimeAssembly assembly, string resourceName, ObjectHandleOnStack assemblyRef, StringHandleOnStack retFileName, StackCrawlMarkHandle stackMark);
        [SecuritySafeCritical]
        public override string[] GetManifestResourceNames()
        {
            return GetManifestResourceNames(this.GetNativeHandle());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern string[] GetManifestResourceNames(RuntimeAssembly assembly);
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public override Stream GetManifestResourceStream(string name)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.GetManifestResourceStream(name, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public override Stream GetManifestResourceStream(Type type, string name)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.GetManifestResourceStream(type, name, false, ref lookForMyCaller);
        }

        [SecurityCritical]
        internal unsafe Stream GetManifestResourceStream(string name, ref StackCrawlMark stackMark, bool skipSecurityCheck)
        {
            ulong length = 0L;
            byte* pointer = GetResource(this.GetNativeHandle(), name, out length, JitHelpers.GetStackCrawlMarkHandle(ref stackMark), skipSecurityCheck);
            if (pointer == null)
            {
                return null;
            }
            if (length > 0x7fffffffffffffffL)
            {
                throw new NotImplementedException(Environment.GetResourceString("NotImplemented_ResourcesLongerThan2^63"));
            }
            return new UnmanagedMemoryStream(pointer, (long) length, (long) length, FileAccess.Read, true);
        }

        [SecurityCritical]
        internal Stream GetManifestResourceStream(Type type, string name, bool skipSecurityCheck, ref StackCrawlMark stackMark)
        {
            StringBuilder builder = new StringBuilder();
            if (type == null)
            {
                if (name == null)
                {
                    throw new ArgumentNullException("type");
                }
            }
            else
            {
                string str = type.Namespace;
                if (str != null)
                {
                    builder.Append(str);
                    if (name != null)
                    {
                        builder.Append(Type.Delimiter);
                    }
                }
            }
            if (name != null)
            {
                builder.Append(name);
            }
            return this.GetManifestResourceStream(builder.ToString(), ref stackMark, skipSecurityCheck);
        }

        [SecuritySafeCritical]
        public override Module GetModule(string name)
        {
            Module o = null;
            GetModule(this.GetNativeHandle(), name, JitHelpers.GetObjectHandleOnStack<Module>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetModule(RuntimeAssembly assembly, string name, ObjectHandleOnStack retModule);
        public override Module[] GetModules(bool getResourceModules)
        {
            return this.GetModulesInternal(true, getResourceModules);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetModules(RuntimeAssembly assembly, bool loadIfNotFound, bool getResourceModules, ObjectHandleOnStack retModuleHandles);
        [SecuritySafeCritical]
        internal Module[] GetModulesInternal(bool loadIfNotFound, bool getResourceModules)
        {
            Module[] o = null;
            GetModules(this.GetNativeHandle(), loadIfNotFound, getResourceModules, JitHelpers.GetObjectHandleOnStack<Module[]>(ref o));
            return o;
        }

        [SecuritySafeCritical]
        public override AssemblyName GetName(bool copiedName)
        {
            AssemblyName name = new AssemblyName();
            string codeBase = this.GetCodeBase(copiedName);
            this.VerifyCodeBaseDiscovery(codeBase);
            name.Init(this.GetSimpleName(), this.GetPublicKey(), null, this.GetVersion(), this.GetLocale(), this.GetHashAlgorithm(), AssemblyVersionCompatibility.SameMachine, codeBase, this.GetFlags() | AssemblyNameFlags.PublicKey, null);
            Module manifestModule = this.ManifestModule;
            if ((manifestModule != null) && (manifestModule.MDStreamVersion > 0x10000))
            {
                PortableExecutableKinds kinds;
                ImageFileMachine machine;
                this.ManifestModule.GetPEKind(out kinds, out machine);
                name.SetProcArchIndex(kinds, machine);
            }
            return name;
        }

        internal RuntimeAssembly GetNativeHandle()
        {
            return this;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            UnitySerializationHolder.GetUnitySerializationInfo(info, 6, this.FullName, this);
        }

        [SecurityCritical]
        internal byte[] GetPublicKey()
        {
            byte[] o = null;
            GetPublicKey(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetPublicKey(RuntimeAssembly assembly, ObjectHandleOnStack retPublicKey);
        [SecuritySafeCritical]
        internal byte[] GetRawBytes()
        {
            byte[] o = null;
            GetRawBytes(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetRawBytes(RuntimeAssembly assembly, ObjectHandleOnStack retRawBytes);
        [SecuritySafeCritical]
        public override AssemblyName[] GetReferencedAssemblies()
        {
            return GetReferencedAssemblies(this.GetNativeHandle());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern AssemblyName[] GetReferencedAssemblies(RuntimeAssembly assembly);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern unsafe byte* GetResource(RuntimeAssembly assembly, string resourceName, out ulong length, StackCrawlMarkHandle stackMark, bool skipSecurityCheck);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override Assembly GetSatelliteAssembly(CultureInfo culture)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalGetSatelliteAssembly(culture, null, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalGetSatelliteAssembly(culture, version, ref lookForMyCaller);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern System.Security.SecurityRuleSet GetSecurityRuleSet(RuntimeAssembly assembly);
        [SecuritySafeCritical]
        internal string GetSimpleName()
        {
            string s = null;
            GetSimpleName(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetSimpleName(RuntimeAssembly assembly, StringHandleOnStack retSimpleName);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetToken(RuntimeAssembly assembly);
        [SecuritySafeCritical]
        public override Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            RuntimeType o = null;
            GetType(this.GetNativeHandle(), name, throwOnError, ignoreCase, JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetType(RuntimeAssembly assembly, string name, bool throwOnError, bool ignoreCase, ObjectHandleOnStack type);
        [SecurityCritical]
        internal Version GetVersion()
        {
            int num;
            int num2;
            int num3;
            int num4;
            GetVersion(this.GetNativeHandle(), out num, out num2, out num3, out num4);
            return new Version(num, num2, num3, num4);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetVersion(RuntimeAssembly assembly, out int majVer, out int minVer, out int buildNum, out int revNum);
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        internal Assembly InternalGetSatelliteAssembly(CultureInfo culture, Version version, ref StackCrawlMark stackMark)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            string name = this.GetSimpleName() + ".resources";
            return this.InternalGetSatelliteAssembly(name, culture, version, true, ref stackMark);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        internal RuntimeAssembly InternalGetSatelliteAssembly(string name, CultureInfo culture, Version version, bool throwOnFileNotFound, ref StackCrawlMark stackMark)
        {
            AssemblyName fileName = new AssemblyName();
            fileName.SetPublicKey(this.GetPublicKey());
            fileName.Flags = this.GetFlags() | AssemblyNameFlags.PublicKey;
            if (version == null)
            {
                fileName.Version = this.GetVersion();
            }
            else
            {
                fileName.Version = version;
            }
            fileName.CultureInfo = culture;
            fileName.Name = name;
            RuntimeAssembly assembly = nLoad(fileName, null, null, this, ref stackMark, throwOnFileNotFound, false, false);
            if (assembly == this)
            {
                throw new FileNotFoundException(string.Format(culture, Environment.GetResourceString("IO.FileNotFound_FileName"), new object[] { fileName.Name }));
            }
            return assembly;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        internal static RuntimeAssembly InternalLoad(string assemblyString, System.Security.Policy.Evidence assemblySecurity, ref StackCrawlMark stackMark, bool forIntrospection)
        {
            if (assemblyString == null)
            {
                throw new ArgumentNullException("assemblyString");
            }
            if ((assemblyString.Length == 0) || (assemblyString[0] == '\0'))
            {
                throw new ArgumentException(Environment.GetResourceString("Format_StringZeroLength"));
            }
            AssemblyName assemblyRef = new AssemblyName();
            RuntimeAssembly assembly = null;
            assemblyRef.Name = assemblyString;
            if (assemblyRef.nInit(out assembly, forIntrospection, true) == -2146234297)
            {
                return assembly;
            }
            return InternalLoadAssemblyName(assemblyRef, assemblySecurity, ref stackMark, forIntrospection, false);
        }

        [SecurityCritical]
        internal static RuntimeAssembly InternalLoadAssemblyName(AssemblyName assemblyRef, System.Security.Policy.Evidence assemblySecurity, ref StackCrawlMark stackMark, bool forIntrospection, bool suppressSecurityChecks)
        {
            if (assemblyRef == null)
            {
                throw new ArgumentNullException("assemblyRef");
            }
            assemblyRef = (AssemblyName) assemblyRef.Clone();
            if (assemblySecurity != null)
            {
                if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
                }
                if (!suppressSecurityChecks)
                {
                    new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
                }
            }
            string strA = VerifyCodeBase(assemblyRef.CodeBase);
            if ((strA != null) && !suppressSecurityChecks)
            {
                if (string.Compare(strA, 0, "file:", 0, 5, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    CreateWebPermission(assemblyRef.EscapedCodeBase).Demand();
                }
                else
                {
                    URLString str2 = new URLString(strA, true);
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, str2.GetFileName()).Demand();
                }
            }
            return nLoad(assemblyRef, strA, assemblySecurity, null, ref stackMark, true, forIntrospection, suppressSecurityChecks);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        internal static RuntimeAssembly InternalLoadFrom(string assemblyFile, System.Security.Policy.Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm, bool forIntrospection, bool suppressSecurityChecks, ref StackCrawlMark stackMark)
        {
            if (assemblyFile == null)
            {
                throw new ArgumentNullException("assemblyFile");
            }
            if ((securityEvidence != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            AssemblyName assemblyRef = new AssemblyName {
                CodeBase = assemblyFile
            };
            assemblyRef.SetHashControl(hashValue, hashAlgorithm);
            return InternalLoadAssemblyName(assemblyRef, securityEvidence, ref stackMark, forIntrospection, suppressSecurityChecks);
        }

        [SecuritySafeCritical]
        internal bool IsAllSecurityCritical()
        {
            return IsAllSecurityCritical(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsAllSecurityCritical(RuntimeAssembly assembly);
        [SecuritySafeCritical]
        internal bool IsAllSecuritySafeCritical()
        {
            return IsAllSecuritySafeCritical(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsAllSecuritySafeCritical(RuntimeAssembly assembly);
        [SecuritySafeCritical]
        internal bool IsAllSecurityTransparent()
        {
            return IsAllSecurityTransparent(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsAllSecurityTransparent(RuntimeAssembly assembly);
        [SecurityCritical, FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private bool IsAssemblyUnderAppBase()
        {
            string location = this.Location;
            if (string.IsNullOrEmpty(location))
            {
                return true;
            }
            FileIOAccess access = new FileIOAccess(Path.GetFullPathInternal(location));
            FileIOAccess operand = new FileIOAccess(Path.GetFullPathInternal(AppDomain.CurrentDomain.BaseDirectory));
            return access.IsSubsetOf(operand);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "caType");
            }
            return CustomAttribute.IsDefined(this, underlyingSystemType);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool IsGlobalAssemblyCache(RuntimeAssembly assembly);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool IsReflectionOnly(RuntimeAssembly assembly);
        private static bool IsSimplyNamed(AssemblyName partialName)
        {
            byte[] publicKeyToken = partialName.GetPublicKeyToken();
            if ((publicKeyToken != null) && (publicKeyToken.Length == 0))
            {
                return true;
            }
            publicKeyToken = partialName.GetPublicKey();
            return ((publicKeyToken != null) && (publicKeyToken.Length == 0));
        }

        [SecuritySafeCritical]
        private static bool IsUserError(Exception e)
        {
            return (Marshal.GetHRForException(e) == -2146234280);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlEvidence=true)]
        public override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
        {
            RuntimeModule o = null;
            LoadModule(this.GetNativeHandle(), moduleName, rawModule, (rawModule != null) ? rawModule.Length : 0, rawSymbolStore, (rawSymbolStore != null) ? rawSymbolStore.Length : 0, JitHelpers.GetObjectHandleOnStack<RuntimeModule>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void LoadModule(RuntimeAssembly assembly, string moduleName, byte[] rawModule, int cbModule, byte[] rawSymbolStore, int cbSymbolStore, ObjectHandleOnStack retModule);
        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private static RuntimeAssembly LoadWithPartialNameHack(string partialName, bool cropPublicKey)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            RuntimeAssembly assembly = null;
            AssemblyName name = new AssemblyName(partialName);
            if (!IsSimplyNamed(name))
            {
                if (cropPublicKey)
                {
                    name.SetPublicKey(null);
                    name.SetPublicKeyToken(null);
                }
                AssemblyName assemblyRef = EnumerateCache(name);
                if (assemblyRef != null)
                {
                    assembly = InternalLoadAssemblyName(assemblyRef, null, ref lookForMyCaller, false, false);
                }
            }
            return assembly;
        }

        [SecurityCritical]
        internal static RuntimeAssembly LoadWithPartialNameInternal(AssemblyName an, System.Security.Policy.Evidence securityEvidence, ref StackCrawlMark stackMark)
        {
            if (securityEvidence != null)
            {
                if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
                }
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
            }
            RuntimeAssembly assembly = null;
            try
            {
                assembly = nLoad(an, null, securityEvidence, null, ref stackMark, true, false, false);
            }
            catch (Exception exception)
            {
                if (exception.IsTransient)
                {
                    throw exception;
                }
                if (IsUserError(exception))
                {
                    throw;
                }
                if (IsSimplyNamed(an))
                {
                    return null;
                }
                AssemblyName assemblyRef = EnumerateCache(an);
                if (assemblyRef != null)
                {
                    return InternalLoadAssemblyName(assemblyRef, securityEvidence, ref stackMark, false, false);
                }
            }
            return assembly;
        }

        [SecurityCritical]
        internal static RuntimeAssembly LoadWithPartialNameInternal(string partialName, System.Security.Policy.Evidence securityEvidence, ref StackCrawlMark stackMark)
        {
            AssemblyName an = new AssemblyName(partialName);
            return LoadWithPartialNameInternal(an, securityEvidence, ref stackMark);
        }

        [SecurityCritical]
        private static RuntimeAssembly nLoad(AssemblyName fileName, string codeBase, System.Security.Policy.Evidence assemblySecurity, RuntimeAssembly locationHint, ref StackCrawlMark stackMark, bool throwOnFileNotFound, bool forIntrospection, bool suppressSecurityChecks)
        {
            return _nLoad(fileName, codeBase, assemblySecurity, locationHint, ref stackMark, throwOnFileNotFound, forIntrospection, suppressSecurityChecks);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeAssembly nLoadFile(string path, System.Security.Policy.Evidence evidence);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeAssembly nLoadImage(byte[] rawAssembly, byte[] rawSymbolStore, System.Security.Policy.Evidence evidence, ref StackCrawlMark stackMark, bool fIntrospection, SecurityContextSource securityContextSource);
        internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
        {
            this.m_cachedData = null;
        }

        private RuntimeModule OnModuleResolveEvent(string moduleName)
        {
            ModuleResolveEventHandler handler = this._ModuleResolve;
            if (handler != null)
            {
                Delegate[] invocationList = handler.GetInvocationList();
                int length = invocationList.Length;
                for (int i = 0; i < length; i++)
                {
                    RuntimeModule module = (RuntimeModule) ((ModuleResolveEventHandler) invocationList[i])(this, new ResolveEventArgs(moduleName, this));
                    if (module != null)
                    {
                        return module;
                    }
                }
            }
            return null;
        }

        [SecurityCritical]
        CustomQueryInterfaceResult ICustomQueryInterface.GetInterface([In] ref Guid iid, out IntPtr ppv)
        {
            if (iid == typeof(NativeMethods.IDispatch).GUID)
            {
                ppv = Marshal.GetComInterfaceForObject(this, typeof(_Assembly));
                return CustomQueryInterfaceResult.Handled;
            }
            ppv = IntPtr.Zero;
            return CustomQueryInterfaceResult.NotHandled;
        }

        private static string VerifyCodeBase(string codebase)
        {
            if (codebase == null)
            {
                return null;
            }
            int length = codebase.Length;
            if (length == 0)
            {
                return null;
            }
            int index = codebase.IndexOf(':');
            if ((((index != -1) && ((index + 2) < length)) && ((codebase[index + 1] == '/') || (codebase[index + 1] == '\\'))) && ((codebase[index + 2] == '/') || (codebase[index + 2] == '\\')))
            {
                return codebase;
            }
            if (((length > 2) && (codebase[0] == '\\')) && (codebase[1] == '\\'))
            {
                return ("file://" + codebase);
            }
            return ("file:///" + Path.GetFullPathInternal(codebase));
        }

        [SecurityCritical]
        private void VerifyCodeBaseDiscovery(string codeBase)
        {
            if ((codeBase != null) && (string.Compare(codeBase, 0, "file:", 0, 5, StringComparison.OrdinalIgnoreCase) == 0))
            {
                URLString str = new URLString(codeBase, true);
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, str.GetFileName()).Demand();
            }
        }

        internal InternalCache Cache
        {
            get
            {
                InternalCache cachedData = this.m_cachedData;
                if (cachedData == null)
                {
                    cachedData = new InternalCache("Assembly");
                    this.m_cachedData = cachedData;
                    if (this.SyncRoot.GetType() != typeof(LoaderAllocator))
                    {
                        GC.ClearCache += new ClearCacheHandler(this.OnCacheClear);
                    }
                }
                return cachedData;
            }
        }

        public override string CodeBase
        {
            [SecuritySafeCritical]
            get
            {
                string codeBase = this.GetCodeBase(false);
                this.VerifyCodeBaseDiscovery(codeBase);
                return codeBase;
            }
        }

        public override MethodInfo EntryPoint
        {
            [SecuritySafeCritical]
            get
            {
                IRuntimeMethodInfo o = null;
                GetEntryPoint(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<IRuntimeMethodInfo>(ref o));
                if (o == null)
                {
                    return null;
                }
                return (MethodInfo) RuntimeType.GetMethodBase(o);
            }
        }

        public override System.Security.Policy.Evidence Evidence
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlEvidence=true)]
            get
            {
                return this.EvidenceNoDemand.Clone();
            }
        }

        internal System.Security.Policy.Evidence EvidenceNoDemand
        {
            [SecurityCritical]
            get
            {
                System.Security.Policy.Evidence o = null;
                GetEvidence(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<System.Security.Policy.Evidence>(ref o));
                return o;
            }
        }

        public override string FullName
        {
            [SecuritySafeCritical]
            get
            {
                string s = (string) this.Cache[CacheObjType.AssemblyName];
                if (s == null)
                {
                    GetFullName(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s));
                    if (s != null)
                    {
                        this.Cache[CacheObjType.AssemblyName] = s;
                    }
                }
                return s;
            }
        }

        public override bool GlobalAssemblyCache
        {
            [SecuritySafeCritical]
            get
            {
                return IsGlobalAssemblyCache(this.GetNativeHandle());
            }
        }

        public override long HostContext
        {
            [SecuritySafeCritical]
            get
            {
                return GetHostContext(this.GetNativeHandle());
            }
        }

        [ComVisible(false)]
        public override string ImageRuntimeVersion
        {
            [SecuritySafeCritical]
            get
            {
                string s = null;
                GetImageRuntimeVersion(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s));
                return s;
            }
        }

        public override bool IsDynamic
        {
            [SecuritySafeCritical]
            get
            {
                return FCallIsDynamic(this.GetNativeHandle());
            }
        }

        internal bool IsStrongNameVerified
        {
            [SecurityCritical]
            get
            {
                return GetIsStrongNameVerified(this.GetNativeHandle());
            }
        }

        public override string Location
        {
            [SecuritySafeCritical]
            get
            {
                string s = null;
                GetLocation(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s));
                if (s != null)
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, s).Demand();
                }
                return s;
            }
        }

        public override Module ManifestModule
        {
            get
            {
                return GetManifestModule(this.GetNativeHandle());
            }
        }

        public override System.Security.PermissionSet PermissionSet
        {
            [SecurityCritical]
            get
            {
                System.Security.PermissionSet newGrant = null;
                System.Security.PermissionSet newDenied = null;
                this.GetGrantSet(out newGrant, out newDenied);
                if (newGrant != null)
                {
                    return newGrant.Copy();
                }
                return new System.Security.PermissionSet(PermissionState.Unrestricted);
            }
        }

        [ComVisible(false)]
        public override bool ReflectionOnly
        {
            [SecuritySafeCritical]
            get
            {
                return IsReflectionOnly(this.GetNativeHandle());
            }
        }

        public override System.Security.SecurityRuleSet SecurityRuleSet
        {
            [SecuritySafeCritical]
            get
            {
                return GetSecurityRuleSet(this.GetNativeHandle());
            }
        }

        internal object SyncRoot
        {
            get
            {
                if (this.m_syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref this.m_syncRoot, new object(), null);
                }
                return this.m_syncRoot;
            }
        }
    }
}


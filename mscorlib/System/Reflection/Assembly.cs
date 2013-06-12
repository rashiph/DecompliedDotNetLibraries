namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    [Serializable, ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_Assembly)), ComVisible(true), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public abstract class Assembly : _Assembly, IEvidenceFactory, ICustomAttributeProvider, ISerializable
    {
        public event ModuleResolveEventHandler ModuleResolve
        {
            [SecurityCritical] add
            {
                throw new NotImplementedException();
            }
            [SecurityCritical] remove
            {
                throw new NotImplementedException();
            }
        }

        protected Assembly()
        {
        }

        public object CreateInstance(string typeName)
        {
            return this.CreateInstance(typeName, false, BindingFlags.Public | BindingFlags.Instance, null, null, null, null);
        }

        public object CreateInstance(string typeName, bool ignoreCase)
        {
            return this.CreateInstance(typeName, ignoreCase, BindingFlags.Public | BindingFlags.Instance, null, null, null, null);
        }

        public virtual object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            Type type = this.GetType(typeName, false, ignoreCase);
            if (type == null)
            {
                return null;
            }
            return Activator.CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
        }

        public static string CreateQualifiedName(string assemblyName, string typeName)
        {
            return (typeName + ", " + assemblyName);
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public static Assembly GetAssembly(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Module module = type.Module;
            if (module == null)
            {
                return null;
            }
            return module.Assembly;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly GetCallingAssembly()
        {
            StackCrawlMark lookForMyCallersCaller = StackCrawlMark.LookForMyCallersCaller;
            return RuntimeAssembly.GetExecutingAssembly(ref lookForMyCallersCaller);
        }

        public virtual object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public virtual IList<CustomAttributeData> GetCustomAttributesData()
        {
            throw new NotImplementedException();
        }

        [SecuritySafeCritical]
        public static Assembly GetEntryAssembly()
        {
            AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
            if (domainManager == null)
            {
                domainManager = new AppDomainManager();
            }
            return domainManager.EntryAssembly;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly GetExecutingAssembly()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.GetExecutingAssembly(ref lookForMyCaller);
        }

        public virtual Type[] GetExportedTypes()
        {
            throw new NotImplementedException();
        }

        public virtual FileStream GetFile(string name)
        {
            throw new NotImplementedException();
        }

        public virtual FileStream[] GetFiles()
        {
            return this.GetFiles(false);
        }

        public virtual FileStream[] GetFiles(bool getResourceModules)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Module[] GetLoadedModules()
        {
            return this.GetLoadedModules(false);
        }

        public virtual Module[] GetLoadedModules(bool getResourceModules)
        {
            throw new NotImplementedException();
        }

        public virtual ManifestResourceInfo GetManifestResourceInfo(string resourceName)
        {
            throw new NotImplementedException();
        }

        public virtual string[] GetManifestResourceNames()
        {
            throw new NotImplementedException();
        }

        public virtual Stream GetManifestResourceStream(string name)
        {
            throw new NotImplementedException();
        }

        public virtual Stream GetManifestResourceStream(Type type, string name)
        {
            throw new NotImplementedException();
        }

        public virtual Module GetModule(string name)
        {
            throw new NotImplementedException();
        }

        public Module[] GetModules()
        {
            return this.GetModules(false);
        }

        public virtual Module[] GetModules(bool getResourceModules)
        {
            throw new NotImplementedException();
        }

        public virtual AssemblyName GetName()
        {
            return this.GetName(false);
        }

        public virtual AssemblyName GetName(bool copiedName)
        {
            throw new NotImplementedException();
        }

        [SecurityCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private string GetNameForConditionalAptca()
        {
            return this.GetName().GetNameWithPublicKey();
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public virtual AssemblyName[] GetReferencedAssemblies()
        {
            throw new NotImplementedException();
        }

        public virtual Assembly GetSatelliteAssembly(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public virtual Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
        {
            throw new NotImplementedException();
        }

        public virtual Type GetType(string name)
        {
            return this.GetType(name, false, false);
        }

        public virtual Type GetType(string name, bool throwOnError)
        {
            return this.GetType(name, throwOnError, false);
        }

        public virtual Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public virtual Type[] GetTypes()
        {
            Module[] modules = this.GetModules(false);
            int length = modules.Length;
            int num2 = 0;
            Type[][] typeArray = new Type[length][];
            for (int i = 0; i < length; i++)
            {
                typeArray[i] = modules[i].GetTypes();
                num2 += typeArray[i].Length;
            }
            int destinationIndex = 0;
            Type[] destinationArray = new Type[num2];
            for (int j = 0; j < length; j++)
            {
                int num6 = typeArray[j].Length;
                Array.Copy(typeArray[j], 0, destinationArray, destinationIndex, num6);
                destinationIndex += num6;
            }
            return destinationArray;
        }

        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly Load(AssemblyName assemblyRef)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadAssemblyName(assemblyRef, null, ref lookForMyCaller, false, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly Load(string assemblyString)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoad(assemblyString, null, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly Load(byte[] rawAssembly)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, null, null, ref lookForMyCaller, false, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static Assembly Load(AssemblyName assemblyRef, System.Security.Policy.Evidence assemblySecurity)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadAssemblyName(assemblyRef, assemblySecurity, ref lookForMyCaller, false, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public static Assembly Load(string assemblyString, System.Security.Policy.Evidence assemblySecurity)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoad(assemblyString, assemblySecurity, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, rawSymbolStore, null, ref lookForMyCaller, false, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of Load which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlEvidence)]
        public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, System.Security.Policy.Evidence securityEvidence)
        {
            if ((securityEvidence != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                Zone hostEvidence = securityEvidence.GetHostEvidence<Zone>();
                if ((hostEvidence == null) || (hostEvidence.SecurityZone != SecurityZone.MyComputer))
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
                }
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, rawSymbolStore, securityEvidence, ref lookForMyCaller, false, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, SecurityContextSource securityContextSource)
        {
            if ((securityContextSource < SecurityContextSource.CurrentAppDomain) || (securityContextSource > SecurityContextSource.CurrentAssembly))
            {
                throw new ArgumentOutOfRangeException("securityContextSource");
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, rawSymbolStore, null, ref lookForMyCaller, false, securityContextSource);
        }

        [SecuritySafeCritical]
        public static Assembly LoadFile(string path)
        {
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, path).Demand();
            return RuntimeAssembly.nLoadFile(path, null);
        }

        [Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of LoadFile which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlEvidence)]
        public static Assembly LoadFile(string path, System.Security.Policy.Evidence securityEvidence)
        {
            if ((securityEvidence != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyImplicit"));
            }
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, path).Demand();
            return RuntimeAssembly.nLoadFile(path, securityEvidence);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly LoadFrom(string assemblyFile)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadFrom(assemblyFile, null, null, AssemblyHashAlgorithm.None, false, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of LoadFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static Assembly LoadFrom(string assemblyFile, System.Security.Policy.Evidence securityEvidence)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadFrom(assemblyFile, securityEvidence, null, AssemblyHashAlgorithm.None, false, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly LoadFrom(string assemblyFile, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadFrom(assemblyFile, null, hashValue, hashAlgorithm, false, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. Please use an overload of LoadFrom which does not take an Evidence parameter. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static Assembly LoadFrom(string assemblyFile, System.Security.Policy.Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadFrom(assemblyFile, securityEvidence, hashValue, hashAlgorithm, false, false, ref lookForMyCaller);
        }

        public Module LoadModule(string moduleName, byte[] rawModule)
        {
            return this.LoadModule(moduleName, rawModule, null);
        }

        public virtual Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecuritySafeCritical]
        public static Assembly LoadWithPartialName(string partialName)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.LoadWithPartialNameInternal(partialName, null, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static Assembly LoadWithPartialName(string partialName, System.Security.Policy.Evidence securityEvidence)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.LoadWithPartialNameInternal(partialName, securityEvidence, ref lookForMyCaller);
        }

        public static bool operator ==(Assembly left, Assembly right)
        {
            return (object.ReferenceEquals(left, right) || ((((left != null) && (right != null)) && (!(left is RuntimeAssembly) && !(right is RuntimeAssembly))) && left.Equals(right)));
        }

        public static bool operator !=(Assembly left, Assembly right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly ReflectionOnlyLoad(string assemblyString)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoad(assemblyString, null, ref lookForMyCaller, true);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly ReflectionOnlyLoad(byte[] rawAssembly)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.nLoadImage(rawAssembly, null, null, ref lookForMyCaller, true, SecurityContextSource.CurrentAssembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static Assembly ReflectionOnlyLoadFrom(string assemblyFile)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadFrom(assemblyFile, null, null, AssemblyHashAlgorithm.None, true, false, ref lookForMyCaller);
        }

        Type _Assembly.GetType()
        {
            return base.GetType();
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

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        public static Assembly UnsafeLoadFrom(string assemblyFile)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RuntimeAssembly.InternalLoadFrom(assemblyFile, null, null, AssemblyHashAlgorithm.None, false, true, ref lookForMyCaller);
        }

        public virtual string CodeBase
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual MethodInfo EntryPoint
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual string EscapedCodeBase
        {
            [SecuritySafeCritical]
            get
            {
                return AssemblyName.EscapeCodeBase(this.CodeBase);
            }
        }

        public virtual System.Security.Policy.Evidence Evidence
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual string FullName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool GlobalAssemblyCache
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ComVisible(false)]
        public virtual long HostContext
        {
            get
            {
                RuntimeAssembly assembly = this as RuntimeAssembly;
                if (assembly == null)
                {
                    throw new NotImplementedException();
                }
                return assembly.HostContext;
            }
        }

        [ComVisible(false)]
        public virtual string ImageRuntimeVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsDynamic
        {
            get
            {
                return false;
            }
        }

        public bool IsFullyTrusted
        {
            [SecuritySafeCritical]
            get
            {
                return this.PermissionSet.IsUnrestricted();
            }
        }

        public virtual string Location
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ComVisible(false)]
        public virtual Module ManifestModule
        {
            get
            {
                RuntimeAssembly assembly = this as RuntimeAssembly;
                if (assembly == null)
                {
                    throw new NotImplementedException();
                }
                return assembly.ManifestModule;
            }
        }

        public virtual System.Security.PermissionSet PermissionSet
        {
            [SecurityCritical]
            get
            {
                throw new NotImplementedException();
            }
        }

        [ComVisible(false)]
        public virtual bool ReflectionOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual System.Security.SecurityRuleSet SecurityRuleSet
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}


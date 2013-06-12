namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.SymbolStore;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    [ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_AssemblyBuilder)), ComVisible(true), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class AssemblyBuilder : Assembly, _AssemblyBuilder
    {
        internal AssemblyBuilderData m_assemblyData;
        private bool m_fManifestModuleUsedAsDefinedModule;
        private InternalAssemblyBuilder m_internalAssemblyBuilder;
        private ModuleBuilder m_manifestModuleBuilder;
        private ModuleBuilder m_onDiskAssemblyModuleBuilder;
        internal const string MANIFEST_MODULE_NAME = "RefEmit_InMemoryManifestModule";

        private AssemblyBuilder()
        {
        }

        [SecurityCritical]
        internal AssemblyBuilder(AppDomain domain, AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.Policy.Evidence evidence, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions, ref StackCrawlMark stackMark, IEnumerable<CustomAttributeBuilder> unsafeAssemblyAttributes, SecurityContextSource securityContextSource)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if ((((access != AssemblyBuilderAccess.Run) && (access != AssemblyBuilderAccess.Save)) && ((access != AssemblyBuilderAccess.RunAndSave) && (access != AssemblyBuilderAccess.ReflectionOnly))) && (access != AssemblyBuilderAccess.RunAndCollect))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) access }), "access");
            }
            if ((securityContextSource < SecurityContextSource.CurrentAppDomain) || (securityContextSource > SecurityContextSource.CurrentAssembly))
            {
                throw new ArgumentOutOfRangeException("securityContextSource");
            }
            if (name.KeyPair != null)
            {
                name.SetPublicKey(name.KeyPair.PublicKey);
            }
            if (evidence != null)
            {
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
            }
            if (access == AssemblyBuilderAccess.RunAndCollect)
            {
                new System.Security.PermissionSet(PermissionState.Unrestricted).Demand();
            }
            List<CustomAttributeBuilder> list = null;
            DynamicAssemblyFlags none = DynamicAssemblyFlags.None;
            byte[] destinationArray = null;
            byte[] buffer2 = null;
            if (unsafeAssemblyAttributes != null)
            {
                list = new List<CustomAttributeBuilder>(unsafeAssemblyAttributes);
                foreach (CustomAttributeBuilder builder in list)
                {
                    if (builder.m_con.DeclaringType == typeof(SecurityTransparentAttribute))
                    {
                        none |= DynamicAssemblyFlags.Transparent;
                    }
                    else if (builder.m_con.DeclaringType == typeof(SecurityCriticalAttribute))
                    {
                        SecurityCriticalScope everything = SecurityCriticalScope.Everything;
                        if (((builder.m_constructorArgs != null) && (builder.m_constructorArgs.Length == 1)) && (builder.m_constructorArgs[0] is SecurityCriticalScope))
                        {
                            everything = (SecurityCriticalScope) builder.m_constructorArgs[0];
                        }
                        none |= DynamicAssemblyFlags.Critical;
                        if (everything == SecurityCriticalScope.Everything)
                        {
                            none |= DynamicAssemblyFlags.AllCritical;
                        }
                    }
                    else if (builder.m_con.DeclaringType == typeof(SecurityRulesAttribute))
                    {
                        destinationArray = new byte[builder.m_blob.Length];
                        Array.Copy(builder.m_blob, destinationArray, destinationArray.Length);
                    }
                    else if (builder.m_con.DeclaringType == typeof(SecurityTreatAsSafeAttribute))
                    {
                        none |= DynamicAssemblyFlags.TreatAsSafe;
                    }
                    else if (builder.m_con.DeclaringType == typeof(AllowPartiallyTrustedCallersAttribute))
                    {
                        none |= DynamicAssemblyFlags.Aptca;
                        buffer2 = new byte[builder.m_blob.Length];
                        Array.Copy(builder.m_blob, buffer2, buffer2.Length);
                    }
                }
            }
            this.m_internalAssemblyBuilder = (InternalAssemblyBuilder) nCreateDynamicAssembly(domain, name, evidence, ref stackMark, requiredPermissions, optionalPermissions, refusedPermissions, destinationArray, buffer2, access, none, securityContextSource);
            this.m_assemblyData = new AssemblyBuilderData(this.m_internalAssemblyBuilder, name.Name, access, dir);
            this.m_assemblyData.AddPermissionRequests(requiredPermissions, optionalPermissions, refusedPermissions);
            this.InitManifestModule();
            if (list != null)
            {
                foreach (CustomAttributeBuilder builder2 in list)
                {
                    this.SetCustomAttribute(builder2);
                }
            }
        }

        [SecurityCritical]
        private void AddDeclarativeSecurity(System.Security.PermissionSet pset, SecurityAction action)
        {
            byte[] blob = pset.EncodeXml();
            AddDeclarativeSecurity(this.GetNativeHandle(), action, blob, blob.Length);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddDeclarativeSecurity(RuntimeAssembly assembly, SecurityAction action, byte[] blob, int length);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int AddExportedTypeInMemory(RuntimeAssembly assembly, string strComTypeName, int tkAssemblyRef, int tkTypeDef, TypeAttributes flags);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int AddExportedTypeOnDisk(RuntimeAssembly assembly, string strComTypeName, int tkAssemblyRef, int tkTypeDef, TypeAttributes flags);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int AddFile(RuntimeAssembly assembly, string strFileName);
        [SecuritySafeCritical]
        public void AddResourceFile(string name, string fileName)
        {
            this.AddResourceFile(name, fileName, ResourceAttributes.Public);
        }

        [SecuritySafeCritical]
        public void AddResourceFile(string name, string fileName, ResourceAttributes attribute)
        {
            lock (this.SyncRoot)
            {
                this.AddResourceFileNoLock(name, fileName, attribute);
            }
        }

        private void AddResourceFileNoLock(string name, string fileName, ResourceAttributes attribute)
        {
            string fullPath;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), name);
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), fileName);
            }
            if (!string.Equals(fileName, Path.GetFileName(fileName)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "fileName");
            }
            this.m_assemblyData.CheckResNameConflict(name);
            this.m_assemblyData.CheckFileNameConflict(fileName);
            if (this.m_assemblyData.m_strDir == null)
            {
                fullPath = Path.Combine(Environment.CurrentDirectory, fileName);
            }
            else
            {
                fullPath = Path.Combine(this.m_assemblyData.m_strDir, fileName);
            }
            fullPath = Path.GetFullPath(fullPath);
            fileName = Path.GetFileName(fullPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(Environment.GetResourceString("IO.FileNotFound_FileName", new object[] { fileName }), fileName);
            }
            this.m_assemblyData.AddResWriter(new ResWriterData(null, null, name, fileName, fullPath, attribute));
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddStandAloneResource(RuntimeAssembly assembly, string strName, string strFileName, string strFullFileName, int attribute);
        internal void CheckContext(params Type[][] typess)
        {
            if (typess != null)
            {
                foreach (Type[] typeArray in typess)
                {
                    if (typeArray != null)
                    {
                        this.CheckContext(typeArray);
                    }
                }
            }
        }

        internal void CheckContext(params Type[] types)
        {
            if (types != null)
            {
                foreach (Type type in types)
                {
                    if (type != null)
                    {
                        if ((type.Module == null) || (type.Module.Assembly == null))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_TypeNotValid"));
                        }
                        if (type.Module.Assembly != typeof(object).Module.Assembly)
                        {
                            if (type.Module.Assembly.ReflectionOnly && !this.ReflectionOnly)
                            {
                                throw new InvalidOperationException(Environment.GetResourceString("Arugment_EmitMixedContext1", new object[] { type.AssemblyQualifiedName }));
                            }
                            if (!type.Module.Assembly.ReflectionOnly && this.ReflectionOnly)
                            {
                                throw new InvalidOperationException(Environment.GetResourceString("Arugment_EmitMixedContext2", new object[] { type.AssemblyQualifiedName }));
                            }
                        }
                    }
                }
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void CreateVersionInfoResource(string filename, string title, string iconFilename, string description, string copyright, string trademark, string company, string product, string productVersion, string fileVersion, int lcid, bool isDll, StringHandleOnStack retFileName);
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ModuleBuilder DefineDynamicModule(string name)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.DefineDynamicModuleInternal(name, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ModuleBuilder DefineDynamicModule(string name, bool emitSymbolInfo)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.DefineDynamicModuleInternal(name, emitSymbolInfo, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ModuleBuilder DefineDynamicModule(string name, string fileName)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.DefineDynamicModuleInternal(name, fileName, false, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ModuleBuilder DefineDynamicModule(string name, string fileName, bool emitSymbolInfo)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.DefineDynamicModuleInternal(name, fileName, emitSymbolInfo, ref lookForMyCaller);
        }

        [SecurityCritical]
        private static Module DefineDynamicModule(RuntimeAssembly containingAssembly, bool emitSymbolInfo, string name, string filename, ref StackCrawlMark stackMark, ref IntPtr pInternalSymWriter, bool fIsTransient, out int tkFile)
        {
            RuntimeModule o = null;
            DefineDynamicModule(containingAssembly.GetNativeHandle(), emitSymbolInfo, name, filename, JitHelpers.GetStackCrawlMarkHandle(ref stackMark), ref pInternalSymWriter, JitHelpers.GetObjectHandleOnStack<RuntimeModule>(ref o), fIsTransient, out tkFile);
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void DefineDynamicModule(RuntimeAssembly containingAssembly, bool emitSymbolInfo, string name, string filename, StackCrawlMarkHandle stackMark, ref IntPtr pInternalSymWriter, ObjectHandleOnStack retModule, bool fIsTransient, out int tkFile);
        [SecurityCritical]
        private ModuleBuilder DefineDynamicModuleInternal(string name, bool emitSymbolInfo, ref StackCrawlMark stackMark)
        {
            lock (this.SyncRoot)
            {
                return this.DefineDynamicModuleInternalNoLock(name, emitSymbolInfo, ref stackMark);
            }
        }

        [SecurityCritical]
        private ModuleBuilder DefineDynamicModuleInternal(string name, string fileName, bool emitSymbolInfo, ref StackCrawlMark stackMark)
        {
            lock (this.SyncRoot)
            {
                return this.DefineDynamicModuleInternalNoLock(name, fileName, emitSymbolInfo, ref stackMark);
            }
        }

        [SecurityCritical]
        private ModuleBuilder DefineDynamicModuleInternalNoLock(string name, bool emitSymbolInfo, ref StackCrawlMark stackMark)
        {
            ModuleBuilder manifestModuleBuilder;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (name[0] == '\0')
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidName"), "name");
            }
            ISymbolWriter writer = null;
            IntPtr pInternalSymWriter = new IntPtr();
            this.m_assemblyData.CheckNameConflict(name);
            if (this.m_fManifestModuleUsedAsDefinedModule)
            {
                int num;
                InternalModuleBuilder internalModuleBuilder = (InternalModuleBuilder) DefineDynamicModule(this.InternalAssembly, emitSymbolInfo, name, name, ref stackMark, ref pInternalSymWriter, true, out num);
                manifestModuleBuilder = new ModuleBuilder(this, internalModuleBuilder);
                manifestModuleBuilder.Init(name, null, num);
            }
            else
            {
                this.m_manifestModuleBuilder.ModifyModuleName(name);
                manifestModuleBuilder = this.m_manifestModuleBuilder;
                if (emitSymbolInfo)
                {
                    pInternalSymWriter = ModuleBuilder.nCreateISymWriterForDynamicModule(manifestModuleBuilder.InternalModule, name);
                }
            }
            if (emitSymbolInfo)
            {
                Type type = this.LoadISymWrapper().GetType("System.Diagnostics.SymbolStore.SymWriter", true, false);
                if ((type != null) && !type.IsVisible)
                {
                    type = null;
                }
                if (type == null)
                {
                    throw new TypeLoadException(Environment.GetResourceString("MissingType", new object[] { "SymWriter" }));
                }
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                try
                {
                    new System.Security.PermissionSet(PermissionState.Unrestricted).Assert();
                    writer = (ISymbolWriter) Activator.CreateInstance(type);
                    writer.SetUnderlyingWriter(pInternalSymWriter);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            manifestModuleBuilder.SetSymWriter(writer);
            this.m_assemblyData.AddModule(manifestModuleBuilder);
            if (manifestModuleBuilder == this.m_manifestModuleBuilder)
            {
                this.m_fManifestModuleUsedAsDefinedModule = true;
            }
            return manifestModuleBuilder;
        }

        [SecurityCritical]
        private ModuleBuilder DefineDynamicModuleInternalNoLock(string name, string fileName, bool emitSymbolInfo, ref StackCrawlMark stackMark)
        {
            int num;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (name[0] == '\0')
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidName"), "name");
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "fileName");
            }
            if (!string.Equals(fileName, Path.GetFileName(fileName)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "fileName");
            }
            if (this.m_assemblyData.m_access == AssemblyBuilderAccess.Run)
            {
                throw new NotSupportedException(Environment.GetResourceString("Argument_BadPersistableModuleInTransientAssembly"));
            }
            if (this.m_assemblyData.m_isSaved)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotAlterAssembly"));
            }
            ISymbolWriter writer = null;
            IntPtr pInternalSymWriter = new IntPtr();
            this.m_assemblyData.CheckNameConflict(name);
            this.m_assemblyData.CheckFileNameConflict(fileName);
            InternalModuleBuilder internalModuleBuilder = (InternalModuleBuilder) DefineDynamicModule(this.InternalAssembly, emitSymbolInfo, name, fileName, ref stackMark, ref pInternalSymWriter, false, out num);
            ModuleBuilder dynModule = new ModuleBuilder(this, internalModuleBuilder);
            dynModule.Init(name, fileName, num);
            if (emitSymbolInfo)
            {
                Type type = this.LoadISymWrapper().GetType("System.Diagnostics.SymbolStore.SymWriter", true, false);
                if ((type != null) && !type.IsVisible)
                {
                    type = null;
                }
                if (type == null)
                {
                    throw new TypeLoadException(Environment.GetResourceString("MissingType", new object[] { "SymWriter" }));
                }
                try
                {
                    new System.Security.PermissionSet(PermissionState.Unrestricted).Assert();
                    writer = (ISymbolWriter) Activator.CreateInstance(type);
                    writer.SetUnderlyingWriter(pInternalSymWriter);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            dynModule.SetSymWriter(writer);
            this.m_assemblyData.AddModule(dynModule);
            return dynModule;
        }

        [SecurityCritical]
        internal int DefineExportedTypeInMemory(Type type, int tkResolutionScope, int tkTypeDef)
        {
            Type declaringType = type.DeclaringType;
            if (declaringType == null)
            {
                return AddExportedTypeInMemory(this.GetNativeHandle(), type.FullName, tkResolutionScope, tkTypeDef, type.Attributes);
            }
            tkResolutionScope = this.DefineExportedTypeInMemory(declaringType, tkResolutionScope, tkTypeDef);
            return AddExportedTypeInMemory(this.GetNativeHandle(), type.Name, tkResolutionScope, tkTypeDef, type.Attributes);
        }

        [SecurityCritical]
        private int DefineNestedComType(Type type, int tkResolutionScope, int tkTypeDef)
        {
            Type declaringType = type.DeclaringType;
            if (declaringType == null)
            {
                return AddExportedTypeOnDisk(this.GetNativeHandle(), type.FullName, tkResolutionScope, tkTypeDef, type.Attributes);
            }
            tkResolutionScope = this.DefineNestedComType(declaringType, tkResolutionScope, tkTypeDef);
            return AddExportedTypeOnDisk(this.GetNativeHandle(), type.Name, tkResolutionScope, tkTypeDef, type.Attributes);
        }

        [SecuritySafeCritical]
        public IResourceWriter DefineResource(string name, string description, string fileName)
        {
            return this.DefineResource(name, description, fileName, ResourceAttributes.Public);
        }

        [SecuritySafeCritical]
        public IResourceWriter DefineResource(string name, string description, string fileName, ResourceAttributes attribute)
        {
            lock (this.SyncRoot)
            {
                return this.DefineResourceNoLock(name, description, fileName, attribute);
            }
        }

        private IResourceWriter DefineResourceNoLock(string name, string description, string fileName, ResourceAttributes attribute)
        {
            ResourceWriter writer;
            string fullPath;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), name);
            }
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "fileName");
            }
            if (!string.Equals(fileName, Path.GetFileName(fileName)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "fileName");
            }
            this.m_assemblyData.CheckResNameConflict(name);
            this.m_assemblyData.CheckFileNameConflict(fileName);
            if (this.m_assemblyData.m_strDir == null)
            {
                fullPath = Path.Combine(Environment.CurrentDirectory, fileName);
                writer = new ResourceWriter(fullPath);
            }
            else
            {
                fullPath = Path.Combine(this.m_assemblyData.m_strDir, fileName);
                writer = new ResourceWriter(fullPath);
            }
            fullPath = Path.GetFullPath(fullPath);
            fileName = Path.GetFileName(fullPath);
            this.m_assemblyData.AddResWriter(new ResWriterData(writer, null, name, fileName, fullPath, attribute));
            return writer;
        }

        public void DefineUnmanagedResource(byte[] resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            lock (this.SyncRoot)
            {
                this.DefineUnmanagedResourceNoLock(resource);
            }
        }

        [SecuritySafeCritical]
        public void DefineUnmanagedResource(string resourceFileName)
        {
            if (resourceFileName == null)
            {
                throw new ArgumentNullException("resourceFileName");
            }
            lock (this.SyncRoot)
            {
                this.DefineUnmanagedResourceNoLock(resourceFileName);
            }
        }

        private void DefineUnmanagedResourceNoLock(byte[] resource)
        {
            if (((this.m_assemblyData.m_strResourceFileName != null) || (this.m_assemblyData.m_resourceBytes != null)) || (this.m_assemblyData.m_nativeVersion != null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
            }
            this.m_assemblyData.m_resourceBytes = new byte[resource.Length];
            Array.Copy(resource, this.m_assemblyData.m_resourceBytes, resource.Length);
        }

        [SecurityCritical]
        private void DefineUnmanagedResourceNoLock(string resourceFileName)
        {
            string fullPath;
            if (((this.m_assemblyData.m_strResourceFileName != null) || (this.m_assemblyData.m_resourceBytes != null)) || (this.m_assemblyData.m_nativeVersion != null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
            }
            if (this.m_assemblyData.m_strDir == null)
            {
                fullPath = Path.Combine(Environment.CurrentDirectory, resourceFileName);
            }
            else
            {
                fullPath = Path.Combine(this.m_assemblyData.m_strDir, resourceFileName);
            }
            fullPath = Path.GetFullPath(resourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(Environment.GetResourceString("IO.FileNotFound_FileName", new object[] { resourceFileName }), resourceFileName);
            }
            this.m_assemblyData.m_strResourceFileName = fullPath;
        }

        public void DefineVersionInfoResource()
        {
            lock (this.SyncRoot)
            {
                this.DefineVersionInfoResourceNoLock();
            }
        }

        public void DefineVersionInfoResource(string product, string productVersion, string company, string copyright, string trademark)
        {
            lock (this.SyncRoot)
            {
                this.DefineVersionInfoResourceNoLock(product, productVersion, company, copyright, trademark);
            }
        }

        private void DefineVersionInfoResourceNoLock()
        {
            if (((this.m_assemblyData.m_strResourceFileName != null) || (this.m_assemblyData.m_resourceBytes != null)) || (this.m_assemblyData.m_nativeVersion != null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
            }
            this.m_assemblyData.m_hasUnmanagedVersionInfo = true;
            this.m_assemblyData.m_nativeVersion = new NativeVersionInfo();
        }

        private void DefineVersionInfoResourceNoLock(string product, string productVersion, string company, string copyright, string trademark)
        {
            if (((this.m_assemblyData.m_strResourceFileName != null) || (this.m_assemblyData.m_resourceBytes != null)) || (this.m_assemblyData.m_nativeVersion != null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
            }
            this.m_assemblyData.m_nativeVersion = new NativeVersionInfo();
            this.m_assemblyData.m_nativeVersion.m_strCopyright = copyright;
            this.m_assemblyData.m_nativeVersion.m_strTrademark = trademark;
            this.m_assemblyData.m_nativeVersion.m_strCompany = company;
            this.m_assemblyData.m_nativeVersion.m_strProduct = product;
            this.m_assemblyData.m_nativeVersion.m_strProductVersion = productVersion;
            this.m_assemblyData.m_hasUnmanagedVersionInfo = true;
            this.m_assemblyData.m_OverrideUnmanagedVersionInfo = true;
        }

        public override bool Equals(object obj)
        {
            return this.InternalAssembly.Equals(obj);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.InternalAssembly.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.InternalAssembly.GetCustomAttributes(attributeType, inherit);
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return this.InternalAssembly.GetCustomAttributesData();
        }

        public ModuleBuilder GetDynamicModule(string name)
        {
            lock (this.SyncRoot)
            {
                return this.GetDynamicModuleNoLock(name);
            }
        }

        private ModuleBuilder GetDynamicModuleNoLock(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            int count = this.m_assemblyData.m_moduleBuilderList.Count;
            for (int i = 0; i < count; i++)
            {
                ModuleBuilder builder = this.m_assemblyData.m_moduleBuilderList[i];
                if (builder.m_moduleData.m_strModuleName.Equals(name))
                {
                    return builder;
                }
            }
            return null;
        }

        public override Type[] GetExportedTypes()
        {
            return this.InternalAssembly.GetExportedTypes();
        }

        [SecuritySafeCritical]
        public override FileStream GetFile(string name)
        {
            return this.InternalAssembly.GetFile(name);
        }

        [SecuritySafeCritical]
        public override FileStream[] GetFiles(bool getResourceModules)
        {
            return this.InternalAssembly.GetFiles(getResourceModules);
        }

        public override int GetHashCode()
        {
            return this.InternalAssembly.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern RuntimeModule GetInMemoryAssemblyModule(RuntimeAssembly assembly);
        public override Module[] GetLoadedModules(bool getResourceModules)
        {
            return this.InternalAssembly.GetLoadedModules(getResourceModules);
        }

        public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
        {
            return this.InternalAssembly.GetManifestResourceInfo(resourceName);
        }

        public override string[] GetManifestResourceNames()
        {
            return this.InternalAssembly.GetManifestResourceNames();
        }

        public override Stream GetManifestResourceStream(string name)
        {
            return this.InternalAssembly.GetManifestResourceStream(name);
        }

        public override Stream GetManifestResourceStream(Type type, string name)
        {
            return this.InternalAssembly.GetManifestResourceStream(type, name);
        }

        [SecuritySafeCritical]
        public override Module GetModule(string name)
        {
            return this.InternalAssembly.GetModule(name);
        }

        internal ModuleBuilder GetModuleBuilder(InternalModuleBuilder module)
        {
            lock (this.SyncRoot)
            {
                foreach (ModuleBuilder builder in this.m_assemblyData.m_moduleBuilderList)
                {
                    if (builder.InternalModule == module)
                    {
                        return builder;
                    }
                }
                if ((this.m_onDiskAssemblyModuleBuilder != null) && (this.m_onDiskAssemblyModuleBuilder.InternalModule == module))
                {
                    return this.m_onDiskAssemblyModuleBuilder;
                }
                if (this.m_manifestModuleBuilder.InternalModule != module)
                {
                    throw new ArgumentException("module");
                }
                return this.m_manifestModuleBuilder;
            }
        }

        public override Module[] GetModules(bool getResourceModules)
        {
            return this.InternalAssembly.GetModules(getResourceModules);
        }

        public override AssemblyName GetName(bool copiedName)
        {
            return this.InternalAssembly.GetName(copiedName);
        }

        internal RuntimeAssembly GetNativeHandle()
        {
            return this.InternalAssembly.GetNativeHandle();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern RuntimeModule GetOnDiskAssemblyModule(RuntimeAssembly assembly);
        [SecurityCritical]
        private ModuleBuilder GetOnDiskAssemblyModuleBuilder()
        {
            if (this.m_onDiskAssemblyModuleBuilder == null)
            {
                Module onDiskAssemblyModule = GetOnDiskAssemblyModule(this.InternalAssembly.GetNativeHandle());
                ModuleBuilder builder = new ModuleBuilder(this, (InternalModuleBuilder) onDiskAssemblyModule);
                builder.Init("RefEmit_OnDiskManifestModule", null, 0);
                this.m_onDiskAssemblyModuleBuilder = builder;
            }
            return this.m_onDiskAssemblyModuleBuilder;
        }

        public override AssemblyName[] GetReferencedAssemblies()
        {
            return this.InternalAssembly.GetReferencedAssemblies();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override Assembly GetSatelliteAssembly(CultureInfo culture)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalAssembly.InternalGetSatelliteAssembly(culture, null, ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalAssembly.InternalGetSatelliteAssembly(culture, version, ref lookForMyCaller);
        }

        public override Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            return this.InternalAssembly.GetType(name, throwOnError, ignoreCase);
        }

        [SecurityCritical]
        internal Version GetVersion()
        {
            return this.InternalAssembly.GetVersion();
        }

        [SecurityCritical]
        private void InitManifestModule()
        {
            InternalModuleBuilder internalModuleBuilder = (InternalModuleBuilder) this.nGetInMemoryAssemblyModule();
            this.m_manifestModuleBuilder = new ModuleBuilder(this, internalModuleBuilder);
            this.m_manifestModuleBuilder.Init("RefEmit_InMemoryManifestModule", null, 0);
            this.m_fManifestModuleUsedAsDefinedModule = false;
        }

        [SecurityCritical]
        internal static AssemblyBuilder InternalDefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, System.Security.Policy.Evidence evidence, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions, ref StackCrawlMark stackMark, IEnumerable<CustomAttributeBuilder> unsafeAssemblyAttributes, SecurityContextSource securityContextSource)
        {
            if ((evidence != null) && !AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            lock (typeof(AssemblyBuilderLock))
            {
                return new AssemblyBuilder(AppDomain.CurrentDomain, name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, unsafeAssemblyAttributes, securityContextSource);
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.InternalAssembly.IsDefined(attributeType, inherit);
        }

        internal bool IsPersistable()
        {
            return ((this.m_assemblyData.m_access & AssemblyBuilderAccess.Save) == AssemblyBuilderAccess.Save);
        }

        private Assembly LoadISymWrapper()
        {
            if (this.m_assemblyData.m_ISymWrapperAssembly != null)
            {
                return this.m_assemblyData.m_ISymWrapperAssembly;
            }
            Assembly assembly = Assembly.Load("ISymWrapper, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            this.m_assemblyData.m_ISymWrapperAssembly = assembly;
            return assembly;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern Assembly nCreateDynamicAssembly(AppDomain domain, AssemblyName name, System.Security.Policy.Evidence identity, ref StackCrawlMark stackMark, System.Security.PermissionSet requiredPermissions, System.Security.PermissionSet optionalPermissions, System.Security.PermissionSet refusedPermissions, byte[] securityRulesBlob, byte[] aptcaBlob, AssemblyBuilderAccess access, DynamicAssemblyFlags flags, SecurityContextSource securityContextSource);
        [SecurityCritical]
        private Module nGetInMemoryAssemblyModule()
        {
            return GetInMemoryAssemblyModule(this.GetNativeHandle());
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void PrepareForSavingManifestToDisk(RuntimeAssembly assembly, RuntimeModule assemblyModule);
        [SecuritySafeCritical]
        public void Save(string assemblyFileName)
        {
            this.Save(assemblyFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
        }

        [SecuritySafeCritical]
        public void Save(string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
        {
            lock (this.SyncRoot)
            {
                this.SaveNoLock(assemblyFileName, portableExecutableKind, imageFileMachine);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SaveManifestToDisk(RuntimeAssembly assembly, string strFileName, int entryPoint, int fileKind, int portableExecutableKind, int ImageFileMachine);
        [SecurityCritical]
        private void SaveNoLock(string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
        {
            if (assemblyFileName == null)
            {
                throw new ArgumentNullException("assemblyFileName");
            }
            if (assemblyFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "assemblyFileName");
            }
            if (!string.Equals(assemblyFileName, Path.GetFileName(assemblyFileName)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "assemblyFileName");
            }
            int[] numArray = null;
            int[] numArray2 = null;
            string s = null;
            try
            {
                int num;
                if (this.m_assemblyData.m_iCABuilder != 0)
                {
                    numArray = new int[this.m_assemblyData.m_iCABuilder];
                }
                if (this.m_assemblyData.m_iCAs != 0)
                {
                    numArray2 = new int[this.m_assemblyData.m_iCAs];
                }
                if (this.m_assemblyData.m_isSaved)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AssemblyHasBeenSaved", new object[] { this.InternalAssembly.GetSimpleName() }));
                }
                if ((this.m_assemblyData.m_access & AssemblyBuilderAccess.Save) != AssemblyBuilderAccess.Save)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CantSaveTransientAssembly"));
                }
                ModuleBuilder mod = this.m_assemblyData.FindModuleWithFileName(assemblyFileName);
                if (mod != null)
                {
                    this.m_onDiskAssemblyModuleBuilder = mod;
                    mod.m_moduleData.FileToken = 0;
                }
                else
                {
                    this.m_assemblyData.CheckFileNameConflict(assemblyFileName);
                }
                if (this.m_assemblyData.m_strDir == null)
                {
                    this.m_assemblyData.m_strDir = Environment.CurrentDirectory;
                }
                else if (!Directory.Exists(this.m_assemblyData.m_strDir))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDirectory", new object[] { this.m_assemblyData.m_strDir }));
                }
                assemblyFileName = Path.Combine(this.m_assemblyData.m_strDir, assemblyFileName);
                assemblyFileName = Path.GetFullPath(assemblyFileName);
                new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Write, assemblyFileName).Demand();
                if (mod != null)
                {
                    for (num = 0; num < this.m_assemblyData.m_iCABuilder; num++)
                    {
                        numArray[num] = this.m_assemblyData.m_CABuilders[num].PrepareCreateCustomAttributeToDisk(mod);
                    }
                    for (num = 0; num < this.m_assemblyData.m_iCAs; num++)
                    {
                        numArray2[num] = mod.InternalGetConstructorToken(this.m_assemblyData.m_CACons[num], true).Token;
                    }
                    mod.PreSave(assemblyFileName, portableExecutableKind, imageFileMachine);
                }
                RuntimeModule assemblyModule = (mod != null) ? mod.ModuleHandle.GetRuntimeModule() : null;
                PrepareForSavingManifestToDisk(this.GetNativeHandle(), assemblyModule);
                ModuleBuilder onDiskAssemblyModuleBuilder = this.GetOnDiskAssemblyModuleBuilder();
                if (this.m_assemblyData.m_strResourceFileName != null)
                {
                    onDiskAssemblyModuleBuilder.DefineUnmanagedResourceFileInternalNoLock(this.m_assemblyData.m_strResourceFileName);
                }
                else if (this.m_assemblyData.m_resourceBytes != null)
                {
                    onDiskAssemblyModuleBuilder.DefineUnmanagedResourceInternalNoLock(this.m_assemblyData.m_resourceBytes);
                }
                else if (this.m_assemblyData.m_hasUnmanagedVersionInfo)
                {
                    this.m_assemblyData.FillUnmanagedVersionInfo();
                    string strFileVersion = this.m_assemblyData.m_nativeVersion.m_strFileVersion;
                    if (strFileVersion == null)
                    {
                        strFileVersion = this.GetVersion().ToString();
                    }
                    CreateVersionInfoResource(assemblyFileName, this.m_assemblyData.m_nativeVersion.m_strTitle, null, this.m_assemblyData.m_nativeVersion.m_strDescription, this.m_assemblyData.m_nativeVersion.m_strCopyright, this.m_assemblyData.m_nativeVersion.m_strTrademark, this.m_assemblyData.m_nativeVersion.m_strCompany, this.m_assemblyData.m_nativeVersion.m_strProduct, this.m_assemblyData.m_nativeVersion.m_strProductVersion, strFileVersion, this.m_assemblyData.m_nativeVersion.m_lcid, this.m_assemblyData.m_peFileKind == PEFileKinds.Dll, JitHelpers.GetStringHandleOnStack(ref s));
                    onDiskAssemblyModuleBuilder.DefineUnmanagedResourceFileInternalNoLock(s);
                }
                if (mod == null)
                {
                    for (num = 0; num < this.m_assemblyData.m_iCABuilder; num++)
                    {
                        numArray[num] = this.m_assemblyData.m_CABuilders[num].PrepareCreateCustomAttributeToDisk(onDiskAssemblyModuleBuilder);
                    }
                    for (num = 0; num < this.m_assemblyData.m_iCAs; num++)
                    {
                        numArray2[num] = onDiskAssemblyModuleBuilder.InternalGetConstructorToken(this.m_assemblyData.m_CACons[num], true).Token;
                    }
                }
                int count = this.m_assemblyData.m_moduleBuilderList.Count;
                for (num = 0; num < count; num++)
                {
                    ModuleBuilder builder5 = this.m_assemblyData.m_moduleBuilderList[num];
                    if (!builder5.IsTransient() && (builder5 != mod))
                    {
                        string strFileName = builder5.m_moduleData.m_strFileName;
                        if (this.m_assemblyData.m_strDir != null)
                        {
                            strFileName = Path.GetFullPath(Path.Combine(this.m_assemblyData.m_strDir, strFileName));
                        }
                        new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Write, strFileName).Demand();
                        builder5.m_moduleData.FileToken = AddFile(this.GetNativeHandle(), builder5.m_moduleData.m_strFileName);
                        builder5.PreSave(strFileName, portableExecutableKind, imageFileMachine);
                        builder5.Save(strFileName, false, portableExecutableKind, imageFileMachine);
                        SetFileHashValue(this.GetNativeHandle(), builder5.m_moduleData.FileToken, strFileName);
                    }
                }
                for (num = 0; num < this.m_assemblyData.m_iPublicComTypeCount; num++)
                {
                    ModuleBuilder moduleBuilder;
                    Type type = this.m_assemblyData.m_publicComTypeList[num];
                    if (type is RuntimeType)
                    {
                        InternalModuleBuilder module = (InternalModuleBuilder) type.Module;
                        moduleBuilder = this.GetModuleBuilder(module);
                        if (moduleBuilder != mod)
                        {
                            this.DefineNestedComType(type, moduleBuilder.m_moduleData.FileToken, type.MetadataToken);
                        }
                    }
                    else
                    {
                        TypeBuilder builder = (TypeBuilder) type;
                        moduleBuilder = builder.GetModuleBuilder();
                        if (moduleBuilder != mod)
                        {
                            this.DefineNestedComType(type, moduleBuilder.m_moduleData.FileToken, builder.MetadataTokenInternal);
                        }
                    }
                }
                if (onDiskAssemblyModuleBuilder != this.m_manifestModuleBuilder)
                {
                    for (num = 0; num < this.m_assemblyData.m_iCABuilder; num++)
                    {
                        this.m_assemblyData.m_CABuilders[num].CreateCustomAttribute(onDiskAssemblyModuleBuilder, 0x20000001, numArray[num], true);
                    }
                    for (num = 0; num < this.m_assemblyData.m_iCAs; num++)
                    {
                        TypeBuilder.DefineCustomAttribute(onDiskAssemblyModuleBuilder, 0x20000001, numArray2[num], this.m_assemblyData.m_CABytes[num], true, false);
                    }
                }
                if (this.m_assemblyData.m_RequiredPset != null)
                {
                    this.AddDeclarativeSecurity(this.m_assemblyData.m_RequiredPset, SecurityAction.RequestMinimum);
                }
                if (this.m_assemblyData.m_RefusedPset != null)
                {
                    this.AddDeclarativeSecurity(this.m_assemblyData.m_RefusedPset, SecurityAction.RequestRefuse);
                }
                if (this.m_assemblyData.m_OptionalPset != null)
                {
                    this.AddDeclarativeSecurity(this.m_assemblyData.m_OptionalPset, SecurityAction.RequestOptional);
                }
                count = this.m_assemblyData.m_resWriterList.Count;
                for (num = 0; num < count; num++)
                {
                    ResWriterData data = null;
                    try
                    {
                        data = this.m_assemblyData.m_resWriterList[num];
                        if (data.m_resWriter != null)
                        {
                            new FileIOPermission(FileIOPermissionAccess.Append | FileIOPermissionAccess.Write, data.m_strFullFileName).Demand();
                        }
                    }
                    finally
                    {
                        if ((data != null) && (data.m_resWriter != null))
                        {
                            data.m_resWriter.Close();
                        }
                    }
                    AddStandAloneResource(this.GetNativeHandle(), data.m_strName, data.m_strFileName, data.m_strFullFileName, (int) data.m_attribute);
                }
                if (mod == null)
                {
                    onDiskAssemblyModuleBuilder.DefineNativeResource(portableExecutableKind, imageFileMachine);
                    int entryPoint = (this.m_assemblyData.m_entryPointModule != null) ? this.m_assemblyData.m_entryPointModule.m_moduleData.FileToken : 0;
                    SaveManifestToDisk(this.GetNativeHandle(), assemblyFileName, entryPoint, (int) this.m_assemblyData.m_peFileKind, (int) portableExecutableKind, (int) imageFileMachine);
                }
                else
                {
                    if ((this.m_assemblyData.m_entryPointModule != null) && (this.m_assemblyData.m_entryPointModule != mod))
                    {
                        mod.SetEntryPoint(new MethodToken(this.m_assemblyData.m_entryPointModule.m_moduleData.FileToken));
                    }
                    mod.Save(assemblyFileName, true, portableExecutableKind, imageFileMachine);
                }
                this.m_assemblyData.m_isSaved = true;
            }
            finally
            {
                if (s != null)
                {
                    File.Delete(s);
                }
            }
        }

        [SecuritySafeCritical]
        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (customBuilder == null)
            {
                throw new ArgumentNullException("customBuilder");
            }
            lock (this.SyncRoot)
            {
                this.SetCustomAttributeNoLock(customBuilder);
            }
        }

        [ComVisible(true), SecuritySafeCritical]
        public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
        {
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }
            if (binaryAttribute == null)
            {
                throw new ArgumentNullException("binaryAttribute");
            }
            lock (this.SyncRoot)
            {
                this.SetCustomAttributeNoLock(con, binaryAttribute);
            }
        }

        [SecurityCritical]
        private void SetCustomAttributeNoLock(CustomAttributeBuilder customBuilder)
        {
            customBuilder.CreateCustomAttribute(this.m_manifestModuleBuilder, 0x20000001);
            if (this.m_assemblyData.m_access != AssemblyBuilderAccess.Run)
            {
                this.m_assemblyData.AddCustomAttribute(customBuilder);
            }
        }

        [SecurityCritical]
        private void SetCustomAttributeNoLock(ConstructorInfo con, byte[] binaryAttribute)
        {
            TypeBuilder.DefineCustomAttribute(this.m_manifestModuleBuilder, 0x20000001, this.m_manifestModuleBuilder.GetConstructorToken(con).Token, binaryAttribute, false, typeof(DebuggableAttribute) == con.DeclaringType);
            if (this.m_assemblyData.m_access != AssemblyBuilderAccess.Run)
            {
                this.m_assemblyData.AddCustomAttribute(con, binaryAttribute);
            }
        }

        public void SetEntryPoint(MethodInfo entryMethod)
        {
            this.SetEntryPoint(entryMethod, PEFileKinds.ConsoleApplication);
        }

        public void SetEntryPoint(MethodInfo entryMethod, PEFileKinds fileKind)
        {
            lock (this.SyncRoot)
            {
                this.SetEntryPointNoLock(entryMethod, fileKind);
            }
        }

        private void SetEntryPointNoLock(MethodInfo entryMethod, PEFileKinds fileKind)
        {
            if (entryMethod == null)
            {
                throw new ArgumentNullException("entryMethod");
            }
            Module module = entryMethod.Module;
            if ((module == null) || !this.InternalAssembly.Equals(module.Assembly))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EntryMethodNotDefinedInAssembly"));
            }
            this.m_assemblyData.m_entryPointMethod = entryMethod;
            this.m_assemblyData.m_peFileKind = fileKind;
            ModuleBuilder builder = module as ModuleBuilder;
            if (builder != null)
            {
                this.m_assemblyData.m_entryPointModule = builder;
            }
            else
            {
                this.m_assemblyData.m_entryPointModule = this.GetModuleBuilder((InternalModuleBuilder) module);
            }
            MethodToken methodToken = this.m_assemblyData.m_entryPointModule.GetMethodToken(entryMethod);
            this.m_assemblyData.m_entryPointModule.SetEntryPoint(methodToken);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetFileHashValue(RuntimeAssembly assembly, int tkFile, string strFullFileName);
        void _AssemblyBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _AssemblyBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _AssemblyBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _AssemblyBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override string CodeBase
        {
            [SecuritySafeCritical]
            get
            {
                return this.InternalAssembly.CodeBase;
            }
        }

        public override MethodInfo EntryPoint
        {
            get
            {
                return this.m_assemblyData.m_entryPointMethod;
            }
        }

        public override System.Security.Policy.Evidence Evidence
        {
            get
            {
                return this.InternalAssembly.Evidence;
            }
        }

        public override string FullName
        {
            get
            {
                return this.InternalAssembly.FullName;
            }
        }

        public override bool GlobalAssemblyCache
        {
            get
            {
                return this.InternalAssembly.GlobalAssemblyCache;
            }
        }

        public override long HostContext
        {
            get
            {
                return this.InternalAssembly.HostContext;
            }
        }

        public override string ImageRuntimeVersion
        {
            get
            {
                return this.InternalAssembly.ImageRuntimeVersion;
            }
        }

        internal InternalAssemblyBuilder InternalAssembly
        {
            get
            {
                return this.m_internalAssemblyBuilder;
            }
        }

        public override bool IsDynamic
        {
            get
            {
                return true;
            }
        }

        public override string Location
        {
            get
            {
                return this.InternalAssembly.Location;
            }
        }

        public override Module ManifestModule
        {
            get
            {
                return this.m_manifestModuleBuilder.InternalModule;
            }
        }

        public override System.Security.PermissionSet PermissionSet
        {
            [SecurityCritical]
            get
            {
                return this.InternalAssembly.PermissionSet;
            }
        }

        public override bool ReflectionOnly
        {
            get
            {
                return this.InternalAssembly.ReflectionOnly;
            }
        }

        public override System.Security.SecurityRuleSet SecurityRuleSet
        {
            get
            {
                return this.InternalAssembly.SecurityRuleSet;
            }
        }

        internal object SyncRoot
        {
            get
            {
                return this.InternalAssembly.SyncRoot;
            }
        }

        private class AssemblyBuilderLock
        {
        }
    }
}


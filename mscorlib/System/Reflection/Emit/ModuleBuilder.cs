namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.SymbolStore;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [ClassInterface(ClassInterfaceType.None), ComVisible(true), ComDefaultInterface(typeof(_ModuleBuilder)), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class ModuleBuilder : Module, _ModuleBuilder
    {
        private AssemblyBuilder m_assemblyBuilder;
        private MethodToken m_EntryPoint;
        internal InternalModuleBuilder m_internalModuleBuilder;
        private ISymbolWriter m_iSymWriter;
        internal ModuleBuilderData m_moduleData;
        private List<Type> m_TypeBuilderList;

        internal ModuleBuilder(AssemblyBuilder assemblyBuilder, InternalModuleBuilder internalModuleBuilder)
        {
            this.m_internalModuleBuilder = internalModuleBuilder;
            this.m_assemblyBuilder = assemblyBuilder;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddResource(RuntimeModule module, string strName, byte[] resBytes, int resByteCount, int tkFile, int attribute, int portableExecutableKind, int imageFileMachine);
        internal void AddType(Type type)
        {
            this.m_TypeBuilderList.Add(type);
        }

        internal void CheckContext(params Type[][] typess)
        {
            this.ContainingAssemblyBuilder.CheckContext(typess);
        }

        internal void CheckContext(params Type[] types)
        {
            this.ContainingAssemblyBuilder.CheckContext(types);
        }

        internal void CheckTypeNameConflict(string strTypeName, TypeBuilder enclosingType)
        {
            for (int i = 0; i < this.m_TypeBuilderList.Count; i++)
            {
                Type type = this.m_TypeBuilderList[i];
                if (type.FullName.Equals(strTypeName) && object.ReferenceEquals(type.DeclaringType, enclosingType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_DuplicateTypeName"));
                }
            }
        }

        public void CreateGlobalFunctions()
        {
            lock (this.SyncRoot)
            {
                this.CreateGlobalFunctionsNoLock();
            }
        }

        private void CreateGlobalFunctionsNoLock()
        {
            if (this.m_moduleData.m_fGlobalBeenCreated)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
            }
            this.m_moduleData.m_globalTypeBuilder.CreateType();
            this.m_moduleData.m_fGlobalBeenCreated = true;
        }

        [SecuritySafeCritical]
        public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            lock (this.SyncRoot)
            {
                return this.DefineDocumentNoLock(url, language, languageVendor, documentType);
            }
        }

        private ISymbolDocumentWriter DefineDocumentNoLock(string url, Guid language, Guid languageVendor, Guid documentType)
        {
            if (this.m_iSymWriter == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
            }
            return this.m_iSymWriter.DefineDocument(url, language, languageVendor, documentType);
        }

        [SecuritySafeCritical]
        public EnumBuilder DefineEnum(string name, TypeAttributes visibility, Type underlyingType)
        {
            this.CheckContext(new Type[] { underlyingType });
            lock (this.SyncRoot)
            {
                return this.DefineEnumNoLock(name, visibility, underlyingType);
            }
        }

        [SecurityCritical]
        private EnumBuilder DefineEnumNoLock(string name, TypeAttributes visibility, Type underlyingType)
        {
            EnumBuilder type = new EnumBuilder(name, underlyingType, visibility, this);
            this.AddType(type);
            return type;
        }

        public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
        {
            return this.DefineGlobalMethod(name, attributes, CallingConventions.Standard, returnType, parameterTypes);
        }

        public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            return this.DefineGlobalMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
        }

        public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
        {
            lock (this.SyncRoot)
            {
                return this.DefineGlobalMethodNoLock(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
            }
        }

        private MethodBuilder DefineGlobalMethodNoLock(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
        {
            if (this.m_moduleData.m_fGlobalBeenCreated)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GlobalsHaveBeenCreated"));
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if ((attributes & MethodAttributes.Static) == MethodAttributes.PrivateScope)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_GlobalFunctionHasToBeStatic"));
            }
            this.CheckContext(new Type[] { returnType });
            this.CheckContext(new Type[][] { requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes });
            this.CheckContext(requiredParameterTypeCustomModifiers);
            this.CheckContext(optionalParameterTypeCustomModifiers);
            this.m_moduleData.m_fHasGlobal = true;
            return this.m_moduleData.m_globalTypeBuilder.DefineMethod(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
        }

        [SecuritySafeCritical]
        public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
        {
            lock (this.SyncRoot)
            {
                return this.DefineInitializedDataNoLock(name, data, attributes);
            }
        }

        private FieldBuilder DefineInitializedDataNoLock(string name, byte[] data, FieldAttributes attributes)
        {
            if (this.m_moduleData.m_fGlobalBeenCreated)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GlobalsHaveBeenCreated"));
            }
            this.m_moduleData.m_fHasGlobal = true;
            return this.m_moduleData.m_globalTypeBuilder.DefineInitializedData(name, data, attributes);
        }

        public void DefineManifestResource(string name, Stream stream, ResourceAttributes attribute)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this.SyncRoot)
            {
                this.DefineManifestResourceNoLock(name, stream, attribute);
            }
        }

        private void DefineManifestResourceNoLock(string name, Stream stream, ResourceAttributes attribute)
        {
            if (this.IsTransient())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (!this.m_assemblyBuilder.IsPersistable())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
            }
            this.m_assemblyBuilder.m_assemblyData.CheckResNameConflict(name);
            ResWriterData data = new ResWriterData(null, stream, name, string.Empty, string.Empty, attribute) {
                m_nextResWriter = this.m_moduleData.m_embeddedRes
            };
            this.m_moduleData.m_embeddedRes = data;
        }

        [SecurityCritical]
        internal void DefineNativeResource(PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
        {
            string strResourceFileName = this.m_moduleData.m_strResourceFileName;
            byte[] resourceBytes = this.m_moduleData.m_resourceBytes;
            if (strResourceFileName != null)
            {
                DefineNativeResourceFile(this.GetNativeHandle(), strResourceFileName, (int) portableExecutableKind, (int) imageFileMachine);
            }
            else if (resourceBytes != null)
            {
                DefineNativeResourceBytes(this.GetNativeHandle(), resourceBytes, resourceBytes.Length, (int) portableExecutableKind, (int) imageFileMachine);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void DefineNativeResourceBytes(RuntimeModule module, byte[] pbResource, int cbResource, int portableExecutableKind, int imageFileMachine);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void DefineNativeResourceFile(RuntimeModule module, string strFilename, int portableExecutableKind, int ImageFileMachine);
        [SecuritySafeCritical]
        public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            return this.DefinePInvokeMethod(name, dllName, name, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
        }

        [SecuritySafeCritical]
        public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            lock (this.SyncRoot)
            {
                return this.DefinePInvokeMethodNoLock(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
            }
        }

        private MethodBuilder DefinePInvokeMethodNoLock(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
        {
            if ((attributes & MethodAttributes.Static) == MethodAttributes.PrivateScope)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_GlobalFunctionHasToBeStatic"));
            }
            this.CheckContext(new Type[] { returnType });
            this.CheckContext(parameterTypes);
            this.m_moduleData.m_fHasGlobal = true;
            return this.m_moduleData.m_globalTypeBuilder.DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
        }

        public IResourceWriter DefineResource(string name, string description)
        {
            return this.DefineResource(name, description, ResourceAttributes.Public);
        }

        public IResourceWriter DefineResource(string name, string description, ResourceAttributes attribute)
        {
            lock (this.SyncRoot)
            {
                return this.DefineResourceNoLock(name, description, attribute);
            }
        }

        private IResourceWriter DefineResourceNoLock(string name, string description, ResourceAttributes attribute)
        {
            if (this.IsTransient())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
            }
            if (!this.m_assemblyBuilder.IsPersistable())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
            }
            this.m_assemblyBuilder.m_assemblyData.CheckResNameConflict(name);
            MemoryStream stream = new MemoryStream();
            ResourceWriter resWriter = new ResourceWriter(stream);
            ResWriterData data = new ResWriterData(resWriter, stream, name, string.Empty, string.Empty, attribute) {
                m_nextResWriter = this.m_moduleData.m_embeddedRes
            };
            this.m_moduleData.m_embeddedRes = data;
            return resWriter;
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineType(string name)
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeNoLock(name);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineType(string name, TypeAttributes attr)
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeNoLock(name, attr);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeNoLock(name, attr, parent);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, int typesize)
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeNoLock(name, attr, parent, typesize);
            }
        }

        [SecuritySafeCritical, ComVisible(true)]
        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeNoLock(name, attr, parent, interfaces);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packsize)
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeNoLock(name, attr, parent, packsize);
            }
        }

        [SecuritySafeCritical]
        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
        {
            lock (this.SyncRoot)
            {
                return this.DefineTypeNoLock(name, attr, parent, packingSize, typesize);
            }
        }

        [SecurityCritical]
        private TypeBuilder DefineTypeNoLock(string name)
        {
            TypeBuilder type = new TypeBuilder(name, TypeAttributes.AnsiClass, null, null, this, PackingSize.Unspecified, null);
            this.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr)
        {
            TypeBuilder type = new TypeBuilder(name, attr, null, null, this, PackingSize.Unspecified, null);
            this.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent)
        {
            this.CheckContext(new Type[] { parent });
            TypeBuilder type = new TypeBuilder(name, attr, parent, null, this, PackingSize.Unspecified, null);
            this.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, int typesize)
        {
            TypeBuilder type = new TypeBuilder(name, attr, parent, this, PackingSize.Unspecified, typesize, null);
            this.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            TypeBuilder type = new TypeBuilder(name, attr, parent, interfaces, this, PackingSize.Unspecified, null);
            this.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, PackingSize packsize)
        {
            TypeBuilder type = new TypeBuilder(name, attr, parent, null, this, packsize, null);
            this.AddType(type);
            return type;
        }

        [SecurityCritical]
        private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
        {
            TypeBuilder type = new TypeBuilder(name, attr, parent, this, packingSize, typesize, null);
            this.AddType(type);
            return type;
        }

        [SecuritySafeCritical]
        public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
        {
            lock (this.SyncRoot)
            {
                return this.DefineUninitializedDataNoLock(name, size, attributes);
            }
        }

        private FieldBuilder DefineUninitializedDataNoLock(string name, int size, FieldAttributes attributes)
        {
            if (this.m_moduleData.m_fGlobalBeenCreated)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GlobalsHaveBeenCreated"));
            }
            this.m_moduleData.m_fHasGlobal = true;
            return this.m_moduleData.m_globalTypeBuilder.DefineUninitializedData(name, size, attributes);
        }

        public void DefineUnmanagedResource(byte[] resource)
        {
            lock (this.SyncRoot)
            {
                this.DefineUnmanagedResourceInternalNoLock(resource);
            }
        }

        [SecuritySafeCritical]
        public void DefineUnmanagedResource(string resourceFileName)
        {
            lock (this.SyncRoot)
            {
                this.DefineUnmanagedResourceFileInternalNoLock(resourceFileName);
            }
        }

        [SecurityCritical]
        internal void DefineUnmanagedResourceFileInternalNoLock(string resourceFileName)
        {
            if (resourceFileName == null)
            {
                throw new ArgumentNullException("resourceFileName");
            }
            if ((this.m_moduleData.m_resourceBytes != null) || (this.m_moduleData.m_strResourceFileName != null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
            }
            string fullPath = Path.GetFullPath(resourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
            new EnvironmentPermission(PermissionState.Unrestricted).Assert();
            try
            {
                if (!File.Exists(resourceFileName))
                {
                    throw new FileNotFoundException(Environment.GetResourceString("IO.FileNotFound_FileName", new object[] { resourceFileName }), resourceFileName);
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            this.m_moduleData.m_strResourceFileName = fullPath;
        }

        internal void DefineUnmanagedResourceInternalNoLock(byte[] resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            if ((this.m_moduleData.m_strResourceFileName != null) || (this.m_moduleData.m_resourceBytes != null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
            }
            this.m_moduleData.m_resourceBytes = new byte[resource.Length];
            Array.Copy(resource, this.m_moduleData.m_resourceBytes, resource.Length);
        }

        public override bool Equals(object obj)
        {
            return this.InternalModule.Equals(obj);
        }

        internal virtual Type FindTypeBuilderWithName(string strTypeName, bool ignoreCase)
        {
            int count = this.m_TypeBuilderList.Count;
            Type type = null;
            int num2 = 0;
            while (num2 < count)
            {
                type = this.m_TypeBuilderList[num2];
                if (ignoreCase)
                {
                    if (string.Compare(type.FullName, strTypeName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                    {
                        break;
                    }
                }
                else if (type.FullName.Equals(strTypeName))
                {
                    break;
                }
                num2++;
            }
            if (num2 == count)
            {
                type = null;
            }
            return type;
        }

        [SecuritySafeCritical]
        public MethodInfo GetArrayMethod(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            this.CheckContext(new Type[] { returnType, arrayClass });
            this.CheckContext(parameterTypes);
            return new SymbolMethod(this, this.GetArrayMethodToken(arrayClass, methodName, callingConvention, returnType, parameterTypes), arrayClass, methodName, callingConvention, returnType, parameterTypes);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetArrayMethodToken(RuntimeModule module, int tkTypeSpec, string methodName, byte[] signature, int sigLength);
        [SecuritySafeCritical]
        public MethodToken GetArrayMethodToken(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            lock (this.SyncRoot)
            {
                return this.GetArrayMethodTokenNoLock(arrayClass, methodName, callingConvention, returnType, parameterTypes);
            }
        }

        [SecurityCritical]
        private MethodToken GetArrayMethodTokenNoLock(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
        {
            int num;
            if (arrayClass == null)
            {
                throw new ArgumentNullException("arrayClass");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            if (methodName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "methodName");
            }
            if (!arrayClass.IsArray)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_HasToBeArrayClass"));
            }
            this.CheckContext(new Type[] { returnType, arrayClass });
            this.CheckContext(parameterTypes);
            byte[] signature = SignatureHelper.GetMethodSigHelper(this, callingConvention, returnType, null, null, parameterTypes, null, null).InternalGetSignature(out num);
            TypeToken typeTokenInternal = this.GetTypeTokenInternal(arrayClass);
            return new MethodToken(GetArrayMethodToken(this.GetNativeHandle(), typeTokenInternal.Token, methodName, signature, num));
        }

        [ComVisible(true), SecuritySafeCritical]
        public MethodToken GetConstructorToken(ConstructorInfo con)
        {
            return this.InternalGetConstructorToken(con, false);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.InternalModule.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.InternalModule.GetCustomAttributes(attributeType, inherit);
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return this.InternalModule.GetCustomAttributesData();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return this.InternalModule.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingFlags)
        {
            return this.InternalModule.GetFields(bindingFlags);
        }

        [SecuritySafeCritical]
        public FieldToken GetFieldToken(FieldInfo field)
        {
            lock (this.SyncRoot)
            {
                return this.GetFieldTokenNoLock(field);
            }
        }

        [SecurityCritical]
        private FieldToken GetFieldTokenNoLock(FieldInfo field)
        {
            int tokenFromTypeSpec;
            if (field == null)
            {
                throw new ArgumentNullException("con");
            }
            int num2 = 0;
            FieldBuilder builder = null;
            FieldOnTypeBuilderInstantiation instantiation = null;
            builder = field as FieldBuilder;
            if (builder != null)
            {
                if ((field.DeclaringType != null) && field.DeclaringType.IsGenericType)
                {
                    int num3;
                    byte[] signature = SignatureHelper.GetTypeSigToken(this, field.DeclaringType).InternalGetSignature(out num3);
                    tokenFromTypeSpec = this.GetTokenFromTypeSpec(signature, num3);
                    num2 = this.GetMemberRef(this, tokenFromTypeSpec, builder.GetToken().Token);
                }
                else
                {
                    if (builder.Module.Equals(this))
                    {
                        return builder.GetToken();
                    }
                    if (field.DeclaringType == null)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
                    }
                    tokenFromTypeSpec = this.GetTypeTokenInternal(field.DeclaringType).Token;
                    num2 = this.GetMemberRef(field.ReflectedType.Module, tokenFromTypeSpec, builder.GetToken().Token);
                }
            }
            else if (field is RuntimeFieldInfo)
            {
                if (field.DeclaringType == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
                }
                int metadataToken = field.MetadataToken;
                if ((field.DeclaringType != null) && field.DeclaringType.IsGenericType)
                {
                    int num5;
                    byte[] buffer2 = SignatureHelper.GetTypeSigToken(this, field.DeclaringType).InternalGetSignature(out num5);
                    tokenFromTypeSpec = this.GetTokenFromTypeSpec(buffer2, num5);
                    num2 = this.GetMemberRefOfFieldInfo(tokenFromTypeSpec, field.DeclaringType.GetTypeHandleInternal(), metadataToken);
                }
                else
                {
                    tokenFromTypeSpec = this.GetTypeTokenInternal(field.DeclaringType).Token;
                    num2 = this.GetMemberRefOfFieldInfo(tokenFromTypeSpec, field.DeclaringType.GetTypeHandleInternal(), metadataToken);
                }
            }
            else
            {
                instantiation = field as FieldOnTypeBuilderInstantiation;
                if (instantiation != null)
                {
                    int num6;
                    FieldInfo fieldInfo = instantiation.FieldInfo;
                    byte[] buffer3 = SignatureHelper.GetTypeSigToken(this, field.DeclaringType).InternalGetSignature(out num6);
                    tokenFromTypeSpec = this.GetTokenFromTypeSpec(buffer3, num6);
                    num2 = this.GetMemberRef(fieldInfo.ReflectedType.Module, tokenFromTypeSpec, instantiation.MetadataTokenInternal);
                }
                else
                {
                    int num7;
                    tokenFromTypeSpec = this.GetTypeTokenInternal(field.ReflectedType).Token;
                    SignatureHelper fieldSigHelper = SignatureHelper.GetFieldSigHelper(this);
                    fieldSigHelper.AddArgument(field.FieldType, field.GetRequiredCustomModifiers(), field.GetOptionalCustomModifiers());
                    byte[] buffer4 = fieldSigHelper.InternalGetSignature(out num7);
                    num2 = this.GetMemberRefFromSignature(tokenFromTypeSpec, field.Name, buffer4, num7);
                }
            }
            return new FieldToken(num2, field.GetType());
        }

        public override int GetHashCode()
        {
            return this.InternalModule.GetHashCode();
        }

        [SecurityCritical]
        private int GetMemberRef(Module refedModule, int tr, int defToken)
        {
            return GetMemberRef(this.GetNativeHandle(), GetRuntimeModuleFromModule(refedModule).GetNativeHandle(), tr, defToken);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetMemberRef(RuntimeModule module, RuntimeModule refedModule, int tr, int defToken);
        [SecurityCritical]
        private int GetMemberRefFromSignature(int tr, string methodName, byte[] signature, int length)
        {
            return GetMemberRefFromSignature(this.GetNativeHandle(), tr, methodName, signature, length);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetMemberRefFromSignature(RuntimeModule module, int tr, string methodName, byte[] signature, int length);
        [SecurityCritical]
        private int GetMemberRefOfFieldInfo(int tkType, RuntimeTypeHandle declaringType, int tkField)
        {
            return GetMemberRefOfFieldInfo(this.GetNativeHandle(), tkType, declaringType, tkField);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetMemberRefOfFieldInfo(RuntimeModule module, int tkType, RuntimeTypeHandle declaringType, int tkField);
        [SecurityCritical]
        private int GetMemberRefOfMethodInfo(int tr, IRuntimeMethodInfo method)
        {
            return GetMemberRefOfMethodInfo(this.GetNativeHandle(), tr, method);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetMemberRefOfMethodInfo(RuntimeModule module, int tr, IRuntimeMethodInfo method);
        [SecurityCritical]
        internal SignatureHelper GetMemberRefSignature(CallingConventions call, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes, int cGenericParameters)
        {
            int length;
            int num2;
            if (parameterTypes == null)
            {
                length = 0;
            }
            else
            {
                length = parameterTypes.Length;
            }
            SignatureHelper helper = SignatureHelper.GetMethodSigHelper(this, call, returnType, cGenericParameters);
            for (num2 = 0; num2 < length; num2++)
            {
                helper.AddArgument(parameterTypes[num2]);
            }
            if ((optionalParameterTypes != null) && (optionalParameterTypes.Length != 0))
            {
                helper.AddSentinel();
                for (num2 = 0; num2 < optionalParameterTypes.Length; num2++)
                {
                    helper.AddArgument(optionalParameterTypes[num2]);
                }
            }
            return helper;
        }

        [SecurityCritical]
        internal int GetMemberRefToken(MethodBase method, Type[] optionalParameterTypes)
        {
            Type[] parameterTypes;
            Type methodBaseReturnType;
            int tokenFromTypeSpec;
            int num4;
            int cGenericParameters = 0;
            if (method.IsGenericMethod)
            {
                if (!method.IsGenericMethodDefinition)
                {
                    throw new InvalidOperationException();
                }
                cGenericParameters = method.GetGenericArguments().Length;
            }
            if ((optionalParameterTypes != null) && ((method.CallingConvention & CallingConventions.VarArgs) == 0))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAVarArgCallingConvention"));
            }
            MethodInfo info = method as MethodInfo;
            if (method.DeclaringType.IsGenericType)
            {
                MethodBase ctor = null;
                MethodOnTypeBuilderInstantiation instantiation = method as MethodOnTypeBuilderInstantiation;
                if (instantiation != null)
                {
                    ctor = instantiation.m_method;
                }
                else
                {
                    ConstructorOnTypeBuilderInstantiation instantiation2 = method as ConstructorOnTypeBuilderInstantiation;
                    if (instantiation2 != null)
                    {
                        ctor = instantiation2.m_ctor;
                    }
                    else if ((method is MethodBuilder) || (method is ConstructorBuilder))
                    {
                        ctor = method;
                    }
                    else if (method.IsGenericMethod)
                    {
                        ctor = info.GetGenericMethodDefinition();
                        ctor = ctor.Module.ResolveMethod(method.MetadataToken, (ctor.DeclaringType != null) ? ctor.DeclaringType.GetGenericArguments() : null, ctor.GetGenericArguments());
                    }
                    else
                    {
                        ctor = method.Module.ResolveMethod(method.MetadataToken, (method.DeclaringType != null) ? method.DeclaringType.GetGenericArguments() : null, null);
                    }
                }
                parameterTypes = ctor.GetParameterTypes();
                methodBaseReturnType = MethodBuilder.GetMethodBaseReturnType(ctor);
            }
            else
            {
                parameterTypes = method.GetParameterTypes();
                methodBaseReturnType = MethodBuilder.GetMethodBaseReturnType(method);
            }
            if (method.DeclaringType.IsGenericType)
            {
                int num3;
                byte[] buffer = SignatureHelper.GetTypeSigToken(this, method.DeclaringType).InternalGetSignature(out num3);
                tokenFromTypeSpec = this.GetTokenFromTypeSpec(buffer, num3);
            }
            else if (!method.Module.Equals(this))
            {
                tokenFromTypeSpec = this.GetTypeToken(method.DeclaringType).Token;
            }
            else if (info != null)
            {
                tokenFromTypeSpec = this.GetMethodToken(info).Token;
            }
            else
            {
                tokenFromTypeSpec = this.GetConstructorToken(method as ConstructorInfo).Token;
            }
            byte[] signature = this.GetMemberRefSignature(method.CallingConvention, methodBaseReturnType, parameterTypes, optionalParameterTypes, cGenericParameters).InternalGetSignature(out num4);
            return this.GetMemberRefFromSignature(tokenFromTypeSpec, method.Name, signature, num4);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return this.InternalModule.GetMethodInternal(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingFlags)
        {
            return this.InternalModule.GetMethods(bindingFlags);
        }

        [SecuritySafeCritical]
        public MethodToken GetMethodToken(MethodInfo method)
        {
            lock (this.SyncRoot)
            {
                return this.GetMethodTokenNoLock(method, true);
            }
        }

        [SecurityCritical]
        internal MethodToken GetMethodTokenInternal(MethodInfo method)
        {
            lock (this.SyncRoot)
            {
                return this.GetMethodTokenNoLock(method, false);
            }
        }

        [SecurityCritical]
        private MethodToken GetMethodTokenNoLock(MethodInfo method, bool getGenericTypeDefinition)
        {
            int num;
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            int str = 0;
            SymbolMethod method2 = null;
            MethodBuilder builder = null;
            builder = method as MethodBuilder;
            if (builder != null)
            {
                int metadataTokenInternal = builder.MetadataTokenInternal;
                if (method.Module.Equals(this))
                {
                    return new MethodToken(metadataTokenInternal);
                }
                if (method.DeclaringType == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
                }
                num = getGenericTypeDefinition ? this.GetTypeToken(method.DeclaringType).Token : this.GetTypeTokenInternal(method.DeclaringType).Token;
                str = this.GetMemberRef(method.DeclaringType.Module, num, metadataTokenInternal);
            }
            else
            {
                if (method is MethodOnTypeBuilderInstantiation)
                {
                    return new MethodToken(this.GetMemberRefToken(method, null));
                }
                method2 = method as SymbolMethod;
                if (method2 != null)
                {
                    if (method2.GetModule() == this)
                    {
                        return method2.GetToken();
                    }
                    return method2.GetToken(this);
                }
                Type declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
                }
                RuntimeMethodInfo info = null;
                if (declaringType.IsArray)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    Type[] parameterTypes = new Type[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        parameterTypes[i] = parameters[i].ParameterType;
                    }
                    return this.GetArrayMethodToken(declaringType, method.Name, method.CallingConvention, method.ReturnType, parameterTypes);
                }
                info = method as RuntimeMethodInfo;
                if (info != null)
                {
                    num = getGenericTypeDefinition ? this.GetTypeToken(method.DeclaringType).Token : this.GetTypeTokenInternal(method.DeclaringType).Token;
                    str = this.GetMemberRefOfMethodInfo(num, info);
                }
                else
                {
                    SignatureHelper helper;
                    int num6;
                    ParameterInfo[] infoArray2 = method.GetParameters();
                    Type[] typeArray2 = new Type[infoArray2.Length];
                    Type[][] requiredParameterTypeCustomModifiers = new Type[typeArray2.Length][];
                    Type[][] optionalParameterTypeCustomModifiers = new Type[typeArray2.Length][];
                    for (int j = 0; j < infoArray2.Length; j++)
                    {
                        typeArray2[j] = infoArray2[j].ParameterType;
                        requiredParameterTypeCustomModifiers[j] = infoArray2[j].GetRequiredCustomModifiers();
                        optionalParameterTypeCustomModifiers[j] = infoArray2[j].GetOptionalCustomModifiers();
                    }
                    num = getGenericTypeDefinition ? this.GetTypeToken(method.DeclaringType).Token : this.GetTypeTokenInternal(method.DeclaringType).Token;
                    try
                    {
                        helper = SignatureHelper.GetMethodSigHelper(this, method.CallingConvention, method.ReturnType, method.ReturnParameter.GetRequiredCustomModifiers(), method.ReturnParameter.GetOptionalCustomModifiers(), typeArray2, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
                    }
                    catch (NotImplementedException)
                    {
                        helper = SignatureHelper.GetMethodSigHelper(this, method.ReturnType, typeArray2);
                    }
                    byte[] signature = helper.InternalGetSignature(out num6);
                    str = this.GetMemberRefFromSignature(num, method.Name, signature, num6);
                }
            }
            return new MethodToken(str);
        }

        internal override ModuleHandle GetModuleHandle()
        {
            return new ModuleHandle(this.GetNativeHandle());
        }

        internal RuntimeModule GetNativeHandle()
        {
            return this.InternalModule.GetNativeHandle();
        }

        public override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
        {
            this.InternalModule.GetPEKind(out peKind, out machine);
        }

        private static RuntimeModule GetRuntimeModuleFromModule(Module m)
        {
            ModuleBuilder builder = m as ModuleBuilder;
            if (builder != null)
            {
                return builder.InternalModule;
            }
            return (m as RuntimeModule);
        }

        [SecuritySafeCritical]
        public SignatureToken GetSignatureToken(SignatureHelper sigHelper)
        {
            int num;
            if (sigHelper == null)
            {
                throw new ArgumentNullException("sigHelper");
            }
            byte[] signature = sigHelper.InternalGetSignature(out num);
            return new SignatureToken(TypeBuilder.GetTokenFromSig(this.GetNativeHandle(), signature, num), this);
        }

        [SecuritySafeCritical]
        public SignatureToken GetSignatureToken(byte[] sigBytes, int sigLength)
        {
            if (sigBytes == null)
            {
                throw new ArgumentNullException("sigBytes");
            }
            byte[] destinationArray = new byte[sigBytes.Length];
            Array.Copy(sigBytes, destinationArray, sigBytes.Length);
            return new SignatureToken(TypeBuilder.GetTokenFromSig(this.GetNativeHandle(), destinationArray, sigLength), this);
        }

        [SecuritySafeCritical]
        public override X509Certificate GetSignerCertificate()
        {
            return this.InternalModule.GetSignerCertificate();
        }

        [SecuritySafeCritical]
        public StringToken GetStringConstant(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return new StringToken(GetStringConstant(this.GetNativeHandle(), str, str.Length));
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetStringConstant(RuntimeModule module, string str, int length);
        public ISymbolWriter GetSymWriter()
        {
            return this.m_iSymWriter;
        }

        [SecurityCritical]
        private int GetTokenFromTypeSpec(byte[] signature, int length)
        {
            return GetTokenFromTypeSpec(this.GetNativeHandle(), signature, length);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetTokenFromTypeSpec(RuntimeModule pModule, byte[] signature, int length);
        [ComVisible(true)]
        public override Type GetType(string className)
        {
            return this.GetType(className, false, false);
        }

        [ComVisible(true)]
        public override Type GetType(string className, bool ignoreCase)
        {
            return this.GetType(className, false, ignoreCase);
        }

        private Type GetType(string strFormat, Type baseType)
        {
            if ((strFormat != null) && !strFormat.Equals(string.Empty))
            {
                return SymbolType.FormCompoundType(strFormat.ToCharArray(), baseType, 0);
            }
            return baseType;
        }

        [ComVisible(true)]
        public override Type GetType(string className, bool throwOnError, bool ignoreCase)
        {
            lock (this.SyncRoot)
            {
                return this.GetTypeNoLock(className, throwOnError, ignoreCase);
            }
        }

        private Type GetTypeNoLock(string className, bool throwOnError, bool ignoreCase)
        {
            Type baseType = this.InternalModule.GetType(className, throwOnError, ignoreCase);
            if (baseType != null)
            {
                return baseType;
            }
            string str = null;
            string strFormat = null;
            int startIndex = 0;
            while (startIndex <= className.Length)
            {
                int length = className.IndexOfAny(new char[] { '[', '*', '&' }, startIndex);
                if (length == -1)
                {
                    str = className;
                    strFormat = null;
                    break;
                }
                int num3 = 0;
                for (int i = length - 1; (i >= 0) && (className[i] == '\\'); i--)
                {
                    num3++;
                }
                if ((num3 % 2) == 1)
                {
                    startIndex = length + 1;
                }
                else
                {
                    str = className.Substring(0, length);
                    strFormat = className.Substring(length);
                    break;
                }
            }
            if (str == null)
            {
                str = className;
                strFormat = null;
            }
            str = str.Replace(@"\\", @"\").Replace(@"\[", "[").Replace(@"\*", "*").Replace(@"\&", "&");
            if (strFormat != null)
            {
                baseType = this.InternalModule.GetType(str, false, ignoreCase);
            }
            if (baseType == null)
            {
                baseType = this.FindTypeBuilderWithName(str, ignoreCase);
                if ((baseType == null) && (this.Assembly is AssemblyBuilder))
                {
                    List<ModuleBuilder> moduleBuilderList = this.ContainingAssemblyBuilder.m_assemblyData.m_moduleBuilderList;
                    int count = moduleBuilderList.Count;
                    for (int j = 0; (j < count) && (baseType == null); j++)
                    {
                        baseType = moduleBuilderList[j].FindTypeBuilderWithName(str, ignoreCase);
                    }
                }
                if (baseType == null)
                {
                    return null;
                }
            }
            if (strFormat == null)
            {
                return baseType;
            }
            return this.GetType(strFormat, baseType);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetTypeRef(RuntimeModule module, string strFullName, RuntimeModule refedModule, string strRefedModuleFileName, int tkResolution);
        [SecurityCritical]
        private int GetTypeRefNested(Type type, Module refedModule, string strRefedModuleFileName)
        {
            Type declaringType = type.DeclaringType;
            int tkResolution = 0;
            string fullName = type.FullName;
            if (declaringType != null)
            {
                tkResolution = this.GetTypeRefNested(declaringType, refedModule, strRefedModuleFileName);
                fullName = UnmangleTypeName(fullName);
            }
            return GetTypeRef(this.GetNativeHandle(), fullName, GetRuntimeModuleFromModule(refedModule).GetNativeHandle(), strRefedModuleFileName, tkResolution);
        }

        public override Type[] GetTypes()
        {
            lock (this.SyncRoot)
            {
                return this.GetTypesNoLock();
            }
        }

        internal Type[] GetTypesNoLock()
        {
            int count = this.m_TypeBuilderList.Count;
            List<Type> list = new List<Type>(count);
            for (int i = 0; i < count; i++)
            {
                TypeBuilder typeBuilder;
                EnumBuilder builder2 = this.m_TypeBuilderList[i] as EnumBuilder;
                if (builder2 != null)
                {
                    typeBuilder = builder2.m_typeBuilder;
                }
                else
                {
                    typeBuilder = this.m_TypeBuilderList[i] as TypeBuilder;
                }
                if (typeBuilder != null)
                {
                    if (typeBuilder.m_hasBeenCreated)
                    {
                        list.Add(typeBuilder.UnderlyingSystemType);
                    }
                    else
                    {
                        list.Add(typeBuilder);
                    }
                }
                else
                {
                    list.Add(this.m_TypeBuilderList[i]);
                }
            }
            return list.ToArray();
        }

        public TypeToken GetTypeToken(string name)
        {
            return this.GetTypeToken(this.InternalModule.GetType(name, false, true));
        }

        [SecuritySafeCritical]
        public TypeToken GetTypeToken(Type type)
        {
            return this.GetTypeTokenInternal(type, true);
        }

        [SecurityCritical]
        internal TypeToken GetTypeTokenInternal(Type type)
        {
            return this.GetTypeTokenInternal(type, false);
        }

        [SecurityCritical]
        private TypeToken GetTypeTokenInternal(Type type, bool getGenericDefinition)
        {
            lock (this.SyncRoot)
            {
                return this.GetTypeTokenWorkerNoLock(type, getGenericDefinition);
            }
        }

        [SecurityCritical]
        private TypeToken GetTypeTokenWorkerNoLock(Type type, bool getGenericDefinition)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.CheckContext(new Type[] { type });
            if (type.IsByRef)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_CannotGetTypeTokenForByRef"));
            }
            if ((type.IsGenericType && (!type.IsGenericTypeDefinition || !getGenericDefinition)) || ((type.IsGenericParameter || type.IsArray) || type.IsPointer))
            {
                int num;
                byte[] signature = SignatureHelper.GetTypeSigToken(this, type).InternalGetSignature(out num);
                return new TypeToken(this.GetTokenFromTypeSpec(signature, num));
            }
            Module refedModule = type.Module;
            if (refedModule.Equals(this))
            {
                TypeBuilder typeBuilder = null;
                GenericTypeParameterBuilder builder2 = null;
                EnumBuilder builder3 = type as EnumBuilder;
                if (builder3 != null)
                {
                    typeBuilder = builder3.m_typeBuilder;
                }
                else
                {
                    typeBuilder = type as TypeBuilder;
                }
                if (typeBuilder != null)
                {
                    return typeBuilder.TypeToken;
                }
                builder2 = type as GenericTypeParameterBuilder;
                if (builder2 != null)
                {
                    return new TypeToken(builder2.MetadataTokenInternal);
                }
                return new TypeToken(this.GetTypeRefNested(type, this, string.Empty));
            }
            ModuleBuilder moduleBuilder = refedModule as ModuleBuilder;
            bool flag = (moduleBuilder != null) ? moduleBuilder.IsTransient() : ((RuntimeModule) refedModule).IsTransientInternal();
            if (!this.IsTransient() && flag)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadTransientModuleReference"));
            }
            string strRefedModuleFileName = string.Empty;
            if (refedModule.Assembly.Equals(this.Assembly))
            {
                if (moduleBuilder == null)
                {
                    moduleBuilder = this.ContainingAssemblyBuilder.GetModuleBuilder((InternalModuleBuilder) refedModule);
                }
                strRefedModuleFileName = moduleBuilder.m_moduleData.m_strFileName;
            }
            return new TypeToken(this.GetTypeRefNested(type, refedModule, strRefedModuleFileName));
        }

        [SecurityCritical]
        internal void Init(string strModuleName, string strFileName, int tkFile)
        {
            this.m_moduleData = new ModuleBuilderData(this, strModuleName, strFileName, tkFile);
            this.m_TypeBuilderList = new List<Type>();
        }

        [SecurityCritical]
        internal MethodToken InternalGetConstructorToken(ConstructorInfo con, bool usingRef)
        {
            int token;
            if (con == null)
            {
                throw new ArgumentNullException("con");
            }
            int str = 0;
            ConstructorBuilder builder = null;
            ConstructorOnTypeBuilderInstantiation instantiation = null;
            RuntimeConstructorInfo method = null;
            builder = con as ConstructorBuilder;
            if (builder != null)
            {
                if (!usingRef && builder.Module.Equals(this))
                {
                    return builder.GetToken();
                }
                token = this.GetTypeTokenInternal(con.ReflectedType).Token;
                str = this.GetMemberRef(con.ReflectedType.Module, token, builder.GetToken().Token);
            }
            else
            {
                instantiation = con as ConstructorOnTypeBuilderInstantiation;
                if (instantiation != null)
                {
                    if (usingRef)
                    {
                        throw new InvalidOperationException();
                    }
                    token = this.GetTypeTokenInternal(con.DeclaringType).Token;
                    str = this.GetMemberRef(con.DeclaringType.Module, token, instantiation.MetadataTokenInternal);
                }
                else if (((method = con as RuntimeConstructorInfo) != null) && !con.ReflectedType.IsArray)
                {
                    token = this.GetTypeTokenInternal(con.ReflectedType).Token;
                    str = this.GetMemberRefOfMethodInfo(token, method);
                }
                else
                {
                    int num5;
                    ParameterInfo[] parameters = con.GetParameters();
                    if (parameters == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidConstructorInfo"));
                    }
                    int length = parameters.Length;
                    Type[] parameterTypes = new Type[length];
                    Type[][] requiredParameterTypeCustomModifiers = new Type[length][];
                    Type[][] optionalParameterTypeCustomModifiers = new Type[length][];
                    for (int i = 0; i < length; i++)
                    {
                        if (parameters[i] == null)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidConstructorInfo"));
                        }
                        parameterTypes[i] = parameters[i].ParameterType;
                        requiredParameterTypeCustomModifiers[i] = parameters[i].GetRequiredCustomModifiers();
                        optionalParameterTypeCustomModifiers[i] = parameters[i].GetOptionalCustomModifiers();
                    }
                    token = this.GetTypeTokenInternal(con.ReflectedType).Token;
                    byte[] signature = SignatureHelper.GetMethodSigHelper(this, con.CallingConvention, null, null, null, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers).InternalGetSignature(out num5);
                    str = this.GetMemberRefFromSignature(token, con.Name, signature, num5);
                }
            }
            return new MethodToken(str);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.InternalModule.IsDefined(attributeType, inherit);
        }

        public override bool IsResource()
        {
            return this.InternalModule.IsResource();
        }

        public bool IsTransient()
        {
            return this.InternalModule.IsTransientInternal();
        }

        [SecurityCritical]
        internal void ModifyModuleName(string name)
        {
            this.m_moduleData.ModifyModuleName(name);
            SetModuleName(this.GetNativeHandle(), name);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern IntPtr nCreateISymWriterForDynamicModule(Module module, string filename);
        [SecurityCritical]
        internal void PreSave(string fileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
        {
            if (this.m_moduleData.m_isSaved)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("InvalidOperation_ModuleHasBeenSaved"), new object[] { this.m_moduleData.m_strModuleName }));
            }
            if (!this.m_moduleData.m_fGlobalBeenCreated && this.m_moduleData.m_fHasGlobal)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_GlobalFunctionNotBaked"));
            }
            int count = this.m_TypeBuilderList.Count;
            for (int i = 0; i < count; i++)
            {
                TypeBuilder typeBuilder;
                object obj2 = this.m_TypeBuilderList[i];
                if (obj2 is TypeBuilder)
                {
                    typeBuilder = (TypeBuilder) obj2;
                }
                else
                {
                    EnumBuilder builder2 = (EnumBuilder) obj2;
                    typeBuilder = builder2.m_typeBuilder;
                }
                if (!typeBuilder.m_hasBeenCreated && !typeBuilder.m_isHiddenType)
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("NotSupported_NotAllTypesAreBaked"), new object[] { typeBuilder.FullName }));
                }
            }
            PreSavePEFile(this.GetNativeHandle(), (int) portableExecutableKind, (int) imageFileMachine);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void PreSavePEFile(RuntimeModule module, int portableExecutableKind, int imageFileMachine);
        public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.InternalModule.ResolveField(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.InternalModule.ResolveMember(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.InternalModule.ResolveMethod(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        public override byte[] ResolveSignature(int metadataToken)
        {
            return this.InternalModule.ResolveSignature(metadataToken);
        }

        public override string ResolveString(int metadataToken)
        {
            return this.InternalModule.ResolveString(metadataToken);
        }

        public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            return this.InternalModule.ResolveType(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        [SecurityCritical]
        internal void Save(string fileName, bool isAssemblyFile, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
        {
            if (this.m_moduleData.m_embeddedRes != null)
            {
                for (ResWriterData data = this.m_moduleData.m_embeddedRes; data != null; data = data.m_nextResWriter)
                {
                    if (data.m_resWriter != null)
                    {
                        data.m_resWriter.Generate();
                    }
                    byte[] buffer = new byte[data.m_memoryStream.Length];
                    data.m_memoryStream.Flush();
                    data.m_memoryStream.Position = 0L;
                    data.m_memoryStream.Read(buffer, 0, buffer.Length);
                    AddResource(this.GetNativeHandle(), data.m_strName, buffer, buffer.Length, this.m_moduleData.FileToken, (int) data.m_attribute, (int) portableExecutableKind, (int) imageFileMachine);
                }
            }
            this.DefineNativeResource(portableExecutableKind, imageFileMachine);
            PEFileKinds kinds = isAssemblyFile ? this.ContainingAssemblyBuilder.m_assemblyData.m_peFileKind : PEFileKinds.Dll;
            SavePEFile(this.GetNativeHandle(), fileName, this.m_EntryPoint.Token, (int) kinds, isAssemblyFile);
            this.m_moduleData.m_isSaved = true;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SavePEFile(RuntimeModule module, string fileName, int entryPoint, int isExe, bool isManifestFile);
        [SecuritySafeCritical]
        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (customBuilder == null)
            {
                throw new ArgumentNullException("customBuilder");
            }
            customBuilder.CreateCustomAttribute(this, 1);
        }

        [SecuritySafeCritical, ComVisible(true)]
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
            TypeBuilder.DefineCustomAttribute(this, 1, this.GetConstructorToken(con).Token, binaryAttribute, false, false);
        }

        internal void SetEntryPoint(MethodToken entryPoint)
        {
            this.m_EntryPoint = entryPoint;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetFieldRVAContent(RuntimeModule module, int fdToken, byte[] data, int length);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void SetModuleName(RuntimeModule module, string strModuleName);
        public void SetSymCustomAttribute(string name, byte[] data)
        {
            lock (this.SyncRoot)
            {
                this.SetSymCustomAttributeNoLock(name, data);
            }
        }

        private void SetSymCustomAttributeNoLock(string name, byte[] data)
        {
            if (this.m_iSymWriter == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
            }
        }

        internal void SetSymWriter(ISymbolWriter writer)
        {
            this.m_iSymWriter = writer;
        }

        [SecuritySafeCritical]
        public void SetUserEntryPoint(MethodInfo entryPoint)
        {
            lock (this.SyncRoot)
            {
                this.SetUserEntryPointNoLock(entryPoint);
            }
        }

        [SecurityCritical]
        private void SetUserEntryPointNoLock(MethodInfo entryPoint)
        {
            if (entryPoint == null)
            {
                throw new ArgumentNullException("entryPoint");
            }
            if (this.m_iSymWriter == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
            }
            if (entryPoint.DeclaringType != null)
            {
                if (!entryPoint.Module.Equals(this))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Argument_NotInTheSameModuleBuilder"));
                }
            }
            else
            {
                MethodBuilder builder = entryPoint as MethodBuilder;
                if ((builder != null) && (builder.GetModuleBuilder() != this))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Argument_NotInTheSameModuleBuilder"));
                }
            }
            SymbolToken entryMethod = new SymbolToken(this.GetMethodTokenInternal(entryPoint).Token);
            this.m_iSymWriter.SetUserEntryPoint(entryMethod);
        }

        void _ModuleBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _ModuleBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _ModuleBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _ModuleBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        internal static string UnmangleTypeName(string typeName)
        {
            int startIndex = typeName.Length - 1;
        Label_0009:
            startIndex = typeName.LastIndexOf('+', startIndex);
            if (startIndex != -1)
            {
                bool flag = true;
                int num2 = startIndex;
                while (typeName[--num2] == '\\')
                {
                    flag = !flag;
                }
                if (!flag)
                {
                    startIndex = num2;
                    goto Label_0009;
                }
            }
            return typeName.Substring(startIndex + 1);
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.m_assemblyBuilder;
            }
        }

        internal AssemblyBuilder ContainingAssemblyBuilder
        {
            get
            {
                return this.m_assemblyBuilder;
            }
        }

        public override string FullyQualifiedName
        {
            [SecuritySafeCritical]
            get
            {
                string strFileName = this.m_moduleData.m_strFileName;
                if (strFileName == null)
                {
                    return null;
                }
                if (this.ContainingAssemblyBuilder.m_assemblyData.m_strDir != null)
                {
                    strFileName = Path.GetFullPath(Path.Combine(this.ContainingAssemblyBuilder.m_assemblyData.m_strDir, strFileName));
                }
                if ((this.ContainingAssemblyBuilder.m_assemblyData.m_strDir != null) && (strFileName != null))
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, strFileName).Demand();
                }
                return strFileName;
            }
        }

        internal InternalModuleBuilder InternalModule
        {
            get
            {
                return this.m_internalModuleBuilder;
            }
        }

        public override int MDStreamVersion
        {
            get
            {
                return this.InternalModule.MDStreamVersion;
            }
        }

        public override int MetadataToken
        {
            get
            {
                return this.InternalModule.MetadataToken;
            }
        }

        public override Guid ModuleVersionId
        {
            get
            {
                return this.InternalModule.ModuleVersionId;
            }
        }

        public override string Name
        {
            get
            {
                return this.InternalModule.Name;
            }
        }

        public override string ScopeName
        {
            get
            {
                return this.InternalModule.ScopeName;
            }
        }

        internal object SyncRoot
        {
            get
            {
                return this.ContainingAssemblyBuilder.SyncRoot;
            }
        }
    }
}


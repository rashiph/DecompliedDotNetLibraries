namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [Serializable, ForceTokenStabilization]
    internal class RuntimeModule : Module
    {
        [ForceTokenStabilization]
        private IntPtr m_pData;
        private IntPtr m_pFields;
        private IntPtr m_pGlobals;
        private IntPtr m_pRefClass;
        private RuntimeAssembly m_runtimeAssembly;
        private System.RuntimeType m_runtimeType;

        internal RuntimeModule()
        {
            throw new NotSupportedException();
        }

        private static RuntimeTypeHandle[] ConvertToTypeHandleArray(Type[] genericArguments)
        {
            if (genericArguments == null)
            {
                return null;
            }
            int length = genericArguments.Length;
            RuntimeTypeHandle[] handleArray = new RuntimeTypeHandle[length];
            for (int i = 0; i < length; i++)
            {
                Type underlyingSystemType = genericArguments[i];
                if (underlyingSystemType == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidGenericInstArray"));
                }
                underlyingSystemType = underlyingSystemType.UnderlyingSystemType;
                if (underlyingSystemType == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidGenericInstArray"));
                }
                if (!(underlyingSystemType is System.RuntimeType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidGenericInstArray"));
                }
                handleArray[i] = underlyingSystemType.GetTypeHandleInternal();
            }
            return handleArray;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return CustomAttribute.GetCustomAttributes(this, typeof(object) as System.RuntimeType);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            System.RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as System.RuntimeType;
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

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (this.RuntimeType == null)
            {
                return null;
            }
            return this.RuntimeType.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingFlags)
        {
            if (this.RuntimeType == null)
            {
                return new FieldInfo[0];
            }
            return this.RuntimeType.GetFields(bindingFlags);
        }

        [SecurityCritical]
        internal string GetFullyQualifiedName()
        {
            string s = null;
            GetFullyQualifiedName(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetFullyQualifiedName(RuntimeModule module, StringHandleOnStack retString);
        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return this.GetMethodInternal(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        internal MethodInfo GetMethodInternal(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (this.RuntimeType == null)
            {
                return null;
            }
            if (types == null)
            {
                return this.RuntimeType.GetMethod(name, bindingAttr);
            }
            return this.RuntimeType.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingFlags)
        {
            if (this.RuntimeType == null)
            {
                return new MethodInfo[0];
            }
            return this.RuntimeType.GetMethods(bindingFlags);
        }

        internal override ModuleHandle GetModuleHandle()
        {
            return new ModuleHandle(this);
        }

        internal RuntimeModule GetNativeHandle()
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
            UnitySerializationHolder.GetUnitySerializationInfo(info, 5, this.ScopeName, this.GetRuntimeAssembly());
        }

        public override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
        {
            ModuleHandle.GetPEKind(this.GetNativeHandle(), out peKind, out machine);
        }

        internal RuntimeAssembly GetRuntimeAssembly()
        {
            return this.m_runtimeAssembly;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetScopeName(RuntimeModule module, StringHandleOnStack retString);
        [SecuritySafeCritical]
        public override X509Certificate GetSignerCertificate()
        {
            byte[] o = null;
            GetSignerCertificate(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            if (o == null)
            {
                return null;
            }
            return new X509Certificate(o);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetSignerCertificate(RuntimeModule module, ObjectHandleOnStack retData);
        [SecuritySafeCritical, ComVisible(true)]
        public override Type GetType(string className, bool throwOnError, bool ignoreCase)
        {
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }
            System.RuntimeType o = null;
            GetType(this.GetNativeHandle(), className, throwOnError, ignoreCase, JitHelpers.GetObjectHandleOnStack<System.RuntimeType>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetType(RuntimeModule module, string className, bool ignoreCase, bool throwOnError, ObjectHandleOnStack type);
        [SecuritySafeCritical]
        public override Type[] GetTypes()
        {
            return GetTypes(this.GetNativeHandle());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern Type[] GetTypes(RuntimeModule module);
        [SecuritySafeCritical]
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            System.RuntimeType underlyingSystemType = attributeType.UnderlyingSystemType as System.RuntimeType;
            if (underlyingSystemType == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
            }
            return CustomAttribute.IsDefined(this, underlyingSystemType);
        }

        [SecuritySafeCritical]
        public override bool IsResource()
        {
            return IsResource(this.GetNativeHandle());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern bool IsResource(RuntimeModule module);
        [SecuritySafeCritical]
        internal bool IsTransientInternal()
        {
            return nIsTransientInternal(this.GetNativeHandle());
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall")]
        private static extern bool nIsTransientInternal(RuntimeModule module);
        [SecuritySafeCritical]
        public override unsafe FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            FieldInfo fieldInfo;
            System.Reflection.MetadataToken token = new System.Reflection.MetadataToken(metadataToken);
            if (!this.MetadataImport.IsValidToken((int) token))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }));
            }
            RuntimeTypeHandle[] typeInstantiationContext = ConvertToTypeHandleArray(genericTypeArguments);
            RuntimeTypeHandle[] methodInstantiationContext = ConvertToTypeHandleArray(genericMethodArguments);
            try
            {
                IRuntimeFieldInfo field = null;
                if (!token.IsFieldDef)
                {
                    if (!token.IsMemberRef)
                    {
                        throw new ArgumentException("metadataToken", Environment.GetResourceString("Argument_ResolveField", new object[] { token, this }));
                    }
                    if (*(((byte*) this.MetadataImport.GetMemberRefProps((int) token).Signature.ToPointer())) != 6)
                    {
                        throw new ArgumentException("metadataToken", Environment.GetResourceString("Argument_ResolveField", new object[] { token, this }));
                    }
                    field = ModuleHandle.ResolveFieldHandleInternal(this.GetNativeHandle(), (int) token, typeInstantiationContext, methodInstantiationContext);
                }
                field = ModuleHandle.ResolveFieldHandleInternal(this.GetNativeHandle(), metadataToken, typeInstantiationContext, methodInstantiationContext);
                Type approxDeclaringType = RuntimeFieldHandle.GetApproxDeclaringType(field.Value);
                if (approxDeclaringType.IsGenericType || approxDeclaringType.IsArray)
                {
                    int parentToken = ModuleHandle.GetMetadataImport(this.GetNativeHandle()).GetParentToken(metadataToken);
                    approxDeclaringType = this.ResolveType(parentToken, genericTypeArguments, genericMethodArguments);
                }
                fieldInfo = System.RuntimeType.GetFieldInfo(approxDeclaringType.GetTypeHandleInternal().GetRuntimeType(), field);
            }
            catch (MissingFieldException)
            {
                fieldInfo = this.ResolveLiteralField((int) token, genericTypeArguments, genericMethodArguments);
            }
            catch (BadImageFormatException exception)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadImageFormatExceptionResolve"), exception);
            }
            return fieldInfo;
        }

        [SecurityCritical]
        private FieldInfo ResolveLiteralField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            FieldInfo field;
            System.Reflection.MetadataToken token = new System.Reflection.MetadataToken(metadataToken);
            if (!this.MetadataImport.IsValidToken((int) token) || !token.IsFieldDef)
            {
                throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }), new object[0]));
            }
            string name = this.MetadataImport.GetName((int) token).ToString();
            int parentToken = this.MetadataImport.GetParentToken((int) token);
            Type type = this.ResolveType(parentToken, genericTypeArguments, genericMethodArguments);
            type.GetFields();
            try
            {
                field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            }
            catch
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ResolveField", new object[] { token, this }), "metadataToken");
            }
            return field;
        }

        [SecuritySafeCritical]
        public override unsafe MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            System.Reflection.MetadataToken token = new System.Reflection.MetadataToken(metadataToken);
            if (token.IsProperty)
            {
                throw new ArgumentException(Environment.GetResourceString("InvalidOperation_PropertyInfoNotAvailable"));
            }
            if (token.IsEvent)
            {
                throw new ArgumentException(Environment.GetResourceString("InvalidOperation_EventInfoNotAvailable"));
            }
            if (token.IsMethodSpec || token.IsMethodDef)
            {
                return this.ResolveMethod(metadataToken, genericTypeArguments, genericMethodArguments);
            }
            if (token.IsFieldDef)
            {
                return this.ResolveField(metadataToken, genericTypeArguments, genericMethodArguments);
            }
            if ((token.IsTypeRef || token.IsTypeDef) || token.IsTypeSpec)
            {
                return this.ResolveType(metadataToken, genericTypeArguments, genericMethodArguments);
            }
            if (!token.IsMemberRef)
            {
                throw new ArgumentException("metadataToken", Environment.GetResourceString("Argument_ResolveMember", new object[] { token, this }));
            }
            if (!this.MetadataImport.IsValidToken((int) token))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }));
            }
            if (*(((byte*) this.MetadataImport.GetMemberRefProps((int) token).Signature.ToPointer())) != 6)
            {
                return this.ResolveMethod((int) token, genericTypeArguments, genericMethodArguments);
            }
            return this.ResolveField((int) token, genericTypeArguments, genericMethodArguments);
        }

        [SecuritySafeCritical]
        public override unsafe MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            MethodBase methodBase;
            System.Reflection.MetadataToken token = new System.Reflection.MetadataToken(metadataToken);
            if (!this.MetadataImport.IsValidToken((int) token))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }));
            }
            RuntimeTypeHandle[] typeInstantiationContext = ConvertToTypeHandleArray(genericTypeArguments);
            RuntimeTypeHandle[] methodInstantiationContext = ConvertToTypeHandleArray(genericMethodArguments);
            try
            {
                if (!token.IsMethodDef && !token.IsMethodSpec)
                {
                    if (!token.IsMemberRef)
                    {
                        throw new ArgumentException("metadataToken", Environment.GetResourceString("Argument_ResolveMethod", new object[] { token, this }));
                    }
                    if (*(((byte*) this.MetadataImport.GetMemberRefProps((int) token).Signature.ToPointer())) == 6)
                    {
                        throw new ArgumentException("metadataToken", Environment.GetResourceString("Argument_ResolveMethod", new object[] { token, this }));
                    }
                }
                IRuntimeMethodInfo method = ModuleHandle.ResolveMethodHandleInternal(this.GetNativeHandle(), (int) token, typeInstantiationContext, methodInstantiationContext);
                Type declaringType = RuntimeMethodHandle.GetDeclaringType(method);
                if (declaringType.IsGenericType || declaringType.IsArray)
                {
                    System.Reflection.MetadataToken token2 = new System.Reflection.MetadataToken(this.MetadataImport.GetParentToken((int) token));
                    if (token.IsMethodSpec)
                    {
                        token2 = new System.Reflection.MetadataToken(this.MetadataImport.GetParentToken((int) token2));
                    }
                    declaringType = this.ResolveType((int) token2, genericTypeArguments, genericMethodArguments);
                }
                methodBase = System.RuntimeType.GetMethodBase(declaringType as System.RuntimeType, method);
            }
            catch (BadImageFormatException exception)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadImageFormatExceptionResolve"), exception);
            }
            return methodBase;
        }

        [SecuritySafeCritical]
        public override byte[] ResolveSignature(int metadataToken)
        {
            ConstArray memberRefProps;
            System.Reflection.MetadataToken token = new System.Reflection.MetadataToken(metadataToken);
            if (!this.MetadataImport.IsValidToken((int) token))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }));
            }
            if (((!token.IsMemberRef && !token.IsMethodDef) && (!token.IsTypeSpec && !token.IsSignature)) && !token.IsFieldDef)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }), "metadataToken");
            }
            if (token.IsMemberRef)
            {
                memberRefProps = this.MetadataImport.GetMemberRefProps(metadataToken);
            }
            else
            {
                memberRefProps = this.MetadataImport.GetSignatureFromToken(metadataToken);
            }
            byte[] buffer = new byte[memberRefProps.Length];
            for (int i = 0; i < memberRefProps.Length; i++)
            {
                buffer[i] = memberRefProps[i];
            }
            return buffer;
        }

        [SecuritySafeCritical]
        public override string ResolveString(int metadataToken)
        {
            System.Reflection.MetadataToken token = new System.Reflection.MetadataToken(metadataToken);
            if (!token.IsString)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_ResolveString"), new object[] { metadataToken, this.ToString() }));
            }
            if (!this.MetadataImport.IsValidToken((int) token))
            {
                throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }), new object[0]));
            }
            string userString = this.MetadataImport.GetUserString(metadataToken);
            if (userString == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_ResolveString"), new object[] { metadataToken, this.ToString() }));
            }
            return userString;
        }

        [SecuritySafeCritical]
        public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            Type type2;
            System.Reflection.MetadataToken token = new System.Reflection.MetadataToken(metadataToken);
            if (token.IsGlobalTypeDefToken)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ResolveModuleType", new object[] { token }), "metadataToken");
            }
            if (!this.MetadataImport.IsValidToken((int) token))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { token, this }));
            }
            if ((!token.IsTypeDef && !token.IsTypeSpec) && !token.IsTypeRef)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ResolveType", new object[] { token, this }), "metadataToken");
            }
            RuntimeTypeHandle[] typeInstantiationContext = ConvertToTypeHandleArray(genericTypeArguments);
            RuntimeTypeHandle[] methodInstantiationContext = ConvertToTypeHandleArray(genericMethodArguments);
            try
            {
                Type runtimeType = this.GetModuleHandle().ResolveTypeHandle(metadataToken, typeInstantiationContext, methodInstantiationContext).GetRuntimeType();
                if (runtimeType == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_ResolveType", new object[] { token, this }), "metadataToken");
                }
                type2 = runtimeType;
            }
            catch (BadImageFormatException exception)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadImageFormatExceptionResolve"), exception);
            }
            return type2;
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.GetRuntimeAssembly();
            }
        }

        public override string FullyQualifiedName
        {
            [SecuritySafeCritical]
            get
            {
                string fullyQualifiedName = this.GetFullyQualifiedName();
                if (fullyQualifiedName != null)
                {
                    bool flag = true;
                    try
                    {
                        Path.GetFullPathInternal(fullyQualifiedName);
                    }
                    catch (ArgumentException)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullyQualifiedName).Demand();
                    }
                }
                return fullyQualifiedName;
            }
        }

        public override int MDStreamVersion
        {
            [SecuritySafeCritical]
            get
            {
                return ModuleHandle.GetMDStreamVersion(this.GetNativeHandle());
            }
        }

        internal System.Reflection.MetadataImport MetadataImport
        {
            [SecurityCritical]
            get
            {
                return ModuleHandle.GetMetadataImport(this.GetNativeHandle());
            }
        }

        public override int MetadataToken
        {
            [SecuritySafeCritical]
            get
            {
                return ModuleHandle.GetToken(this.GetNativeHandle());
            }
        }

        public override Guid ModuleVersionId
        {
            [SecuritySafeCritical]
            get
            {
                Guid guid;
                this.MetadataImport.GetScopeProps(out guid);
                return guid;
            }
        }

        public override string Name
        {
            [SecuritySafeCritical]
            get
            {
                string fullyQualifiedName = this.GetFullyQualifiedName();
                int num = fullyQualifiedName.LastIndexOf('\\');
                if (num == -1)
                {
                    return fullyQualifiedName;
                }
                return new string(fullyQualifiedName.ToCharArray(), num + 1, (fullyQualifiedName.Length - num) - 1);
            }
        }

        internal System.RuntimeType RuntimeType
        {
            get
            {
                if (this.m_runtimeType == null)
                {
                    this.m_runtimeType = ModuleHandle.GetModuleType(this.GetNativeHandle());
                }
                return this.m_runtimeType;
            }
        }

        public override string ScopeName
        {
            [SecuritySafeCritical]
            get
            {
                string s = null;
                GetScopeName(this.GetNativeHandle(), JitHelpers.GetStringHandleOnStack(ref s));
                return s;
            }
        }
    }
}


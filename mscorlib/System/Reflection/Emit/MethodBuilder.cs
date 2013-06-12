namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.SymbolStore;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [ComDefaultInterface(typeof(_MethodBuilder)), ClassInterface(ClassInterfaceType.None), ComVisible(true), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class MethodBuilder : MethodInfo, _MethodBuilder
    {
        internal bool m_bIsBaked;
        private bool m_bIsGenMethDef;
        private bool m_bIsGlobalMethod;
        private CallingConventions m_callingConvention;
        internal bool m_canBeRuntimeImpl;
        internal TypeBuilder m_containingType;
        private MethodImplAttributes m_dwMethodImplFlags;
        private __ExceptionInstance[] m_exceptions;
        private bool m_fInitLocals;
        private MethodAttributes m_iAttributes;
        internal ILGenerator m_ilGenerator;
        private GenericTypeParameterBuilder[] m_inst;
        internal bool m_isDllImport;
        private SignatureHelper m_localSignature;
        internal LocalSymInfo m_localSymInfo;
        private int[] m_mdMethodFixups;
        private ModuleBuilder m_module;
        private int m_numExceptions;
        private Type[][] m_parameterTypeOptionalCustomModifiers;
        private Type[][] m_parameterTypeRequiredCustomModifiers;
        internal Type[] m_parameterTypes;
        private ParameterBuilder m_retParam;
        private Type m_returnType;
        private Type[] m_returnTypeOptionalCustomModifiers;
        private Type[] m_returnTypeRequiredCustomModifiers;
        private int[] m_RVAFixups;
        private SignatureHelper m_signature;
        internal string m_strName;
        private List<SymCustomAttr> m_symCustomAttrs;
        private MethodToken m_tkMethod;
        private byte[] m_ubBody;

        internal MethodBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, ModuleBuilder mod, TypeBuilder type, bool bIsGlobalMethod)
        {
            this.Init(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, mod, type, bIsGlobalMethod);
        }

        internal MethodBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, ModuleBuilder mod, TypeBuilder type, bool bIsGlobalMethod)
        {
            this.Init(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, mod, type, bIsGlobalMethod);
        }

        [SecuritySafeCritical]
        public void AddDeclarativeSecurity(SecurityAction action, PermissionSet pset)
        {
            if (pset == null)
            {
                throw new ArgumentNullException("pset");
            }
            this.ThrowIfGeneric();
            if ((!Enum.IsDefined(typeof(SecurityAction), action) || (action == SecurityAction.RequestMinimum)) || ((action == SecurityAction.RequestOptional) || (action == SecurityAction.RequestRefuse)))
            {
                throw new ArgumentOutOfRangeException("action");
            }
            this.m_containingType.ThrowIfCreated();
            byte[] blob = null;
            int cb = 0;
            if (!pset.IsEmpty())
            {
                blob = pset.EncodeXml();
                cb = blob.Length;
            }
            TypeBuilder.AddDeclarativeSecurity(this.m_module.GetNativeHandle(), this.MetadataTokenInternal, action, blob, cb);
        }

        internal int CalculateNumberOfExceptions(__ExceptionInfo[] excp)
        {
            int num = 0;
            if (excp == null)
            {
                return 0;
            }
            for (int i = 0; i < excp.Length; i++)
            {
                num += excp[i].GetNumberOfCatches();
            }
            return num;
        }

        internal void CheckContext(params Type[][] typess)
        {
            this.m_module.CheckContext(typess);
        }

        internal void CheckContext(params Type[] types)
        {
            this.m_module.CheckContext(types);
        }

        public void CreateMethodBody(byte[] il, int count)
        {
            this.ThrowIfGeneric();
            if (this.m_bIsBaked)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MethodBaked"));
            }
            this.m_containingType.ThrowIfCreated();
            if ((il != null) && ((count < 0) || (count > il.Length)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (il == null)
            {
                this.m_ubBody = null;
            }
            else
            {
                this.m_ubBody = new byte[count];
                Array.Copy(il, this.m_ubBody, count);
                this.m_bIsBaked = true;
            }
        }

        [SecurityCritical]
        internal void CreateMethodBodyHelper(ILGenerator il)
        {
            if (il == null)
            {
                throw new ArgumentNullException("il");
            }
            int num = 0;
            ModuleBuilder module = this.m_module;
            this.m_containingType.ThrowIfCreated();
            if (this.m_bIsBaked)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MethodHasBody"));
            }
            if ((il.m_methodBuilder != this) && (il.m_methodBuilder != null))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadILGeneratorUsage"));
            }
            this.ThrowIfShouldNotHaveBody();
            if (il.m_ScopeTree.m_iOpenScopeCount != 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OpenLocalVariableScope"));
            }
            this.m_ubBody = il.BakeByteArray();
            this.m_RVAFixups = il.GetRVAFixups();
            this.m_mdMethodFixups = il.GetTokenFixups();
            __ExceptionInfo[] exceptions = il.GetExceptions();
            this.m_numExceptions = this.CalculateNumberOfExceptions(exceptions);
            if (this.m_numExceptions > 0)
            {
                this.m_exceptions = new __ExceptionInstance[this.m_numExceptions];
                for (int i = 0; i < exceptions.Length; i++)
                {
                    int[] filterAddresses = exceptions[i].GetFilterAddresses();
                    int[] catchAddresses = exceptions[i].GetCatchAddresses();
                    int[] catchEndAddresses = exceptions[i].GetCatchEndAddresses();
                    Type[] catchClass = exceptions[i].GetCatchClass();
                    for (int j = 0; j < catchClass.Length; j++)
                    {
                        if (catchClass[j] != null)
                        {
                            module.GetTypeTokenInternal(catchClass[j]);
                        }
                    }
                    int numberOfCatches = exceptions[i].GetNumberOfCatches();
                    int startAddress = exceptions[i].GetStartAddress();
                    int endAddress = exceptions[i].GetEndAddress();
                    int[] exceptionTypes = exceptions[i].GetExceptionTypes();
                    for (int k = 0; k < numberOfCatches; k++)
                    {
                        int exceptionClass = 0;
                        if (catchClass[k] != null)
                        {
                            exceptionClass = module.GetTypeTokenInternal(catchClass[k]).Token;
                        }
                        switch (exceptionTypes[k])
                        {
                            case 0:
                            case 1:
                            case 4:
                                this.m_exceptions[num++] = new __ExceptionInstance(startAddress, endAddress, filterAddresses[k], catchAddresses[k], catchEndAddresses[k], exceptionTypes[k], exceptionClass);
                                break;

                            case 2:
                                this.m_exceptions[num++] = new __ExceptionInstance(startAddress, exceptions[i].GetFinallyEndAddress(), filterAddresses[k], catchAddresses[k], catchEndAddresses[k], exceptionTypes[k], exceptionClass);
                                break;
                        }
                    }
                }
            }
            this.m_bIsBaked = true;
            if (module.GetSymWriter() != null)
            {
                SymbolToken method = new SymbolToken(this.MetadataTokenInternal);
                ISymbolWriter symWriter = module.GetSymWriter();
                symWriter.OpenMethod(method);
                symWriter.OpenScope(0);
                if (this.m_symCustomAttrs != null)
                {
                    foreach (SymCustomAttr attr in this.m_symCustomAttrs)
                    {
                        module.GetSymWriter().SetSymAttribute(new SymbolToken(this.MetadataTokenInternal), attr.m_name, attr.m_data);
                    }
                }
                if (this.m_localSymInfo != null)
                {
                    this.m_localSymInfo.EmitLocalSymInfo(symWriter);
                }
                il.m_ScopeTree.EmitScopeTree(symWriter);
                il.m_LineNumberInfo.EmitLineNumberInfo(symWriter);
                symWriter.CloseScope(il.ILOffset);
                symWriter.CloseMethod();
            }
        }

        public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            if (names.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyArray"), "names");
            }
            if (this.m_inst != null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GenericParametersAlreadySet"));
            }
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == null)
                {
                    throw new ArgumentNullException("names");
                }
            }
            if (this.m_tkMethod.Token != 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MethodBuilderBaked"));
            }
            this.m_bIsGenMethDef = true;
            this.m_inst = new GenericTypeParameterBuilder[names.Length];
            for (int j = 0; j < names.Length; j++)
            {
                this.m_inst[j] = new GenericTypeParameterBuilder(new TypeBuilder(names[j], j, this));
            }
            return this.m_inst;
        }

        [SecuritySafeCritical]
        public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName)
        {
            if (position < 0)
            {
                throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_ParamSequence"));
            }
            this.ThrowIfGeneric();
            this.m_containingType.ThrowIfCreated();
            if ((position > 0) && ((this.m_parameterTypes == null) || (position > this.m_parameterTypes.Length)))
            {
                throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_ParamSequence"));
            }
            attributes &= ~ParameterAttributes.ReservedMask;
            return new ParameterBuilder(this, position, attributes, strParamName);
        }

        [SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            if (!(obj is MethodBuilder))
            {
                return false;
            }
            if (!this.m_strName.Equals(((MethodBuilder) obj).m_strName))
            {
                return false;
            }
            if (this.m_iAttributes != ((MethodBuilder) obj).m_iAttributes)
            {
                return false;
            }
            return ((MethodBuilder) obj).GetMethodSignature().Equals(this.GetMethodSignature());
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        internal byte[] GetBody()
        {
            return this.m_ubBody;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        internal __ExceptionInstance[] GetExceptionInstances()
        {
            return this.m_exceptions;
        }

        public override Type[] GetGenericArguments()
        {
            return this.m_inst;
        }

        public override MethodInfo GetGenericMethodDefinition()
        {
            if (!this.IsGenericMethod)
            {
                throw new InvalidOperationException();
            }
            return this;
        }

        public override int GetHashCode()
        {
            return this.m_strName.GetHashCode();
        }

        public ILGenerator GetILGenerator()
        {
            this.ThrowIfGeneric();
            this.ThrowIfShouldNotHaveBody();
            if (this.m_ilGenerator == null)
            {
                this.m_ilGenerator = new ILGenerator(this);
            }
            return this.m_ilGenerator;
        }

        public ILGenerator GetILGenerator(int size)
        {
            this.ThrowIfGeneric();
            this.ThrowIfShouldNotHaveBody();
            if (this.m_ilGenerator == null)
            {
                this.m_ilGenerator = new ILGenerator(this, size);
            }
            return this.m_ilGenerator;
        }

        internal SignatureHelper GetLocalsSignature()
        {
            if ((this.m_ilGenerator != null) && (this.m_ilGenerator.m_localCount != 0))
            {
                return this.m_ilGenerator.m_localSignature;
            }
            return this.m_localSignature;
        }

        internal static Type GetMethodBaseReturnType(MethodBase method)
        {
            MethodInfo info = null;
            ConstructorInfo info2 = null;
            info = method as MethodInfo;
            if (info != null)
            {
                return info.ReturnType;
            }
            info2 = method as ConstructorInfo;
            if (info2 != null)
            {
                return info2.GetReturnType();
            }
            return null;
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.m_dwMethodImplFlags;
        }

        [SecurityCritical]
        internal SignatureHelper GetMethodSignature()
        {
            if (this.m_parameterTypes == null)
            {
                this.m_parameterTypes = new Type[0];
            }
            this.m_signature = SignatureHelper.GetMethodSigHelper(this.m_module, this.m_callingConvention, (this.m_inst != null) ? this.m_inst.Length : 0, (this.m_returnType == null) ? typeof(void) : this.m_returnType, this.m_returnTypeRequiredCustomModifiers, this.m_returnTypeOptionalCustomModifiers, this.m_parameterTypes, this.m_parameterTypeRequiredCustomModifiers, this.m_parameterTypeOptionalCustomModifiers);
            return this.m_signature;
        }

        public System.Reflection.Module GetModule()
        {
            return this.GetModuleBuilder();
        }

        internal ModuleBuilder GetModuleBuilder()
        {
            return this.m_module;
        }

        internal int GetNumberOfExceptions()
        {
            return this.m_numExceptions;
        }

        public override ParameterInfo[] GetParameters()
        {
            if ((!this.m_bIsBaked || (this.m_containingType == null)) || (this.m_containingType.m_runtimeType == null))
            {
                throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_TypeNotCreated"));
            }
            return this.m_containingType.m_runtimeType.GetMethod(this.m_strName, this.m_parameterTypes).GetParameters();
        }

        internal override Type[] GetParameterTypes()
        {
            if (this.m_parameterTypes == null)
            {
                this.m_parameterTypes = new Type[0];
            }
            return this.m_parameterTypes;
        }

        internal int[] GetRVAFixups()
        {
            return this.m_RVAFixups;
        }

        [SecuritySafeCritical]
        public MethodToken GetToken()
        {
            if (this.m_tkMethod.Token != 0)
            {
                return this.m_tkMethod;
            }
            MethodBuilder builder = null;
            MethodToken tokenNoLock = new MethodToken(0);
            lock (this.m_containingType.m_listMethods)
            {
                if (this.m_tkMethod.Token != 0)
                {
                    return this.m_tkMethod;
                }
                int num = this.m_containingType.m_lastTokenizedMethod + 1;
                while (num < this.m_containingType.m_listMethods.Count)
                {
                    builder = this.m_containingType.m_listMethods[num];
                    tokenNoLock = builder.GetTokenNoLock();
                    if (builder == this)
                    {
                        break;
                    }
                    num++;
                }
                this.m_containingType.m_lastTokenizedMethod = num;
            }
            return tokenNoLock;
        }

        internal int[] GetTokenFixups()
        {
            return this.m_mdMethodFixups;
        }

        [SecurityCritical]
        private MethodToken GetTokenNoLock()
        {
            int num;
            byte[] signature = this.GetMethodSignature().InternalGetSignature(out num);
            int str = TypeBuilder.DefineMethod(this.m_module.GetNativeHandle(), this.m_containingType.MetadataTokenInternal, this.m_strName, signature, num, this.Attributes);
            this.m_tkMethod = new MethodToken(str);
            if (this.m_inst != null)
            {
                foreach (GenericTypeParameterBuilder builder in this.m_inst)
                {
                    if (!builder.m_type.IsCreated())
                    {
                        builder.m_type.CreateType();
                    }
                }
            }
            TypeBuilder.SetMethodImpl(this.m_module.GetNativeHandle(), str, this.m_dwMethodImplFlags);
            return this.m_tkMethod;
        }

        internal TypeBuilder GetTypeBuilder()
        {
            return this.m_containingType;
        }

        private void Init(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, ModuleBuilder mod, TypeBuilder type, bool bIsGlobalMethod)
        {
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
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalName"), "name");
            }
            if (mod == null)
            {
                throw new ArgumentNullException("mod");
            }
            if (parameterTypes != null)
            {
                foreach (Type type2 in parameterTypes)
                {
                    if (type2 == null)
                    {
                        throw new ArgumentNullException("parameterTypes");
                    }
                }
            }
            this.m_strName = name;
            this.m_module = mod;
            this.m_containingType = type;
            this.m_localSignature = SignatureHelper.GetLocalVarSigHelper(mod);
            this.m_returnType = returnType;
            if ((attributes & MethodAttributes.Static) == MethodAttributes.PrivateScope)
            {
                callingConvention |= CallingConventions.HasThis;
            }
            else if ((attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NoStaticVirtual"));
            }
            if ((((attributes & MethodAttributes.SpecialName) != MethodAttributes.SpecialName) && ((type.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask)) && (((attributes & (MethodAttributes.Abstract | MethodAttributes.Virtual)) != (MethodAttributes.Abstract | MethodAttributes.Virtual)) && ((attributes & MethodAttributes.Static) == MethodAttributes.PrivateScope)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadAttributeOnInterfaceMethod"));
            }
            this.m_callingConvention = callingConvention;
            if (parameterTypes != null)
            {
                this.m_parameterTypes = new Type[parameterTypes.Length];
                Array.Copy(parameterTypes, this.m_parameterTypes, parameterTypes.Length);
            }
            else
            {
                this.m_parameterTypes = null;
            }
            this.m_returnTypeRequiredCustomModifiers = returnTypeRequiredCustomModifiers;
            this.m_returnTypeOptionalCustomModifiers = returnTypeOptionalCustomModifiers;
            this.m_parameterTypeRequiredCustomModifiers = parameterTypeRequiredCustomModifiers;
            this.m_parameterTypeOptionalCustomModifiers = parameterTypeOptionalCustomModifiers;
            this.m_iAttributes = attributes;
            this.m_bIsGlobalMethod = bIsGlobalMethod;
            this.m_bIsBaked = false;
            this.m_fInitLocals = true;
            this.m_localSymInfo = new LocalSymInfo();
            this.m_ubBody = null;
            this.m_ilGenerator = null;
            this.m_dwMethodImplFlags = MethodImplAttributes.IL;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
        }

        private bool IsKnownCA(ConstructorInfo con)
        {
            Type declaringType = con.DeclaringType;
            return ((declaringType == typeof(MethodImplAttribute)) || (declaringType == typeof(DllImportAttribute)));
        }

        internal bool IsTypeCreated()
        {
            return ((this.m_containingType != null) && this.m_containingType.m_hasBeenCreated);
        }

        public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            return MethodBuilderInstantiation.MakeGenericMethod(this, typeArguments);
        }

        private void ParseCA(ConstructorInfo con, byte[] blob)
        {
            Type declaringType = con.DeclaringType;
            if (declaringType == typeof(MethodImplAttribute))
            {
                this.m_canBeRuntimeImpl = true;
            }
            else if (declaringType == typeof(DllImportAttribute))
            {
                this.m_canBeRuntimeImpl = true;
                this.m_isDllImport = true;
            }
        }

        internal void ReleaseBakedStructures()
        {
            if (this.m_bIsBaked)
            {
                this.m_ubBody = null;
                this.m_localSymInfo = null;
                this.m_RVAFixups = null;
                this.m_mdMethodFixups = null;
                this.m_exceptions = null;
            }
        }

        [SecuritySafeCritical]
        public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
        {
            if (customBuilder == null)
            {
                throw new ArgumentNullException("customBuilder");
            }
            this.ThrowIfGeneric();
            customBuilder.CreateCustomAttribute(this.m_module, this.MetadataTokenInternal);
            if (this.IsKnownCA(customBuilder.m_con))
            {
                this.ParseCA(customBuilder.m_con, customBuilder.m_blob);
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
            this.ThrowIfGeneric();
            TypeBuilder.DefineCustomAttribute(this.m_module, this.MetadataTokenInternal, this.m_module.GetConstructorToken(con).Token, binaryAttribute, false, false);
            if (this.IsKnownCA(con))
            {
                this.ParseCA(con, binaryAttribute);
            }
        }

        [SecuritySafeCritical]
        public void SetImplementationFlags(MethodImplAttributes attributes)
        {
            this.ThrowIfGeneric();
            this.m_containingType.ThrowIfCreated();
            this.m_dwMethodImplFlags = attributes;
            this.m_canBeRuntimeImpl = true;
            TypeBuilder.SetMethodImpl(this.m_module.GetNativeHandle(), this.MetadataTokenInternal, attributes);
        }

        [Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead. http://go.microsoft.com/fwlink/?linkid=14202"), SecuritySafeCritical]
        public void SetMarshal(UnmanagedMarshal unmanagedMarshal)
        {
            this.ThrowIfGeneric();
            this.m_containingType.ThrowIfCreated();
            if (this.m_retParam == null)
            {
                this.m_retParam = new ParameterBuilder(this, 0, ParameterAttributes.None, null);
            }
            this.m_retParam.SetMarshal(unmanagedMarshal);
        }

        [SecuritySafeCritical]
        public void SetParameters(params Type[] parameterTypes)
        {
            this.CheckContext(parameterTypes);
            this.SetSignature(null, null, null, parameterTypes, null, null);
        }

        [SecuritySafeCritical]
        public void SetReturnType(Type returnType)
        {
            this.CheckContext(new Type[] { returnType });
            this.SetSignature(returnType, null, null, null, null, null);
        }

        [SecuritySafeCritical]
        public void SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
        {
            if (this.m_tkMethod.Token == 0)
            {
                this.CheckContext(new Type[] { returnType });
                this.CheckContext(new Type[][] { returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes });
                this.CheckContext(parameterTypeRequiredCustomModifiers);
                this.CheckContext(parameterTypeOptionalCustomModifiers);
                this.ThrowIfGeneric();
                if (returnType != null)
                {
                    this.m_returnType = returnType;
                }
                if (parameterTypes != null)
                {
                    this.m_parameterTypes = new Type[parameterTypes.Length];
                    Array.Copy(parameterTypes, this.m_parameterTypes, parameterTypes.Length);
                }
                this.m_returnTypeRequiredCustomModifiers = returnTypeRequiredCustomModifiers;
                this.m_returnTypeOptionalCustomModifiers = returnTypeOptionalCustomModifiers;
                this.m_parameterTypeRequiredCustomModifiers = parameterTypeRequiredCustomModifiers;
                this.m_parameterTypeOptionalCustomModifiers = parameterTypeOptionalCustomModifiers;
            }
        }

        public void SetSymCustomAttribute(string name, byte[] data)
        {
            this.ThrowIfGeneric();
            this.m_containingType.ThrowIfCreated();
            if (this.m_module.GetSymWriter() == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
            }
            if (this.m_symCustomAttrs == null)
            {
                this.m_symCustomAttrs = new List<SymCustomAttr>();
            }
            this.m_symCustomAttrs.Add(new SymCustomAttr(name, data));
        }

        internal void SetToken(MethodToken token)
        {
            this.m_tkMethod = token;
        }

        void _MethodBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _MethodBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _MethodBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        internal void ThrowIfGeneric()
        {
            if (this.IsGenericMethod && !this.IsGenericMethodDefinition)
            {
                throw new InvalidOperationException();
            }
        }

        private void ThrowIfShouldNotHaveBody()
        {
            if ((((this.m_dwMethodImplFlags & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL) || ((this.m_dwMethodImplFlags & MethodImplAttributes.ManagedMask) != MethodImplAttributes.IL)) || (((this.m_iAttributes & MethodAttributes.PinvokeImpl) != MethodAttributes.PrivateScope) || this.m_isDllImport))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ShouldNotHaveMethodBody"));
            }
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x3e8);
            builder.Append("Name: " + this.m_strName + " " + Environment.NewLine);
            builder.Append("Attributes: " + ((int) this.m_iAttributes) + Environment.NewLine);
            builder.Append("Method Signature: " + this.GetMethodSignature() + Environment.NewLine);
            builder.Append(Environment.NewLine);
            return builder.ToString();
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.m_iAttributes;
            }
        }

        public override CallingConventions CallingConvention
        {
            get
            {
                return this.m_callingConvention;
            }
        }

        public override bool ContainsGenericParameters
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override Type DeclaringType
        {
            get
            {
                if (this.m_containingType.m_isHiddenGlobalType)
                {
                    return null;
                }
                return this.m_containingType;
            }
        }

        public bool InitLocals
        {
            get
            {
                this.ThrowIfGeneric();
                return this.m_fInitLocals;
            }
            set
            {
                this.ThrowIfGeneric();
                this.m_fInitLocals = value;
            }
        }

        public override bool IsGenericMethod
        {
            get
            {
                return (this.m_inst != null);
            }
        }

        public override bool IsGenericMethodDefinition
        {
            get
            {
                return this.m_bIsGenMethDef;
            }
        }

        public override bool IsSecurityCritical
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
            }
        }

        public override bool IsSecuritySafeCritical
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
            }
        }

        public override bool IsSecurityTransparent
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
            }
        }

        internal int MetadataTokenInternal
        {
            get
            {
                return this.GetToken().Token;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_containingType.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_strName;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.DeclaringType;
            }
        }

        public override ParameterInfo ReturnParameter
        {
            get
            {
                if ((!this.m_bIsBaked || (this.m_containingType == null)) || (this.m_containingType.m_runtimeType == null))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TypeNotCreated"));
                }
                return this.m_containingType.m_runtimeType.GetMethod(this.m_strName, this.m_parameterTypes).ReturnParameter;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.m_returnType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return null;
            }
        }

        public string Signature
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetMethodSignature().ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SymCustomAttr
        {
            public string m_name;
            public byte[] m_data;
            public SymCustomAttr(string name, byte[] data)
            {
                this.m_name = name;
                this.m_data = data;
            }
        }
    }
}


namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [ComVisible(true)]
    public sealed class DynamicMethod : MethodInfo
    {
        internal CompressedStack m_creationContext;
        private DynamicILInfo m_DynamicILInfo;
        private RTDynamicMethod m_dynMethod;
        private bool m_fInitLocals;
        private DynamicILGenerator m_ilGenerator;
        internal IRuntimeMethodInfo m_methodHandle;
        internal RuntimeModule m_module;
        private RuntimeType[] m_parameterTypes;
        internal DynamicResolver m_resolver;
        internal bool m_restrictedSkipVisibility;
        private RuntimeType m_returnType;
        internal bool m_skipVisibility;
        internal RuntimeType m_typeOwner;
        private static InternalModuleBuilder s_anonymouslyHostedDynamicMethodsModule;
        private static readonly object s_anonymouslyHostedDynamicMethodsModuleLock = new object();

        private DynamicMethod()
        {
        }

        [SecuritySafeCritical]
        public DynamicMethod(string name, Type returnType, Type[] parameterTypes)
        {
            this.Init(name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, null, null, false, true);
        }

        [SecuritySafeCritical]
        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, bool restrictedSkipVisibility)
        {
            this.Init(name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, null, null, restrictedSkipVisibility, true);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, System.Reflection.Module m)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            PerformSecurityCheck(m, ref lookForMyCaller, false);
            this.Init(name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, null, m, false, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            PerformSecurityCheck(owner, ref lookForMyCaller, false);
            this.Init(name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, owner, null, false, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, System.Reflection.Module m, bool skipVisibility)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            PerformSecurityCheck(m, ref lookForMyCaller, skipVisibility);
            this.Init(name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, null, m, skipVisibility, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            PerformSecurityCheck(owner, ref lookForMyCaller, skipVisibility);
            this.Init(name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, owner, null, skipVisibility, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public DynamicMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, System.Reflection.Module m, bool skipVisibility)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            PerformSecurityCheck(m, ref lookForMyCaller, skipVisibility);
            this.Init(name, attributes, callingConvention, returnType, parameterTypes, null, m, skipVisibility, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public DynamicMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            PerformSecurityCheck(owner, ref lookForMyCaller, skipVisibility);
            this.Init(name, attributes, callingConvention, returnType, parameterTypes, owner, null, skipVisibility, false);
        }

        private static void CheckConsistency(MethodAttributes attributes, CallingConventions callingConvention)
        {
            if ((attributes & ~MethodAttributes.MemberAccessMask) != MethodAttributes.Static)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
            }
            if ((attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
            }
            if ((callingConvention != CallingConventions.Standard) && (callingConvention != CallingConventions.VarArgs))
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
            }
            if (callingConvention == CallingConventions.VarArgs)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
            }
        }

        [ComVisible(true), SecuritySafeCritical]
        public Delegate CreateDelegate(Type delegateType)
        {
            if (this.m_restrictedSkipVisibility)
            {
                this.GetMethodDescriptor();
                RuntimeHelpers._CompileMethod(this.m_methodHandle);
            }
            MulticastDelegate delegate2 = (MulticastDelegate) Delegate.CreateDelegate(delegateType, null, this.GetMethodDescriptor());
            delegate2.StoreDynamicMethod(this.GetMethodInfo());
            return delegate2;
        }

        [ComVisible(true), SecuritySafeCritical]
        public Delegate CreateDelegate(Type delegateType, object target)
        {
            if (this.m_restrictedSkipVisibility)
            {
                this.GetMethodDescriptor();
                RuntimeHelpers._CompileMethod(this.m_methodHandle);
            }
            MulticastDelegate delegate2 = (MulticastDelegate) Delegate.CreateDelegate(delegateType, target, this.GetMethodDescriptor());
            delegate2.StoreDynamicMethod(this.GetMethodInfo());
            return delegate2;
        }

        public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string parameterName)
        {
            if ((position < 0) || (position > this.m_parameterTypes.Length))
            {
                throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_ParamSequence"));
            }
            position--;
            if (position >= 0)
            {
                ParameterInfo[] infoArray = this.m_dynMethod.LoadParameters();
                infoArray[position].SetName(parameterName);
                infoArray[position].SetAttributes(attributes);
            }
            return null;
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.m_dynMethod.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.m_dynMethod.GetCustomAttributes(attributeType, inherit);
        }

        [SecuritySafeCritical]
        public DynamicILInfo GetDynamicILInfo()
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            if (this.m_DynamicILInfo != null)
            {
                return this.m_DynamicILInfo;
            }
            return this.GetDynamicILInfo(new DynamicScope());
        }

        [SecurityCritical]
        internal DynamicILInfo GetDynamicILInfo(DynamicScope scope)
        {
            if (this.m_DynamicILInfo == null)
            {
                byte[] signature = SignatureHelper.GetMethodSigHelper(null, this.CallingConvention, this.ReturnType, null, null, this.m_parameterTypes, null, null).GetSignature(true);
                this.m_DynamicILInfo = new DynamicILInfo(scope, this, signature);
            }
            return this.m_DynamicILInfo;
        }

        [SecurityCritical]
        private static RuntimeModule GetDynamicMethodsModule()
        {
            if (s_anonymouslyHostedDynamicMethodsModule == null)
            {
                lock (s_anonymouslyHostedDynamicMethodsModuleLock)
                {
                    if (s_anonymouslyHostedDynamicMethodsModule != null)
                    {
                        return s_anonymouslyHostedDynamicMethodsModule;
                    }
                    CustomAttributeBuilder builder = new CustomAttributeBuilder(typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes), new object[0]);
                    List<CustomAttributeBuilder> unsafeAssemblyAttributes = new List<CustomAttributeBuilder> {
                        builder
                    };
                    CustomAttributeBuilder item = new CustomAttributeBuilder(typeof(SecurityRulesAttribute).GetConstructor(new Type[] { typeof(SecurityRuleSet) }), new object[] { SecurityRuleSet.Level1 });
                    unsafeAssemblyAttributes.Add(item);
                    AssemblyName name = new AssemblyName("Anonymously Hosted DynamicMethods Assembly");
                    StackCrawlMark lookForMe = StackCrawlMark.LookForMe;
                    AssemblyBuilder builder3 = AssemblyBuilder.InternalDefineDynamicAssembly(name, AssemblyBuilderAccess.Run, null, null, null, null, null, ref lookForMe, unsafeAssemblyAttributes, SecurityContextSource.CurrentAssembly);
                    AppDomain.PublishAnonymouslyHostedDynamicMethodsAssembly(builder3.GetNativeHandle());
                    s_anonymouslyHostedDynamicMethodsModule = (InternalModuleBuilder) builder3.ManifestModule;
                }
            }
            return s_anonymouslyHostedDynamicMethodsModule;
        }

        public ILGenerator GetILGenerator()
        {
            return this.GetILGenerator(0x40);
        }

        [SecuritySafeCritical]
        public ILGenerator GetILGenerator(int streamSize)
        {
            if (this.m_ilGenerator == null)
            {
                byte[] signature = SignatureHelper.GetMethodSigHelper(null, this.CallingConvention, this.ReturnType, null, null, this.m_parameterTypes, null, null).GetSignature(true);
                this.m_ilGenerator = new DynamicILGenerator(this, signature, streamSize);
            }
            return this.m_ilGenerator;
        }

        [SecurityCritical]
        internal RuntimeMethodHandle GetMethodDescriptor()
        {
            if (this.m_methodHandle == null)
            {
                lock (this)
                {
                    if (this.m_methodHandle == null)
                    {
                        if (this.m_DynamicILInfo != null)
                        {
                            this.m_DynamicILInfo.GetCallableMethod(this.m_module, this);
                        }
                        else
                        {
                            if ((this.m_ilGenerator == null) || (this.m_ilGenerator.ILOffset == 0))
                            {
                                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadEmptyMethodBody", new object[] { this.Name }));
                            }
                            this.m_ilGenerator.GetCallableMethod(this.m_module, this);
                        }
                    }
                }
            }
            return new RuntimeMethodHandle(this.m_methodHandle);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return this.m_dynMethod.GetMethodImplementationFlags();
        }

        internal MethodInfo GetMethodInfo()
        {
            return this.m_dynMethod;
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.m_dynMethod.GetParameters();
        }

        [SecurityCritical]
        private void Init(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] signature, Type owner, System.Reflection.Module m, bool skipVisibility, bool transparentMethod)
        {
            CheckConsistency(attributes, callingConvention);
            if (signature != null)
            {
                this.m_parameterTypes = new RuntimeType[signature.Length];
                for (int i = 0; i < signature.Length; i++)
                {
                    if (signature[i] == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_InvalidTypeInSignature"));
                    }
                    this.m_parameterTypes[i] = signature[i].UnderlyingSystemType as RuntimeType;
                    if (((this.m_parameterTypes[i] == null) || !this.m_parameterTypes[i].IsRuntimeType) || (this.m_parameterTypes[i] == typeof(void)))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_InvalidTypeInSignature"));
                    }
                }
            }
            else
            {
                this.m_parameterTypes = new RuntimeType[0];
            }
            this.m_returnType = (returnType == null) ? ((RuntimeType) typeof(void)) : (returnType.UnderlyingSystemType as RuntimeType);
            if (((this.m_returnType == null) || !this.m_returnType.IsRuntimeType) || this.m_returnType.IsByRef)
            {
                throw new NotSupportedException(Environment.GetResourceString("Arg_InvalidTypeInRetType"));
            }
            if (transparentMethod)
            {
                this.m_module = GetDynamicMethodsModule();
                if (skipVisibility)
                {
                    this.m_restrictedSkipVisibility = true;
                    this.m_creationContext = CompressedStack.Capture();
                }
            }
            else
            {
                if (m != null)
                {
                    this.m_module = m.ModuleHandle.GetRuntimeModule();
                }
                else if (((owner != null) && (owner.UnderlyingSystemType != null)) && owner.UnderlyingSystemType.IsRuntimeType)
                {
                    this.m_typeOwner = owner.UnderlyingSystemType.TypeHandle.GetRuntimeType();
                    if ((this.m_typeOwner.HasElementType || this.m_typeOwner.ContainsGenericParameters) || (this.m_typeOwner.IsGenericParameter || this.m_typeOwner.IsInterface))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidTypeForDynamicMethod"));
                    }
                    this.m_module = (RuntimeModule) this.m_typeOwner.Module;
                }
                this.m_skipVisibility = skipVisibility;
            }
            this.m_ilGenerator = null;
            this.m_fInitLocals = true;
            this.m_methodHandle = null;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.m_dynMethod = new RTDynamicMethod(this, name, attributes, callingConvention);
        }

        [SecuritySafeCritical]
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            if ((this.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_CallToVarArg"));
            }
            this.GetMethodDescriptor();
            Signature sig = new Signature(this.m_methodHandle, this.m_parameterTypes, this.m_returnType, this.CallingConvention);
            int length = sig.Arguments.Length;
            int num2 = (parameters != null) ? parameters.Length : 0;
            if (length != num2)
            {
                throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
            }
            object obj2 = null;
            if (num2 > 0)
            {
                object[] arguments = base.CheckArguments(parameters, binder, invokeAttr, culture, sig);
                obj2 = RuntimeMethodHandle.InvokeMethodFast(this.m_methodHandle, null, arguments, sig, this.Attributes, null);
                for (int i = 0; i < num2; i++)
                {
                    parameters[i] = arguments[i];
                }
            }
            else
            {
                obj2 = RuntimeMethodHandle.InvokeMethodFast(this.m_methodHandle, null, null, sig, this.Attributes, null);
            }
            GC.KeepAlive(this);
            return obj2;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.m_dynMethod.IsDefined(attributeType, inherit);
        }

        [SecurityCritical]
        private static void PerformSecurityCheck(System.Reflection.Module m, ref StackCrawlMark stackMark, bool skipVisibility)
        {
            RuntimeModule internalModule;
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }
            ModuleBuilder builder = m as ModuleBuilder;
            if (builder != null)
            {
                internalModule = builder.InternalModule;
            }
            else
            {
                internalModule = m as RuntimeModule;
            }
            if (internalModule == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeModule"), "m");
            }
            if (internalModule == s_anonymouslyHostedDynamicMethodsModule)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"), "m");
            }
            if (skipVisibility)
            {
                new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
            }
            RuntimeType callerType = RuntimeMethodHandle.GetCallerType(ref stackMark);
            if (m.Assembly != callerType.Assembly)
            {
                CodeAccessSecurityEngine.ReflectionTargetDemandHelper(PermissionType.SecurityControlEvidence, m.Assembly.PermissionSet);
            }
        }

        [SecurityCritical]
        private static void PerformSecurityCheck(Type owner, ref StackCrawlMark stackMark, bool skipVisibility)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            RuntimeType underlyingSystemType = owner as RuntimeType;
            if (underlyingSystemType == null)
            {
                underlyingSystemType = owner.UnderlyingSystemType as RuntimeType;
            }
            if (underlyingSystemType == null)
            {
                throw new ArgumentNullException("owner", Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            RuntimeType callerType = RuntimeMethodHandle.GetCallerType(ref stackMark);
            if (skipVisibility)
            {
                new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
            }
            else if (callerType != underlyingSystemType)
            {
                new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
            }
            if (underlyingSystemType.Assembly != callerType.Assembly)
            {
                CodeAccessSecurityEngine.ReflectionTargetDemandHelper(PermissionType.SecurityControlEvidence, owner.Assembly.PermissionSet);
            }
        }

        public override string ToString()
        {
            return this.m_dynMethod.ToString();
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.m_dynMethod.Attributes;
            }
        }

        public override CallingConventions CallingConvention
        {
            get
            {
                return this.m_dynMethod.CallingConvention;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.m_dynMethod.DeclaringType;
            }
        }

        public bool InitLocals
        {
            get
            {
                return this.m_fInitLocals;
            }
            set
            {
                this.m_fInitLocals = value;
            }
        }

        public override bool IsSecurityCritical
        {
            [SecuritySafeCritical]
            get
            {
                this.GetMethodDescriptor();
                return RuntimeMethodHandle.IsSecurityCritical(this.m_methodHandle);
            }
        }

        public override bool IsSecuritySafeCritical
        {
            [SecuritySafeCritical]
            get
            {
                this.GetMethodDescriptor();
                return RuntimeMethodHandle.IsSecuritySafeCritical(this.m_methodHandle);
            }
        }

        public override bool IsSecurityTransparent
        {
            [SecuritySafeCritical]
            get
            {
                this.GetMethodDescriptor();
                return RuntimeMethodHandle.IsSecurityTransparent(this.m_methodHandle);
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.m_dynMethod.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.m_dynMethod.Name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.m_dynMethod.ReflectedType;
            }
        }

        public override ParameterInfo ReturnParameter
        {
            get
            {
                return this.m_dynMethod.ReturnParameter;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return this.m_dynMethod.ReturnType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return this.m_dynMethod.ReturnTypeCustomAttributes;
            }
        }

        internal class RTDynamicMethod : MethodInfo
        {
            private MethodAttributes m_attributes;
            private CallingConventions m_callingConvention;
            private string m_name;
            internal DynamicMethod m_owner;
            private ParameterInfo[] m_parameters;

            private RTDynamicMethod()
            {
            }

            internal RTDynamicMethod(DynamicMethod owner, string name, MethodAttributes attributes, CallingConventions callingConvention)
            {
                this.m_owner = owner;
                this.m_name = name;
                this.m_attributes = attributes;
                this.m_callingConvention = callingConvention;
            }

            public override MethodInfo GetBaseDefinition()
            {
                return this;
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return new object[] { new MethodImplAttribute(this.GetMethodImplementationFlags()) };
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                if (attributeType == null)
                {
                    throw new ArgumentNullException("attributeType");
                }
                if (attributeType.IsAssignableFrom(typeof(MethodImplAttribute)))
                {
                    return new object[] { new MethodImplAttribute(this.GetMethodImplementationFlags()) };
                }
                return new object[0];
            }

            private ICustomAttributeProvider GetEmptyCAHolder()
            {
                return new EmptyCAHolder();
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return MethodImplAttributes.NoInlining;
            }

            public override ParameterInfo[] GetParameters()
            {
                ParameterInfo[] sourceArray = this.LoadParameters();
                ParameterInfo[] destinationArray = new ParameterInfo[sourceArray.Length];
                Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                return destinationArray;
            }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "this");
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                if (attributeType == null)
                {
                    throw new ArgumentNullException("attributeType");
                }
                return attributeType.IsAssignableFrom(typeof(MethodImplAttribute));
            }

            internal ParameterInfo[] LoadParameters()
            {
                if (this.m_parameters == null)
                {
                    Type[] parameterTypes = this.m_owner.m_parameterTypes;
                    ParameterInfo[] infoArray = new ParameterInfo[parameterTypes.Length];
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        infoArray[i] = new RuntimeParameterInfo(this, null, parameterTypes[i], i);
                    }
                    if (this.m_parameters == null)
                    {
                        this.m_parameters = infoArray;
                    }
                }
                return this.m_parameters;
            }

            [SecuritySafeCritical]
            public override string ToString()
            {
                return (this.ReturnType.SigToString() + " " + this.ConstructName());
            }

            public override MethodAttributes Attributes
            {
                get
                {
                    return this.m_attributes;
                }
            }

            public override CallingConventions CallingConvention
            {
                get
                {
                    return this.m_callingConvention;
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return null;
                }
            }

            public override bool IsSecurityCritical
            {
                get
                {
                    return this.m_owner.IsSecurityCritical;
                }
            }

            public override bool IsSecuritySafeCritical
            {
                get
                {
                    return this.m_owner.IsSecuritySafeCritical;
                }
            }

            public override bool IsSecurityTransparent
            {
                get
                {
                    return this.m_owner.IsSecurityTransparent;
                }
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
                }
            }

            public override System.Reflection.Module Module
            {
                [SecuritySafeCritical]
                get
                {
                    return this.m_owner.m_module;
                }
            }

            public override string Name
            {
                get
                {
                    return this.m_name;
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return null;
                }
            }

            public override ParameterInfo ReturnParameter
            {
                get
                {
                    return null;
                }
            }

            public override Type ReturnType
            {
                get
                {
                    return this.m_owner.m_returnType;
                }
            }

            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get
                {
                    return this.GetEmptyCAHolder();
                }
            }

            private class EmptyCAHolder : ICustomAttributeProvider
            {
                internal EmptyCAHolder()
                {
                }

                object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit)
                {
                    return new object[0];
                }

                object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit)
                {
                    return new object[0];
                }

                bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit)
                {
                    return false;
                }
            }
        }
    }
}


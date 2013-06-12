namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection.Cache;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable]
    internal sealed class RuntimeConstructorInfo : ConstructorInfo, ISerializable, IRuntimeMethodInfo
    {
        private object _empty1;
        private object _empty2;
        private object _empty3;
        private System.Reflection.BindingFlags m_bindingFlags;
        private InternalCache m_cachedData;
        private RuntimeType m_declaringType;
        private IntPtr m_handle;
        private INVOCATION_FLAGS m_invocationFlags;
        private MethodAttributes m_methodAttributes;
        private ParameterInfo[] m_parameters;
        private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;
        private System.Signature m_signature;
        private string m_toString;

        [SecurityCritical]
        internal RuntimeConstructorInfo(RuntimeMethodHandleInternal handle, RuntimeType declaringType, RuntimeType.RuntimeTypeCache reflectedTypeCache, MethodAttributes methodAttributes, System.Reflection.BindingFlags bindingFlags)
        {
            this.m_bindingFlags = bindingFlags;
            this.m_reflectedTypeCache = reflectedTypeCache;
            this.m_declaringType = declaringType;
            this.m_handle = handle.Value;
            this.m_methodAttributes = methodAttributes;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal override bool CacheEquals(object o)
        {
            RuntimeConstructorInfo info = o as RuntimeConstructorInfo;
            if (info == null)
            {
                return false;
            }
            return (info.m_handle == this.m_handle);
        }

        internal static void CheckCanCreateInstance(Type declaringType, bool isVarArg)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }
            if (declaringType is ReflectionOnlyType)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyInvoke"));
            }
            if (declaringType.IsInterface)
            {
                throw new MemberAccessException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Acc_CreateInterfaceEx"), new object[] { declaringType }));
            }
            if (declaringType.IsAbstract)
            {
                throw new MemberAccessException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Acc_CreateAbstEx"), new object[] { declaringType }));
            }
            if (declaringType.GetRootElementType() == typeof(ArgIterator))
            {
                throw new NotSupportedException();
            }
            if (isVarArg)
            {
                throw new NotSupportedException();
            }
            if (declaringType.ContainsGenericParameters)
            {
                throw new MemberAccessException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Acc_CreateGenericEx"), new object[] { declaringType }));
            }
            if (declaringType == typeof(void))
            {
                throw new MemberAccessException(Environment.GetResourceString("Access_Void"));
            }
        }

        private void CheckConsistency(object target)
        {
            if (((target != null) || !base.IsStatic) && !this.m_declaringType.IsInstanceOfType(target))
            {
                if (target == null)
                {
                    throw new TargetException(Environment.GetResourceString("RFLCT.Targ_StatMethReqTarg"));
                }
                throw new TargetException(Environment.GetResourceString("RFLCT.Targ_ITargMismatch"));
            }
        }

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

        [SecuritySafeCritical, ReflectionPermission(SecurityAction.Demand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public override MethodBody GetMethodBody()
        {
            MethodBody methodBody = RuntimeMethodHandle.GetMethodBody(this, this.m_reflectedTypeCache.RuntimeType);
            if (methodBody != null)
            {
                methodBody.m_methodBase = this;
            }
            return methodBody;
        }

        internal RuntimeMethodHandle GetMethodHandle()
        {
            return new RuntimeMethodHandle(this);
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return RuntimeMethodHandle.GetImplAttributes(this);
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            MemberInfoSerializationHolder.GetSerializationInfo(info, this.Name, this.ReflectedTypeInternal, this.ToString(), MemberTypes.Constructor);
        }

        public override ParameterInfo[] GetParameters()
        {
            ParameterInfo[] parametersNoCopy = this.GetParametersNoCopy();
            if (parametersNoCopy.Length == 0)
            {
                return parametersNoCopy;
            }
            ParameterInfo[] destinationArray = new ParameterInfo[parametersNoCopy.Length];
            Array.Copy(parametersNoCopy, destinationArray, parametersNoCopy.Length);
            return destinationArray;
        }

        [SecuritySafeCritical]
        internal override ParameterInfo[] GetParametersNoCopy()
        {
            if (this.m_parameters == null)
            {
                this.m_parameters = RuntimeParameterInfo.GetParameters(this, this, this.Signature);
            }
            return this.m_parameters;
        }

        internal override Type GetReturnType()
        {
            return this.Signature.ReturnType;
        }

        [SecuritySafeCritical]
        internal RuntimeModule GetRuntimeModule()
        {
            return RuntimeTypeHandle.GetModule(this.m_declaringType);
        }

        [DebuggerStepThrough, SecuritySafeCritical, DebuggerHidden]
        public override object Invoke(System.Reflection.BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            INVOCATION_FLAGS invocationFlags = this.InvocationFlags;
            RuntimeTypeHandle typeHandle = this.m_declaringType.TypeHandle;
            if ((invocationFlags & (INVOCATION_FLAGS.INVOCATION_FLAGS_CONTAINS_STACK_POINTERS | INVOCATION_FLAGS.INVOCATION_FLAGS_NO_CTOR_INVOKE | INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE)) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                this.ThrowNoInvokeException();
            }
            if ((invocationFlags & (INVOCATION_FLAGS.INVOCATION_FLAGS_FIELD_SPECIAL_CAST | INVOCATION_FLAGS.INVOCATION_FLAGS_IS_DELEGATE_CTOR | INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY)) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_FIELD_SPECIAL_CAST) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    CodeAccessPermission.Demand(PermissionType.ReflectionMemberAccess);
                }
                if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    RuntimeMethodHandle.PerformSecurityCheck(null, this, this.m_declaringType, (uint) (this.m_invocationFlags | INVOCATION_FLAGS.INVOCATION_FLAGS_CONSTRUCTOR_INVOKE));
                }
                if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_IS_DELEGATE_CTOR) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                }
            }
            int length = this.Signature.Arguments.Length;
            int num2 = (parameters != null) ? parameters.Length : 0;
            if (length != num2)
            {
                throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
            }
            if (num2 <= 0)
            {
                return RuntimeMethodHandle.InvokeConstructor(this, null, (SignatureStruct) this.Signature, this.m_declaringType);
            }
            object[] args = base.CheckArguments(parameters, binder, invokeAttr, culture, this.Signature);
            object obj2 = RuntimeMethodHandle.InvokeConstructor(this, args, (SignatureStruct) this.Signature, this.m_declaringType);
            for (int i = 0; i < num2; i++)
            {
                parameters[i] = args[i];
            }
            return obj2;
        }

        [DebuggerHidden, SecuritySafeCritical, DebuggerStepThrough]
        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            INVOCATION_FLAGS invocationFlags = this.InvocationFlags;
            if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                this.ThrowNoInvokeException();
            }
            this.CheckConsistency(obj);
            if (obj != null)
            {
                new SecurityPermission(SecurityPermissionFlag.SkipVerification).Demand();
            }
            if ((invocationFlags & (INVOCATION_FLAGS.INVOCATION_FLAGS_FIELD_SPECIAL_CAST | INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY)) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_FIELD_SPECIAL_CAST) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    CodeAccessPermission.Demand(PermissionType.ReflectionMemberAccess);
                }
                if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    RuntimeMethodHandle.PerformSecurityCheck(obj, this, this.m_declaringType, (uint) this.m_invocationFlags);
                }
            }
            int length = this.Signature.Arguments.Length;
            int num2 = (parameters != null) ? parameters.Length : 0;
            if (length != num2)
            {
                throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
            }
            if (num2 <= 0)
            {
                return RuntimeMethodHandle.InvokeMethodFast(this, obj, null, this.Signature, this.m_methodAttributes, (RuntimeType) this.DeclaringType);
            }
            object[] arguments = base.CheckArguments(parameters, binder, invokeAttr, culture, this.Signature);
            object obj2 = RuntimeMethodHandle.InvokeMethodFast(this, obj, arguments, this.Signature, this.m_methodAttributes, (RuntimeType) this.ReflectedType);
            for (int i = 0; i < num2; i++)
            {
                parameters[i] = arguments[i];
            }
            return obj2;
        }

        [SecuritySafeCritical]
        public override bool IsDefined(Type attributeType, bool inherit)
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
            return CustomAttribute.IsDefined(this, underlyingSystemType);
        }

        internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
        {
            this.m_cachedData = null;
        }

        internal void SerializationInvoke(object target, SerializationInfo info, StreamingContext context)
        {
            RuntimeMethodHandle.SerializationInvoke(this, target, (SignatureStruct) this.Signature, info, context);
        }

        internal void ThrowNoInvokeException()
        {
            CheckCanCreateInstance(this.DeclaringType, (this.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs);
            if ((this.Attributes & MethodAttributes.Static) == MethodAttributes.Static)
            {
                throw new MemberAccessException(Environment.GetResourceString("Acc_NotClassInit"));
            }
            throw new TargetException();
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            if (this.m_toString == null)
            {
                this.m_toString = "Void " + this.ConstructName();
            }
            return this.m_toString;
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return this.m_methodAttributes;
            }
        }

        internal System.Reflection.BindingFlags BindingFlags
        {
            get
            {
                return this.m_bindingFlags;
            }
        }

        public override CallingConventions CallingConvention
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.Signature.CallingConvention;
            }
        }

        public override bool ContainsGenericParameters
        {
            get
            {
                return ((this.DeclaringType != null) && this.DeclaringType.ContainsGenericParameters);
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (!this.m_reflectedTypeCache.IsGlobal)
                {
                    return this.m_declaringType;
                }
                return null;
            }
        }

        private INVOCATION_FLAGS InvocationFlags
        {
            [SecurityCritical]
            get
            {
                if ((this.m_invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_INITIALIZED) == INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    INVOCATION_FLAGS invocation_flags = INVOCATION_FLAGS.INVOCATION_FLAGS_IS_CTOR;
                    Type declaringType = this.DeclaringType;
                    if (((declaringType == typeof(void)) || ((declaringType != null) && declaringType.ContainsGenericParameters)) || (((this.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs) || ((this.Attributes & MethodAttributes.RequireSecObject) == MethodAttributes.RequireSecObject)))
                    {
                        invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE;
                    }
                    else if (base.IsStatic || ((declaringType != null) && declaringType.IsAbstract))
                    {
                        invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NO_CTOR_INVOKE;
                    }
                    else
                    {
                        invocation_flags |= RuntimeMethodHandle.GetSecurityFlags(this);
                        if (((invocation_flags & INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY) == INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN) && (((this.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) || ((declaringType != null) && declaringType.NeedsReflectionSecurityCheck)))
                        {
                            invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY;
                        }
                        if (typeof(Delegate).IsAssignableFrom(this.DeclaringType))
                        {
                            invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_IS_DELEGATE_CTOR;
                        }
                    }
                    this.m_invocationFlags = invocation_flags | INVOCATION_FLAGS.INVOCATION_FLAGS_INITIALIZED;
                }
                return this.m_invocationFlags;
            }
        }

        internal bool IsOverloaded
        {
            get
            {
                return (this.m_reflectedTypeCache.GetConstructorList(MemberListType.CaseSensitive, this.Name).Count > 1);
            }
        }

        public override bool IsSecurityCritical
        {
            get
            {
                return RuntimeMethodHandle.IsSecurityCritical(this);
            }
        }

        public override bool IsSecuritySafeCritical
        {
            get
            {
                return RuntimeMethodHandle.IsSecuritySafeCritical(this);
            }
        }

        public override bool IsSecurityTransparent
        {
            get
            {
                return RuntimeMethodHandle.IsSecurityTransparent(this);
            }
        }

        [ComVisible(true)]
        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Constructor;
            }
        }

        public override int MetadataToken
        {
            [SecuritySafeCritical]
            get
            {
                return RuntimeMethodHandle.GetMethodDef(this);
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                Type declaringType = this.DeclaringType;
                if (((declaringType == null) && this.Module.Assembly.ReflectionOnly) || (declaringType is ReflectionOnlyType))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
                }
                return new RuntimeMethodHandle(this);
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.GetRuntimeModule();
            }
        }

        public override string Name
        {
            [SecuritySafeCritical]
            get
            {
                return RuntimeMethodHandle.GetName(this);
            }
        }

        public override Type ReflectedType
        {
            get
            {
                if (!this.m_reflectedTypeCache.IsGlobal)
                {
                    return this.m_reflectedTypeCache.RuntimeType;
                }
                return null;
            }
        }

        private RuntimeType ReflectedTypeInternal
        {
            get
            {
                return this.m_reflectedTypeCache.GetRuntimeType();
            }
        }

        internal InternalCache RemotingCache
        {
            get
            {
                InternalCache cachedData = this.m_cachedData;
                if (cachedData == null)
                {
                    cachedData = new InternalCache("MemberInfo");
                    InternalCache cache2 = Interlocked.CompareExchange<InternalCache>(ref this.m_cachedData, cachedData, null);
                    if (cache2 != null)
                    {
                        cachedData = cache2;
                    }
                    GC.ClearCache += new ClearCacheHandler(this.OnCacheClear);
                }
                return cachedData;
            }
        }

        private System.Signature Signature
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_signature == null)
                {
                    this.m_signature = new System.Signature(this, this.m_declaringType);
                }
                return this.m_signature;
            }
        }

        RuntimeMethodHandleInternal IRuntimeMethodInfo.Value
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return new RuntimeMethodHandleInternal(this.m_handle);
            }
        }
    }
}


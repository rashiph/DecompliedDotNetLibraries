namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection.Cache;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable]
    internal sealed class RuntimeMethodInfo : MethodInfo, ISerializable, IRuntimeMethodInfo
    {
        private System.Reflection.BindingFlags m_bindingFlags;
        private InternalCache m_cachedData;
        private RuntimeType m_declaringType;
        private IntPtr m_handle;
        private INVOCATION_FLAGS m_invocationFlags;
        private object m_keepalive;
        private MethodAttributes m_methodAttributes;
        private string m_name;
        private ParameterInfo[] m_parameters;
        private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;
        private ParameterInfo m_returnParameter;
        private System.Signature m_signature;
        private string m_toString;

        [SecurityCritical]
        internal RuntimeMethodInfo(RuntimeMethodHandleInternal handle, RuntimeType declaringType, RuntimeType.RuntimeTypeCache reflectedTypeCache, MethodAttributes methodAttributes, System.Reflection.BindingFlags bindingFlags, object keepalive)
        {
            this.m_bindingFlags = bindingFlags;
            this.m_declaringType = declaringType;
            this.m_keepalive = keepalive;
            this.m_handle = handle.Value;
            this.m_reflectedTypeCache = reflectedTypeCache;
            this.m_methodAttributes = methodAttributes;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal override bool CacheEquals(object o)
        {
            RuntimeMethodInfo info = o as RuntimeMethodInfo;
            if (info == null)
            {
                return false;
            }
            return (info.m_handle == this.m_handle);
        }

        private void CheckConsistency(object target)
        {
            if (((this.m_methodAttributes & MethodAttributes.Static) != MethodAttributes.Static) && !this.m_declaringType.IsInstanceOfType(target))
            {
                if (target == null)
                {
                    throw new TargetException(Environment.GetResourceString("RFLCT.Targ_StatMethReqTarg"));
                }
                throw new TargetException(Environment.GetResourceString("RFLCT.Targ_ITargMismatch"));
            }
        }

        internal override string ConstructName()
        {
            StringBuilder builder = new StringBuilder(this.Name);
            if (this.IsGenericMethod)
            {
                builder.Append(RuntimeMethodHandle.ConstructInstantiation(this));
            }
            builder.Append("(");
            builder.Append(MethodBase.ConstructParameters(this.GetParametersNoCopy(), this.CallingConvention));
            builder.Append(")");
            return builder.ToString();
        }

        [SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            if (!this.IsGenericMethod)
            {
                return (obj == this);
            }
            RuntimeMethodInfo method = obj as RuntimeMethodInfo;
            if ((method == null) || !method.IsGenericMethod)
            {
                return false;
            }
            IRuntimeMethodInfo info2 = RuntimeMethodHandle.StripMethodInstantiation(this);
            IRuntimeMethodInfo info3 = RuntimeMethodHandle.StripMethodInstantiation(method);
            if (info2.Value.Value != info3.Value.Value)
            {
                return false;
            }
            Type[] genericArguments = this.GetGenericArguments();
            Type[] typeArray2 = method.GetGenericArguments();
            if (genericArguments.Length != typeArray2.Length)
            {
                return false;
            }
            for (int i = 0; i < genericArguments.Length; i++)
            {
                if (genericArguments[i] != typeArray2[i])
                {
                    return false;
                }
            }
            if (this.DeclaringType != method.DeclaringType)
            {
                return false;
            }
            if (this.ReflectedType != method.ReflectedType)
            {
                return false;
            }
            return true;
        }

        [SecurityCritical]
        private ParameterInfo[] FetchNonReturnParameters()
        {
            if (this.m_parameters == null)
            {
                this.m_parameters = RuntimeParameterInfo.GetParameters(this, this, this.Signature);
            }
            return this.m_parameters;
        }

        [SecurityCritical]
        private ParameterInfo FetchReturnParameter()
        {
            if (this.m_returnParameter == null)
            {
                this.m_returnParameter = RuntimeParameterInfo.GetReturnParameter(this, this, this.Signature);
            }
            return this.m_returnParameter;
        }

        [SecuritySafeCritical]
        public override MethodInfo GetBaseDefinition()
        {
            if ((!base.IsVirtual || base.IsStatic) || ((this.m_declaringType == null) || this.m_declaringType.IsInterface))
            {
                return this;
            }
            int slot = RuntimeMethodHandle.GetSlot(this);
            RuntimeType declaringType = (RuntimeType) this.DeclaringType;
            RuntimeType reflectedType = declaringType;
            RuntimeMethodHandleInternal methodHandle = new RuntimeMethodHandleInternal();
            do
            {
                if (RuntimeTypeHandle.GetNumVirtuals(declaringType) <= slot)
                {
                    break;
                }
                methodHandle = RuntimeTypeHandle.GetMethodAt(declaringType, slot);
                reflectedType = declaringType;
                declaringType = (RuntimeType) declaringType.BaseType;
            }
            while (declaringType != null);
            return (MethodInfo) RuntimeType.GetMethodBase(reflectedType, methodHandle);
        }

        [SecuritySafeCritical]
        public override object[] GetCustomAttributes(bool inherit)
        {
            return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType, inherit);
        }

        [SecuritySafeCritical]
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
            return CustomAttribute.GetCustomAttributes(this, underlyingSystemType, inherit);
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return CustomAttributeData.GetCustomAttributesInternal(this);
        }

        internal RuntimeType GetDeclaringTypeInternal()
        {
            return this.m_declaringType;
        }

        [SecuritySafeCritical]
        public override Type[] GetGenericArguments()
        {
            Type[] methodInstantiationPublic = RuntimeMethodHandle.GetMethodInstantiationPublic(this);
            if (methodInstantiationPublic == null)
            {
                methodInstantiationPublic = new Type[0];
            }
            return methodInstantiationPublic;
        }

        [SecuritySafeCritical]
        internal RuntimeType[] GetGenericArgumentsInternal()
        {
            return RuntimeMethodHandle.GetMethodInstantiationInternal(this);
        }

        [SecuritySafeCritical]
        public override MethodInfo GetGenericMethodDefinition()
        {
            if (!this.IsGenericMethod)
            {
                throw new InvalidOperationException();
            }
            return (RuntimeType.GetMethodBase(this.m_declaringType, RuntimeMethodHandle.StripMethodInstantiation(this)) as MethodInfo);
        }

        public override int GetHashCode()
        {
            if (this.IsGenericMethod)
            {
                return ValueType.GetHashCodeOfPtr(this.m_handle);
            }
            return base.GetHashCode();
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
            if (this.m_reflectedTypeCache.IsGlobal)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_GlobalMethodSerialization"));
            }
            MemberInfoSerializationHolder.GetSerializationInfo(info, this.Name, this.ReflectedTypeInternal, this.ToString(), MemberTypes.Method, (this.IsGenericMethod & !this.IsGenericMethodDefinition) ? this.GetGenericArguments() : null);
        }

        [SecuritySafeCritical]
        public override ParameterInfo[] GetParameters()
        {
            this.FetchNonReturnParameters();
            if (this.m_parameters.Length == 0)
            {
                return this.m_parameters;
            }
            ParameterInfo[] destinationArray = new ParameterInfo[this.m_parameters.Length];
            Array.Copy(this.m_parameters, destinationArray, this.m_parameters.Length);
            return destinationArray;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        internal override ParameterInfo[] GetParametersNoCopy()
        {
            this.FetchNonReturnParameters();
            return this.m_parameters;
        }

        [SecuritySafeCritical]
        internal RuntimeMethodInfo GetParentDefinition()
        {
            if (!base.IsVirtual || this.m_declaringType.IsInterface)
            {
                return null;
            }
            RuntimeType baseType = (RuntimeType) this.m_declaringType.BaseType;
            if (baseType == null)
            {
                return null;
            }
            int slot = RuntimeMethodHandle.GetSlot(this);
            if (RuntimeTypeHandle.GetNumVirtuals(baseType) <= slot)
            {
                return null;
            }
            return (RuntimeMethodInfo) RuntimeType.GetMethodBase(baseType, RuntimeTypeHandle.GetMethodAt(baseType, slot));
        }

        internal RuntimeModule GetRuntimeModule()
        {
            return this.m_declaringType.GetRuntimeModule();
        }

        internal static MethodBase InternalGetCurrentMethod(ref StackCrawlMark stackMark)
        {
            IRuntimeMethodInfo currentMethod = RuntimeMethodHandle.GetCurrentMethod(ref stackMark);
            if (currentMethod == null)
            {
                return null;
            }
            return RuntimeType.GetMethodBase(currentMethod);
        }

        [DebuggerHidden, SecuritySafeCritical, DebuggerStepThrough]
        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return this.Invoke(obj, invokeAttr, binder, parameters, culture, false);
        }

        [DebuggerStepThrough, SecurityCritical, DebuggerHidden]
        internal object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture, bool skipVisibilityChecks)
        {
            int length = this.Signature.Arguments.Length;
            int num2 = (parameters != null) ? parameters.Length : 0;
            INVOCATION_FLAGS invocationFlags = this.InvocationFlags;
            if ((invocationFlags & (INVOCATION_FLAGS.INVOCATION_FLAGS_CONTAINS_STACK_POINTERS | INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE)) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                this.ThrowNoInvokeException();
            }
            this.CheckConsistency(obj);
            if (length != num2)
            {
                throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
            }
            if (num2 > 0xffff)
            {
                throw new TargetParameterCountException(Environment.GetResourceString("NotSupported_TooManyArgs"));
            }
            if (!skipVisibilityChecks && ((invocationFlags & (INVOCATION_FLAGS.INVOCATION_FLAGS_FIELD_SPECIAL_CAST | INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY)) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN))
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
            RuntimeType typeOwner = null;
            if (!this.m_reflectedTypeCache.IsGlobal)
            {
                typeOwner = this.m_declaringType;
            }
            if (num2 == 0)
            {
                return RuntimeMethodHandle.InvokeMethodFast(this, obj, null, this.Signature, this.m_methodAttributes, typeOwner);
            }
            object[] arguments = base.CheckArguments(parameters, binder, invokeAttr, culture, this.Signature);
            object obj2 = RuntimeMethodHandle.InvokeMethodFast(this, obj, arguments, this.Signature, this.m_methodAttributes, typeOwner);
            for (int i = 0; i < num2; i++)
            {
                parameters[i] = arguments[i];
            }
            return obj2;
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
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
            }
            return CustomAttribute.IsDefined(this, underlyingSystemType, inherit);
        }

        [SecuritySafeCritical]
        public override MethodInfo MakeGenericMethod(params Type[] methodInstantiation)
        {
            if (methodInstantiation == null)
            {
                throw new ArgumentNullException("methodInstantiation");
            }
            RuntimeType[] genericArguments = new RuntimeType[methodInstantiation.Length];
            if (!this.IsGenericMethodDefinition)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericMethodDefinition", new object[] { this }));
            }
            for (int i = 0; i < methodInstantiation.Length; i++)
            {
                Type type = methodInstantiation[i];
                if (type == null)
                {
                    throw new ArgumentNullException();
                }
                RuntimeType type2 = type as RuntimeType;
                if (type2 == null)
                {
                    Type[] typeArray2 = new Type[methodInstantiation.Length];
                    for (int j = 0; j < methodInstantiation.Length; j++)
                    {
                        typeArray2[j] = methodInstantiation[j];
                    }
                    methodInstantiation = typeArray2;
                    return MethodBuilderInstantiation.MakeGenericMethod(this, methodInstantiation);
                }
                genericArguments[i] = type2;
            }
            RuntimeType[] genericArgumentsInternal = this.GetGenericArgumentsInternal();
            RuntimeType.SanityCheckGenericArguments(genericArguments, genericArgumentsInternal);
            MethodInfo methodBase = null;
            try
            {
                methodBase = RuntimeType.GetMethodBase(this.m_reflectedTypeCache.RuntimeType, RuntimeMethodHandle.GetStubIfNeeded(new RuntimeMethodHandleInternal(this.m_handle), this.m_declaringType, genericArguments)) as MethodInfo;
            }
            catch (VerificationException exception)
            {
                RuntimeType.ValidateGenericArguments(this, genericArguments, exception);
                throw;
            }
            return methodBase;
        }

        internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
        {
            this.m_cachedData = null;
        }

        [SecuritySafeCritical]
        private void ThrowNoInvokeException()
        {
            Type declaringType = this.DeclaringType;
            if (((declaringType == null) && this.Module.Assembly.ReflectionOnly) || (declaringType is ReflectionOnlyType))
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyInvoke"));
            }
            if ((this.InvocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_CONTAINS_STACK_POINTERS) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                throw new NotSupportedException();
            }
            if ((this.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
            {
                throw new NotSupportedException();
            }
            if (this.DeclaringType.ContainsGenericParameters || this.ContainsGenericParameters)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_UnboundGenParam"));
            }
            if (base.IsAbstract)
            {
                throw new MemberAccessException();
            }
            if (this.ReturnType.IsByRef)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_ByRefReturn"));
            }
            throw new TargetException();
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            if (this.m_toString == null)
            {
                this.m_toString = this.ReturnType.SigToString() + " " + this.ConstructName();
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
                if ((this.DeclaringType != null) && this.DeclaringType.ContainsGenericParameters)
                {
                    return true;
                }
                if (this.IsGenericMethod)
                {
                    Type[] genericArguments = this.GetGenericArguments();
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        if (genericArguments[i].ContainsGenericParameters)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.m_reflectedTypeCache.IsGlobal)
                {
                    return null;
                }
                return this.m_declaringType;
            }
        }

        private INVOCATION_FLAGS InvocationFlags
        {
            [SecurityCritical]
            get
            {
                if ((this.m_invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_INITIALIZED) == INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    INVOCATION_FLAGS securityFlags = INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN;
                    Type declaringType = this.DeclaringType;
                    if (((this.ContainsGenericParameters || this.ReturnType.IsByRef) || ((declaringType != null) && declaringType.ContainsGenericParameters)) || (((this.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs) || ((this.Attributes & MethodAttributes.RequireSecObject) == MethodAttributes.RequireSecObject)))
                    {
                        securityFlags = INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE;
                    }
                    else
                    {
                        securityFlags = RuntimeMethodHandle.GetSecurityFlags(this);
                        if ((securityFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY) == INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                        {
                            if (((this.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) || ((declaringType != null) && declaringType.NeedsReflectionSecurityCheck))
                            {
                                securityFlags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY;
                            }
                            else if (this.IsGenericMethod)
                            {
                                Type[] genericArguments = this.GetGenericArguments();
                                for (int i = 0; i < genericArguments.Length; i++)
                                {
                                    if (genericArguments[i].NeedsReflectionSecurityCheck)
                                    {
                                        securityFlags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    this.m_invocationFlags = securityFlags | INVOCATION_FLAGS.INVOCATION_FLAGS_INITIALIZED;
                }
                return this.m_invocationFlags;
            }
        }

        public override bool IsGenericMethod
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return RuntimeMethodHandle.HasMethodInstantiation(this);
            }
        }

        public override bool IsGenericMethodDefinition
        {
            [SecuritySafeCritical]
            get
            {
                return RuntimeMethodHandle.IsGenericMethodDefinition(this);
            }
        }

        internal bool IsOverloaded
        {
            get
            {
                return (this.m_reflectedTypeCache.GetMethodList(MemberListType.CaseSensitive, this.Name).Count > 1);
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

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Method;
            }
        }

        public override int MetadataToken
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return RuntimeMethodHandle.GetMethodDef(this);
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            [SecuritySafeCritical]
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
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.m_name == null)
                {
                    this.m_name = RuntimeMethodHandle.GetName(this);
                }
                return this.m_name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                if (this.m_reflectedTypeCache.IsGlobal)
                {
                    return null;
                }
                return this.m_reflectedTypeCache.RuntimeType;
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

        public override ParameterInfo ReturnParameter
        {
            [SecuritySafeCritical]
            get
            {
                this.FetchReturnParameter();
                return this.m_returnParameter;
            }
        }

        public override Type ReturnType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.Signature.ReturnType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return this.ReturnParameter;
            }
        }

        internal System.Signature Signature
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
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new RuntimeMethodHandleInternal(this.m_handle);
            }
        }
    }
}


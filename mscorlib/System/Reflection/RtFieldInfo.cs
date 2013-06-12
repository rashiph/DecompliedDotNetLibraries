namespace System.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [Serializable]
    internal sealed class RtFieldInfo : RuntimeFieldInfo, IRuntimeFieldInfo
    {
        private FieldAttributes m_fieldAttributes;
        [ForceTokenStabilization]
        private IntPtr m_fieldHandle;
        private RuntimeType m_fieldType;
        private INVOCATION_FLAGS m_invocationFlags;
        private string m_name;

        [SecurityCritical]
        internal RtFieldInfo(RuntimeFieldHandleInternal handle, RuntimeType declaringType, RuntimeType.RuntimeTypeCache reflectedTypeCache, BindingFlags bindingFlags) : base(reflectedTypeCache, declaringType, bindingFlags)
        {
            this.m_fieldHandle = handle.Value;
            this.m_fieldAttributes = RuntimeFieldHandle.GetAttributes(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal override bool CacheEquals(object o)
        {
            RtFieldInfo info = o as RtFieldInfo;
            if (info == null)
            {
                return false;
            }
            return (info.m_fieldHandle == this.m_fieldHandle);
        }

        private void CheckConsistency(object target)
        {
            if (((this.m_fieldAttributes & FieldAttributes.Static) != FieldAttributes.Static) && !base.m_declaringType.IsInstanceOfType(target))
            {
                if (target == null)
                {
                    throw new TargetException(Environment.GetResourceString("RFLCT.Targ_StatFldReqTarg"));
                }
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_FieldDeclTarget"), new object[] { this.Name, base.m_declaringType, target.GetType() }));
            }
        }

        internal IntPtr GetFieldHandle()
        {
            return this.m_fieldHandle;
        }

        [SecuritySafeCritical]
        public override Type[] GetOptionalCustomModifiers()
        {
            return new Signature(this, base.m_declaringType).GetCustomModifiers(1, false);
        }

        public override object GetRawConstantValue()
        {
            throw new InvalidOperationException();
        }

        [SecuritySafeCritical]
        public override Type[] GetRequiredCustomModifiers()
        {
            return new Signature(this, base.m_declaringType).GetCustomModifiers(1, true);
        }

        [SecuritySafeCritical]
        internal override RuntimeModule GetRuntimeModule()
        {
            return RuntimeTypeHandle.GetModule(RuntimeFieldHandle.GetApproxDeclaringType(this));
        }

        public override object GetValue(object obj)
        {
            return this.InternalGetValue(obj, true);
        }

        [SecuritySafeCritical, DebuggerStepThrough, DebuggerHidden]
        public override unsafe object GetValueDirect(TypedReference obj)
        {
            if (obj.IsNull)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_TypedReference_Null"));
            }
            return RuntimeFieldHandle.GetValueDirect(this, (RuntimeType) this.FieldType, (void*) &obj, (RuntimeType) this.DeclaringType);
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal object InternalGetValue(object obj, bool doVisibilityCheck)
        {
            return this.InternalGetValue(obj, doVisibilityCheck, true);
        }

        [DebuggerHidden, SecuritySafeCritical, DebuggerStepThrough]
        internal object InternalGetValue(object obj, bool doVisibilityCheck, bool doCheckConsistency)
        {
            INVOCATION_FLAGS invocationFlags = this.InvocationFlags;
            RuntimeType declaringType = this.DeclaringType as RuntimeType;
            if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                if ((declaringType != null) && this.DeclaringType.ContainsGenericParameters)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_UnboundGenField"));
                }
                if (((declaringType != null) || !this.Module.Assembly.ReflectionOnly) && !(declaringType is ReflectionOnlyType))
                {
                    throw new FieldAccessException();
                }
                throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyField"));
            }
            if (doCheckConsistency)
            {
                this.CheckConsistency(obj);
            }
            RuntimeType fieldType = (RuntimeType) this.FieldType;
            if (doVisibilityCheck && ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN))
            {
                PerformVisibilityCheckOnField(this.m_fieldHandle, obj, base.m_declaringType, this.m_fieldAttributes, (uint) (this.m_invocationFlags & ~INVOCATION_FLAGS.INVOCATION_FLAGS_IS_CTOR));
            }
            bool domainInitialized = false;
            if (declaringType == null)
            {
                return RuntimeFieldHandle.GetValue(this, obj, fieldType, null, ref domainInitialized);
            }
            domainInitialized = declaringType.DomainInitialized;
            object obj2 = RuntimeFieldHandle.GetValue(this, obj, fieldType, declaringType, ref domainInitialized);
            declaringType.DomainInitialized = domainInitialized;
            return obj2;
        }

        [DebuggerStepThrough, DebuggerHidden, SecurityCritical]
        internal void InternalSetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture, bool doVisibilityCheck)
        {
            this.InternalSetValue(obj, value, invokeAttr, binder, culture, doVisibilityCheck, true);
        }

        [DebuggerHidden, SecurityCritical, DebuggerStepThrough]
        internal void InternalSetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture, bool doVisibilityCheck, bool doCheckConsistency)
        {
            INVOCATION_FLAGS invocationFlags = this.InvocationFlags;
            RuntimeType declaringType = this.DeclaringType as RuntimeType;
            if ((invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
            {
                if ((declaringType != null) && declaringType.ContainsGenericParameters)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_UnboundGenField"));
                }
                if (((declaringType != null) || !this.Module.Assembly.ReflectionOnly) && !(declaringType is ReflectionOnlyType))
                {
                    throw new FieldAccessException();
                }
                throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyField"));
            }
            if (doCheckConsistency)
            {
                this.CheckConsistency(obj);
            }
            RuntimeType fieldType = (RuntimeType) this.FieldType;
            value = fieldType.CheckValue(value, binder, culture, invokeAttr);
            if (doVisibilityCheck && ((invocationFlags & (INVOCATION_FLAGS.INVOCATION_FLAGS_IS_CTOR | INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY)) != INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN))
            {
                PerformVisibilityCheckOnField(this.m_fieldHandle, obj, base.m_declaringType, this.m_fieldAttributes, (uint) this.m_invocationFlags);
            }
            bool domainInitialized = false;
            if (declaringType == null)
            {
                RuntimeFieldHandle.SetValue(this, obj, value, fieldType, this.m_fieldAttributes, null, ref domainInitialized);
            }
            else
            {
                domainInitialized = declaringType.DomainInitialized;
                RuntimeFieldHandle.SetValue(this, obj, value, fieldType, this.m_fieldAttributes, declaringType, ref domainInitialized);
                declaringType.DomainInitialized = domainInitialized;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void PerformVisibilityCheckOnField(IntPtr field, object target, RuntimeType declaringType, FieldAttributes attr, uint invocationFlags);
        [SecuritySafeCritical, DebuggerHidden, DebuggerStepThrough]
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            this.InternalSetValue(obj, value, invokeAttr, binder, culture, true);
        }

        [DebuggerHidden, SecuritySafeCritical, DebuggerStepThrough]
        public override unsafe void SetValueDirect(TypedReference obj, object value)
        {
            if (obj.IsNull)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_TypedReference_Null"));
            }
            RuntimeFieldHandle.SetValueDirect(this, (RuntimeType) this.FieldType, (void*) &obj, value, (RuntimeType) this.DeclaringType);
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return this.m_fieldAttributes;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            [SecuritySafeCritical]
            get
            {
                Type declaringType = this.DeclaringType;
                if (((declaringType == null) && this.Module.Assembly.ReflectionOnly) || (declaringType is ReflectionOnlyType))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
                }
                return new RuntimeFieldHandle(this);
            }
        }

        public override Type FieldType
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_fieldType == null)
                {
                    this.m_fieldType = new Signature(this, base.m_declaringType).FieldType;
                }
                return this.m_fieldType;
            }
        }

        private INVOCATION_FLAGS InvocationFlags
        {
            get
            {
                if ((this.m_invocationFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_INITIALIZED) == INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                {
                    Type declaringType = this.DeclaringType;
                    bool flag = declaringType is ReflectionOnlyType;
                    INVOCATION_FLAGS invocation_flags = INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN;
                    if ((((declaringType != null) && declaringType.ContainsGenericParameters) || ((declaringType == null) && this.Module.Assembly.ReflectionOnly)) || flag)
                    {
                        invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NO_INVOKE;
                    }
                    if (invocation_flags == INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN)
                    {
                        if ((this.m_fieldAttributes & FieldAttributes.InitOnly) != FieldAttributes.PrivateScope)
                        {
                            invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_IS_CTOR;
                        }
                        if ((this.m_fieldAttributes & FieldAttributes.HasFieldRVA) != FieldAttributes.PrivateScope)
                        {
                            invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_IS_CTOR;
                        }
                        bool flag2 = this.IsSecurityCritical && !this.IsSecuritySafeCritical;
                        bool flag3 = ((this.m_fieldAttributes & FieldAttributes.FieldAccessMask) != FieldAttributes.Public) || ((declaringType != null) && declaringType.NeedsReflectionSecurityCheck);
                        if (flag2 || flag3)
                        {
                            invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY;
                        }
                        Type fieldType = this.FieldType;
                        if ((fieldType.IsPointer || fieldType.IsEnum) || fieldType.IsPrimitive)
                        {
                            invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_FIELD_SPECIAL_CAST;
                        }
                    }
                    invocation_flags |= INVOCATION_FLAGS.INVOCATION_FLAGS_INITIALIZED;
                    this.m_invocationFlags = invocation_flags;
                }
                return this.m_invocationFlags;
            }
        }

        public override int MetadataToken
        {
            [SecuritySafeCritical]
            get
            {
                return RuntimeFieldHandle.GetToken(this);
            }
        }

        public override string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                if (this.m_name == null)
                {
                    this.m_name = RuntimeFieldHandle.GetName(this);
                }
                return this.m_name;
            }
        }

        RuntimeFieldHandleInternal IRuntimeFieldInfo.Value
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return new RuntimeFieldHandleInternal(this.m_fieldHandle);
            }
        }
    }
}


namespace System
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Cache;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable]
    internal class RuntimeType : Type, ISerializable, ICloneable
    {
        private const BindingFlags BinderGetSetField = (BindingFlags.SetField | BindingFlags.GetField);
        private const BindingFlags BinderGetSetProperty = (BindingFlags.SetProperty | BindingFlags.GetProperty);
        private const BindingFlags BinderNonCreateInstance = (BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.InvokeMethod);
        private const BindingFlags BinderNonFieldGetSet = 0xfff300;
        private const BindingFlags BinderSetInvokeField = (BindingFlags.SetField | BindingFlags.InvokeMethod);
        private const BindingFlags BinderSetInvokeProperty = (BindingFlags.SetProperty | BindingFlags.InvokeMethod);
        private const BindingFlags ClassicBindingMask = (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.InvokeMethod);
        private static readonly RuntimeType DelegateType = ((RuntimeType) typeof(Delegate));
        internal static readonly RuntimeType EnumType = ((RuntimeType) typeof(Enum));
        private const BindingFlags InvocationMask = (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.CreateInstance | BindingFlags.InvokeMethod);
        private IntPtr m_cache;
        private InternalCache m_cachedData;
        [ForceTokenStabilization]
        internal IntPtr m_handle;
        private object m_keepalive;
        private const BindingFlags MemberBindingMask = 0xff;
        private static readonly RuntimeType ObjectType = ((RuntimeType) typeof(object));
        private static ActivatorCache s_ActivatorCache;
        private static OleAutBinder s_ForwardCallBinder;
        private static TypeCacheQueue s_typeCache = null;
        private static RuntimeType s_typedRef = ((RuntimeType) typeof(TypedReference));
        private static readonly RuntimeType StringType = ((RuntimeType) typeof(string));
        internal static readonly RuntimeType ValueType = ((RuntimeType) typeof(System.ValueType));

        internal RuntimeType()
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object _CreateEnum(RuntimeType enumType, long value);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object AllocateValueType(RuntimeType type, object value, bool fForceTypeChange);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal override bool CacheEquals(object o)
        {
            RuntimeType type = o as RuntimeType;
            if (type == null)
            {
                return false;
            }
            return type.m_handle.Equals(this.m_handle);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool CanValueSpecialCast(RuntimeType valueType, RuntimeType targetType);
        [SecuritySafeCritical]
        internal object CheckValue(object value, Binder binder, CultureInfo culture, BindingFlags invokeAttr)
        {
            if (this.IsInstanceOfType(value))
            {
                if (!object.ReferenceEquals(value.GetType(), this) && RuntimeTypeHandle.IsValueType(this))
                {
                    return AllocateValueType(this.TypeHandle.GetRuntimeType(), value, true);
                }
                return value;
            }
            if (base.IsByRef)
            {
                Type elementType = this.GetElementType();
                if (elementType.IsInstanceOfType(value) || (value == null))
                {
                    return AllocateValueType(elementType.TypeHandle.GetRuntimeType(), value, false);
                }
            }
            else
            {
                if (value == null)
                {
                    return value;
                }
                if (this == s_typedRef)
                {
                    return value;
                }
            }
            bool needsSpecialCast = (base.IsPointer || this.IsEnum) || base.IsPrimitive;
            if (needsSpecialCast)
            {
                RuntimeType pointerType;
                Pointer pointer = value as Pointer;
                if (pointer != null)
                {
                    pointerType = pointer.GetPointerType();
                }
                else
                {
                    pointerType = (RuntimeType) value.GetType();
                }
                if (CanValueSpecialCast(pointerType, this))
                {
                    if (pointer != null)
                    {
                        return pointer.GetPointerValue();
                    }
                    return value;
                }
            }
            if ((invokeAttr & BindingFlags.ExactBinding) == BindingFlags.ExactBinding)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), new object[] { value.GetType(), this }));
            }
            return this.TryChangeType(value, binder, culture, needsSpecialCast);
        }

        public object Clone()
        {
            return this;
        }

        [SecuritySafeCritical]
        internal static object CreateEnum(RuntimeType enumType, long value)
        {
            return _CreateEnum(enumType, value);
        }

        private void CreateInstanceCheckThis()
        {
            if (this is ReflectionOnlyType)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ReflectionOnlyInvoke"));
            }
            if (this.ContainsGenericParameters)
            {
                throw new ArgumentException(Environment.GetResourceString("Acc_CreateGenericEx", new object[] { this }));
            }
            Type rootElementType = this.GetRootElementType();
            if (object.ReferenceEquals(rootElementType, typeof(ArgIterator)))
            {
                throw new NotSupportedException(Environment.GetResourceString("Acc_CreateArgIterator"));
            }
            if (object.ReferenceEquals(rootElementType, typeof(void)))
            {
                throw new NotSupportedException(Environment.GetResourceString("Acc_CreateVoid"));
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal object CreateInstanceDefaultCtor(bool publicOnly)
        {
            return this.CreateInstanceDefaultCtor(publicOnly, false, false, true);
        }

        [DebuggerHidden, SecuritySafeCritical, DebuggerStepThrough]
        internal object CreateInstanceDefaultCtor(bool publicOnly, bool skipVisibilityChecks, bool skipCheckThis, bool fillCache)
        {
            if (base.GetType() == typeof(ReflectionOnlyType))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
            }
            ActivatorCache cache = s_ActivatorCache;
            if (cache != null)
            {
                ActivatorCacheEntry entry = cache.GetEntry(this);
                if (entry != null)
                {
                    if ((publicOnly && (entry.m_ctor != null)) && ((entry.m_ctorAttributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public))
                    {
                        throw new MissingMethodException(Environment.GetResourceString("Arg_NoDefCTor"));
                    }
                    object obj2 = RuntimeTypeHandle.Allocate(this);
                    if (entry.m_ctor != null)
                    {
                        if (!skipVisibilityChecks && entry.m_bNeedSecurityCheck)
                        {
                            RuntimeMethodHandle.PerformSecurityCheck(obj2, entry.m_hCtorMethodHandle, this, 0x10000000);
                        }
                        try
                        {
                            entry.m_ctor(obj2);
                        }
                        catch (Exception exception)
                        {
                            throw new TargetInvocationException(exception);
                        }
                    }
                    return obj2;
                }
            }
            return this.CreateInstanceSlow(publicOnly, skipCheckThis, fillCache);
        }

        [SecurityCritical]
        internal object CreateInstanceImpl(BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            this.CreateInstanceCheckThis();
            object obj2 = null;
            try
            {
                try
                {
                    MethodBase base2;
                    if (activationAttributes != null)
                    {
                        ActivationServices.PushActivationAttributes(this, activationAttributes);
                    }
                    if (args == null)
                    {
                        args = new object[0];
                    }
                    int length = args.Length;
                    if (binder == null)
                    {
                        binder = Type.DefaultBinder;
                    }
                    if ((((length == 0) && ((bindingAttr & BindingFlags.Public) != BindingFlags.Default)) && ((bindingAttr & BindingFlags.Instance) != BindingFlags.Default)) && (this.IsGenericCOMObjectImpl() || base.IsValueType))
                    {
                        return this.CreateInstanceDefaultCtor((bindingAttr & BindingFlags.NonPublic) == BindingFlags.Default);
                    }
                    ConstructorInfo[] constructors = this.GetConstructors(bindingAttr);
                    List<MethodBase> list = new List<MethodBase>(constructors.Length);
                    Type[] argumentTypes = new Type[length];
                    for (int i = 0; i < length; i++)
                    {
                        if (args[i] != null)
                        {
                            argumentTypes[i] = args[i].GetType();
                        }
                    }
                    for (int j = 0; j < constructors.Length; j++)
                    {
                        if (FilterApplyConstructorInfo((RuntimeConstructorInfo) constructors[j], bindingAttr, CallingConventions.Any, argumentTypes))
                        {
                            list.Add(constructors[j]);
                        }
                    }
                    MethodBase[] array = new MethodBase[list.Count];
                    list.CopyTo(array);
                    if ((array != null) && (array.Length == 0))
                    {
                        array = null;
                    }
                    if (array == null)
                    {
                        if (activationAttributes != null)
                        {
                            ActivationServices.PopActivationAttributes(this);
                            activationAttributes = null;
                        }
                        throw new MissingMethodException(Environment.GetResourceString("MissingConstructor_Name", new object[] { this.FullName }));
                    }
                    object state = null;
                    try
                    {
                        base2 = binder.BindToMethod(bindingAttr, array, ref args, null, culture, null, out state);
                    }
                    catch (MissingMethodException)
                    {
                        base2 = null;
                    }
                    if (base2 == null)
                    {
                        if (activationAttributes != null)
                        {
                            ActivationServices.PopActivationAttributes(this);
                            activationAttributes = null;
                        }
                        throw new MissingMethodException(Environment.GetResourceString("MissingConstructor_Name", new object[] { this.FullName }));
                    }
                    if (DelegateType.IsAssignableFrom(base2.DeclaringType))
                    {
                        new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                    }
                    if (base2.GetParametersNoCopy().Length == 0)
                    {
                        if (args.Length != 0)
                        {
                            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("NotSupported_CallToVarArg"), new object[0]));
                        }
                        return Activator.CreateInstance(this, true);
                    }
                    obj2 = ((ConstructorInfo) base2).Invoke(bindingAttr, binder, args, culture);
                    if (state != null)
                    {
                        binder.ReorderArgumentArray(ref args, state);
                    }
                    return obj2;
                }
                finally
                {
                    if (activationAttributes != null)
                    {
                        ActivationServices.PopActivationAttributes(this);
                        activationAttributes = null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return obj2;
        }

        [SecuritySafeCritical]
        private object CreateInstanceSlow(bool publicOnly, bool skipCheckThis, bool fillCache)
        {
            RuntimeMethodHandleInternal ctor = new RuntimeMethodHandleInternal();
            bool bNeedSecurityCheck = true;
            bool canBeCached = false;
            bool noCheck = false;
            if (!skipCheckThis)
            {
                this.CreateInstanceCheckThis();
            }
            if (!fillCache)
            {
                noCheck = true;
            }
            object obj2 = RuntimeTypeHandle.CreateInstance(this, publicOnly, noCheck, ref canBeCached, ref ctor, ref bNeedSecurityCheck);
            if (canBeCached && fillCache)
            {
                ActivatorCache cache = s_ActivatorCache;
                if (cache == null)
                {
                    cache = new ActivatorCache();
                    Thread.MemoryBarrier();
                    s_ActivatorCache = cache;
                }
                ActivatorCacheEntry ace = new ActivatorCacheEntry(this, ctor, bNeedSecurityCheck);
                Thread.MemoryBarrier();
                cache.SetEntry(ace);
            }
            return obj2;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override bool Equals(object obj)
        {
            return (obj == this);
        }

        private static bool FilterApplyBase(MemberInfo memberInfo, BindingFlags bindingFlags, bool isPublic, bool isNonProtectedInternal, bool isStatic, string name, bool prefixLookup)
        {
            if (isPublic)
            {
                if ((bindingFlags & BindingFlags.Public) == BindingFlags.Default)
                {
                    return false;
                }
            }
            else if ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                return false;
            }
            bool flag = !object.ReferenceEquals(memberInfo.DeclaringType, memberInfo.ReflectedType);
            if (((bindingFlags & BindingFlags.DeclaredOnly) != BindingFlags.Default) && flag)
            {
                return false;
            }
            if ((memberInfo.MemberType != MemberTypes.TypeInfo) && (memberInfo.MemberType != MemberTypes.NestedType))
            {
                if (isStatic)
                {
                    if (((bindingFlags & BindingFlags.FlattenHierarchy) == BindingFlags.Default) && flag)
                    {
                        return false;
                    }
                    if ((bindingFlags & BindingFlags.Static) == BindingFlags.Default)
                    {
                        return false;
                    }
                }
                else if ((bindingFlags & BindingFlags.Instance) == BindingFlags.Default)
                {
                    return false;
                }
            }
            if (prefixLookup && !FilterApplyPrefixLookup(memberInfo, name, (bindingFlags & BindingFlags.IgnoreCase) != BindingFlags.Default))
            {
                return false;
            }
            if (((((bindingFlags & BindingFlags.DeclaredOnly) == BindingFlags.Default) && flag) && (isNonProtectedInternal && ((bindingFlags & BindingFlags.NonPublic) != BindingFlags.Default))) && (!isStatic && ((bindingFlags & BindingFlags.Instance) != BindingFlags.Default)))
            {
                MethodInfo info = memberInfo as MethodInfo;
                if (info == null)
                {
                    return false;
                }
                if (!info.IsVirtual && !info.IsAbstract)
                {
                    return false;
                }
            }
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static bool FilterApplyConstructorInfo(RuntimeConstructorInfo constructor, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
        {
            return FilterApplyMethodBase(constructor, constructor.BindingFlags, bindingFlags, callConv, argumentTypes);
        }

        private static bool FilterApplyMethodBase(MethodBase methodBase, BindingFlags methodFlags, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
        {
            bindingFlags ^= BindingFlags.DeclaredOnly;
            if ((bindingFlags & methodFlags) != methodFlags)
            {
                return false;
            }
            if ((callConv & CallingConventions.Any) == 0)
            {
                if (((callConv & CallingConventions.VarArgs) != 0) && ((methodBase.CallingConvention & CallingConventions.VarArgs) == 0))
                {
                    return false;
                }
                if (((callConv & CallingConventions.Standard) != 0) && ((methodBase.CallingConvention & CallingConventions.Standard) == 0))
                {
                    return false;
                }
            }
            if (argumentTypes != null)
            {
                ParameterInfo[] parametersNoCopy = methodBase.GetParametersNoCopy();
                if (argumentTypes.Length != parametersNoCopy.Length)
                {
                    if ((bindingFlags & (BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.CreateInstance | BindingFlags.InvokeMethod)) == BindingFlags.Default)
                    {
                        return false;
                    }
                    bool flag = false;
                    if (argumentTypes.Length > parametersNoCopy.Length)
                    {
                        if ((methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
                        {
                            flag = true;
                        }
                    }
                    else if ((bindingFlags & BindingFlags.OptionalParamBinding) == BindingFlags.Default)
                    {
                        flag = true;
                    }
                    else if (!parametersNoCopy[argumentTypes.Length].IsOptional)
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        if (parametersNoCopy.Length != 0)
                        {
                            if (argumentTypes.Length < (parametersNoCopy.Length - 1))
                            {
                                return false;
                            }
                            ParameterInfo info = parametersNoCopy[parametersNoCopy.Length - 1];
                            if (!info.ParameterType.IsArray)
                            {
                                return false;
                            }
                            if (info.IsDefined(typeof(ParamArrayAttribute), false))
                            {
                                goto Label_0121;
                            }
                        }
                        return false;
                    }
                }
                else if (((bindingFlags & BindingFlags.ExactBinding) != BindingFlags.Default) && ((bindingFlags & BindingFlags.InvokeMethod) == BindingFlags.Default))
                {
                    for (int i = 0; i < parametersNoCopy.Length; i++)
                    {
                        if ((argumentTypes[i] != null) && !object.ReferenceEquals(parametersNoCopy[i].ParameterType, argumentTypes[i]))
                        {
                            return false;
                        }
                    }
                }
            }
        Label_0121:
            return true;
        }

        private static bool FilterApplyMethodInfo(RuntimeMethodInfo method, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
        {
            return FilterApplyMethodBase(method, method.BindingFlags, bindingFlags, callConv, argumentTypes);
        }

        private static bool FilterApplyPrefixLookup(MemberInfo memberInfo, string name, bool ignoreCase)
        {
            if (ignoreCase)
            {
                if (!memberInfo.Name.ToLower(CultureInfo.InvariantCulture).StartsWith(name, StringComparison.Ordinal))
                {
                    return false;
                }
            }
            else if (!memberInfo.Name.StartsWith(name, StringComparison.Ordinal))
            {
                return false;
            }
            return true;
        }

        private static bool FilterApplyType(Type type, BindingFlags bindingFlags, string name, bool prefixLookup, string ns)
        {
            bool isPublic = type.IsNestedPublic || type.IsPublic;
            bool isStatic = false;
            if (!FilterApplyBase(type, bindingFlags, isPublic, type.IsNestedAssembly, isStatic, name, prefixLookup))
            {
                return false;
            }
            if ((ns != null) && !type.Namespace.Equals(ns))
            {
                return false;
            }
            return true;
        }

        private static void FilterHelper(BindingFlags bindingFlags, ref string name, out bool ignoreCase, out MemberListType listType)
        {
            bool flag;
            FilterHelper(bindingFlags, ref name, false, out flag, out ignoreCase, out listType);
        }

        private static void FilterHelper(BindingFlags bindingFlags, ref string name, bool allowPrefixLookup, out bool prefixLookup, out bool ignoreCase, out MemberListType listType)
        {
            prefixLookup = false;
            ignoreCase = false;
            if (name != null)
            {
                if ((bindingFlags & BindingFlags.IgnoreCase) != BindingFlags.Default)
                {
                    name = name.ToLower(CultureInfo.InvariantCulture);
                    ignoreCase = true;
                    listType = MemberListType.CaseInsensitive;
                }
                else
                {
                    listType = MemberListType.CaseSensitive;
                }
                if (allowPrefixLookup && name.EndsWith("*", StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - 1);
                    prefixLookup = true;
                    listType = MemberListType.All;
                }
            }
            else
            {
                listType = MemberListType.All;
            }
        }

        internal static BindingFlags FilterPreCalculate(bool isPublic, bool isInherited, bool isStatic)
        {
            BindingFlags flags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            if (isInherited)
            {
                flags |= BindingFlags.DeclaredOnly;
                if (isStatic)
                {
                    return (flags | (BindingFlags.FlattenHierarchy | BindingFlags.Static));
                }
                return (flags | BindingFlags.Instance);
            }
            if (isStatic)
            {
                return (flags | BindingFlags.Static);
            }
            return (flags | BindingFlags.Instance);
        }

        [SecuritySafeCritical]
        private object ForwardCallToInvokeMember(string memberName, BindingFlags flags, object target, int[] aWrapperTypes, ref MessageData msgData)
        {
            ParameterModifier[] modifiers = null;
            object obj2 = null;
            Message msg = new Message();
            msg.InitFields(msgData);
            MethodInfo methodBase = (MethodInfo) msg.GetMethodBase();
            object[] args = msg.Args;
            int length = args.Length;
            ParameterInfo[] parametersNoCopy = methodBase.GetParametersNoCopy();
            if (length > 0)
            {
                ParameterModifier modifier = new ParameterModifier(length);
                for (int j = 0; j < length; j++)
                {
                    if (parametersNoCopy[j].ParameterType.IsByRef)
                    {
                        modifier[j] = true;
                    }
                }
                modifiers = new ParameterModifier[] { modifier };
                if (aWrapperTypes != null)
                {
                    this.WrapArgsForInvokeCall(args, aWrapperTypes);
                }
            }
            if (object.ReferenceEquals(methodBase.ReturnType, typeof(void)))
            {
                flags |= BindingFlags.IgnoreReturn;
            }
            try
            {
                obj2 = this.InvokeMember(memberName, flags, null, target, args, modifiers, null, null);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            for (int i = 0; i < length; i++)
            {
                if (modifiers[0][i] && (args[i] != null))
                {
                    Type elementType = parametersNoCopy[i].ParameterType.GetElementType();
                    if (!object.ReferenceEquals(elementType, args[i].GetType()))
                    {
                        args[i] = this.ForwardCallBinder.ChangeType(args[i], elementType, null);
                    }
                }
            }
            if (obj2 != null)
            {
                Type returnType = methodBase.ReturnType;
                if (!object.ReferenceEquals(returnType, obj2.GetType()))
                {
                    obj2 = this.ForwardCallBinder.ChangeType(obj2, returnType, null);
                }
            }
            RealProxy.PropagateOutParameters(msg, args, obj2);
            return obj2;
        }

        [SecuritySafeCritical]
        public override int GetArrayRank()
        {
            if (!this.IsArrayImpl())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_HasToBeArrayClass"));
            }
            return RuntimeTypeHandle.GetArrayRank(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return RuntimeTypeHandle.GetAttributes(this);
        }

        private RuntimeType GetBaseType()
        {
            if (base.IsInterface)
            {
                return null;
            }
            if (!RuntimeTypeHandle.IsGenericVariable(this))
            {
                return RuntimeTypeHandle.GetBaseType(this);
            }
            Type[] genericParameterConstraints = this.GetGenericParameterConstraints();
            RuntimeType objectType = ObjectType;
            for (int i = 0; i < genericParameterConstraints.Length; i++)
            {
                RuntimeType type2 = (RuntimeType) genericParameterConstraints[i];
                if (!type2.IsInterface)
                {
                    if (type2.IsGenericParameter)
                    {
                        System.Reflection.GenericParameterAttributes attributes = type2.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.SpecialConstraintMask;
                        if (((attributes & System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint) == System.Reflection.GenericParameterAttributes.None) && ((attributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) == System.Reflection.GenericParameterAttributes.None))
                        {
                            continue;
                        }
                    }
                    objectType = type2;
                }
            }
            if (objectType == ObjectType)
            {
                System.Reflection.GenericParameterAttributes attributes2 = this.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.SpecialConstraintMask;
                if ((attributes2 & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != System.Reflection.GenericParameterAttributes.None)
                {
                    objectType = ValueType;
                }
            }
            return objectType;
        }

        private ConstructorInfo[] GetConstructorCandidates(string name, BindingFlags bindingAttr, CallingConventions callConv, Type[] types, bool allowPrefixLookup)
        {
            bool flag;
            bool flag2;
            MemberListType type;
            FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out flag2, out type);
            CerArrayList<RuntimeConstructorInfo> constructorList = this.Cache.GetConstructorList(type, name);
            List<ConstructorInfo> list2 = new List<ConstructorInfo>(constructorList.Count);
            for (int i = 0; i < constructorList.Count; i++)
            {
                RuntimeConstructorInfo constructor = constructorList[i];
                if (FilterApplyConstructorInfo(constructor, bindingAttr, callConv, types) && (!flag || FilterApplyPrefixLookup(constructor, name, flag2)))
                {
                    list2.Add(constructor);
                }
            }
            return list2.ToArray();
        }

        [SecuritySafeCritical]
        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            ConstructorInfo[] match = this.GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, types, false);
            if (binder == null)
            {
                binder = Type.DefaultBinder;
            }
            if (match.Length == 0)
            {
                return null;
            }
            if ((types.Length == 0) && (match.Length == 1))
            {
                ParameterInfo[] parametersNoCopy = match[0].GetParametersNoCopy();
                if ((parametersNoCopy == null) || (parametersNoCopy.Length == 0))
                {
                    return match[0];
                }
            }
            if ((bindingAttr & BindingFlags.ExactBinding) != BindingFlags.Default)
            {
                return (DefaultBinder.ExactBinding(match, types, modifiers) as ConstructorInfo);
            }
            return (binder.SelectMethod(bindingAttr, match, types, modifiers) as ConstructorInfo);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ComVisible(true)]
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return this.GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, null, false);
        }

        [SecuritySafeCritical]
        public override object[] GetCustomAttributes(bool inherit)
        {
            return CustomAttribute.GetCustomAttributes(this, ObjectType, inherit);
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

        [SecurityCritical]
        internal virtual string GetDefaultMemberName()
        {
            string memberName = (string) this.RemotingCache[CacheObjType.DefaultMember];
            if (memberName == null)
            {
                object[] customAttributes = this.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
                if (customAttributes.Length > 1)
                {
                    throw new InvalidProgramException(Environment.GetResourceString("ExecutionEngine_InvalidAttribute"));
                }
                if (customAttributes.Length == 0)
                {
                    return null;
                }
                memberName = ((DefaultMemberAttribute) customAttributes[0]).MemberName;
                this.RemotingCache[CacheObjType.DefaultMember] = memberName;
            }
            return memberName;
        }

        [SecuritySafeCritical]
        public override MemberInfo[] GetDefaultMembers()
        {
            string name = (string) this.RemotingCache[CacheObjType.DefaultMember];
            if (name == null)
            {
                CustomAttributeData data = null;
                Type objB = typeof(DefaultMemberAttribute);
                for (RuntimeType type2 = this; type2 != null; type2 = type2.GetBaseType())
                {
                    IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(type2);
                    for (int i = 0; i < customAttributes.Count; i++)
                    {
                        if (object.ReferenceEquals(customAttributes[i].Constructor.DeclaringType, objB))
                        {
                            data = customAttributes[i];
                            break;
                        }
                    }
                    if (data != null)
                    {
                        break;
                    }
                }
                if (data == null)
                {
                    return new MemberInfo[0];
                }
                CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                name = argument.Value as string;
                this.RemotingCache[CacheObjType.DefaultMember] = name;
            }
            MemberInfo[] member = base.GetMember(name);
            if (member == null)
            {
                member = new MemberInfo[0];
            }
            return member;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public override Type GetElementType()
        {
            return RuntimeTypeHandle.GetElementType(this);
        }

        public override string GetEnumName(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Type t = value.GetType();
            if (!t.IsEnum && !Type.IsIntegerType(t))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnumBaseTypeOrEnum"), "value");
            }
            ulong[] values = Enum.InternalGetValues(this);
            ulong num = Enum.ToUInt64(value);
            int index = Array.BinarySearch<ulong>(values, num);
            if (index >= 0)
            {
                return Enum.InternalGetNames(this)[index];
            }
            return null;
        }

        public override string[] GetEnumNames()
        {
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            string[] names = Enum.InternalGetNames(this);
            string[] destinationArray = new string[names.Length];
            Array.Copy(names, destinationArray, names.Length);
            return destinationArray;
        }

        [SecuritySafeCritical]
        public override Type GetEnumUnderlyingType()
        {
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            return Enum.InternalGetUnderlyingType(this);
        }

        [SecuritySafeCritical]
        public override Array GetEnumValues()
        {
            if (!this.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            ulong[] values = Enum.InternalGetValues(this);
            Array array = Array.UnsafeCreateInstance(this, values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                object obj2 = Enum.ToObject(this, values[i]);
                array.SetValue(obj2, i);
            }
            return array;
        }

        [SecuritySafeCritical]
        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            bool flag;
            MemberListType type;
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            FilterHelper(bindingAttr, ref name, out flag, out type);
            CerArrayList<RuntimeEventInfo> eventList = this.Cache.GetEventList(type, name);
            EventInfo info = null;
            bindingAttr ^= BindingFlags.DeclaredOnly;
            for (int i = 0; i < eventList.Count; i++)
            {
                RuntimeEventInfo info2 = eventList[i];
                if ((bindingAttr & info2.BindingFlags) == info2.BindingFlags)
                {
                    if (info != null)
                    {
                        throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                    }
                    info = info2;
                }
            }
            return info;
        }

        private EventInfo[] GetEventCandidates(string name, BindingFlags bindingAttr, bool allowPrefixLookup)
        {
            bool flag;
            bool flag2;
            MemberListType type;
            FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out flag2, out type);
            CerArrayList<RuntimeEventInfo> eventList = this.Cache.GetEventList(type, name);
            bindingAttr ^= BindingFlags.DeclaredOnly;
            List<EventInfo> list2 = new List<EventInfo>(eventList.Count);
            for (int i = 0; i < eventList.Count; i++)
            {
                RuntimeEventInfo memberInfo = eventList[i];
                if (((bindingAttr & memberInfo.BindingFlags) == memberInfo.BindingFlags) && (!flag || FilterApplyPrefixLookup(memberInfo, name, flag2)))
                {
                    list2.Add(memberInfo);
                }
            }
            return list2.ToArray();
        }

        [SecuritySafeCritical]
        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return this.GetEventCandidates(null, bindingAttr, false);
        }

        [SecuritySafeCritical]
        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            bool flag;
            MemberListType type;
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            FilterHelper(bindingAttr, ref name, out flag, out type);
            CerArrayList<RuntimeFieldInfo> fieldList = this.Cache.GetFieldList(type, name);
            FieldInfo info = null;
            bindingAttr ^= BindingFlags.DeclaredOnly;
            bool flag2 = false;
            for (int i = 0; i < fieldList.Count; i++)
            {
                RuntimeFieldInfo info2 = fieldList[i];
                if ((bindingAttr & info2.BindingFlags) == info2.BindingFlags)
                {
                    if (info != null)
                    {
                        if (object.ReferenceEquals(info2.DeclaringType, info.DeclaringType))
                        {
                            throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                        }
                        if (info.DeclaringType.IsInterface && info2.DeclaringType.IsInterface)
                        {
                            flag2 = true;
                        }
                    }
                    if (((info == null) || info2.DeclaringType.IsSubclassOf(info.DeclaringType)) || info.DeclaringType.IsInterface)
                    {
                        info = info2;
                    }
                }
            }
            if (flag2 && info.DeclaringType.IsInterface)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
            }
            return info;
        }

        [SecuritySafeCritical]
        private FieldInfo[] GetFieldCandidates(string name, BindingFlags bindingAttr, bool allowPrefixLookup)
        {
            bool flag;
            bool flag2;
            MemberListType type;
            FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out flag2, out type);
            CerArrayList<RuntimeFieldInfo> fieldList = this.Cache.GetFieldList(type, name);
            bindingAttr ^= BindingFlags.DeclaredOnly;
            List<FieldInfo> list2 = new List<FieldInfo>(fieldList.Count);
            for (int i = 0; i < fieldList.Count; i++)
            {
                RuntimeFieldInfo memberInfo = fieldList[i];
                if (((bindingAttr & memberInfo.BindingFlags) == memberInfo.BindingFlags) && (!flag || FilterApplyPrefixLookup(memberInfo, name, flag2)))
                {
                    list2.Add(memberInfo);
                }
            }
            return list2.ToArray();
        }

        [SecuritySafeCritical]
        internal static FieldInfo GetFieldInfo(IRuntimeFieldInfo fieldHandle)
        {
            return GetFieldInfo(RuntimeFieldHandle.GetApproxDeclaringType(fieldHandle), fieldHandle);
        }

        [SecuritySafeCritical]
        internal static FieldInfo GetFieldInfo(RuntimeType reflectedType, IRuntimeFieldInfo field)
        {
            RuntimeFieldHandleInternal internal2 = field.Value;
            if (reflectedType == null)
            {
                reflectedType = RuntimeFieldHandle.GetApproxDeclaringType(internal2);
            }
            else
            {
                RuntimeType approxDeclaringType = RuntimeFieldHandle.GetApproxDeclaringType(internal2);
                if ((reflectedType != approxDeclaringType) && (!RuntimeFieldHandle.AcquiresContextFromThis(internal2) || !RuntimeTypeHandle.CompareCanonicalHandles(approxDeclaringType, reflectedType)))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveFieldHandle"), new object[] { reflectedType.ToString(), approxDeclaringType.ToString() }));
                }
            }
            FieldInfo info = reflectedType.Cache.GetField(internal2);
            GC.KeepAlive(field);
            return info;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return this.GetFieldCandidates(null, bindingAttr, false);
        }

        [SecuritySafeCritical]
        public override Type[] GetGenericArguments()
        {
            Type[] instantiationPublic = this.GetRootElementType().GetTypeHandleInternal().GetInstantiationPublic();
            if (instantiationPublic == null)
            {
                instantiationPublic = new Type[0];
            }
            return instantiationPublic;
        }

        internal RuntimeType[] GetGenericArgumentsInternal()
        {
            return this.GetRootElementType().GetTypeHandleInternal().GetInstantiationInternal();
        }

        [SecuritySafeCritical]
        public override Type[] GetGenericParameterConstraints()
        {
            if (!this.IsGenericParameter)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
            }
            Type[] constraints = new RuntimeTypeHandle(this).GetConstraints();
            if (constraints == null)
            {
                constraints = new Type[0];
            }
            return constraints;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public override Type GetGenericTypeDefinition()
        {
            if (!this.IsGenericType)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotGenericType"));
            }
            return RuntimeTypeHandle.GetGenericTypeDefinition(this);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void GetGUID(ref Guid result);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        [SecuritySafeCritical]
        public override Type GetInterface(string fullname, bool ignoreCase)
        {
            string str;
            string str2;
            MemberListType type;
            if (fullname == null)
            {
                throw new ArgumentNullException();
            }
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public;
            bindingFlags &= ~BindingFlags.Static;
            if (ignoreCase)
            {
                bindingFlags |= BindingFlags.IgnoreCase;
            }
            SplitName(fullname, out str, out str2);
            FilterHelper(bindingFlags, ref str, out ignoreCase, out type);
            CerArrayList<RuntimeType> interfaceList = this.Cache.GetInterfaceList(type, str);
            RuntimeType type2 = null;
            for (int i = 0; i < interfaceList.Count; i++)
            {
                RuntimeType type3 = interfaceList[i];
                if (FilterApplyType(type3, bindingFlags, str, false, str2))
                {
                    if (type2 != null)
                    {
                        throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                    }
                    type2 = type3;
                }
            }
            return type2;
        }

        [SecuritySafeCritical]
        public override InterfaceMapping GetInterfaceMap(Type ifaceType)
        {
            InterfaceMapping mapping;
            if (this.IsGenericParameter)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_GenericParameter"));
            }
            if (ifaceType == null)
            {
                throw new ArgumentNullException("ifaceType");
            }
            RuntimeType type = ifaceType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "ifaceType");
            }
            RuntimeTypeHandle typeHandleInternal = type.GetTypeHandleInternal();
            this.GetTypeHandleInternal().VerifyInterfaceIsImplemented(typeHandleInternal);
            if (this.IsSzArray && ifaceType.IsGenericType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ArrayGetInterfaceMap"));
            }
            int interfaceMethodSlots = RuntimeTypeHandle.GetInterfaceMethodSlots(type);
            int num2 = 0;
            for (int i = 0; i < interfaceMethodSlots; i++)
            {
                if ((RuntimeMethodHandle.GetAttributes(RuntimeTypeHandle.GetMethodAt(type, i)) & MethodAttributes.Static) != MethodAttributes.PrivateScope)
                {
                    num2++;
                }
            }
            int num4 = interfaceMethodSlots - num2;
            mapping.InterfaceType = ifaceType;
            mapping.TargetType = this;
            mapping.InterfaceMethods = new MethodInfo[num4];
            mapping.TargetMethods = new MethodInfo[num4];
            for (int j = 0; j < interfaceMethodSlots; j++)
            {
                RuntimeMethodHandleInternal methodAt = RuntimeTypeHandle.GetMethodAt(type, j);
                if ((num2 > 0) && ((RuntimeMethodHandle.GetAttributes(methodAt) & MethodAttributes.Static) != MethodAttributes.PrivateScope))
                {
                    num2--;
                }
                else
                {
                    MethodBase methodBase = GetMethodBase(type, methodAt);
                    mapping.InterfaceMethods[j] = (MethodInfo) methodBase;
                    int interfaceMethodImplementationSlot = this.GetTypeHandleInternal().GetInterfaceMethodImplementationSlot(typeHandleInternal, methodAt);
                    if (interfaceMethodImplementationSlot != -1)
                    {
                        RuntimeMethodHandleInternal methodHandle = RuntimeTypeHandle.GetMethodAt(this, interfaceMethodImplementationSlot);
                        MethodBase base3 = GetMethodBase(this, methodHandle);
                        mapping.TargetMethods[j] = (MethodInfo) base3;
                    }
                }
            }
            return mapping;
        }

        [SecuritySafeCritical]
        public override Type[] GetInterfaces()
        {
            CerArrayList<RuntimeType> interfaceList = this.Cache.GetInterfaceList(MemberListType.All, null);
            Type[] target = new Type[interfaceList.Count];
            for (int i = 0; i < interfaceList.Count; i++)
            {
                JitHelpers.UnsafeSetArrayElement(target, i, interfaceList[i]);
            }
            return target;
        }

        [SecuritySafeCritical]
        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            MethodInfo[] sourceArray = new MethodInfo[0];
            ConstructorInfo[] infoArray2 = new ConstructorInfo[0];
            PropertyInfo[] infoArray3 = new PropertyInfo[0];
            EventInfo[] infoArray4 = new EventInfo[0];
            FieldInfo[] infoArray5 = new FieldInfo[0];
            Type[] typeArray = new Type[0];
            if ((type & MemberTypes.Method) != 0)
            {
                sourceArray = this.GetMethodCandidates(name, bindingAttr, CallingConventions.Any, null, true);
            }
            if ((type & MemberTypes.Constructor) != 0)
            {
                infoArray2 = this.GetConstructorCandidates(name, bindingAttr, CallingConventions.Any, null, true);
            }
            if ((type & MemberTypes.Property) != 0)
            {
                infoArray3 = this.GetPropertyCandidates(name, bindingAttr, null, true);
            }
            if ((type & MemberTypes.Event) != 0)
            {
                infoArray4 = this.GetEventCandidates(name, bindingAttr, true);
            }
            if ((type & MemberTypes.Field) != 0)
            {
                infoArray5 = this.GetFieldCandidates(name, bindingAttr, true);
            }
            if ((type & (MemberTypes.NestedType | MemberTypes.TypeInfo)) != 0)
            {
                typeArray = this.GetNestedTypeCandidates(name, bindingAttr, true);
            }
            switch (type)
            {
                case MemberTypes.Constructor:
                    return infoArray2;

                case MemberTypes.Event:
                    return infoArray4;

                case MemberTypes.Field:
                    return infoArray5;

                case MemberTypes.Method:
                    return sourceArray;

                case (MemberTypes.Method | MemberTypes.Constructor):
                {
                    MethodBase[] baseArray = new MethodBase[sourceArray.Length + infoArray2.Length];
                    Array.Copy(sourceArray, baseArray, sourceArray.Length);
                    Array.Copy(infoArray2, 0, baseArray, sourceArray.Length, infoArray2.Length);
                    return baseArray;
                }
                case MemberTypes.Property:
                    return infoArray3;

                case MemberTypes.TypeInfo:
                    return typeArray;

                case MemberTypes.NestedType:
                    return typeArray;
            }
            MemberInfo[] destinationArray = new MemberInfo[((((sourceArray.Length + infoArray2.Length) + infoArray3.Length) + infoArray4.Length) + infoArray5.Length) + typeArray.Length];
            int destinationIndex = 0;
            if (sourceArray.Length > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationIndex, sourceArray.Length);
            }
            destinationIndex += sourceArray.Length;
            if (infoArray2.Length > 0)
            {
                Array.Copy(infoArray2, 0, destinationArray, destinationIndex, infoArray2.Length);
            }
            destinationIndex += infoArray2.Length;
            if (infoArray3.Length > 0)
            {
                Array.Copy(infoArray3, 0, destinationArray, destinationIndex, infoArray3.Length);
            }
            destinationIndex += infoArray3.Length;
            if (infoArray4.Length > 0)
            {
                Array.Copy(infoArray4, 0, destinationArray, destinationIndex, infoArray4.Length);
            }
            destinationIndex += infoArray4.Length;
            if (infoArray5.Length > 0)
            {
                Array.Copy(infoArray5, 0, destinationArray, destinationIndex, infoArray5.Length);
            }
            destinationIndex += infoArray5.Length;
            if (typeArray.Length > 0)
            {
                Array.Copy(typeArray, 0, destinationArray, destinationIndex, typeArray.Length);
            }
            destinationIndex += typeArray.Length;
            return destinationArray;
        }

        [SecuritySafeCritical]
        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            MethodInfo[] sourceArray = this.GetMethodCandidates(null, bindingAttr, CallingConventions.Any, null, false);
            ConstructorInfo[] infoArray2 = this.GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, null, false);
            PropertyInfo[] infoArray3 = this.GetPropertyCandidates(null, bindingAttr, null, false);
            EventInfo[] infoArray4 = this.GetEventCandidates(null, bindingAttr, false);
            FieldInfo[] infoArray5 = this.GetFieldCandidates(null, bindingAttr, false);
            Type[] typeArray = this.GetNestedTypeCandidates(null, bindingAttr, false);
            MemberInfo[] destinationArray = new MemberInfo[((((sourceArray.Length + infoArray2.Length) + infoArray3.Length) + infoArray4.Length) + infoArray5.Length) + typeArray.Length];
            int destinationIndex = 0;
            Array.Copy(sourceArray, 0, destinationArray, destinationIndex, sourceArray.Length);
            destinationIndex += sourceArray.Length;
            Array.Copy(infoArray2, 0, destinationArray, destinationIndex, infoArray2.Length);
            destinationIndex += infoArray2.Length;
            Array.Copy(infoArray3, 0, destinationArray, destinationIndex, infoArray3.Length);
            destinationIndex += infoArray3.Length;
            Array.Copy(infoArray4, 0, destinationArray, destinationIndex, infoArray4.Length);
            destinationIndex += infoArray4.Length;
            Array.Copy(infoArray5, 0, destinationArray, destinationIndex, infoArray5.Length);
            destinationIndex += infoArray5.Length;
            Array.Copy(typeArray, 0, destinationArray, destinationIndex, typeArray.Length);
            destinationIndex += typeArray.Length;
            return destinationArray;
        }

        internal static MethodBase GetMethodBase(IRuntimeMethodInfo methodHandle)
        {
            return GetMethodBase(null, methodHandle);
        }

        internal static MethodBase GetMethodBase(RuntimeModule scope, int typeMetadataToken)
        {
            return GetMethodBase(ModuleHandle.ResolveMethodHandleInternal(scope, typeMetadataToken));
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static MethodBase GetMethodBase(RuntimeType reflectedType, IRuntimeMethodInfo methodHandle)
        {
            MethodBase methodBase = GetMethodBase(reflectedType, methodHandle.Value);
            GC.KeepAlive(methodHandle);
            return methodBase;
        }

        [SecurityCritical]
        internal static MethodBase GetMethodBase(RuntimeType reflectedType, RuntimeMethodHandleInternal methodHandle)
        {
            MethodBase constructor;
            if (RuntimeMethodHandle.IsDynamicMethod(methodHandle))
            {
                Resolver resolver = RuntimeMethodHandle.GetResolver(methodHandle);
                if (resolver != null)
                {
                    return resolver.GetDynamicMethod();
                }
                return null;
            }
            RuntimeType declaringType = RuntimeMethodHandle.GetDeclaringType(methodHandle);
            RuntimeType[] methodInstantiation = null;
            if (reflectedType == null)
            {
                reflectedType = declaringType;
            }
            if ((reflectedType != declaringType) && !reflectedType.IsSubclassOf(declaringType))
            {
                if (reflectedType.IsArray)
                {
                    MethodBase[] baseArray = reflectedType.GetMember(RuntimeMethodHandle.GetName(methodHandle), MemberTypes.Method | MemberTypes.Constructor, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) as MethodBase[];
                    bool flag = false;
                    for (int i = 0; i < baseArray.Length; i++)
                    {
                        IRuntimeMethodInfo info = (IRuntimeMethodInfo) baseArray[i];
                        if (info.Value.Value == methodHandle.Value)
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), new object[] { reflectedType.ToString(), declaringType.ToString() }));
                    }
                }
                else if (!declaringType.IsGenericType)
                {
                    if (!declaringType.IsAssignableFrom(reflectedType))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), new object[] { reflectedType.ToString(), declaringType.ToString() }));
                    }
                }
                else
                {
                    RuntimeType genericTypeDefinition = (RuntimeType) declaringType.GetGenericTypeDefinition();
                    RuntimeType baseType = reflectedType;
                    while (baseType != null)
                    {
                        RuntimeType type4 = baseType;
                        if (type4.IsGenericType && !baseType.IsGenericTypeDefinition)
                        {
                            type4 = (RuntimeType) type4.GetGenericTypeDefinition();
                        }
                        if (type4 == genericTypeDefinition)
                        {
                            break;
                        }
                        baseType = baseType.GetBaseType();
                    }
                    if (baseType == null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), new object[] { reflectedType.ToString(), declaringType.ToString() }));
                    }
                    declaringType = baseType;
                    if (!RuntimeMethodHandle.IsGenericMethodDefinition(methodHandle))
                    {
                        methodInstantiation = RuntimeMethodHandle.GetMethodInstantiationInternal(methodHandle);
                    }
                    methodHandle = RuntimeMethodHandle.GetMethodFromCanonical(methodHandle, declaringType);
                }
            }
            methodHandle = RuntimeMethodHandle.GetStubIfNeeded(methodHandle, declaringType, methodInstantiation);
            if (RuntimeMethodHandle.IsConstructor(methodHandle))
            {
                constructor = reflectedType.Cache.GetConstructor(declaringType, methodHandle);
            }
            else if (RuntimeMethodHandle.HasMethodInstantiation(methodHandle) && !RuntimeMethodHandle.IsGenericMethodDefinition(methodHandle))
            {
                constructor = reflectedType.Cache.GetGenericMethodInfo(methodHandle);
            }
            else
            {
                constructor = reflectedType.Cache.GetMethod(declaringType, methodHandle);
            }
            GC.KeepAlive(methodInstantiation);
            return constructor;
        }

        private MethodInfo[] GetMethodCandidates(string name, BindingFlags bindingAttr, CallingConventions callConv, Type[] types, bool allowPrefixLookup)
        {
            bool flag;
            bool flag2;
            MemberListType type;
            FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out flag2, out type);
            CerArrayList<RuntimeMethodInfo> methodList = this.Cache.GetMethodList(type, name);
            List<MethodInfo> list2 = new List<MethodInfo>(methodList.Count);
            for (int i = 0; i < methodList.Count; i++)
            {
                RuntimeMethodInfo method = methodList[i];
                if (FilterApplyMethodInfo(method, bindingAttr, callConv, types) && (!flag || FilterApplyPrefixLookup(method, name, flag2)))
                {
                    list2.Add(method);
                }
            }
            return list2.ToArray();
        }

        [SecuritySafeCritical]
        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConv, Type[] types, ParameterModifier[] modifiers)
        {
            MethodInfo[] match = this.GetMethodCandidates(name, bindingAttr, callConv, types, false);
            if (match.Length == 0)
            {
                return null;
            }
            if ((types == null) || (types.Length == 0))
            {
                if (match.Length == 1)
                {
                    return match[0];
                }
                if (types == null)
                {
                    for (int i = 1; i < match.Length; i++)
                    {
                        MethodInfo info = match[i];
                        if (!DefaultBinder.CompareMethodSigAndName(info, match[0]))
                        {
                            throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                        }
                    }
                    return (DefaultBinder.FindMostDerivedNewSlotMeth(match, match.Length) as MethodInfo);
                }
            }
            if (binder == null)
            {
                binder = Type.DefaultBinder;
            }
            return (binder.SelectMethod(bindingAttr, match, types, modifiers) as MethodInfo);
        }

        [SecuritySafeCritical]
        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return this.GetMethodCandidates(null, bindingAttr, CallingConventions.Any, null, false);
        }

        [SecuritySafeCritical]
        public override Type GetNestedType(string fullname, BindingFlags bindingAttr)
        {
            bool flag;
            string str;
            string str2;
            MemberListType type;
            if (fullname == null)
            {
                throw new ArgumentNullException();
            }
            bindingAttr &= ~BindingFlags.Static;
            SplitName(fullname, out str, out str2);
            FilterHelper(bindingAttr, ref str, out flag, out type);
            CerArrayList<RuntimeType> nestedTypeList = this.Cache.GetNestedTypeList(type, str);
            RuntimeType type2 = null;
            for (int i = 0; i < nestedTypeList.Count; i++)
            {
                RuntimeType type3 = nestedTypeList[i];
                if (FilterApplyType(type3, bindingAttr, str, false, str2))
                {
                    if (type2 != null)
                    {
                        throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                    }
                    type2 = type3;
                }
            }
            return type2;
        }

        private Type[] GetNestedTypeCandidates(string fullname, BindingFlags bindingAttr, bool allowPrefixLookup)
        {
            bool flag;
            bool flag2;
            string str;
            string str2;
            MemberListType type;
            bindingAttr &= ~BindingFlags.Static;
            SplitName(fullname, out str, out str2);
            FilterHelper(bindingAttr, ref str, allowPrefixLookup, out flag, out flag2, out type);
            CerArrayList<RuntimeType> nestedTypeList = this.Cache.GetNestedTypeList(type, str);
            List<Type> list2 = new List<Type>(nestedTypeList.Count);
            for (int i = 0; i < nestedTypeList.Count; i++)
            {
                RuntimeType type2 = nestedTypeList[i];
                if (FilterApplyType(type2, bindingAttr, str, flag, str2))
                {
                    list2.Add(type2);
                }
            }
            return list2.ToArray();
        }

        [SecuritySafeCritical]
        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return this.GetNestedTypeCandidates(null, bindingAttr, false);
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            UnitySerializationHolder.GetUnitySerializationInfo(info, this);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return this.GetPropertyCandidates(null, bindingAttr, null, false);
        }

        private PropertyInfo[] GetPropertyCandidates(string name, BindingFlags bindingAttr, Type[] types, bool allowPrefixLookup)
        {
            bool flag;
            bool flag2;
            MemberListType type;
            FilterHelper(bindingAttr, ref name, allowPrefixLookup, out flag, out flag2, out type);
            CerArrayList<RuntimePropertyInfo> propertyList = this.Cache.GetPropertyList(type, name);
            bindingAttr ^= BindingFlags.DeclaredOnly;
            List<PropertyInfo> list2 = new List<PropertyInfo>(propertyList.Count);
            for (int i = 0; i < propertyList.Count; i++)
            {
                RuntimePropertyInfo memberInfo = propertyList[i];
                if ((((bindingAttr & memberInfo.BindingFlags) == memberInfo.BindingFlags) && (!flag || FilterApplyPrefixLookup(memberInfo, name, flag2))) && ((types == null) || (memberInfo.GetIndexParameters().Length == types.Length)))
                {
                    list2.Add(memberInfo);
                }
            }
            return list2.ToArray();
        }

        [SecuritySafeCritical]
        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            PropertyInfo[] match = this.GetPropertyCandidates(name, bindingAttr, types, false);
            if (binder == null)
            {
                binder = Type.DefaultBinder;
            }
            if (match.Length == 0)
            {
                return null;
            }
            if ((types == null) || (types.Length == 0))
            {
                if (match.Length == 1)
                {
                    if ((returnType != null) && !returnType.IsEquivalentTo(match[0].PropertyType))
                    {
                        return null;
                    }
                    return match[0];
                }
                if (returnType == null)
                {
                    throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                }
            }
            if ((bindingAttr & BindingFlags.ExactBinding) != BindingFlags.Default)
            {
                return DefaultBinder.ExactPropertyBinding(match, returnType, types, modifiers);
            }
            return binder.SelectProperty(bindingAttr, match, returnType, types, modifiers);
        }

        private static PropertyInfo GetPropertyInfo(RuntimeType reflectedType, int tkProperty)
        {
            RuntimePropertyInfo info = null;
            CerArrayList<RuntimePropertyInfo> propertyList = reflectedType.Cache.GetPropertyList(MemberListType.All, null);
            for (int i = 0; i < propertyList.Count; i++)
            {
                info = propertyList[i];
                if (info.MetadataToken == tkProperty)
                {
                    return info;
                }
            }
            throw new SystemException();
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal RuntimeAssembly GetRuntimeAssembly()
        {
            return RuntimeTypeHandle.GetAssembly(this);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal RuntimeModule GetRuntimeModule()
        {
            return RuntimeTypeHandle.GetModule(this);
        }

        internal static RuntimeType GetType(string typeName, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            return RuntimeTypeHandle.GetTypeByName(typeName, throwOnError, ignoreCase, reflectionOnly, ref stackMark, false);
        }

        [SecuritySafeCritical]
        protected override TypeCode GetTypeCodeImpl()
        {
            TypeCode typeCode = this.Cache.TypeCode;
            if (typeCode == TypeCode.Empty)
            {
                switch (RuntimeTypeHandle.GetCorElementType(this))
                {
                    case CorElementType.Boolean:
                        typeCode = TypeCode.Boolean;
                        break;

                    case CorElementType.Char:
                        typeCode = TypeCode.Char;
                        break;

                    case CorElementType.I1:
                        typeCode = TypeCode.SByte;
                        break;

                    case CorElementType.U1:
                        typeCode = TypeCode.Byte;
                        break;

                    case CorElementType.I2:
                        typeCode = TypeCode.Int16;
                        break;

                    case CorElementType.U2:
                        typeCode = TypeCode.UInt16;
                        break;

                    case CorElementType.I4:
                        typeCode = TypeCode.Int32;
                        break;

                    case CorElementType.U4:
                        typeCode = TypeCode.UInt32;
                        break;

                    case CorElementType.I8:
                        typeCode = TypeCode.Int64;
                        break;

                    case CorElementType.U8:
                        typeCode = TypeCode.UInt64;
                        break;

                    case CorElementType.R4:
                        typeCode = TypeCode.Single;
                        break;

                    case CorElementType.R8:
                        typeCode = TypeCode.Double;
                        break;

                    case CorElementType.String:
                        typeCode = TypeCode.String;
                        break;

                    case CorElementType.ValueType:
                        if (!(this == Convert.ConvertTypes[15]))
                        {
                            if (this == Convert.ConvertTypes[0x10])
                            {
                                typeCode = TypeCode.DateTime;
                            }
                            else if (this.IsEnum)
                            {
                                typeCode = Type.GetTypeCode(Enum.GetUnderlyingType(this));
                            }
                            else
                            {
                                typeCode = TypeCode.Object;
                            }
                        }
                        else
                        {
                            typeCode = TypeCode.Decimal;
                        }
                        break;

                    default:
                        if (this == Convert.ConvertTypes[2])
                        {
                            typeCode = TypeCode.DBNull;
                        }
                        else if (this == Convert.ConvertTypes[0x12])
                        {
                            typeCode = TypeCode.String;
                        }
                        else
                        {
                            typeCode = TypeCode.Object;
                        }
                        break;
                }
                this.Cache.TypeCode = typeCode;
            }
            return typeCode;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern Type GetTypeFromCLSIDImpl(Guid clsid, string server, bool throwOnError);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern Type GetTypeFromProgIDImpl(string progID, string server, bool throwOnError);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal sealed override RuntimeTypeHandle GetTypeHandleInternal()
        {
            return new RuntimeTypeHandle(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected override bool HasElementTypeImpl()
        {
            return RuntimeTypeHandle.HasElementType(this);
        }

        [SecuritySafeCritical]
        internal override bool HasProxyAttributeImpl()
        {
            return RuntimeTypeHandle.HasProxyAttribute(this);
        }

        internal void InvalidateCachedNestedType()
        {
            this.Cache.InvalidateCachedNestedType();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern object InvokeDispMethod(string name, BindingFlags invokeAttr, object target, object[] args, bool[] byrefModifiers, int culture, string[] namedParameters);
        [DebuggerStepThrough, SecuritySafeCritical, DebuggerHidden]
        public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
        {
            if (this.IsGenericParameter)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_GenericParameter"));
            }
            if ((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.CreateInstance | BindingFlags.InvokeMethod)) == BindingFlags.Default)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NoAccessSpec"), "bindingFlags");
            }
            if ((bindingFlags & 0xff) == BindingFlags.Default)
            {
                bindingFlags |= BindingFlags.Public | BindingFlags.Instance;
                if ((bindingFlags & BindingFlags.CreateInstance) == BindingFlags.Default)
                {
                    bindingFlags |= BindingFlags.Static;
                }
            }
            if (namedParams != null)
            {
                if (providedArgs != null)
                {
                    if (namedParams.Length > providedArgs.Length)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamTooBig"), "namedParams");
                    }
                }
                else if (namedParams.Length != 0)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamTooBig"), "namedParams");
                }
            }
            if ((target != null) && target.GetType().IsCOMObject)
            {
                if ((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.InvokeMethod)) == BindingFlags.Default)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_COMAccess"), "bindingFlags");
                }
                if (((bindingFlags & BindingFlags.GetProperty) != BindingFlags.Default) && (((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.InvokeMethod)) & ~(BindingFlags.GetProperty | BindingFlags.InvokeMethod)) != BindingFlags.Default))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_PropSetGet"), "bindingFlags");
                }
                if (((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default) && (((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.InvokeMethod)) & ~(BindingFlags.GetProperty | BindingFlags.InvokeMethod)) != BindingFlags.Default))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_PropSetInvoke"), "bindingFlags");
                }
                if (((bindingFlags & BindingFlags.SetProperty) != BindingFlags.Default) && (((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.InvokeMethod)) & ~BindingFlags.SetProperty) != BindingFlags.Default))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
                }
                if (((bindingFlags & BindingFlags.PutDispProperty) != BindingFlags.Default) && (((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.InvokeMethod)) & ~BindingFlags.PutDispProperty) != BindingFlags.Default))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
                }
                if (((bindingFlags & BindingFlags.PutRefDispProperty) != BindingFlags.Default) && (((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.InvokeMethod)) & ~BindingFlags.PutRefDispProperty) != BindingFlags.Default))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
                }
                if (RemotingServices.IsTransparentProxy(target))
                {
                    return ((MarshalByRefObject) target).InvokeMember(name, bindingFlags, binder, providedArgs, modifiers, culture, namedParams);
                }
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                bool[] byrefModifiers = (modifiers == null) ? null : modifiers[0].IsByRefArray;
                int num = (culture == null) ? 0x409 : culture.LCID;
                return this.InvokeDispMethod(name, bindingFlags, target, providedArgs, byrefModifiers, num, namedParams);
            }
            if ((namedParams != null) && (Array.IndexOf<string>(namedParams, null) != -1))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamNull"), "namedParams");
            }
            int num2 = (providedArgs != null) ? providedArgs.Length : 0;
            if (binder == null)
            {
                binder = Type.DefaultBinder;
            }
            Binder defaultBinder = Type.DefaultBinder;
            if ((bindingFlags & BindingFlags.CreateInstance) != BindingFlags.Default)
            {
                if (((bindingFlags & BindingFlags.CreateInstance) != BindingFlags.Default) && ((bindingFlags & (BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.InvokeMethod)) != BindingFlags.Default))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_CreatInstAccess"), "bindingFlags");
                }
                return Activator.CreateInstance(this, bindingFlags, binder, providedArgs, culture);
            }
            if ((bindingFlags & (BindingFlags.PutRefDispProperty | BindingFlags.PutDispProperty)) != BindingFlags.Default)
            {
                bindingFlags |= BindingFlags.SetProperty;
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if ((name.Length == 0) || name.Equals("[DISPID=0]"))
            {
                name = this.GetDefaultMemberName();
                if (name == null)
                {
                    name = "ToString";
                }
            }
            bool flag = (bindingFlags & BindingFlags.GetField) != BindingFlags.Default;
            bool flag2 = (bindingFlags & BindingFlags.SetField) != BindingFlags.Default;
            if (flag || flag2)
            {
                if (flag)
                {
                    if (flag2)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_FldSetGet"), "bindingFlags");
                    }
                    if ((bindingFlags & BindingFlags.SetProperty) != BindingFlags.Default)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_FldGetPropSet"), "bindingFlags");
                    }
                }
                else
                {
                    if (providedArgs == null)
                    {
                        throw new ArgumentNullException("providedArgs");
                    }
                    if ((bindingFlags & BindingFlags.GetProperty) != BindingFlags.Default)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_FldSetPropGet"), "bindingFlags");
                    }
                    if ((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_FldSetInvoke"), "bindingFlags");
                    }
                }
                FieldInfo info = null;
                FieldInfo[] match = this.GetMember(name, MemberTypes.Field, bindingFlags) as FieldInfo[];
                if (match.Length == 1)
                {
                    info = match[0];
                }
                else if (match.Length > 0)
                {
                    info = binder.BindToField(bindingFlags, match, flag ? Empty.Value : providedArgs[0], culture);
                }
                if (info != null)
                {
                    if (info.FieldType.IsArray || object.ReferenceEquals(info.FieldType, typeof(Array)))
                    {
                        int num3;
                        if ((bindingFlags & BindingFlags.GetField) != BindingFlags.Default)
                        {
                            num3 = num2;
                        }
                        else
                        {
                            num3 = num2 - 1;
                        }
                        if (num3 > 0)
                        {
                            int[] indices = new int[num3];
                            for (int i = 0; i < num3; i++)
                            {
                                try
                                {
                                    indices[i] = ((IConvertible) providedArgs[i]).ToInt32(null);
                                }
                                catch (InvalidCastException)
                                {
                                    throw new ArgumentException(Environment.GetResourceString("Arg_IndexMustBeInt"));
                                }
                            }
                            Array array = (Array) info.GetValue(target);
                            if ((bindingFlags & BindingFlags.GetField) != BindingFlags.Default)
                            {
                                return array.GetValue(indices);
                            }
                            array.SetValue(providedArgs[num3], indices);
                            return null;
                        }
                    }
                    if (flag)
                    {
                        if (num2 != 0)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Arg_FldGetArgErr"), "bindingFlags");
                        }
                        return info.GetValue(target);
                    }
                    if (num2 != 1)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_FldSetArgErr"), "bindingFlags");
                    }
                    info.SetValue(target, providedArgs[0], bindingFlags, binder, culture);
                    return null;
                }
                if ((bindingFlags & 0xfff300) == BindingFlags.Default)
                {
                    throw new MissingFieldException(this.FullName, name);
                }
            }
            bool flag3 = (bindingFlags & BindingFlags.GetProperty) != BindingFlags.Default;
            bool flag4 = (bindingFlags & BindingFlags.SetProperty) != BindingFlags.Default;
            if (flag3 || flag4)
            {
                if (flag3)
                {
                    if (flag4)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Arg_PropSetGet"), "bindingFlags");
                    }
                }
                else if ((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_PropSetInvoke"), "bindingFlags");
                }
            }
            MethodInfo[] infoArray2 = null;
            MethodInfo info2 = null;
            if ((bindingFlags & BindingFlags.InvokeMethod) != BindingFlags.Default)
            {
                MethodInfo[] infoArray3 = this.GetMember(name, MemberTypes.Method, bindingFlags) as MethodInfo[];
                List<MethodInfo> list = null;
                for (int j = 0; j < infoArray3.Length; j++)
                {
                    MethodInfo item = infoArray3[j];
                    if (FilterApplyMethodInfo((RuntimeMethodInfo) item, bindingFlags, CallingConventions.Any, new Type[num2]))
                    {
                        if (info2 == null)
                        {
                            info2 = item;
                        }
                        else
                        {
                            if (list == null)
                            {
                                list = new List<MethodInfo>(infoArray3.Length) {
                                    info2
                                };
                            }
                            list.Add(item);
                        }
                    }
                }
                if (list != null)
                {
                    infoArray2 = new MethodInfo[list.Count];
                    list.CopyTo(infoArray2);
                }
            }
            if (((info2 == null) && flag3) || flag4)
            {
                PropertyInfo[] infoArray4 = this.GetMember(name, MemberTypes.Property, bindingFlags) as PropertyInfo[];
                List<MethodInfo> list2 = null;
                for (int k = 0; k < infoArray4.Length; k++)
                {
                    MethodInfo setMethod = null;
                    if (flag4)
                    {
                        setMethod = infoArray4[k].GetSetMethod(true);
                    }
                    else
                    {
                        setMethod = infoArray4[k].GetGetMethod(true);
                    }
                    if ((setMethod != null) && FilterApplyMethodInfo((RuntimeMethodInfo) setMethod, bindingFlags, CallingConventions.Any, new Type[num2]))
                    {
                        if (info2 == null)
                        {
                            info2 = setMethod;
                        }
                        else
                        {
                            if (list2 == null)
                            {
                                list2 = new List<MethodInfo>(infoArray4.Length) {
                                    info2
                                };
                            }
                            list2.Add(setMethod);
                        }
                    }
                }
                if (list2 != null)
                {
                    infoArray2 = new MethodInfo[list2.Count];
                    list2.CopyTo(infoArray2);
                }
            }
            if (info2 == null)
            {
                throw new MissingMethodException(this.FullName, name);
            }
            if (((infoArray2 == null) && (num2 == 0)) && ((info2.GetParametersNoCopy().Length == 0) && ((bindingFlags & BindingFlags.OptionalParamBinding) == BindingFlags.Default)))
            {
                return info2.Invoke(target, bindingFlags, binder, providedArgs, culture);
            }
            if (infoArray2 == null)
            {
                infoArray2 = new MethodInfo[] { info2 };
            }
            if (providedArgs == null)
            {
                providedArgs = new object[0];
            }
            object state = null;
            MethodBase base2 = null;
            try
            {
                base2 = binder.BindToMethod(bindingFlags, infoArray2, ref providedArgs, modifiers, culture, namedParams, out state);
            }
            catch (MissingMethodException)
            {
            }
            if (base2 == null)
            {
                throw new MissingMethodException(this.FullName, name);
            }
            object obj3 = ((MethodInfo) base2).Invoke(target, bindingFlags, binder, providedArgs, culture);
            if (state != null)
            {
                binder.ReorderArgumentArray(ref providedArgs, state);
            }
            return obj3;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected override bool IsArrayImpl()
        {
            return RuntimeTypeHandle.IsArray(this);
        }

        [SecuritySafeCritical]
        public override bool IsAssignableFrom(Type c)
        {
            if (c != null)
            {
                if (object.ReferenceEquals(c, this))
                {
                    return true;
                }
                RuntimeType underlyingSystemType = c.UnderlyingSystemType as RuntimeType;
                if (underlyingSystemType != null)
                {
                    return RuntimeTypeHandle.CanCastTo(underlyingSystemType, this);
                }
                if (c is TypeBuilder)
                {
                    if (c.IsSubclassOf(this))
                    {
                        return true;
                    }
                    if (base.IsInterface)
                    {
                        return c.ImplementInterface(this);
                    }
                    if (this.IsGenericParameter)
                    {
                        Type[] genericParameterConstraints = this.GetGenericParameterConstraints();
                        for (int i = 0; i < genericParameterConstraints.Length; i++)
                        {
                            if (!genericParameterConstraints[i].IsAssignableFrom(c))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected override bool IsByRefImpl()
        {
            return RuntimeTypeHandle.IsByRef(this);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected override bool IsCOMObjectImpl()
        {
            return RuntimeTypeHandle.IsComObject(this, false);
        }

        [SecuritySafeCritical]
        protected override bool IsContextfulImpl()
        {
            return RuntimeTypeHandle.IsContextful(this);
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
            return CustomAttribute.IsDefined(this, underlyingSystemType, inherit);
        }

        public override bool IsEnumDefined(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            RuntimeType t = (RuntimeType) value.GetType();
            if (t.IsEnum)
            {
                if (!t.IsEquivalentTo(this))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType", new object[] { t.ToString(), this.ToString() }));
                }
                t = (RuntimeType) t.GetEnumUnderlyingType();
            }
            if (t == StringType)
            {
                return (Array.IndexOf<object>(Enum.InternalGetNames(this), value) >= 0);
            }
            if (!Type.IsIntegerType(t))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
            }
            RuntimeType underlyingType = Enum.InternalGetUnderlyingType(this);
            if (underlyingType != t)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumUnderlyingTypeAndObjectMustBeSameType", new object[] { t.ToString(), underlyingType.ToString() }));
            }
            ulong[] values = Enum.InternalGetValues(this);
            ulong num = Enum.ToUInt64(value);
            return (Array.BinarySearch<ulong>(values, num) >= 0);
        }

        public override bool IsEquivalentTo(Type other)
        {
            RuntimeType type = other as RuntimeType;
            if (type == null)
            {
                return false;
            }
            return ((type == this) || RuntimeTypeHandle.IsEquivalentTo(this, type));
        }

        [SecuritySafeCritical]
        internal bool IsGenericCOMObjectImpl()
        {
            return RuntimeTypeHandle.IsComObject(this, true);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override bool IsInstanceOfType(object o)
        {
            return RuntimeTypeHandle.IsInstanceOfType(this, o);
        }

        protected override bool IsPointerImpl()
        {
            return RuntimeTypeHandle.IsPointer(this);
        }

        protected override bool IsPrimitiveImpl()
        {
            return RuntimeTypeHandle.IsPrimitive(this);
        }

        internal bool IsSpecialSerializableType()
        {
            RuntimeType baseType = this;
            do
            {
                if ((baseType == DelegateType) || (baseType == EnumType))
                {
                    return true;
                }
                baseType = baseType.GetBaseType();
            }
            while (baseType != null);
            return false;
        }

        [ComVisible(true)]
        public override bool IsSubclassOf(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            RuntimeType type2 = type as RuntimeType;
            if (type2 == null)
            {
                return false;
            }
            for (RuntimeType type3 = this.GetBaseType(); type3 != null; type3 = type3.GetBaseType())
            {
                if (type3 == type2)
                {
                    return true;
                }
            }
            return ((type2 == ObjectType) && (type2 != this));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected override bool IsValueTypeImpl()
        {
            return (((this != typeof(System.ValueType)) && (this != typeof(Enum))) && this.IsSubclassOf(typeof(System.ValueType)));
        }

        [SecuritySafeCritical]
        public override Type MakeArrayType()
        {
            RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
            return handle.MakeSZArray();
        }

        [SecuritySafeCritical]
        public override Type MakeArrayType(int rank)
        {
            if (rank <= 0)
            {
                throw new IndexOutOfRangeException();
            }
            RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
            return handle.MakeArray(rank);
        }

        [SecuritySafeCritical]
        public override Type MakeByRefType()
        {
            RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
            return handle.MakeByRef();
        }

        [SecuritySafeCritical]
        public override Type MakeGenericType(params Type[] instantiation)
        {
            if (instantiation == null)
            {
                throw new ArgumentNullException("instantiation");
            }
            RuntimeType[] genericArguments = new RuntimeType[instantiation.Length];
            if (!this.IsGenericTypeDefinition)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericTypeDefinition", new object[] { this }));
            }
            if (this.GetGenericArguments().Length != instantiation.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_GenericArgsCount"), "instantiation");
            }
            for (int i = 0; i < instantiation.Length; i++)
            {
                Type type = instantiation[i];
                if (type == null)
                {
                    throw new ArgumentNullException();
                }
                RuntimeType type2 = type as RuntimeType;
                if (type2 == null)
                {
                    Type[] typeArray2 = new Type[instantiation.Length];
                    for (int j = 0; j < instantiation.Length; j++)
                    {
                        typeArray2[j] = instantiation[j];
                    }
                    instantiation = typeArray2;
                    return TypeBuilderInstantiation.MakeGenericType(this, instantiation);
                }
                genericArguments[i] = type2;
            }
            RuntimeType[] genericArgumentsInternal = this.GetGenericArgumentsInternal();
            SanityCheckGenericArguments(genericArguments, genericArgumentsInternal);
            Type type3 = null;
            try
            {
                type3 = new RuntimeTypeHandle(this).Instantiate(genericArguments);
            }
            catch (TypeLoadException exception)
            {
                ValidateGenericArguments(this, genericArguments, exception);
                throw exception;
            }
            return type3;
        }

        [SecuritySafeCritical]
        public override Type MakePointerType()
        {
            RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
            return handle.MakePointer();
        }

        internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
        {
            this.m_cachedData = null;
        }

        public static bool operator ==(RuntimeType left, RuntimeType right)
        {
            return object.ReferenceEquals(left, right);
        }

        public static bool operator !=(RuntimeType left, RuntimeType right)
        {
            return !object.ReferenceEquals(left, right);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void PrepareMemberInfoCache(RuntimeTypeHandle rt);
        internal static void SanityCheckGenericArguments(RuntimeType[] genericArguments, RuntimeType[] genericParamters)
        {
            if (genericArguments == null)
            {
                throw new ArgumentNullException();
            }
            for (int i = 0; i < genericArguments.Length; i++)
            {
                if (genericArguments[i] == null)
                {
                    throw new ArgumentNullException();
                }
                ThrowIfTypeNeverValidGenericArgument(genericArguments[i]);
            }
            if (genericArguments.Length != genericParamters.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotEnoughGenArguments", new object[] { genericArguments.Length, genericParamters.Length }));
            }
        }

        private static void SplitName(string fullname, out string name, out string ns)
        {
            name = null;
            ns = null;
            if (fullname != null)
            {
                int length = fullname.LastIndexOf(".", StringComparison.Ordinal);
                if (length != -1)
                {
                    ns = fullname.Substring(0, length);
                    int num2 = (fullname.Length - ns.Length) - 1;
                    if (num2 != 0)
                    {
                        name = fullname.Substring(length + 1, num2);
                    }
                    else
                    {
                        name = "";
                    }
                }
                else
                {
                    name = fullname;
                }
            }
        }

        private static void ThrowIfTypeNeverValidGenericArgument(RuntimeType type)
        {
            if ((type.IsPointer || type.IsByRef) || (type == typeof(void)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NeverValidGenericArgument", new object[] { type.ToString() }));
            }
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            return this.Cache.GetToString();
        }

        [SecurityCritical]
        private object TryChangeType(object value, Binder binder, CultureInfo culture, bool needsSpecialCast)
        {
            if ((binder != null) && (binder != Type.DefaultBinder))
            {
                value = binder.ChangeType(value, this, culture);
                if (this.IsInstanceOfType(value))
                {
                    return value;
                }
                if (base.IsByRef)
                {
                    Type elementType = this.GetElementType();
                    if (elementType.IsInstanceOfType(value) || (value == null))
                    {
                        return AllocateValueType(elementType.TypeHandle.GetRuntimeType(), value, false);
                    }
                }
                else if (value == null)
                {
                    return value;
                }
                if (needsSpecialCast)
                {
                    RuntimeType pointerType;
                    Pointer pointer = value as Pointer;
                    if (pointer != null)
                    {
                        pointerType = pointer.GetPointerType();
                    }
                    else
                    {
                        pointerType = (RuntimeType) value.GetType();
                    }
                    if (CanValueSpecialCast(pointerType, this))
                    {
                        if (pointer != null)
                        {
                            return pointer.GetPointerValue();
                        }
                        return value;
                    }
                }
            }
            throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), new object[] { value.GetType(), this }));
        }

        [SecuritySafeCritical]
        internal static void ValidateGenericArguments(MemberInfo definition, RuntimeType[] genericArguments, Exception e)
        {
            RuntimeType[] typeContext = null;
            RuntimeType[] methodContext = null;
            RuntimeType[] genericArgumentsInternal = null;
            if (definition is Type)
            {
                genericArgumentsInternal = ((RuntimeType) definition).GetGenericArgumentsInternal();
                typeContext = genericArguments;
            }
            else
            {
                RuntimeMethodInfo info = (RuntimeMethodInfo) definition;
                genericArgumentsInternal = info.GetGenericArgumentsInternal();
                methodContext = genericArguments;
                RuntimeType declaringType = (RuntimeType) info.DeclaringType;
                if (declaringType != null)
                {
                    typeContext = declaringType.GetTypeHandleInternal().GetInstantiationInternal();
                }
            }
            for (int i = 0; i < genericArguments.Length; i++)
            {
                Type type3 = genericArguments[i];
                Type type4 = genericArgumentsInternal[i];
                if (!RuntimeTypeHandle.SatisfiesConstraints(type4.GetTypeHandleInternal().GetTypeChecked(), typeContext, methodContext, type3.GetTypeHandleInternal().GetTypeChecked()))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_GenConstraintViolation", new object[] { i.ToString(CultureInfo.CurrentCulture), type3.ToString(), definition.ToString(), type4.ToString() }), e);
                }
            }
        }

        [SecuritySafeCritical]
        private void WrapArgsForInvokeCall(object[] aArgs, int[] aWrapperTypes)
        {
            int length = aArgs.Length;
            for (int i = 0; i < length; i++)
            {
                if (aWrapperTypes[i] == 0)
                {
                    continue;
                }
                if ((aWrapperTypes[i] & 0x10000) != 0)
                {
                    ConstructorInfo constructor;
                    Type elementType = null;
                    bool flag = false;
                    switch ((((DispatchWrapperType) aWrapperTypes[i]) & ~DispatchWrapperType.SafeArray))
                    {
                        case DispatchWrapperType.Currency:
                            elementType = typeof(CurrencyWrapper);
                            break;

                        case DispatchWrapperType.BStr:
                            elementType = typeof(BStrWrapper);
                            flag = true;
                            break;

                        case DispatchWrapperType.Unknown:
                            elementType = typeof(UnknownWrapper);
                            break;

                        case DispatchWrapperType.Dispatch:
                            elementType = typeof(DispatchWrapper);
                            break;

                        case DispatchWrapperType.Error:
                            elementType = typeof(ErrorWrapper);
                            break;
                    }
                    Array array = (Array) aArgs[i];
                    int num3 = array.Length;
                    object[] objArray = (object[]) Array.UnsafeCreateInstance(elementType, num3);
                    if (flag)
                    {
                        constructor = elementType.GetConstructor(new Type[] { typeof(string) });
                    }
                    else
                    {
                        constructor = elementType.GetConstructor(new Type[] { typeof(object) });
                    }
                    for (int j = 0; j < num3; j++)
                    {
                        if (flag)
                        {
                            objArray[j] = constructor.Invoke(new object[] { (string) array.GetValue(j) });
                        }
                        else
                        {
                            objArray[j] = constructor.Invoke(new object[] { array.GetValue(j) });
                        }
                    }
                    aArgs[i] = objArray;
                    continue;
                }
                switch (((DispatchWrapperType) aWrapperTypes[i]))
                {
                    case DispatchWrapperType.Currency:
                        aArgs[i] = new CurrencyWrapper(aArgs[i]);
                        break;

                    case DispatchWrapperType.BStr:
                        aArgs[i] = new BStrWrapper((string) aArgs[i]);
                        break;

                    case DispatchWrapperType.Unknown:
                        aArgs[i] = new UnknownWrapper(aArgs[i]);
                        break;

                    case DispatchWrapperType.Dispatch:
                        aArgs[i] = new DispatchWrapper(aArgs[i]);
                        break;

                    case DispatchWrapperType.Error:
                        aArgs[i] = new ErrorWrapper(aArgs[i]);
                        break;
                }
            }
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.GetRuntimeAssembly();
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                if (!this.IsGenericTypeDefinition && this.ContainsGenericParameters)
                {
                    return null;
                }
                return System.Reflection.Assembly.CreateQualifiedName(this.Assembly.FullName, this.FullName);
            }
        }

        public override Type BaseType
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetBaseType();
            }
        }

        private RuntimeTypeCache Cache
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_cache.IsNull())
                {
                    IntPtr gCHandle = new RuntimeTypeHandle(this).GetGCHandle(GCHandleType.WeakTrackResurrection);
                    if (!Interlocked.CompareExchange(ref this.m_cache, gCHandle, IntPtr.Zero).IsNull())
                    {
                        RuntimeTypeHandle handle2 = new RuntimeTypeHandle(this);
                        if (!handle2.IsCollectible())
                        {
                            GCHandle.InternalFree(gCHandle);
                        }
                    }
                }
                RuntimeTypeCache cache = GCHandle.InternalGet(this.m_cache) as RuntimeTypeCache;
                if (cache == null)
                {
                    cache = new RuntimeTypeCache(this);
                    RuntimeTypeCache cache2 = GCHandle.InternalCompareExchange(this.m_cache, cache, null, false) as RuntimeTypeCache;
                    if (cache2 != null)
                    {
                        cache = cache2;
                    }
                    if (s_typeCache == null)
                    {
                        s_typeCache = new TypeCacheQueue();
                    }
                }
                return cache;
            }
        }

        public override bool ContainsGenericParameters
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return this.GetRootElementType().GetTypeHandleInternal().ContainsGenericVariables();
            }
        }

        public override MethodBase DeclaringMethod
        {
            [SecuritySafeCritical]
            get
            {
                if (!this.IsGenericParameter)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
                }
                IRuntimeMethodInfo declaringMethod = RuntimeTypeHandle.GetDeclaringMethod(this);
                if (declaringMethod == null)
                {
                    return null;
                }
                return GetMethodBase(RuntimeMethodHandle.GetDeclaringType(declaringMethod), declaringMethod);
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return this.Cache.GetEnclosingType();
            }
        }

        internal bool DomainInitialized
        {
            get
            {
                return this.Cache.DomainInitialized;
            }
            set
            {
                this.Cache.DomainInitialized = value;
            }
        }

        private OleAutBinder ForwardCallBinder
        {
            get
            {
                if (s_ForwardCallBinder == null)
                {
                    s_ForwardCallBinder = new OleAutBinder();
                }
                return s_ForwardCallBinder;
            }
        }

        public override string FullName
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                return this.Cache.GetFullName();
            }
        }

        public override System.Reflection.GenericParameterAttributes GenericParameterAttributes
        {
            [SecuritySafeCritical]
            get
            {
                System.Reflection.GenericParameterAttributes attributes;
                if (!this.IsGenericParameter)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
                }
                RuntimeTypeHandle.GetMetadataImport(this).GetGenericParamProps(this.MetadataToken, out attributes);
                return attributes;
            }
        }

        public override int GenericParameterPosition
        {
            [SecuritySafeCritical]
            get
            {
                if (!this.IsGenericParameter)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
                }
                RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
                return handle.GetGenericVariableIndex();
            }
        }

        public override Guid GUID
        {
            [SecuritySafeCritical]
            get
            {
                Guid result = new Guid();
                this.GetGUID(ref result);
                return result;
            }
        }

        public override bool IsEnum
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.GetBaseType() == EnumType);
            }
        }

        public override bool IsGenericParameter
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return RuntimeTypeHandle.IsGenericVariable(this);
            }
        }

        public override bool IsGenericType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return RuntimeTypeHandle.IsGenericType(this);
            }
        }

        public override bool IsGenericTypeDefinition
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return RuntimeTypeHandle.IsGenericTypeDefinition(this);
            }
        }

        internal override bool IsRuntimeType
        {
            get
            {
                return true;
            }
        }

        public override bool IsSecurityCritical
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
                return handle.IsSecurityCritical();
            }
        }

        public override bool IsSecuritySafeCritical
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
                return handle.IsSecuritySafeCritical();
            }
        }

        public override bool IsSecurityTransparent
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                RuntimeTypeHandle handle = new RuntimeTypeHandle(this);
                return handle.IsSecurityTransparent();
            }
        }

        internal override bool IsSzArray
        {
            get
            {
                return RuntimeTypeHandle.IsSzArray(this);
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                if (!base.IsPublic && !base.IsNotPublic)
                {
                    return MemberTypes.NestedType;
                }
                return MemberTypes.TypeInfo;
            }
        }

        public override int MetadataToken
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return RuntimeTypeHandle.GetToken(this);
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
                return this.Cache.GetName();
            }
        }

        public override string Namespace
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                string nameSpace = this.Cache.GetNameSpace();
                if ((nameSpace != null) && (nameSpace.Length != 0))
                {
                    return nameSpace;
                }
                return null;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.DeclaringType;
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

        public override System.Runtime.InteropServices.StructLayoutAttribute StructLayoutAttribute
        {
            [SecuritySafeCritical]
            get
            {
                return (System.Runtime.InteropServices.StructLayoutAttribute) System.Runtime.InteropServices.StructLayoutAttribute.GetCustomAttribute(this);
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                return new RuntimeTypeHandle(this);
            }
        }

        public override Type UnderlyingSystemType
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this;
            }
        }

        private class ActivatorCache
        {
            private RuntimeType.ActivatorCacheEntry[] cache = new RuntimeType.ActivatorCacheEntry[0x10];
            private const int CACHE_SIZE = 0x10;
            private PermissionSet delegateCreatePermissions;
            private ConstructorInfo delegateCtorInfo;
            private int hash_counter;

            internal RuntimeType.ActivatorCacheEntry GetEntry(Type t)
            {
                int index = this.hash_counter;
                for (int i = 0; i < 0x10; i++)
                {
                    RuntimeType.ActivatorCacheEntry ace = this.cache[index];
                    if ((ace != null) && (ace.m_type == t))
                    {
                        if (!ace.m_bFullyInitialized)
                        {
                            this.InitializeCacheEntry(ace);
                        }
                        return ace;
                    }
                    index = (index + 1) & 15;
                }
                return null;
            }

            [SecuritySafeCritical]
            private void InitializeCacheEntry(RuntimeType.ActivatorCacheEntry ace)
            {
                if (!ace.m_type.IsValueType)
                {
                    if (this.delegateCtorInfo == null)
                    {
                        this.InitializeDelegateCreator();
                    }
                    this.delegateCreatePermissions.Assert();
                    object[] parameters = new object[2];
                    parameters[1] = RuntimeMethodHandle.GetFunctionPointer(ace.m_hCtorMethodHandle);
                    CtorDelegate delegate2 = (CtorDelegate) this.delegateCtorInfo.Invoke(parameters);
                    Thread.MemoryBarrier();
                    ace.m_ctor = delegate2;
                }
                ace.m_bFullyInitialized = true;
            }

            [SecuritySafeCritical]
            private void InitializeDelegateCreator()
            {
                PermissionSet set = new PermissionSet(PermissionState.None);
                set.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
                set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
                Thread.MemoryBarrier();
                this.delegateCreatePermissions = set;
                ConstructorInfo constructor = typeof(CtorDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
                Thread.MemoryBarrier();
                this.delegateCtorInfo = constructor;
            }

            internal void SetEntry(RuntimeType.ActivatorCacheEntry ace)
            {
                int index = (this.hash_counter - 1) & 15;
                this.hash_counter = index;
                this.cache[index] = ace;
            }
        }

        private class ActivatorCacheEntry
        {
            internal bool m_bFullyInitialized;
            internal bool m_bNeedSecurityCheck;
            internal CtorDelegate m_ctor;
            internal MethodAttributes m_ctorAttributes;
            internal RuntimeMethodHandleInternal m_hCtorMethodHandle;
            internal Type m_type;

            [SecurityCritical]
            internal ActivatorCacheEntry(Type t, RuntimeMethodHandleInternal rmh, bool bNeedSecurityCheck)
            {
                this.m_type = t;
                this.m_bNeedSecurityCheck = bNeedSecurityCheck;
                this.m_hCtorMethodHandle = rmh;
                if (!this.m_hCtorMethodHandle.IsNullHandle())
                {
                    this.m_ctorAttributes = RuntimeMethodHandle.GetAttributes(this.m_hCtorMethodHandle);
                }
            }
        }

        [Flags]
        private enum DispatchWrapperType
        {
            BStr = 0x20,
            Currency = 0x10,
            Dispatch = 2,
            Error = 8,
            Record = 4,
            SafeArray = 0x10000,
            Unknown = 1
        }

        [Serializable]
        internal class RuntimeTypeCache
        {
            private bool m_bIsDomainInitialized;
            private MemberInfoCache<RuntimeConstructorInfo> m_constructorInfoCache;
            private System.RuntimeType m_enclosingType;
            private MemberInfoCache<RuntimeEventInfo> m_eventInfoCache;
            private MemberInfoCache<RuntimeFieldInfo> m_fieldInfoCache;
            private string m_fullname;
            private MemberInfoCache<System.RuntimeType> m_interfaceCache;
            private bool m_isGlobal;
            private MemberInfoCache<RuntimeMethodInfo> m_methodInfoCache;
            private string m_name;
            private string m_namespace;
            private MemberInfoCache<System.RuntimeType> m_nestedClassesCache;
            private MemberInfoCache<RuntimePropertyInfo> m_propertyInfoCache;
            private System.RuntimeType m_runtimeType;
            private string m_toString;
            private System.TypeCode m_typeCode = System.TypeCode.Empty;
            private WhatsCached m_whatsCached;
            private const int MAXNAMELEN = 0x400;
            private static bool s_dontrunhack = false;
            private static CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo> s_methodInstantiations;

            internal RuntimeTypeCache(System.RuntimeType runtimeType)
            {
                this.m_runtimeType = runtimeType;
                this.m_isGlobal = RuntimeTypeHandle.GetModule(runtimeType).RuntimeType == runtimeType;
                s_dontrunhack = true;
                Prejitinit_HACK();
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            private string ConstructName(ref string name, bool nameSpace, bool fullinst, bool assembly)
            {
                if (name == null)
                {
                    name = new RuntimeTypeHandle(this.m_runtimeType).ConstructName(nameSpace, fullinst, assembly);
                }
                return name;
            }

            internal MethodBase GetConstructor(System.RuntimeType declaringType, RuntimeMethodHandleInternal constructor)
            {
                this.GetMemberCache<RuntimeConstructorInfo>(ref this.m_constructorInfoCache);
                return this.m_constructorInfoCache.AddMethod(declaringType, constructor, CacheType.Constructor);
            }

            internal CerArrayList<RuntimeConstructorInfo> GetConstructorList(MemberListType listType, string name)
            {
                return this.GetMemberList<RuntimeConstructorInfo>(ref this.m_constructorInfoCache, listType, name, CacheType.Constructor);
            }

            [SecuritySafeCritical]
            internal System.RuntimeType GetEnclosingType()
            {
                if ((this.m_whatsCached & WhatsCached.EnclosingType) == WhatsCached.Nothing)
                {
                    this.m_enclosingType = RuntimeTypeHandle.GetDeclaringType(this.GetRuntimeType());
                    this.m_whatsCached |= WhatsCached.EnclosingType;
                }
                return this.m_enclosingType;
            }

            internal CerArrayList<RuntimeEventInfo> GetEventList(MemberListType listType, string name)
            {
                return this.GetMemberList<RuntimeEventInfo>(ref this.m_eventInfoCache, listType, name, CacheType.Event);
            }

            internal FieldInfo GetField(RuntimeFieldHandleInternal field)
            {
                this.GetMemberCache<RuntimeFieldInfo>(ref this.m_fieldInfoCache);
                return this.m_fieldInfoCache.AddField(field);
            }

            internal CerArrayList<RuntimeFieldInfo> GetFieldList(MemberListType listType, string name)
            {
                return this.GetMemberList<RuntimeFieldInfo>(ref this.m_fieldInfoCache, listType, name, CacheType.Field);
            }

            internal string GetFullName()
            {
                if (!this.m_runtimeType.IsGenericTypeDefinition && this.m_runtimeType.ContainsGenericParameters)
                {
                    return null;
                }
                return this.ConstructName(ref this.m_fullname, true, true, false);
            }

            [SecurityCritical]
            internal MethodInfo GetGenericMethodInfo(RuntimeMethodHandleInternal genericMethod)
            {
                if (s_methodInstantiations == null)
                {
                    Interlocked.CompareExchange<CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>>(ref s_methodInstantiations, new CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>(), null);
                }
                CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo> methodInstantiations = s_methodInstantiations;
                LoaderAllocator loaderAllocator = (LoaderAllocator) RuntimeMethodHandle.GetLoaderAllocator(genericMethod);
                if (loaderAllocator != null)
                {
                    if (loaderAllocator.m_methodInstantiations == null)
                    {
                        Interlocked.CompareExchange<CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>>(ref loaderAllocator.m_methodInstantiations, new CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>(), null);
                    }
                    methodInstantiations = loaderAllocator.m_methodInstantiations;
                }
                RuntimeMethodInfo info = new RuntimeMethodInfo(genericMethod, RuntimeMethodHandle.GetDeclaringType(genericMethod), this, RuntimeMethodHandle.GetAttributes(genericMethod), ~BindingFlags.Default, loaderAllocator);
                RuntimeMethodInfo info2 = null;
                info2 = methodInstantiations[info];
                if (info2 != null)
                {
                    return info2;
                }
                bool lockTaken = false;
                bool flag2 = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    Monitor.Enter(methodInstantiations, ref lockTaken);
                    info2 = methodInstantiations[info];
                    if (info2 != null)
                    {
                        return info2;
                    }
                    methodInstantiations.Preallocate(1);
                    flag2 = true;
                }
                finally
                {
                    if (flag2)
                    {
                        methodInstantiations[info] = info;
                    }
                    if (lockTaken)
                    {
                        Monitor.Exit(methodInstantiations);
                    }
                }
                return info;
            }

            internal CerArrayList<System.RuntimeType> GetInterfaceList(MemberListType listType, string name)
            {
                return this.GetMemberList<System.RuntimeType>(ref this.m_interfaceCache, listType, name, CacheType.Interface);
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            private MemberInfoCache<T> GetMemberCache<T>(ref MemberInfoCache<T> m_cache) where T: MemberInfo
            {
                MemberInfoCache<T> cache = m_cache;
                if (cache == null)
                {
                    MemberInfoCache<T> cache2 = new MemberInfoCache<T>(this);
                    cache = Interlocked.CompareExchange<MemberInfoCache<T>>(ref m_cache, cache2, null);
                    if (cache == null)
                    {
                        cache = cache2;
                    }
                }
                return cache;
            }

            private CerArrayList<T> GetMemberList<T>(ref MemberInfoCache<T> m_cache, MemberListType listType, string name, CacheType cacheType) where T: MemberInfo
            {
                return this.GetMemberCache<T>(ref m_cache).GetMemberList(listType, name, cacheType);
            }

            internal MethodBase GetMethod(System.RuntimeType declaringType, RuntimeMethodHandleInternal method)
            {
                this.GetMemberCache<RuntimeMethodInfo>(ref this.m_methodInfoCache);
                return this.m_methodInfoCache.AddMethod(declaringType, method, CacheType.Method);
            }

            internal CerArrayList<RuntimeMethodInfo> GetMethodList(MemberListType listType, string name)
            {
                return this.GetMemberList<RuntimeMethodInfo>(ref this.m_methodInfoCache, listType, name, CacheType.Method);
            }

            internal string GetName()
            {
                return this.ConstructName(ref this.m_name, false, false, false);
            }

            [SecurityCritical]
            internal string GetNameSpace()
            {
                if (this.m_namespace == null)
                {
                    Type rootElementType = this.m_runtimeType.GetRootElementType();
                    while (rootElementType.IsNested)
                    {
                        rootElementType = rootElementType.DeclaringType;
                    }
                    this.m_namespace = RuntimeTypeHandle.GetMetadataImport((System.RuntimeType) rootElementType).GetNamespace(rootElementType.MetadataToken).ToString();
                }
                return this.m_namespace;
            }

            internal CerArrayList<System.RuntimeType> GetNestedTypeList(MemberListType listType, string name)
            {
                return this.GetMemberList<System.RuntimeType>(ref this.m_nestedClassesCache, listType, name, CacheType.NestedType);
            }

            internal CerArrayList<RuntimePropertyInfo> GetPropertyList(MemberListType listType, string name)
            {
                return this.GetMemberList<RuntimePropertyInfo>(ref this.m_propertyInfoCache, listType, name, CacheType.Property);
            }

            internal System.RuntimeType GetRuntimeType()
            {
                return this.m_runtimeType;
            }

            internal string GetToString()
            {
                return this.ConstructName(ref this.m_toString, true, false, false);
            }

            internal void InvalidateCachedNestedType()
            {
                this.m_nestedClassesCache = null;
            }

            [SecuritySafeCritical]
            internal static void Prejitinit_HACK()
            {
                if (!s_dontrunhack)
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        MemberInfoCache<RuntimeMethodInfo> cache = new MemberInfoCache<RuntimeMethodInfo>(null);
                        CerArrayList<RuntimeMethodInfo> list = null;
                        cache.Insert(ref list, "dummy", MemberListType.All);
                        MemberInfoCache<RuntimeConstructorInfo> cache2 = new MemberInfoCache<RuntimeConstructorInfo>(null);
                        CerArrayList<RuntimeConstructorInfo> list2 = null;
                        cache2.Insert(ref list2, "dummy", MemberListType.All);
                        MemberInfoCache<RuntimeFieldInfo> cache3 = new MemberInfoCache<RuntimeFieldInfo>(null);
                        CerArrayList<RuntimeFieldInfo> list3 = null;
                        cache3.Insert(ref list3, "dummy", MemberListType.All);
                        MemberInfoCache<System.RuntimeType> cache4 = new MemberInfoCache<System.RuntimeType>(null);
                        CerArrayList<System.RuntimeType> list4 = null;
                        cache4.Insert(ref list4, "dummy", MemberListType.All);
                        MemberInfoCache<RuntimePropertyInfo> cache5 = new MemberInfoCache<RuntimePropertyInfo>(null);
                        CerArrayList<RuntimePropertyInfo> list5 = null;
                        cache5.Insert(ref list5, "dummy", MemberListType.All);
                        MemberInfoCache<RuntimeEventInfo> cache6 = new MemberInfoCache<RuntimeEventInfo>(null);
                        CerArrayList<RuntimeEventInfo> list6 = null;
                        cache6.Insert(ref list6, "dummy", MemberListType.All);
                    }
                }
            }

            internal bool DomainInitialized
            {
                get
                {
                    return this.m_bIsDomainInitialized;
                }
                set
                {
                    this.m_bIsDomainInitialized = value;
                }
            }

            internal bool IsGlobal
            {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                get
                {
                    return this.m_isGlobal;
                }
            }

            internal System.RuntimeType RuntimeType
            {
                get
                {
                    return this.m_runtimeType;
                }
            }

            internal System.TypeCode TypeCode
            {
                get
                {
                    return this.m_typeCode;
                }
                set
                {
                    this.m_typeCode = value;
                }
            }

            internal enum CacheType
            {
                Method,
                Constructor,
                Field,
                Property,
                Event,
                Interface,
                NestedType
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Filter
            {
                private Utf8String m_name;
                private MemberListType m_listType;
                private uint m_nameHash;
                [SecurityCritical]
                public unsafe Filter(byte* pUtf8Name, int cUtf8Name, MemberListType listType)
                {
                    this.m_name = new Utf8String((void*) pUtf8Name, cUtf8Name);
                    this.m_listType = listType;
                    this.m_nameHash = 0;
                    if (this.RequiresStringComparison())
                    {
                        this.m_nameHash = this.m_name.HashCaseInsensitive();
                    }
                }

                public bool Match(Utf8String name)
                {
                    bool flag = true;
                    if (this.m_listType == MemberListType.CaseSensitive)
                    {
                        return this.m_name.Equals(name);
                    }
                    if (this.m_listType == MemberListType.CaseInsensitive)
                    {
                        flag = this.m_name.EqualsCaseInsensitive(name);
                    }
                    return flag;
                }

                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
                public bool RequiresStringComparison()
                {
                    if (this.m_listType != MemberListType.CaseSensitive)
                    {
                        return (this.m_listType == MemberListType.CaseInsensitive);
                    }
                    return true;
                }

                public uint GetHashToMatch()
                {
                    return this.m_nameHash;
                }
            }

            [Serializable]
            private class MemberInfoCache<T> where T: MemberInfo
            {
                private bool m_cacheComplete;
                private CerHashtable<string, CerArrayList<T>> m_cisMemberInfos;
                private CerHashtable<string, CerArrayList<T>> m_csMemberInfos;
                private CerArrayList<T> m_root;
                private RuntimeType.RuntimeTypeCache m_runtimeTypeCache;

                [SecuritySafeCritical]
                static MemberInfoCache()
                {
                    RuntimeType.PrepareMemberInfoCache(typeof(RuntimeType.RuntimeTypeCache.MemberInfoCache<T>).TypeHandle);
                }

                [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
                internal MemberInfoCache(RuntimeType.RuntimeTypeCache runtimeTypeCache)
                {
                    Mda.MemberInfoCacheCreation();
                    this.m_runtimeTypeCache = runtimeTypeCache;
                    this.m_cacheComplete = false;
                }

                private static void AddElementTypes(Type template, IList<Type> types)
                {
                    if (template.HasElementType)
                    {
                        RuntimeType.RuntimeTypeCache.MemberInfoCache<T>.AddElementTypes(template.GetElementType(), types);
                        for (int i = 0; i < types.Count; i++)
                        {
                            if (template.IsArray)
                            {
                                if (template.IsSzArray)
                                {
                                    types[i] = types[i].MakeArrayType();
                                }
                                else
                                {
                                    types[i] = types[i].MakeArrayType(template.GetArrayRank());
                                }
                            }
                            else if (template.IsPointer)
                            {
                                types[i] = types[i].MakePointerType();
                            }
                        }
                    }
                }

                [SecuritySafeCritical]
                internal FieldInfo AddField(RuntimeFieldHandleInternal field)
                {
                    List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>(1);
                    FieldAttributes attributes = RuntimeFieldHandle.GetAttributes(field);
                    bool isPublic = (attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
                    bool isStatic = (attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope;
                    bool isInherited = RuntimeFieldHandle.GetApproxDeclaringType(field) != this.ReflectedType;
                    BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
                    list.Add(new RtFieldInfo(field, this.ReflectedType, this.m_runtimeTypeCache, bindingFlags));
                    CerArrayList<T> list2 = new CerArrayList<T>((List<T>) list);
                    this.Insert(ref list2, null, MemberListType.HandleToInfo);
                    return (FieldInfo) list2[0];
                }

                [SecuritySafeCritical]
                internal MethodBase AddMethod(RuntimeType declaringType, RuntimeMethodHandleInternal method, RuntimeType.RuntimeTypeCache.CacheType cacheType)
                {
                    object obj2 = null;
                    MethodAttributes methodAttributes = RuntimeMethodHandle.GetAttributes(method);
                    bool isPublic = (methodAttributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
                    bool isStatic = (methodAttributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
                    bool isInherited = declaringType != this.ReflectedType;
                    BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
                    switch (cacheType)
                    {
                        case RuntimeType.RuntimeTypeCache.CacheType.Method:
                        {
                            List<RuntimeMethodInfo> list = new List<RuntimeMethodInfo>(1) {
                                new RuntimeMethodInfo(method, declaringType, this.m_runtimeTypeCache, methodAttributes, bindingFlags, null)
                            };
                            obj2 = list;
                            break;
                        }
                        case RuntimeType.RuntimeTypeCache.CacheType.Constructor:
                        {
                            List<RuntimeConstructorInfo> list2 = new List<RuntimeConstructorInfo>(1) {
                                new RuntimeConstructorInfo(method, declaringType, this.m_runtimeTypeCache, methodAttributes, bindingFlags)
                            };
                            obj2 = list2;
                            break;
                        }
                    }
                    CerArrayList<T> list3 = new CerArrayList<T>((List<T>) obj2);
                    this.Insert(ref list3, null, MemberListType.HandleToInfo);
                    return (MethodBase) list3[0];
                }

                [SecurityCritical]
                private unsafe List<T> GetListByName(char* pName, int cNameLen, byte* pUtf8Name, int cUtf8Name, MemberListType listType, RuntimeType.RuntimeTypeCache.CacheType cacheType)
                {
                    if (cNameLen != 0)
                    {
                        Encoding.UTF8.GetBytes(pName, cNameLen, pUtf8Name, cUtf8Name);
                    }
                    RuntimeType.RuntimeTypeCache.Filter filter = new RuntimeType.RuntimeTypeCache.Filter(pUtf8Name, cUtf8Name, listType);
                    switch (cacheType)
                    {
                        case RuntimeType.RuntimeTypeCache.CacheType.Method:
                            return (this.PopulateMethods(filter) as List<T>);

                        case RuntimeType.RuntimeTypeCache.CacheType.Constructor:
                            return (this.PopulateConstructors(filter) as List<T>);

                        case RuntimeType.RuntimeTypeCache.CacheType.Field:
                            return (this.PopulateFields(filter) as List<T>);

                        case RuntimeType.RuntimeTypeCache.CacheType.Property:
                            return (this.PopulateProperties(filter) as List<T>);

                        case RuntimeType.RuntimeTypeCache.CacheType.Event:
                            return (this.PopulateEvents(filter) as List<T>);

                        case RuntimeType.RuntimeTypeCache.CacheType.Interface:
                            return (this.PopulateInterfaces(filter) as List<T>);

                        case RuntimeType.RuntimeTypeCache.CacheType.NestedType:
                            return (this.PopulateNestedClasses(filter) as List<T>);
                    }
                    return null;
                }

                internal CerArrayList<T> GetMemberList(MemberListType listType, string name, RuntimeType.RuntimeTypeCache.CacheType cacheType)
                {
                    CerArrayList<T> list = null;
                    switch (listType)
                    {
                        case MemberListType.All:
                            if (!this.m_cacheComplete)
                            {
                                return this.Populate(null, listType, cacheType);
                            }
                            return this.m_root;

                        case MemberListType.CaseSensitive:
                            if (this.m_csMemberInfos != null)
                            {
                                list = this.m_csMemberInfos[name];
                                if (list == null)
                                {
                                    return this.Populate(name, listType, cacheType);
                                }
                                return list;
                            }
                            return this.Populate(name, listType, cacheType);
                    }
                    if (this.m_cisMemberInfos == null)
                    {
                        return this.Populate(name, listType, cacheType);
                    }
                    list = this.m_cisMemberInfos[name];
                    if (list == null)
                    {
                        return this.Populate(name, listType, cacheType);
                    }
                    return list;
                }

                [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
                internal void Insert(ref CerArrayList<T> list, string name, MemberListType listType)
                {
                    bool lockTaken = false;
                    bool flag2 = false;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        Monitor.Enter(this, ref lockTaken);
                        if (listType == MemberListType.CaseSensitive)
                        {
                            if (this.m_csMemberInfos == null)
                            {
                                this.m_csMemberInfos = new CerHashtable<string, CerArrayList<T>>();
                            }
                            else
                            {
                                this.m_csMemberInfos.Preallocate(1);
                            }
                        }
                        else if (listType == MemberListType.CaseInsensitive)
                        {
                            if (this.m_cisMemberInfos == null)
                            {
                                this.m_cisMemberInfos = new CerHashtable<string, CerArrayList<T>>();
                            }
                            else
                            {
                                this.m_cisMemberInfos.Preallocate(1);
                            }
                        }
                        if (this.m_root == null)
                        {
                            this.m_root = new CerArrayList<T>(list.Count);
                        }
                        else
                        {
                            this.m_root.Preallocate(list.Count);
                        }
                        flag2 = true;
                    }
                    finally
                    {
                        try
                        {
                            if (flag2)
                            {
                                if (listType == MemberListType.CaseSensitive)
                                {
                                    CerArrayList<T> list2 = this.m_csMemberInfos[name];
                                    if (list2 == null)
                                    {
                                        this.MergeWithGlobalList(list);
                                        this.m_csMemberInfos[name] = list;
                                    }
                                    else
                                    {
                                        list = list2;
                                    }
                                }
                                else if (listType == MemberListType.CaseInsensitive)
                                {
                                    CerArrayList<T> list3 = this.m_cisMemberInfos[name];
                                    if (list3 == null)
                                    {
                                        this.MergeWithGlobalList(list);
                                        this.m_cisMemberInfos[name] = list;
                                    }
                                    else
                                    {
                                        list = list3;
                                    }
                                }
                                else
                                {
                                    this.MergeWithGlobalList(list);
                                }
                                if (listType == MemberListType.All)
                                {
                                    this.m_cacheComplete = true;
                                }
                            }
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Monitor.Exit(this);
                            }
                        }
                    }
                }

                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                private void MergeWithGlobalList(CerArrayList<T> list)
                {
                    int count = this.m_root.Count;
                    for (int i = 0; i < list.Count; i++)
                    {
                        T local = list[i];
                        T o = default(T);
                        bool flag = false;
                        for (int j = 0; j < count; j++)
                        {
                            o = this.m_root[j];
                            if (local.CacheEquals(o))
                            {
                                list.Replace(i, o);
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            this.m_root.Add(local);
                        }
                    }
                }

                [SecuritySafeCritical]
                private unsafe CerArrayList<T> Populate(string name, MemberListType listType, RuntimeType.RuntimeTypeCache.CacheType cacheType)
                {
                    List<T> list = null;
                    if (((name == null) || (name.Length == 0)) || (((cacheType == RuntimeType.RuntimeTypeCache.CacheType.Constructor) && (name.FirstChar != '.')) && (name.FirstChar != '*')))
                    {
                        list = this.GetListByName(null, 0, null, 0, listType, cacheType);
                    }
                    else
                    {
                        int length = name.Length;
                        fixed (char* str = ((char*) name))
                        {
                            char* chars = str;
                            int byteCount = Encoding.UTF8.GetByteCount(chars, length);
                            if (byteCount > 0x400)
                            {
                                fixed (byte* numRef = new byte[byteCount])
                                {
                                    list = this.GetListByName(chars, length, numRef, byteCount, listType, cacheType);
                                }
                            }
                            else
                            {
                                byte* numPtr = stackalloc byte[(IntPtr) byteCount];
                                list = this.GetListByName(chars, length, numPtr, byteCount, listType, cacheType);
                            }
                        }
                    }
                    CerArrayList<T> list2 = new CerArrayList<T>(list);
                    this.Insert(ref list2, name, listType);
                    return list2;
                }

                [SecuritySafeCritical]
                private List<RuntimeConstructorInfo> PopulateConstructors(RuntimeType.RuntimeTypeCache.Filter filter)
                {
                    List<RuntimeConstructorInfo> list = new List<RuntimeConstructorInfo>();
                    if (!this.ReflectedType.IsGenericParameter)
                    {
                        RuntimeType reflectedType = this.ReflectedType;
                        RuntimeTypeHandle.IntroducedMethodEnumerator enumerator = RuntimeTypeHandle.GetIntroducedMethods(reflectedType).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            RuntimeMethodHandleInternal current = enumerator.Current;
                            if (!filter.RequiresStringComparison() || (RuntimeMethodHandle.MatchesNameHash(current, filter.GetHashToMatch()) && filter.Match(RuntimeMethodHandle.GetUtf8Name(current))))
                            {
                                MethodAttributes methodAttributes = RuntimeMethodHandle.GetAttributes(current);
                                if (((methodAttributes & MethodAttributes.RTSpecialName) != MethodAttributes.PrivateScope) && !RuntimeMethodHandle.IsILStub(current))
                                {
                                    bool isPublic = (methodAttributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
                                    bool isStatic = (methodAttributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
                                    bool isInherited = false;
                                    BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
                                    RuntimeConstructorInfo item = new RuntimeConstructorInfo(RuntimeMethodHandle.GetStubIfNeeded(current, reflectedType, null), this.ReflectedType, this.m_runtimeTypeCache, methodAttributes, bindingFlags);
                                    list.Add(item);
                                }
                            }
                        }
                    }
                    return list;
                }

                [SecuritySafeCritical]
                private List<RuntimeEventInfo> PopulateEvents(RuntimeType.RuntimeTypeCache.Filter filter)
                {
                    Dictionary<string, RuntimeEventInfo> csEventInfos = new Dictionary<string, RuntimeEventInfo>();
                    RuntimeType reflectedType = this.ReflectedType;
                    List<RuntimeEventInfo> list = new List<RuntimeEventInfo>();
                    if ((RuntimeTypeHandle.GetAttributes(reflectedType) & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
                    {
                        while (RuntimeTypeHandle.IsGenericVariable(reflectedType))
                        {
                            reflectedType = reflectedType.GetBaseType();
                        }
                        while (reflectedType != null)
                        {
                            this.PopulateEvents(filter, reflectedType, csEventInfos, list);
                            reflectedType = RuntimeTypeHandle.GetBaseType(reflectedType);
                        }
                        return list;
                    }
                    this.PopulateEvents(filter, reflectedType, csEventInfos, list);
                    return list;
                }

                [SecuritySafeCritical]
                private unsafe void PopulateEvents(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, Dictionary<string, RuntimeEventInfo> csEventInfos, List<RuntimeEventInfo> list)
                {
                    int token = RuntimeTypeHandle.GetToken(declaringType);
                    if (!MetadataToken.IsNullToken(token))
                    {
                        MetadataImport metadataImport = RuntimeTypeHandle.GetMetadataImport(declaringType);
                        int count = metadataImport.EnumEventsCount(token);
                        int* result = (int*) stackalloc byte[(((IntPtr) count) * 4)];
                        metadataImport.EnumEvents(token, result, count);
                        this.PopulateEvents(filter, declaringType, metadataImport, result, count, csEventInfos, list);
                    }
                }

                [SecurityCritical]
                private unsafe void PopulateEvents(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, MetadataImport scope, int* tkEvents, int cAssociates, Dictionary<string, RuntimeEventInfo> csEventInfos, List<RuntimeEventInfo> list)
                {
                    for (int i = 0; i < cAssociates; i++)
                    {
                        bool flag;
                        int mdToken = tkEvents[i];
                        if (filter.RequiresStringComparison())
                        {
                            Utf8String name = scope.GetName(mdToken);
                            if (!filter.Match(name))
                            {
                                continue;
                            }
                        }
                        RuntimeEventInfo item = new RuntimeEventInfo(mdToken, declaringType, this.m_runtimeTypeCache, out flag);
                        if (((declaringType == this.m_runtimeTypeCache.GetRuntimeType()) || !flag) && (csEventInfos.GetValueOrDefault(item.Name) == null))
                        {
                            csEventInfos[item.Name] = item;
                            list.Add(item);
                        }
                    }
                }

                [SecuritySafeCritical]
                private List<RuntimeFieldInfo> PopulateFields(RuntimeType.RuntimeTypeCache.Filter filter)
                {
                    List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>();
                    RuntimeType reflectedType = this.ReflectedType;
                    while (RuntimeTypeHandle.IsGenericVariable(reflectedType))
                    {
                        reflectedType = reflectedType.GetBaseType();
                    }
                    while (reflectedType != null)
                    {
                        this.PopulateRtFields(filter, reflectedType, list);
                        this.PopulateLiteralFields(filter, reflectedType, list);
                        reflectedType = RuntimeTypeHandle.GetBaseType(reflectedType);
                    }
                    if (this.ReflectedType.IsGenericParameter)
                    {
                        Type[] typeArray = this.ReflectedType.BaseType.GetInterfaces();
                        for (int i = 0; i < typeArray.Length; i++)
                        {
                            this.PopulateLiteralFields(filter, (RuntimeType) typeArray[i], list);
                            this.PopulateRtFields(filter, (RuntimeType) typeArray[i], list);
                        }
                        return list;
                    }
                    Type[] interfaces = RuntimeTypeHandle.GetInterfaces(this.ReflectedType);
                    if (interfaces != null)
                    {
                        for (int j = 0; j < interfaces.Length; j++)
                        {
                            this.PopulateLiteralFields(filter, (RuntimeType) interfaces[j], list);
                            this.PopulateRtFields(filter, (RuntimeType) interfaces[j], list);
                        }
                    }
                    return list;
                }

                [SecuritySafeCritical]
                private List<RuntimeType> PopulateInterfaces(RuntimeType.RuntimeTypeCache.Filter filter)
                {
                    List<RuntimeType> list = new List<RuntimeType>();
                    RuntimeType reflectedType = this.ReflectedType;
                    if (!RuntimeTypeHandle.IsGenericVariable(reflectedType))
                    {
                        Type[] interfaces = RuntimeTypeHandle.GetInterfaces(reflectedType);
                        if (interfaces != null)
                        {
                            for (int k = 0; k < interfaces.Length; k++)
                            {
                                RuntimeType type = (RuntimeType) interfaces[k];
                                if (!filter.RequiresStringComparison() || filter.Match(RuntimeTypeHandle.GetUtf8Name(type)))
                                {
                                    list.Add(type);
                                }
                            }
                        }
                        if (this.ReflectedType.IsSzArray)
                        {
                            RuntimeType elementType = (RuntimeType) this.ReflectedType.GetElementType();
                            if (elementType.IsPointer)
                            {
                                return list;
                            }
                            RuntimeType type4 = (RuntimeType) typeof(IList<>).MakeGenericType(new Type[] { elementType });
                            if (!type4.IsAssignableFrom(this.ReflectedType))
                            {
                                return list;
                            }
                            if (filter.Match(RuntimeTypeHandle.GetUtf8Name(type4)))
                            {
                                list.Add(type4);
                            }
                            foreach (RuntimeType type5 in type4.GetInterfaces())
                            {
                                if (type5.IsGenericType && filter.Match(RuntimeTypeHandle.GetUtf8Name(type5)))
                                {
                                    list.Add(type5);
                                }
                            }
                        }
                        return list;
                    }
                    List<RuntimeType> list2 = new List<RuntimeType>();
                    foreach (RuntimeType type6 in reflectedType.GetGenericParameterConstraints())
                    {
                        if (type6.IsInterface)
                        {
                            list2.Add(type6);
                        }
                        Type[] typeArray4 = type6.GetInterfaces();
                        for (int m = 0; m < typeArray4.Length; m++)
                        {
                            list2.Add(typeArray4[m] as RuntimeType);
                        }
                    }
                    Dictionary<RuntimeType, RuntimeType> dictionary = new Dictionary<RuntimeType, RuntimeType>();
                    for (int i = 0; i < list2.Count; i++)
                    {
                        RuntimeType key = list2[i];
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary[key] = key;
                        }
                    }
                    RuntimeType[] array = new RuntimeType[dictionary.Values.Count];
                    dictionary.Values.CopyTo(array, 0);
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (!filter.RequiresStringComparison() || filter.Match(RuntimeTypeHandle.GetUtf8Name(array[j])))
                        {
                            list.Add(array[j]);
                        }
                    }
                    return list;
                }

                [SecuritySafeCritical]
                private unsafe void PopulateLiteralFields(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, List<RuntimeFieldInfo> list)
                {
                    int token = RuntimeTypeHandle.GetToken(declaringType);
                    if (!MetadataToken.IsNullToken(token))
                    {
                        MetadataImport metadataImport = RuntimeTypeHandle.GetMetadataImport(declaringType);
                        int count = metadataImport.EnumFieldsCount(token);
                        int* result = (int*) stackalloc byte[(((IntPtr) count) * 4)];
                        metadataImport.EnumFields(token, result, count);
                        for (int i = 0; i < count; i++)
                        {
                            FieldAttributes attributes;
                            int mdToken = result[i];
                            metadataImport.GetFieldDefProps(mdToken, out attributes);
                            FieldAttributes attributes2 = attributes & FieldAttributes.FieldAccessMask;
                            if ((attributes & FieldAttributes.Literal) != FieldAttributes.PrivateScope)
                            {
                                bool isInherited = declaringType != this.ReflectedType;
                                if (!isInherited || (attributes2 != FieldAttributes.Private))
                                {
                                    if (filter.RequiresStringComparison())
                                    {
                                        Utf8String name = metadataImport.GetName(mdToken);
                                        if (!filter.Match(name))
                                        {
                                            continue;
                                        }
                                    }
                                    bool isPublic = attributes2 == FieldAttributes.Public;
                                    bool isStatic = (attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope;
                                    BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
                                    RuntimeFieldInfo item = new MdFieldInfo(mdToken, attributes, declaringType.GetTypeHandleInternal(), this.m_runtimeTypeCache, bindingFlags);
                                    list.Add(item);
                                }
                            }
                        }
                    }
                }

                [SecuritySafeCritical]
                private unsafe List<RuntimeMethodInfo> PopulateMethods(RuntimeType.RuntimeTypeCache.Filter filter)
                {
                    bool* flagPtr;
                    bool isValueType;
                    List<RuntimeMethodInfo> list = new List<RuntimeMethodInfo>();
                    RuntimeType reflectedType = this.ReflectedType;
                    if ((RuntimeTypeHandle.GetAttributes(reflectedType) & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
                    {
                        while (RuntimeTypeHandle.IsGenericVariable(reflectedType))
                        {
                            reflectedType = reflectedType.GetBaseType();
                        }
                        flagPtr = (bool*) stackalloc byte[((IntPtr) RuntimeTypeHandle.GetNumVirtuals(reflectedType))];
                        isValueType = reflectedType.IsValueType;
                    }
                    else
                    {
                        RuntimeTypeHandle.IntroducedMethodEnumerator enumerator = RuntimeTypeHandle.GetIntroducedMethods(reflectedType).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            RuntimeMethodHandleInternal current = enumerator.Current;
                            if (!filter.RequiresStringComparison() || (RuntimeMethodHandle.MatchesNameHash(current, filter.GetHashToMatch()) && filter.Match(RuntimeMethodHandle.GetUtf8Name(current))))
                            {
                                MethodAttributes methodAttributes = RuntimeMethodHandle.GetAttributes(current);
                                bool isPublic = (methodAttributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
                                bool isStatic = (methodAttributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
                                bool isInherited = false;
                                BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
                                if (((methodAttributes & MethodAttributes.RTSpecialName) == MethodAttributes.PrivateScope) && !RuntimeMethodHandle.IsILStub(current))
                                {
                                    RuntimeMethodInfo item = new RuntimeMethodInfo(RuntimeMethodHandle.GetStubIfNeeded(current, reflectedType, null), reflectedType, this.m_runtimeTypeCache, methodAttributes, bindingFlags, null);
                                    list.Add(item);
                                }
                            }
                        }
                        return list;
                    }
                    do
                    {
                        int numVirtuals = RuntimeTypeHandle.GetNumVirtuals(reflectedType);
                        RuntimeTypeHandle.IntroducedMethodEnumerator enumerator4 = RuntimeTypeHandle.GetIntroducedMethods(reflectedType).GetEnumerator();
                        while (enumerator4.MoveNext())
                        {
                            RuntimeMethodHandleInternal method = enumerator4.Current;
                            if (!filter.RequiresStringComparison() || (RuntimeMethodHandle.MatchesNameHash(method, filter.GetHashToMatch()) && filter.Match(RuntimeMethodHandle.GetUtf8Name(method))))
                            {
                                MethodAttributes attributes = RuntimeMethodHandle.GetAttributes(method);
                                MethodAttributes attributes3 = attributes & MethodAttributes.MemberAccessMask;
                                if (((attributes & MethodAttributes.RTSpecialName) == MethodAttributes.PrivateScope) && !RuntimeMethodHandle.IsILStub(method))
                                {
                                    bool flag6 = false;
                                    int slot = 0;
                                    if ((attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope)
                                    {
                                        slot = RuntimeMethodHandle.GetSlot(method);
                                        flag6 = slot < numVirtuals;
                                    }
                                    bool flag7 = attributes3 == MethodAttributes.Private;
                                    bool flag8 = flag6 & flag7;
                                    bool flag9 = reflectedType != this.ReflectedType;
                                    if ((!flag9 || !flag7) || flag8)
                                    {
                                        if (flag6)
                                        {
                                            if (*(((sbyte*) (flagPtr + slot))) != 0)
                                            {
                                                continue;
                                            }
                                            *((sbyte*) (flagPtr + slot)) = 1;
                                        }
                                        else if (isValueType && ((attributes & (MethodAttributes.Abstract | MethodAttributes.Virtual)) != MethodAttributes.PrivateScope))
                                        {
                                            continue;
                                        }
                                        bool flag10 = attributes3 == MethodAttributes.Public;
                                        bool flag11 = (attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
                                        BindingFlags flags2 = RuntimeType.FilterPreCalculate(flag10, flag9, flag11);
                                        RuntimeMethodInfo info2 = new RuntimeMethodInfo(RuntimeMethodHandle.GetStubIfNeeded(method, reflectedType, null), reflectedType, this.m_runtimeTypeCache, attributes, flags2, null);
                                        list.Add(info2);
                                    }
                                }
                            }
                        }
                        reflectedType = RuntimeTypeHandle.GetBaseType(reflectedType);
                    }
                    while (reflectedType != null);
                    return list;
                }

                [SecuritySafeCritical]
                private unsafe List<RuntimeType> PopulateNestedClasses(RuntimeType.RuntimeTypeCache.Filter filter)
                {
                    List<RuntimeType> list = new List<RuntimeType>();
                    RuntimeType reflectedType = this.ReflectedType;
                    while (RuntimeTypeHandle.IsGenericVariable(reflectedType))
                    {
                        reflectedType = reflectedType.GetBaseType();
                    }
                    int token = RuntimeTypeHandle.GetToken(reflectedType);
                    if (!MetadataToken.IsNullToken(token))
                    {
                        RuntimeModule module = RuntimeTypeHandle.GetModule(reflectedType);
                        MetadataImport metadataImport = ModuleHandle.GetMetadataImport(module);
                        int count = metadataImport.EnumNestedTypesCount(token);
                        int* result = (int*) stackalloc byte[(((IntPtr) count) * 4)];
                        metadataImport.EnumNestedTypes(token, result, count);
                        for (int i = 0; i < count; i++)
                        {
                            RuntimeType type = null;
                            try
                            {
                                type = ModuleHandle.ResolveTypeHandleInternal(module, result[i], null, null);
                            }
                            catch (TypeLoadException)
                            {
                                continue;
                            }
                            if (!filter.RequiresStringComparison() || filter.Match(RuntimeTypeHandle.GetUtf8Name(type)))
                            {
                                list.Add(type);
                            }
                        }
                    }
                    return list;
                }

                [SecuritySafeCritical]
                private List<RuntimePropertyInfo> PopulateProperties(RuntimeType.RuntimeTypeCache.Filter filter)
                {
                    RuntimeType reflectedType = this.ReflectedType;
                    List<RuntimePropertyInfo> list = new List<RuntimePropertyInfo>();
                    if ((RuntimeTypeHandle.GetAttributes(reflectedType) & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
                    {
                        while (RuntimeTypeHandle.IsGenericVariable(reflectedType))
                        {
                            reflectedType = reflectedType.GetBaseType();
                        }
                        Dictionary<string, List<RuntimePropertyInfo>> csPropertyInfos = new Dictionary<string, List<RuntimePropertyInfo>>();
                        bool[] usedSlots = new bool[RuntimeTypeHandle.GetNumVirtuals(reflectedType)];
                        do
                        {
                            this.PopulateProperties(filter, reflectedType, csPropertyInfos, usedSlots, list);
                            reflectedType = RuntimeTypeHandle.GetBaseType(reflectedType);
                        }
                        while (reflectedType != null);
                        return list;
                    }
                    this.PopulateProperties(filter, reflectedType, null, null, list);
                    return list;
                }

                [SecuritySafeCritical]
                private unsafe void PopulateProperties(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, Dictionary<string, List<RuntimePropertyInfo>> csPropertyInfos, bool[] usedSlots, List<RuntimePropertyInfo> list)
                {
                    int token = RuntimeTypeHandle.GetToken(declaringType);
                    if (!MetadataToken.IsNullToken(token))
                    {
                        MetadataImport metadataImport = RuntimeTypeHandle.GetMetadataImport(declaringType);
                        int count = metadataImport.EnumPropertiesCount(token);
                        int* result = (int*) stackalloc byte[(((IntPtr) count) * 4)];
                        metadataImport.EnumProperties(token, result, count);
                        this.PopulateProperties(filter, declaringType, result, count, csPropertyInfos, usedSlots, list);
                    }
                }

                [SecurityCritical]
                private unsafe void PopulateProperties(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, int* tkProperties, int cProperties, Dictionary<string, List<RuntimePropertyInfo>> csPropertyInfos, bool[] usedSlots, List<RuntimePropertyInfo> list)
                {
                    RuntimeModule module = RuntimeTypeHandle.GetModule(declaringType);
                    int numVirtuals = RuntimeTypeHandle.GetNumVirtuals(declaringType);
                    for (int i = 0; i < cProperties; i++)
                    {
                        bool flag;
                        int propertyToken = tkProperties[i];
                        if (filter.RequiresStringComparison())
                        {
                            if (!ModuleHandle.ContainsPropertyMatchingHash(module, propertyToken, filter.GetHashToMatch()))
                            {
                                continue;
                            }
                            Utf8String name = declaringType.GetRuntimeModule().MetadataImport.GetName(propertyToken);
                            if (!filter.Match(name))
                            {
                                continue;
                            }
                        }
                        RuntimePropertyInfo item = new RuntimePropertyInfo(propertyToken, declaringType, this.m_runtimeTypeCache, out flag);
                        if (usedSlots != null)
                        {
                            if ((declaringType != this.ReflectedType) && flag)
                            {
                                continue;
                            }
                            RuntimeMethodInfo getMethod = (RuntimeMethodInfo) item.GetGetMethod();
                            if (getMethod != null)
                            {
                                int slot = RuntimeMethodHandle.GetSlot(getMethod);
                                if (slot < numVirtuals)
                                {
                                    if (usedSlots[slot])
                                    {
                                        continue;
                                    }
                                    usedSlots[slot] = true;
                                }
                            }
                            else
                            {
                                RuntimeMethodInfo setMethod = (RuntimeMethodInfo) item.GetSetMethod();
                                if (setMethod != null)
                                {
                                    int index = RuntimeMethodHandle.GetSlot(setMethod);
                                    if (index < numVirtuals)
                                    {
                                        if (usedSlots[index])
                                        {
                                            continue;
                                        }
                                        usedSlots[index] = true;
                                    }
                                }
                            }
                            List<RuntimePropertyInfo> valueOrDefault = csPropertyInfos.GetValueOrDefault(item.Name);
                            if (valueOrDefault == null)
                            {
                                valueOrDefault = new List<RuntimePropertyInfo>(1);
                                csPropertyInfos[item.Name] = valueOrDefault;
                            }
                            else
                            {
                                for (int j = 0; j < valueOrDefault.Count; j++)
                                {
                                    if (item.EqualsSig(valueOrDefault[j]))
                                    {
                                        valueOrDefault = null;
                                        break;
                                    }
                                }
                            }
                            if (valueOrDefault == null)
                            {
                                continue;
                            }
                            valueOrDefault.Add(item);
                        }
                        list.Add(item);
                    }
                }

                [SecuritySafeCritical]
                private unsafe void PopulateRtFields(RuntimeType.RuntimeTypeCache.Filter filter, RuntimeType declaringType, List<RuntimeFieldInfo> list)
                {
                    IntPtr* result = (IntPtr*) stackalloc byte[(((IntPtr) 0x40) * sizeof(IntPtr))];
                    int count = 0x40;
                    if (!RuntimeTypeHandle.GetFields(declaringType, result, &count))
                    {
                        fixed (IntPtr* ptrRef = new IntPtr[count])
                        {
                            RuntimeTypeHandle.GetFields(declaringType, ptrRef, &count);
                            this.PopulateRtFields(filter, ptrRef, count, declaringType, list);
                        }
                    }
                    else if (count > 0)
                    {
                        this.PopulateRtFields(filter, result, count, declaringType, list);
                    }
                }

                [SecurityCritical]
                private unsafe void PopulateRtFields(RuntimeType.RuntimeTypeCache.Filter filter, IntPtr* ppFieldHandles, int count, RuntimeType declaringType, List<RuntimeFieldInfo> list)
                {
                    bool flag = RuntimeTypeHandle.HasInstantiation(declaringType) && !RuntimeTypeHandle.ContainsGenericVariables(declaringType);
                    bool isInherited = declaringType != this.ReflectedType;
                    for (int i = 0; i < count; i++)
                    {
                        RuntimeFieldHandleInternal field = new RuntimeFieldHandleInternal(ppFieldHandles[i]);
                        if (!filter.RequiresStringComparison() || (RuntimeFieldHandle.MatchesNameHash(field, filter.GetHashToMatch()) && filter.Match(RuntimeFieldHandle.GetUtf8Name(field))))
                        {
                            FieldAttributes attributes = RuntimeFieldHandle.GetAttributes(field);
                            FieldAttributes attributes2 = attributes & FieldAttributes.FieldAccessMask;
                            if (!isInherited || (attributes2 != FieldAttributes.Private))
                            {
                                bool isPublic = attributes2 == FieldAttributes.Public;
                                bool isStatic = (attributes & FieldAttributes.Static) != FieldAttributes.PrivateScope;
                                BindingFlags bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
                                if (flag && isStatic)
                                {
                                    field = RuntimeFieldHandle.GetStaticFieldForGenericType(field, declaringType);
                                }
                                RuntimeFieldInfo item = new RtFieldInfo(field, declaringType, this.m_runtimeTypeCache, bindingFlags);
                                list.Add(item);
                            }
                        }
                    }
                }

                internal RuntimeType ReflectedType
                {
                    get
                    {
                        return this.m_runtimeTypeCache.GetRuntimeType();
                    }
                }
            }

            internal enum WhatsCached
            {
                Nothing,
                EnclosingType
            }
        }

        private class TypeCacheQueue
        {
            private object[] liveCache = new object[4];
            private const int QUEUE_SIZE = 4;

            internal TypeCacheQueue()
            {
            }
        }
    }
}


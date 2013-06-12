namespace System
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct RuntimeMethodHandle : ISerializable
    {
        [ForceTokenStabilization]
        private IRuntimeMethodInfo m_value;
        internal static IRuntimeMethodInfo EnsureNonNullMethodInfo(IRuntimeMethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(null, Environment.GetResourceString("Arg_InvalidHandle"));
            }
            return method;
        }

        internal static RuntimeMethodHandle EmptyHandle
        {
            [SecuritySafeCritical]
            get
            {
                return new RuntimeMethodHandle();
            }
        }
        internal RuntimeMethodHandle(IRuntimeMethodInfo method)
        {
            this.m_value = method;
        }

        internal IRuntimeMethodInfo GetMethodInfo()
        {
            return this.m_value;
        }

        [SecurityCritical, ForceTokenStabilization]
        private static IntPtr GetValueInternal(RuntimeMethodHandle rmh)
        {
            return rmh.Value;
        }

        [SecurityCritical]
        private RuntimeMethodHandle(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            MethodBase base2 = (MethodBase) info.GetValue("MethodObj", typeof(MethodBase));
            this.m_value = base2.MethodHandle.m_value;
            if (this.m_value == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
            }
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            if (this.m_value == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFieldState"));
            }
            MethodBase methodBase = RuntimeType.GetMethodBase(this.m_value);
            info.AddValue("MethodObj", methodBase, typeof(MethodBase));
        }

        public IntPtr Value
        {
            [SecurityCritical]
            get
            {
                if (this.m_value == null)
                {
                    return IntPtr.Zero;
                }
                return this.m_value.Value.Value;
            }
        }
        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            return ValueType.GetHashCodeOfPtr(this.Value);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            if (!(obj is RuntimeMethodHandle))
            {
                return false;
            }
            RuntimeMethodHandle handle = (RuntimeMethodHandle) obj;
            return (handle.Value == this.Value);
        }

        public static bool operator ==(RuntimeMethodHandle left, RuntimeMethodHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeMethodHandle left, RuntimeMethodHandle right)
        {
            return !left.Equals(right);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public bool Equals(RuntimeMethodHandle handle)
        {
            return (handle.Value == this.Value);
        }

        [SecuritySafeCritical]
        internal bool IsNullHandle()
        {
            return (this.m_value == null);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern IntPtr GetFunctionPointer(RuntimeMethodHandleInternal handle);
        [SecurityCritical]
        public IntPtr GetFunctionPointer()
        {
            IntPtr functionPointer = GetFunctionPointer(EnsureNonNullMethodInfo(this.m_value).Value);
            GC.KeepAlive(this.m_value);
            return functionPointer;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void CheckLinktimeDemands(IRuntimeMethodInfo method, RuntimeModule module, bool isDecoratedTargetSecurityTransparent);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool _IsVisibleFromModule(IRuntimeMethodInfo method, RuntimeModule source);
        [SecuritySafeCritical]
        internal static bool IsVisibleFromModule(IRuntimeMethodInfo method, RuntimeModule source)
        {
            return _IsVisibleFromModule(method, source.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool _IsVisibleFromType(IRuntimeMethodInfo handle, RuntimeTypeHandle source);
        [SecuritySafeCritical]
        internal static bool IsVisibleFromType(IRuntimeMethodInfo handle, RuntimeTypeHandle source)
        {
            return _IsVisibleFromType(handle, source);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern IRuntimeMethodInfo _GetCurrentMethod(ref StackCrawlMark stackMark);
        [SecuritySafeCritical]
        internal static IRuntimeMethodInfo GetCurrentMethod(ref StackCrawlMark stackMark)
        {
            return _GetCurrentMethod(ref stackMark);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern MethodAttributes GetAttributes(RuntimeMethodHandleInternal method);
        [SecurityCritical]
        internal static MethodAttributes GetAttributes(IRuntimeMethodInfo method)
        {
            MethodAttributes attributes = GetAttributes(method.Value);
            GC.KeepAlive(method);
            return attributes;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern MethodImplAttributes GetImplAttributes(IRuntimeMethodInfo method);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void ConstructInstantiation(IRuntimeMethodInfo method, StringHandleOnStack retString);
        [SecuritySafeCritical]
        internal static string ConstructInstantiation(IRuntimeMethodInfo method)
        {
            string s = null;
            ConstructInstantiation(EnsureNonNullMethodInfo(method), JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeType GetDeclaringType(RuntimeMethodHandleInternal method);
        [SecuritySafeCritical]
        internal static RuntimeType GetDeclaringType(IRuntimeMethodInfo method)
        {
            RuntimeType declaringType = GetDeclaringType(method.Value);
            GC.KeepAlive(method);
            return declaringType;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetSlot(RuntimeMethodHandleInternal method);
        [SecurityCritical]
        internal static int GetSlot(IRuntimeMethodInfo method)
        {
            int slot = GetSlot(method.Value);
            GC.KeepAlive(method);
            return slot;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetMethodDef(IRuntimeMethodInfo method);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string GetName(RuntimeMethodHandleInternal method);
        [SecurityCritical]
        internal static string GetName(IRuntimeMethodInfo method)
        {
            string name = GetName(method.Value);
            GC.KeepAlive(method);
            return name;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void* _GetUtf8Name(RuntimeMethodHandleInternal method);
        [SecurityCritical]
        internal static Utf8String GetUtf8Name(RuntimeMethodHandleInternal method)
        {
            return new Utf8String(_GetUtf8Name(method));
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool MatchesNameHash(RuntimeMethodHandleInternal method, uint hash);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, DebuggerHidden, DebuggerStepThrough]
        private static extern object _InvokeMethodFast(IRuntimeMethodInfo method, object target, object[] arguments, ref SignatureStruct sig, MethodAttributes methodAttributes, RuntimeType typeOwner);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, DebuggerStepThrough, DebuggerHidden]
        internal static object InvokeMethodFast(IRuntimeMethodInfo method, object target, object[] arguments, Signature sig, MethodAttributes methodAttributes, RuntimeType typeOwner)
        {
            SignatureStruct signature = sig.m_signature;
            object obj2 = _InvokeMethodFast(method, target, arguments, ref signature, methodAttributes, typeOwner);
            sig.m_signature = signature;
            return obj2;
        }

        [SecurityCritical]
        internal static INVOCATION_FLAGS GetSecurityFlags(IRuntimeMethodInfo handle)
        {
            INVOCATION_FLAGS specialSecurityFlags = (INVOCATION_FLAGS) GetSpecialSecurityFlags(handle);
            if ((((specialSecurityFlags & INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY) == INVOCATION_FLAGS.INVOCATION_FLAGS_UNKNOWN) && IsSecurityCritical(handle)) && !IsSecuritySafeCritical(handle))
            {
                specialSecurityFlags |= INVOCATION_FLAGS.INVOCATION_FLAGS_NEED_SECURITY;
            }
            return specialSecurityFlags;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern uint GetSpecialSecurityFlags(IRuntimeMethodInfo method);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void PerformSecurityCheck(object obj, RuntimeMethodHandleInternal method, RuntimeType parent, uint invocationFlags);
        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static void PerformSecurityCheck(object obj, IRuntimeMethodInfo method, RuntimeType parent, uint invocationFlags)
        {
            PerformSecurityCheck(obj, method.Value, parent, invocationFlags);
            GC.KeepAlive(method);
        }

        [MethodImpl(MethodImplOptions.InternalCall), DebuggerHidden, SecurityCritical, DebuggerStepThrough]
        private static extern object _InvokeConstructor(IRuntimeMethodInfo method, object[] args, ref SignatureStruct signature, RuntimeType declaringType);
        [DebuggerHidden, SecuritySafeCritical, DebuggerStepThrough, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static object InvokeConstructor(IRuntimeMethodInfo method, object[] args, SignatureStruct signature, RuntimeType declaringType)
        {
            return _InvokeConstructor(method, args, ref signature, declaringType);
        }

        [MethodImpl(MethodImplOptions.InternalCall), DebuggerStepThrough, SecurityCritical, DebuggerHidden]
        private static extern void _SerializationInvoke(IRuntimeMethodInfo method, object target, ref SignatureStruct declaringTypeSig, SerializationInfo info, StreamingContext context);
        [SecuritySafeCritical, DebuggerStepThrough, DebuggerHidden]
        internal static void SerializationInvoke(IRuntimeMethodInfo method, object target, SignatureStruct declaringTypeSig, SerializationInfo info, StreamingContext context)
        {
            _SerializationInvoke(method, target, ref declaringTypeSig, info, context);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsILStub(RuntimeMethodHandleInternal method);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool _IsTokenSecurityTransparent(RuntimeModule module, int metaDataToken);
        [SecurityCritical]
        internal static bool IsTokenSecurityTransparent(Module module, int metaDataToken)
        {
            return _IsTokenSecurityTransparent(module.ModuleHandle.GetRuntimeModule(), metaDataToken);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool _IsSecurityCritical(IRuntimeMethodInfo method);
        [SecuritySafeCritical]
        internal static bool IsSecurityCritical(IRuntimeMethodInfo method)
        {
            return _IsSecurityCritical(method);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool _IsSecuritySafeCritical(IRuntimeMethodInfo method);
        [SecuritySafeCritical]
        internal static bool IsSecuritySafeCritical(IRuntimeMethodInfo method)
        {
            return _IsSecuritySafeCritical(method);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool _IsSecurityTransparent(IRuntimeMethodInfo method);
        [SecuritySafeCritical]
        internal static bool IsSecurityTransparent(IRuntimeMethodInfo method)
        {
            return _IsSecurityTransparent(method);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetMethodInstantiation(RuntimeMethodHandleInternal method, ObjectHandleOnStack types, bool fAsRuntimeTypeArray);
        [SecuritySafeCritical]
        internal static RuntimeType[] GetMethodInstantiationInternal(IRuntimeMethodInfo method)
        {
            RuntimeType[] o = null;
            GetMethodInstantiation(EnsureNonNullMethodInfo(method).Value, JitHelpers.GetObjectHandleOnStack<RuntimeType[]>(ref o), true);
            GC.KeepAlive(method);
            return o;
        }

        [SecuritySafeCritical]
        internal static RuntimeType[] GetMethodInstantiationInternal(RuntimeMethodHandleInternal method)
        {
            RuntimeType[] o = null;
            GetMethodInstantiation(method, JitHelpers.GetObjectHandleOnStack<RuntimeType[]>(ref o), true);
            return o;
        }

        [SecuritySafeCritical]
        internal static Type[] GetMethodInstantiationPublic(IRuntimeMethodInfo method)
        {
            RuntimeType[] o = null;
            GetMethodInstantiation(EnsureNonNullMethodInfo(method).Value, JitHelpers.GetObjectHandleOnStack<RuntimeType[]>(ref o), false);
            GC.KeepAlive(method);
            return o;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool HasMethodInstantiation(RuntimeMethodHandleInternal method);
        [SecuritySafeCritical]
        internal static bool HasMethodInstantiation(IRuntimeMethodInfo method)
        {
            bool flag = HasMethodInstantiation(method.Value);
            GC.KeepAlive(method);
            return flag;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeMethodHandleInternal GetStubIfNeeded(RuntimeMethodHandleInternal method, RuntimeType declaringType, RuntimeType[] methodInstantiation);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeMethodHandleInternal GetMethodFromCanonical(RuntimeMethodHandleInternal method, RuntimeType declaringType);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsGenericMethodDefinition(RuntimeMethodHandleInternal method);
        [SecuritySafeCritical]
        internal static bool IsGenericMethodDefinition(IRuntimeMethodInfo method)
        {
            bool flag = IsGenericMethodDefinition(method.Value);
            GC.KeepAlive(method);
            return flag;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool IsTypicalMethodDefinition(IRuntimeMethodInfo method);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetTypicalMethodDefinition(IRuntimeMethodInfo method, ObjectHandleOnStack outMethod);
        [SecuritySafeCritical]
        internal static IRuntimeMethodInfo GetTypicalMethodDefinition(IRuntimeMethodInfo method)
        {
            if (!IsTypicalMethodDefinition(method))
            {
                GetTypicalMethodDefinition(method, JitHelpers.GetObjectHandleOnStack<IRuntimeMethodInfo>(ref method));
            }
            return method;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void StripMethodInstantiation(IRuntimeMethodInfo method, ObjectHandleOnStack outMethod);
        [SecuritySafeCritical]
        internal static IRuntimeMethodInfo StripMethodInstantiation(IRuntimeMethodInfo method)
        {
            IRuntimeMethodInfo o = method;
            StripMethodInstantiation(method, JitHelpers.GetObjectHandleOnStack<IRuntimeMethodInfo>(ref o));
            return o;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool IsDynamicMethod(RuntimeMethodHandleInternal method);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void Destroy(RuntimeMethodHandleInternal method);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern Resolver GetResolver(RuntimeMethodHandleInternal method);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetCallerType(StackCrawlMarkHandle stackMark, ObjectHandleOnStack retType);
        [SecuritySafeCritical]
        internal static RuntimeType GetCallerType(ref StackCrawlMark stackMark)
        {
            RuntimeType o = null;
            GetCallerType(JitHelpers.GetStackCrawlMarkHandle(ref stackMark), JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern MethodBody GetMethodBody(IRuntimeMethodInfo method, RuntimeType declaringType);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsConstructor(RuntimeMethodHandleInternal method);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object GetLoaderAllocator(RuntimeMethodHandleInternal method);
    }
}


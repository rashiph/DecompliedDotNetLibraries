namespace System
{
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct ModuleHandle
    {
        public static readonly ModuleHandle EmptyHandle;
        private RuntimeModule m_ptr;
        [SecuritySafeCritical]
        private static ModuleHandle GetEmptyMH()
        {
            return new ModuleHandle();
        }

        internal ModuleHandle(RuntimeModule module)
        {
            this.m_ptr = module;
        }

        internal RuntimeModule GetRuntimeModule()
        {
            return this.m_ptr;
        }

        [SecuritySafeCritical]
        internal bool IsNullHandle()
        {
            return (this.m_ptr == null);
        }

        public override int GetHashCode()
        {
            if (this.m_ptr == null)
            {
                return 0;
            }
            return this.m_ptr.GetHashCode();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public override bool Equals(object obj)
        {
            if (!(obj is ModuleHandle))
            {
                return false;
            }
            ModuleHandle handle = (ModuleHandle) obj;
            return (handle.m_ptr == this.m_ptr);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public bool Equals(ModuleHandle handle)
        {
            return (handle.m_ptr == this.m_ptr);
        }

        public static bool operator ==(ModuleHandle left, ModuleHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleHandle left, ModuleHandle right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern IRuntimeMethodInfo GetDynamicMethod(DynamicMethod method, RuntimeModule module, string name, byte[] sig, Resolver resolver);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetToken(RuntimeModule module);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static void ValidateModulePointer(RuntimeModule module)
        {
            if (module == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NullModuleHandle"));
            }
        }

        public RuntimeTypeHandle GetRuntimeTypeHandleFromMetadataToken(int typeToken)
        {
            return this.ResolveTypeHandle(typeToken);
        }

        public RuntimeTypeHandle ResolveTypeHandle(int typeToken)
        {
            return new RuntimeTypeHandle(ResolveTypeHandleInternal(this.GetRuntimeModule(), typeToken, null, null));
        }

        public RuntimeTypeHandle ResolveTypeHandle(int typeToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
        {
            return new RuntimeTypeHandle(ResolveTypeHandleInternal(this.GetRuntimeModule(), typeToken, typeInstantiationContext, methodInstantiationContext));
        }

        [SecuritySafeCritical]
        internal static unsafe RuntimeType ResolveTypeHandleInternal(RuntimeModule module, int typeToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
        {
            int num;
            int num2;
            ValidateModulePointer(module);
            if (!GetMetadataImport(module).IsValidToken(typeToken))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { typeToken, new ModuleHandle(module) }));
            }
            IntPtr[] ptrArray = RuntimeTypeHandle.CopyRuntimeTypeHandles(typeInstantiationContext, out num);
            IntPtr[] ptrArray2 = RuntimeTypeHandle.CopyRuntimeTypeHandles(methodInstantiationContext, out num2);
            fixed (IntPtr* ptrRef = ptrArray)
            {
                fixed (IntPtr* ptrRef2 = ptrArray2)
                {
                    RuntimeType o = null;
                    ResolveType(module, typeToken, ptrRef, num, ptrRef2, num2, JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
                    GC.KeepAlive(typeInstantiationContext);
                    GC.KeepAlive(methodInstantiationContext);
                    return o;
                }
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern unsafe void ResolveType(RuntimeModule module, int typeToken, IntPtr* typeInstArgs, int typeInstCount, IntPtr* methodInstArgs, int methodInstCount, ObjectHandleOnStack type);
        public RuntimeMethodHandle GetRuntimeMethodHandleFromMetadataToken(int methodToken)
        {
            return this.ResolveMethodHandle(methodToken);
        }

        public RuntimeMethodHandle ResolveMethodHandle(int methodToken)
        {
            return this.ResolveMethodHandle(methodToken, null, null);
        }

        internal static IRuntimeMethodInfo ResolveMethodHandleInternal(RuntimeModule module, int methodToken)
        {
            return ResolveMethodHandleInternal(module, methodToken, null, null);
        }

        [SecuritySafeCritical]
        public RuntimeMethodHandle ResolveMethodHandle(int methodToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
        {
            return new RuntimeMethodHandle(ResolveMethodHandleInternal(this.GetRuntimeModule(), methodToken, typeInstantiationContext, methodInstantiationContext));
        }

        [SecuritySafeCritical]
        internal static IRuntimeMethodInfo ResolveMethodHandleInternal(RuntimeModule module, int methodToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
        {
            int num;
            int num2;
            IntPtr[] ptrArray = RuntimeTypeHandle.CopyRuntimeTypeHandles(typeInstantiationContext, out num);
            IntPtr[] ptrArray2 = RuntimeTypeHandle.CopyRuntimeTypeHandles(methodInstantiationContext, out num2);
            RuntimeMethodHandleInternal methodHandleValue = ResolveMethodHandleInternalCore(module, methodToken, ptrArray, num, ptrArray2, num2);
            IRuntimeMethodInfo info = new RuntimeMethodInfoStub(methodHandleValue, RuntimeMethodHandle.GetLoaderAllocator(methodHandleValue));
            GC.KeepAlive(typeInstantiationContext);
            GC.KeepAlive(methodInstantiationContext);
            return info;
        }

        [SecurityCritical]
        internal static unsafe RuntimeMethodHandleInternal ResolveMethodHandleInternalCore(RuntimeModule module, int methodToken, IntPtr[] typeInstantiationContext, int typeInstCount, IntPtr[] methodInstantiationContext, int methodInstCount)
        {
            ValidateModulePointer(module);
            if (!GetMetadataImport(module.GetNativeHandle()).IsValidToken(methodToken))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { methodToken, new ModuleHandle(module) }));
            }
            fixed (IntPtr* ptrRef = typeInstantiationContext)
            {
                fixed (IntPtr* ptrRef2 = methodInstantiationContext)
                {
                    return ResolveMethod(module.GetNativeHandle(), methodToken, ptrRef, typeInstCount, ptrRef2, methodInstCount);
                }
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern unsafe RuntimeMethodHandleInternal ResolveMethod(RuntimeModule module, int methodToken, IntPtr* typeInstArgs, int typeInstCount, IntPtr* methodInstArgs, int methodInstCount);
        public RuntimeFieldHandle GetRuntimeFieldHandleFromMetadataToken(int fieldToken)
        {
            return this.ResolveFieldHandle(fieldToken);
        }

        public RuntimeFieldHandle ResolveFieldHandle(int fieldToken)
        {
            return new RuntimeFieldHandle(ResolveFieldHandleInternal(this.GetRuntimeModule(), fieldToken, null, null));
        }

        public RuntimeFieldHandle ResolveFieldHandle(int fieldToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
        {
            return new RuntimeFieldHandle(ResolveFieldHandleInternal(this.GetRuntimeModule(), fieldToken, typeInstantiationContext, methodInstantiationContext));
        }

        [SecuritySafeCritical]
        internal static unsafe IRuntimeFieldInfo ResolveFieldHandleInternal(RuntimeModule module, int fieldToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
        {
            int num;
            int num2;
            ValidateModulePointer(module);
            if (!GetMetadataImport(module.GetNativeHandle()).IsValidToken(fieldToken))
            {
                throw new ArgumentOutOfRangeException("metadataToken", Environment.GetResourceString("Argument_InvalidToken", new object[] { fieldToken, new ModuleHandle(module) }));
            }
            IntPtr[] ptrArray = RuntimeTypeHandle.CopyRuntimeTypeHandles(typeInstantiationContext, out num);
            IntPtr[] ptrArray2 = RuntimeTypeHandle.CopyRuntimeTypeHandles(methodInstantiationContext, out num2);
            fixed (IntPtr* ptrRef = ptrArray)
            {
                fixed (IntPtr* ptrRef2 = ptrArray2)
                {
                    IRuntimeFieldInfo o = null;
                    ResolveField(module.GetNativeHandle(), fieldToken, ptrRef, num, ptrRef2, num2, JitHelpers.GetObjectHandleOnStack<IRuntimeFieldInfo>(ref o));
                    GC.KeepAlive(typeInstantiationContext);
                    GC.KeepAlive(methodInstantiationContext);
                    return o;
                }
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern unsafe void ResolveField(RuntimeModule module, int fieldToken, IntPtr* typeInstArgs, int typeInstCount, IntPtr* methodInstArgs, int methodInstCount, ObjectHandleOnStack retField);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool _ContainsPropertyMatchingHash(RuntimeModule module, int propertyToken, uint hash);
        [SecurityCritical]
        internal static bool ContainsPropertyMatchingHash(RuntimeModule module, int propertyToken, uint hash)
        {
            return _ContainsPropertyMatchingHash(module.GetNativeHandle(), propertyToken, hash);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetAssembly(RuntimeModule handle, ObjectHandleOnStack retAssembly);
        [SecuritySafeCritical]
        internal static RuntimeAssembly GetAssembly(RuntimeModule module)
        {
            RuntimeAssembly o = null;
            GetAssembly(module.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<RuntimeAssembly>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void GetModuleType(RuntimeModule handle, ObjectHandleOnStack type);
        [SecuritySafeCritical]
        internal static RuntimeType GetModuleType(RuntimeModule module)
        {
            RuntimeType o = null;
            GetModuleType(module.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetPEKind(RuntimeModule handle, out int peKind, out int machine);
        [SecuritySafeCritical]
        internal static void GetPEKind(RuntimeModule module, out PortableExecutableKinds peKind, out ImageFileMachine machine)
        {
            int num;
            int num2;
            GetPEKind(module.GetNativeHandle(), out num, out num2);
            peKind = (PortableExecutableKinds) num;
            machine = (ImageFileMachine) num2;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetMDStreamVersion(RuntimeModule module);
        public int MDStreamVersion
        {
            [SecuritySafeCritical]
            get
            {
                return GetMDStreamVersion(this.GetRuntimeModule().GetNativeHandle());
            }
        }
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern IntPtr _GetMetadataImport(RuntimeModule module);
        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static MetadataImport GetMetadataImport(RuntimeModule module)
        {
            return new MetadataImport(_GetMetadataImport(module.GetNativeHandle()), module);
        }

        static ModuleHandle()
        {
            EmptyHandle = GetEmptyMH();
        }
    }
}


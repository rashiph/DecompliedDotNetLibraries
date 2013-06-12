namespace System
{
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct RuntimeTypeHandle : ISerializable
    {
        private const int MAX_CLASS_NAME = 0x400;
        [ForceTokenStabilization]
        private RuntimeType m_type;
        [SecuritySafeCritical]
        internal RuntimeTypeHandle GetNativeHandle()
        {
            RuntimeType type = this.m_type;
            if (type == null)
            {
                throw new ArgumentNullException(null, Environment.GetResourceString("Arg_InvalidHandle"));
            }
            return new RuntimeTypeHandle(type);
        }

        internal RuntimeType GetTypeChecked()
        {
            RuntimeType type = this.m_type;
            if (type == null)
            {
                throw new ArgumentNullException(null, Environment.GetResourceString("Arg_InvalidHandle"));
            }
            return type;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsInstanceOfType(RuntimeType type, object o);
        [SecuritySafeCritical]
        internal static unsafe Type GetTypeHelper(Type typeStart, Type[] genericArgs, IntPtr pModifiers, int cModifiers)
        {
            Type type = typeStart;
            if (genericArgs != null)
            {
                type = type.MakeGenericType(genericArgs);
            }
            if (cModifiers > 0)
            {
                int* numPtr = (int*) pModifiers.ToPointer();
                for (int i = 0; i < cModifiers; i++)
                {
                    if (((byte) Marshal.ReadInt32((IntPtr) numPtr, i * 4)) == 15)
                    {
                        type = type.MakePointerType();
                    }
                    else if (((byte) Marshal.ReadInt32((IntPtr) numPtr, i * 4)) == 0x10)
                    {
                        type = type.MakeByRefType();
                    }
                    else if (((byte) Marshal.ReadInt32((IntPtr) numPtr, i * 4)) == 0x1d)
                    {
                        type = type.MakeArrayType();
                    }
                    else
                    {
                        type = type.MakeArrayType(Marshal.ReadInt32((IntPtr) numPtr, ++i * 4));
                    }
                }
            }
            return type;
        }

        public static bool operator ==(RuntimeTypeHandle left, object right)
        {
            return left.Equals(right);
        }

        public static bool operator ==(object left, RuntimeTypeHandle right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(RuntimeTypeHandle left, object right)
        {
            return !left.Equals(right);
        }

        public static bool operator !=(object left, RuntimeTypeHandle right)
        {
            return !right.Equals(left);
        }

        internal static RuntimeTypeHandle EmptyHandle
        {
            [SecuritySafeCritical]
            get
            {
                return new RuntimeTypeHandle(null);
            }
        }
        public override int GetHashCode()
        {
            if (this.m_type == null)
            {
                return 0;
            }
            return this.m_type.GetHashCode();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public override bool Equals(object obj)
        {
            if (!(obj is RuntimeTypeHandle))
            {
                return false;
            }
            RuntimeTypeHandle handle = (RuntimeTypeHandle) obj;
            return (handle.m_type == this.m_type);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public bool Equals(RuntimeTypeHandle handle)
        {
            return (handle.m_type == this.m_type);
        }

        public IntPtr Value
        {
            [SecurityCritical]
            get
            {
                if (this.m_type == null)
                {
                    return IntPtr.Zero;
                }
                return this.m_type.m_handle;
            }
        }
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization, SecuritySafeCritical]
        internal static extern IntPtr GetValueInternal(RuntimeTypeHandle handle);
        internal RuntimeTypeHandle(RuntimeType type)
        {
            this.m_type = type;
        }

        [SecuritySafeCritical]
        internal bool IsNullHandle()
        {
            return (this.m_type == null);
        }

        [SecuritySafeCritical]
        internal static bool IsPrimitive(RuntimeType type)
        {
            CorElementType corElementType = GetCorElementType(type);
            if (((corElementType < CorElementType.Boolean) || (corElementType > CorElementType.R8)) && (corElementType != CorElementType.I))
            {
                return (corElementType == CorElementType.U);
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static bool IsByRef(RuntimeType type)
        {
            return (GetCorElementType(type) == CorElementType.ByRef);
        }

        [SecuritySafeCritical]
        internal static bool IsPointer(RuntimeType type)
        {
            return (GetCorElementType(type) == CorElementType.Ptr);
        }

        [SecuritySafeCritical]
        internal static bool IsArray(RuntimeType type)
        {
            CorElementType corElementType = GetCorElementType(type);
            if (corElementType != CorElementType.Array)
            {
                return (corElementType == CorElementType.SzArray);
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static bool IsSzArray(RuntimeType type)
        {
            return (GetCorElementType(type) == CorElementType.SzArray);
        }

        [SecuritySafeCritical]
        internal static bool HasElementType(RuntimeType type)
        {
            CorElementType corElementType = GetCorElementType(type);
            if (((corElementType != CorElementType.Array) && (corElementType != CorElementType.SzArray)) && (corElementType != CorElementType.Ptr))
            {
                return (corElementType == CorElementType.ByRef);
            }
            return true;
        }

        internal static bool IsGenericType(RuntimeType type)
        {
            return (!HasElementType(type) && HasInstantiation(type));
        }

        [SecurityCritical]
        internal static IntPtr[] CopyRuntimeTypeHandles(RuntimeTypeHandle[] inHandles, out int length)
        {
            if ((inHandles == null) || (inHandles.Length == 0))
            {
                length = 0;
                return null;
            }
            IntPtr[] ptrArray = new IntPtr[inHandles.Length];
            for (int i = 0; i < inHandles.Length; i++)
            {
                ptrArray[i] = inHandles[i].Value;
            }
            length = ptrArray.Length;
            return ptrArray;
        }

        [SecurityCritical]
        internal static IntPtr[] CopyRuntimeTypeHandles(Type[] inHandles, out int length)
        {
            if ((inHandles == null) || (inHandles.Length == 0))
            {
                length = 0;
                return null;
            }
            IntPtr[] ptrArray = new IntPtr[inHandles.Length];
            for (int i = 0; i < inHandles.Length; i++)
            {
                ptrArray[i] = inHandles[i].GetTypeHandleInternal().Value;
            }
            length = ptrArray.Length;
            return ptrArray;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object CreateInstance(RuntimeType type, bool publicOnly, bool noCheck, ref bool canBeCached, ref RuntimeMethodHandleInternal ctor, ref bool bNeedSecurityCheck);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object CreateCaInstance(RuntimeType type, IRuntimeMethodInfo ctor);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object Allocate(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object CreateInstanceForAnotherGenericParameter(RuntimeType type, RuntimeType genericParameter);
        internal RuntimeType GetRuntimeType()
        {
            return this.m_type;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern CorElementType GetCorElementType(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern RuntimeAssembly GetAssembly(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        internal static extern RuntimeModule GetModule(RuntimeType type);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), CLSCompliant(false)]
        public ModuleHandle GetModuleHandle()
        {
            return new ModuleHandle(GetModule(this.m_type));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern RuntimeType GetBaseType(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern TypeAttributes GetAttributes(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern RuntimeType GetElementType(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool CompareCanonicalHandles(RuntimeType left, RuntimeType right);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetArrayRank(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetToken(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern RuntimeMethodHandleInternal GetMethodAt(RuntimeType type, int slot);
        internal static IntroducedMethodEnumerator GetIntroducedMethods(RuntimeType type)
        {
            return new IntroducedMethodEnumerator(type);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern RuntimeMethodHandleInternal GetFirstIntroducedMethod(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void GetNextIntroducedMethod(ref RuntimeMethodHandleInternal method);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe bool GetFields(RuntimeType type, IntPtr* result, int* count);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern Type[] GetInterfaces(RuntimeType type);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetConstraints(RuntimeTypeHandle handle, ObjectHandleOnStack types);
        [SecuritySafeCritical]
        internal Type[] GetConstraints()
        {
            Type[] o = null;
            GetConstraints(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<Type[]>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern IntPtr GetGCHandle(RuntimeTypeHandle handle, GCHandleType type);
        [SecurityCritical]
        internal IntPtr GetGCHandle(GCHandleType type)
        {
            return GetGCHandle(this.GetNativeHandle(), type);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetNumVirtuals(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetInterfaceMethodSlots(RuntimeType type);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void VerifyInterfaceIsImplemented(RuntimeTypeHandle handle, RuntimeTypeHandle interfaceHandle);
        [SecuritySafeCritical]
        internal void VerifyInterfaceIsImplemented(RuntimeTypeHandle interfaceHandle)
        {
            VerifyInterfaceIsImplemented(this.GetNativeHandle(), interfaceHandle.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int GetInterfaceMethodImplementationSlot(RuntimeTypeHandle handle, RuntimeTypeHandle interfaceHandle, RuntimeMethodHandleInternal interfaceMethodHandle);
        [SecuritySafeCritical]
        internal int GetInterfaceMethodImplementationSlot(RuntimeTypeHandle interfaceHandle, RuntimeMethodHandleInternal interfaceMethodHandle)
        {
            return GetInterfaceMethodImplementationSlot(this.GetNativeHandle(), interfaceHandle.GetNativeHandle(), interfaceMethodHandle);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsComObject(RuntimeType type, bool isGenericCOM);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsContextful(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsInterface(RuntimeType type);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool _IsVisible(RuntimeTypeHandle typeHandle);
        [SecuritySafeCritical]
        internal static bool IsVisible(RuntimeType type)
        {
            return _IsVisible(new RuntimeTypeHandle(type));
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsSecurityCritical(RuntimeTypeHandle typeHandle);
        [SecuritySafeCritical]
        internal bool IsSecurityCritical()
        {
            return IsSecurityCritical(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsSecuritySafeCritical(RuntimeTypeHandle typeHandle);
        [SecuritySafeCritical]
        internal bool IsSecuritySafeCritical()
        {
            return IsSecuritySafeCritical(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsSecurityTransparent(RuntimeTypeHandle typeHandle);
        [SecuritySafeCritical]
        internal bool IsSecurityTransparent()
        {
            return IsSecurityTransparent(this.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsVisibleFromModule(RuntimeTypeHandle handle, RuntimeModule module);
        [SecuritySafeCritical]
        internal bool IsVisibleFromModule(RuntimeModule module)
        {
            return IsVisibleFromModule(this.GetNativeHandle(), module.GetNativeHandle());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool HasProxyAttribute(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool IsValueType(RuntimeType type);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void ConstructName(RuntimeTypeHandle handle, bool nameSpace, bool fullInst, bool assembly, StringHandleOnStack retString);
        [SecuritySafeCritical]
        internal string ConstructName(bool nameSpace, bool fullInst, bool assembly)
        {
            string s = null;
            ConstructName(this.GetNativeHandle(), nameSpace, fullInst, assembly, JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void* _GetUtf8Name(RuntimeType type);
        [SecuritySafeCritical]
        internal static Utf8String GetUtf8Name(RuntimeType type)
        {
            return new Utf8String(_GetUtf8Name(type));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool CanCastTo(RuntimeType type, RuntimeType target);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeType GetDeclaringType(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern IRuntimeMethodInfo GetDeclaringMethod(RuntimeType type);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetDefaultConstructor(RuntimeTypeHandle handle, ObjectHandleOnStack method);
        [SecuritySafeCritical]
        internal IRuntimeMethodInfo GetDefaultConstructor()
        {
            IRuntimeMethodInfo o = null;
            GetDefaultConstructor(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<IRuntimeMethodInfo>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetTypeByName(string name, bool throwOnError, bool ignoreCase, bool reflectionOnly, StackCrawlMarkHandle stackMark, bool loadTypeFromPartialName, ObjectHandleOnStack type);
        [SecuritySafeCritical]
        internal static RuntimeType GetTypeByName(string name, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark, bool loadTypeFromPartialName)
        {
            if ((name == null) || (name.Length == 0))
            {
                if (throwOnError)
                {
                    throw new TypeLoadException(Environment.GetResourceString("Arg_TypeLoadNullStr"));
                }
                return null;
            }
            RuntimeType o = null;
            GetTypeByName(name, throwOnError, ignoreCase, reflectionOnly, JitHelpers.GetStackCrawlMarkHandle(ref stackMark), loadTypeFromPartialName, JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        internal static Type GetTypeByName(string name, ref StackCrawlMark stackMark)
        {
            return GetTypeByName(name, false, false, false, ref stackMark, false);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetTypeByNameUsingCARules(string name, RuntimeModule scope, ObjectHandleOnStack type);
        [SecuritySafeCritical]
        internal static RuntimeType GetTypeByNameUsingCARules(string name, RuntimeModule scope)
        {
            if ((name == null) || (name.Length == 0))
            {
                throw new ArgumentException("name");
            }
            RuntimeType o = null;
            GetTypeByNameUsingCARules(name, scope.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void GetInstantiation(RuntimeTypeHandle type, ObjectHandleOnStack types, bool fAsRuntimeTypeArray);
        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal RuntimeType[] GetInstantiationInternal()
        {
            RuntimeType[] o = null;
            GetInstantiation(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<RuntimeType[]>(ref o), true);
            return o;
        }

        [SecuritySafeCritical]
        internal Type[] GetInstantiationPublic()
        {
            Type[] o = null;
            GetInstantiation(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<Type[]>(ref o), false);
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern unsafe void Instantiate(RuntimeTypeHandle handle, IntPtr* pInst, int numGenericArgs, ObjectHandleOnStack type);
        [SecurityCritical]
        internal unsafe RuntimeType Instantiate(Type[] inst)
        {
            int num;
            fixed (IntPtr* ptrRef = CopyRuntimeTypeHandles(inst, out num))
            {
                RuntimeType o = null;
                Instantiate(this.GetNativeHandle(), ptrRef, num, JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
                GC.KeepAlive(inst);
                return o;
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void MakeArray(RuntimeTypeHandle handle, int rank, ObjectHandleOnStack type);
        [SecuritySafeCritical]
        internal RuntimeType MakeArray(int rank)
        {
            RuntimeType o = null;
            MakeArray(this.GetNativeHandle(), rank, JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void MakeSZArray(RuntimeTypeHandle handle, ObjectHandleOnStack type);
        [SecuritySafeCritical]
        internal RuntimeType MakeSZArray()
        {
            RuntimeType o = null;
            MakeSZArray(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void MakeByRef(RuntimeTypeHandle handle, ObjectHandleOnStack type);
        [SecuritySafeCritical]
        internal RuntimeType MakeByRef()
        {
            RuntimeType o = null;
            MakeByRef(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void MakePointer(RuntimeTypeHandle handle, ObjectHandleOnStack type);
        [SecurityCritical]
        internal RuntimeType MakePointer()
        {
            RuntimeType o = null;
            MakePointer(this.GetNativeHandle(), JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsCollectible(RuntimeTypeHandle handle);
        [SecuritySafeCritical]
        internal bool IsCollectible()
        {
            return IsCollectible(this.GetNativeHandle());
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool HasInstantiation(RuntimeType type);
        [SecuritySafeCritical]
        internal bool HasInstantiation()
        {
            return HasInstantiation(this.GetTypeChecked());
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetGenericTypeDefinition(RuntimeTypeHandle type, ObjectHandleOnStack retType);
        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static RuntimeType GetGenericTypeDefinition(RuntimeType type)
        {
            RuntimeType type2 = type;
            if (HasInstantiation(type2))
            {
                RuntimeTypeHandle typeHandleInternal = type2.GetTypeHandleInternal();
                GetGenericTypeDefinition(typeHandleInternal, JitHelpers.GetObjectHandleOnStack<RuntimeType>(ref type2));
            }
            return type2;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool IsGenericTypeDefinition(RuntimeType type);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool IsGenericVariable(RuntimeType type);
        [SecuritySafeCritical]
        internal bool IsGenericVariable()
        {
            return IsGenericVariable(this.GetTypeChecked());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int GetGenericVariableIndex(RuntimeType type);
        [SecuritySafeCritical]
        internal int GetGenericVariableIndex()
        {
            RuntimeType typeChecked = this.GetTypeChecked();
            if (!IsGenericVariable(typeChecked))
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
            }
            return GetGenericVariableIndex(typeChecked);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool ContainsGenericVariables(RuntimeType handle);
        [SecuritySafeCritical]
        internal bool ContainsGenericVariables()
        {
            return ContainsGenericVariables(this.GetTypeChecked());
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe bool SatisfiesConstraints(RuntimeType paramType, IntPtr* pTypeContext, int typeContextLength, IntPtr* pMethodContext, int methodContextLength, RuntimeType toType);
        [SecurityCritical]
        internal static unsafe bool SatisfiesConstraints(RuntimeType paramType, RuntimeType[] typeContext, RuntimeType[] methodContext, RuntimeType toType)
        {
            int num;
            int num2;
            IntPtr[] ptrArray = CopyRuntimeTypeHandles(typeContext, out num);
            IntPtr[] ptrArray2 = CopyRuntimeTypeHandles(methodContext, out num2);
            fixed (IntPtr* ptrRef = ptrArray)
            {
                fixed (IntPtr* ptrRef2 = ptrArray2)
                {
                    bool flag = SatisfiesConstraints(paramType, ptrRef, num, ptrRef2, num2, toType);
                    GC.KeepAlive(typeContext);
                    GC.KeepAlive(methodContext);
                    return flag;
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern IntPtr _GetMetadataImport(RuntimeType type);
        [SecurityCritical]
        internal static MetadataImport GetMetadataImport(RuntimeType type)
        {
            return new MetadataImport(_GetMetadataImport(type), type);
        }

        [SecurityCritical]
        private RuntimeTypeHandle(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            RuntimeType type = (RuntimeType) info.GetValue("TypeObj", typeof(RuntimeType));
            this.m_type = type;
            if (this.m_type == null)
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
            if (this.m_type == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFieldState"));
            }
            info.AddValue("TypeObj", this.m_type, typeof(RuntimeType));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool IsEquivalentTo(RuntimeType rtType1, RuntimeType rtType2);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool IsEquivalentType(RuntimeType type);
        [StructLayout(LayoutKind.Sequential)]
        internal struct IntroducedMethodEnumerator
        {
            private bool _firstCall;
            private RuntimeMethodHandleInternal _handle;
            [SecuritySafeCritical]
            internal IntroducedMethodEnumerator(RuntimeType type)
            {
                this._handle = RuntimeTypeHandle.GetFirstIntroducedMethod(type);
                this._firstCall = true;
            }

            [SecuritySafeCritical]
            public bool MoveNext()
            {
                if (this._firstCall)
                {
                    this._firstCall = false;
                }
                else if (this._handle.Value != IntPtr.Zero)
                {
                    RuntimeTypeHandle.GetNextIntroducedMethod(ref this._handle);
                }
                return !(this._handle.Value == IntPtr.Zero);
            }

            public RuntimeMethodHandleInternal Current
            {
                get
                {
                    return this._handle;
                }
            }
            public RuntimeTypeHandle.IntroducedMethodEnumerator GetEnumerator()
            {
                return this;
            }
        }
    }
}


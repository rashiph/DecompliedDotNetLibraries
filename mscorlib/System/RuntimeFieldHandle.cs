namespace System
{
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct RuntimeFieldHandle : ISerializable
    {
        [ForceTokenStabilization]
        private IRuntimeFieldInfo m_ptr;
        internal RuntimeFieldHandle GetNativeHandle()
        {
            IRuntimeFieldInfo ptr = this.m_ptr;
            if (ptr == null)
            {
                throw new ArgumentNullException(null, Environment.GetResourceString("Arg_InvalidHandle"));
            }
            return new RuntimeFieldHandle(ptr);
        }

        internal RuntimeFieldHandle(IRuntimeFieldInfo fieldInfo)
        {
            this.m_ptr = fieldInfo;
        }

        internal IRuntimeFieldInfo GetRuntimeFieldInfo()
        {
            return this.m_ptr;
        }

        public IntPtr Value
        {
            [SecurityCritical]
            get
            {
                if (this.m_ptr == null)
                {
                    return IntPtr.Zero;
                }
                return this.m_ptr.Value.Value;
            }
        }
        [SecuritySafeCritical]
        internal bool IsNullHandle()
        {
            return (this.m_ptr == null);
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            return ValueType.GetHashCodeOfPtr(this.Value);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            if (!(obj is RuntimeFieldHandle))
            {
                return false;
            }
            RuntimeFieldHandle handle = (RuntimeFieldHandle) obj;
            return (handle.Value == this.Value);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public bool Equals(RuntimeFieldHandle handle)
        {
            return (handle.Value == this.Value);
        }

        public static bool operator ==(RuntimeFieldHandle left, RuntimeFieldHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeFieldHandle left, RuntimeFieldHandle right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string GetName(RtFieldInfo field);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void* _GetUtf8Name(RuntimeFieldHandleInternal field);
        [SecuritySafeCritical]
        internal static Utf8String GetUtf8Name(RuntimeFieldHandleInternal field)
        {
            return new Utf8String(_GetUtf8Name(field));
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool MatchesNameHash(IntPtr handle, uint hash);
        [SecuritySafeCritical]
        internal static bool MatchesNameHash(RuntimeFieldHandleInternal field, uint hash)
        {
            return MatchesNameHash(field.Value, hash);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern FieldAttributes GetAttributes(RuntimeFieldHandleInternal field);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeType GetApproxDeclaringType(RuntimeFieldHandleInternal field);
        [SecurityCritical]
        internal static RuntimeType GetApproxDeclaringType(IRuntimeFieldInfo field)
        {
            RuntimeType approxDeclaringType = GetApproxDeclaringType(field.Value);
            GC.KeepAlive(field);
            return approxDeclaringType;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetToken(RtFieldInfo field);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern object GetValue(RtFieldInfo field, object instance, RuntimeType fieldType, RuntimeType declaringType, ref bool domainInitialized);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe object GetValueDirect(RtFieldInfo field, RuntimeType fieldType, void* pTypedRef, RuntimeType contextType);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void SetValue(RtFieldInfo field, object obj, object value, RuntimeType fieldType, FieldAttributes fieldAttr, RuntimeType declaringType, ref bool domainInitialized);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe void SetValueDirect(RtFieldInfo field, RuntimeType fieldType, void* pTypedRef, object value, RuntimeType contextType);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern RuntimeFieldHandleInternal GetStaticFieldForGenericType(RuntimeFieldHandleInternal field, RuntimeType declaringType);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool AcquiresContextFromThis(RuntimeFieldHandleInternal field);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsSecurityCritical(RuntimeFieldHandle fieldHandle);
        [SecuritySafeCritical]
        internal bool IsSecurityCritical()
        {
            return IsSecurityCritical(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsSecuritySafeCritical(RuntimeFieldHandle fieldHandle);
        [SecuritySafeCritical]
        internal bool IsSecuritySafeCritical()
        {
            return IsSecuritySafeCritical(this.GetNativeHandle());
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool IsSecurityTransparent(RuntimeFieldHandle fieldHandle);
        [SecuritySafeCritical]
        internal bool IsSecurityTransparent()
        {
            return IsSecurityTransparent(this.GetNativeHandle());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void CheckAttributeAccess(RuntimeFieldHandle fieldHandle, RuntimeModule decoratedTarget);
        [SecurityCritical]
        private RuntimeFieldHandle(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            FieldInfo info2 = (RuntimeFieldInfo) info.GetValue("FieldObj", typeof(RuntimeFieldInfo));
            if (info2 == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
            }
            this.m_ptr = info2.FieldHandle.m_ptr;
            if (this.m_ptr == null)
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
            if (this.m_ptr == null)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFieldState"));
            }
            RuntimeFieldInfo fieldInfo = (RuntimeFieldInfo) RuntimeType.GetFieldInfo(this.GetRuntimeFieldInfo());
            info.AddValue("FieldObj", fieldInfo, typeof(RuntimeFieldInfo));
        }
    }
}


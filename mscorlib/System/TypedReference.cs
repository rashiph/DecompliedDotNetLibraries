namespace System
{
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential), ComVisible(true), CLSCompliant(false)]
    public struct TypedReference
    {
        private IntPtr Value;
        private IntPtr Type;
        [CLSCompliant(false), SecurityCritical]
        public static unsafe TypedReference MakeTypedReference(object target, FieldInfo[] flds)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (flds == null)
            {
                throw new ArgumentNullException("flds");
            }
            if (flds.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ArrayZeroError"));
            }
            IntPtr[] ptrArray = new IntPtr[flds.Length];
            System.Type type = target.GetType();
            for (int i = 0; i < flds.Length; i++)
            {
                FieldInfo info = flds[i];
                if (!(info is RuntimeFieldInfo))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeFieldInfo"));
                }
                if (info.IsInitOnly || info.IsStatic)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_TypedReferenceInvalidField"));
                }
                if ((type != info.DeclaringType) && !type.IsSubclassOf(info.DeclaringType))
                {
                    throw new MissingMemberException(Environment.GetResourceString("MissingMemberTypeRef"));
                }
                System.Type fieldType = info.FieldType;
                if (fieldType.IsPrimitive)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_TypeRefPrimitve"));
                }
                if ((i < (flds.Length - 1)) && !fieldType.IsValueType)
                {
                    throw new MissingMemberException(Environment.GetResourceString("MissingMemberNestErr"));
                }
                ptrArray[i] = info.FieldHandle.Value;
                type = fieldType;
            }
            TypedReference reference = new TypedReference();
            InternalMakeTypedReference((void*) &reference, target, ptrArray, type.TypeHandle.GetRuntimeType());
            return reference;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void InternalMakeTypedReference(void* result, object target, IntPtr[] flds, RuntimeType lastFieldType);
        public override unsafe int GetHashCode()
        {
            if (this.Type == IntPtr.Zero)
            {
                return 0;
            }
            return System.Type.GetTypeFromHandle(*((RuntimeTypeHandle*) this)).GetHashCode();
        }

        public override bool Equals(object o)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NYI"));
        }

        [SecuritySafeCritical]
        public static unsafe object ToObject(TypedReference value)
        {
            return InternalToObject((void*) &value);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe object InternalToObject(void* value);
        internal bool IsNull
        {
            get
            {
                return (this.Value.IsNull() && this.Type.IsNull());
            }
        }
        public static System.Type GetTargetType(TypedReference value)
        {
            return System.Type.GetTypeFromHandle((RuntimeTypeHandle) value);
        }

        public static RuntimeTypeHandle TargetTypeToken(TypedReference value)
        {
            return System.Type.GetTypeFromHandle((RuntimeTypeHandle) value).TypeHandle;
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public static unsafe void SetTypedReference(TypedReference target, object value)
        {
            InternalSetTypedReference((void*) &target, value);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe void InternalSetTypedReference(void* target, object value);
        [SecurityCritical]
        internal IntPtr GetPointerOnStack()
        {
            return this.Value;
        }
    }
}


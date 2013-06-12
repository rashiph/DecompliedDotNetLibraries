namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true), CLSCompliant(false)]
    public sealed class Pointer : ISerializable
    {
        private unsafe void* _ptr;
        private RuntimeType _ptrType;

        private Pointer()
        {
        }

        [SecurityCritical]
        private unsafe Pointer(SerializationInfo info, StreamingContext context)
        {
            this._ptr = ((IntPtr) info.GetValue("_ptr", typeof(IntPtr))).ToPointer();
            this._ptrType = (RuntimeType) info.GetValue("_ptrType", typeof(RuntimeType));
        }

        [SecurityCritical]
        public static unsafe object Box(void* ptr, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsPointer)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePointer"), "ptr");
            }
            RuntimeType type2 = type as RuntimeType;
            if (type2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePointer"), "ptr");
            }
            return new Pointer { _ptr = ptr, _ptrType = type2 };
        }

        internal RuntimeType GetPointerType()
        {
            return this._ptrType;
        }

        [SecurityCritical]
        internal unsafe object GetPointerValue()
        {
            return (IntPtr) this._ptr;
        }

        [SecurityCritical]
        unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_ptr", new IntPtr(this._ptr));
            info.AddValue("_ptrType", this._ptrType);
        }

        [SecurityCritical]
        public static unsafe void* Unbox(object ptr)
        {
            if (!(ptr is Pointer))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePointer"), "ptr");
            }
            return ((Pointer) ptr)._ptr;
        }
    }
}


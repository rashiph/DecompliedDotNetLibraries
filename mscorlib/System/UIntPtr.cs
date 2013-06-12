namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), CLSCompliant(false), ComVisible(true)]
    public struct UIntPtr : ISerializable
    {
        private unsafe void* m_value;
        public static readonly UIntPtr Zero;
        [SecuritySafeCritical]
        public unsafe UIntPtr(uint value)
        {
            this.m_value = (void*) value;
        }

        [SecuritySafeCritical]
        public unsafe UIntPtr(ulong value)
        {
            this.m_value = (void*) ((uint) value);
        }

        [SecurityCritical, CLSCompliant(false)]
        public unsafe UIntPtr(void* value)
        {
            this.m_value = value;
        }

        [SecurityCritical]
        private unsafe UIntPtr(SerializationInfo info, StreamingContext context)
        {
            ulong num = info.GetUInt64("value");
            if ((Size == 4) && (num > 0xffffffffL))
            {
                throw new ArgumentException(Environment.GetResourceString("Serialization_InvalidPtrValue"));
            }
            this.m_value = (void*) num;
        }

        [SecurityCritical]
        unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("value", (ulong) this.m_value);
        }

        [SecuritySafeCritical]
        public override unsafe bool Equals(object obj)
        {
            if (obj is UIntPtr)
            {
                UIntPtr ptr = (UIntPtr) obj;
                return (this.m_value == ptr.m_value);
            }
            return false;
        }

        [SecuritySafeCritical]
        public override unsafe int GetHashCode()
        {
            return (((int) ((ulong) this.m_value)) & 0x7fffffff);
        }

        [SecuritySafeCritical]
        public unsafe uint ToUInt32()
        {
            return (uint) this.m_value;
        }

        [SecuritySafeCritical]
        public unsafe ulong ToUInt64()
        {
            return (ulong) this.m_value;
        }

        [SecuritySafeCritical]
        public override unsafe string ToString()
        {
            uint num = (uint) this.m_value;
            return num.ToString(CultureInfo.InvariantCulture);
        }

        public static explicit operator UIntPtr(uint value)
        {
            return new UIntPtr(value);
        }

        public static explicit operator UIntPtr(ulong value)
        {
            return new UIntPtr(value);
        }

        [SecuritySafeCritical]
        public static unsafe explicit operator uint(UIntPtr value)
        {
            return (uint) value.m_value;
        }

        [SecuritySafeCritical]
        public static unsafe explicit operator ulong(UIntPtr value)
        {
            return (ulong) value.m_value;
        }

        [SecurityCritical, CLSCompliant(false)]
        public static unsafe explicit operator UIntPtr(void* value)
        {
            return new UIntPtr(value);
        }

        [SecurityCritical, CLSCompliant(false)]
        public static unsafe explicit operator void*(UIntPtr value)
        {
            return value.ToPointer();
        }

        [SecuritySafeCritical]
        public static unsafe bool operator ==(UIntPtr value1, UIntPtr value2)
        {
            return (value1.m_value == value2.m_value);
        }

        [SecuritySafeCritical]
        public static unsafe bool operator !=(UIntPtr value1, UIntPtr value2)
        {
            return (value1.m_value != value2.m_value);
        }

        [SecuritySafeCritical]
        public static UIntPtr Add(UIntPtr pointer, int offset)
        {
            return (pointer + offset);
        }

        [SecuritySafeCritical]
        public static UIntPtr operator +(UIntPtr pointer, int offset)
        {
            return new UIntPtr(pointer.ToUInt32() + ((uint) offset));
        }

        [SecuritySafeCritical]
        public static UIntPtr Subtract(UIntPtr pointer, int offset)
        {
            return (pointer - offset);
        }

        [SecuritySafeCritical]
        public static UIntPtr operator -(UIntPtr pointer, int offset)
        {
            return new UIntPtr(pointer.ToUInt32() - ((uint) offset));
        }

        public static int Size
        {
            get
            {
                return 4;
            }
        }
        [SecuritySafeCritical, CLSCompliant(false)]
        public unsafe void* ToPointer()
        {
            return this.m_value;
        }
    }
}


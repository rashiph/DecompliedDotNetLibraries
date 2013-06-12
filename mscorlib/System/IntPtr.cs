namespace System
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct IntPtr : ISerializable
    {
        private unsafe void* m_value;
        public static readonly IntPtr Zero;
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        internal unsafe bool IsNull()
        {
            return (this.m_value == null);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public unsafe IntPtr(int value)
        {
            this.m_value = (void*) value;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), SecuritySafeCritical]
        public unsafe IntPtr(long value)
        {
            this.m_value = (void*) ((int) value);
        }

        [SecurityCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), CLSCompliant(false)]
        public unsafe IntPtr(void* value)
        {
            this.m_value = value;
        }

        [SecurityCritical]
        private unsafe IntPtr(SerializationInfo info, StreamingContext context)
        {
            long num = info.GetInt64("value");
            if ((Size == 4) && ((num > 0x7fffffffL) || (num < -2147483648L)))
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
            info.AddValue("value", (long) ((int) this.m_value));
        }

        [SecuritySafeCritical]
        public override unsafe bool Equals(object obj)
        {
            if (obj is IntPtr)
            {
                IntPtr ptr = (IntPtr) obj;
                return (this.m_value == ptr.m_value);
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public override unsafe int GetHashCode()
        {
            return (int) ((ulong) this.m_value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public unsafe int ToInt32()
        {
            return (int) this.m_value;
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public unsafe long ToInt64()
        {
            return (long) ((int) this.m_value);
        }

        [SecuritySafeCritical]
        public override unsafe string ToString()
        {
            int num = (int) this.m_value;
            return num.ToString(CultureInfo.InvariantCulture);
        }

        [SecuritySafeCritical]
        public unsafe string ToString(string format)
        {
            int num = (int) this.m_value;
            return num.ToString(format, CultureInfo.InvariantCulture);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static explicit operator IntPtr(int value)
        {
            return new IntPtr(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static explicit operator IntPtr(long value)
        {
            return new IntPtr(value);
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), CLSCompliant(false), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static unsafe explicit operator IntPtr(void* value)
        {
            return new IntPtr(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, CLSCompliant(false)]
        public static unsafe explicit operator void*(IntPtr value)
        {
            return value.ToPointer();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public static unsafe explicit operator int(IntPtr value)
        {
            return (int) value.m_value;
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static unsafe explicit operator long(IntPtr value)
        {
            return (long) ((int) value.m_value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public static unsafe bool operator ==(IntPtr value1, IntPtr value2)
        {
            return (value1.m_value == value2.m_value);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static unsafe bool operator !=(IntPtr value1, IntPtr value2)
        {
            return (value1.m_value != value2.m_value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical, ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static IntPtr Add(IntPtr pointer, int offset)
        {
            return (pointer + offset);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static IntPtr operator +(IntPtr pointer, int offset)
        {
            return new IntPtr(pointer.ToInt32() + offset);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public static IntPtr Subtract(IntPtr pointer, int offset)
        {
            return (pointer - offset);
        }

        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public static IntPtr operator -(IntPtr pointer, int offset)
        {
            return new IntPtr(pointer.ToInt32() - offset);
        }

        public static int Size
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return 4;
            }
        }
        [SecuritySafeCritical, CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public unsafe void* ToPointer()
        {
            return this.m_value;
        }
    }
}


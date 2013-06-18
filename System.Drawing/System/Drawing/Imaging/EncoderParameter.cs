namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class EncoderParameter : IDisposable
    {
        [MarshalAs(UnmanagedType.Struct)]
        private Guid parameterGuid;
        private int numberOfValues;
        private int parameterValueType;
        private IntPtr parameterValue;
        ~EncoderParameter()
        {
            this.Dispose(false);
        }

        public System.Drawing.Imaging.Encoder Encoder
        {
            get
            {
                return new System.Drawing.Imaging.Encoder(this.parameterGuid);
            }
            set
            {
                this.parameterGuid = value.Guid;
            }
        }
        public EncoderParameterValueType Type
        {
            get
            {
                return (EncoderParameterValueType) this.parameterValueType;
            }
        }
        public EncoderParameterValueType ValueType
        {
            get
            {
                return (EncoderParameterValueType) this.parameterValueType;
            }
        }
        public int NumberOfValues
        {
            get
            {
                return this.numberOfValues;
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.KeepAlive(this);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.parameterValue != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.parameterValue);
            }
            this.parameterValue = IntPtr.Zero;
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte value)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 1;
            this.numberOfValues = 1;
            this.parameterValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteByte(this.parameterValue, value);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte value, bool undefined)
        {
            this.parameterGuid = encoder.Guid;
            if (undefined)
            {
                this.parameterValueType = 7;
            }
            else
            {
                this.parameterValueType = 1;
            }
            this.numberOfValues = 1;
            this.parameterValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteByte(this.parameterValue, value);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, short value)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 3;
            this.numberOfValues = 1;
            this.parameterValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(short)));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteInt16(this.parameterValue, value);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long value)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 4;
            this.numberOfValues = 1;
            this.parameterValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteInt32(this.parameterValue, (int) value);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numerator, int demoninator)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 5;
            this.numberOfValues = 1;
            int b = Marshal.SizeOf(typeof(int));
            this.parameterValue = Marshal.AllocHGlobal((int) (2 * b));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteInt32(this.parameterValue, numerator);
            Marshal.WriteInt32(Add(this.parameterValue, b), demoninator);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long rangebegin, long rangeend)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 6;
            this.numberOfValues = 1;
            int b = Marshal.SizeOf(typeof(int));
            this.parameterValue = Marshal.AllocHGlobal((int) (2 * b));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteInt32(this.parameterValue, (int) rangebegin);
            Marshal.WriteInt32(Add(this.parameterValue, b), (int) rangeend);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int numerator1, int demoninator1, int numerator2, int demoninator2)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 8;
            this.numberOfValues = 1;
            int b = Marshal.SizeOf(typeof(int));
            this.parameterValue = Marshal.AllocHGlobal((int) (4 * b));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.WriteInt32(this.parameterValue, numerator1);
            Marshal.WriteInt32(Add(this.parameterValue, b), demoninator1);
            Marshal.WriteInt32(Add(this.parameterValue, 2 * b), numerator2);
            Marshal.WriteInt32(Add(this.parameterValue, 3 * b), demoninator2);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, string value)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 2;
            this.numberOfValues = value.Length;
            this.parameterValue = Marshal.StringToHGlobalAnsi(value);
            GC.KeepAlive(this);
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte[] value)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 1;
            this.numberOfValues = value.Length;
            this.parameterValue = Marshal.AllocHGlobal(this.numberOfValues);
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.Copy(value, 0, this.parameterValue, this.numberOfValues);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, byte[] value, bool undefined)
        {
            this.parameterGuid = encoder.Guid;
            if (undefined)
            {
                this.parameterValueType = 7;
            }
            else
            {
                this.parameterValueType = 1;
            }
            this.numberOfValues = value.Length;
            this.parameterValue = Marshal.AllocHGlobal(this.numberOfValues);
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.Copy(value, 0, this.parameterValue, this.numberOfValues);
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, short[] value)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 3;
            this.numberOfValues = value.Length;
            int num = Marshal.SizeOf(typeof(short));
            this.parameterValue = Marshal.AllocHGlobal((int) (this.numberOfValues * num));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            Marshal.Copy(value, 0, this.parameterValue, this.numberOfValues);
            GC.KeepAlive(this);
        }

        public unsafe EncoderParameter(System.Drawing.Imaging.Encoder encoder, long[] value)
        {
            this.parameterGuid = encoder.Guid;
            this.parameterValueType = 4;
            this.numberOfValues = value.Length;
            int num = Marshal.SizeOf(typeof(int));
            this.parameterValue = Marshal.AllocHGlobal((int) (this.numberOfValues * num));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            int* parameterValue = (int*) this.parameterValue;
            fixed (long* numRef = value)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    parameterValue[i] = (int) numRef[i];
                }
            }
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int[] numerator, int[] denominator)
        {
            this.parameterGuid = encoder.Guid;
            if (numerator.Length != denominator.Length)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            this.parameterValueType = 5;
            this.numberOfValues = numerator.Length;
            int num = Marshal.SizeOf(typeof(int));
            this.parameterValue = Marshal.AllocHGlobal((int) ((this.numberOfValues * 2) * num));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            for (int i = 0; i < this.numberOfValues; i++)
            {
                Marshal.WriteInt32(Add((i * 2) * num, this.parameterValue), numerator[i]);
                Marshal.WriteInt32(Add(((i * 2) + 1) * num, this.parameterValue), denominator[i]);
            }
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, long[] rangebegin, long[] rangeend)
        {
            this.parameterGuid = encoder.Guid;
            if (rangebegin.Length != rangeend.Length)
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            this.parameterValueType = 6;
            this.numberOfValues = rangebegin.Length;
            int num = Marshal.SizeOf(typeof(int));
            this.parameterValue = Marshal.AllocHGlobal((int) ((this.numberOfValues * 2) * num));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            for (int i = 0; i < this.numberOfValues; i++)
            {
                Marshal.WriteInt32(Add((i * 2) * num, this.parameterValue), (int) rangebegin[i]);
                Marshal.WriteInt32(Add(((i * 2) + 1) * num, this.parameterValue), (int) rangeend[i]);
            }
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int[] numerator1, int[] denominator1, int[] numerator2, int[] denominator2)
        {
            this.parameterGuid = encoder.Guid;
            if (((numerator1.Length != denominator1.Length) || (numerator1.Length != denominator2.Length)) || (denominator1.Length != denominator2.Length))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            this.parameterValueType = 8;
            this.numberOfValues = numerator1.Length;
            int num = Marshal.SizeOf(typeof(int));
            this.parameterValue = Marshal.AllocHGlobal((int) ((this.numberOfValues * 4) * num));
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            for (int i = 0; i < this.numberOfValues; i++)
            {
                Marshal.WriteInt32(Add(this.parameterValue, (4 * i) * num), numerator1[i]);
                Marshal.WriteInt32(Add(this.parameterValue, ((4 * i) + 1) * num), denominator1[i]);
                Marshal.WriteInt32(Add(this.parameterValue, ((4 * i) + 2) * num), numerator2[i]);
                Marshal.WriteInt32(Add(this.parameterValue, ((4 * i) + 3) * num), denominator2[i]);
            }
            GC.KeepAlive(this);
        }

        public EncoderParameter(System.Drawing.Imaging.Encoder encoder, int NumberOfValues, int Type, int Value)
        {
            int num;
            IntSecurity.UnmanagedCode.Demand();
            switch (((EncoderParameterValueType) Type))
            {
                case EncoderParameterValueType.ValueTypeByte:
                case EncoderParameterValueType.ValueTypeAscii:
                    num = 1;
                    break;

                case EncoderParameterValueType.ValueTypeShort:
                    num = 2;
                    break;

                case EncoderParameterValueType.ValueTypeLong:
                    num = 4;
                    break;

                case EncoderParameterValueType.ValueTypeRational:
                case EncoderParameterValueType.ValueTypeLongRange:
                    num = 8;
                    break;

                case EncoderParameterValueType.ValueTypeUndefined:
                    num = 1;
                    break;

                case EncoderParameterValueType.ValueTypeRationalRange:
                    num = 0x10;
                    break;

                default:
                    throw SafeNativeMethods.Gdip.StatusException(8);
            }
            int cb = num * NumberOfValues;
            this.parameterValue = Marshal.AllocHGlobal(cb);
            if (this.parameterValue == IntPtr.Zero)
            {
                throw SafeNativeMethods.Gdip.StatusException(3);
            }
            for (int i = 0; i < cb; i++)
            {
                Marshal.WriteByte(Add(this.parameterValue, i), Marshal.ReadByte((IntPtr) (Value + i)));
            }
            this.parameterValueType = Type;
            this.numberOfValues = NumberOfValues;
            this.parameterGuid = encoder.Guid;
            GC.KeepAlive(this);
        }

        private static IntPtr Add(IntPtr a, int b)
        {
            return (IntPtr) (((long) a) + b);
        }

        private static IntPtr Add(int a, IntPtr b)
        {
            return (IntPtr) (a + ((long) b));
        }
    }
}


namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal abstract class NativeMsmqMessage : IDisposable
    {
        private object[] buffersForAsync;
        private bool disposed;
        private int[] ids;
        private GCHandle idsHandle;
        private UnsafeNativeMethods.MQMSGPROPS nativeProperties;
        private GCHandle nativePropertiesHandle;
        private MsmqProperty[] properties;
        private UnsafeNativeMethods.MQPROPVARIANT[] variants;
        private GCHandle variantsHandle;

        protected NativeMsmqMessage(int propertyCount)
        {
            this.properties = new MsmqProperty[propertyCount];
            this.nativeProperties = new UnsafeNativeMethods.MQMSGPROPS();
            this.ids = new int[propertyCount];
            this.variants = new UnsafeNativeMethods.MQPROPVARIANT[propertyCount];
            this.nativePropertiesHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            this.idsHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            this.variantsHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                for (int i = 0; i < this.nativeProperties.count; i++)
                {
                    this.properties[i].Dispose();
                }
                this.disposed = true;
            }
            if (this.nativePropertiesHandle.IsAllocated)
            {
                this.nativePropertiesHandle.Free();
            }
            if (this.idsHandle.IsAllocated)
            {
                this.idsHandle.Free();
            }
            if (this.variantsHandle.IsAllocated)
            {
                this.variantsHandle.Free();
            }
        }

        ~NativeMsmqMessage()
        {
            this.Dispose(false);
        }

        public object[] GetBuffersForAsync()
        {
            if (this.buffersForAsync == null)
            {
                int num = 0;
                for (int j = 0; j < this.nativeProperties.count; j++)
                {
                    if (this.properties[j].MaintainsBuffer)
                    {
                        num++;
                    }
                }
                this.buffersForAsync = new object[num + 3];
            }
            int index = 0;
            for (int i = 0; i < this.nativeProperties.count; i++)
            {
                if (this.properties[i].MaintainsBuffer)
                {
                    this.buffersForAsync[index++] = this.properties[i].MaintainedBuffer;
                }
            }
            this.buffersForAsync[index++] = this.ids;
            this.buffersForAsync[index++] = this.variants;
            this.buffersForAsync[index] = this.nativeProperties;
            return this.buffersForAsync;
        }

        public virtual void GrowBuffers()
        {
        }

        public IntPtr Pin()
        {
            for (int i = 0; i < this.nativeProperties.count; i++)
            {
                this.properties[i].Pin();
            }
            this.idsHandle.Target = this.ids;
            this.variantsHandle.Target = this.variants;
            this.nativeProperties.status = IntPtr.Zero;
            this.nativeProperties.variants = this.variantsHandle.AddrOfPinnedObject();
            this.nativeProperties.ids = this.idsHandle.AddrOfPinnedObject();
            this.nativePropertiesHandle.Target = this.nativeProperties;
            return this.nativePropertiesHandle.AddrOfPinnedObject();
        }

        public void Unpin()
        {
            this.nativePropertiesHandle.Target = null;
            this.idsHandle.Target = null;
            this.variantsHandle.Target = null;
            for (int i = 0; i < this.nativeProperties.count; i++)
            {
                this.properties[i].Unpin();
            }
        }

        public class BooleanProperty : NativeMsmqMessage.MsmqProperty
        {
            public BooleanProperty(NativeMsmqMessage message, int id) : base(message, id, 11)
            {
            }

            public BooleanProperty(NativeMsmqMessage message, int id, bool value) : this(message, id)
            {
                this.Value = value;
            }

            public bool Value
            {
                get
                {
                    return (base.Variants[base.Index].shortValue != 0);
                }
                set
                {
                    base.Variants[base.Index].shortValue = value ? ((short) (-1)) : ((short) 0);
                }
            }
        }

        public class BufferProperty : NativeMsmqMessage.MsmqProperty
        {
            private byte[] buffer;
            private GCHandle bufferHandle;

            public BufferProperty(NativeMsmqMessage message, int id) : base(message, id, 0x1011)
            {
                this.bufferHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            }

            public BufferProperty(NativeMsmqMessage message, int id, byte[] buffer) : this(message, id, buffer.Length)
            {
                System.Buffer.BlockCopy(buffer, 0, this.Buffer, 0, buffer.Length);
            }

            public BufferProperty(NativeMsmqMessage message, int id, int length) : this(message, id)
            {
                this.SetBufferReference(DiagnosticUtility.Utility.AllocateByteArray(length));
            }

            public override void Dispose()
            {
                base.Dispose();
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (this.bufferHandle.IsAllocated)
                {
                    this.bufferHandle.Free();
                }
            }

            public void EnsureBufferLength(int length)
            {
                if (this.buffer.Length < length)
                {
                    this.SetBufferReference(DiagnosticUtility.Utility.AllocateByteArray(length));
                }
            }

            ~BufferProperty()
            {
                this.Dispose(false);
            }

            public byte[] GetBufferCopy(int length)
            {
                byte[] dst = DiagnosticUtility.Utility.AllocateByteArray(length);
                System.Buffer.BlockCopy(this.buffer, 0, dst, 0, length);
                return dst;
            }

            public override void Pin()
            {
                this.bufferHandle.Target = this.buffer;
                base.Variants[base.Index].byteArrayValue.intPtr = this.bufferHandle.AddrOfPinnedObject();
            }

            public void SetBufferReference(byte[] buffer)
            {
                this.SetBufferReference(buffer, buffer.Length);
            }

            public void SetBufferReference(byte[] buffer, int length)
            {
                this.buffer = buffer;
                this.BufferLength = length;
            }

            public override void Unpin()
            {
                base.Variants[base.Index].byteArrayValue.intPtr = IntPtr.Zero;
                this.bufferHandle.Target = null;
            }

            public byte[] Buffer
            {
                get
                {
                    return this.buffer;
                }
            }

            public int BufferLength
            {
                get
                {
                    return base.Variants[base.Index].byteArrayValue.size;
                }
                set
                {
                    if (value > this.buffer.Length)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                    }
                    base.Variants[base.Index].byteArrayValue.size = value;
                }
            }

            public override object MaintainedBuffer
            {
                get
                {
                    return this.buffer;
                }
            }

            public override bool MaintainsBuffer
            {
                get
                {
                    return true;
                }
            }
        }

        public class ByteProperty : NativeMsmqMessage.MsmqProperty
        {
            public ByteProperty(NativeMsmqMessage message, int id) : base(message, id, 0x11)
            {
            }

            public ByteProperty(NativeMsmqMessage message, int id, byte value) : this(message, id)
            {
                this.Value = value;
            }

            public byte Value
            {
                get
                {
                    return base.Variants[base.Index].byteValue;
                }
                set
                {
                    base.Variants[base.Index].byteValue = value;
                }
            }
        }

        public class IntProperty : NativeMsmqMessage.MsmqProperty
        {
            public IntProperty(NativeMsmqMessage message, int id) : base(message, id, 0x13)
            {
            }

            public IntProperty(NativeMsmqMessage message, int id, int value) : this(message, id)
            {
                this.Value = value;
            }

            public int Value
            {
                get
                {
                    return base.Variants[base.Index].intValue;
                }
                set
                {
                    base.Variants[base.Index].intValue = value;
                }
            }
        }

        public class LongProperty : NativeMsmqMessage.MsmqProperty
        {
            public LongProperty(NativeMsmqMessage message, int id) : base(message, id, 0x15)
            {
            }

            public LongProperty(NativeMsmqMessage message, int id, long value) : this(message, id)
            {
                this.Value = value;
            }

            public long Value
            {
                get
                {
                    return base.Variants[base.Index].longValue;
                }
                set
                {
                    base.Variants[base.Index].longValue = value;
                }
            }
        }

        public abstract class MsmqProperty : IDisposable
        {
            private int index;
            private UnsafeNativeMethods.MQPROPVARIANT[] variants;

            protected MsmqProperty(NativeMsmqMessage message, int id, ushort vt)
            {
                this.variants = message.variants;
                this.index = message.nativeProperties.count++;
                message.variants[this.index].vt = vt;
                message.ids[this.index] = id;
                message.properties[this.index] = this;
            }

            public virtual void Dispose()
            {
            }

            public virtual void Pin()
            {
            }

            public virtual void Unpin()
            {
            }

            protected int Index
            {
                get
                {
                    return this.index;
                }
            }

            public virtual object MaintainedBuffer
            {
                get
                {
                    return null;
                }
            }

            public virtual bool MaintainsBuffer
            {
                get
                {
                    return false;
                }
            }

            protected UnsafeNativeMethods.MQPROPVARIANT[] Variants
            {
                get
                {
                    return this.variants;
                }
            }
        }

        public class ShortProperty : NativeMsmqMessage.MsmqProperty
        {
            public ShortProperty(NativeMsmqMessage message, int id) : base(message, id, 0x12)
            {
            }

            public ShortProperty(NativeMsmqMessage message, int id, short value) : this(message, id)
            {
                this.Value = value;
            }

            public short Value
            {
                get
                {
                    return base.Variants[base.Index].shortValue;
                }
                set
                {
                    base.Variants[base.Index].shortValue = value;
                }
            }
        }

        public class StringProperty : NativeMsmqMessage.MsmqProperty
        {
            private char[] buffer;
            private GCHandle bufferHandle;

            internal StringProperty(NativeMsmqMessage message, int id, int length) : base(message, id, 0x1f)
            {
                this.buffer = DiagnosticUtility.Utility.AllocateCharArray(length);
                this.bufferHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            }

            internal StringProperty(NativeMsmqMessage message, int id, string value) : this(message, id, (int) (value.Length + 1))
            {
                this.CopyValueToBuffer(value);
            }

            private void CopyValueToBuffer(string value)
            {
                value.CopyTo(0, this.buffer, 0, value.Length);
                this.buffer[value.Length] = '\0';
            }

            public override void Dispose()
            {
                base.Dispose();
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (this.bufferHandle.IsAllocated)
                {
                    this.bufferHandle.Free();
                }
            }

            public void EnsureValueLength(int length)
            {
                if (length > this.buffer.Length)
                {
                    this.buffer = DiagnosticUtility.Utility.AllocateCharArray(length);
                }
            }

            ~StringProperty()
            {
                this.Dispose(false);
            }

            public string GetValue(int length)
            {
                if (length == 0)
                {
                    return null;
                }
                return new string(this.buffer, 0, length - 1);
            }

            public override void Pin()
            {
                this.bufferHandle.Target = this.buffer;
                base.Variants[base.Index].intPtr = this.bufferHandle.AddrOfPinnedObject();
            }

            public void SetValue(string value)
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.EnsureValueLength(value.Length + 1);
                this.CopyValueToBuffer(value);
            }

            public override void Unpin()
            {
                base.Variants[base.Index].intPtr = IntPtr.Zero;
                this.bufferHandle.Target = null;
            }

            public override object MaintainedBuffer
            {
                get
                {
                    return this.buffer;
                }
            }

            public override bool MaintainsBuffer
            {
                get
                {
                    return true;
                }
            }
        }
    }
}


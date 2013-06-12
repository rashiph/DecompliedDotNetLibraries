namespace System.IO
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [Serializable, ComVisible(true)]
    public class BinaryWriter : IDisposable
    {
        private byte[] _buffer;
        private System.Text.Encoder _encoder;
        private Encoding _encoding;
        private byte[] _largeByteBuffer;
        private int _maxChars;
        private char[] _tmpOneCharBuffer;
        private const int LargeByteBufferSize = 0x100;
        public static readonly BinaryWriter Null = new BinaryWriter();
        protected Stream OutStream;

        protected BinaryWriter()
        {
            this._tmpOneCharBuffer = new char[1];
            this.OutStream = Stream.Null;
            this._buffer = new byte[0x10];
            this._encoding = new UTF8Encoding(false, true);
            this._encoder = this._encoding.GetEncoder();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public BinaryWriter(Stream output) : this(output, new UTF8Encoding(false, true))
        {
        }

        public BinaryWriter(Stream output, Encoding encoding)
        {
            this._tmpOneCharBuffer = new char[1];
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (!output.CanWrite)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotWritable"));
            }
            this.OutStream = output;
            this._buffer = new byte[0x10];
            this._encoding = encoding;
            this._encoder = this._encoding.GetEncoder();
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.OutStream.Close();
            }
        }

        public virtual void Flush()
        {
            this.OutStream.Flush();
        }

        public virtual long Seek(int offset, SeekOrigin origin)
        {
            return this.OutStream.Seek((long) offset, origin);
        }

        public virtual void Write(bool value)
        {
            this._buffer[0] = value ? ((byte) 1) : ((byte) 0);
            this.OutStream.Write(this._buffer, 0, 1);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual void Write(byte value)
        {
            this.OutStream.WriteByte(value);
        }

        public virtual void Write(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            this.OutStream.Write(buffer, 0, buffer.Length);
        }

        [SecuritySafeCritical]
        public virtual unsafe void Write(char ch)
        {
            if (char.IsSurrogate(ch))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_SurrogatesNotAllowedAsSingleChar"));
            }
            int count = 0;
            fixed (byte* numRef = this._buffer)
            {
                count = this._encoder.GetBytes(&ch, 1, numRef, 0x10, true);
            }
            this.OutStream.Write(this._buffer, 0, count);
        }

        public virtual void Write(char[] chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            byte[] buffer = this._encoding.GetBytes(chars, 0, chars.Length);
            this.OutStream.Write(buffer, 0, buffer.Length);
        }

        public virtual void Write(decimal value)
        {
            decimal.GetBytes(value, this._buffer);
            this.OutStream.Write(this._buffer, 0, 0x10);
        }

        [SecuritySafeCritical]
        public virtual unsafe void Write(double value)
        {
            ulong num = *((ulong*) &value);
            this._buffer[0] = (byte) num;
            this._buffer[1] = (byte) (num >> 8);
            this._buffer[2] = (byte) (num >> 0x10);
            this._buffer[3] = (byte) (num >> 0x18);
            this._buffer[4] = (byte) (num >> 0x20);
            this._buffer[5] = (byte) (num >> 40);
            this._buffer[6] = (byte) (num >> 0x30);
            this._buffer[7] = (byte) (num >> 0x38);
            this.OutStream.Write(this._buffer, 0, 8);
        }

        public virtual void Write(short value)
        {
            this._buffer[0] = (byte) value;
            this._buffer[1] = (byte) (value >> 8);
            this.OutStream.Write(this._buffer, 0, 2);
        }

        public virtual void Write(int value)
        {
            this._buffer[0] = (byte) value;
            this._buffer[1] = (byte) (value >> 8);
            this._buffer[2] = (byte) (value >> 0x10);
            this._buffer[3] = (byte) (value >> 0x18);
            this.OutStream.Write(this._buffer, 0, 4);
        }

        public virtual void Write(long value)
        {
            this._buffer[0] = (byte) value;
            this._buffer[1] = (byte) (value >> 8);
            this._buffer[2] = (byte) (value >> 0x10);
            this._buffer[3] = (byte) (value >> 0x18);
            this._buffer[4] = (byte) (value >> 0x20);
            this._buffer[5] = (byte) (value >> 40);
            this._buffer[6] = (byte) (value >> 0x30);
            this._buffer[7] = (byte) (value >> 0x38);
            this.OutStream.Write(this._buffer, 0, 8);
        }

        [CLSCompliant(false)]
        public virtual void Write(sbyte value)
        {
            this.OutStream.WriteByte((byte) value);
        }

        [SecuritySafeCritical]
        public virtual unsafe void Write(float value)
        {
            uint num = *((uint*) &value);
            this._buffer[0] = (byte) num;
            this._buffer[1] = (byte) (num >> 8);
            this._buffer[2] = (byte) (num >> 0x10);
            this._buffer[3] = (byte) (num >> 0x18);
            this.OutStream.Write(this._buffer, 0, 4);
        }

        [SecuritySafeCritical]
        public virtual unsafe void Write(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int byteCount = this._encoding.GetByteCount(value);
            this.Write7BitEncodedInt(byteCount);
            if (this._largeByteBuffer == null)
            {
                this._largeByteBuffer = new byte[0x100];
                this._maxChars = 0x100 / this._encoding.GetMaxByteCount(1);
            }
            if (byteCount <= 0x100)
            {
                this._encoding.GetBytes(value, 0, value.Length, this._largeByteBuffer, 0);
                this.OutStream.Write(this._largeByteBuffer, 0, byteCount);
            }
            else
            {
                int num4;
                int num2 = 0;
                for (int i = value.Length; i > 0; i -= num4)
                {
                    num4 = (i > this._maxChars) ? this._maxChars : i;
                    fixed (char* str = ((char*) value))
                    {
                        int num5;
                        char* chPtr = str;
                        fixed (byte* numRef = this._largeByteBuffer)
                        {
                            num5 = this._encoder.GetBytes(chPtr + num2, num4, numRef, 0x100, num4 == i);
                            str = null;
                        }
                        this.OutStream.Write(this._largeByteBuffer, 0, num5);
                        num2 += num4;
                    }
                }
            }
        }

        [CLSCompliant(false)]
        public virtual void Write(ushort value)
        {
            this._buffer[0] = (byte) value;
            this._buffer[1] = (byte) (value >> 8);
            this.OutStream.Write(this._buffer, 0, 2);
        }

        [CLSCompliant(false)]
        public virtual void Write(uint value)
        {
            this._buffer[0] = (byte) value;
            this._buffer[1] = (byte) (value >> 8);
            this._buffer[2] = (byte) (value >> 0x10);
            this._buffer[3] = (byte) (value >> 0x18);
            this.OutStream.Write(this._buffer, 0, 4);
        }

        [CLSCompliant(false)]
        public virtual void Write(ulong value)
        {
            this._buffer[0] = (byte) value;
            this._buffer[1] = (byte) (value >> 8);
            this._buffer[2] = (byte) (value >> 0x10);
            this._buffer[3] = (byte) (value >> 0x18);
            this._buffer[4] = (byte) (value >> 0x20);
            this._buffer[5] = (byte) (value >> 40);
            this._buffer[6] = (byte) (value >> 0x30);
            this._buffer[7] = (byte) (value >> 0x38);
            this.OutStream.Write(this._buffer, 0, 8);
        }

        public virtual void Write(byte[] buffer, int index, int count)
        {
            this.OutStream.Write(buffer, index, count);
        }

        public virtual void Write(char[] chars, int index, int count)
        {
            byte[] buffer = this._encoding.GetBytes(chars, index, count);
            this.OutStream.Write(buffer, 0, buffer.Length);
        }

        protected void Write7BitEncodedInt(int value)
        {
            uint num = (uint) value;
            while (num >= 0x80)
            {
                this.Write((byte) (num | 0x80));
                num = num >> 7;
            }
            this.Write((byte) num);
        }

        public virtual Stream BaseStream
        {
            get
            {
                this.Flush();
                return this.OutStream;
            }
        }
    }
}


namespace System.IO
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [ComVisible(true)]
    public class BinaryReader : IDisposable
    {
        private bool m_2BytesPerChar;
        private byte[] m_buffer;
        private char[] m_charBuffer;
        private byte[] m_charBytes;
        private System.Text.Decoder m_decoder;
        private bool m_isMemoryStream;
        private int m_maxCharsSize;
        private char[] m_singleChar;
        private Stream m_stream;
        private const int MaxCharBytesSize = 0x80;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public BinaryReader(Stream input) : this(input, new UTF8Encoding())
        {
        }

        public BinaryReader(Stream input, Encoding encoding)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (!input.CanRead)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_StreamNotReadable"));
            }
            this.m_stream = input;
            this.m_decoder = encoding.GetDecoder();
            this.m_maxCharsSize = encoding.GetMaxCharCount(0x80);
            int maxByteCount = encoding.GetMaxByteCount(1);
            if (maxByteCount < 0x10)
            {
                maxByteCount = 0x10;
            }
            this.m_buffer = new byte[maxByteCount];
            this.m_charBuffer = null;
            this.m_charBytes = null;
            this.m_2BytesPerChar = encoding is UnicodeEncoding;
            this.m_isMemoryStream = this.m_stream.GetType() == typeof(MemoryStream);
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
                Stream stream = this.m_stream;
                this.m_stream = null;
                if (stream != null)
                {
                    stream.Close();
                }
            }
            this.m_stream = null;
            this.m_buffer = null;
            this.m_decoder = null;
            this.m_charBytes = null;
            this.m_singleChar = null;
            this.m_charBuffer = null;
        }

        protected virtual void FillBuffer(int numBytes)
        {
            if ((this.m_buffer != null) && ((numBytes < 0) || (numBytes > this.m_buffer.Length)))
            {
                throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_BinaryReaderFillBuffer"));
            }
            int offset = 0;
            int num2 = 0;
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            if (numBytes == 1)
            {
                num2 = this.m_stream.ReadByte();
                if (num2 == -1)
                {
                    __Error.EndOfFile();
                }
                this.m_buffer[0] = (byte) num2;
            }
            else
            {
                do
                {
                    num2 = this.m_stream.Read(this.m_buffer, offset, numBytes - offset);
                    if (num2 == 0)
                    {
                        __Error.EndOfFile();
                    }
                    offset += num2;
                }
                while (offset < numBytes);
            }
        }

        [SecuritySafeCritical]
        private unsafe int InternalReadChars(char[] buffer, int index, int count)
        {
            int num = 0;
            int charCount = count;
            if (this.m_charBytes == null)
            {
                this.m_charBytes = new byte[0x80];
            }
            while (charCount > 0)
            {
                int num3 = 0;
                num = charCount;
                UTF8Encoding.UTF8Decoder decoder = this.m_decoder as UTF8Encoding.UTF8Decoder;
                if (((decoder != null) && decoder.HasState) && (num > 1))
                {
                    num--;
                }
                if (this.m_2BytesPerChar)
                {
                    num = num << 1;
                }
                if (num > 0x80)
                {
                    num = 0x80;
                }
                int position = 0;
                byte[] charBytes = null;
                if (this.m_isMemoryStream)
                {
                    MemoryStream stream = this.m_stream as MemoryStream;
                    position = stream.InternalGetPosition();
                    num = stream.InternalEmulateRead(num);
                    charBytes = stream.InternalGetBuffer();
                }
                else
                {
                    num = this.m_stream.Read(this.m_charBytes, 0, num);
                    charBytes = this.m_charBytes;
                }
                if (num == 0)
                {
                    return (count - charCount);
                }
                fixed (byte* numRef = charBytes)
                {
                    fixed (char* chRef = buffer)
                    {
                        num3 = this.m_decoder.GetChars(numRef + position, num, chRef + index, charCount, false);
                    }
                }
                charCount -= num3;
                index += num3;
            }
            return (count - charCount);
        }

        private int InternalReadOneChar()
        {
            long position;
            int num = 0;
            int byteCount = 0;
            position = position = 0L;
            if (this.m_stream.CanSeek)
            {
                position = this.m_stream.Position;
            }
            if (this.m_charBytes == null)
            {
                this.m_charBytes = new byte[0x80];
            }
            if (this.m_singleChar == null)
            {
                this.m_singleChar = new char[1];
            }
            while (num == 0)
            {
                byteCount = this.m_2BytesPerChar ? 2 : 1;
                int num4 = this.m_stream.ReadByte();
                this.m_charBytes[0] = (byte) num4;
                if (num4 == -1)
                {
                    byteCount = 0;
                }
                if (byteCount == 2)
                {
                    num4 = this.m_stream.ReadByte();
                    this.m_charBytes[1] = (byte) num4;
                    if (num4 == -1)
                    {
                        byteCount = 1;
                    }
                }
                if (byteCount == 0)
                {
                    return -1;
                }
                try
                {
                    num = this.m_decoder.GetChars(this.m_charBytes, 0, byteCount, this.m_singleChar, 0);
                    continue;
                }
                catch
                {
                    if (this.m_stream.CanSeek)
                    {
                        this.m_stream.Seek(position - this.m_stream.Position, SeekOrigin.Current);
                    }
                    throw;
                }
            }
            if (num == 0)
            {
                return -1;
            }
            return this.m_singleChar[0];
        }

        public virtual int PeekChar()
        {
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            if (!this.m_stream.CanSeek)
            {
                return -1;
            }
            long position = this.m_stream.Position;
            int num2 = this.Read();
            this.m_stream.Position = position;
            return num2;
        }

        public virtual int Read()
        {
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            return this.InternalReadOneChar();
        }

        public virtual int Read(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            return this.m_stream.Read(buffer, index, count);
        }

        public virtual int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            return this.InternalReadChars(buffer, index, count);
        }

        protected internal int Read7BitEncodedInt()
        {
            byte num3;
            int num = 0;
            int num2 = 0;
            do
            {
                if (num2 == 0x23)
                {
                    throw new FormatException(Environment.GetResourceString("Format_Bad7BitInt32"));
                }
                num3 = this.ReadByte();
                num |= (num3 & 0x7f) << num2;
                num2 += 7;
            }
            while ((num3 & 0x80) != 0);
            return num;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual bool ReadBoolean()
        {
            this.FillBuffer(1);
            return (this.m_buffer[0] != 0);
        }

        public virtual byte ReadByte()
        {
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            int num = this.m_stream.ReadByte();
            if (num == -1)
            {
                __Error.EndOfFile();
            }
            return (byte) num;
        }

        [SecuritySafeCritical]
        public virtual byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            byte[] buffer = new byte[count];
            int offset = 0;
            do
            {
                int num2 = this.m_stream.Read(buffer, offset, count);
                if (num2 == 0)
                {
                    break;
                }
                offset += num2;
                count -= num2;
            }
            while (count > 0);
            if (offset != buffer.Length)
            {
                byte[] dst = new byte[offset];
                Buffer.InternalBlockCopy(buffer, 0, dst, 0, offset);
                buffer = dst;
            }
            return buffer;
        }

        public virtual char ReadChar()
        {
            int num = this.Read();
            if (num == -1)
            {
                __Error.EndOfFile();
            }
            return (char) num;
        }

        [SecuritySafeCritical]
        public virtual char[] ReadChars(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            char[] buffer = new char[count];
            int num = this.InternalReadChars(buffer, 0, count);
            if (num != count)
            {
                char[] dst = new char[num];
                Buffer.InternalBlockCopy(buffer, 0, dst, 0, 2 * num);
                buffer = dst;
            }
            return buffer;
        }

        public virtual decimal ReadDecimal()
        {
            decimal num;
            this.FillBuffer(0x10);
            try
            {
                num = decimal.ToDecimal(this.m_buffer);
            }
            catch (ArgumentException exception)
            {
                throw new IOException(Environment.GetResourceString("Arg_DecBitCtor"), exception);
            }
            return num;
        }

        [SecuritySafeCritical]
        public virtual unsafe double ReadDouble()
        {
            this.FillBuffer(8);
            uint num = (uint) (((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
            uint num2 = (uint) (((this.m_buffer[4] | (this.m_buffer[5] << 8)) | (this.m_buffer[6] << 0x10)) | (this.m_buffer[7] << 0x18));
            ulong num3 = (num2 << 0x20) | num;
            return *(((double*) &num3));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual short ReadInt16()
        {
            this.FillBuffer(2);
            return (short) (this.m_buffer[0] | (this.m_buffer[1] << 8));
        }

        public virtual int ReadInt32()
        {
            if (this.m_isMemoryStream)
            {
                if (this.m_stream == null)
                {
                    __Error.FileNotOpen();
                }
                MemoryStream stream = this.m_stream as MemoryStream;
                return stream.InternalReadInt32();
            }
            this.FillBuffer(4);
            return (((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
        }

        public virtual long ReadInt64()
        {
            this.FillBuffer(8);
            uint num = (uint) (((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
            uint num2 = (uint) (((this.m_buffer[4] | (this.m_buffer[5] << 8)) | (this.m_buffer[6] << 0x10)) | (this.m_buffer[7] << 0x18));
            return (long) ((num2 << 0x20) | num);
        }

        [CLSCompliant(false), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual sbyte ReadSByte()
        {
            this.FillBuffer(1);
            return (sbyte) this.m_buffer[0];
        }

        [SecuritySafeCritical]
        public virtual unsafe float ReadSingle()
        {
            this.FillBuffer(4);
            uint num = (uint) (((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
            return *(((float*) &num));
        }

        [SecuritySafeCritical]
        public virtual string ReadString()
        {
            if (this.m_stream == null)
            {
                __Error.FileNotOpen();
            }
            int num = 0;
            int capacity = this.Read7BitEncodedInt();
            if (capacity < 0)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_InvalidStringLen_Len", new object[] { capacity }));
            }
            if (capacity == 0)
            {
                return string.Empty;
            }
            if (this.m_charBytes == null)
            {
                this.m_charBytes = new byte[0x80];
            }
            if (this.m_charBuffer == null)
            {
                this.m_charBuffer = new char[this.m_maxCharsSize];
            }
            StringBuilder builder = null;
            do
            {
                int count = ((capacity - num) > 0x80) ? 0x80 : (capacity - num);
                int byteCount = this.m_stream.Read(this.m_charBytes, 0, count);
                if (byteCount == 0)
                {
                    __Error.EndOfFile();
                }
                int length = this.m_decoder.GetChars(this.m_charBytes, 0, byteCount, this.m_charBuffer, 0);
                if ((num == 0) && (byteCount == capacity))
                {
                    return new string(this.m_charBuffer, 0, length);
                }
                if (builder == null)
                {
                    builder = new StringBuilder(capacity);
                }
                builder.Append(this.m_charBuffer, 0, length);
                num += byteCount;
            }
            while (num < capacity);
            return builder.ToString();
        }

        [CLSCompliant(false)]
        public virtual ushort ReadUInt16()
        {
            this.FillBuffer(2);
            return (ushort) (this.m_buffer[0] | (this.m_buffer[1] << 8));
        }

        [CLSCompliant(false)]
        public virtual uint ReadUInt32()
        {
            this.FillBuffer(4);
            return (uint) (((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
        }

        [CLSCompliant(false)]
        public virtual ulong ReadUInt64()
        {
            this.FillBuffer(8);
            uint num = (uint) (((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
            uint num2 = (uint) (((this.m_buffer[4] | (this.m_buffer[5] << 8)) | (this.m_buffer[6] << 0x10)) | (this.m_buffer[7] << 0x18));
            return ((num2 << 0x20) | num);
        }

        public virtual Stream BaseStream
        {
            get
            {
                return this.m_stream;
            }
        }
    }
}


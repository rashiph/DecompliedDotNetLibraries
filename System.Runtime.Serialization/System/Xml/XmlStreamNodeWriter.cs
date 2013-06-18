namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal abstract class XmlStreamNodeWriter : XmlNodeWriter
    {
        private byte[] buffer = new byte[0x200];
        private const int bufferLength = 0x200;
        private Encoding encoding = UTF8Encoding;
        private const int maxBytesPerChar = 3;
        private const int maxEntityLength = 0x20;
        private int offset;
        private bool ownsStream;
        private System.IO.Stream stream;
        private static System.Text.UTF8Encoding UTF8Encoding = new System.Text.UTF8Encoding(false, true);

        protected XmlStreamNodeWriter()
        {
        }

        protected void Advance(int count)
        {
            this.offset += count;
        }

        public override void Close()
        {
            if (this.stream != null)
            {
                if (this.ownsStream)
                {
                    this.stream.Close();
                }
                this.stream = null;
            }
        }

        private void EnsureByte()
        {
            if (this.offset >= 0x200)
            {
                this.FlushBuffer();
            }
        }

        public override void Flush()
        {
            this.FlushBuffer();
            this.stream.Flush();
        }

        protected virtual void FlushBuffer()
        {
            if (this.offset != 0)
            {
                this.stream.Write(this.buffer, 0, this.offset);
                this.offset = 0;
            }
        }

        protected byte[] GetBuffer(int count, out int offset)
        {
            int num = this.offset;
            if ((num + count) <= 0x200)
            {
                offset = num;
            }
            else
            {
                this.FlushBuffer();
                offset = 0;
            }
            return this.buffer;
        }

        protected void SetOutput(System.IO.Stream stream, bool ownsStream, Encoding encoding)
        {
            this.stream = stream;
            this.ownsStream = ownsStream;
            this.offset = 0;
            if (encoding != null)
            {
                this.encoding = encoding;
            }
        }

        [SecurityCritical]
        protected unsafe int UnsafeGetUnicodeChars(char* chars, int charCount, byte[] buffer, int offset)
        {
            char* chPtr = chars + charCount;
            while (chars < chPtr)
            {
                chars++;
                char ch = chars[0];
                buffer[offset++] = (byte) ch;
                ch = ch >> 8;
                buffer[offset++] = (byte) ch;
            }
            return (charCount * 2);
        }

        [SecurityCritical]
        protected unsafe int UnsafeGetUTF8Chars(char* chars, int charCount, byte[] buffer, int offset)
        {
            if (charCount <= 0)
            {
                return 0;
            }
            fixed (byte* numRef = &(buffer[offset]))
            {
                byte* bytes = numRef;
                byte* numPtr2 = bytes + (buffer.Length - offset);
                char* chPtr = chars + charCount;
                do
                {
                    while ((chars < chPtr) && (chars[0] < '\x0080'))
                    {
                        bytes[0] = (byte) chars[0];
                        bytes++;
                        chars++;
                    }
                    if (chars >= chPtr)
                    {
                        break;
                    }
                    char* chPtr2 = chars;
                    while ((chars < chPtr) && (chars[0] >= '\x0080'))
                    {
                        chars++;
                    }
                    bytes += this.encoding.GetBytes(chPtr2, (int) ((long) ((chars - chPtr2) / 2)), bytes, (int) ((long) ((numPtr2 - bytes) / 1)));
                }
                while (chars < chPtr);
                return (int) ((long) ((bytes - numRef) / 1));
            }
        }

        [SecurityCritical]
        protected unsafe int UnsafeGetUTF8Length(char* chars, int charCount)
        {
            char* chPtr = chars + charCount;
            while (chars < chPtr)
            {
                if (chars[0] >= '\x0080')
                {
                    break;
                }
                chars++;
            }
            if (chars == chPtr)
            {
                return charCount;
            }
            return (((int) ((long) ((chars - (chPtr - charCount)) / 2))) + this.encoding.GetByteCount(chars, (int) ((long) ((chPtr - chars) / 2))));
        }

        [SecurityCritical]
        protected unsafe void UnsafeWriteBytes(byte* bytes, int byteCount)
        {
            this.FlushBuffer();
            byte[] buffer = this.buffer;
            while (byteCount >= 0x200)
            {
                for (int j = 0; j < 0x200; j++)
                {
                    buffer[j] = bytes[j];
                }
                this.stream.Write(buffer, 0, 0x200);
                bytes += 0x200;
                byteCount -= 0x200;
            }
            for (int i = 0; i < byteCount; i++)
            {
                buffer[i] = bytes[i];
            }
            this.stream.Write(buffer, 0, byteCount);
        }

        [SecurityCritical]
        protected unsafe void UnsafeWriteUnicodeChars(char* chars, int charCount)
        {
            while (charCount > 0x100)
            {
                int num;
                int num2 = 0x100;
                if ((chars[num2 - 1] & 0xfc00) == 0xd800)
                {
                    num2--;
                }
                byte[] buffer = this.GetBuffer(num2 * 2, out num);
                this.Advance(this.UnsafeGetUnicodeChars(chars, num2, buffer, num));
                charCount -= num2;
                chars += num2;
            }
            if (charCount > 0)
            {
                int num3;
                byte[] buffer2 = this.GetBuffer(charCount * 2, out num3);
                this.Advance(this.UnsafeGetUnicodeChars(chars, charCount, buffer2, num3));
            }
        }

        [SecurityCritical]
        protected unsafe void UnsafeWriteUTF8Chars(char* chars, int charCount)
        {
            while (charCount > 170)
            {
                int num;
                int num2 = 170;
                if ((chars[num2 - 1] & 0xfc00) == 0xd800)
                {
                    num2--;
                }
                byte[] buffer = this.GetBuffer(num2 * 3, out num);
                this.Advance(this.UnsafeGetUTF8Chars(chars, num2, buffer, num));
                charCount -= num2;
                chars += num2;
            }
            if (charCount > 0)
            {
                int num3;
                byte[] buffer2 = this.GetBuffer(charCount * 3, out num3);
                this.Advance(this.UnsafeGetUTF8Chars(chars, charCount, buffer2, num3));
            }
        }

        protected void WriteByte(byte b)
        {
            this.EnsureByte();
            this.buffer[this.offset++] = b;
        }

        protected void WriteByte(char ch)
        {
            this.WriteByte((byte) ch);
        }

        protected void WriteBytes(byte b1, byte b2)
        {
            byte[] buffer = this.buffer;
            int offset = this.offset;
            if ((offset + 1) >= 0x200)
            {
                this.FlushBuffer();
                offset = 0;
            }
            buffer[offset] = b1;
            buffer[offset + 1] = b2;
            this.offset += 2;
        }

        protected void WriteBytes(char ch1, char ch2)
        {
            this.WriteBytes((byte) ch1, (byte) ch2);
        }

        public void WriteBytes(byte[] byteBuffer, int byteOffset, int byteCount)
        {
            if (byteCount < 0x200)
            {
                int num;
                byte[] dst = this.GetBuffer(byteCount, out num);
                Buffer.BlockCopy(byteBuffer, byteOffset, dst, num, byteCount);
                this.Advance(byteCount);
            }
            else
            {
                this.FlushBuffer();
                this.stream.Write(byteBuffer, byteOffset, byteCount);
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteUTF8Char(int ch)
        {
            if (ch < 0x80)
            {
                this.WriteByte((byte) ch);
            }
            else if (ch <= 0xffff)
            {
                char* chars = (char*) stackalloc byte[(((IntPtr) 1) * 2)];
                chars[0] = (char) ch;
                this.UnsafeWriteUTF8Chars(chars, 1);
            }
            else
            {
                SurrogateChar ch2 = new SurrogateChar(ch);
                char* chPtr2 = (char*) stackalloc byte[(((IntPtr) 2) * 2)];
                chPtr2[0] = ch2.HighChar;
                chPtr2[1] = ch2.LowChar;
                this.UnsafeWriteUTF8Chars(chPtr2, 2);
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteUTF8Chars(string value)
        {
            int length = value.Length;
            if (length > 0)
            {
                fixed (char* str = ((char*) value))
                {
                    char* chars = str;
                    this.UnsafeWriteUTF8Chars(chars, length);
                }
            }
        }

        protected void WriteUTF8Chars(byte[] chars, int charOffset, int charCount)
        {
            if (charCount < 0x200)
            {
                int num;
                byte[] dst = this.GetBuffer(charCount, out num);
                Buffer.BlockCopy(chars, charOffset, dst, num, charCount);
                this.Advance(charCount);
            }
            else
            {
                this.FlushBuffer();
                this.stream.Write(chars, charOffset, charCount);
            }
        }

        public int BufferOffset
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.offset;
            }
        }

        public int Position
        {
            get
            {
                return (((int) this.stream.Position) + this.offset);
            }
        }

        public System.IO.Stream Stream
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.stream;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.stream = value;
            }
        }

        public byte[] StreamBuffer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buffer;
            }
        }
    }
}


namespace System.Runtime.Serialization.Json
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    internal class JsonEncodingStreamWrapper : Stream
    {
        private const int BufferLength = 0x80;
        private byte[] byteBuffer = new byte[1];
        private int byteCount;
        private int byteOffset;
        private byte[] bytes;
        private char[] chars;
        private System.Text.Decoder dec;
        private System.Text.Encoder enc;
        private Encoding encoding;
        private SupportedEncoding encodingCode;
        private bool isReading;
        private static readonly UnicodeEncoding SafeBEUTF16 = new UnicodeEncoding(true, false, false);
        private static readonly UnicodeEncoding SafeUTF16 = new UnicodeEncoding(false, false, false);
        private static readonly UTF8Encoding SafeUTF8 = new UTF8Encoding(false, false);
        private Stream stream;
        private static readonly UnicodeEncoding ValidatingBEUTF16 = new UnicodeEncoding(true, false, true);
        private static readonly UnicodeEncoding ValidatingUTF16 = new UnicodeEncoding(false, false, true);
        private static readonly UTF8Encoding ValidatingUTF8 = new UTF8Encoding(false, true);

        public JsonEncodingStreamWrapper(Stream stream, Encoding encoding, bool isReader)
        {
            this.isReading = isReader;
            if (isReader)
            {
                this.InitForReading(stream, encoding);
            }
            else
            {
                if (encoding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
                }
                this.InitForWriting(stream, encoding);
            }
        }

        private void CleanupCharBreak()
        {
            int num3;
            int num = this.byteOffset + this.byteCount;
            if ((this.byteCount % 2) != 0)
            {
                int num2 = this.stream.ReadByte();
                if (num2 < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonUnexpectedEndOfFile")));
                }
                this.bytes[num++] = (byte) num2;
                this.byteCount++;
            }
            if (this.encodingCode == SupportedEncoding.UTF16LE)
            {
                num3 = this.bytes[num - 2] + (this.bytes[num - 1] << 8);
            }
            else
            {
                num3 = this.bytes[num - 1] + (this.bytes[num - 2] << 8);
            }
            if ((((num3 & 0xdc00) != 0xdc00) && (num3 >= 0xd800)) && (num3 <= 0xdbff))
            {
                int num4 = this.stream.ReadByte();
                int num5 = this.stream.ReadByte();
                if (num5 < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonUnexpectedEndOfFile")));
                }
                this.bytes[num++] = (byte) num4;
                this.bytes[num++] = (byte) num5;
                this.byteCount += 2;
            }
        }

        public override void Close()
        {
            this.Flush();
            base.Close();
            this.stream.Close();
        }

        private void EnsureBuffers()
        {
            this.EnsureByteBuffer();
            if (this.chars == null)
            {
                this.chars = new char[0x80];
            }
        }

        private void EnsureByteBuffer()
        {
            if (this.bytes == null)
            {
                this.bytes = new byte[0x200];
                this.byteOffset = 0;
                this.byteCount = 0;
            }
        }

        private void FillBuffer(int count)
        {
            count -= this.byteCount;
            while (count > 0)
            {
                int num = this.stream.Read(this.bytes, this.byteOffset + this.byteCount, count);
                if (num == 0)
                {
                    return;
                }
                this.byteCount += num;
                count -= num;
            }
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        private static Encoding GetEncoding(SupportedEncoding e)
        {
            switch (e)
            {
                case SupportedEncoding.UTF8:
                    return ValidatingUTF8;

                case SupportedEncoding.UTF16LE:
                    return ValidatingUTF16;

                case SupportedEncoding.UTF16BE:
                    return ValidatingBEUTF16;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncodingNotSupported")));
        }

        private static string GetEncodingName(SupportedEncoding enc)
        {
            switch (enc)
            {
                case SupportedEncoding.UTF8:
                    return "utf-8";

                case SupportedEncoding.UTF16LE:
                    return "utf-16LE";

                case SupportedEncoding.UTF16BE:
                    return "utf-16BE";
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncodingNotSupported")));
        }

        private static SupportedEncoding GetSupportedEncoding(Encoding encoding)
        {
            if (encoding == null)
            {
                return SupportedEncoding.None;
            }
            if (encoding.WebName == ValidatingUTF8.WebName)
            {
                return SupportedEncoding.UTF8;
            }
            if (encoding.WebName == ValidatingUTF16.WebName)
            {
                return SupportedEncoding.UTF16LE;
            }
            if (encoding.WebName != ValidatingBEUTF16.WebName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonEncodingNotSupported")));
            }
            return SupportedEncoding.UTF16BE;
        }

        private void InitForReading(Stream inputStream, Encoding expectedEncoding)
        {
            try
            {
                this.stream = new BufferedStream(inputStream);
                SupportedEncoding supportedEncoding = GetSupportedEncoding(expectedEncoding);
                SupportedEncoding actualEnc = this.ReadEncoding();
                if ((supportedEncoding != SupportedEncoding.None) && (supportedEncoding != actualEnc))
                {
                    ThrowExpectedEncodingMismatch(supportedEncoding, actualEnc);
                }
                if (actualEnc != SupportedEncoding.UTF8)
                {
                    this.EnsureBuffers();
                    this.FillBuffer(0xfe);
                    this.encodingCode = actualEnc;
                    this.encoding = GetEncoding(actualEnc);
                    this.CleanupCharBreak();
                    int charCount = this.encoding.GetChars(this.bytes, this.byteOffset, this.byteCount, this.chars, 0);
                    this.byteOffset = 0;
                    this.byteCount = ValidatingUTF8.GetBytes(this.chars, 0, charCount, this.bytes, 0);
                }
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidBytes"), exception));
            }
        }

        private void InitForWriting(Stream outputStream, Encoding writeEncoding)
        {
            this.encoding = writeEncoding;
            this.stream = new BufferedStream(outputStream);
            this.encodingCode = GetSupportedEncoding(writeEncoding);
            if (this.encodingCode != SupportedEncoding.UTF8)
            {
                this.EnsureBuffers();
                this.dec = ValidatingUTF8.GetDecoder();
                this.enc = this.encoding.GetEncoder();
            }
        }

        public static ArraySegment<byte> ProcessBuffer(byte[] buffer, int offset, int count, Encoding encoding)
        {
            ArraySegment<byte> segment;
            try
            {
                SupportedEncoding encoding3;
                SupportedEncoding supportedEncoding = GetSupportedEncoding(encoding);
                if (count < 2)
                {
                    encoding3 = SupportedEncoding.UTF8;
                }
                else
                {
                    encoding3 = ReadEncoding(buffer[offset], buffer[offset + 1]);
                }
                if ((supportedEncoding != SupportedEncoding.None) && (supportedEncoding != encoding3))
                {
                    ThrowExpectedEncodingMismatch(supportedEncoding, encoding3);
                }
                if (encoding3 == SupportedEncoding.UTF8)
                {
                    return new ArraySegment<byte>(buffer, offset, count);
                }
                segment = new ArraySegment<byte>(ValidatingUTF8.GetBytes(GetEncoding(encoding3).GetChars(buffer, offset, count)));
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidBytes"), exception));
            }
            return segment;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num2;
            try
            {
                if (this.byteCount == 0)
                {
                    if (this.encodingCode == SupportedEncoding.UTF8)
                    {
                        return this.stream.Read(buffer, offset, count);
                    }
                    this.byteOffset = 0;
                    this.byteCount = this.stream.Read(this.bytes, this.byteCount, (this.chars.Length - 1) * 2);
                    if (this.byteCount == 0)
                    {
                        return 0;
                    }
                    this.CleanupCharBreak();
                    int charCount = this.encoding.GetChars(this.bytes, 0, this.byteCount, this.chars, 0);
                    this.byteCount = Encoding.UTF8.GetBytes(this.chars, 0, charCount, this.bytes, 0);
                }
                if (this.byteCount < count)
                {
                    count = this.byteCount;
                }
                Buffer.BlockCopy(this.bytes, this.byteOffset, buffer, offset, count);
                this.byteOffset += count;
                this.byteCount -= count;
                num2 = count;
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidBytes"), exception));
            }
            return num2;
        }

        public override int ReadByte()
        {
            if ((this.byteCount == 0) && (this.encodingCode == SupportedEncoding.UTF8))
            {
                return this.stream.ReadByte();
            }
            if (this.Read(this.byteBuffer, 0, 1) == 0)
            {
                return -1;
            }
            return this.byteBuffer[0];
        }

        private SupportedEncoding ReadEncoding()
        {
            SupportedEncoding encoding;
            int num = this.stream.ReadByte();
            int num2 = this.stream.ReadByte();
            this.EnsureByteBuffer();
            if (num == -1)
            {
                encoding = SupportedEncoding.UTF8;
                this.byteCount = 0;
                return encoding;
            }
            if (num2 == -1)
            {
                encoding = SupportedEncoding.UTF8;
                this.bytes[0] = (byte) num;
                this.byteCount = 1;
                return encoding;
            }
            encoding = ReadEncoding((byte) num, (byte) num2);
            this.bytes[0] = (byte) num;
            this.bytes[1] = (byte) num2;
            this.byteCount = 2;
            return encoding;
        }

        private static SupportedEncoding ReadEncoding(byte b1, byte b2)
        {
            if ((b1 == 0) && (b2 != 0))
            {
                return SupportedEncoding.UTF16BE;
            }
            if ((b1 != 0) && (b2 == 0))
            {
                return SupportedEncoding.UTF16LE;
            }
            if ((b1 == 0) && (b2 == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonInvalidBytes")));
            }
            return SupportedEncoding.UTF8;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        private static void ThrowExpectedEncodingMismatch(SupportedEncoding expEnc, SupportedEncoding actualEnc)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("JsonExpectedEncoding", new object[] { GetEncodingName(expEnc), GetEncodingName(actualEnc) })));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.encodingCode == SupportedEncoding.UTF8)
            {
                this.stream.Write(buffer, offset, count);
            }
            else
            {
                while (count > 0)
                {
                    int byteCount = (this.chars.Length < count) ? this.chars.Length : count;
                    int charCount = this.dec.GetChars(buffer, offset, byteCount, this.chars, 0, false);
                    this.byteCount = this.enc.GetBytes(this.chars, 0, charCount, this.bytes, 0, false);
                    this.stream.Write(this.bytes, 0, this.byteCount);
                    offset += byteCount;
                    count -= byteCount;
                }
            }
        }

        public override void WriteByte(byte b)
        {
            if (this.encodingCode == SupportedEncoding.UTF8)
            {
                this.stream.WriteByte(b);
            }
            else
            {
                this.byteBuffer[0] = b;
                this.Write(this.byteBuffer, 0, 1);
            }
        }

        public override bool CanRead
        {
            get
            {
                if (!this.isReading)
                {
                    return false;
                }
                return this.stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this.stream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (this.isReading)
                {
                    return false;
                }
                return this.stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this.stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return this.stream.ReadTimeout;
            }
            set
            {
                this.stream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return this.stream.WriteTimeout;
            }
            set
            {
                this.stream.WriteTimeout = value;
            }
        }

        private enum SupportedEncoding
        {
            UTF8,
            UTF16LE,
            UTF16BE,
            None
        }
    }
}


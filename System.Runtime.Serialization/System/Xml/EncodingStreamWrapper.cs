namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;

    internal class EncodingStreamWrapper : Stream
    {
        private const int BufferLength = 0x80;
        private byte[] byteBuffer;
        private int byteCount;
        private int byteOffset;
        private byte[] bytes;
        private char[] chars;
        private System.Text.Decoder dec;
        private System.Text.Encoder enc;
        private Encoding encoding;
        private static readonly byte[] encodingAttr = new byte[] { 0x65, 110, 0x63, 0x6f, 100, 0x69, 110, 0x67 };
        private SupportedEncoding encodingCode;
        private static readonly byte[] encodingUnicode = new byte[] { 0x75, 0x74, 0x66, 0x2d, 0x31, 0x36 };
        private static readonly byte[] encodingUnicodeBE = new byte[] { 0x75, 0x74, 0x66, 0x2d, 0x31, 0x36, 0x62, 0x65 };
        private static readonly byte[] encodingUnicodeLE = new byte[] { 0x75, 0x74, 0x66, 0x2d, 0x31, 0x36, 0x6c, 0x65 };
        private static readonly byte[] encodingUTF8 = new byte[] { 0x75, 0x74, 0x66, 0x2d, 0x38 };
        private bool isReading;
        private static readonly UnicodeEncoding SafeBEUTF16 = new UnicodeEncoding(true, false, false);
        private static readonly UnicodeEncoding SafeUTF16 = new UnicodeEncoding(false, false, false);
        private static readonly UTF8Encoding SafeUTF8 = new UTF8Encoding(false, false);
        private Stream stream;
        private static readonly UnicodeEncoding ValidatingBEUTF16 = new UnicodeEncoding(true, false, true);
        private static readonly UnicodeEncoding ValidatingUTF16 = new UnicodeEncoding(false, false, true);
        private static readonly UTF8Encoding ValidatingUTF8 = new UTF8Encoding(false, true);

        public EncodingStreamWrapper(Stream stream, Encoding encoding)
        {
            this.byteBuffer = new byte[1];
            try
            {
                this.isReading = true;
                this.stream = new BufferedStream(stream);
                SupportedEncoding supportedEncoding = GetSupportedEncoding(encoding);
                SupportedEncoding actualEnc = this.ReadBOMEncoding(encoding == null);
                if ((supportedEncoding != SupportedEncoding.None) && (supportedEncoding != actualEnc))
                {
                    ThrowExpectedEncodingMismatch(supportedEncoding, actualEnc);
                }
                if (actualEnc == SupportedEncoding.UTF8)
                {
                    this.FillBuffer(2);
                    if ((this.bytes[this.byteOffset + 1] == 0x3f) && (this.bytes[this.byteOffset] == 60))
                    {
                        this.FillBuffer(0x80);
                        CheckUTF8DeclarationEncoding(this.bytes, this.byteOffset, this.byteCount, actualEnc, supportedEncoding);
                    }
                }
                else
                {
                    this.EnsureBuffers();
                    this.FillBuffer(0xfe);
                    this.SetReadDocumentEncoding(actualEnc);
                    this.CleanupCharBreak();
                    int charCount = this.encoding.GetChars(this.bytes, this.byteOffset, this.byteCount, this.chars, 0);
                    this.byteOffset = 0;
                    this.byteCount = ValidatingUTF8.GetBytes(this.chars, 0, charCount, this.bytes, 0);
                    if ((this.bytes[1] == 0x3f) && (this.bytes[0] == 60))
                    {
                        CheckUTF8DeclarationEncoding(this.bytes, 0, this.byteCount, actualEnc, supportedEncoding);
                    }
                    else if (supportedEncoding == SupportedEncoding.None)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclarationRequired")));
                    }
                }
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidBytes"), exception));
            }
        }

        public EncodingStreamWrapper(Stream stream, Encoding encoding, bool emitBOM)
        {
            this.byteBuffer = new byte[1];
            this.isReading = false;
            this.encoding = encoding;
            this.stream = new BufferedStream(stream);
            this.encodingCode = GetSupportedEncoding(encoding);
            if (this.encodingCode != SupportedEncoding.UTF8)
            {
                this.EnsureBuffers();
                this.dec = ValidatingUTF8.GetDecoder();
                this.enc = this.encoding.GetEncoder();
                if (emitBOM)
                {
                    byte[] preamble = this.encoding.GetPreamble();
                    if (preamble.Length > 0)
                    {
                        this.stream.Write(preamble, 0, preamble.Length);
                    }
                }
            }
        }

        private static void CheckUTF8DeclarationEncoding(byte[] buffer, int offset, int count, SupportedEncoding e, SupportedEncoding expectedEnc)
        {
            byte num = 0;
            int num2 = -1;
            int num3 = offset + Math.Min(count, 0x80);
            int index = 0;
            int num5 = 0;
            for (index = offset + 2; index < num3; index++)
            {
                if (num == 0)
                {
                    if ((buffer[index] != 0x27) && (buffer[index] != 0x22))
                    {
                        goto Label_003E;
                    }
                    num = buffer[index];
                }
                else if (buffer[index] == num)
                {
                    num = 0;
                }
                continue;
            Label_003E:
                if (buffer[index] == 0x3d)
                {
                    if (num5 == 1)
                    {
                        num2 = index;
                        break;
                    }
                    num5++;
                }
                else if (buffer[index] == 0x3f)
                {
                    break;
                }
            }
            if (num2 == -1)
            {
                if ((e != SupportedEncoding.UTF8) && (expectedEnc == SupportedEncoding.None))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclarationRequired")));
                }
            }
            else
            {
                if (num2 < 0x1c)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
                }
                index = num2 - 1;
                while (IsWhitespace(buffer[index]))
                {
                    index--;
                }
                if (!Compare(encodingAttr, buffer, (index - encodingAttr.Length) + 1))
                {
                    if ((e != SupportedEncoding.UTF8) && (expectedEnc == SupportedEncoding.None))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclarationRequired")));
                    }
                }
                else
                {
                    index = num2 + 1;
                    while ((index < num3) && IsWhitespace(buffer[index]))
                    {
                        index++;
                    }
                    if ((buffer[index] != 0x27) && (buffer[index] != 0x22))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
                    }
                    num = buffer[index];
                    int num6 = index;
                    index = num6 + 1;
                    while ((buffer[index] != num) && (index < num3))
                    {
                        index++;
                    }
                    if (buffer[index] != num)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlMalformedDecl")));
                    }
                    int num7 = num6 + 1;
                    int num8 = index - num7;
                    SupportedEncoding encoding = e;
                    if ((num8 == encodingUTF8.Length) && CompareCaseInsensitive(encodingUTF8, buffer, num7))
                    {
                        encoding = SupportedEncoding.UTF8;
                    }
                    else if ((num8 == encodingUnicodeLE.Length) && CompareCaseInsensitive(encodingUnicodeLE, buffer, num7))
                    {
                        encoding = SupportedEncoding.UTF16LE;
                    }
                    else if ((num8 == encodingUnicodeBE.Length) && CompareCaseInsensitive(encodingUnicodeBE, buffer, num7))
                    {
                        encoding = SupportedEncoding.UTF16BE;
                    }
                    else if ((num8 == encodingUnicode.Length) && CompareCaseInsensitive(encodingUnicode, buffer, num7))
                    {
                        if (e == SupportedEncoding.UTF8)
                        {
                            ThrowEncodingMismatch(SafeUTF8.GetString(buffer, num7, num8), SafeUTF8.GetString(encodingUTF8, 0, encodingUTF8.Length));
                        }
                    }
                    else
                    {
                        ThrowEncodingMismatch(SafeUTF8.GetString(buffer, num7, num8), e);
                    }
                    if (e != encoding)
                    {
                        ThrowEncodingMismatch(SafeUTF8.GetString(buffer, num7, num8), e);
                    }
                }
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
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

        private static bool Compare(byte[] key, byte[] buffer, int offset)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != buffer[offset + i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CompareCaseInsensitive(byte[] key, byte[] buffer, int offset)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if ((key[i] != buffer[offset + i]) && (key[i] != char.ToLower((char) buffer[offset + i], CultureInfo.InvariantCulture)))
                {
                    return false;
                }
            }
            return true;
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
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlEncodingNotSupported")));
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
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlEncodingNotSupported")));
        }

        private static Encoding GetSafeEncoding(SupportedEncoding e)
        {
            switch (e)
            {
                case SupportedEncoding.UTF8:
                    return SafeUTF8;

                case SupportedEncoding.UTF16LE:
                    return SafeUTF16;

                case SupportedEncoding.UTF16BE:
                    return SafeBEUTF16;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlEncodingNotSupported")));
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlEncodingNotSupported")));
            }
            return SupportedEncoding.UTF16BE;
        }

        private static bool IsWhitespace(byte ch)
        {
            if (((ch != 0x20) && (ch != 10)) && (ch != 9))
            {
                return (ch == 13);
            }
            return true;
        }

        internal static ArraySegment<byte> ProcessBuffer(byte[] buffer, int offset, int count, Encoding encoding)
        {
            ArraySegment<byte> segment2;
            if (count < 4)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
            }
            try
            {
                int num;
                SupportedEncoding supportedEncoding = GetSupportedEncoding(encoding);
                SupportedEncoding actualEnc = ReadBOMEncoding(buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3], encoding == null, out num);
                if ((supportedEncoding != SupportedEncoding.None) && (supportedEncoding != actualEnc))
                {
                    ThrowExpectedEncodingMismatch(supportedEncoding, actualEnc);
                }
                offset += 4 - num;
                count -= 4 - num;
                if (actualEnc == SupportedEncoding.UTF8)
                {
                    if ((buffer[offset + 1] == 0x3f) && (buffer[offset] == 60))
                    {
                        CheckUTF8DeclarationEncoding(buffer, offset, count, actualEnc, supportedEncoding);
                    }
                    return new ArraySegment<byte>(buffer, offset, count);
                }
                Encoding safeEncoding = GetSafeEncoding(actualEnc);
                int byteCount = Math.Min(count, 0x100);
                char[] chars = new char[safeEncoding.GetMaxCharCount(byteCount)];
                int charCount = safeEncoding.GetChars(buffer, offset, byteCount, chars, 0);
                byte[] bytes = new byte[ValidatingUTF8.GetMaxByteCount(charCount)];
                int num4 = ValidatingUTF8.GetBytes(chars, 0, charCount, bytes, 0);
                if ((bytes[1] == 0x3f) && (bytes[0] == 60))
                {
                    CheckUTF8DeclarationEncoding(bytes, 0, num4, actualEnc, supportedEncoding);
                }
                else if (supportedEncoding == SupportedEncoding.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclarationRequired")));
                }
                segment2 = new ArraySegment<byte>(ValidatingUTF8.GetBytes(GetEncoding(actualEnc).GetChars(buffer, offset, count)));
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidBytes"), exception));
            }
            return segment2;
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidBytes"), exception));
            }
            return num2;
        }

        private SupportedEncoding ReadBOMEncoding(bool notOutOfBand)
        {
            int num5;
            int num = this.stream.ReadByte();
            int num2 = this.stream.ReadByte();
            int num3 = this.stream.ReadByte();
            int num4 = this.stream.ReadByte();
            if (num4 == -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("UnexpectedEndOfFile")));
            }
            SupportedEncoding encoding = ReadBOMEncoding((byte) num, (byte) num2, (byte) num3, (byte) num4, notOutOfBand, out num5);
            this.EnsureByteBuffer();
            switch (num5)
            {
                case 1:
                    this.bytes[0] = (byte) num4;
                    break;

                case 2:
                    this.bytes[0] = (byte) num3;
                    this.bytes[1] = (byte) num4;
                    break;

                case 4:
                    this.bytes[0] = (byte) num;
                    this.bytes[1] = (byte) num2;
                    this.bytes[2] = (byte) num3;
                    this.bytes[3] = (byte) num4;
                    break;
            }
            this.byteCount = num5;
            return encoding;
        }

        private static SupportedEncoding ReadBOMEncoding(byte b1, byte b2, byte b3, byte b4, bool notOutOfBand, out int preserve)
        {
            SupportedEncoding encoding = SupportedEncoding.UTF8;
            preserve = 0;
            if ((b1 == 60) && (b2 != 0))
            {
                encoding = SupportedEncoding.UTF8;
                preserve = 4;
                return encoding;
            }
            if ((b1 == 0xff) && (b2 == 0xfe))
            {
                encoding = SupportedEncoding.UTF16LE;
                preserve = 2;
                return encoding;
            }
            if ((b1 == 0xfe) && (b2 == 0xff))
            {
                encoding = SupportedEncoding.UTF16BE;
                preserve = 2;
                return encoding;
            }
            if ((b1 == 0) && (b2 == 60))
            {
                encoding = SupportedEncoding.UTF16BE;
                if (notOutOfBand && ((b3 != 0) || (b4 != 0x3f)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclMissing")));
                }
                preserve = 4;
                return encoding;
            }
            if ((b1 == 60) && (b2 == 0))
            {
                encoding = SupportedEncoding.UTF16LE;
                if (notOutOfBand && ((b3 != 0x3f) || (b4 != 0)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlDeclMissing")));
                }
                preserve = 4;
                return encoding;
            }
            if ((b1 == 0xef) && (b2 == 0xbb))
            {
                if (notOutOfBand && (b3 != 0xbf))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlBadBOM")));
                }
                preserve = 1;
                return encoding;
            }
            preserve = 4;
            return encoding;
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

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        private void SetReadDocumentEncoding(SupportedEncoding e)
        {
            this.EnsureBuffers();
            this.encodingCode = e;
            this.encoding = GetEncoding(e);
        }

        private static void ThrowEncodingMismatch(string declEnc, string docEnc)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlEncodingMismatch", new object[] { declEnc, docEnc })));
        }

        private static void ThrowEncodingMismatch(string declEnc, SupportedEncoding enc)
        {
            ThrowEncodingMismatch(declEnc, GetEncodingName(enc));
        }

        private static void ThrowExpectedEncodingMismatch(SupportedEncoding expEnc, SupportedEncoding actualEnc)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlExpectedEncoding", new object[] { GetEncodingName(expEnc), GetEncodingName(actualEnc) })));
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
                return this.stream.CanRead;
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


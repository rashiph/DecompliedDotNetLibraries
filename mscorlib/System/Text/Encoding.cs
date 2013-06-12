namespace System.Text
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public abstract class Encoding : ICloneable
    {
        private static Encoding asciiEncoding;
        private static Encoding bigEndianUnicode;
        private const int ChineseHZ = 0xcec8;
        internal const int CodePageASCII = 0x4e9f;
        private const int CodePageBigEndian = 0x4b1;
        private const int CodePageDefault = 0;
        private const int CodePageDLLKorean = 0x51d5;
        private const int CodePageGB2312 = 0x51c8;
        private const int CodePageMacGB2312 = 0x2718;
        private const int CodePageMacKorean = 0x2713;
        private const int CodePageNoMac = 2;
        private const int CodePageNoOEM = 1;
        private const int CodePageNoSymbol = 0x2a;
        private const int CodePageNoThread = 3;
        private const int CodePageUnicode = 0x4b0;
        private const int CodePageUTF32 = 0x2ee0;
        private const int CodePageUTF32BE = 0x2ee1;
        private const int CodePageUTF7 = 0xfde8;
        private const int CodePageUTF8 = 0xfde9;
        private const int CodePageWindows1252 = 0x4e4;
        internal CodePageDataItem dataItem;
        [OptionalField(VersionAdded=2)]
        internal System.Text.DecoderFallback decoderFallback;
        private static Encoding defaultEncoding;
        private const int DuplicateEUCCN = 0xcae0;
        internal static readonly byte[] emptyByteArray = new byte[0];
        private const int ENC50229 = 0xc435;
        [OptionalField(VersionAdded=2)]
        internal System.Text.EncoderFallback encoderFallback;
        private static Hashtable encodings;
        private const int EUCCN = 0x3a8;
        private const int EUCJP = 0xcadc;
        private const int EUCKR = 0xcaed;
        private const int GB18030 = 0xd698;
        private const int ISCIIAssemese = 0xdeae;
        private const int ISCIIBengali = 0xdeab;
        private const int ISCIIDevanagari = 0xdeaa;
        private const int ISCIIGujarathi = 0xdeb2;
        private const int ISCIIKannada = 0xdeb0;
        private const int ISCIIMalayalam = 0xdeb1;
        private const int ISCIIOriya = 0xdeaf;
        private const int ISCIIPanjabi = 0xdeb3;
        private const int ISCIITamil = 0xdeac;
        private const int ISCIITelugu = 0xdead;
        internal const int ISO_8859_1 = 0x6faf;
        private const int ISO_8859_8_Visual = 0x6fb6;
        private const int ISO_8859_8I = 0x96c6;
        private const int ISO2022JP = 0xc42c;
        private const int ISO2022JPESC = 0xc42d;
        private const int ISO2022JPSISO = 0xc42e;
        private const int ISOKorean = 0xc431;
        private const int ISOSimplifiedCN = 0xc433;
        private static Encoding latin1Encoding;
        internal int m_codePage;
        [NonSerialized]
        internal bool m_deserializedFromEverett;
        [OptionalField(VersionAdded=2)]
        private bool m_isReadOnly;
        private const int MIMECONTF_BROWSER = 2;
        private const int MIMECONTF_MAILNEWS = 1;
        private const int MIMECONTF_SAVABLE_BROWSER = 0x200;
        private const int MIMECONTF_SAVABLE_MAILNEWS = 0x100;
        private static object s_InternalSyncObject;
        private static Encoding unicodeEncoding;
        private static Encoding utf32Encoding;
        private static Encoding utf7Encoding;
        private static Encoding utf8Encoding;

        protected Encoding() : this(0)
        {
        }

        [SecuritySafeCritical]
        protected Encoding(int codePage)
        {
            this.m_isReadOnly = true;
            if (codePage < 0)
            {
                throw new ArgumentOutOfRangeException("codePage");
            }
            this.m_codePage = codePage;
            this.SetDefaultFallbacks();
        }

        [ComVisible(false), SecuritySafeCritical]
        public virtual object Clone()
        {
            Encoding encoding = (Encoding) base.MemberwiseClone();
            encoding.m_isReadOnly = false;
            return encoding;
        }

        public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            return Convert(srcEncoding, dstEncoding, bytes, 0, bytes.Length);
        }

        public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes, int index, int count)
        {
            if ((srcEncoding == null) || (dstEncoding == null))
            {
                throw new ArgumentNullException((srcEncoding == null) ? "srcEncoding" : "dstEncoding", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            return dstEncoding.GetBytes(srcEncoding.GetChars(bytes, index, count));
        }

        [SecurityCritical]
        private static Encoding CreateDefaultEncoding()
        {
            int aCP = Win32Native.GetACP();
            if (aCP == 0x4e4)
            {
                return new SBCSCodePageEncoding(aCP);
            }
            return GetEncoding(aCP);
        }

        internal void DeserializeEncoding(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_codePage = (int) info.GetValue("m_codePage", typeof(int));
            this.dataItem = null;
            try
            {
                this.m_isReadOnly = (bool) info.GetValue("m_isReadOnly", typeof(bool));
                this.encoderFallback = (System.Text.EncoderFallback) info.GetValue("encoderFallback", typeof(System.Text.EncoderFallback));
                this.decoderFallback = (System.Text.DecoderFallback) info.GetValue("decoderFallback", typeof(System.Text.DecoderFallback));
            }
            catch (SerializationException)
            {
                this.m_deserializedFromEverett = true;
                this.m_isReadOnly = true;
                this.SetDefaultFallbacks();
            }
        }

        public override bool Equals(object value)
        {
            Encoding encoding = value as Encoding;
            if (encoding == null)
            {
                return false;
            }
            return (((this.m_codePage == encoding.m_codePage) && this.EncoderFallback.Equals(encoding.EncoderFallback)) && this.DecoderFallback.Equals(encoding.DecoderFallback));
        }

        internal virtual char[] GetBestFitBytesToUnicodeData()
        {
            return new char[0];
        }

        internal virtual char[] GetBestFitUnicodeToBytesData()
        {
            return new char[0];
        }

        public virtual int GetByteCount(char[] chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            return this.GetByteCount(chars, 0, chars.Length);
        }

        public virtual int GetByteCount(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            char[] chars = s.ToCharArray();
            return this.GetByteCount(chars, 0, chars.Length);
        }

        [ComVisible(false), SecurityCritical, CLSCompliant(false)]
        public virtual unsafe int GetByteCount(char* chars, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            char[] chArray = new char[count];
            for (int i = 0; i < count; i++)
            {
                chArray[i] = chars[i];
            }
            return this.GetByteCount(chArray, 0, count);
        }

        public abstract int GetByteCount(char[] chars, int index, int count);
        [SecurityCritical]
        internal virtual unsafe int GetByteCount(char* chars, int count, EncoderNLS encoder)
        {
            return this.GetByteCount(chars, count);
        }

        public virtual byte[] GetBytes(char[] chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            return this.GetBytes(chars, 0, chars.Length);
        }

        public virtual byte[] GetBytes(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s", Environment.GetResourceString("ArgumentNull_String"));
            }
            char[] chars = s.ToCharArray();
            return this.GetBytes(chars, 0, chars.Length);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual byte[] GetBytes(char[] chars, int index, int count)
        {
            byte[] bytes = new byte[this.GetByteCount(chars, index, count)];
            this.GetBytes(chars, index, count, bytes, 0);
            return bytes;
        }

        [SecurityCritical, CLSCompliant(false), ComVisible(false)]
        public virtual unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
        {
            int num;
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charCount < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            char[] chArray = new char[charCount];
            for (num = 0; num < charCount; num++)
            {
                chArray[num] = chars[num];
            }
            byte[] buffer = new byte[byteCount];
            int num2 = this.GetBytes(chArray, 0, charCount, buffer, 0);
            if (num2 < byteCount)
            {
                byteCount = num2;
            }
            for (num = 0; num < byteCount; num++)
            {
                bytes[num] = buffer[num];
            }
            return byteCount;
        }

        public abstract int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex);
        public virtual int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return this.GetBytes(s.ToCharArray(), charIndex, charCount, bytes, byteIndex);
        }

        [SecurityCritical]
        internal virtual unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
        {
            return this.GetBytes(chars, charCount, bytes, byteCount);
        }

        public virtual int GetCharCount(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            return this.GetCharCount(bytes, 0, bytes.Length);
        }

        [ComVisible(false), SecurityCritical, CLSCompliant(false)]
        public virtual unsafe int GetCharCount(byte* bytes, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            byte[] buffer = new byte[count];
            for (int i = 0; i < count; i++)
            {
                buffer[i] = bytes[i];
            }
            return this.GetCharCount(buffer, 0, count);
        }

        public abstract int GetCharCount(byte[] bytes, int index, int count);
        [SecurityCritical]
        internal virtual unsafe int GetCharCount(byte* bytes, int count, DecoderNLS decoder)
        {
            return this.GetCharCount(bytes, count);
        }

        public virtual char[] GetChars(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            return this.GetChars(bytes, 0, bytes.Length);
        }

        public virtual char[] GetChars(byte[] bytes, int index, int count)
        {
            char[] chars = new char[this.GetCharCount(bytes, index, count)];
            this.GetChars(bytes, index, count, chars, 0);
            return chars;
        }

        [ComVisible(false), CLSCompliant(false), SecurityCritical]
        public virtual unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            int num;
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteCount < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            byte[] buffer = new byte[byteCount];
            for (num = 0; num < byteCount; num++)
            {
                buffer[num] = bytes[num];
            }
            char[] chArray = new char[charCount];
            int num2 = this.GetChars(buffer, 0, byteCount, chArray, 0);
            if (num2 < charCount)
            {
                charCount = num2;
            }
            for (num = 0; num < charCount; num++)
            {
                chars[num] = chArray[num];
            }
            return charCount;
        }

        public abstract int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);
        [SecurityCritical]
        internal virtual unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS decoder)
        {
            return this.GetChars(bytes, byteCount, chars, charCount);
        }

        private void GetDataItem()
        {
            if (this.dataItem == null)
            {
                this.dataItem = EncodingTable.GetCodePageDataItem(this.m_codePage);
                if (this.dataItem == null)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoCodepageData", new object[] { this.m_codePage }));
                }
            }
        }

        public virtual System.Text.Decoder GetDecoder()
        {
            return new DefaultDecoder(this);
        }

        public virtual System.Text.Encoder GetEncoder()
        {
            return new DefaultEncoder(this);
        }

        [SecuritySafeCritical]
        public static Encoding GetEncoding(int codepage)
        {
            if ((codepage < 0) || (codepage > 0xffff))
            {
                throw new ArgumentOutOfRangeException("codepage", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0, 0xffff }));
            }
            Encoding unicode = null;
            if (encodings != null)
            {
                unicode = (Encoding) encodings[codepage];
            }
            if (unicode == null)
            {
                lock (InternalSyncObject)
                {
                    if (encodings == null)
                    {
                        encodings = new Hashtable();
                    }
                    unicode = (Encoding) encodings[codepage];
                    if (unicode != null)
                    {
                        return unicode;
                    }
                    switch (codepage)
                    {
                        case 0:
                            unicode = Default;
                            break;

                        case 1:
                        case 2:
                        case 3:
                        case 0x2a:
                            throw new ArgumentException(Environment.GetResourceString("Argument_CodepageNotSupported", new object[] { codepage }), "codepage");

                        case 0x4b0:
                            unicode = Unicode;
                            break;

                        case 0x4b1:
                            unicode = BigEndianUnicode;
                            break;

                        case 0x6faf:
                            unicode = Latin1;
                            break;

                        case 0xfde9:
                            unicode = UTF8;
                            break;

                        case 0x4e4:
                            unicode = new SBCSCodePageEncoding(codepage);
                            break;

                        case 0x4e9f:
                            unicode = ASCII;
                            break;

                        default:
                            unicode = GetEncodingCodePage(codepage);
                            if (unicode == null)
                            {
                                unicode = GetEncodingRare(codepage);
                            }
                            break;
                    }
                    encodings.Add(codepage, unicode);
                }
            }
            return unicode;
        }

        [SecuritySafeCritical]
        public static Encoding GetEncoding(string name)
        {
            return GetEncoding(EncodingTable.GetCodePageFromName(name));
        }

        [SecuritySafeCritical]
        public static Encoding GetEncoding(int codepage, System.Text.EncoderFallback encoderFallback, System.Text.DecoderFallback decoderFallback)
        {
            Encoding encoding2 = (Encoding) GetEncoding(codepage).Clone();
            encoding2.EncoderFallback = encoderFallback;
            encoding2.DecoderFallback = decoderFallback;
            return encoding2;
        }

        [SecuritySafeCritical]
        public static Encoding GetEncoding(string name, System.Text.EncoderFallback encoderFallback, System.Text.DecoderFallback decoderFallback)
        {
            return GetEncoding(EncodingTable.GetCodePageFromName(name), encoderFallback, decoderFallback);
        }

        [SecurityCritical]
        private static Encoding GetEncodingCodePage(int CodePage)
        {
            switch (BaseCodePageEncoding.GetCodePageByteSize(CodePage))
            {
                case 1:
                    return new SBCSCodePageEncoding(CodePage);

                case 2:
                    return new DBCSCodePageEncoding(CodePage);
            }
            return null;
        }

        [SecurityCritical]
        private static Encoding GetEncodingRare(int codepage)
        {
            int num = codepage;
            if (num <= 0xcadc)
            {
                switch (num)
                {
                    case 0x2ee0:
                        return UTF32;

                    case 0x2ee1:
                        return new UTF32Encoding(true, true);

                    case 0x2718:
                        return new DBCSCodePageEncoding(0x2718, 0x51c8);

                    case 0x2713:
                        return new DBCSCodePageEncoding(0x2713, 0x51d5);

                    case 0xc42c:
                    case 0xc42d:
                    case 0xc42e:
                    case 0xc431:
                        goto Label_0169;

                    case 0xc433:
                        goto Label_0172;

                    case 0xc435:
                        throw new NotSupportedException(Environment.GetResourceString("NotSupported_CodePage50229"));

                    case 0xcadc:
                        return new EUCJPEncoding();

                    case 0x96c6:
                        return new SBCSCodePageEncoding(codepage, 0x6fb6);
                }
                goto Label_01B4;
            }
            if (num <= 0xcec8)
            {
                switch (num)
                {
                    case 0xcae0:
                        goto Label_0172;

                    case 0xcaed:
                        return new DBCSCodePageEncoding(codepage, 0x51d5);

                    case 0xcec8:
                        goto Label_0169;
                }
                goto Label_01B4;
            }
            switch (num)
            {
                case 0xdeaa:
                case 0xdeab:
                case 0xdeac:
                case 0xdead:
                case 0xdeae:
                case 0xdeaf:
                case 0xdeb0:
                case 0xdeb1:
                case 0xdeb2:
                case 0xdeb3:
                    return new ISCIIEncoding(codepage);

                case 0xd698:
                    return new GB18030Encoding();

                default:
                    if (num != 0xfde8)
                    {
                        goto Label_01B4;
                    }
                    return UTF7;
            }
        Label_0169:
            return new ISO2022Encoding(codepage);
        Label_0172:
            return new DBCSCodePageEncoding(codepage, 0x3a8);
        Label_01B4:;
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoCodepageData", new object[] { codepage }));
        }

        public static EncodingInfo[] GetEncodings()
        {
            return EncodingTable.GetEncodings();
        }

        public override int GetHashCode()
        {
            return ((this.m_codePage + this.EncoderFallback.GetHashCode()) + this.DecoderFallback.GetHashCode());
        }

        public abstract int GetMaxByteCount(int charCount);
        public abstract int GetMaxCharCount(int byteCount);
        public virtual byte[] GetPreamble()
        {
            return emptyByteArray;
        }

        public virtual string GetString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            return this.GetString(bytes, 0, bytes.Length);
        }

        public virtual string GetString(byte[] bytes, int index, int count)
        {
            return new string(this.GetChars(bytes, index, count));
        }

        [ComVisible(false)]
        public bool IsAlwaysNormalized()
        {
            return this.IsAlwaysNormalized(NormalizationForm.FormC);
        }

        [ComVisible(false)]
        public virtual bool IsAlwaysNormalized(NormalizationForm form)
        {
            return false;
        }

        internal void OnDeserialized()
        {
            if ((this.encoderFallback == null) || (this.decoderFallback == null))
            {
                this.m_deserializedFromEverett = true;
                this.SetDefaultFallbacks();
            }
            this.dataItem = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.OnDeserialized();
        }

        internal void OnDeserializing()
        {
            this.encoderFallback = null;
            this.decoderFallback = null;
            this.m_isReadOnly = true;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.OnDeserializing();
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this.dataItem = null;
        }

        internal void SerializeEncoding(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("m_isReadOnly", this.m_isReadOnly);
            info.AddValue("encoderFallback", this.EncoderFallback);
            info.AddValue("decoderFallback", this.DecoderFallback);
            info.AddValue("m_codePage", this.m_codePage);
            info.AddValue("dataItem", null);
            info.AddValue("Encoding+m_codePage", this.m_codePage);
            info.AddValue("Encoding+dataItem", null);
        }

        internal virtual void SetDefaultFallbacks()
        {
            this.encoderFallback = new InternalEncoderBestFitFallback(this);
            this.decoderFallback = new InternalDecoderBestFitFallback(this);
        }

        internal void ThrowBytesOverflow()
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_EncodingConversionOverflowBytes", new object[] { this.EncodingName, this.EncoderFallback.GetType() }), "bytes");
        }

        [SecurityCritical]
        internal void ThrowBytesOverflow(EncoderNLS encoder, bool nothingEncoded)
        {
            if (((encoder == null) || encoder.m_throwOnOverflow) || nothingEncoded)
            {
                if ((encoder != null) && encoder.InternalHasFallbackBuffer)
                {
                    encoder.FallbackBuffer.InternalReset();
                }
                this.ThrowBytesOverflow();
            }
            encoder.ClearMustFlush();
        }

        internal void ThrowCharsOverflow()
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_EncodingConversionOverflowChars", new object[] { this.EncodingName, this.DecoderFallback.GetType() }), "chars");
        }

        [SecurityCritical]
        internal void ThrowCharsOverflow(DecoderNLS decoder, bool nothingDecoded)
        {
            if (((decoder == null) || decoder.m_throwOnOverflow) || nothingDecoded)
            {
                if ((decoder != null) && decoder.InternalHasFallbackBuffer)
                {
                    decoder.FallbackBuffer.InternalReset();
                }
                this.ThrowCharsOverflow();
            }
            decoder.ClearMustFlush();
        }

        public static Encoding ASCII
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (asciiEncoding == null)
                {
                    asciiEncoding = new ASCIIEncoding();
                }
                return asciiEncoding;
            }
        }

        public static Encoding BigEndianUnicode
        {
            get
            {
                if (bigEndianUnicode == null)
                {
                    bigEndianUnicode = new UnicodeEncoding(true, true);
                }
                return bigEndianUnicode;
            }
        }

        public virtual string BodyName
        {
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return this.dataItem.BodyName;
            }
        }

        public virtual int CodePage
        {
            get
            {
                return this.m_codePage;
            }
        }

        [ComVisible(false)]
        public System.Text.DecoderFallback DecoderFallback
        {
            get
            {
                return this.decoderFallback;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.decoderFallback = value;
            }
        }

        public static Encoding Default
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
            get
            {
                if (defaultEncoding == null)
                {
                    defaultEncoding = CreateDefaultEncoding();
                }
                return defaultEncoding;
            }
        }

        [ComVisible(false)]
        public System.Text.EncoderFallback EncoderFallback
        {
            get
            {
                return this.encoderFallback;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.encoderFallback = value;
            }
        }

        public virtual string EncodingName
        {
            get
            {
                return Environment.GetResourceString("Globalization.cp_" + this.m_codePage);
            }
        }

        public virtual string HeaderName
        {
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return this.dataItem.HeaderName;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        public virtual bool IsBrowserDisplay
        {
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return ((this.dataItem.Flags & 2) != 0);
            }
        }

        public virtual bool IsBrowserSave
        {
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return ((this.dataItem.Flags & 0x200) != 0);
            }
        }

        public virtual bool IsMailNewsDisplay
        {
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return ((this.dataItem.Flags & 1) != 0);
            }
        }

        public virtual bool IsMailNewsSave
        {
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return ((this.dataItem.Flags & 0x100) != 0);
            }
        }

        [ComVisible(false)]
        public bool IsReadOnly
        {
            get
            {
                return this.m_isReadOnly;
            }
        }

        [ComVisible(false)]
        public virtual bool IsSingleByte
        {
            get
            {
                return false;
            }
        }

        private static Encoding Latin1
        {
            get
            {
                if (latin1Encoding == null)
                {
                    latin1Encoding = new Latin1Encoding();
                }
                return latin1Encoding;
            }
        }

        public static Encoding Unicode
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (unicodeEncoding == null)
                {
                    unicodeEncoding = new UnicodeEncoding(false, true);
                }
                return unicodeEncoding;
            }
        }

        public static Encoding UTF32
        {
            get
            {
                if (utf32Encoding == null)
                {
                    utf32Encoding = new UTF32Encoding(false, true);
                }
                return utf32Encoding;
            }
        }

        public static Encoding UTF7
        {
            get
            {
                if (utf7Encoding == null)
                {
                    utf7Encoding = new UTF7Encoding();
                }
                return utf7Encoding;
            }
        }

        public static Encoding UTF8
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (utf8Encoding == null)
                {
                    utf8Encoding = new UTF8Encoding(true);
                }
                return utf8Encoding;
            }
        }

        public virtual string WebName
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return this.dataItem.WebName;
            }
        }

        public virtual int WindowsCodePage
        {
            get
            {
                if (this.dataItem == null)
                {
                    this.GetDataItem();
                }
                return this.dataItem.UIFamilyCodePage;
            }
        }

        [Serializable]
        internal class DefaultDecoder : System.Text.Decoder, ISerializable, IObjectReference
        {
            private Encoding m_encoding;
            [NonSerialized]
            private bool m_hasInitializedEncoding;

            public DefaultDecoder(Encoding encoding)
            {
                this.m_encoding = encoding;
                this.m_hasInitializedEncoding = true;
            }

            internal DefaultDecoder(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.m_encoding = (Encoding) info.GetValue("encoding", typeof(Encoding));
                try
                {
                    base.m_fallback = (DecoderFallback) info.GetValue("m_fallback", typeof(DecoderFallback));
                }
                catch (SerializationException)
                {
                    base.m_fallback = null;
                }
            }

            [SecurityCritical]
            public override unsafe int GetCharCount(byte* bytes, int count, bool flush)
            {
                return this.m_encoding.GetCharCount(bytes, count);
            }

            public override int GetCharCount(byte[] bytes, int index, int count)
            {
                return this.GetCharCount(bytes, index, count, false);
            }

            public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
            {
                return this.m_encoding.GetCharCount(bytes, index, count);
            }

            [SecurityCritical]
            public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
            {
                return this.m_encoding.GetChars(bytes, byteCount, chars, charCount);
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            {
                return this.GetChars(bytes, byteIndex, byteCount, chars, charIndex, false);
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
            {
                return this.m_encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            }

            [SecurityCritical]
            public object GetRealObject(StreamingContext context)
            {
                if (this.m_hasInitializedEncoding)
                {
                    return this;
                }
                System.Text.Decoder decoder = this.m_encoding.GetDecoder();
                if (base.m_fallback != null)
                {
                    decoder.m_fallback = base.m_fallback;
                }
                return decoder;
            }

            [SecurityCritical]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                info.AddValue("encoding", this.m_encoding);
            }
        }

        [Serializable]
        internal class DefaultEncoder : System.Text.Encoder, ISerializable, IObjectReference
        {
            [NonSerialized]
            internal char charLeftOver;
            private Encoding m_encoding;
            [NonSerialized]
            private bool m_hasInitializedEncoding;

            public DefaultEncoder(Encoding encoding)
            {
                this.m_encoding = encoding;
                this.m_hasInitializedEncoding = true;
            }

            internal DefaultEncoder(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.m_encoding = (Encoding) info.GetValue("encoding", typeof(Encoding));
                try
                {
                    base.m_fallback = (EncoderFallback) info.GetValue("m_fallback", typeof(EncoderFallback));
                    this.charLeftOver = (char) info.GetValue("charLeftOver", typeof(char));
                }
                catch (SerializationException)
                {
                }
            }

            [SecurityCritical]
            public override unsafe int GetByteCount(char* chars, int count, bool flush)
            {
                return this.m_encoding.GetByteCount(chars, count);
            }

            public override int GetByteCount(char[] chars, int index, int count, bool flush)
            {
                return this.m_encoding.GetByteCount(chars, index, count);
            }

            [SecurityCritical]
            public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
            {
                return this.m_encoding.GetBytes(chars, charCount, bytes, byteCount);
            }

            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
            {
                return this.m_encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            }

            [SecurityCritical]
            public object GetRealObject(StreamingContext context)
            {
                if (this.m_hasInitializedEncoding)
                {
                    return this;
                }
                System.Text.Encoder encoder = this.m_encoding.GetEncoder();
                if (base.m_fallback != null)
                {
                    encoder.m_fallback = base.m_fallback;
                }
                if (this.charLeftOver != '\0')
                {
                    EncoderNLS rnls = encoder as EncoderNLS;
                    if (rnls != null)
                    {
                        rnls.charLeftOver = this.charLeftOver;
                    }
                }
                return encoder;
            }

            [SecurityCritical]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                info.AddValue("encoding", this.m_encoding);
            }
        }

        internal class EncodingByteBuffer
        {
            private int byteCountResult;
            private unsafe byte* byteEnd;
            private unsafe byte* bytes;
            private unsafe byte* byteStart;
            private unsafe char* charEnd;
            private unsafe char* chars;
            private unsafe char* charStart;
            private Encoding enc;
            private EncoderNLS encoder;
            internal EncoderFallbackBuffer fallbackBuffer;

            [SecurityCritical]
            internal unsafe EncodingByteBuffer(Encoding inEncoding, EncoderNLS inEncoder, byte* inByteStart, int inByteCount, char* inCharStart, int inCharCount)
            {
                this.enc = inEncoding;
                this.encoder = inEncoder;
                this.charStart = inCharStart;
                this.chars = inCharStart;
                this.charEnd = inCharStart + inCharCount;
                this.bytes = inByteStart;
                this.byteStart = inByteStart;
                this.byteEnd = inByteStart + inByteCount;
                if (this.encoder == null)
                {
                    this.fallbackBuffer = this.enc.EncoderFallback.CreateFallbackBuffer();
                }
                else
                {
                    this.fallbackBuffer = this.encoder.FallbackBuffer;
                    if ((this.encoder.m_throwOnOverflow && this.encoder.InternalHasFallbackBuffer) && (this.fallbackBuffer.Remaining > 0))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.encoder.Encoding.EncodingName, this.encoder.Fallback.GetType() }));
                    }
                }
                this.fallbackBuffer.InternalInitialize(this.chars, this.charEnd, this.encoder, this.bytes != null);
            }

            [SecurityCritical]
            internal bool AddByte(byte b1)
            {
                return this.AddByte(b1, 0);
            }

            [SecurityCritical]
            internal bool AddByte(byte b1, byte b2)
            {
                return this.AddByte(b1, b2, 0);
            }

            [SecurityCritical]
            internal unsafe bool AddByte(byte b, int moreBytesExpected)
            {
                if (this.bytes != null)
                {
                    byte* numPtr;
                    if (this.bytes >= (this.byteEnd - moreBytesExpected))
                    {
                        this.MovePrevious(true);
                        return false;
                    }
                    this.bytes = (numPtr = this.bytes) + 1;
                    numPtr[0] = b;
                }
                this.byteCountResult++;
                return true;
            }

            [SecurityCritical]
            internal bool AddByte(byte b1, byte b2, byte b3)
            {
                return this.AddByte(b1, b2, b3, 0);
            }

            [SecurityCritical]
            internal bool AddByte(byte b1, byte b2, int moreBytesExpected)
            {
                return (this.AddByte(b1, (int) (1 + moreBytesExpected)) && this.AddByte(b2, moreBytesExpected));
            }

            [SecurityCritical]
            internal bool AddByte(byte b1, byte b2, byte b3, byte b4)
            {
                return (((this.AddByte(b1, 3) && this.AddByte(b2, 2)) && this.AddByte(b3, 1)) && this.AddByte(b4, 0));
            }

            [SecurityCritical]
            internal bool AddByte(byte b1, byte b2, byte b3, int moreBytesExpected)
            {
                return ((this.AddByte(b1, (int) (2 + moreBytesExpected)) && this.AddByte(b2, (int) (1 + moreBytesExpected))) && this.AddByte(b3, moreBytesExpected));
            }

            [SecurityCritical]
            internal unsafe bool Fallback(char charFallback)
            {
                return this.fallbackBuffer.InternalFallback(charFallback, ref this.chars);
            }

            [SecurityCritical]
            internal unsafe char GetNextChar()
            {
                char nextChar = this.fallbackBuffer.InternalGetNextChar();
                if ((nextChar == '\0') && (this.chars < this.charEnd))
                {
                    char* chPtr;
                    this.chars = (chPtr = this.chars) + 1;
                    nextChar = chPtr[0];
                }
                return nextChar;
            }

            [SecurityCritical]
            internal unsafe void MovePrevious(bool bThrow)
            {
                if (this.fallbackBuffer.bFallingBack)
                {
                    this.fallbackBuffer.MovePrevious();
                }
                else if (this.chars > this.charStart)
                {
                    this.chars--;
                }
                if (bThrow)
                {
                    this.enc.ThrowBytesOverflow(this.encoder, this.bytes == this.byteStart);
                }
            }

            internal int CharsUsed
            {
                [SecurityCritical]
                get
                {
                    return (int) ((long) ((this.chars - this.charStart) / 2));
                }
            }

            internal int Count
            {
                get
                {
                    return this.byteCountResult;
                }
            }

            internal bool MoreData
            {
                [SecurityCritical]
                get
                {
                    if (this.fallbackBuffer.Remaining <= 0)
                    {
                        return (this.chars < this.charEnd);
                    }
                    return true;
                }
            }
        }

        internal class EncodingCharBuffer
        {
            private unsafe byte* byteEnd;
            private unsafe byte* bytes;
            private unsafe byte* byteStart;
            private int charCountResult;
            private unsafe char* charEnd;
            private unsafe char* chars;
            private unsafe char* charStart;
            private DecoderNLS decoder;
            private Encoding enc;
            private DecoderFallbackBuffer fallbackBuffer;

            [SecurityCritical]
            internal unsafe EncodingCharBuffer(Encoding enc, DecoderNLS decoder, char* charStart, int charCount, byte* byteStart, int byteCount)
            {
                this.enc = enc;
                this.decoder = decoder;
                this.chars = charStart;
                this.charStart = charStart;
                this.charEnd = charStart + charCount;
                this.byteStart = byteStart;
                this.bytes = byteStart;
                this.byteEnd = byteStart + byteCount;
                if (this.decoder == null)
                {
                    this.fallbackBuffer = enc.DecoderFallback.CreateFallbackBuffer();
                }
                else
                {
                    this.fallbackBuffer = this.decoder.FallbackBuffer;
                }
                this.fallbackBuffer.InternalInitialize(this.bytes, this.charEnd);
            }

            [SecurityCritical]
            internal bool AddChar(char ch)
            {
                return this.AddChar(ch, 1);
            }

            [SecurityCritical]
            internal unsafe bool AddChar(char ch, int numBytes)
            {
                if (this.chars != null)
                {
                    char* chPtr;
                    if (this.chars >= this.charEnd)
                    {
                        this.bytes -= numBytes;
                        this.enc.ThrowCharsOverflow(this.decoder, this.bytes <= this.byteStart);
                        return false;
                    }
                    this.chars = (chPtr = this.chars) + 1;
                    chPtr[0] = ch;
                }
                this.charCountResult++;
                return true;
            }

            [SecurityCritical]
            internal unsafe bool AddChar(char ch1, char ch2, int numBytes)
            {
                if (this.chars >= (this.charEnd - 1))
                {
                    this.bytes -= numBytes;
                    this.enc.ThrowCharsOverflow(this.decoder, this.bytes <= this.byteStart);
                    return false;
                }
                return (this.AddChar(ch1, numBytes) && this.AddChar(ch2, numBytes));
            }

            [SecurityCritical]
            internal unsafe void AdjustBytes(int count)
            {
                this.bytes += count;
            }

            [SecurityCritical]
            internal unsafe bool EvenMoreData(int count)
            {
                return (this.bytes <= (this.byteEnd - count));
            }

            [SecurityCritical]
            internal bool Fallback(byte fallbackByte)
            {
                byte[] byteBuffer = new byte[] { fallbackByte };
                return this.Fallback(byteBuffer);
            }

            [SecurityCritical]
            internal unsafe bool Fallback(byte[] byteBuffer)
            {
                if (this.chars != null)
                {
                    char* chars = this.chars;
                    if (!this.fallbackBuffer.InternalFallback(byteBuffer, this.bytes, ref this.chars))
                    {
                        this.bytes -= byteBuffer.Length;
                        this.fallbackBuffer.InternalReset();
                        this.enc.ThrowCharsOverflow(this.decoder, this.chars == this.charStart);
                        return false;
                    }
                    this.charCountResult += (int) ((long) ((this.chars - chars) / 2));
                }
                else
                {
                    this.charCountResult += this.fallbackBuffer.InternalFallback(byteBuffer, this.bytes);
                }
                return true;
            }

            [SecurityCritical]
            internal bool Fallback(byte byte1, byte byte2)
            {
                byte[] byteBuffer = new byte[] { byte1, byte2 };
                return this.Fallback(byteBuffer);
            }

            [SecurityCritical]
            internal bool Fallback(byte byte1, byte byte2, byte byte3, byte byte4)
            {
                byte[] byteBuffer = new byte[] { byte1, byte2, byte3, byte4 };
                return this.Fallback(byteBuffer);
            }

            [SecurityCritical]
            internal unsafe byte GetNextByte()
            {
                byte* numPtr;
                if (this.bytes >= this.byteEnd)
                {
                    return 0;
                }
                this.bytes = (numPtr = this.bytes) + 1;
                return numPtr[0];
            }

            internal int BytesUsed
            {
                [SecurityCritical]
                get
                {
                    return (int) ((long) ((this.bytes - this.byteStart) / 1));
                }
            }

            internal int Count
            {
                get
                {
                    return this.charCountResult;
                }
            }

            internal bool MoreData
            {
                [SecurityCritical]
                get
                {
                    return (this.bytes < this.byteEnd);
                }
            }
        }
    }
}


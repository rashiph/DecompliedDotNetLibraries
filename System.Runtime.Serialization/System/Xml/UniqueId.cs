namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    public class UniqueId
    {
        private static short[] char2val = new short[] { 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0, 0x10, 0x20, 0x30, 0x40, 80, 0x60, 0x70, 0x80, 0x90, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 160, 0xb0, 0xc0, 0xd0, 0xe0, 240, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 10, 11, 12, 13, 14, 15, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 
            0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100, 0x100
         };
        private const int guidLength = 0x10;
        private long idHigh;
        private long idLow;
        [SecurityCritical]
        private string s;
        private const int uuidLength = 0x2d;
        private const string val2char = "0123456789abcdef";

        public UniqueId() : this(Guid.NewGuid())
        {
        }

        public UniqueId(Guid guid) : this(guid.ToByteArray())
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UniqueId(byte[] guid) : this(guid, 0)
        {
        }

        [SecuritySafeCritical]
        public unsafe UniqueId(string value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            if (value.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidUniqueId")));
            }
            fixed (char* str = ((char*) value))
            {
                char* chars = str;
                this.UnsafeParse(chars, value.Length);
            }
            this.s = value;
        }

        [SecuritySafeCritical]
        public unsafe UniqueId(byte[] guid, int offset)
        {
            if (guid == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("guid"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > guid.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { guid.Length })));
            }
            if (0x10 > (guid.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlArrayTooSmallInput", new object[] { 0x10 }), "guid"));
            }
            fixed (byte* numRef = &(guid[offset]))
            {
                this.idLow = this.UnsafeGetInt64(numRef);
                this.idHigh = this.UnsafeGetInt64(numRef + 8);
            }
        }

        [SecuritySafeCritical]
        public unsafe UniqueId(char[] chars, int offset, int count)
        {
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > chars.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { chars.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (chars.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { chars.Length - offset })));
            }
            if (count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidUniqueId")));
            }
            fixed (char* chRef = &(chars[offset]))
            {
                this.UnsafeParse(chRef, count);
            }
            if (!this.IsGuid)
            {
                this.s = new string(chars, offset, count);
            }
        }

        public override bool Equals(object obj)
        {
            return (this == (obj as UniqueId));
        }

        public override int GetHashCode()
        {
            if (this.IsGuid)
            {
                long num = this.idLow ^ this.idHigh;
                return (((int) (num >> 0x20)) ^ ((int) num));
            }
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(UniqueId id1, UniqueId id2)
        {
            if (object.ReferenceEquals(id1, null) && object.ReferenceEquals(id2, null))
            {
                return true;
            }
            if (object.ReferenceEquals(id1, null) || object.ReferenceEquals(id2, null))
            {
                return false;
            }
            if (!id1.IsGuid || !id2.IsGuid)
            {
                return (id1.ToString() == id2.ToString());
            }
            return ((id1.idLow == id2.idLow) && (id1.idHigh == id2.idHigh));
        }

        public static bool operator !=(UniqueId id1, UniqueId id2)
        {
            return !(id1 == id2);
        }

        [SecuritySafeCritical]
        public unsafe int ToCharArray(char[] chars, int offset)
        {
            int charArrayLength = this.CharArrayLength;
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > chars.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { chars.Length })));
            }
            if (charArrayLength > (chars.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("chars", System.Runtime.Serialization.SR.GetString("XmlArrayTooSmallOutput", new object[] { charArrayLength })));
            }
            if (this.s != null)
            {
                this.s.CopyTo(0, chars, offset, charArrayLength);
                return charArrayLength;
            }
            byte* pb = stackalloc byte[0x10];
            this.UnsafeSetInt64(this.idLow, pb);
            this.UnsafeSetInt64(this.idHigh, pb + 8);
            fixed (char* chRef = &(chars[offset]))
            {
                char* chPtr = chRef;
                chPtr[0] = 'u';
                chPtr[1] = 'r';
                chPtr[2] = 'n';
                chPtr[3] = ':';
                chPtr[4] = 'u';
                chPtr[5] = 'u';
                chPtr[6] = 'i';
                chPtr[7] = 'd';
                chPtr[8] = ':';
                chPtr[0x11] = '-';
                chPtr[0x16] = '-';
                chPtr[0x1b] = '-';
                chPtr[0x20] = '-';
                fixed (char* str = "0123456789abcdef")
                {
                    char* chPtr3 = str;
                    this.UnsafeEncode(chPtr3, pb[0], chPtr + 15);
                    this.UnsafeEncode(chPtr3, pb[1], chPtr + 13);
                    this.UnsafeEncode(chPtr3, pb[2], chPtr + 11);
                    this.UnsafeEncode(chPtr3, pb[3], chPtr + 9);
                    this.UnsafeEncode(chPtr3, pb[4], chPtr + 20);
                    this.UnsafeEncode(chPtr3, pb[5], chPtr + 0x12);
                    this.UnsafeEncode(chPtr3, pb[6], chPtr + 0x19);
                    this.UnsafeEncode(chPtr3, pb[7], chPtr + 0x17);
                    this.UnsafeEncode(chPtr3, pb[8], chPtr + 0x1c);
                    this.UnsafeEncode(chPtr3, pb[9], chPtr + 30);
                    this.UnsafeEncode(chPtr3, pb[10], chPtr + 0x21);
                    this.UnsafeEncode(chPtr3, pb[11], chPtr + 0x23);
                    this.UnsafeEncode(chPtr3, pb[12], chPtr + 0x25);
                    this.UnsafeEncode(chPtr3, pb[13], chPtr + 0x27);
                    this.UnsafeEncode(chPtr3, pb[14], chPtr + 0x29);
                    this.UnsafeEncode(chPtr3, pb[15], chPtr + 0x2b);
                }
            }
            return charArrayLength;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            if (this.s == null)
            {
                int charArrayLength = this.CharArrayLength;
                char[] chars = new char[charArrayLength];
                this.ToCharArray(chars, 0);
                this.s = new string(chars, 0, charArrayLength);
            }
            return this.s;
        }

        public bool TryGetGuid(out Guid guid)
        {
            byte[] buffer = new byte[0x10];
            if (!this.TryGetGuid(buffer, 0))
            {
                guid = Guid.Empty;
                return false;
            }
            guid = new Guid(buffer);
            return true;
        }

        [SecuritySafeCritical]
        public unsafe bool TryGetGuid(byte[] buffer, int offset)
        {
            if (!this.IsGuid)
            {
                return false;
            }
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > buffer.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { buffer.Length })));
            }
            if (0x10 > (buffer.Length - offset))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("buffer", System.Runtime.Serialization.SR.GetString("XmlArrayTooSmallOutput", new object[] { 0x10 })));
            }
            fixed (byte* numRef = &(buffer[offset]))
            {
                this.UnsafeSetInt64(this.idLow, numRef);
                this.UnsafeSetInt64(this.idHigh, numRef + 8);
            }
            return true;
        }

        [SecurityCritical]
        private unsafe int UnsafeDecode(short* char2val, char ch1, char ch2)
        {
            if ((ch1 | ch2) >= 0x80)
            {
                return 0x100;
            }
            return (char2val[ch1] | char2val['\x0080' + ch2]);
        }

        [SecurityCritical]
        private unsafe void UnsafeEncode(char* val2char, byte b, char* pch)
        {
            pch[0] = val2char[b >> 4];
            pch[1] = val2char[b & 15];
        }

        [SecurityCritical]
        private unsafe int UnsafeGetInt32(byte* pb)
        {
            int num = pb[3];
            num = num << 8;
            num |= pb[2];
            num = num << 8;
            num |= pb[1];
            num = num << 8;
            return (num | pb[0]);
        }

        [SecurityCritical]
        private unsafe long UnsafeGetInt64(byte* pb)
        {
            int num = this.UnsafeGetInt32(pb);
            return ((this.UnsafeGetInt32((byte*) (pb + 4)) << 0x20) | ((long) ((ulong) num)));
        }

        [SecurityCritical]
        private unsafe void UnsafeParse(char* chars, int charCount)
        {
            if (((((charCount == 0x2d) && (chars[0] == 'u')) && ((chars[1] == 'r') && (chars[2] == 'n'))) && (((chars[3] == ':') && (chars[4] == 'u')) && ((chars[5] == 'u') && (chars[6] == 'i')))) && ((((chars[7] == 'd') && (chars[8] == ':')) && ((chars[0x11] == '-') && (chars[0x16] == '-'))) && ((chars[0x1b] == '-') && (chars[0x20] == '-'))))
            {
                byte* pb = stackalloc byte[0x10];
                int num = 0;
                int num2 = 0;
                fixed (short* numRef = char2val)
                {
                    short* numPtr2 = numRef;
                    num = this.UnsafeDecode(numPtr2, chars[15], chars[0x10]);
                    pb[0] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[13], chars[14]);
                    pb[1] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[11], chars[12]);
                    pb[2] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[9], chars[10]);
                    pb[3] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[20], chars[0x15]);
                    pb[4] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x12], chars[0x13]);
                    pb[5] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x19], chars[0x1a]);
                    pb[6] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x17], chars[0x18]);
                    pb[7] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x1c], chars[0x1d]);
                    pb[8] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[30], chars[0x1f]);
                    pb[9] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x21], chars[0x22]);
                    pb[10] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x23], chars[0x24]);
                    pb[11] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x25], chars[0x26]);
                    pb[12] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x27], chars[40]);
                    pb[13] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x29], chars[0x2a]);
                    pb[14] = (byte) num;
                    num2 |= num;
                    num = this.UnsafeDecode(numPtr2, chars[0x2b], chars[0x2c]);
                    pb[15] = (byte) num;
                    num2 |= num;
                    if (num2 < 0x100)
                    {
                        this.idLow = this.UnsafeGetInt64(pb);
                        this.idHigh = this.UnsafeGetInt64(pb + 8);
                    }
                }
            }
        }

        [SecurityCritical]
        private unsafe void UnsafeSetInt32(int value, byte* pb)
        {
            pb[0] = (byte) value;
            value = value >> 8;
            pb[1] = (byte) value;
            value = value >> 8;
            pb[2] = (byte) value;
            value = value >> 8;
            pb[3] = (byte) value;
        }

        [SecurityCritical]
        private unsafe void UnsafeSetInt64(long value, byte* pb)
        {
            this.UnsafeSetInt32((int) value, pb);
            this.UnsafeSetInt32((int) (value >> 0x20), pb + 4);
        }

        public int CharArrayLength
        {
            [SecuritySafeCritical]
            get
            {
                if (this.s != null)
                {
                    return this.s.Length;
                }
                return 0x2d;
            }
        }

        public bool IsGuid
        {
            get
            {
                return ((this.idLow | this.idHigh) != 0L);
            }
        }
    }
}


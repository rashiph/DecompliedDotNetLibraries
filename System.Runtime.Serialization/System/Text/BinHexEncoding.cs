namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;

    internal class BinHexEncoding : Encoding
    {
        private static byte[] char2val = new byte[] { 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 10, 11, 12, 13, 14, 15, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 10, 11, 12, 13, 14, 15, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
         };
        private static string val2char = "0123456789ABCDEF";

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return this.GetMaxByteCount(count);
        }

        [SecuritySafeCritical]
        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (charIndex < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (charIndex > chars.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { chars.Length })));
            }
            if (charCount < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charCount", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (charCount > (chars.Length - charIndex))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charCount", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { chars.Length - charIndex })));
            }
            if (bytes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bytes"));
            }
            if (byteIndex < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (byteIndex > bytes.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { bytes.Length })));
            }
            int num = this.GetByteCount(chars, charIndex, charCount);
            if ((num < 0) || (num > (bytes.Length - byteIndex)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlArrayTooSmall"), "bytes"));
            }
            if (charCount > 0)
            {
                fixed (byte* numRef = char2val)
                {
                    fixed (byte* numRef2 = &(bytes[byteIndex]))
                    {
                        fixed (char* chRef = &(chars[charIndex]))
                        {
                            char* chPtr = chRef;
                            char* chPtr2 = chRef + charCount;
                            for (byte* numPtr = numRef2; chPtr < chPtr2; numPtr++)
                            {
                                char ch = chPtr[0];
                                char ch2 = chPtr[1];
                                if ((ch | ch2) >= 0x80)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBinHexSequence", new object[] { new string(chPtr, 0, 2), charIndex + ((int) ((long) ((chPtr - chRef) / 2))) })));
                                }
                                byte num2 = numRef[(int) ((byte*) ch)];
                                byte num3 = numRef[(int) ((byte*) ch2)];
                                if ((num2 | num3) == 0xff)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBinHexSequence", new object[] { new string(chPtr, 0, 2), charIndex + ((int) ((long) ((chPtr - chRef) / 2))) })));
                                }
                                numPtr[0] = (byte) ((num2 << 4) + num3);
                                chPtr += 2;
                            }
                        }
                    }
                }
            }
            return num;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return this.GetMaxCharCount(count);
        }

        [SecuritySafeCritical]
        public override unsafe int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (bytes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bytes"));
            }
            if (byteIndex < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (byteIndex > bytes.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { bytes.Length })));
            }
            if (byteCount < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteCount", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (byteCount > (bytes.Length - byteIndex))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteCount", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { bytes.Length - byteIndex })));
            }
            int num = this.GetCharCount(bytes, byteIndex, byteCount);
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (charIndex < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (charIndex > chars.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { chars.Length })));
            }
            if ((num < 0) || (num > (chars.Length - charIndex)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlArrayTooSmall"), "chars"));
            }
            if (byteCount > 0)
            {
                fixed (char* str = ((char*) val2char))
                {
                    char* chPtr = str;
                    fixed (byte* numRef = &(bytes[byteIndex]))
                    {
                        fixed (char* chRef = &(chars[charIndex]))
                        {
                            char* chPtr2 = chRef;
                            byte* numPtr = numRef;
                            byte* numPtr2 = numRef + byteCount;
                            while (numPtr < numPtr2)
                            {
                                chPtr2[0] = chPtr[numPtr[0] >> 4];
                                chPtr2[1] = chPtr[numPtr[0] & 15];
                                numPtr++;
                                chPtr2 += 2;
                            }
                        }
                    }
                }
            }
            return num;
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charCount", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if ((charCount % 2) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBinHexLength", new object[] { charCount.ToString(NumberFormatInfo.CurrentInfo) })));
            }
            return (charCount / 2);
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if ((byteCount < 0) || (byteCount > 0x3fffffff))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteCount", System.Runtime.Serialization.SR.GetString("ValueMustBeInRange", new object[] { 0, 0x3fffffff })));
            }
            return (byteCount * 2);
        }
    }
}


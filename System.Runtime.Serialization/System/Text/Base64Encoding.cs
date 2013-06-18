namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;

    internal class Base64Encoding : Encoding
    {
        private static byte[] char2val = new byte[] { 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x3e, 0xff, 0xff, 0xff, 0x3f, 
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 60, 0x3d, 0xff, 0xff, 0xff, 0x40, 0xff, 0xff, 
            0xff, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 
            15, 0x10, 0x11, 0x12, 0x13, 20, 0x15, 0x16, 0x17, 0x18, 0x19, 0xff, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0x1a, 0x1b, 0x1c, 0x1d, 30, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 40, 
            0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 50, 0x33, 0xff, 0xff, 0xff, 0xff, 0xff
         };
        private static byte[] val2byte = new byte[] { 
            0x41, 0x42, 0x43, 0x44, 0x45, 70, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 80, 
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 90, 0x61, 0x62, 0x63, 100, 0x65, 0x66, 
            0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 110, 0x6f, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 
            0x77, 120, 0x79, 0x7a, 0x30, 0x31, 50, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x2b, 0x2f
         };
        private static string val2char = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        [SecuritySafeCritical]
        public override unsafe int GetByteCount(char[] chars, int index, int count)
        {
            byte[] buffer;
            if (chars == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            }
            if (index < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (index > chars.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", System.Runtime.Serialization.SR.GetString("OffsetExceedsBufferSize", new object[] { chars.Length })));
            }
            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("ValueMustBeNonNegative")));
            }
            if (count > (chars.Length - index))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.Runtime.Serialization.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { chars.Length - index })));
            }
            if (count == 0)
            {
                return 0;
            }
            if ((count % 4) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Length", new object[] { count.ToString(NumberFormatInfo.CurrentInfo) })));
            }
            if (((buffer = char2val) == null) || (buffer.Length == 0))
            {
                numRef = null;
                goto Label_0134;
            }
            fixed (byte* numRef = buffer)
            {
            Label_0134:
                fixed (char* chRef = &(chars[index]))
                {
                    int num = 0;
                    char* chPtr = chRef;
                    char* chPtr2 = chRef + count;
                    while (chPtr < chPtr2)
                    {
                        char ch = chPtr[0];
                        char ch2 = chPtr[1];
                        char ch3 = chPtr[2];
                        char ch4 = chPtr[3];
                        if ((((ch | ch2) | ch3) | ch4) >= 0x80)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Sequence", new object[] { new string(chPtr, 0, 4), index + ((int) ((long) ((chPtr - chRef) / 2))) })));
                        }
                        int num2 = numRef[ch];
                        int num3 = numRef[ch2];
                        int num4 = numRef[ch3];
                        int num5 = numRef[ch4];
                        if (!this.IsValidLeadBytes(num2, num3, num4, num5) || !this.IsValidTailBytes(num4, num5))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Sequence", new object[] { new string(chPtr, 0, 4), index + ((int) ((long) ((chPtr - chRef) / 2))) })));
                        }
                        int num6 = (num5 != 0x40) ? 3 : ((num4 != 0x40) ? 2 : 1);
                        num += num6;
                        chPtr += 4;
                    }
                    return num;
                }
            }
        }

        [SecuritySafeCritical]
        public virtual unsafe int GetBytes(byte[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            byte[] buffer;
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
            if (charCount == 0)
            {
                return 0;
            }
            if ((charCount % 4) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Length", new object[] { charCount.ToString(NumberFormatInfo.CurrentInfo) })));
            }
            if (((buffer = char2val) == null) || (buffer.Length == 0))
            {
                numRef = null;
                goto Label_01AF;
            }
            fixed (byte* numRef = buffer)
            {
            Label_01AF:
                fixed (byte* numRef2 = &(chars[charIndex]))
                {
                    fixed (byte* numRef3 = &(bytes[byteIndex]))
                    {
                        byte* numPtr = numRef2;
                        byte* numPtr2 = numRef2 + charCount;
                        byte* numPtr3 = numRef3;
                        byte* numPtr4 = (numRef3 + bytes.Length) - byteIndex;
                        while (numPtr < numPtr2)
                        {
                            byte index = numPtr[0];
                            byte num2 = numPtr[1];
                            byte num3 = numPtr[2];
                            byte num4 = numPtr[3];
                            if ((((index | num2) | num3) | num4) >= 0x80)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Sequence", new object[] { new string((sbyte*) numPtr, 0, 4), charIndex + ((int) ((long) ((numPtr - numRef2) / 1))) })));
                            }
                            int num5 = numRef[index];
                            int num6 = numRef[num2];
                            int num7 = numRef[num3];
                            int num8 = numRef[num4];
                            if (!this.IsValidLeadBytes(num5, num6, num7, num8) || !this.IsValidTailBytes(num7, num8))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Sequence", new object[] { new string((sbyte*) numPtr, 0, 4), charIndex + ((int) ((long) ((numPtr - numRef2) / 1))) })));
                            }
                            int num9 = (num8 != 0x40) ? 3 : ((num7 != 0x40) ? 2 : 1);
                            if ((numPtr3 + num9) > numPtr4)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlArrayTooSmall"), "bytes"));
                            }
                            numPtr3[0] = (byte) ((num5 << 2) | ((num6 >> 4) & 3));
                            if (num9 > 1)
                            {
                                numPtr3[1] = (byte) ((num6 << 4) | ((num7 >> 2) & 15));
                                if (num9 > 2)
                                {
                                    numPtr3[2] = (byte) ((num7 << 6) | (num8 & 0x3f));
                                }
                            }
                            numPtr3 += num9;
                            numPtr += 4;
                        }
                        return (int) ((long) ((numPtr3 - numRef3) / 1));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            byte[] buffer;
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
            if (charCount == 0)
            {
                return 0;
            }
            if ((charCount % 4) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Length", new object[] { charCount.ToString(NumberFormatInfo.CurrentInfo) })));
            }
            if (((buffer = char2val) == null) || (buffer.Length == 0))
            {
                numRef = null;
                goto Label_01AF;
            }
            fixed (byte* numRef = buffer)
            {
            Label_01AF:
                fixed (char* chRef = &(chars[charIndex]))
                {
                    fixed (byte* numRef2 = &(bytes[byteIndex]))
                    {
                        char* chPtr = chRef;
                        char* chPtr2 = chRef + charCount;
                        byte* numPtr = numRef2;
                        byte* numPtr2 = (numRef2 + bytes.Length) - byteIndex;
                        while (chPtr < chPtr2)
                        {
                            char index = chPtr[0];
                            char ch2 = chPtr[1];
                            char ch3 = chPtr[2];
                            char ch4 = chPtr[3];
                            if ((((index | ch2) | ch3) | ch4) >= 0x80)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Sequence", new object[] { new string(chPtr, 0, 4), charIndex + ((int) ((long) ((chPtr - chRef) / 2))) })));
                            }
                            int num = numRef[index];
                            int num2 = numRef[ch2];
                            int num3 = numRef[ch3];
                            int num4 = numRef[ch4];
                            if (!this.IsValidLeadBytes(num, num2, num3, num4) || !this.IsValidTailBytes(num3, num4))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Sequence", new object[] { new string(chPtr, 0, 4), charIndex + ((int) ((long) ((chPtr - chRef) / 2))) })));
                            }
                            int num5 = (num4 != 0x40) ? 3 : ((num3 != 0x40) ? 2 : 1);
                            if ((numPtr + num5) > numPtr2)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString("XmlArrayTooSmall"), "bytes"));
                            }
                            numPtr[0] = (byte) ((num << 2) | ((num2 >> 4) & 3));
                            if (num5 > 1)
                            {
                                numPtr[1] = (byte) ((num2 << 4) | ((num3 >> 2) & 15));
                                if (num5 > 2)
                                {
                                    numPtr[2] = (byte) ((num3 << 6) | (num4 & 0x3f));
                                }
                            }
                            numPtr += num5;
                            chPtr += 4;
                        }
                        return (int) ((long) ((numPtr - numRef2) / 1));
                    }
                }
            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return this.GetMaxCharCount(count);
        }

        [SecuritySafeCritical]
        public unsafe int GetChars(byte[] bytes, int byteIndex, int byteCount, byte[] chars, int charIndex)
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
                fixed (byte* numRef = val2byte)
                {
                    fixed (byte* numRef2 = &(bytes[byteIndex]))
                    {
                        fixed (byte* numRef3 = &(chars[charIndex]))
                        {
                            byte* numPtr = numRef2;
                            byte* numPtr2 = (numPtr + byteCount) - 3;
                            byte* numPtr3 = numRef3;
                            while (numPtr <= numPtr2)
                            {
                                numPtr3[0] = numRef[numPtr[0] >> 2];
                                numPtr3[1] = numRef[((numPtr[0] & 3) << 4) | (numPtr[1] >> 4)];
                                numPtr3[2] = numRef[((numPtr[1] & 15) << 2) | (numPtr[2] >> 6)];
                                numPtr3[3] = numRef[numPtr[2] & 0x3f];
                                numPtr += 3;
                                numPtr3 += 4;
                            }
                            if (((long) ((numPtr - numPtr2) / 1)) == 2L)
                            {
                                numPtr3[0] = numRef[numPtr[0] >> 2];
                                numPtr3[1] = numRef[(numPtr[0] & 3) << 4];
                                numPtr3[2] = 0x3d;
                                numPtr3[3] = 0x3d;
                            }
                            else if (((long) ((numPtr - numPtr2) / 1)) == 1L)
                            {
                                numPtr3[0] = numRef[numPtr[0] >> 2];
                                numPtr3[1] = numRef[((numPtr[0] & 3) << 4) | (numPtr[1] >> 4)];
                                numPtr3[2] = numRef[(numPtr[1] & 15) << 2];
                                numPtr3[3] = 0x3d;
                            }
                        }
                    }
                }
            }
            return num;
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
                            byte* numPtr = numRef;
                            byte* numPtr2 = (numPtr + byteCount) - 3;
                            char* chPtr2 = chRef;
                            while (numPtr <= numPtr2)
                            {
                                chPtr2[0] = chPtr[numPtr[0] >> 2];
                                chPtr2[1] = chPtr[((numPtr[0] & 3) << 4) | (numPtr[1] >> 4)];
                                chPtr2[2] = chPtr[((numPtr[1] & 15) << 2) | (numPtr[2] >> 6)];
                                chPtr2[3] = chPtr[numPtr[2] & 0x3f];
                                numPtr += 3;
                                chPtr2 += 4;
                            }
                            if (((long) ((numPtr - numPtr2) / 1)) == 2L)
                            {
                                chPtr2[0] = chPtr[numPtr[0] >> 2];
                                chPtr2[1] = chPtr[(numPtr[0] & 3) << 4];
                                chPtr2[2] = '=';
                                chPtr2[3] = '=';
                            }
                            else if (((long) ((numPtr - numPtr2) / 1)) == 1L)
                            {
                                chPtr2[0] = chPtr[numPtr[0] >> 2];
                                chPtr2[1] = chPtr[((numPtr[0] & 3) << 4) | (numPtr[1] >> 4)];
                                chPtr2[2] = chPtr[(numPtr[1] & 15) << 2];
                                chPtr2[3] = '=';
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
            if ((charCount % 4) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(System.Runtime.Serialization.SR.GetString("XmlInvalidBase64Length", new object[] { charCount.ToString(NumberFormatInfo.CurrentInfo) })));
            }
            return ((charCount / 4) * 3);
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if ((byteCount < 0) || (byteCount > 0x5ffffffb))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteCount", System.Runtime.Serialization.SR.GetString("ValueMustBeInRange", new object[] { 0, 0x5ffffffb })));
            }
            return (((byteCount + 2) / 3) * 4);
        }

        private bool IsValidLeadBytes(int v1, int v2, int v3, int v4)
        {
            return (((v1 | v2) < 0x40) && ((v3 | v4) != 0xff));
        }

        private bool IsValidTailBytes(int v3, int v4)
        {
            if (v3 == 0x40)
            {
                return (v4 == 0x40);
            }
            return true;
        }
    }
}


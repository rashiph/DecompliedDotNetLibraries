namespace System.Globalization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    public static class CharUnicodeInfo
    {
        internal const int BIDI_CATEGORY_OFFSET = 1;
        internal const char HIGH_SURROGATE_END = '\udbff';
        internal const char HIGH_SURROGATE_START = '\ud800';
        internal const char LOW_SURROGATE_END = '\udfff';
        internal const char LOW_SURROGATE_START = '\udc00';
        private unsafe static byte* s_pCategoriesValue;
        private unsafe static ushort* s_pCategoryLevel1Index;
        private unsafe static byte* s_pDataTable = GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(CharUnicodeInfo).Assembly, "charinfo.nlp");
        private unsafe static DigitValues* s_pDigitValues;
        private unsafe static ushort* s_pNumericLevel1Index;
        private unsafe static byte* s_pNumericValues;
        internal const int UNICODE_CATEGORY_OFFSET = 0;
        internal const string UNICODE_INFO_FILE_NAME = "charinfo.nlp";
        internal const int UNICODE_PLANE01_START = 0x10000;

        [SecuritySafeCritical]
        static unsafe CharUnicodeInfo()
        {
            UnicodeDataHeader* headerPtr = (UnicodeDataHeader*) s_pDataTable;
            s_pCategoryLevel1Index = (ushort*) (s_pDataTable + headerPtr->OffsetToCategoriesIndex);
            s_pCategoriesValue = s_pDataTable + ((byte*) headerPtr->OffsetToCategoriesValue);
            s_pNumericLevel1Index = (ushort*) (s_pDataTable + headerPtr->OffsetToNumbericIndex);
            s_pNumericValues = s_pDataTable + ((byte*) headerPtr->OffsetToNumbericValue);
            s_pDigitValues = (DigitValues*) (s_pDataTable + headerPtr->OffsetToDigitValue);
            nativeInitTable(s_pDataTable);
        }

        internal static BidiCategory GetBidiCategory(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return (BidiCategory) InternalGetCategoryValue(InternalConvertToUtf32(s, index), 1);
        }

        public static int GetDecimalDigitValue(char ch)
        {
            return InternalGetDecimalDigitValue(ch);
        }

        public static int GetDecimalDigitValue(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if ((index < 0) || (index >= s.Length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            return InternalGetDecimalDigitValue(InternalConvertToUtf32(s, index));
        }

        public static int GetDigitValue(char ch)
        {
            return InternalGetDigitValue(ch);
        }

        public static int GetDigitValue(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if ((index < 0) || (index >= s.Length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            return InternalGetDigitValue(InternalConvertToUtf32(s, index));
        }

        [SecuritySafeCritical]
        public static double GetNumericValue(char ch)
        {
            return InternalGetNumericValue(ch);
        }

        [SecuritySafeCritical]
        public static double GetNumericValue(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if ((index < 0) || (index >= s.Length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            return InternalGetNumericValue(InternalConvertToUtf32(s, index));
        }

        [SecuritySafeCritical]
        public static UnicodeCategory GetUnicodeCategory(char ch)
        {
            return InternalGetUnicodeCategory(ch);
        }

        [SecuritySafeCritical]
        public static UnicodeCategory GetUnicodeCategory(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return InternalGetUnicodeCategory(s, index);
        }

        internal static int InternalConvertToUtf32(string s, int index)
        {
            if (index < (s.Length - 1))
            {
                int num = s[index] - 0xd800;
                if ((num >= 0) && (num <= 0x3ff))
                {
                    int num2 = s[index + 1] - 0xdc00;
                    if ((num2 >= 0) && (num2 <= 0x3ff))
                    {
                        return (((num * 0x400) + num2) + 0x10000);
                    }
                }
            }
            return s[index];
        }

        internal static int InternalConvertToUtf32(string s, int index, out int charLength)
        {
            charLength = 1;
            if (index < (s.Length - 1))
            {
                int num = s[index] - 0xd800;
                if ((num >= 0) && (num <= 0x3ff))
                {
                    int num2 = s[index + 1] - 0xdc00;
                    if ((num2 >= 0) && (num2 <= 0x3ff))
                    {
                        charLength++;
                        return (((num * 0x400) + num2) + 0x10000);
                    }
                }
            }
            return s[index];
        }

        [SecuritySafeCritical]
        internal static unsafe byte InternalGetCategoryValue(int ch, int offset)
        {
            ushort num = s_pCategoryLevel1Index[ch >> 8];
            num = s_pCategoryLevel1Index[num + ((ch >> 4) & 15)];
            byte* numPtr = (byte*) (s_pCategoryLevel1Index + num);
            byte num2 = numPtr[ch & 15];
            return s_pCategoriesValue[(num2 * 2) + offset];
        }

        [SecuritySafeCritical]
        internal static sbyte InternalGetDecimalDigitValue(int ch)
        {
            return InternalGetDigitValues(ch).decimalDigit;
        }

        [SecuritySafeCritical]
        internal static sbyte InternalGetDigitValue(int ch)
        {
            return InternalGetDigitValues(ch).digit;
        }

        [SecuritySafeCritical]
        internal static unsafe DigitValues* InternalGetDigitValues(int ch)
        {
            ushort num = s_pNumericLevel1Index[ch >> 8];
            num = s_pNumericLevel1Index[num + ((ch >> 4) & 15)];
            byte* numPtr = (byte*) (s_pNumericLevel1Index + num);
            return (s_pDigitValues + numPtr[ch & 15]);
        }

        [SecuritySafeCritical]
        internal static unsafe double InternalGetNumericValue(int ch)
        {
            ushort num = s_pNumericLevel1Index[ch >> 8];
            num = s_pNumericLevel1Index[num + ((ch >> 4) & 15)];
            byte* numPtr = (byte*) (s_pNumericLevel1Index + num);
            return *(((double*) (s_pNumericValues + (numPtr[ch & 15] * 8))));
        }

        internal static UnicodeCategory InternalGetUnicodeCategory(int ch)
        {
            return (UnicodeCategory) InternalGetCategoryValue(ch, 0);
        }

        internal static UnicodeCategory InternalGetUnicodeCategory(string value, int index)
        {
            return InternalGetUnicodeCategory(InternalConvertToUtf32(value, index));
        }

        internal static UnicodeCategory InternalGetUnicodeCategory(string str, int index, out int charLength)
        {
            return InternalGetUnicodeCategory(InternalConvertToUtf32(str, index, out charLength));
        }

        internal static bool IsCombiningCategory(UnicodeCategory uc)
        {
            if ((uc != UnicodeCategory.NonSpacingMark) && (uc != UnicodeCategory.SpacingCombiningMark))
            {
                return (uc == UnicodeCategory.EnclosingMark);
            }
            return true;
        }

        internal static bool IsWhiteSpace(char c)
        {
            switch (GetUnicodeCategory(c))
            {
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.ParagraphSeparator:
                    return true;
            }
            return false;
        }

        internal static bool IsWhiteSpace(string s, int index)
        {
            switch (GetUnicodeCategory(s, index))
            {
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.ParagraphSeparator:
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void nativeInitTable(byte* bytePtr);

        [StructLayout(LayoutKind.Sequential, Pack=2)]
        internal struct DigitValues
        {
            internal sbyte decimalDigit;
            internal sbyte digit;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct UnicodeDataHeader
        {
            [FieldOffset(40)]
            internal uint OffsetToCategoriesIndex;
            [FieldOffset(0x2c)]
            internal uint OffsetToCategoriesValue;
            [FieldOffset(0x34)]
            internal uint OffsetToDigitValue;
            [FieldOffset(0x30)]
            internal uint OffsetToNumbericIndex;
            [FieldOffset(0x38)]
            internal uint OffsetToNumbericValue;
            [FieldOffset(0)]
            internal char TableName;
            [FieldOffset(0x20)]
            internal ushort version;
        }
    }
}


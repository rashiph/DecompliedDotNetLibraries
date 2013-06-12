namespace System.Globalization
{
    using System;
    using System.Text;

    public sealed class IdnMapping
    {
        private const int damp = 700;
        private const char delimiter = '-';
        private const int initial_bias = 0x48;
        private const int initial_n = 0x80;
        private bool m_bAllowUnassigned;
        private bool m_bUseStd3AsciiRules;
        private const int M_defaultNameLimit = 0xff;
        private static char[] M_Dots = new char[] { '.', '。', '．', '｡' };
        private const int M_labelLimit = 0x3f;
        private const string M_strAcePrefix = "xn--";
        private const int maxint = 0x7ffffff;
        private const int punycodeBase = 0x24;
        private const int skew = 0x26;
        private const int tmax = 0x1a;
        private const int tmin = 1;

        private static int adapt(int delta, int numpoints, bool firsttime)
        {
            delta = firsttime ? (delta / 700) : (delta / 2);
            delta += delta / numpoints;
            uint num = 0;
            while (delta > 0x1c7)
            {
                delta /= 0x23;
                num += 0x24;
            }
            return (((int) num) + ((0x24 * delta) / (delta + 0x26)));
        }

        private static bool basic(uint cp)
        {
            return (cp < 0x80);
        }

        private static int decode_digit(char cp)
        {
            if ((cp >= '0') && (cp <= '9'))
            {
                return ((cp - '0') + 0x1a);
            }
            if ((cp >= 'a') && (cp <= 'z'))
            {
                return (cp - 'a');
            }
            if ((cp < 'A') || (cp > 'Z'))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
            }
            return (cp - 'A');
        }

        private static char encode_basic(char bcp)
        {
            if (HasUpperCaseFlag(bcp))
            {
                bcp = (char) (bcp + ' ');
            }
            return bcp;
        }

        private static char encode_digit(int d)
        {
            if (d > 0x19)
            {
                return (char) ((d - 0x1a) + 0x30);
            }
            return (char) (d + 0x61);
        }

        public override bool Equals(object obj)
        {
            IdnMapping mapping = obj as IdnMapping;
            if (mapping == null)
            {
                return false;
            }
            return ((this.m_bAllowUnassigned == mapping.m_bAllowUnassigned) && (this.m_bUseStd3AsciiRules == mapping.m_bUseStd3AsciiRules));
        }

        public string GetAscii(string unicode)
        {
            return this.GetAscii(unicode, 0);
        }

        public string GetAscii(string unicode, int index)
        {
            if (unicode == null)
            {
                throw new ArgumentNullException("unicode");
            }
            return this.GetAscii(unicode, index, unicode.Length - index);
        }

        public string GetAscii(string unicode, int index, int count)
        {
            if (unicode == null)
            {
                throw new ArgumentNullException("unicode");
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (index > unicode.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (index > (unicode.Length - count))
            {
                throw new ArgumentOutOfRangeException("unicode", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            unicode = unicode.Substring(index, count);
            if (ValidateStd3AndAscii(unicode, this.UseStd3AsciiRules, true))
            {
                return unicode;
            }
            if (unicode[unicode.Length - 1] <= '\x001f')
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequence", new object[] { unicode.Length - 1 }), "unicode");
            }
            bool flag = (unicode.Length > 0) && IsDot(unicode[unicode.Length - 1]);
            unicode = unicode.Normalize(this.m_bAllowUnassigned ? ((NormalizationForm) 13) : ((NormalizationForm) 0x10d));
            if ((!flag && (unicode.Length > 0)) && IsDot(unicode[unicode.Length - 1]))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "unicode");
            }
            if (this.UseStd3AsciiRules)
            {
                ValidateStd3AndAscii(unicode, true, false);
            }
            return punycode_encode(unicode);
        }

        public override int GetHashCode()
        {
            return ((this.m_bAllowUnassigned ? 100 : 200) + (this.m_bUseStd3AsciiRules ? 0x3e8 : 0x7d0));
        }

        public string GetUnicode(string ascii)
        {
            return this.GetUnicode(ascii, 0);
        }

        public string GetUnicode(string ascii, int index)
        {
            if (ascii == null)
            {
                throw new ArgumentNullException("ascii");
            }
            return this.GetUnicode(ascii, index, ascii.Length - index);
        }

        public string GetUnicode(string ascii, int index, int count)
        {
            if (ascii == null)
            {
                throw new ArgumentNullException("ascii");
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (index > ascii.Length)
            {
                throw new ArgumentOutOfRangeException("byteIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (index > (ascii.Length - count))
            {
                throw new ArgumentOutOfRangeException("ascii", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            ascii = ascii.Substring(index, count);
            string unicode = punycode_decode(ascii);
            if (!ascii.Equals(this.GetAscii(unicode), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnIllegalName"), "ascii");
            }
            return unicode;
        }

        private static bool HasUpperCaseFlag(char punychar)
        {
            return ((punychar >= 'A') && (punychar <= 'Z'));
        }

        private static bool IsDot(char c)
        {
            if (((c != '.') && (c != '。')) && (c != 0xff0e))
            {
                return (c == 0xff61);
            }
            return true;
        }

        private static bool IsSupplementary(int cTest)
        {
            return (cTest >= 0x10000);
        }

        private static string punycode_decode(string ascii)
        {
            if (ascii.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "ascii");
            }
            if (ascii.Length > (0xff - (IsDot(ascii[ascii.Length - 1]) ? 0 : 1)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadNameSize", new object[] { 0xff - (IsDot(ascii[ascii.Length - 1]) ? 0 : 1) }), "ascii");
            }
            StringBuilder builder = new StringBuilder(ascii.Length);
            int index = 0;
            int startIndex = 0;
            for (int i = 0; index < ascii.Length; i = builder.Length)
            {
                index = ascii.IndexOf('.', startIndex);
                if ((index < 0) || (index > ascii.Length))
                {
                    index = ascii.Length;
                }
                if (index == startIndex)
                {
                    if (index != ascii.Length)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "ascii");
                    }
                    break;
                }
                if ((index - startIndex) > 0x3f)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "ascii");
                }
                if ((ascii.Length < ("xn--".Length + startIndex)) || !ascii.Substring(startIndex, "xn--".Length).Equals("xn--", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Append(ascii.Substring(startIndex, index - startIndex));
                }
                else
                {
                    int num5;
                    startIndex += "xn--".Length;
                    int num4 = ascii.LastIndexOf('-', index - 1);
                    if (num4 == (index - 1))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                    }
                    if (num4 <= startIndex)
                    {
                        num5 = 0;
                    }
                    else
                    {
                        num5 = num4 - startIndex;
                        for (int k = startIndex; k < (startIndex + num5); k++)
                        {
                            if (ascii[k] > '\x007f')
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                            }
                            builder.Append(((ascii[k] >= 'A') && (ascii[k] <= 'Z')) ? ((char) ((ascii[k] - 'A') + 0x61)) : ascii[k]);
                        }
                    }
                    int num7 = startIndex + ((num5 > 0) ? (num5 + 1) : 0);
                    int num8 = 0x80;
                    int num9 = 0x48;
                    int num10 = 0;
                    int num13 = 0;
                    while (num7 < index)
                    {
                        int num17;
                        int num14 = num10;
                        int num11 = 1;
                        int num12 = 0x24;
                        while (true)
                        {
                            if (num7 >= index)
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                            }
                            int num15 = decode_digit(ascii[num7++]);
                            if (num15 > ((0x7ffffff - num10) / num11))
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                            }
                            num10 += num15 * num11;
                            int num16 = (num12 <= num9) ? 1 : ((num12 >= (num9 + 0x1a)) ? 0x1a : (num12 - num9));
                            if (num15 < num16)
                            {
                                break;
                            }
                            if (num11 > (0x7ffffff / (0x24 - num16)))
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                            }
                            num11 *= 0x24 - num16;
                            num12 += 0x24;
                        }
                        num9 = adapt(num10 - num14, ((builder.Length - i) - num13) + 1, num14 == 0);
                        if ((num10 / (((builder.Length - i) - num13) + 1)) > (0x7ffffff - num8))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                        }
                        num8 += num10 / (((builder.Length - i) - num13) + 1);
                        num10 = num10 % (((builder.Length - i) - num13) + 1);
                        if (((num8 < 0) || (num8 > 0x10ffff)) || ((num8 >= 0xd800) && (num8 <= 0xdfff)))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                        }
                        string str = char.ConvertFromUtf32(num8);
                        if (num13 > 0)
                        {
                            int num18 = num10;
                            num17 = i;
                            while (num18 > 0)
                            {
                                if (num17 >= builder.Length)
                                {
                                    throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "ascii");
                                }
                                if (char.IsSurrogate(builder[num17]))
                                {
                                    num17++;
                                }
                                num18--;
                                num17++;
                            }
                        }
                        else
                        {
                            num17 = i + num10;
                        }
                        builder.Insert(num17, str);
                        if (IsSupplementary(num8))
                        {
                            num13++;
                        }
                        num10++;
                    }
                    bool flag = false;
                    BidiCategory bidiCategory = CharUnicodeInfo.GetBidiCategory(builder.ToString(), i);
                    switch (bidiCategory)
                    {
                        case BidiCategory.RightToLeft:
                        case BidiCategory.RightToLeftArabic:
                            flag = true;
                            break;
                    }
                    for (int j = i; j < builder.Length; j++)
                    {
                        if (!char.IsLowSurrogate(builder.ToString(), j))
                        {
                            bidiCategory = CharUnicodeInfo.GetBidiCategory(builder.ToString(), j);
                            if ((flag && (bidiCategory == BidiCategory.LeftToRight)) || (!flag && ((bidiCategory == BidiCategory.RightToLeft) || (bidiCategory == BidiCategory.RightToLeftArabic))))
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadBidi"), "ascii");
                            }
                        }
                    }
                    if ((flag && (bidiCategory != BidiCategory.RightToLeft)) && (bidiCategory != BidiCategory.RightToLeftArabic))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadBidi"), "ascii");
                    }
                }
                if ((index - startIndex) > 0x3f)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "ascii");
                }
                if (index != ascii.Length)
                {
                    builder.Append('.');
                }
                startIndex = index + 1;
            }
            if (builder.Length > (0xff - (IsDot(builder[builder.Length - 1]) ? 0 : 1)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadNameSize", new object[] { 0xff - (IsDot(builder[builder.Length - 1]) ? 0 : 1) }), "ascii");
            }
            return builder.ToString();
        }

        private static string punycode_encode(string unicode)
        {
            if (unicode.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "unicode");
            }
            StringBuilder builder = new StringBuilder(unicode.Length);
            int length = 0;
            int startIndex = 0;
            for (int i = 0; length < unicode.Length; i = builder.Length)
            {
                length = unicode.IndexOfAny(M_Dots, startIndex);
                if (length < 0)
                {
                    length = unicode.Length;
                }
                if (length == startIndex)
                {
                    if (length != unicode.Length)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "unicode");
                    }
                    break;
                }
                builder.Append("xn--");
                bool flag = false;
                BidiCategory bidiCategory = CharUnicodeInfo.GetBidiCategory(unicode, startIndex);
                switch (bidiCategory)
                {
                    case BidiCategory.RightToLeft:
                    case BidiCategory.RightToLeftArabic:
                    {
                        flag = true;
                        int index = length - 1;
                        if (char.IsLowSurrogate(unicode, index))
                        {
                            index--;
                        }
                        bidiCategory = CharUnicodeInfo.GetBidiCategory(unicode, index);
                        if ((bidiCategory != BidiCategory.RightToLeft) && (bidiCategory != BidiCategory.RightToLeftArabic))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadBidi"), "unicode");
                        }
                        break;
                    }
                }
                int num6 = 0;
                for (int j = startIndex; j < length; j++)
                {
                    BidiCategory category2 = CharUnicodeInfo.GetBidiCategory(unicode, j);
                    if (flag && (category2 == BidiCategory.LeftToRight))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadBidi"), "unicode");
                    }
                    if (!flag && ((category2 == BidiCategory.RightToLeft) || (category2 == BidiCategory.RightToLeftArabic)))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadBidi"), "unicode");
                    }
                    if (basic(unicode[j]))
                    {
                        builder.Append(encode_basic(unicode[j]));
                        num6++;
                    }
                    else if (char.IsSurrogatePair(unicode, j))
                    {
                        j++;
                    }
                }
                int num7 = num6;
                if (num7 == (length - startIndex))
                {
                    builder.Remove(i, "xn--".Length);
                }
                else
                {
                    if (((unicode.Length - startIndex) >= "xn--".Length) && unicode.Substring(startIndex, "xn--".Length).Equals("xn--", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadPunycode"), "unicode");
                    }
                    int num8 = 0;
                    if (num7 > 0)
                    {
                        builder.Append('-');
                    }
                    int num9 = 0x80;
                    int delta = 0;
                    int num11 = 0x48;
                    while (num6 < (length - startIndex))
                    {
                        int cTest = 0;
                        int num13 = 0x7ffffff;
                        int num12 = startIndex;
                        while (num12 < length)
                        {
                            cTest = char.ConvertToUtf32(unicode, num12);
                            if ((cTest >= num9) && (cTest < num13))
                            {
                                num13 = cTest;
                            }
                            num12 += IsSupplementary(cTest) ? 2 : 1;
                        }
                        delta += (num13 - num9) * ((num6 - num8) + 1);
                        num9 = num13;
                        for (num12 = startIndex; num12 < length; num12 += IsSupplementary(cTest) ? 2 : 1)
                        {
                            cTest = char.ConvertToUtf32(unicode, num12);
                            if (cTest < num9)
                            {
                                delta++;
                            }
                            if (cTest == num9)
                            {
                                int d = delta;
                                int num16 = 0x24;
                                while (true)
                                {
                                    int num17 = (num16 <= num11) ? 1 : ((num16 >= (num11 + 0x1a)) ? 0x1a : (num16 - num11));
                                    if (d < num17)
                                    {
                                        break;
                                    }
                                    builder.Append(encode_digit(num17 + ((d - num17) % (0x24 - num17))));
                                    d = (d - num17) / (0x24 - num17);
                                    num16 += 0x24;
                                }
                                builder.Append(encode_digit(d));
                                num11 = adapt(delta, (num6 - num8) + 1, num6 == num7);
                                delta = 0;
                                num6++;
                                if (IsSupplementary(num13))
                                {
                                    num6++;
                                    num8++;
                                }
                            }
                        }
                        delta++;
                        num9++;
                    }
                }
                if ((builder.Length - i) > 0x3f)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "unicode");
                }
                if (length != unicode.Length)
                {
                    builder.Append('.');
                }
                startIndex = length + 1;
            }
            if (builder.Length > (0xff - (IsDot(unicode[unicode.Length - 1]) ? 0 : 1)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadNameSize", new object[] { 0xff - (IsDot(unicode[unicode.Length - 1]) ? 0 : 1) }), "unicode");
            }
            return builder.ToString();
        }

        private static void ValidateStd3(char c, bool bNextToDot)
        {
            if (((((c <= ',') || (c == '/')) || ((c >= ':') && (c <= '@'))) || ((c >= '[') && (c <= '`'))) || (((c >= '{') && (c <= '\x007f')) || ((c == '-') && bNextToDot)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadStd3", new object[] { c }), "Unicode");
            }
        }

        private static bool ValidateStd3AndAscii(string unicode, bool bUseStd3, bool bCheckAscii)
        {
            if (unicode.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "unicode");
            }
            int num = -1;
            for (int i = 0; i < unicode.Length; i++)
            {
                if (unicode[i] <= '\x001f')
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequence", new object[] { i }), "unicode");
                }
                if (bCheckAscii && (unicode[i] >= '\x007f'))
                {
                    return false;
                }
                if (IsDot(unicode[i]))
                {
                    if (i == (num + 1))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "unicode");
                    }
                    if ((i - num) > 0x40)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "Unicode");
                    }
                    if (bUseStd3 && (i > 0))
                    {
                        ValidateStd3(unicode[i - 1], true);
                    }
                    num = i;
                }
                else if (bUseStd3)
                {
                    ValidateStd3(unicode[i], i == (num + 1));
                }
            }
            if ((num == -1) && (unicode.Length > 0x3f))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadLabelSize"), "unicode");
            }
            if (unicode.Length > (0xff - (IsDot(unicode[unicode.Length - 1]) ? 0 : 1)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IdnBadNameSize", new object[] { 0xff - (IsDot(unicode[unicode.Length - 1]) ? 0 : 1) }), "unicode");
            }
            if (bUseStd3 && !IsDot(unicode[unicode.Length - 1]))
            {
                ValidateStd3(unicode[unicode.Length - 1], true);
            }
            return true;
        }

        public bool AllowUnassigned
        {
            get
            {
                return this.m_bAllowUnassigned;
            }
            set
            {
                this.m_bAllowUnassigned = value;
            }
        }

        public bool UseStd3AsciiRules
        {
            get
            {
                return this.m_bUseStd3AsciiRules;
            }
            set
            {
                this.m_bUseStd3AsciiRules = value;
            }
        }
    }
}


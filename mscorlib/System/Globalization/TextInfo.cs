namespace System.Globalization
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;

    [Serializable, ComVisible(true)]
    public class TextInfo : ICloneable, IDeserializationCallback
    {
        [OptionalField(VersionAdded=2)]
        private string customCultureName;
        internal static TextInfo Invariant = new TextInfo(CultureData.Invariant);
        [NonSerialized]
        private CultureData m_cultureData;
        [OptionalField(VersionAdded=3)]
        private string m_cultureName;
        [NonSerialized]
        private IntPtr m_dataHandle;
        [NonSerialized]
        private bool? m_IsAsciiCasingSameAsInvariant;
        [OptionalField(VersionAdded=2)]
        private bool m_isReadOnly;
        [OptionalField(VersionAdded=2)]
        private string m_listSeparator;
        [OptionalField(VersionAdded=1)]
        internal int m_nDataItem;
        [NonSerialized]
        private string m_textInfoName;
        [OptionalField(VersionAdded=1)]
        internal bool m_useUserOverride;
        [OptionalField(VersionAdded=1)]
        internal int m_win32LangID;
        private const int wordSeparatorMask = 0x1ffcf800;

        [SecuritySafeCritical]
        internal TextInfo(CultureData cultureData)
        {
            this.m_cultureData = cultureData;
            this.m_cultureName = this.m_cultureData.CultureName;
            this.m_textInfoName = this.m_cultureData.STEXTINFO;
            this.m_dataHandle = CompareInfo.InternalInitSortHandle(this.m_textInfoName);
        }

        private static int AddNonLetter(ref StringBuilder result, ref string input, int inputIndex, int charLen)
        {
            if (charLen == 2)
            {
                result.Append(input[inputIndex++]);
                result.Append(input[inputIndex]);
                return inputIndex;
            }
            result.Append(input[inputIndex]);
            return inputIndex;
        }

        private int AddTitlecaseLetter(ref StringBuilder result, ref string input, int inputIndex, int charLen)
        {
            if (charLen == 2)
            {
                result.Append(this.ToUpper(input.Substring(inputIndex, charLen)));
                inputIndex++;
                return inputIndex;
            }
            switch (input[inputIndex])
            {
                case 'Ǆ':
                case 'ǅ':
                case 'ǆ':
                    result.Append('ǅ');
                    return inputIndex;

                case 'Ǉ':
                case 'ǈ':
                case 'ǉ':
                    result.Append('ǈ');
                    return inputIndex;

                case 'Ǌ':
                case 'ǋ':
                case 'ǌ':
                    result.Append('ǋ');
                    return inputIndex;

                case 'Ǳ':
                case 'ǲ':
                case 'ǳ':
                    result.Append('ǲ');
                    return inputIndex;
            }
            result.Append(this.ToUpper(input[inputIndex]));
            return inputIndex;
        }

        [SecuritySafeCritical, ComVisible(false)]
        public virtual object Clone()
        {
            object obj2 = base.MemberwiseClone();
            ((TextInfo) obj2).SetReadOnlyState(false);
            return obj2;
        }

        [SecuritySafeCritical]
        internal static int CompareOrdinalIgnoreCase(string str1, string str2)
        {
            return InternalCompareStringOrdinalIgnoreCase(str1, 0, str2, 0, str1.Length, str2.Length);
        }

        [SecuritySafeCritical]
        internal static int CompareOrdinalIgnoreCaseEx(string strA, int indexA, string strB, int indexB, int lengthA, int lengthB)
        {
            return InternalCompareStringOrdinalIgnoreCase(strA, indexA, strB, indexB, lengthA, lengthB);
        }

        public override bool Equals(object obj)
        {
            TextInfo info = obj as TextInfo;
            return ((info != null) && this.CultureName.Equals(info.CultureName));
        }

        [SecuritySafeCritical]
        internal int GetCaseInsensitiveHashCode(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return InternalGetCaseInsHash(this.m_dataHandle, this.m_textInfoName, str);
        }

        public override int GetHashCode()
        {
            return this.CultureName.GetHashCode();
        }

        [SecuritySafeCritical]
        internal static int GetHashCodeOrdinalIgnoreCase(string s)
        {
            return Invariant.GetCaseInsensitiveHashCode(s);
        }

        [SecuritySafeCritical]
        internal static int IndexOfStringOrdinalIgnoreCase(string source, string value, int startIndex, int count)
        {
            if ((source.Length == 0) && (value.Length == 0))
            {
                return 0;
            }
            int num = startIndex + count;
            int num2 = num - value.Length;
            while (startIndex <= num2)
            {
                if (CompareOrdinalIgnoreCaseEx(source, startIndex, value, 0, value.Length, value.Length) == 0)
                {
                    return startIndex;
                }
                startIndex++;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern char InternalChangeCaseChar(IntPtr handle, string localeName, char ch, bool isToUpper);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern string InternalChangeCaseString(IntPtr handle, string localeName, string str, bool isToUpper);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int InternalCompareStringOrdinalIgnoreCase(string string1, int index1, string string2, int index2, int length1, int length2);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int InternalGetCaseInsHash(IntPtr handle, string localeName, string str);
        private static bool IsAscii(char c)
        {
            return (c < '\x0080');
        }

        private static bool IsLetterCategory(UnicodeCategory uc)
        {
            if (((uc != UnicodeCategory.UppercaseLetter) && (uc != UnicodeCategory.LowercaseLetter)) && ((uc != UnicodeCategory.TitlecaseLetter) && (uc != UnicodeCategory.ModifierLetter)))
            {
                return (uc == UnicodeCategory.OtherLetter);
            }
            return true;
        }

        private static bool IsWordSeparator(UnicodeCategory category)
        {
            return ((0x1ffcf800 & (((int) 1) << category)) != 0);
        }

        [SecuritySafeCritical]
        internal static int LastIndexOfStringOrdinalIgnoreCase(string source, string value, int startIndex, int count)
        {
            if (value.Length == 0)
            {
                return startIndex;
            }
            int num = (startIndex - count) + 1;
            if (value.Length > 0)
            {
                startIndex -= value.Length - 1;
            }
            while (startIndex >= num)
            {
                if (CompareOrdinalIgnoreCaseEx(source, startIndex, value, 0, value.Length, value.Length) == 0)
                {
                    return startIndex;
                }
                startIndex--;
            }
            return -1;
        }

        private void OnDeserialized()
        {
            if (this.m_cultureData == null)
            {
                if (this.m_cultureName == null)
                {
                    if (this.customCultureName != null)
                    {
                        this.m_cultureName = this.customCultureName;
                    }
                    else
                    {
                        this.m_cultureName = CultureInfo.GetCultureInfo(this.m_win32LangID).m_cultureData.CultureName;
                    }
                }
                this.m_cultureData = CultureInfo.GetCultureInfo(this.m_cultureName).m_cultureData;
                this.m_textInfoName = this.m_cultureData.STEXTINFO;
                this.m_dataHandle = CompareInfo.InternalInitSortHandle(this.m_textInfoName);
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.OnDeserialized();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.m_cultureData = null;
            this.m_cultureName = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this.m_useUserOverride = false;
            this.customCultureName = this.m_cultureName;
            this.m_win32LangID = CultureInfo.GetCultureInfo(this.m_cultureName).LCID;
        }

        [ComVisible(false), SecuritySafeCritical]
        public static TextInfo ReadOnly(TextInfo textInfo)
        {
            if (textInfo == null)
            {
                throw new ArgumentNullException("textInfo");
            }
            if (textInfo.IsReadOnly)
            {
                return textInfo;
            }
            TextInfo info = (TextInfo) textInfo.MemberwiseClone();
            info.SetReadOnlyState(true);
            return info;
        }

        internal void SetReadOnlyState(bool readOnly)
        {
            this.m_isReadOnly = readOnly;
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            this.OnDeserialized();
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual char ToLower(char c)
        {
            if (IsAscii(c) && this.IsAsciiCasingSameAsInvariant)
            {
                return ToLowerAsciiInvariant(c);
            }
            return InternalChangeCaseChar(this.m_dataHandle, this.m_textInfoName, c, false);
        }

        [SecuritySafeCritical]
        public virtual string ToLower(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return InternalChangeCaseString(this.m_dataHandle, this.m_textInfoName, str, false);
        }

        private static char ToLowerAsciiInvariant(char c)
        {
            if (('A' <= c) && (c <= 'Z'))
            {
                c = (char) (c | ' ');
            }
            return c;
        }

        public override string ToString()
        {
            return ("TextInfo - " + this.m_cultureData.CultureName);
        }

        public string ToTitleCase(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Length == 0)
            {
                return str;
            }
            StringBuilder result = new StringBuilder();
            string str2 = null;
            for (int i = 0; i < str.Length; i++)
            {
                int num2;
                UnicodeCategory uc = CharUnicodeInfo.InternalGetUnicodeCategory(str, i, out num2);
                if (char.CheckLetter(uc))
                {
                    i = this.AddTitlecaseLetter(ref result, ref str, i, num2) + 1;
                    int startIndex = i;
                    bool flag = uc == UnicodeCategory.LowercaseLetter;
                    while (i < str.Length)
                    {
                        uc = CharUnicodeInfo.InternalGetUnicodeCategory(str, i, out num2);
                        if (IsLetterCategory(uc))
                        {
                            if (uc == UnicodeCategory.LowercaseLetter)
                            {
                                flag = true;
                            }
                            i += num2;
                        }
                        else
                        {
                            if (str[i] == '\'')
                            {
                                i++;
                                if (flag)
                                {
                                    if (str2 == null)
                                    {
                                        str2 = this.ToLower(str);
                                    }
                                    result.Append(str2, startIndex, i - startIndex);
                                }
                                else
                                {
                                    result.Append(str, startIndex, i - startIndex);
                                }
                                startIndex = i;
                                flag = true;
                                continue;
                            }
                            if (IsWordSeparator(uc))
                            {
                                break;
                            }
                            i += num2;
                        }
                    }
                    int count = i - startIndex;
                    if (count > 0)
                    {
                        if (flag)
                        {
                            if (str2 == null)
                            {
                                str2 = this.ToLower(str);
                            }
                            result.Append(str2, startIndex, count);
                        }
                        else
                        {
                            result.Append(str, startIndex, count);
                        }
                    }
                    if (i < str.Length)
                    {
                        i = AddNonLetter(ref result, ref str, i, num2);
                    }
                    continue;
                }
                i = AddNonLetter(ref result, ref str, i, num2);
            }
            return result.ToString();
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual char ToUpper(char c)
        {
            if (IsAscii(c) && this.IsAsciiCasingSameAsInvariant)
            {
                return ToUpperAsciiInvariant(c);
            }
            return InternalChangeCaseChar(this.m_dataHandle, this.m_textInfoName, c, true);
        }

        [SecuritySafeCritical]
        public virtual string ToUpper(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return InternalChangeCaseString(this.m_dataHandle, this.m_textInfoName, str, true);
        }

        private static char ToUpperAsciiInvariant(char c)
        {
            if (('a' <= c) && (c <= 'z'))
            {
                c = (char) (c & '￟');
            }
            return c;
        }

        private void VerifyWritable()
        {
            if (this.m_isReadOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
            }
        }

        public virtual int ANSICodePage
        {
            get
            {
                return this.m_cultureData.IDEFAULTANSICODEPAGE;
            }
        }

        [ComVisible(false)]
        public string CultureName
        {
            get
            {
                return this.m_textInfoName;
            }
        }

        public virtual int EBCDICCodePage
        {
            get
            {
                return this.m_cultureData.IDEFAULTEBCDICCODEPAGE;
            }
        }

        private bool IsAsciiCasingSameAsInvariant
        {
            get
            {
                if (!this.m_IsAsciiCasingSameAsInvariant.HasValue)
                {
                    this.m_IsAsciiCasingSameAsInvariant = new bool?(CultureInfo.GetCultureInfo(this.m_textInfoName).CompareInfo.Compare("abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", CompareOptions.IgnoreCase) == 0);
                }
                return this.m_IsAsciiCasingSameAsInvariant.Value;
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
        public bool IsRightToLeft
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.m_cultureData.IsRightToLeft;
            }
        }

        [ComVisible(false)]
        public int LCID
        {
            get
            {
                return CultureInfo.GetCultureInfo(this.m_textInfoName).LCID;
            }
        }

        public virtual string ListSeparator
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_listSeparator == null)
                {
                    this.m_listSeparator = this.m_cultureData.SLIST;
                }
                return this.m_listSeparator;
            }
            [ComVisible(false)]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.m_listSeparator = value;
            }
        }

        public virtual int MacCodePage
        {
            get
            {
                return this.m_cultureData.IDEFAULTMACCODEPAGE;
            }
        }

        public virtual int OEMCodePage
        {
            get
            {
                return this.m_cultureData.IDEFAULTOEMCODEPAGE;
            }
        }
    }
}


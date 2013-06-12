namespace System.Globalization
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class CompareInfo : IDeserializationCallback
    {
        private const int COMPARE_OPTIONS_ORDINAL = 0x40000000;
        private int culture;
        private const int LINGUISTIC_IGNORECASE = 0x10;
        private const int LINGUISTIC_IGNOREDIACRITIC = 0x20;
        [NonSerialized]
        private IntPtr m_dataHandle;
        [OptionalField(VersionAdded=2)]
        private string m_name;
        [NonSerialized]
        private string m_sortName;
        private const int NORM_IGNORECASE = 1;
        private const int NORM_IGNOREKANATYPE = 0x10000;
        private const int NORM_IGNORENONSPACE = 2;
        private const int NORM_IGNORESYMBOLS = 4;
        private const int NORM_IGNOREWIDTH = 0x20000;
        internal const int NORM_LINGUISTIC_CASING = 0x8000000;
        private const int SORT_STRINGSORT = 0x1000;
        private const int SORT_VERSION_WHIDBEY = 0x1000;
        private const CompareOptions ValidCompareMaskOffFlags = ~(CompareOptions.StringSort | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
        private const CompareOptions ValidHashCodeOfStringMaskOffFlags = ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
        private const CompareOptions ValidIndexMaskOffFlags = ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
        [OptionalField(VersionAdded=1)]
        private int win32LCID;

        [SecuritySafeCritical]
        internal CompareInfo(CultureInfo culture)
        {
            this.m_name = culture.m_name;
            this.m_sortName = culture.SortName;
            this.m_dataHandle = InternalInitSortHandle(this.m_sortName);
        }

        public virtual int Compare(string string1, string string2)
        {
            return this.Compare(string1, string2, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual int Compare(string string1, string string2, CompareOptions options)
        {
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                return string.Compare(string1, string2, StringComparison.OrdinalIgnoreCase);
            }
            if ((options & CompareOptions.Ordinal) != CompareOptions.None)
            {
                if (options != CompareOptions.Ordinal)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_CompareOptionOrdinal"), "options");
                }
                return string.CompareOrdinal(string1, string2);
            }
            if ((options & ~(CompareOptions.StringSort | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            if (string1 == null)
            {
                if (string2 == null)
                {
                    return 0;
                }
                return -1;
            }
            if (string2 == null)
            {
                return 1;
            }
            return InternalCompareString(this.m_dataHandle, this.m_sortName, string1, 0, string1.Length, string2, 0, string2.Length, GetNativeCompareFlags(options));
        }

        public virtual int Compare(string string1, int offset1, string string2, int offset2)
        {
            return this.Compare(string1, offset1, string2, offset2, CompareOptions.None);
        }

        public virtual int Compare(string string1, int offset1, string string2, int offset2, CompareOptions options)
        {
            return this.Compare(string1, offset1, (string1 == null) ? 0 : (string1.Length - offset1), string2, offset2, (string2 == null) ? 0 : (string2.Length - offset2), options);
        }

        public virtual int Compare(string string1, int offset1, int length1, string string2, int offset2, int length2)
        {
            return this.Compare(string1, offset1, length1, string2, offset2, length2, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual int Compare(string string1, int offset1, int length1, string string2, int offset2, int length2, CompareOptions options)
        {
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                int num = string.Compare(string1, offset1, string2, offset2, (length1 < length2) ? length1 : length2, StringComparison.OrdinalIgnoreCase);
                if ((length1 == length2) || (num != 0))
                {
                    return num;
                }
                if (length1 <= length2)
                {
                    return -1;
                }
                return 1;
            }
            if ((length1 < 0) || (length2 < 0))
            {
                throw new ArgumentOutOfRangeException((length1 < 0) ? "length1" : "length2", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if ((offset1 < 0) || (offset2 < 0))
            {
                throw new ArgumentOutOfRangeException((offset1 < 0) ? "offset1" : "offset2", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (offset1 > (((string1 == null) ? 0 : string1.Length) - length1))
            {
                throw new ArgumentOutOfRangeException("string1", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
            }
            if (offset2 > (((string2 == null) ? 0 : string2.Length) - length2))
            {
                throw new ArgumentOutOfRangeException("string2", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
            }
            if ((options & CompareOptions.Ordinal) != CompareOptions.None)
            {
                if (options != CompareOptions.Ordinal)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_CompareOptionOrdinal"), "options");
                }
            }
            else if ((options & ~(CompareOptions.StringSort | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            if (string1 == null)
            {
                if (string2 == null)
                {
                    return 0;
                }
                return -1;
            }
            if (string2 == null)
            {
                return 1;
            }
            if (options == CompareOptions.Ordinal)
            {
                return CompareOrdinal(string1, offset1, length1, string2, offset2, length2);
            }
            return InternalCompareString(this.m_dataHandle, this.m_sortName, string1, offset1, length1, string2, offset2, length2, GetNativeCompareFlags(options));
        }

        [SecurityCritical]
        private static int CompareOrdinal(string string1, int offset1, int length1, string string2, int offset2, int length2)
        {
            int num = string.nativeCompareOrdinalEx(string1, offset1, string2, offset2, (length1 < length2) ? length1 : length2);
            if ((length1 == length2) || (num != 0))
            {
                return num;
            }
            if (length1 <= length2)
            {
                return -1;
            }
            return 1;
        }

        [SecuritySafeCritical]
        private SortKey CreateSortKey(string source, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((options & ~(CompareOptions.StringSort | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            byte[] target = null;
            if (string.IsNullOrEmpty(source))
            {
                target = new byte[0];
                source = "\0";
            }
            int nativeCompareFlags = GetNativeCompareFlags(options);
            int num2 = InternalGetSortKey(this.m_dataHandle, this.m_sortName, nativeCompareFlags, source, source.Length, null, 0);
            if (num2 == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "source");
            }
            if (target == null)
            {
                target = new byte[num2];
                num2 = InternalGetSortKey(this.m_dataHandle, this.m_sortName, nativeCompareFlags, source, source.Length, target, target.Length);
            }
            else
            {
                source = string.Empty;
            }
            return new SortKey(this.Name, source, options, target);
        }

        public override bool Equals(object value)
        {
            CompareInfo info = value as CompareInfo;
            return ((info != null) && (this.Name == info.Name));
        }

        public static CompareInfo GetCompareInfo(int culture)
        {
            if (CultureData.IsCustomCultureId(culture))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_CustomCultureCannotBePassedByNumber", new object[] { "culture" }));
            }
            return CultureInfo.GetCultureInfo(culture).CompareInfo;
        }

        [SecuritySafeCritical]
        public static CompareInfo GetCompareInfo(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return CultureInfo.GetCultureInfo(name).CompareInfo;
        }

        public static CompareInfo GetCompareInfo(int culture, Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (assembly != typeof(object).Module.Assembly)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_OnlyMscorlib"));
            }
            return GetCompareInfo(culture);
        }

        public static CompareInfo GetCompareInfo(string name, Assembly assembly)
        {
            if ((name == null) || (assembly == null))
            {
                throw new ArgumentNullException((name == null) ? "name" : "assembly");
            }
            if (assembly != typeof(object).Module.Assembly)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_OnlyMscorlib"));
            }
            return GetCompareInfo(name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        [SecuritySafeCritical]
        internal int GetHashCodeOfString(string source, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((options & ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            if (source.Length == 0)
            {
                return 0;
            }
            return InternalGetGlobalizedHashCode(this.m_dataHandle, this.m_sortName, source, source.Length, GetNativeCompareFlags(options));
        }

        internal static int GetNativeCompareFlags(CompareOptions options)
        {
            int num = 0x8000000;
            if ((options & CompareOptions.IgnoreCase) != CompareOptions.None)
            {
                num |= 1;
            }
            if ((options & CompareOptions.IgnoreKanaType) != CompareOptions.None)
            {
                num |= 0x10000;
            }
            if ((options & CompareOptions.IgnoreNonSpace) != CompareOptions.None)
            {
                num |= 2;
            }
            if ((options & CompareOptions.IgnoreSymbols) != CompareOptions.None)
            {
                num |= 4;
            }
            if ((options & CompareOptions.IgnoreWidth) != CompareOptions.None)
            {
                num |= 0x20000;
            }
            if ((options & CompareOptions.StringSort) != CompareOptions.None)
            {
                num |= 0x1000;
            }
            if (options == CompareOptions.Ordinal)
            {
                num = 0x40000000;
            }
            return num;
        }

        public virtual SortKey GetSortKey(string source)
        {
            return this.CreateSortKey(source, CompareOptions.None);
        }

        public virtual SortKey GetSortKey(string source, CompareOptions options)
        {
            return this.CreateSortKey(source, options);
        }

        public virtual int IndexOf(string source, char value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, 0, source.Length, CompareOptions.None);
        }

        public virtual int IndexOf(string source, string value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, 0, source.Length, CompareOptions.None);
        }

        public virtual int IndexOf(string source, char value, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, 0, source.Length, options);
        }

        public virtual int IndexOf(string source, char value, int startIndex)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, startIndex, source.Length - startIndex, CompareOptions.None);
        }

        public virtual int IndexOf(string source, string value, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, 0, source.Length, options);
        }

        public virtual int IndexOf(string source, string value, int startIndex)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, startIndex, source.Length - startIndex, CompareOptions.None);
        }

        public virtual int IndexOf(string source, char value, int startIndex, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, startIndex, source.Length - startIndex, options);
        }

        public virtual int IndexOf(string source, char value, int startIndex, int count)
        {
            return this.IndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        public virtual int IndexOf(string source, string value, int startIndex, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.IndexOf(source, value, startIndex, source.Length - startIndex, options);
        }

        public virtual int IndexOf(string source, string value, int startIndex, int count)
        {
            return this.IndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual int IndexOf(string source, char value, int startIndex, int count, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (startIndex > (source.Length - count)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                return TextInfo.IndexOfStringOrdinalIgnoreCase(source, value.ToString(), startIndex, count);
            }
            if (((options & ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None) && (options != CompareOptions.Ordinal))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            return InternalFindNLSStringEx(this.m_dataHandle, this.m_sortName, GetNativeCompareFlags(options) | 0x400000, source, count, startIndex, new string(value, 1), 1);
        }

        [SecuritySafeCritical]
        public virtual int IndexOf(string source, string value, int startIndex, int count, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (startIndex > source.Length)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (source.Length == 0)
            {
                if (value.Length == 0)
                {
                    return 0;
                }
                return -1;
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (startIndex > (source.Length - count)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                return TextInfo.IndexOfStringOrdinalIgnoreCase(source, value, startIndex, count);
            }
            if (((options & ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None) && (options != CompareOptions.Ordinal))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            return InternalFindNLSStringEx(this.m_dataHandle, this.m_sortName, GetNativeCompareFlags(options) | 0x400000, source, count, startIndex, value, value.Length);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int InternalCompareString(IntPtr handle, string localeName, string string1, int offset1, int length1, string string2, int offset2, int length2, int flags);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int InternalFindNLSStringEx(IntPtr handle, string localeName, int flags, string source, int sourceCount, int startIndex, string target, int targetCount);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int InternalGetGlobalizedHashCode(IntPtr handle, string localeName, string source, int length, int dwFlags);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern int InternalGetSortKey(IntPtr handle, string localeName, int flags, string source, int sourceCount, byte[] target, int targetCount);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern uint InternalGetSortVersion();
        [SecuritySafeCritical]
        internal static IntPtr InternalInitSortHandle(string localeName)
        {
            return InternalInitSortHandle(localeName, Version);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern IntPtr InternalInitSortHandle(string localeName, uint version);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool InternalIsSortable(IntPtr handle, string source, int length);
        public virtual bool IsPrefix(string source, string prefix)
        {
            return this.IsPrefix(source, prefix, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual bool IsPrefix(string source, string prefix, CompareOptions options)
        {
            if ((source == null) || (prefix == null))
            {
                throw new ArgumentNullException((source == null) ? "source" : "prefix", Environment.GetResourceString("ArgumentNull_String"));
            }
            if (prefix.Length == 0)
            {
                return true;
            }
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                return source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }
            if (options == CompareOptions.Ordinal)
            {
                return source.StartsWith(prefix, StringComparison.Ordinal);
            }
            if ((options & ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            return (InternalFindNLSStringEx(this.m_dataHandle, this.m_sortName, GetNativeCompareFlags(options) | 0x100000, source, source.Length, 0, prefix, prefix.Length) > -1);
        }

        [ComVisible(false)]
        public static bool IsSortable(char ch)
        {
            return IsSortable(ch.ToString());
        }

        [SecuritySafeCritical, ComVisible(false)]
        public static bool IsSortable(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (text.Length == 0)
            {
                return false;
            }
            return InternalIsSortable(CultureInfo.InvariantCulture.CompareInfo.m_dataHandle, text, text.Length);
        }

        public virtual bool IsSuffix(string source, string suffix)
        {
            return this.IsSuffix(source, suffix, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual bool IsSuffix(string source, string suffix, CompareOptions options)
        {
            if ((source == null) || (suffix == null))
            {
                throw new ArgumentNullException((source == null) ? "source" : "suffix", Environment.GetResourceString("ArgumentNull_String"));
            }
            if (suffix.Length == 0)
            {
                return true;
            }
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                return source.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
            }
            if (options == CompareOptions.Ordinal)
            {
                return source.EndsWith(suffix, StringComparison.Ordinal);
            }
            if ((options & ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            return (InternalFindNLSStringEx(this.m_dataHandle, this.m_sortName, GetNativeCompareFlags(options) | 0x200000, source, source.Length, source.Length - 1, suffix, suffix.Length) >= 0);
        }

        public virtual int LastIndexOf(string source, char value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.LastIndexOf(source, value, source.Length - 1, source.Length, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual int LastIndexOf(string source, string value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.LastIndexOf(source, value, source.Length - 1, source.Length, CompareOptions.None);
        }

        public virtual int LastIndexOf(string source, char value, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.LastIndexOf(source, value, source.Length - 1, source.Length, options);
        }

        public virtual int LastIndexOf(string source, char value, int startIndex)
        {
            return this.LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual int LastIndexOf(string source, string value, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.LastIndexOf(source, value, source.Length - 1, source.Length, options);
        }

        public virtual int LastIndexOf(string source, string value, int startIndex)
        {
            return this.LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
        }

        public virtual int LastIndexOf(string source, char value, int startIndex, CompareOptions options)
        {
            return this.LastIndexOf(source, value, startIndex, startIndex + 1, options);
        }

        public virtual int LastIndexOf(string source, char value, int startIndex, int count)
        {
            return this.LastIndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        public virtual int LastIndexOf(string source, string value, int startIndex, CompareOptions options)
        {
            return this.LastIndexOf(source, value, startIndex, startIndex + 1, options);
        }

        public virtual int LastIndexOf(string source, string value, int startIndex, int count)
        {
            return this.LastIndexOf(source, value, startIndex, count, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public virtual int LastIndexOf(string source, char value, int startIndex, int count, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((((options & ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None) && (options != CompareOptions.Ordinal)) && (options != CompareOptions.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            if ((source.Length == 0) && ((startIndex == -1) || (startIndex == 0)))
            {
                return -1;
            }
            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (startIndex == source.Length)
            {
                startIndex--;
                if (count > 0)
                {
                    count--;
                }
            }
            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                return TextInfo.LastIndexOfStringOrdinalIgnoreCase(source, value.ToString(), startIndex, count);
            }
            return InternalFindNLSStringEx(this.m_dataHandle, this.m_sortName, GetNativeCompareFlags(options) | 0x800000, source, count, startIndex, new string(value, 1), 1);
        }

        [SecuritySafeCritical]
        public virtual int LastIndexOf(string source, string value, int startIndex, int count, CompareOptions options)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((((options & ~(CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase)) != CompareOptions.None) && (options != CompareOptions.Ordinal)) && (options != CompareOptions.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
            }
            if ((source.Length == 0) && ((startIndex == -1) || (startIndex == 0)))
            {
                if (value.Length != 0)
                {
                    return -1;
                }
                return 0;
            }
            if ((startIndex < 0) || (startIndex > source.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (startIndex == source.Length)
            {
                startIndex--;
                if (count > 0)
                {
                    count--;
                }
                if (((value.Length == 0) && (count >= 0)) && (((startIndex - count) + 1) >= 0))
                {
                    return startIndex;
                }
            }
            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            if (options == CompareOptions.OrdinalIgnoreCase)
            {
                return TextInfo.LastIndexOfStringOrdinalIgnoreCase(source, value, startIndex, count);
            }
            return InternalFindNLSStringEx(this.m_dataHandle, this.m_sortName, GetNativeCompareFlags(options) | 0x800000, source, count, startIndex, value, value.Length);
        }

        [SecuritySafeCritical]
        private void OnDeserialized()
        {
            CultureInfo cultureInfo;
            if (this.m_name == null)
            {
                cultureInfo = CultureInfo.GetCultureInfo(this.culture);
                this.m_name = cultureInfo.m_name;
            }
            else
            {
                cultureInfo = CultureInfo.GetCultureInfo(this.m_name);
            }
            this.m_sortName = cultureInfo.SortName;
            this.m_dataHandle = InternalInitSortHandle(this.m_sortName);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this.OnDeserialized();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.m_name = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this.culture = CultureInfo.GetCultureInfo(this.Name).LCID;
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            this.OnDeserialized();
        }

        public override string ToString()
        {
            return ("CompareInfo - " + this.Name);
        }

        internal static bool IsLegacy20SortingBehaviorRequested
        {
            get
            {
                return (Version == 0x1000);
            }
        }

        public int LCID
        {
            get
            {
                return CultureInfo.GetCultureInfo(this.Name).LCID;
            }
        }

        [ComVisible(false)]
        public virtual string Name
        {
            get
            {
                if (!(this.m_name == "zh-CHT") && !(this.m_name == "zh-CHS"))
                {
                    return this.m_sortName;
                }
                return this.m_name;
            }
        }

        private static uint Version
        {
            [SecuritySafeCritical]
            get
            {
                bool? nullable = AppDomain.CurrentDomain.IsCompatibilitySwitchSet("NetFx40_Legacy20SortingBehavior");
                if (nullable.HasValue && nullable.Value)
                {
                    return 0x1000;
                }
                return InternalGetSortVersion();
            }
        }
    }
}


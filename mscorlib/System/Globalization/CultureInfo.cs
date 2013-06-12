namespace System.Globalization
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public class CultureInfo : ICloneable, IFormatProvider
    {
        internal System.Globalization.Calendar calendar;
        internal System.Globalization.CompareInfo compareInfo;
        [OptionalField(VersionAdded=1)]
        internal int cultureID;
        internal DateTimeFormatInfo dateTimeInfo;
        private static readonly bool init = Init();
        internal const int LOCALE_CUSTOM_DEFAULT = 0xc00;
        internal const int LOCALE_CUSTOM_UNSPECIFIED = 0x1000;
        internal const int LOCALE_INVARIANT = 0x7f;
        internal const int LOCALE_NEUTRAL = 0;
        private const int LOCALE_SORTID_MASK = 0xf0000;
        private const int LOCALE_SYSTEM_DEFAULT = 0x800;
        private const int LOCALE_TRADITIONAL_SPANISH = 0x40a;
        private const int LOCALE_USER_DEFAULT = 0x400;
        [NonSerialized]
        private CultureInfo m_consoleFallbackCulture;
        [NonSerialized]
        private int m_createdDomainID;
        [NonSerialized]
        internal CultureData m_cultureData;
        [OptionalField(VersionAdded=1)]
        internal int m_dataItem;
        [NonSerialized]
        internal bool m_isInherited;
        internal bool m_isReadOnly;
        [NonSerialized]
        private bool m_isSafeCrossDomain;
        internal string m_name;
        [NonSerialized]
        private string m_nonSortName;
        [NonSerialized]
        private CultureInfo m_parent;
        [NonSerialized]
        private string m_sortName;
        private bool m_useUserOverride;
        internal NumberFormatInfo numInfo;
        [NonSerialized]
        internal RegionInfo regionInfo;
        private static CultureInfo s_InstalledUICultureInfo;
        private static CultureInfo s_InvariantCultureInfo;
        private static bool? s_isTaiwanSku;
        private static Hashtable s_LcidCachedCultures;
        private static Hashtable s_NameCachedCultures;
        private static CultureInfo s_userDefaultCulture;
        private static CultureInfo s_userDefaultUICulture;
        internal System.Globalization.TextInfo textInfo;

        public CultureInfo(int culture) : this(culture, true)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public CultureInfo(string name) : this(name, true)
        {
        }

        public CultureInfo(int culture, bool useUserOverride)
        {
            this.cultureID = 0x7f;
            if (culture < 0)
            {
                throw new ArgumentOutOfRangeException("culture", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            this.InitializeFromCultureId(culture, useUserOverride);
        }

        public CultureInfo(string name, bool useUserOverride)
        {
            this.cultureID = 0x7f;
            if (name == null)
            {
                throw new ArgumentNullException("name", Environment.GetResourceString("ArgumentNull_String"));
            }
            this.m_cultureData = CultureData.GetCultureData(name, useUserOverride);
            if (this.m_cultureData == null)
            {
                throw new CultureNotFoundException("name", name, Environment.GetResourceString("Argument_CultureNotSupported"));
            }
            this.m_name = this.m_cultureData.CultureName;
            this.m_isInherited = base.GetType() != typeof(CultureInfo);
        }

        internal CultureInfo(string cultureName, string textAndCompareCultureName)
        {
            this.cultureID = 0x7f;
            if (cultureName == null)
            {
                throw new ArgumentNullException("cultureName", Environment.GetResourceString("ArgumentNull_String"));
            }
            this.m_cultureData = CultureData.GetCultureData(cultureName, false);
            if (this.m_cultureData == null)
            {
                throw new CultureNotFoundException("cultureName", cultureName, Environment.GetResourceString("Argument_CultureNotSupported"));
            }
            this.m_name = this.m_cultureData.CultureName;
            CultureInfo cultureInfo = GetCultureInfo(textAndCompareCultureName);
            this.compareInfo = cultureInfo.CompareInfo;
            this.textInfo = cultureInfo.TextInfo;
        }

        internal bool CanSendCrossDomain()
        {
            bool flag = false;
            if (base.GetType() == typeof(CultureInfo))
            {
                flag = true;
            }
            return flag;
        }

        internal static void CheckDomainSafetyObject(object obj, object container)
        {
            if (obj.GetType().Assembly != typeof(CultureInfo).Assembly)
            {
                throw new InvalidOperationException(string.Format(CurrentCulture, Environment.GetResourceString("InvalidOperation_SubclassedObject"), new object[] { obj.GetType(), container.GetType() }));
            }
        }

        public void ClearCachedData()
        {
            s_userDefaultUICulture = null;
            s_userDefaultCulture = null;
            RegionInfo.s_currentRegionInfo = null;
            TimeZone.ResetTimeZone();
            TimeZoneInfo.ClearCachedData();
            s_LcidCachedCultures = null;
            s_NameCachedCultures = null;
            CultureData.ClearCachedData();
        }

        [SecuritySafeCritical]
        public virtual object Clone()
        {
            CultureInfo info = (CultureInfo) base.MemberwiseClone();
            info.m_isReadOnly = false;
            if (!this.m_isInherited)
            {
                if (this.dateTimeInfo != null)
                {
                    info.dateTimeInfo = (DateTimeFormatInfo) this.dateTimeInfo.Clone();
                }
                if (this.numInfo != null)
                {
                    info.numInfo = (NumberFormatInfo) this.numInfo.Clone();
                }
            }
            else
            {
                info.DateTimeFormat = (DateTimeFormatInfo) this.DateTimeFormat.Clone();
                info.NumberFormat = (NumberFormatInfo) this.NumberFormat.Clone();
            }
            if (this.textInfo != null)
            {
                info.textInfo = (System.Globalization.TextInfo) this.textInfo.Clone();
            }
            if (this.calendar != null)
            {
                info.calendar = (System.Globalization.Calendar) this.calendar.Clone();
            }
            return info;
        }

        public static CultureInfo CreateSpecificCulture(string name)
        {
            CultureInfo info;
            try
            {
                info = new CultureInfo(name);
            }
            catch (ArgumentException)
            {
                info = null;
                for (int i = 0; i < name.Length; i++)
                {
                    if ('-' == name[i])
                    {
                        try
                        {
                            info = new CultureInfo(name.Substring(0, i));
                            break;
                        }
                        catch (ArgumentException)
                        {
                            throw;
                        }
                    }
                }
                if (info == null)
                {
                    throw;
                }
            }
            if (!info.IsNeutralCulture)
            {
                return info;
            }
            return new CultureInfo(info.m_cultureData.SSPECIFICCULTURE);
        }

        public override bool Equals(object value)
        {
            if (object.ReferenceEquals(this, value))
            {
                return true;
            }
            CultureInfo info = value as CultureInfo;
            if (info == null)
            {
                return false;
            }
            return (this.Name.Equals(info.Name) && this.CompareInfo.Equals(info.CompareInfo));
        }

        internal static System.Globalization.Calendar GetCalendarInstance(int calType)
        {
            if (calType == 1)
            {
                return new GregorianCalendar();
            }
            return GetCalendarInstanceRare(calType);
        }

        internal static System.Globalization.Calendar GetCalendarInstanceRare(int calType)
        {
            switch (calType)
            {
                case 2:
                case 9:
                case 10:
                case 11:
                case 12:
                    return new GregorianCalendar((GregorianCalendarTypes) calType);

                case 3:
                    return new JapaneseCalendar();

                case 4:
                    return new TaiwanCalendar();

                case 5:
                    return new KoreanCalendar();

                case 6:
                    return new HijriCalendar();

                case 7:
                    return new ThaiBuddhistCalendar();

                case 8:
                    return new HebrewCalendar();

                case 14:
                    return new JapaneseLunisolarCalendar();

                case 15:
                    return new ChineseLunisolarCalendar();

                case 20:
                    return new KoreanLunisolarCalendar();

                case 0x15:
                    return new TaiwanLunisolarCalendar();

                case 0x16:
                    return new PersianCalendar();

                case 0x17:
                    return new UmAlQuraCalendar();
            }
            return new GregorianCalendar();
        }

        [SecuritySafeCritical, ComVisible(false)]
        public CultureInfo GetConsoleFallbackUICulture()
        {
            CultureInfo consoleFallbackCulture = this.m_consoleFallbackCulture;
            if (consoleFallbackCulture == null)
            {
                consoleFallbackCulture = CreateSpecificCulture(this.m_cultureData.SCONSOLEFALLBACKNAME);
                consoleFallbackCulture.m_isReadOnly = true;
                this.m_consoleFallbackCulture = consoleFallbackCulture;
            }
            return consoleFallbackCulture;
        }

        private static CultureInfo GetCultureByName(string name, bool userOverride)
        {
            try
            {
                return (userOverride ? new CultureInfo(name) : GetCultureInfo(name));
            }
            catch (ArgumentException)
            {
            }
            return null;
        }

        public static CultureInfo GetCultureInfo(int culture)
        {
            if (culture <= 0)
            {
                throw new ArgumentOutOfRangeException("culture", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            CultureInfo info = GetCultureInfoHelper(culture, null, null);
            if (info == null)
            {
                throw new CultureNotFoundException("culture", culture, Environment.GetResourceString("Argument_CultureNotSupported"));
            }
            return info;
        }

        public static CultureInfo GetCultureInfo(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            CultureInfo info = GetCultureInfoHelper(0, name, null);
            if (info == null)
            {
                throw new CultureNotFoundException("name", name, Environment.GetResourceString("Argument_CultureNotSupported"));
            }
            return info;
        }

        public static CultureInfo GetCultureInfo(string name, string altName)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (altName == null)
            {
                throw new ArgumentNullException("altName");
            }
            CultureInfo info = GetCultureInfoHelper(-1, name, altName);
            if (info == null)
            {
                throw new CultureNotFoundException("name or altName", string.Format(CurrentCulture, Environment.GetResourceString("Argument_OneOfCulturesNotSupported"), new object[] { name, altName }));
            }
            return info;
        }

        public static CultureInfo GetCultureInfoByIetfLanguageTag(string name)
        {
            if ((name == "zh-CHT") || (name == "zh-CHS"))
            {
                throw new CultureNotFoundException("name", string.Format(CurrentCulture, Environment.GetResourceString("Argument_CultureIetfNotSupported"), new object[] { name }));
            }
            CultureInfo cultureInfo = GetCultureInfo(name);
            if ((cultureInfo.LCID > 0xffff) || (cultureInfo.LCID == 0x40a))
            {
                throw new CultureNotFoundException("name", string.Format(CurrentCulture, Environment.GetResourceString("Argument_CultureIetfNotSupported"), new object[] { name }));
            }
            return cultureInfo;
        }

        internal static CultureInfo GetCultureInfoHelper(int lcid, string name, string altName)
        {
            CultureInfo info;
            Hashtable hashtable = s_NameCachedCultures;
            if (name != null)
            {
                name = CultureData.AnsiToLower(name);
            }
            if (altName != null)
            {
                altName = CultureData.AnsiToLower(altName);
            }
            if (hashtable == null)
            {
                hashtable = Hashtable.Synchronized(new Hashtable());
            }
            else if (lcid == -1)
            {
                info = (CultureInfo) hashtable[name + ((char) 0xfffd) + altName];
                if (info != null)
                {
                    return info;
                }
            }
            else if (lcid == 0)
            {
                info = (CultureInfo) hashtable[name];
                if (info != null)
                {
                    return info;
                }
            }
            Hashtable hashtable2 = s_LcidCachedCultures;
            if (hashtable2 == null)
            {
                hashtable2 = Hashtable.Synchronized(new Hashtable());
            }
            else if (lcid > 0)
            {
                info = (CultureInfo) hashtable2[lcid];
                if (info != null)
                {
                    return info;
                }
            }
            try
            {
                switch (lcid)
                {
                    case -1:
                        info = new CultureInfo(name, altName);
                        goto Label_00D5;

                    case 0:
                        info = new CultureInfo(name, false);
                        goto Label_00D5;
                }
                info = new CultureInfo(lcid, false);
            }
            catch (ArgumentException)
            {
                return null;
            }
        Label_00D5:
            info.m_isReadOnly = true;
            if (lcid == -1)
            {
                hashtable[name + ((char) 0xfffd) + altName] = info;
                info.TextInfo.SetReadOnlyState(true);
            }
            else
            {
                string str = CultureData.AnsiToLower(info.m_name);
                hashtable[str] = info;
                if (((info.LCID != 4) || (str != "zh-hans")) && ((info.LCID != 0x7c04) || (str != "zh-hant")))
                {
                    hashtable2[info.LCID] = info;
                }
            }
            if (-1 != lcid)
            {
                s_LcidCachedCultures = hashtable2;
            }
            s_NameCachedCultures = hashtable;
            return info;
        }

        public static CultureInfo[] GetCultures(System.Globalization.CultureTypes types)
        {
            if ((types & System.Globalization.CultureTypes.UserCustomCulture) == System.Globalization.CultureTypes.UserCustomCulture)
            {
                types |= System.Globalization.CultureTypes.ReplacementCultures;
            }
            return CultureData.GetCultures(types);
        }

        [SecurityCritical]
        private static string GetDefaultLocaleName(int localeType)
        {
            string s = null;
            if (InternalGetDefaultLocaleName(localeType, JitHelpers.GetStringHandleOnStack(ref s)))
            {
                return s;
            }
            return string.Empty;
        }

        [SecuritySafeCritical]
        public virtual object GetFormat(Type formatType)
        {
            if (formatType == typeof(NumberFormatInfo))
            {
                return this.NumberFormat;
            }
            if (formatType == typeof(DateTimeFormatInfo))
            {
                return this.DateTimeFormat;
            }
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override int GetHashCode()
        {
            return (this.Name.GetHashCode() + this.CompareInfo.GetHashCode());
        }

        [SecuritySafeCritical]
        private static string GetSystemDefaultUILanguage()
        {
            string s = null;
            if (InternalGetSystemDefaultUILanguage(JitHelpers.GetStringHandleOnStack(ref s)))
            {
                return s;
            }
            return string.Empty;
        }

        [SecuritySafeCritical]
        private static string GetUserDefaultUILanguage()
        {
            string s = null;
            if (InternalGetUserDefaultUILanguage(JitHelpers.GetStringHandleOnStack(ref s)))
            {
                return s;
            }
            return string.Empty;
        }

        private static bool Init()
        {
            if (s_InvariantCultureInfo == null)
            {
                CultureInfo info = new CultureInfo("", false) {
                    m_isReadOnly = true
                };
                s_InvariantCultureInfo = info;
            }
            s_userDefaultCulture = s_userDefaultUICulture = s_InvariantCultureInfo;
            s_userDefaultCulture = InitUserDefaultCulture();
            s_userDefaultUICulture = InitUserDefaultUICulture();
            return true;
        }

        private void InitializeFromCultureId(int culture, bool useUserOverride)
        {
            switch (culture)
            {
                case 0x800:
                case 0xc00:
                case 0x1000:
                case 0:
                case 0x400:
                    throw new CultureNotFoundException("culture", culture, Environment.GetResourceString("Argument_CultureNotSupported"));
            }
            this.m_cultureData = CultureData.GetCultureData(culture, useUserOverride);
            this.m_isInherited = base.GetType() != typeof(CultureInfo);
            this.m_name = this.m_cultureData.CultureName;
        }

        [SecuritySafeCritical]
        private static CultureInfo InitUserDefaultCulture()
        {
            string defaultLocaleName = GetDefaultLocaleName(0x400);
            if (defaultLocaleName == null)
            {
                defaultLocaleName = GetDefaultLocaleName(0x800);
                if (defaultLocaleName == null)
                {
                    return InvariantCulture;
                }
            }
            CultureInfo cultureByName = GetCultureByName(defaultLocaleName, true);
            cultureByName.m_isReadOnly = true;
            return cultureByName;
        }

        [SecuritySafeCritical]
        private static CultureInfo InitUserDefaultUICulture()
        {
            string userDefaultUILanguage = GetUserDefaultUILanguage();
            if (userDefaultUILanguage == UserDefaultCulture.Name)
            {
                return UserDefaultCulture;
            }
            CultureInfo cultureByName = GetCultureByName(userDefaultUILanguage, true);
            if (cultureByName == null)
            {
                return InvariantCulture;
            }
            cultureByName.m_isReadOnly = true;
            return cultureByName;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool InternalGetDefaultLocaleName(int localetype, StringHandleOnStack localeString);
        [return: MarshalAs(UnmanagedType.Bool)]
        [MethodImpl(MethodImplOptions.InternalCall), SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool InternalGetSystemDefaultUILanguage(StringHandleOnStack systemDefaultUiLanguage);
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool InternalGetUserDefaultUILanguage(StringHandleOnStack userDefaultUiLanguage);
        private static bool IsAlternateSortLcid(int lcid)
        {
            return ((lcid == 0x40a) || ((lcid & 0xf0000) != 0));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string nativeGetLocaleInfoEx(string localeName, uint field);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern int nativeGetLocaleInfoExInt(string localeName, uint field);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool nativeSetThreadLocale(string localeName);
        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if ((this.m_name == null) || IsAlternateSortLcid(this.cultureID))
            {
                this.InitializeFromCultureId(this.cultureID, this.m_useUserOverride);
            }
            else
            {
                this.m_cultureData = CultureData.GetCultureData(this.m_name, this.m_useUserOverride);
                if (this.m_cultureData == null)
                {
                    throw new CultureNotFoundException("m_name", this.m_name, Environment.GetResourceString("Argument_CultureNotSupported"));
                }
            }
            this.m_isInherited = base.GetType() != typeof(CultureInfo);
            if (base.GetType().Assembly == typeof(CultureInfo).Assembly)
            {
                if (this.textInfo != null)
                {
                    CheckDomainSafetyObject(this.textInfo, this);
                }
                if (this.compareInfo != null)
                {
                    CheckDomainSafetyObject(this.compareInfo, this);
                }
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this.m_name = this.m_cultureData.CultureName;
            this.m_useUserOverride = this.m_cultureData.UseUserOverride;
            this.cultureID = this.m_cultureData.ILANGUAGE;
        }

        [SecuritySafeCritical]
        public static CultureInfo ReadOnly(CultureInfo ci)
        {
            if (ci == null)
            {
                throw new ArgumentNullException("ci");
            }
            if (ci.IsReadOnly)
            {
                return ci;
            }
            CultureInfo info = (CultureInfo) ci.MemberwiseClone();
            if (!ci.IsNeutralCulture)
            {
                if (!ci.m_isInherited)
                {
                    if (ci.dateTimeInfo != null)
                    {
                        info.dateTimeInfo = DateTimeFormatInfo.ReadOnly(ci.dateTimeInfo);
                    }
                    if (ci.numInfo != null)
                    {
                        info.numInfo = NumberFormatInfo.ReadOnly(ci.numInfo);
                    }
                }
                else
                {
                    info.DateTimeFormat = DateTimeFormatInfo.ReadOnly(ci.DateTimeFormat);
                    info.NumberFormat = NumberFormatInfo.ReadOnly(ci.NumberFormat);
                }
            }
            if (ci.textInfo != null)
            {
                info.textInfo = System.Globalization.TextInfo.ReadOnly(ci.textInfo);
            }
            if (ci.calendar != null)
            {
                info.calendar = System.Globalization.Calendar.ReadOnly(ci.calendar);
            }
            info.m_isReadOnly = true;
            return info;
        }

        [SecuritySafeCritical]
        internal void StartCrossDomainTracking()
        {
            if (this.m_createdDomainID == 0)
            {
                if (this.CanSendCrossDomain())
                {
                    this.m_isSafeCrossDomain = true;
                }
                Thread.MemoryBarrier();
                this.m_createdDomainID = Thread.GetDomainID();
            }
        }

        public override string ToString()
        {
            return this.m_name;
        }

        internal static bool VerifyCultureName(CultureInfo culture, bool throwException)
        {
            return (!culture.m_isInherited || VerifyCultureName(culture.Name, throwException));
        }

        internal static bool VerifyCultureName(string cultureName, bool throwException)
        {
            for (int i = 0; i < cultureName.Length; i++)
            {
                char c = cultureName[i];
                if ((!char.IsLetterOrDigit(c) && (c != '-')) && (c != '_'))
                {
                    if (throwException)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidResourceCultureName", new object[] { cultureName }));
                    }
                    return false;
                }
            }
            return true;
        }

        private void VerifyWritable()
        {
            if (this.m_isReadOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
            }
        }

        public virtual System.Globalization.Calendar Calendar
        {
            [SecuritySafeCritical]
            get
            {
                if (this.calendar == null)
                {
                    System.Globalization.Calendar defaultCalendar = this.m_cultureData.DefaultCalendar;
                    Thread.MemoryBarrier();
                    defaultCalendar.SetReadOnlyState(this.m_isReadOnly);
                    this.calendar = defaultCalendar;
                }
                return this.calendar;
            }
        }

        public virtual System.Globalization.CompareInfo CompareInfo
        {
            get
            {
                if (this.compareInfo == null)
                {
                    System.Globalization.CompareInfo info = this.UseUserOverride ? GetCultureInfo(this.m_name).CompareInfo : new System.Globalization.CompareInfo(this);
                    if (!OkayToCacheClassWithCompatibilityBehavior)
                    {
                        return info;
                    }
                    this.compareInfo = info;
                }
                return this.compareInfo;
            }
        }

        internal int CreatedDomainID
        {
            get
            {
                return this.m_createdDomainID;
            }
        }

        [ComVisible(false)]
        public System.Globalization.CultureTypes CultureTypes
        {
            get
            {
                System.Globalization.CultureTypes types = 0;
                if (this.m_cultureData.IsNeutralCulture)
                {
                    types |= System.Globalization.CultureTypes.NeutralCultures;
                }
                else
                {
                    types |= System.Globalization.CultureTypes.SpecificCultures;
                }
                types |= this.m_cultureData.IsWin32Installed ? System.Globalization.CultureTypes.InstalledWin32Cultures : 0;
                types |= this.m_cultureData.IsFramework ? System.Globalization.CultureTypes.FrameworkCultures : 0;
                types |= this.m_cultureData.IsUserCustomCulture ? System.Globalization.CultureTypes.UserCustomCulture : 0;
                return (types | (this.m_cultureData.IsReplacementCulture ? (System.Globalization.CultureTypes.ReplacementCultures | System.Globalization.CultureTypes.UserCustomCulture) : 0));
            }
        }

        public static CultureInfo CurrentCulture
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        public static CultureInfo CurrentUICulture
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }

        public virtual DateTimeFormatInfo DateTimeFormat
        {
            [SecuritySafeCritical]
            get
            {
                if (this.dateTimeInfo == null)
                {
                    DateTimeFormatInfo info = new DateTimeFormatInfo(this.m_cultureData, this.Calendar) {
                        m_isReadOnly = this.m_isReadOnly
                    };
                    Thread.MemoryBarrier();
                    this.dateTimeInfo = info;
                }
                return this.dateTimeInfo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Obj"));
                }
                this.VerifyWritable();
                this.dateTimeInfo = value;
            }
        }

        public virtual string DisplayName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SLOCALIZEDDISPLAYNAME;
            }
        }

        public virtual string EnglishName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SENGDISPLAYNAME;
            }
        }

        internal bool HasInvariantCultureName
        {
            get
            {
                return (this.Name == InvariantCulture.Name);
            }
        }

        [ComVisible(false)]
        public string IetfLanguageTag
        {
            get
            {
                switch (this.Name)
                {
                    case "zh-CHT":
                        return "zh-Hant";

                    case "zh-CHS":
                        return "zh-Hans";
                }
                return this.Name;
            }
        }

        public static CultureInfo InstalledUICulture
        {
            get
            {
                CultureInfo cultureByName = s_InstalledUICultureInfo;
                if (cultureByName == null)
                {
                    cultureByName = GetCultureByName(GetSystemDefaultUILanguage(), true);
                    if (cultureByName == null)
                    {
                        cultureByName = InvariantCulture;
                    }
                    cultureByName.m_isReadOnly = true;
                    s_InstalledUICultureInfo = cultureByName;
                }
                return cultureByName;
            }
        }

        public static CultureInfo InvariantCulture
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return s_InvariantCultureInfo;
            }
        }

        public virtual bool IsNeutralCulture
        {
            get
            {
                return this.m_cultureData.IsNeutralCulture;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.m_isReadOnly;
            }
        }

        internal bool IsSafeCrossDomain
        {
            get
            {
                return this.m_isSafeCrossDomain;
            }
        }

        internal static bool IsTaiwanSku
        {
            get
            {
                if (!s_isTaiwanSku.HasValue)
                {
                    s_isTaiwanSku = new bool?(GetSystemDefaultUILanguage() == "zh-TW");
                }
                return s_isTaiwanSku.Value;
            }
        }

        [ComVisible(false)]
        public virtual int KeyboardLayoutId
        {
            get
            {
                return this.m_cultureData.IINPUTLANGUAGEHANDLE;
            }
        }

        public virtual int LCID
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.m_cultureData.ILANGUAGE;
            }
        }

        public virtual string Name
        {
            get
            {
                if (this.m_nonSortName == null)
                {
                    this.m_nonSortName = this.m_cultureData.SNAME;
                    if (this.m_nonSortName == null)
                    {
                        this.m_nonSortName = string.Empty;
                    }
                }
                return this.m_nonSortName;
            }
        }

        public virtual string NativeName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SNATIVEDISPLAYNAME;
            }
        }

        public virtual NumberFormatInfo NumberFormat
        {
            [SecuritySafeCritical]
            get
            {
                if (this.numInfo == null)
                {
                    NumberFormatInfo info = new NumberFormatInfo(this.m_cultureData) {
                        isReadOnly = this.m_isReadOnly
                    };
                    this.numInfo = info;
                }
                return this.numInfo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Obj"));
                }
                this.VerifyWritable();
                this.numInfo = value;
            }
        }

        private static bool OkayToCacheClassWithCompatibilityBehavior
        {
            get
            {
                return AppDomain.CurrentDomain.IsCompatibilitySwitchSet("a").HasValue;
            }
        }

        public virtual System.Globalization.Calendar[] OptionalCalendars
        {
            [SecuritySafeCritical]
            get
            {
                int[] calendarIds = this.m_cultureData.CalendarIds;
                System.Globalization.Calendar[] calendarArray = new System.Globalization.Calendar[calendarIds.Length];
                for (int i = 0; i < calendarArray.Length; i++)
                {
                    calendarArray[i] = GetCalendarInstance(calendarIds[i]);
                }
                return calendarArray;
            }
        }

        public virtual CultureInfo Parent
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_parent == null)
                {
                    try
                    {
                        string sPARENT = this.m_cultureData.SPARENT;
                        if (string.IsNullOrEmpty(sPARENT))
                        {
                            this.m_parent = InvariantCulture;
                        }
                        else
                        {
                            this.m_parent = new CultureInfo(sPARENT, this.m_cultureData.UseUserOverride);
                        }
                    }
                    catch (ArgumentException)
                    {
                        this.m_parent = InvariantCulture;
                    }
                }
                return this.m_parent;
            }
        }

        private RegionInfo Region
        {
            get
            {
                if (this.regionInfo == null)
                {
                    RegionInfo info = new RegionInfo(this.m_cultureData);
                    this.regionInfo = info;
                }
                return this.regionInfo;
            }
        }

        internal string SortName
        {
            get
            {
                if (this.m_sortName == null)
                {
                    this.m_sortName = this.m_cultureData.SCOMPAREINFO;
                }
                return this.m_sortName;
            }
        }

        public virtual System.Globalization.TextInfo TextInfo
        {
            get
            {
                if (this.textInfo == null)
                {
                    System.Globalization.TextInfo info = new System.Globalization.TextInfo(this.m_cultureData);
                    info.SetReadOnlyState(this.m_isReadOnly);
                    if (!OkayToCacheClassWithCompatibilityBehavior)
                    {
                        return info;
                    }
                    this.textInfo = info;
                }
                return this.textInfo;
            }
        }

        public virtual string ThreeLetterISOLanguageName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SISO639LANGNAME2;
            }
        }

        public virtual string ThreeLetterWindowsLanguageName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SABBREVLANGNAME;
            }
        }

        public virtual string TwoLetterISOLanguageName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SISO639LANGNAME;
            }
        }

        internal static CultureInfo UserDefaultCulture
        {
            get
            {
                CultureInfo info = s_userDefaultCulture;
                if (info == null)
                {
                    s_userDefaultCulture = InvariantCulture;
                    info = InitUserDefaultCulture();
                    s_userDefaultCulture = info;
                }
                return info;
            }
        }

        internal static CultureInfo UserDefaultUICulture
        {
            get
            {
                CultureInfo info = s_userDefaultUICulture;
                if (info == null)
                {
                    s_userDefaultUICulture = InvariantCulture;
                    info = InitUserDefaultUICulture();
                    s_userDefaultUICulture = info;
                }
                return info;
            }
        }

        public bool UseUserOverride
        {
            get
            {
                return this.m_cultureData.UseUserOverride;
            }
        }
    }
}


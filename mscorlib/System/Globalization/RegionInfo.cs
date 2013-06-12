namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class RegionInfo
    {
        private static readonly int[] IdFromEverettRegionInfoDataItem = new int[] { 
            0x3801, 0x41c, 0x42b, 0x2c0a, 0xc07, 0xc09, 0x42c, 0x80c, 0x402, 0x3c01, 0x83e, 0x400a, 0x416, 0x423, 0x2809, 0xc0c, 
            0x2409, 0x807, 0x340a, 0x804, 0x240a, 0x140a, 0x405, 0x407, 0x406, 0x1c0a, 0x1401, 0x300a, 0x425, 0xc01, 0x403, 0x40b, 
            0x438, 0x40c, 0x809, 0x437, 0x408, 0x100a, 0xc04, 0x480a, 0x41a, 0x40e, 0x421, 0x1809, 0x40d, 0x439, 0x801, 0x429, 
            0x40f, 0x410, 0x2009, 0x2c01, 0x411, 0x441, 0x440, 0x412, 0x3401, 0x43f, 0x3001, 0x1407, 0x427, 0x1007, 0x426, 0x1001, 
            0x1801, 0x180c, 0x42f, 0x450, 0x1404, 0x465, 0x80a, 0x43e, 0x4c0a, 0x413, 0x414, 0x1409, 0x2001, 0x180a, 0x280a, 0x3409, 
            0x420, 0x415, 0x500a, 0x816, 0x3c0a, 0x4001, 0x418, 0x419, 0x401, 0x41d, 0x1004, 0x424, 0x41b, 0x81a, 0x440a, 0x45a, 
            0x41e, 0x1c01, 0x41f, 0x2c09, 0x404, 0x422, 0x409, 0x380a, 0x443, 0x200a, 0x42a, 0x2401, 0x436, 0x3009
         };
        [NonSerialized]
        internal CultureData m_cultureData;
        [OptionalField(VersionAdded=2)]
        private int m_cultureId;
        [OptionalField(VersionAdded=2)]
        internal int m_dataItem;
        internal string m_name;
        internal static RegionInfo s_currentRegionInfo;

        [SecuritySafeCritical]
        internal RegionInfo(CultureData cultureData)
        {
            this.m_cultureData = cultureData;
            this.m_name = this.m_cultureData.SREGIONNAME;
        }

        [SecuritySafeCritical]
        public RegionInfo(int culture)
        {
            if (culture == 0x7f)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NoRegionInvariantCulture"));
            }
            if (culture == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_CultureIsNeutral", new object[] { culture }), "culture");
            }
            if (culture == 0xc00)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_CustomCultureCannotBePassedByNumber", new object[] { culture }), "culture");
            }
            this.m_cultureData = CultureData.GetCultureData(culture, true);
            this.m_name = this.m_cultureData.SREGIONNAME;
            if (this.m_cultureData.IsNeutralCulture)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_CultureIsNeutral", new object[] { culture }), "culture");
            }
            this.m_cultureId = culture;
        }

        [SecuritySafeCritical]
        public RegionInfo(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NoRegionInvariantCulture"));
            }
            this.m_cultureData = CultureData.GetCultureDataForRegion(name, true);
            if (this.m_cultureData == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidCultureName"), new object[] { name }), "name");
            }
            if (this.m_cultureData.IsNeutralCulture)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNeutralRegionName", new object[] { name }), "name");
            }
            this.SetName(name);
        }

        public override bool Equals(object value)
        {
            RegionInfo info = value as RegionInfo;
            return ((info != null) && this.Name.Equals(info.Name));
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        [SecurityCritical, OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (this.m_name == null)
            {
                this.m_cultureId = IdFromEverettRegionInfoDataItem[this.m_dataItem];
            }
            if (this.m_cultureId == 0)
            {
                this.m_cultureData = CultureData.GetCultureDataForRegion(this.m_name, true);
            }
            else
            {
                this.m_cultureData = CultureData.GetCultureData(this.m_cultureId, true);
            }
            if (this.m_cultureData == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidCultureName"), new object[] { this.m_name }), "m_name");
            }
            if (this.m_cultureId == 0)
            {
                this.SetName(this.m_name);
            }
            else
            {
                this.m_name = this.m_cultureData.SREGIONNAME;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
        }

        [SecurityCritical]
        private void SetName(string name)
        {
            this.m_name = name.Equals(this.m_cultureData.SREGIONNAME, StringComparison.OrdinalIgnoreCase) ? this.m_cultureData.SREGIONNAME : this.m_cultureData.CultureName;
        }

        public override string ToString()
        {
            return this.Name;
        }

        [ComVisible(false)]
        public virtual string CurrencyEnglishName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SENGLISHCURRENCY;
            }
        }

        [ComVisible(false)]
        public virtual string CurrencyNativeName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SNATIVECURRENCY;
            }
        }

        public virtual string CurrencySymbol
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SCURRENCY;
            }
        }

        public static RegionInfo CurrentRegion
        {
            [SecuritySafeCritical]
            get
            {
                RegionInfo info = s_currentRegionInfo;
                if (info == null)
                {
                    info = new RegionInfo(CultureInfo.CurrentCulture.m_cultureData) {
                        m_name = info.m_cultureData.SREGIONNAME
                    };
                    s_currentRegionInfo = info;
                }
                return info;
            }
        }

        public virtual string DisplayName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SLOCALIZEDCOUNTRY;
            }
        }

        public virtual string EnglishName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SENGCOUNTRY;
            }
        }

        [ComVisible(false)]
        public virtual int GeoId
        {
            get
            {
                return this.m_cultureData.IGEOID;
            }
        }

        public virtual bool IsMetric
        {
            [SecuritySafeCritical]
            get
            {
                return (this.m_cultureData.IMEASURE == 0);
            }
        }

        public virtual string ISOCurrencySymbol
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SINTLSYMBOL;
            }
        }

        public virtual string Name
        {
            get
            {
                return this.m_name;
            }
        }

        [ComVisible(false)]
        public virtual string NativeName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SNATIVECOUNTRY;
            }
        }

        public virtual string ThreeLetterISORegionName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SISO3166CTRYNAME2;
            }
        }

        public virtual string ThreeLetterWindowsRegionName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SABBREVCTRYNAME;
            }
        }

        public virtual string TwoLetterISORegionName
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_cultureData.SISO3166CTRYNAME;
            }
        }
    }
}


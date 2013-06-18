namespace System.Configuration
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct OverrideModeSetting
    {
        private byte _mode;
        internal static OverrideModeSetting SectionDefault;
        internal static OverrideModeSetting LocationDefault;
        private const byte ApiDefinedLegacy = 0x10;
        private const byte ApiDefinedNewMode = 0x20;
        private const byte ApiDefinedAny = 0x30;
        private const byte XmlDefinedLegacy = 0x40;
        private const byte XmlDefinedNewMode = 0x80;
        private const byte XmlDefinedAny = 0xc0;
        private const byte ModeMask = 15;
        static OverrideModeSetting()
        {
            SectionDefault = new OverrideModeSetting();
            SectionDefault._mode = 1;
            LocationDefault = new OverrideModeSetting();
            LocationDefault._mode = 0;
        }

        internal static OverrideModeSetting CreateFromXmlReadValue(bool allowOverride)
        {
            OverrideModeSetting setting = new OverrideModeSetting();
            setting.SetMode(allowOverride ? System.Configuration.OverrideMode.Inherit : System.Configuration.OverrideMode.Deny);
            setting._mode = (byte) (setting._mode | 0x40);
            return setting;
        }

        internal static OverrideModeSetting CreateFromXmlReadValue(System.Configuration.OverrideMode mode)
        {
            OverrideModeSetting setting = new OverrideModeSetting();
            setting.SetMode(mode);
            setting._mode = (byte) (setting._mode | 0x80);
            return setting;
        }

        internal static System.Configuration.OverrideMode ParseOverrideModeXmlValue(string value, XmlUtil xmlUtil)
        {
            switch (value)
            {
                case "Inherit":
                    return System.Configuration.OverrideMode.Inherit;

                case "Allow":
                    return System.Configuration.OverrideMode.Allow;

                case "Deny":
                    return System.Configuration.OverrideMode.Deny;
            }
            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_section_override_mode_attribute_invalid"), xmlUtil);
        }

        internal static bool CanUseSameLocationTag(OverrideModeSetting x, OverrideModeSetting y)
        {
            bool flag = false;
            flag = x.OverrideMode == y.OverrideMode;
            if (!flag)
            {
                return flag;
            }
            flag = false;
            if ((x._mode & 0x30) != 0)
            {
                return IsMatchingApiChangedLocationTag(x, y);
            }
            if ((y._mode & 0x30) != 0)
            {
                return IsMatchingApiChangedLocationTag(y, x);
            }
            if (((x._mode & 0xc0) != 0) || ((y._mode & 0xc0) != 0))
            {
                return ((x._mode & 0xc0) == (y._mode & 0xc0));
            }
            return true;
        }

        private static bool IsMatchingApiChangedLocationTag(OverrideModeSetting x, OverrideModeSetting y)
        {
            bool flag = false;
            if ((y._mode & 0x30) != 0)
            {
                return ((x._mode & 0x30) == (y._mode & 0x30));
            }
            if ((y._mode & 0xc0) != 0)
            {
                flag = (((x._mode & 0x10) != 0) && ((y._mode & 0x40) != 0)) || (((x._mode & 0x20) != 0) && ((y._mode & 0x80) != 0));
            }
            return flag;
        }

        internal bool IsDefaultForSection
        {
            get
            {
                System.Configuration.OverrideMode overrideMode = this.OverrideMode;
                if (overrideMode != System.Configuration.OverrideMode.Allow)
                {
                    return (overrideMode == System.Configuration.OverrideMode.Inherit);
                }
                return true;
            }
        }
        internal bool IsDefaultForLocationTag
        {
            get
            {
                return (((LocationDefault.OverrideMode == this.OverrideMode) && ((this._mode & 0x30) == 0)) && ((this._mode & 0xc0) == 0));
            }
        }
        internal bool IsLocked
        {
            get
            {
                return (this.OverrideMode == System.Configuration.OverrideMode.Deny);
            }
        }
        internal string LocationTagXmlString
        {
            get
            {
                string str = string.Empty;
                string overrideModeXmlValue = null;
                string str3 = null;
                bool flag = false;
                bool flag2 = false;
                if ((this._mode & 0x30) != 0)
                {
                    flag2 = (this._mode & 0x10) != 0;
                    flag = true;
                }
                else if ((this._mode & 0xc0) != 0)
                {
                    flag2 = (this._mode & 0x40) != 0;
                    flag = true;
                }
                if (!flag)
                {
                    return str;
                }
                if (flag2)
                {
                    str3 = "allowOverride";
                    overrideModeXmlValue = this.AllowOverride ? "true" : "false";
                }
                else
                {
                    str3 = "overrideMode";
                    overrideModeXmlValue = this.OverrideModeXmlValue;
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"", new object[] { str3, overrideModeXmlValue });
            }
        }
        internal string OverrideModeXmlValue
        {
            get
            {
                switch (this.OverrideMode)
                {
                    case System.Configuration.OverrideMode.Inherit:
                        return "Inherit";

                    case System.Configuration.OverrideMode.Allow:
                        return "Allow";

                    case System.Configuration.OverrideMode.Deny:
                        return "Deny";
                }
                return null;
            }
        }
        internal void ChangeModeInternal(System.Configuration.OverrideMode mode)
        {
            this.SetMode(mode);
        }

        internal System.Configuration.OverrideMode OverrideMode
        {
            get
            {
                return (((System.Configuration.OverrideMode) this._mode) & ((System.Configuration.OverrideMode) 15));
            }
            set
            {
                this.VerifyConsistentChangeModel(0x20);
                this.SetMode(value);
                this._mode = (byte) (this._mode | 0x20);
            }
        }
        internal bool AllowOverride
        {
            get
            {
                switch (this.OverrideMode)
                {
                    case System.Configuration.OverrideMode.Inherit:
                    case System.Configuration.OverrideMode.Allow:
                        return true;

                    case System.Configuration.OverrideMode.Deny:
                        return false;
                }
                return true;
            }
            set
            {
                this.VerifyConsistentChangeModel(0x10);
                this.SetMode(value ? System.Configuration.OverrideMode.Inherit : System.Configuration.OverrideMode.Deny);
                this._mode = (byte) (this._mode | 0x10);
            }
        }
        private void SetMode(System.Configuration.OverrideMode mode)
        {
            this._mode = (byte) mode;
        }

        private void VerifyConsistentChangeModel(byte required)
        {
            byte num = (byte) (this._mode & 0x30);
            if ((num != 0) && (num != required))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Cannot_change_both_AllowOverride_and_OverrideMode"));
            }
        }
    }
}


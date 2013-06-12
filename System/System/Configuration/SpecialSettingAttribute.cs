namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class SpecialSettingAttribute : Attribute
    {
        private readonly System.Configuration.SpecialSetting _specialSetting;

        public SpecialSettingAttribute(System.Configuration.SpecialSetting specialSetting)
        {
            this._specialSetting = specialSetting;
        }

        public System.Configuration.SpecialSetting SpecialSetting
        {
            get
            {
                return this._specialSetting;
            }
        }
    }
}


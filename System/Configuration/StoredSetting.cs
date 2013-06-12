namespace System.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoredSetting
    {
        internal SettingsSerializeAs SerializeAs;
        internal XmlNode Value;
        internal StoredSetting(SettingsSerializeAs serializeAs, XmlNode value)
        {
            this.SerializeAs = serializeAs;
            this.Value = value;
        }
    }
}


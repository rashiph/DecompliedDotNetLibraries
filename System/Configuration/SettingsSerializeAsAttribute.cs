namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class SettingsSerializeAsAttribute : Attribute
    {
        private readonly SettingsSerializeAs _serializeAs;

        public SettingsSerializeAsAttribute(SettingsSerializeAs serializeAs)
        {
            this._serializeAs = serializeAs;
        }

        public SettingsSerializeAs SerializeAs
        {
            get
            {
                return this._serializeAs;
            }
        }
    }
}


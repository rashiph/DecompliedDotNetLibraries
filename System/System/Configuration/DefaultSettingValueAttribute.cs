namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DefaultSettingValueAttribute : Attribute
    {
        private readonly string _value;

        public DefaultSettingValueAttribute(string value)
        {
            this._value = value;
        }

        public string Value
        {
            get
            {
                return this._value;
            }
        }
    }
}


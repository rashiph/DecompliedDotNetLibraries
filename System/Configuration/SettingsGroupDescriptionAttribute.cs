namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SettingsGroupDescriptionAttribute : Attribute
    {
        private readonly string _desc;

        public SettingsGroupDescriptionAttribute(string description)
        {
            this._desc = description;
        }

        public string Description
        {
            get
            {
                return this._desc;
            }
        }
    }
}


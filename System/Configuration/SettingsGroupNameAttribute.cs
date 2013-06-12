namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SettingsGroupNameAttribute : Attribute
    {
        private readonly string _groupName;

        public SettingsGroupNameAttribute(string groupName)
        {
            this._groupName = groupName;
        }

        public string GroupName
        {
            get
            {
                return this._groupName;
            }
        }
    }
}


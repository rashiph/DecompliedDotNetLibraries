namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class SettingsManageabilityAttribute : Attribute
    {
        private readonly SettingsManageability _manageability;

        public SettingsManageabilityAttribute(SettingsManageability manageability)
        {
            this._manageability = manageability;
        }

        public SettingsManageability Manageability
        {
            get
            {
                return this._manageability;
            }
        }
    }
}


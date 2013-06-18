namespace System.Web.Profile
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingsAllowAnonymousAttribute : Attribute
    {
        private bool _Allow;

        public SettingsAllowAnonymousAttribute(bool allow)
        {
            this._Allow = allow;
        }

        public override bool IsDefaultAttribute()
        {
            return !this._Allow;
        }

        public bool Allow
        {
            get
            {
                return this._Allow;
            }
        }
    }
}


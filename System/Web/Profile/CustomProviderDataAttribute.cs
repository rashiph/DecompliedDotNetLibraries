namespace System.Web.Profile
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CustomProviderDataAttribute : Attribute
    {
        private string _CustomProviderData;

        public CustomProviderDataAttribute(string customProviderData)
        {
            this._CustomProviderData = customProviderData;
        }

        public override bool IsDefaultAttribute()
        {
            return string.IsNullOrEmpty(this._CustomProviderData);
        }

        public string CustomProviderData
        {
            get
            {
                return this._CustomProviderData;
            }
        }
    }
}


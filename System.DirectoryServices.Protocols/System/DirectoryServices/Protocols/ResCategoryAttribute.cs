namespace System.DirectoryServices.Protocols
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class ResCategoryAttribute : CategoryAttribute
    {
        public ResCategoryAttribute(string category) : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return Res.GetString(value);
        }
    }
}


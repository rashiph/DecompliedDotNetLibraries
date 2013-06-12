namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class LicenseProviderAttribute : Attribute
    {
        public static readonly LicenseProviderAttribute Default = new LicenseProviderAttribute();
        private string licenseProviderName;
        private Type licenseProviderType;

        public LicenseProviderAttribute() : this((string) null)
        {
        }

        public LicenseProviderAttribute(string typeName)
        {
            this.licenseProviderName = typeName;
        }

        public LicenseProviderAttribute(Type type)
        {
            this.licenseProviderType = type;
        }

        public override bool Equals(object value)
        {
            if ((value is LicenseProviderAttribute) && (value != null))
            {
                Type licenseProvider = ((LicenseProviderAttribute) value).LicenseProvider;
                if (licenseProvider == this.LicenseProvider)
                {
                    return true;
                }
                if ((licenseProvider != null) && licenseProvider.Equals(this.LicenseProvider))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Type LicenseProvider
        {
            get
            {
                if ((this.licenseProviderType == null) && (this.licenseProviderName != null))
                {
                    this.licenseProviderType = Type.GetType(this.licenseProviderName);
                }
                return this.licenseProviderType;
            }
        }

        public override object TypeId
        {
            get
            {
                string licenseProviderName = this.licenseProviderName;
                if ((licenseProviderName == null) && (this.licenseProviderType != null))
                {
                    licenseProviderName = this.licenseProviderType.FullName;
                }
                return (base.GetType().FullName + licenseProviderName);
            }
        }
    }
}


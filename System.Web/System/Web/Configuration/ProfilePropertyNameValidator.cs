namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    internal sealed class ProfilePropertyNameValidator : ConfigurationValidatorBase
    {
        internal static ProfilePropertyNameValidator SingletonInstance = new ProfilePropertyNameValidator();

        public override bool CanValidate(Type type)
        {
            return (type == typeof(string));
        }

        public override void Validate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            string str = value as string;
            if (str != null)
            {
                str = str.Trim();
            }
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException(System.Web.SR.GetString("Profile_name_can_not_be_empty"));
            }
            if (str.Contains("."))
            {
                throw new ArgumentException(System.Web.SR.GetString("Profile_name_can_not_contain_period"));
            }
        }
    }
}


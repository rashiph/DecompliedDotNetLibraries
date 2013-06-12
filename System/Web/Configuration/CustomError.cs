namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Util;

    public sealed class CustomError : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propRedirect = new ConfigurationProperty("redirect", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propStatusCode = new ConfigurationProperty("statusCode", typeof(int), null, null, new IntegerValidator(100, 0x3e7), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);

        static CustomError()
        {
            _properties.Add(_propStatusCode);
            _properties.Add(_propRedirect);
        }

        internal CustomError()
        {
        }

        public CustomError(int statusCode, string redirect) : this()
        {
            this.StatusCode = statusCode;
            this.Redirect = redirect;
        }

        public override bool Equals(object customError)
        {
            CustomError error = customError as CustomError;
            return (((error != null) && (error.StatusCode == this.StatusCode)) && (error.Redirect == this.Redirect));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this.StatusCode, this.Redirect.GetHashCode());
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("redirect", IsRequired=true)]
        public string Redirect
        {
            get
            {
                return (string) base[_propRedirect];
            }
            set
            {
                base[_propRedirect] = value;
            }
        }

        [IntegerValidator(MinValue=100, MaxValue=0x3e7), ConfigurationProperty("statusCode", IsRequired=true, IsKey=true)]
        public int StatusCode
        {
            get
            {
                return (int) base[_propStatusCode];
            }
            set
            {
                base[_propStatusCode] = value;
            }
        }
    }
}


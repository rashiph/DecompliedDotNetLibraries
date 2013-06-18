namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Compilation;

    public sealed class ExpressionBuilder : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propExpressionPrefix = new ConfigurationProperty("expressionPrefix", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);

        static ExpressionBuilder()
        {
            _properties.Add(_propExpressionPrefix);
            _properties.Add(_propType);
        }

        internal ExpressionBuilder()
        {
        }

        public ExpressionBuilder(string expressionPrefix, string theType)
        {
            this.ExpressionPrefix = expressionPrefix;
            this.Type = theType;
        }

        [StringValidator(MinLength=1), ConfigurationProperty("expressionPrefix", IsRequired=true, IsKey=true, DefaultValue="")]
        public string ExpressionPrefix
        {
            get
            {
                return (string) base[_propExpressionPrefix];
            }
            set
            {
                base[_propExpressionPrefix] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("type", IsRequired=true, DefaultValue=""), StringValidator(MinLength=1)]
        public string Type
        {
            get
            {
                return (string) base[_propType];
            }
            set
            {
                base[_propType] = value;
            }
        }

        internal System.Type TypeInternal
        {
            get
            {
                return CompilationUtil.LoadTypeWithChecks(this.Type, typeof(System.Web.Compilation.ExpressionBuilder), null, this, "type");
            }
        }
    }
}


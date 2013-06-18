namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;

    public sealed class ConfigurationProperty
    {
        private string _addElementName;
        private string _clearElementName;
        private TypeConverter _converter;
        private object _defaultValue;
        private string _description;
        private bool _isConfigurationElementType;
        private bool _isTypeInited;
        private string _name;
        private ConfigurationPropertyOptions _options;
        private string _providedName;
        private string _removeElementName;
        private System.Type _type;
        private ConfigurationValidatorBase _validator;
        internal static readonly string DefaultCollectionPropertyName = "";
        private static readonly ConfigurationValidatorBase DefaultValidatorInstance = new DefaultValidator();
        internal static readonly ConfigurationValidatorBase NonEmptyStringValidator = new StringValidator(1);

        internal ConfigurationProperty(PropertyInfo info)
        {
            TypeConverterAttribute attribute = null;
            ConfigurationPropertyAttribute attribProperty = null;
            ConfigurationValidatorAttribute attribute3 = null;
            DescriptionAttribute attribute4 = null;
            DefaultValueAttribute attribStdDefault = null;
            TypeConverter converter = null;
            ConfigurationValidatorBase validator = null;
            foreach (Attribute attribute6 in Attribute.GetCustomAttributes(info))
            {
                if (attribute6 is TypeConverterAttribute)
                {
                    attribute = (TypeConverterAttribute) attribute6;
                    converter = (TypeConverter) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(attribute.ConverterTypeName);
                }
                else if (attribute6 is ConfigurationPropertyAttribute)
                {
                    attribProperty = (ConfigurationPropertyAttribute) attribute6;
                }
                else if (attribute6 is ConfigurationValidatorAttribute)
                {
                    if (validator != null)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Validator_multiple_validator_attributes", new object[] { info.Name }));
                    }
                    attribute3 = (ConfigurationValidatorAttribute) attribute6;
                    validator = attribute3.ValidatorInstance;
                }
                else if (attribute6 is DescriptionAttribute)
                {
                    attribute4 = (DescriptionAttribute) attribute6;
                }
                else if (attribute6 is DefaultValueAttribute)
                {
                    attribStdDefault = (DefaultValueAttribute) attribute6;
                }
            }
            System.Type propertyType = info.PropertyType;
            if (typeof(ConfigurationElementCollection).IsAssignableFrom(propertyType))
            {
                ConfigurationCollectionAttribute customAttribute = Attribute.GetCustomAttribute(info, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                if (customAttribute == null)
                {
                    customAttribute = Attribute.GetCustomAttribute(propertyType, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                }
                if (customAttribute != null)
                {
                    if (customAttribute.AddItemName.IndexOf(',') == -1)
                    {
                        this._addElementName = customAttribute.AddItemName;
                    }
                    this._removeElementName = customAttribute.RemoveItemName;
                    this._clearElementName = customAttribute.ClearItemsName;
                }
            }
            this.ConstructorInit(attribProperty.Name, info.PropertyType, attribProperty.Options, validator, converter);
            this.InitDefaultValueFromTypeInfo(attribProperty, attribStdDefault);
            if ((attribute4 != null) && !string.IsNullOrEmpty(attribute4.Description))
            {
                this._description = attribute4.Description;
            }
        }

        public ConfigurationProperty(string name, System.Type type)
        {
            object obj2 = null;
            this.ConstructorInit(name, type, ConfigurationPropertyOptions.None, null, null);
            if (type == typeof(string))
            {
                obj2 = string.Empty;
            }
            else if (type.IsValueType)
            {
                obj2 = System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(type);
            }
            this.SetDefaultValue(obj2);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationProperty(string name, System.Type type, object defaultValue) : this(name, type, defaultValue, ConfigurationPropertyOptions.None)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationProperty(string name, System.Type type, object defaultValue, ConfigurationPropertyOptions options) : this(name, type, defaultValue, null, null, options)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationProperty(string name, System.Type type, object defaultValue, TypeConverter typeConverter, ConfigurationValidatorBase validator, ConfigurationPropertyOptions options) : this(name, type, defaultValue, typeConverter, validator, options, null)
        {
        }

        public ConfigurationProperty(string name, System.Type type, object defaultValue, TypeConverter typeConverter, ConfigurationValidatorBase validator, ConfigurationPropertyOptions options, string description)
        {
            this.ConstructorInit(name, type, options, validator, typeConverter);
            this.SetDefaultValue(defaultValue);
        }

        private void ConstructorInit(string name, System.Type type, ConfigurationPropertyOptions options, ConfigurationValidatorBase validator, TypeConverter converter)
        {
            if (typeof(ConfigurationSection).IsAssignableFrom(type))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_properties_may_not_be_derived_from_configuration_section", new object[] { name }));
            }
            this._providedName = name;
            if (((options & ConfigurationPropertyOptions.IsDefaultCollection) != ConfigurationPropertyOptions.None) && string.IsNullOrEmpty(name))
            {
                name = DefaultCollectionPropertyName;
            }
            else
            {
                this.ValidatePropertyName(name);
            }
            this._name = name;
            this._type = type;
            this._options = options;
            this._validator = validator;
            this._converter = converter;
            if (this._validator == null)
            {
                this._validator = DefaultValidatorInstance;
            }
            else if (!this._validator.CanValidate(this._type))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Validator_does_not_support_prop_type", new object[] { this._name }));
            }
        }

        internal object ConvertFromString(string value)
        {
            object obj2 = null;
            try
            {
                obj2 = this.Converter.ConvertFromInvariantString(value);
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Top_level_conversion_error_from_string", new object[] { this._name, exception.Message }));
            }
            return obj2;
        }

        internal string ConvertToString(object value)
        {
            string str = null;
            try
            {
                if (this._type == typeof(bool))
                {
                    return (((bool) value) ? "true" : "false");
                }
                str = this.Converter.ConvertToInvariantString(value);
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Top_level_conversion_error_to_string", new object[] { this._name, exception.Message }));
            }
            return str;
        }

        private void CreateConverter()
        {
            if (this._converter == null)
            {
                if (this._type.IsEnum)
                {
                    this._converter = new GenericEnumConverter(this._type);
                }
                else if (!this._type.IsSubclassOf(typeof(ConfigurationElement)))
                {
                    this._converter = TypeDescriptor.GetConverter(this._type);
                    if (((this._converter == null) || !this._converter.CanConvertFrom(typeof(string))) || !this._converter.CanConvertTo(typeof(string)))
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("No_converter", new object[] { this._name, this._type.Name }));
                    }
                }
            }
        }

        private void InitDefaultValueFromTypeInfo(ConfigurationPropertyAttribute attribProperty, DefaultValueAttribute attribStdDefault)
        {
            object defaultValue = attribProperty.DefaultValue;
            if (((defaultValue == null) || (defaultValue == ConfigurationElement.s_nullPropertyValue)) && (attribStdDefault != null))
            {
                defaultValue = attribStdDefault.Value;
            }
            if (((defaultValue != null) && (defaultValue is string)) && (this._type != typeof(string)))
            {
                try
                {
                    defaultValue = this.Converter.ConvertFromInvariantString((string) defaultValue);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Default_value_conversion_error_from_string", new object[] { this._name, exception.Message }));
                }
            }
            if ((defaultValue == null) || (defaultValue == ConfigurationElement.s_nullPropertyValue))
            {
                if (this._type == typeof(string))
                {
                    defaultValue = string.Empty;
                }
                else if (this._type.IsValueType)
                {
                    defaultValue = System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(this._type);
                }
            }
            this.SetDefaultValue(defaultValue);
        }

        private void SetDefaultValue(object value)
        {
            if ((value != null) && (value != ConfigurationElement.s_nullPropertyValue))
            {
                bool flag = this._type.IsAssignableFrom(value.GetType());
                if (!flag && this.Converter.CanConvertFrom(value.GetType()))
                {
                    value = this.Converter.ConvertFrom(value);
                }
                else if (!flag)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Default_value_wrong_type", new object[] { this._name }));
                }
                this.Validate(value);
                this._defaultValue = value;
            }
        }

        internal void Validate(object value)
        {
            try
            {
                this._validator.Validate(value);
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Top_level_validation_error", new object[] { this._name, exception.Message }), exception);
            }
        }

        private void ValidatePropertyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("String_null_or_empty"), "name");
            }
            if (BaseConfigurationRecord.IsReservedAttributeName(name))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Property_name_reserved", new object[] { name }));
            }
        }

        internal string AddElementName
        {
            get
            {
                return this._addElementName;
            }
        }

        internal string ClearElementName
        {
            get
            {
                return this._clearElementName;
            }
        }

        public TypeConverter Converter
        {
            get
            {
                this.CreateConverter();
                return this._converter;
            }
        }

        public object DefaultValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._defaultValue;
            }
        }

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._description;
            }
        }

        public bool IsAssemblyStringTransformationRequired
        {
            get
            {
                return ((this._options & ConfigurationPropertyOptions.IsAssemblyStringTransformationRequired) != ConfigurationPropertyOptions.None);
            }
        }

        internal bool IsConfigurationElementType
        {
            get
            {
                if (!this._isTypeInited)
                {
                    this._isConfigurationElementType = typeof(ConfigurationElement).IsAssignableFrom(this._type);
                    this._isTypeInited = true;
                }
                return this._isConfigurationElementType;
            }
        }

        public bool IsDefaultCollection
        {
            get
            {
                return ((this._options & ConfigurationPropertyOptions.IsDefaultCollection) != ConfigurationPropertyOptions.None);
            }
        }

        public bool IsKey
        {
            get
            {
                return ((this._options & ConfigurationPropertyOptions.IsKey) != ConfigurationPropertyOptions.None);
            }
        }

        public bool IsRequired
        {
            get
            {
                return ((this._options & ConfigurationPropertyOptions.IsRequired) != ConfigurationPropertyOptions.None);
            }
        }

        public bool IsTypeStringTransformationRequired
        {
            get
            {
                return ((this._options & ConfigurationPropertyOptions.IsTypeStringTransformationRequired) != ConfigurationPropertyOptions.None);
            }
        }

        public bool IsVersionCheckRequired
        {
            get
            {
                return ((this._options & ConfigurationPropertyOptions.IsVersionCheckRequired) != ConfigurationPropertyOptions.None);
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
        }

        internal string ProvidedName
        {
            get
            {
                return this._providedName;
            }
        }

        internal string RemoveElementName
        {
            get
            {
                return this._removeElementName;
            }
        }

        public System.Type Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._type;
            }
        }

        public ConfigurationValidatorBase Validator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._validator;
            }
        }
    }
}


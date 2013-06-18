namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    public abstract class ConfigurationElement
    {
        private bool _bDataToWrite;
        private bool _bElementPresent;
        private bool _bInited;
        private bool _bModified;
        private bool _bReadOnly;
        internal BaseConfigurationRecord _configRecord;
        private ConfigurationElementProperty _elementProperty = s_ElementProperty;
        private string _elementTagName;
        internal ContextInformation _evalContext;
        private System.Configuration.ElementInformation _evaluationElement;
        internal ConfigurationValueFlags _fItemLocked;
        internal ConfigurationLockCollection _lockedAllExceptAttributesList;
        internal ConfigurationLockCollection _lockedAllExceptElementsList;
        internal ConfigurationLockCollection _lockedAttributesList;
        internal ConfigurationLockCollection _lockedElementsList;
        private ConfigurationValues _values = new ConfigurationValues();
        internal const string DefaultCollectionPropertyName = "";
        private const string LockAll = "*";
        private const string LockAllAttributesExceptKey = "lockAllAttributesExcept";
        private const string LockAllElementsExceptKey = "lockAllElementsExcept";
        private const string LockAttributesKey = "lockAttributes";
        private const string LockElementsKey = "lockElements";
        private const string LockItemKey = "lockItem";
        private static ConfigurationElementProperty s_ElementProperty = new ConfigurationElementProperty(new DefaultValidator());
        private static string[] s_lockAttributeNames = new string[] { "lockAttributes", "lockAllAttributesExcept", "lockElements", "lockAllElementsExcept", "lockItem" };
        internal static readonly object s_nullPropertyValue = new object();
        private static Dictionary<Type, ConfigurationValidatorBase> s_perTypeValidators;
        private static Hashtable s_propertyBags = new Hashtable();

        protected ConfigurationElement()
        {
            ApplyValidator(this);
        }

        private static void ApplyInstanceAttributes(object instance)
        {
            foreach (PropertyInfo info in instance.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                ConfigurationPropertyAttribute customAttribute = Attribute.GetCustomAttribute(info, typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;
                if (customAttribute != null)
                {
                    Type propertyType = info.PropertyType;
                    if (typeof(ConfigurationElementCollection).IsAssignableFrom(propertyType))
                    {
                        ConfigurationCollectionAttribute attribute2 = Attribute.GetCustomAttribute(info, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                        if (attribute2 == null)
                        {
                            attribute2 = Attribute.GetCustomAttribute(propertyType, typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                        }
                        ConfigurationElementCollection elements = info.GetValue(instance, null) as ConfigurationElementCollection;
                        if (elements == null)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_element_null_instance", new object[] { info.Name, customAttribute.Name }));
                        }
                        if (attribute2 != null)
                        {
                            if (attribute2.AddItemName.IndexOf(',') == -1)
                            {
                                elements.AddElementName = attribute2.AddItemName;
                            }
                            elements.RemoveElementName = attribute2.RemoveItemName;
                            elements.ClearElementName = attribute2.ClearItemsName;
                        }
                    }
                    else if (typeof(ConfigurationElement).IsAssignableFrom(propertyType))
                    {
                        object obj2 = info.GetValue(instance, null);
                        if (obj2 == null)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_element_null_instance", new object[] { info.Name, customAttribute.Name }));
                        }
                        ApplyInstanceAttributes(obj2);
                    }
                }
            }
        }

        private static void ApplyValidator(ConfigurationElement elem)
        {
            if ((s_perTypeValidators != null) && s_perTypeValidators.ContainsKey(elem.GetType()))
            {
                elem._elementProperty = new ConfigurationElementProperty(s_perTypeValidators[elem.GetType()]);
            }
        }

        private static void ApplyValidatorsRecursive(ConfigurationElement root)
        {
            ApplyValidator(root);
            foreach (ConfigurationElement element in root._values.ConfigurationElements)
            {
                ApplyValidatorsRecursive(element);
            }
        }

        internal virtual void AssociateContext(BaseConfigurationRecord configRecord)
        {
            this._configRecord = configRecord;
            this.Values.AssociateContext(configRecord);
        }

        private static void CachePerTypeValidator(Type type, ConfigurationValidatorBase validator)
        {
            if (s_perTypeValidators == null)
            {
                s_perTypeValidators = new Dictionary<Type, ConfigurationValidatorBase>();
            }
            if (!validator.CanValidate(type))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Validator_does_not_support_elem_type", new object[] { type.Name }));
            }
            s_perTypeValidators.Add(type, validator);
        }

        internal void CallInit()
        {
            if (!this._bInited)
            {
                this.Init();
                this._bInited = true;
            }
        }

        internal void CheckLockedElement(string elementName, XmlReader reader)
        {
            if ((elementName != null) && ((((this._lockedElementsList != null) && (this._lockedElementsList.DefinedInParent("*") || this._lockedElementsList.DefinedInParent(elementName))) || (((this._lockedAllExceptElementsList != null) && (this._lockedAllExceptElementsList.Count != 0)) && (this._lockedAllExceptElementsList.HasParentElements && !this._lockedAllExceptElementsList.DefinedInParent(elementName)))) || ((this._fItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default)))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_element_locked", new object[] { elementName }), reader);
            }
        }

        private static ConfigurationProperty CreateConfigurationPropertyFromAttributes(PropertyInfo propertyInformation)
        {
            ConfigurationProperty property = null;
            if (Attribute.GetCustomAttribute(propertyInformation, typeof(ConfigurationPropertyAttribute)) is ConfigurationPropertyAttribute)
            {
                property = new ConfigurationProperty(propertyInformation);
            }
            if ((property != null) && typeof(ConfigurationElement).IsAssignableFrom(property.Type))
            {
                ConfigurationPropertyCollection result = null;
                PropertiesFromType(property.Type, out result);
            }
            return property;
        }

        internal static ConfigurationElement CreateElement(Type type)
        {
            ConfigurationElement element = (ConfigurationElement) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(type);
            element.CallInit();
            return element;
        }

        private static ConfigurationPropertyCollection CreatePropertyBagFromType(Type type)
        {
            if (typeof(ConfigurationElement).IsAssignableFrom(type))
            {
                ConfigurationValidatorAttribute customAttribute = Attribute.GetCustomAttribute(type, typeof(ConfigurationValidatorAttribute)) as ConfigurationValidatorAttribute;
                if (customAttribute != null)
                {
                    ConfigurationValidatorBase validatorInstance = customAttribute.ValidatorInstance;
                    if (validatorInstance != null)
                    {
                        CachePerTypeValidator(type, validatorInstance);
                    }
                }
            }
            ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                ConfigurationProperty property = CreateConfigurationPropertyFromAttributes(info);
                if (property != null)
                {
                    propertys.Add(property);
                }
            }
            return propertys;
        }

        protected internal virtual void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            ConfigurationPropertyCollection properties = this.Properties;
            ConfigurationValue value2 = null;
            ConfigurationValue value3 = null;
            ConfigurationValue value4 = null;
            ConfigurationValue value5 = null;
            bool flag = false;
            this._bElementPresent = true;
            ConfigurationElement element = null;
            ConfigurationProperty property = (properties != null) ? properties.DefaultCollectionProperty : null;
            if (property != null)
            {
                element = (ConfigurationElement) this[property];
            }
            this._elementTagName = reader.Name;
            PropertySourceInfo sourceInfo = new PropertySourceInfo(reader);
            this._values.SetValue(reader.Name, null, ConfigurationValueFlags.Modified, sourceInfo);
            this._values.SetValue("", element, ConfigurationValueFlags.Modified, sourceInfo);
            if (((this._lockedElementsList != null) && (this._lockedElementsList.Contains(reader.Name) || (this._lockedElementsList.Contains("*") && (reader.Name != this.ElementTagName)))) || ((((this._lockedAllExceptElementsList != null) && (this._lockedAllExceptElementsList.Count != 0)) && !this._lockedAllExceptElementsList.Contains(reader.Name)) || (((this._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) && ((this._fItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default))))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_element_locked", new object[] { reader.Name }), reader);
            }
            if (reader.AttributeCount > 0)
            {
                while (reader.MoveToNextAttribute())
                {
                    string name = reader.Name;
                    if ((((this._lockedAttributesList != null) && (this._lockedAttributesList.Contains(name) || this._lockedAttributesList.Contains("*"))) || ((this._lockedAllExceptAttributesList != null) && !this._lockedAllExceptAttributesList.Contains(name))) && ((name != "lockAttributes") && (name != "lockAllAttributesExcept")))
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { name }), reader);
                    }
                    ConfigurationProperty prop = (properties != null) ? properties[name] : null;
                    if (prop != null)
                    {
                        if (serializeCollectionKey && !prop.IsKey)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_attribute", new object[] { name }), reader);
                        }
                        this._values.SetValue(name, this.DeserializePropertyValue(prop, reader), ConfigurationValueFlags.Modified, new PropertySourceInfo(reader));
                    }
                    else
                    {
                        if (name == "lockItem")
                        {
                            try
                            {
                                flag = bool.Parse(reader.Value);
                                continue;
                            }
                            catch
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_invalid_boolean_attribute", new object[] { name }), reader);
                            }
                        }
                        if (name == "lockAttributes")
                        {
                            value2 = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                        }
                        else
                        {
                            if (name == "lockAllAttributesExcept")
                            {
                                value3 = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                                continue;
                            }
                            if (name == "lockElements")
                            {
                                value4 = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                                continue;
                            }
                            if (name == "lockAllElementsExcept")
                            {
                                value5 = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                                continue;
                            }
                            if (serializeCollectionKey || !this.OnDeserializeUnrecognizedAttribute(name, reader.Value))
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_attribute", new object[] { name }), reader);
                            }
                        }
                    }
                }
            }
            reader.MoveToElement();
            try
            {
                HybridDictionary dictionary = new HybridDictionary();
                if (!reader.IsEmptyElement)
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string elementName = reader.Name;
                            this.CheckLockedElement(elementName, null);
                            ConfigurationProperty property3 = (properties != null) ? properties[elementName] : null;
                            if (property3 == null)
                            {
                                if (!this.OnDeserializeUnrecognizedElement(elementName, reader) && ((element == null) || !element.OnDeserializeUnrecognizedElement(elementName, reader)))
                                {
                                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_element_name", new object[] { elementName }), reader);
                                }
                            }
                            else
                            {
                                if (!property3.IsConfigurationElementType)
                                {
                                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_property_is_not_a_configuration_element", new object[] { elementName }), reader);
                                }
                                if (dictionary.Contains(elementName))
                                {
                                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_element_cannot_have_multiple_child_elements", new object[] { elementName }), reader);
                                }
                                dictionary.Add(elementName, elementName);
                                ConfigurationElement elem = (ConfigurationElement) this[property3];
                                elem.DeserializeElement(reader, serializeCollectionKey);
                                ValidateElement(elem, property3.Validator, false);
                            }
                        }
                        else
                        {
                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                            if ((reader.NodeType == XmlNodeType.CDATA) || (reader.NodeType == XmlNodeType.Text))
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_section_invalid_content"), reader);
                            }
                        }
                    }
                }
                this.EnsureRequiredProperties(serializeCollectionKey);
                ValidateElement(this, null, false);
            }
            catch (ConfigurationException exception)
            {
                if ((exception.Filename != null) && (exception.Filename.Length != 0))
                {
                    throw exception;
                }
                throw new ConfigurationErrorsException(exception.Message, reader);
            }
            if (flag)
            {
                this.SetLocked();
                this._fItemLocked = ConfigurationValueFlags.Locked;
            }
            if (value2 != null)
            {
                if (this._lockedAttributesList == null)
                {
                    this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                }
                foreach (string str3 in this.ParseLockedAttributes(value2, ConfigurationLockCollectionType.LockedAttributes))
                {
                    if (!this._lockedAttributesList.Contains(str3))
                    {
                        this._lockedAttributesList.Add(str3, ConfigurationValueFlags.Default);
                    }
                    else
                    {
                        this._lockedAttributesList.Add(str3, ConfigurationValueFlags.Modified | ConfigurationValueFlags.Inherited);
                    }
                }
            }
            if (value3 != null)
            {
                ConfigurationLockCollection parentCollection = this.ParseLockedAttributes(value3, ConfigurationLockCollectionType.LockedExceptionList);
                if (this._lockedAllExceptAttributesList == null)
                {
                    this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, string.Empty, parentCollection);
                    this._lockedAllExceptAttributesList.ClearSeedList();
                }
                StringCollection strings = this.IntersectLockCollections(this._lockedAllExceptAttributesList, parentCollection);
                this._lockedAllExceptAttributesList.ClearInternal(false);
                foreach (string str4 in strings)
                {
                    this._lockedAllExceptAttributesList.Add(str4, ConfigurationValueFlags.Default);
                }
            }
            if (value4 != null)
            {
                if (this._lockedElementsList == null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                ConfigurationLockCollection locks2 = this.ParseLockedAttributes(value4, ConfigurationLockCollectionType.LockedElements);
                ConfigurationElementCollection elements = null;
                if (properties.DefaultCollectionProperty != null)
                {
                    elements = this[properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                    if ((elements != null) && (elements._lockedElementsList == null))
                    {
                        elements._lockedElementsList = this._lockedElementsList;
                    }
                }
                foreach (string str5 in locks2)
                {
                    if (!this._lockedElementsList.Contains(str5))
                    {
                        this._lockedElementsList.Add(str5, ConfigurationValueFlags.Default);
                        ConfigurationProperty property4 = this.Properties[str5];
                        if ((property4 != null) && typeof(ConfigurationElement).IsAssignableFrom(property4.Type))
                        {
                            ((ConfigurationElement) this[str5]).SetLocked();
                        }
                        if (str5 == "*")
                        {
                            foreach (ConfigurationProperty property5 in this.Properties)
                            {
                                if (!string.IsNullOrEmpty(property5.Name) && property5.IsConfigurationElementType)
                                {
                                    ((ConfigurationElement) this[property5]).SetLocked();
                                }
                            }
                        }
                    }
                }
            }
            if (value5 != null)
            {
                ConfigurationLockCollection locks3 = this.ParseLockedAttributes(value5, ConfigurationLockCollectionType.LockedElementsExceptionList);
                if (this._lockedAllExceptElementsList == null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, this._elementTagName, locks3);
                    this._lockedAllExceptElementsList.ClearSeedList();
                }
                StringCollection strings2 = this.IntersectLockCollections(this._lockedAllExceptElementsList, locks3);
                ConfigurationElementCollection elements2 = null;
                if (properties.DefaultCollectionProperty != null)
                {
                    elements2 = this[properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                    if ((elements2 != null) && (elements2._lockedAllExceptElementsList == null))
                    {
                        elements2._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
                    }
                }
                this._lockedAllExceptElementsList.ClearInternal(false);
                foreach (string str6 in strings2)
                {
                    if (!this._lockedAllExceptElementsList.Contains(str6) || (str6 == this.ElementTagName))
                    {
                        this._lockedAllExceptElementsList.Add(str6, ConfigurationValueFlags.Default);
                    }
                }
                foreach (ConfigurationProperty property6 in this.Properties)
                {
                    if ((!string.IsNullOrEmpty(property6.Name) && !this._lockedAllExceptElementsList.Contains(property6.Name)) && property6.IsConfigurationElementType)
                    {
                        ((ConfigurationElement) this[property6]).SetLocked();
                    }
                }
            }
            if (property != null)
            {
                element = (ConfigurationElement) this[property];
                if (this._lockedElementsList == null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                element._lockedElementsList = this._lockedElementsList;
                if (this._lockedAllExceptElementsList == null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, reader.Name);
                    this._lockedAllExceptElementsList.ClearSeedList();
                }
                element._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
            }
            this.PostDeserialize();
        }

        private object DeserializePropertyValue(ConfigurationProperty prop, XmlReader reader)
        {
            string str = reader.Value;
            object obj2 = null;
            try
            {
                obj2 = prop.ConvertFromString(str);
                prop.Validate(obj2);
            }
            catch (ConfigurationException exception)
            {
                if (string.IsNullOrEmpty(exception.Filename))
                {
                    exception = new ConfigurationErrorsException(exception.Message, reader);
                }
                obj2 = new InvalidPropValue(str, exception);
            }
            catch
            {
            }
            return obj2;
        }

        internal virtual void Dump(TextWriter tw)
        {
            tw.WriteLine("Type: " + base.GetType().FullName);
            foreach (PropertyInfo info in base.GetType().GetProperties())
            {
                tw.WriteLine("{0}: {1}", info.Name, info.GetValue(this, null));
            }
        }

        private void EnsureRequiredProperties(bool ensureKeysOnly)
        {
            ConfigurationPropertyCollection properties = this.Properties;
            if (properties != null)
            {
                foreach (ConfigurationProperty property in properties)
                {
                    if ((property.IsRequired && !this._values.Contains(property.Name)) && (!ensureKeysOnly || property.IsKey))
                    {
                        this._values[property.Name] = this.OnRequiredPropertyNotFound(property.Name);
                    }
                }
            }
        }

        public override bool Equals(object compareTo)
        {
            ConfigurationElement element = compareTo as ConfigurationElement;
            if (((element == null) || (compareTo.GetType() != base.GetType())) || ((element != null) && (element.Properties.Count != this.Properties.Count)))
            {
                return false;
            }
            foreach (ConfigurationProperty property in this.Properties)
            {
                if ((!object.Equals(this.Values[property.Name], element.Values[property.Name]) && (((this.Values[property.Name] != null) && (this.Values[property.Name] != s_nullPropertyValue)) || !object.Equals(element.Values[property.Name], property.DefaultValue))) && (((element.Values[property.Name] != null) && (element.Values[property.Name] != s_nullPropertyValue)) || !object.Equals(this.Values[property.Name], property.DefaultValue)))
                {
                    return false;
                }
            }
            return true;
        }

        internal ConfigurationErrorsException GetErrors()
        {
            ArrayList errorsList = this.GetErrorsList();
            if (errorsList.Count == 0)
            {
                return null;
            }
            return new ConfigurationErrorsException(errorsList);
        }

        internal ArrayList GetErrorsList()
        {
            ArrayList errorList = new ArrayList();
            this.ListErrors(errorList);
            return errorList;
        }

        public override int GetHashCode()
        {
            int num = 0;
            foreach (ConfigurationProperty property in this.Properties)
            {
                if (this[property] != null)
                {
                    num ^= this[property].GetHashCode();
                }
            }
            return num;
        }

        protected virtual string GetTransformedAssemblyString(string assemblyName)
        {
            if (((assemblyName != null) && (this._configRecord != null)) && this._configRecord.AssemblyStringTransformerIsSet)
            {
                return this._configRecord.AssemblyStringTransformer(assemblyName);
            }
            return assemblyName;
        }

        protected virtual string GetTransformedTypeString(string typeName)
        {
            if (((typeName != null) && (this._configRecord != null)) && this._configRecord.TypeStringTransformerIsSet)
            {
                return this._configRecord.TypeStringTransformer(typeName);
            }
            return typeName;
        }

        internal void HandleLockedAttributes(ConfigurationElement source)
        {
            if ((source != null) && ((source._lockedAttributesList != null) || (source._lockedAllExceptAttributesList != null)))
            {
                foreach (PropertyInformation information in source.ElementInformation.Properties)
                {
                    if ((((source._lockedAttributesList != null) && (source._lockedAttributesList.Contains(information.Name) || source._lockedAttributesList.Contains("*"))) || ((source._lockedAllExceptAttributesList != null) && !source._lockedAllExceptAttributesList.Contains(information.Name))) && ((information.Name != "lockAttributes") && (information.Name != "lockAllAttributesExcept")))
                    {
                        if (this.ElementInformation.Properties[information.Name] == null)
                        {
                            ConfigurationPropertyCollection properties = this.Properties;
                            ConfigurationProperty property = source.Properties[information.Name];
                            properties.Add(property);
                            this._evaluationElement = null;
                            ConfigurationValueFlags valueFlags = ConfigurationValueFlags.Locked | ConfigurationValueFlags.Inherited;
                            this._values.SetValue(information.Name, information.Value, valueFlags, source.PropertyInfoInternal(information.Name));
                        }
                        else
                        {
                            if (this.ElementInformation.Properties[information.Name].ValueOrigin == PropertyValueOrigin.SetHere)
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { information.Name }));
                            }
                            this.ElementInformation.Properties[information.Name].Value = information.Value;
                        }
                    }
                }
            }
        }

        protected internal virtual void Init()
        {
            this._bInited = true;
        }

        protected internal virtual void InitializeDefault()
        {
        }

        private StringCollection IntersectLockCollections(ConfigurationLockCollection Collection1, ConfigurationLockCollection Collection2)
        {
            ConfigurationLockCollection locks = (Collection1.Count < Collection2.Count) ? Collection1 : Collection2;
            ConfigurationLockCollection locks2 = (Collection1.Count >= Collection2.Count) ? Collection1 : Collection2;
            StringCollection strings = new StringCollection();
            foreach (string str in locks)
            {
                if (locks2.Contains(str) || (str == this.ElementTagName))
                {
                    strings.Add(str);
                }
            }
            return strings;
        }

        internal static bool IsLockAttributeName(string name)
        {
            if (StringUtil.StartsWith(name, "lock"))
            {
                foreach (string str in s_lockAttributeNames)
                {
                    if (name == str)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected internal virtual bool IsModified()
        {
            if (this._bModified)
            {
                return true;
            }
            if ((this._lockedAttributesList != null) && this._lockedAttributesList.IsModified)
            {
                return true;
            }
            if ((this._lockedAllExceptAttributesList != null) && this._lockedAllExceptAttributesList.IsModified)
            {
                return true;
            }
            if ((this._lockedElementsList != null) && this._lockedElementsList.IsModified)
            {
                return true;
            }
            if ((this._lockedAllExceptElementsList != null) && this._lockedAllExceptElementsList.IsModified)
            {
                return true;
            }
            if ((this._fItemLocked & ConfigurationValueFlags.Modified) != ConfigurationValueFlags.Default)
            {
                return true;
            }
            foreach (ConfigurationElement element in this._values.ConfigurationElements)
            {
                if (element.IsModified())
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool IsReadOnly()
        {
            return this._bReadOnly;
        }

        protected virtual void ListErrors(IList errorList)
        {
            foreach (InvalidPropValue value2 in this._values.InvalidValues)
            {
                errorList.Add(value2.Error);
            }
            foreach (ConfigurationElement element in this._values.ConfigurationElements)
            {
                element.ListErrors(errorList);
                ConfigurationElementCollection elements = element as ConfigurationElementCollection;
                if (elements != null)
                {
                    foreach (ConfigurationElement element2 in elements)
                    {
                        element2.ListErrors(errorList);
                    }
                }
            }
        }

        internal void MergeLocks(ConfigurationElement source)
        {
            if (source != null)
            {
                this._fItemLocked = ((source._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) ? (ConfigurationValueFlags.Inherited | source._fItemLocked) : this._fItemLocked;
                if (source._lockedAttributesList != null)
                {
                    if (this._lockedAttributesList == null)
                    {
                        this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                    }
                    foreach (string str in source._lockedAttributesList)
                    {
                        this._lockedAttributesList.Add(str, ConfigurationValueFlags.Inherited);
                    }
                }
                if (source._lockedAllExceptAttributesList != null)
                {
                    if (this._lockedAllExceptAttributesList == null)
                    {
                        this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, string.Empty, source._lockedAllExceptAttributesList);
                    }
                    StringCollection strings = this.IntersectLockCollections(this._lockedAllExceptAttributesList, source._lockedAllExceptAttributesList);
                    this._lockedAllExceptAttributesList.ClearInternal(false);
                    foreach (string str2 in strings)
                    {
                        this._lockedAllExceptAttributesList.Add(str2, ConfigurationValueFlags.Default);
                    }
                }
                if (source._lockedElementsList != null)
                {
                    if (this._lockedElementsList == null)
                    {
                        this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                    }
                    ConfigurationElementCollection elements = null;
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        elements = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (elements != null)
                        {
                            elements.internalElementTagName = source.ElementTagName;
                            if (elements._lockedElementsList == null)
                            {
                                elements._lockedElementsList = this._lockedElementsList;
                            }
                        }
                    }
                    foreach (string str3 in source._lockedElementsList)
                    {
                        this._lockedElementsList.Add(str3, ConfigurationValueFlags.Inherited);
                        if (elements != null)
                        {
                            elements._lockedElementsList.Add(str3, ConfigurationValueFlags.Inherited);
                        }
                    }
                }
                if (source._lockedAllExceptElementsList != null)
                {
                    if ((this._lockedAllExceptElementsList == null) || (this._lockedAllExceptElementsList.Count == 0))
                    {
                        this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, source._elementTagName, source._lockedAllExceptElementsList);
                    }
                    StringCollection strings2 = this.IntersectLockCollections(this._lockedAllExceptElementsList, source._lockedAllExceptElementsList);
                    ConfigurationElementCollection elements2 = null;
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        elements2 = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if ((elements2 != null) && (elements2._lockedAllExceptElementsList == null))
                        {
                            elements2._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
                        }
                    }
                    this._lockedAllExceptElementsList.ClearInternal(false);
                    foreach (string str4 in strings2)
                    {
                        if (!this._lockedAllExceptElementsList.Contains(str4) || (str4 == this.ElementTagName))
                        {
                            this._lockedAllExceptElementsList.Add(str4, ConfigurationValueFlags.Default);
                        }
                    }
                    if (this._lockedAllExceptElementsList.HasParentElements)
                    {
                        foreach (ConfigurationProperty property in this.Properties)
                        {
                            if (!this._lockedAllExceptElementsList.Contains(property.Name) && property.IsConfigurationElementType)
                            {
                                ((ConfigurationElement) this[property]).SetLocked();
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            return false;
        }

        protected virtual bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            return false;
        }

        protected virtual object OnRequiredPropertyNotFound(string name)
        {
            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_required_attribute_missing", new object[] { name }), this.PropertyFileName(name), this.PropertyLineNumber(name));
        }

        private ConfigurationLockCollection ParseLockedAttributes(ConfigurationValue value, ConfigurationLockCollectionType lockType)
        {
            ConfigurationLockCollection locks = new ConfigurationLockCollection(this, lockType);
            string str = (string) value.Value;
            if (string.IsNullOrEmpty(str))
            {
                if (lockType == ConfigurationLockCollectionType.LockedAttributes)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Empty_attribute", new object[] { "lockAttributes" }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
                if (lockType == ConfigurationLockCollectionType.LockedElements)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Empty_attribute", new object[] { "lockElements" }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
                if (lockType == ConfigurationLockCollectionType.LockedExceptionList)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_empty_lock_attributes_except", new object[] { "lockAllAttributesExcept", "lockAttributes" }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
                if (lockType == ConfigurationLockCollectionType.LockedElementsExceptionList)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_empty_lock_element_except", new object[] { "lockAllElementsExcept", "lockElements" }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                }
            }
            foreach (string str2 in str.Split(new char[] { ',', ':', ';' }))
            {
                string str3 = str2.Trim();
                if (!string.IsNullOrEmpty(str3))
                {
                    if (((lockType != ConfigurationLockCollectionType.LockedElements) && (lockType != ConfigurationLockCollectionType.LockedAttributes)) || (str3 != "*"))
                    {
                        ConfigurationProperty property = this.Properties[str3];
                        if (((((property == null) || (str3 == "lockAttributes")) || ((str3 == "lockAllAttributesExcept") || (str3 == "lockElements"))) || (((lockType != ConfigurationLockCollectionType.LockedElements) && (lockType != ConfigurationLockCollectionType.LockedElementsExceptionList)) && typeof(ConfigurationElement).IsAssignableFrom(property.Type))) || (((lockType == ConfigurationLockCollectionType.LockedElements) || (lockType == ConfigurationLockCollectionType.LockedElementsExceptionList)) && !typeof(ConfigurationElement).IsAssignableFrom(property.Type)))
                        {
                            ConfigurationElementCollection elements = this as ConfigurationElementCollection;
                            if ((elements == null) && (this.Properties.DefaultCollectionProperty != null))
                            {
                                elements = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                            }
                            if (((elements == null) || (lockType == ConfigurationLockCollectionType.LockedAttributes)) || (lockType == ConfigurationLockCollectionType.LockedExceptionList))
                            {
                                this.ReportInvalidLock(str3, lockType, value, null);
                            }
                            else if (!elements.IsLockableElement(str3))
                            {
                                this.ReportInvalidLock(str3, lockType, value, elements.LockableElements);
                            }
                        }
                        if ((property != null) && property.IsRequired)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_required_attribute_lock_attempt", new object[] { property.Name }));
                        }
                    }
                    locks.Add(str3, ConfigurationValueFlags.Default);
                }
            }
            return locks;
        }

        protected virtual void PostDeserialize()
        {
        }

        protected virtual void PreSerialize(XmlWriter writer)
        {
        }

        private static bool PropertiesFromType(Type type, out ConfigurationPropertyCollection result)
        {
            ConfigurationPropertyCollection propertys = (ConfigurationPropertyCollection) s_propertyBags[type];
            result = null;
            bool flag = false;
            if (propertys == null)
            {
                lock (s_propertyBags.SyncRoot)
                {
                    propertys = (ConfigurationPropertyCollection) s_propertyBags[type];
                    if (propertys == null)
                    {
                        propertys = CreatePropertyBagFromType(type);
                        s_propertyBags[type] = propertys;
                        flag = true;
                    }
                }
            }
            result = propertys;
            return flag;
        }

        internal string PropertyFileName(string propertyName)
        {
            PropertySourceInfo info = this.PropertyInfoInternal(propertyName);
            if (info == null)
            {
                info = this.PropertyInfoInternal(string.Empty);
            }
            if (info == null)
            {
                return string.Empty;
            }
            return info.FileName;
        }

        internal PropertySourceInfo PropertyInfoInternal(string propertyName)
        {
            return this._values.GetSourceInfo(propertyName);
        }

        internal int PropertyLineNumber(string propertyName)
        {
            PropertySourceInfo info = this.PropertyInfoInternal(propertyName);
            if (info == null)
            {
                info = this.PropertyInfoInternal(string.Empty);
            }
            if (info == null)
            {
                return 0;
            }
            return info.LineNumber;
        }

        internal void RemoveAllInheritedLocks()
        {
            if (this._lockedAttributesList != null)
            {
                this._lockedAttributesList.RemoveInheritedLocks();
            }
            if (this._lockedElementsList != null)
            {
                this._lockedElementsList.RemoveInheritedLocks();
            }
            if (this._lockedAllExceptAttributesList != null)
            {
                this._lockedAllExceptAttributesList.RemoveInheritedLocks();
            }
            if (this._lockedAllExceptElementsList != null)
            {
                this._lockedAllExceptElementsList.RemoveInheritedLocks();
            }
        }

        internal void ReportInvalidLock(string attribToLockTrim, ConfigurationLockCollectionType lockedType, ConfigurationValue value, string collectionProperties)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(collectionProperties) && ((lockedType == ConfigurationLockCollectionType.LockedElements) || (lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList)))
            {
                if (builder.Length != 0)
                {
                    builder.Append(',');
                }
                builder.Append(collectionProperties);
            }
            foreach (object obj2 in this.Properties)
            {
                ConfigurationProperty property = (ConfigurationProperty) obj2;
                if (((property.Name != "lockAttributes") && (property.Name != "lockAllAttributesExcept")) && ((property.Name != "lockElements") && (property.Name != "lockAllElementsExcept")))
                {
                    if ((lockedType == ConfigurationLockCollectionType.LockedElements) || (lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList))
                    {
                        if (typeof(ConfigurationElement).IsAssignableFrom(property.Type))
                        {
                            if (builder.Length != 0)
                            {
                                builder.Append(", ");
                            }
                            builder.Append("'");
                            builder.Append(property.Name);
                            builder.Append("'");
                        }
                    }
                    else if (!typeof(ConfigurationElement).IsAssignableFrom(property.Type))
                    {
                        if (builder.Length != 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append("'");
                        builder.Append(property.Name);
                        builder.Append("'");
                    }
                }
            }
            string format = null;
            if ((lockedType == ConfigurationLockCollectionType.LockedElements) || (lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList))
            {
                if (value != null)
                {
                    format = System.Configuration.SR.GetString("Config_base_invalid_element_to_lock");
                }
                else
                {
                    format = System.Configuration.SR.GetString("Config_base_invalid_element_to_lock_by_add");
                }
            }
            else if (value != null)
            {
                format = System.Configuration.SR.GetString("Config_base_invalid_attribute_to_lock");
            }
            else
            {
                format = System.Configuration.SR.GetString("Config_base_invalid_attribute_to_lock_by_add");
            }
            if (value != null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, format, new object[] { attribToLockTrim, builder.ToString() }), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
            }
            throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, format, new object[] { attribToLockTrim, builder.ToString() }));
        }

        protected internal virtual void Reset(ConfigurationElement parentElement)
        {
            this.Values.Clear();
            this.ResetLockLists(parentElement);
            ConfigurationPropertyCollection properties = this.Properties;
            this._bElementPresent = false;
            if (parentElement == null)
            {
                this.InitializeDefault();
            }
            else
            {
                bool flag = false;
                ConfigurationPropertyCollection propertys2 = null;
                for (int i = 0; i < parentElement.Values.Count; i++)
                {
                    string key = parentElement.Values.GetKey(i);
                    ConfigurationValue configValue = parentElement.Values.GetConfigValue(i);
                    object obj2 = (configValue != null) ? configValue.Value : null;
                    PropertySourceInfo sourceInfo = (configValue != null) ? configValue.SourceInfo : null;
                    ConfigurationProperty property = parentElement.Properties[key];
                    if ((property != null) && ((propertys2 == null) || propertys2.Contains(property.Name)))
                    {
                        if (property.IsConfigurationElementType)
                        {
                            flag = true;
                        }
                        else
                        {
                            ConfigurationValueFlags valueFlags = ConfigurationValueFlags.Inherited | ((((this._lockedAttributesList != null) && (this._lockedAttributesList.Contains(key) || this._lockedAttributesList.Contains("*"))) || ((this._lockedAllExceptAttributesList != null) && !this._lockedAllExceptAttributesList.Contains(key))) ? ConfigurationValueFlags.Locked : ConfigurationValueFlags.Default);
                            if (obj2 != s_nullPropertyValue)
                            {
                                this._values.SetValue(key, obj2, valueFlags, sourceInfo);
                            }
                            if (!properties.Contains(key))
                            {
                                properties.Add(property);
                                this._values.SetValue(key, obj2, valueFlags, sourceInfo);
                            }
                        }
                    }
                }
                if (flag)
                {
                    for (int j = 0; j < parentElement.Values.Count; j++)
                    {
                        string str2 = parentElement.Values.GetKey(j);
                        object obj3 = parentElement.Values[j];
                        ConfigurationProperty property2 = parentElement.Properties[str2];
                        if ((property2 != null) && property2.IsConfigurationElementType)
                        {
                            ((ConfigurationElement) this[property2]).Reset((ConfigurationElement) obj3);
                        }
                    }
                }
            }
        }

        internal void ResetLockLists(ConfigurationElement parentElement)
        {
            this._lockedAttributesList = null;
            this._lockedAllExceptAttributesList = null;
            this._lockedElementsList = null;
            this._lockedAllExceptElementsList = null;
            if (parentElement != null)
            {
                this._fItemLocked = ((parentElement._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) ? (ConfigurationValueFlags.Inherited | parentElement._fItemLocked) : ConfigurationValueFlags.Default;
                if (parentElement._lockedAttributesList != null)
                {
                    this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                    foreach (string str in parentElement._lockedAttributesList)
                    {
                        this._lockedAttributesList.Add(str, ConfigurationValueFlags.Inherited);
                    }
                }
                if (parentElement._lockedAllExceptAttributesList != null)
                {
                    this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, string.Empty, parentElement._lockedAllExceptAttributesList);
                }
                if (parentElement._lockedElementsList != null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                    ConfigurationElementCollection elements = null;
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        elements = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (elements != null)
                        {
                            elements.internalElementTagName = parentElement.ElementTagName;
                            if (elements._lockedElementsList == null)
                            {
                                elements._lockedElementsList = this._lockedElementsList;
                            }
                        }
                    }
                    foreach (string str2 in parentElement._lockedElementsList)
                    {
                        this._lockedElementsList.Add(str2, ConfigurationValueFlags.Inherited);
                    }
                }
                if (parentElement._lockedAllExceptElementsList != null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, parentElement._elementTagName, parentElement._lockedAllExceptElementsList);
                    ConfigurationElementCollection elements2 = null;
                    if (this.Properties.DefaultCollectionProperty != null)
                    {
                        elements2 = this[this.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if ((elements2 != null) && (elements2._lockedAllExceptElementsList == null))
                        {
                            elements2._lockedAllExceptElementsList = this._lockedAllExceptElementsList;
                        }
                    }
                }
            }
        }

        protected internal virtual void ResetModified()
        {
            this._bModified = false;
            if (this._lockedAttributesList != null)
            {
                this._lockedAttributesList.ResetModified();
            }
            if (this._lockedAllExceptAttributesList != null)
            {
                this._lockedAllExceptAttributesList.ResetModified();
            }
            if (this._lockedElementsList != null)
            {
                this._lockedElementsList.ResetModified();
            }
            if (this._lockedAllExceptElementsList != null)
            {
                this._lockedAllExceptElementsList.ResetModified();
            }
            foreach (ConfigurationElement element in this._values.ConfigurationElements)
            {
                element.ResetModified();
            }
        }

        protected internal virtual bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            this.PreSerialize(writer);
            bool flag = this._bDataToWrite;
            bool flag2 = false;
            bool flag3 = false;
            ConfigurationPropertyCollection properties = this.Properties;
            ConfigurationPropertyCollection propertys2 = null;
            for (int i = 0; i < this._values.Count; i++)
            {
                string key = this._values.GetKey(i);
                object obj2 = this._values[i];
                ConfigurationProperty property = properties[key];
                if ((property != null) && ((propertys2 == null) || propertys2.Contains(property.Name)))
                {
                    if ((property.IsVersionCheckRequired && (this._configRecord != null)) && (this._configRecord.TargetFramework != null))
                    {
                        ConfigurationSection section = null;
                        if (this._configRecord.SectionsStack.Count > 0)
                        {
                            section = this._configRecord.SectionsStack.Peek() as ConfigurationSection;
                        }
                        if ((section != null) && !section.ShouldSerializePropertyInTargetVersion(property, property.Name, this._configRecord.TargetFramework, this))
                        {
                            continue;
                        }
                    }
                    if (property.IsConfigurationElementType)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        if ((((this._lockedAllExceptAttributesList != null) && this._lockedAllExceptAttributesList.HasParentElements) && !this._lockedAllExceptAttributesList.DefinedInParent(property.Name)) || ((this._lockedAttributesList != null) && this._lockedAttributesList.DefinedInParent(property.Name)))
                        {
                            if (property.IsRequired)
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_required_attribute_locked", new object[] { property.Name }));
                            }
                            obj2 = s_nullPropertyValue;
                        }
                        if ((obj2 != s_nullPropertyValue) && (!serializeCollectionKey || property.IsKey))
                        {
                            string typeName = null;
                            if (obj2 is InvalidPropValue)
                            {
                                typeName = ((InvalidPropValue) obj2).Value;
                            }
                            else
                            {
                                property.Validate(obj2);
                                typeName = property.ConvertToString(obj2);
                            }
                            if ((typeName != null) && (writer != null))
                            {
                                if (property.IsTypeStringTransformationRequired)
                                {
                                    typeName = this.GetTransformedTypeString(typeName);
                                }
                                if (property.IsAssemblyStringTransformationRequired)
                                {
                                    typeName = this.GetTransformedAssemblyString(typeName);
                                }
                                writer.WriteAttributeString(property.Name, typeName);
                            }
                            flag = flag || (typeName != null);
                        }
                    }
                }
            }
            if (!serializeCollectionKey)
            {
                flag |= this.SerializeLockList(this._lockedAttributesList, "lockAttributes", writer);
                flag |= this.SerializeLockList(this._lockedAllExceptAttributesList, "lockAllAttributesExcept", writer);
                flag |= this.SerializeLockList(this._lockedElementsList, "lockElements", writer);
                flag |= this.SerializeLockList(this._lockedAllExceptElementsList, "lockAllElementsExcept", writer);
                if ((((this._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) && ((this._fItemLocked & ConfigurationValueFlags.Inherited) == ConfigurationValueFlags.Default)) && ((this._fItemLocked & ConfigurationValueFlags.XMLParentInherited) == ConfigurationValueFlags.Default))
                {
                    flag = true;
                    if (writer != null)
                    {
                        bool flag4 = true;
                        writer.WriteAttributeString("lockItem", flag4.ToString().ToLower(CultureInfo.InvariantCulture));
                    }
                }
            }
            if (flag2)
            {
                for (int j = 0; j < this._values.Count; j++)
                {
                    string name = this._values.GetKey(j);
                    object obj3 = this._values[j];
                    ConfigurationProperty property2 = properties[name];
                    if (((!serializeCollectionKey || property2.IsKey) && ((obj3 is ConfigurationElement) && ((this._lockedElementsList == null) || !this._lockedElementsList.DefinedInParent(name)))) && (((this._lockedAllExceptElementsList == null) || !this._lockedAllExceptElementsList.HasParentElements) || this._lockedAllExceptElementsList.DefinedInParent(name)))
                    {
                        ConfigurationElement element = (ConfigurationElement) obj3;
                        if (property2.Name == ConfigurationProperty.DefaultCollectionPropertyName)
                        {
                            if (flag3)
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_element_cannot_have_multiple_child_elements", new object[] { property2.Name }));
                            }
                            element._lockedAttributesList = null;
                            element._lockedAllExceptAttributesList = null;
                            element._lockedElementsList = null;
                            element._lockedAllExceptElementsList = null;
                            flag |= element.SerializeElement(writer, false);
                            flag3 = true;
                        }
                        else
                        {
                            flag |= element.SerializeToXmlElement(writer, property2.Name);
                        }
                    }
                }
            }
            return flag;
        }

        private bool SerializeLockList(ConfigurationLockCollection list, string elementKey, XmlWriter writer)
        {
            StringBuilder builder = new StringBuilder();
            if (list != null)
            {
                foreach (string str in list)
                {
                    if (!list.DefinedInParent(str))
                    {
                        if (builder.Length != 0)
                        {
                            builder.Append(',');
                        }
                        builder.Append(str);
                    }
                }
            }
            if ((writer != null) && (builder.Length != 0))
            {
                writer.WriteAttributeString(elementKey, builder.ToString());
            }
            return (builder.Length != 0);
        }

        protected internal virtual bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            if ((this._configRecord != null) && (this._configRecord.TargetFramework != null))
            {
                ConfigurationSection section = null;
                if (this._configRecord.SectionsStack.Count > 0)
                {
                    section = this._configRecord.SectionsStack.Peek() as ConfigurationSection;
                }
                if ((section != null) && !section.ShouldSerializeElementInTargetVersion(this, elementName, this._configRecord.TargetFramework))
                {
                    return false;
                }
            }
            bool flag = this._bDataToWrite;
            if ((this._lockedElementsList == null) || !this._lockedElementsList.DefinedInParent(elementName))
            {
                if (((this._lockedAllExceptElementsList != null) && this._lockedAllExceptElementsList.HasParentElements) && !this._lockedAllExceptElementsList.DefinedInParent(elementName))
                {
                    return flag;
                }
                if (this.SerializeElement(null, false))
                {
                    if (writer != null)
                    {
                        writer.WriteStartElement(elementName);
                    }
                    flag |= this.SerializeElement(writer, false);
                    if (writer != null)
                    {
                        writer.WriteEndElement();
                    }
                }
            }
            return flag;
        }

        internal void SetLocked()
        {
            this._fItemLocked = ConfigurationValueFlags.XMLParentInherited | ConfigurationValueFlags.Locked;
            foreach (ConfigurationProperty property in this.Properties)
            {
                ConfigurationElement element = this[property] as ConfigurationElement;
                if (element != null)
                {
                    if (element.GetType() != base.GetType())
                    {
                        element.SetLocked();
                    }
                    ConfigurationElementCollection elements = this[property] as ConfigurationElementCollection;
                    if (elements != null)
                    {
                        foreach (object obj2 in elements)
                        {
                            ConfigurationElement element2 = obj2 as ConfigurationElement;
                            if (element2 != null)
                            {
                                element2.SetLocked();
                            }
                        }
                    }
                }
            }
        }

        protected void SetPropertyValue(ConfigurationProperty prop, object value, bool ignoreLocks)
        {
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
            if (!ignoreLocks && (((((this._lockedAllExceptAttributesList != null) && this._lockedAllExceptAttributesList.HasParentElements) && !this._lockedAllExceptAttributesList.DefinedInParent(prop.Name)) || ((this._lockedAttributesList != null) && (this._lockedAttributesList.DefinedInParent(prop.Name) || this._lockedAttributesList.DefinedInParent("*")))) || (((this._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) && ((this._fItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default))))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { prop.Name }));
            }
            this._bModified = true;
            if (value != null)
            {
                prop.Validate(value);
            }
            this._values[prop.Name] = (value != null) ? value : s_nullPropertyValue;
        }

        protected internal virtual void SetReadOnly()
        {
            this._bReadOnly = true;
            foreach (ConfigurationElement element in this._values.ConfigurationElements)
            {
                element.SetReadOnly();
            }
        }

        protected internal virtual void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement != null)
            {
                bool flag = false;
                this._lockedAllExceptAttributesList = sourceElement._lockedAllExceptAttributesList;
                this._lockedAllExceptElementsList = sourceElement._lockedAllExceptElementsList;
                this._fItemLocked = sourceElement._fItemLocked;
                this._lockedAttributesList = sourceElement._lockedAttributesList;
                this._lockedElementsList = sourceElement._lockedElementsList;
                this.AssociateContext(sourceElement._configRecord);
                if (parentElement != null)
                {
                    if (parentElement._lockedAttributesList != null)
                    {
                        this._lockedAttributesList = this.UnMergeLockList(sourceElement._lockedAttributesList, parentElement._lockedAttributesList, saveMode);
                    }
                    if (parentElement._lockedElementsList != null)
                    {
                        this._lockedElementsList = this.UnMergeLockList(sourceElement._lockedElementsList, parentElement._lockedElementsList, saveMode);
                    }
                    if (parentElement._lockedAllExceptAttributesList != null)
                    {
                        this._lockedAllExceptAttributesList = this.UnMergeLockList(sourceElement._lockedAllExceptAttributesList, parentElement._lockedAllExceptAttributesList, saveMode);
                    }
                    if (parentElement._lockedAllExceptElementsList != null)
                    {
                        this._lockedAllExceptElementsList = this.UnMergeLockList(sourceElement._lockedAllExceptElementsList, parentElement._lockedAllExceptElementsList, saveMode);
                    }
                }
                ConfigurationPropertyCollection properties = this.Properties;
                ConfigurationPropertyCollection propertys2 = null;
                for (int i = 0; i < sourceElement.Values.Count; i++)
                {
                    string key = sourceElement.Values.GetKey(i);
                    object obj2 = sourceElement.Values[i];
                    ConfigurationProperty property = sourceElement.Properties[key];
                    if ((property != null) && ((propertys2 == null) || propertys2.Contains(property.Name)))
                    {
                        if (property.IsConfigurationElementType)
                        {
                            flag = true;
                        }
                        else if ((obj2 != s_nullPropertyValue) && !properties.Contains(key))
                        {
                            ConfigurationValueFlags valueFlags = sourceElement.Values.RetrieveFlags(key);
                            this._values.SetValue(key, obj2, valueFlags, null);
                            properties.Add(property);
                        }
                    }
                }
                foreach (ConfigurationProperty property2 in this.Properties)
                {
                    object defaultValue;
                    if ((property2 != null) && ((propertys2 == null) || propertys2.Contains(property2.Name)))
                    {
                        if (property2.IsConfigurationElementType)
                        {
                            flag = true;
                        }
                        else
                        {
                            object objA = sourceElement.Values[property2.Name];
                            if (((property2.IsRequired || (saveMode == ConfigurationSaveMode.Full)) && ((objA == null) || (objA == s_nullPropertyValue))) && (property2.DefaultValue != null))
                            {
                                objA = property2.DefaultValue;
                            }
                            if ((objA != null) && (objA != s_nullPropertyValue))
                            {
                                defaultValue = null;
                                if (parentElement != null)
                                {
                                    defaultValue = parentElement.Values[property2.Name];
                                }
                                if (defaultValue == null)
                                {
                                    defaultValue = property2.DefaultValue;
                                }
                                switch (saveMode)
                                {
                                    case ConfigurationSaveMode.Modified:
                                    {
                                        bool flag2 = sourceElement.Values.IsModified(property2.Name);
                                        bool flag3 = sourceElement.Values.IsInherited(property2.Name);
                                        if (((property2.IsRequired || flag2) || !flag3) || (((parentElement == null) && flag3) && !object.Equals(objA, defaultValue)))
                                        {
                                            this._values[property2.Name] = objA;
                                        }
                                        break;
                                    }
                                    case ConfigurationSaveMode.Minimal:
                                        if (!object.Equals(objA, defaultValue) || property2.IsRequired)
                                        {
                                            this._values[property2.Name] = objA;
                                        }
                                        break;

                                    case ConfigurationSaveMode.Full:
                                        if ((objA == null) || (objA == s_nullPropertyValue))
                                        {
                                            goto Label_031E;
                                        }
                                        this._values[property2.Name] = objA;
                                        break;
                                }
                            }
                        }
                    }
                    continue;
                Label_031E:
                    this._values[property2.Name] = defaultValue;
                }
                if (flag)
                {
                    foreach (ConfigurationProperty property3 in this.Properties)
                    {
                        if (property3.IsConfigurationElementType)
                        {
                            ConfigurationElement element = (parentElement != null) ? ((ConfigurationElement) parentElement[property3]) : null;
                            ConfigurationElement element2 = (ConfigurationElement) this[property3];
                            if (((ConfigurationElement) sourceElement[property3]) != null)
                            {
                                element2.Unmerge((ConfigurationElement) sourceElement[property3], element, saveMode);
                            }
                        }
                    }
                }
            }
        }

        internal ConfigurationLockCollection UnMergeLockList(ConfigurationLockCollection sourceLockList, ConfigurationLockCollection parentLockList, ConfigurationSaveMode saveMode)
        {
            if (!sourceLockList.ExceptionList)
            {
                switch (saveMode)
                {
                    case ConfigurationSaveMode.Modified:
                    {
                        ConfigurationLockCollection locks = new ConfigurationLockCollection(this, sourceLockList.LockType);
                        foreach (string str in sourceLockList)
                        {
                            if (!parentLockList.Contains(str) || sourceLockList.IsValueModified(str))
                            {
                                locks.Add(str, ConfigurationValueFlags.Default);
                            }
                        }
                        return locks;
                    }
                    case ConfigurationSaveMode.Minimal:
                    {
                        ConfigurationLockCollection locks2 = new ConfigurationLockCollection(this, sourceLockList.LockType);
                        foreach (string str2 in sourceLockList)
                        {
                            if (!parentLockList.Contains(str2))
                            {
                                locks2.Add(str2, ConfigurationValueFlags.Default);
                            }
                        }
                        return locks2;
                    }
                }
                return sourceLockList;
            }
            if ((saveMode == ConfigurationSaveMode.Modified) || (saveMode == ConfigurationSaveMode.Minimal))
            {
                bool flag = false;
                if (sourceLockList.Count == parentLockList.Count)
                {
                    flag = true;
                    foreach (string str3 in sourceLockList)
                    {
                        if (!parentLockList.Contains(str3) || (sourceLockList.IsValueModified(str3) && (saveMode == ConfigurationSaveMode.Modified)))
                        {
                            flag = false;
                        }
                    }
                }
                if (flag)
                {
                    return null;
                }
            }
            return sourceLockList;
        }

        internal static void ValidateElement(ConfigurationElement elem, ConfigurationValidatorBase propValidator, bool recursive)
        {
            ConfigurationValidatorBase validator = propValidator;
            if ((validator == null) && (elem.ElementProperty != null))
            {
                validator = elem.ElementProperty.Validator;
                if ((validator != null) && !validator.CanValidate(elem.GetType()))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Validator_does_not_support_elem_type", new object[] { elem.GetType().Name }));
                }
            }
            try
            {
                if (validator != null)
                {
                    validator.Validate(elem);
                }
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Validator_element_not_valid", new object[] { elem._elementTagName, exception.Message }));
            }
            if (recursive)
            {
                if ((elem is ConfigurationElementCollection) && (elem is ConfigurationElementCollection))
                {
                    IEnumerator elementsEnumerator = ((ConfigurationElementCollection) elem).GetElementsEnumerator();
                    while (elementsEnumerator.MoveNext())
                    {
                        ValidateElement((ConfigurationElement) elementsEnumerator.Current, null, true);
                    }
                }
                for (int i = 0; i < elem.Values.Count; i++)
                {
                    ConfigurationElement element = elem.Values[i] as ConfigurationElement;
                    if (element != null)
                    {
                        ValidateElement(element, null, true);
                    }
                }
            }
        }

        public System.Configuration.Configuration CurrentConfiguration
        {
            get
            {
                if (this._configRecord != null)
                {
                    return this._configRecord.CurrentConfiguration;
                }
                return null;
            }
        }

        internal bool DataToWriteInternal
        {
            get
            {
                return this._bDataToWrite;
            }
            set
            {
                this._bDataToWrite = value;
            }
        }

        public System.Configuration.ElementInformation ElementInformation
        {
            get
            {
                if (this._evaluationElement == null)
                {
                    this._evaluationElement = new System.Configuration.ElementInformation(this);
                }
                return this._evaluationElement;
            }
        }

        internal bool ElementPresent
        {
            get
            {
                return this._bElementPresent;
            }
            set
            {
                this._bElementPresent = value;
            }
        }

        protected internal virtual ConfigurationElementProperty ElementProperty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._elementProperty;
            }
        }

        internal string ElementTagName
        {
            get
            {
                return this._elementTagName;
            }
        }

        protected ContextInformation EvaluationContext
        {
            get
            {
                if (this._evalContext == null)
                {
                    if (this._configRecord == null)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_element_no_context"));
                    }
                    this._evalContext = new ContextInformation(this._configRecord);
                }
                return this._evalContext;
            }
        }

        protected internal object this[ConfigurationProperty prop]
        {
            get
            {
                object defaultValue = this._values[prop.Name];
                if (defaultValue == null)
                {
                    if (prop.IsConfigurationElementType)
                    {
                        lock (this._values.SyncRoot)
                        {
                            defaultValue = this._values[prop.Name];
                            if (defaultValue == null)
                            {
                                ConfigurationElement element = CreateElement(prop.Type);
                                if (this._bReadOnly)
                                {
                                    element.SetReadOnly();
                                }
                                if (typeof(ConfigurationElementCollection).IsAssignableFrom(prop.Type))
                                {
                                    ConfigurationElementCollection elements = element as ConfigurationElementCollection;
                                    if (prop.AddElementName != null)
                                    {
                                        elements.AddElementName = prop.AddElementName;
                                    }
                                    if (prop.RemoveElementName != null)
                                    {
                                        elements.RemoveElementName = prop.RemoveElementName;
                                    }
                                    if (prop.ClearElementName != null)
                                    {
                                        elements.ClearElementName = prop.ClearElementName;
                                    }
                                }
                                this._values.SetValue(prop.Name, element, ConfigurationValueFlags.Inherited, null);
                                defaultValue = element;
                            }
                            goto Label_00FC;
                        }
                    }
                    defaultValue = prop.DefaultValue;
                }
                else if (defaultValue == s_nullPropertyValue)
                {
                    defaultValue = null;
                }
            Label_00FC:
                if (defaultValue is InvalidPropValue)
                {
                    throw ((InvalidPropValue) defaultValue).Error;
                }
                return defaultValue;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.SetPropertyValue(prop, value, false);
            }
        }

        protected internal object this[string propertyName]
        {
            get
            {
                ConfigurationProperty property = this.Properties[propertyName];
                if (property == null)
                {
                    property = this.Properties[""];
                    if (property.ProvidedName != propertyName)
                    {
                        return null;
                    }
                }
                return this[property];
            }
            set
            {
                this.SetPropertyValue(this.Properties[propertyName], value, false);
            }
        }

        internal ConfigurationValueFlags ItemLocked
        {
            get
            {
                return this._fItemLocked;
            }
        }

        public ConfigurationLockCollection LockAllAttributesExcept
        {
            get
            {
                if (this._lockedAllExceptAttributesList == null)
                {
                    this._lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, this._elementTagName);
                }
                return this._lockedAllExceptAttributesList;
            }
        }

        public ConfigurationLockCollection LockAllElementsExcept
        {
            get
            {
                if (this._lockedAllExceptElementsList == null)
                {
                    this._lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, this._elementTagName);
                }
                return this._lockedAllExceptElementsList;
            }
        }

        public ConfigurationLockCollection LockAttributes
        {
            get
            {
                if (this._lockedAttributesList == null)
                {
                    this._lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                }
                return this._lockedAttributesList;
            }
        }

        internal ConfigurationLockCollection LockedAllExceptAttributesList
        {
            get
            {
                return this._lockedAllExceptAttributesList;
            }
        }

        internal ConfigurationLockCollection LockedAttributesList
        {
            get
            {
                return this._lockedAttributesList;
            }
        }

        public ConfigurationLockCollection LockElements
        {
            get
            {
                if (this._lockedElementsList == null)
                {
                    this._lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                return this._lockedElementsList;
            }
        }

        public bool LockItem
        {
            get
            {
                return ((this._fItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default);
            }
            set
            {
                if ((this._fItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { "lockItem" }));
                }
                this._fItemLocked = value ? ConfigurationValueFlags.Locked : ConfigurationValueFlags.Default;
                this._fItemLocked |= ConfigurationValueFlags.Modified;
            }
        }

        protected internal virtual ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection result = null;
                if (PropertiesFromType(base.GetType(), out result))
                {
                    ApplyInstanceAttributes(this);
                    ApplyValidatorsRecursive(this);
                }
                return result;
            }
        }

        internal ConfigurationValues Values
        {
            get
            {
                return this._values;
            }
        }
    }
}


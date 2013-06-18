namespace System.Configuration
{
    using System;
    using System.Collections;

    public sealed class ElementInformation
    {
        private ConfigurationException[] _errors;
        private PropertyInformationCollection _internalProperties;
        private ConfigurationElement _thisElement;

        internal ElementInformation(ConfigurationElement thisElement)
        {
            this._thisElement = thisElement;
        }

        internal void ChangeSourceAndLineNumber(PropertySourceInfo sourceInformation)
        {
            this._thisElement.Values.ChangeSourceInfo(this._thisElement.ElementTagName, sourceInformation);
        }

        private ConfigurationException[] GetReadOnlyErrorsList()
        {
            ArrayList errorsList = this._thisElement.GetErrorsList();
            int count = errorsList.Count;
            ConfigurationException[] array = new ConfigurationException[errorsList.Count];
            if (count != 0)
            {
                errorsList.CopyTo(array, 0);
            }
            return array;
        }

        internal PropertySourceInfo PropertyInfoInternal()
        {
            return this._thisElement.PropertyInfoInternal(this._thisElement.ElementTagName);
        }

        public ICollection Errors
        {
            get
            {
                if (this._errors == null)
                {
                    this._errors = this.GetReadOnlyErrorsList();
                }
                return this._errors;
            }
        }

        public bool IsCollection
        {
            get
            {
                ConfigurationElementCollection elements = this._thisElement as ConfigurationElementCollection;
                if ((elements == null) && (this._thisElement.Properties.DefaultCollectionProperty != null))
                {
                    elements = this._thisElement[this._thisElement.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                }
                return (elements != null);
            }
        }

        public bool IsLocked
        {
            get
            {
                return (((this._thisElement.ItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) && ((this._thisElement.ItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default));
            }
        }

        public bool IsPresent
        {
            get
            {
                return this._thisElement.ElementPresent;
            }
        }

        public int LineNumber
        {
            get
            {
                PropertySourceInfo sourceInfo = this._thisElement.Values.GetSourceInfo(this._thisElement.ElementTagName);
                if (sourceInfo == null)
                {
                    return 0;
                }
                return sourceInfo.LineNumber;
            }
        }

        public PropertyInformationCollection Properties
        {
            get
            {
                if (this._internalProperties == null)
                {
                    this._internalProperties = new PropertyInformationCollection(this._thisElement);
                }
                return this._internalProperties;
            }
        }

        public string Source
        {
            get
            {
                PropertySourceInfo sourceInfo = this._thisElement.Values.GetSourceInfo(this._thisElement.ElementTagName);
                if (sourceInfo == null)
                {
                    return null;
                }
                return sourceInfo.FileName;
            }
        }

        public System.Type Type
        {
            get
            {
                return this._thisElement.GetType();
            }
        }

        public ConfigurationValidatorBase Validator
        {
            get
            {
                return this._thisElement.ElementProperty.Validator;
            }
        }
    }
}


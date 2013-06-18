namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    public sealed class PropertyInformation
    {
        private ConfigurationProperty _Prop;
        private const string LockAll = "*";
        private string PropertyName;
        private ConfigurationElement ThisElement;

        internal PropertyInformation(ConfigurationElement thisElement, string propertyName)
        {
            this.PropertyName = propertyName;
            this.ThisElement = thisElement;
        }

        public TypeConverter Converter
        {
            get
            {
                return this.Prop.Converter;
            }
        }

        public object DefaultValue
        {
            get
            {
                return this.Prop.DefaultValue;
            }
        }

        public string Description
        {
            get
            {
                return this.Prop.Description;
            }
        }

        public bool IsKey
        {
            get
            {
                return this.Prop.IsKey;
            }
        }

        public bool IsLocked
        {
            get
            {
                if (((this.ThisElement.LockedAllExceptAttributesList == null) || this.ThisElement.LockedAllExceptAttributesList.DefinedInParent(this.PropertyName)) && ((this.ThisElement.LockedAttributesList == null) || (!this.ThisElement.LockedAttributesList.DefinedInParent(this.PropertyName) && !this.ThisElement.LockedAttributesList.DefinedInParent("*"))))
                {
                    return (((this.ThisElement.ItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) && ((this.ThisElement.ItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default));
                }
                return true;
            }
        }

        public bool IsModified
        {
            get
            {
                if (this.ThisElement.Values[this.PropertyName] == null)
                {
                    return false;
                }
                return this.ThisElement.Values.IsModified(this.PropertyName);
            }
        }

        public bool IsRequired
        {
            get
            {
                return this.Prop.IsRequired;
            }
        }

        public int LineNumber
        {
            get
            {
                PropertySourceInfo sourceInfo = this.ThisElement.Values.GetSourceInfo(this.PropertyName);
                if (sourceInfo == null)
                {
                    sourceInfo = this.ThisElement.Values.GetSourceInfo(string.Empty);
                }
                if (sourceInfo == null)
                {
                    return 0;
                }
                return sourceInfo.LineNumber;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.PropertyName;
            }
        }

        private ConfigurationProperty Prop
        {
            get
            {
                if (this._Prop == null)
                {
                    this._Prop = this.ThisElement.Properties[this.PropertyName];
                }
                return this._Prop;
            }
        }

        internal string ProvidedName
        {
            get
            {
                return this.Prop.ProvidedName;
            }
        }

        public string Source
        {
            get
            {
                PropertySourceInfo sourceInfo = this.ThisElement.Values.GetSourceInfo(this.PropertyName);
                if (sourceInfo == null)
                {
                    sourceInfo = this.ThisElement.Values.GetSourceInfo(string.Empty);
                }
                if (sourceInfo == null)
                {
                    return string.Empty;
                }
                return sourceInfo.FileName;
            }
        }

        public System.Type Type
        {
            get
            {
                return this.Prop.Type;
            }
        }

        public ConfigurationValidatorBase Validator
        {
            get
            {
                return this.Prop.Validator;
            }
        }

        public object Value
        {
            get
            {
                return this.ThisElement[this.PropertyName];
            }
            set
            {
                this.ThisElement[this.PropertyName] = value;
            }
        }

        public PropertyValueOrigin ValueOrigin
        {
            get
            {
                if (this.ThisElement.Values[this.PropertyName] == null)
                {
                    return PropertyValueOrigin.Default;
                }
                if (this.ThisElement.Values.IsInherited(this.PropertyName))
                {
                    return PropertyValueOrigin.Inherited;
                }
                return PropertyValueOrigin.SetHere;
            }
        }
    }
}


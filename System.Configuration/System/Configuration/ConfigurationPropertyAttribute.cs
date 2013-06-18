namespace System.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigurationPropertyAttribute : Attribute
    {
        private object _DefaultValue = ConfigurationElement.s_nullPropertyValue;
        private ConfigurationPropertyOptions _Flags;
        private string _Name;
        internal static readonly string DefaultCollectionPropertyName = "";

        public ConfigurationPropertyAttribute(string name)
        {
            this._Name = name;
        }

        public object DefaultValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._DefaultValue;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._DefaultValue = value;
            }
        }

        public bool IsDefaultCollection
        {
            get
            {
                return ((this.Options & ConfigurationPropertyOptions.IsDefaultCollection) != ConfigurationPropertyOptions.None);
            }
            set
            {
                if (value)
                {
                    this.Options |= ConfigurationPropertyOptions.IsDefaultCollection;
                }
                else
                {
                    this.Options &= ~ConfigurationPropertyOptions.IsDefaultCollection;
                }
            }
        }

        public bool IsKey
        {
            get
            {
                return ((this.Options & ConfigurationPropertyOptions.IsKey) != ConfigurationPropertyOptions.None);
            }
            set
            {
                if (value)
                {
                    this.Options |= ConfigurationPropertyOptions.IsKey;
                }
                else
                {
                    this.Options &= ~ConfigurationPropertyOptions.IsKey;
                }
            }
        }

        public bool IsRequired
        {
            get
            {
                return ((this.Options & ConfigurationPropertyOptions.IsRequired) != ConfigurationPropertyOptions.None);
            }
            set
            {
                if (value)
                {
                    this.Options |= ConfigurationPropertyOptions.IsRequired;
                }
                else
                {
                    this.Options &= ~ConfigurationPropertyOptions.IsRequired;
                }
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._Name;
            }
        }

        public ConfigurationPropertyOptions Options
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._Flags;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._Flags = value;
            }
        }
    }
}


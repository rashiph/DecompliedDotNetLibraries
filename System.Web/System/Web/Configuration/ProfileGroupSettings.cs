namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Xml;

    public sealed class ProfileGroupSettings : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, ProfilePropertyNameValidator.SingletonInstance, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propProperties = new ConfigurationProperty(null, typeof(ProfilePropertySettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static ProfileGroupSettings()
        {
            _properties.Add(_propName);
            _properties.Add(_propProperties);
        }

        internal ProfileGroupSettings()
        {
        }

        public ProfileGroupSettings(string name)
        {
            base[_propName] = name;
        }

        public override bool Equals(object obj)
        {
            ProfileGroupSettings settings = obj as ProfileGroupSettings;
            return (((settings != null) && (this.Name == settings.Name)) && object.Equals(this.PropertySettings, settings.PropertySettings));
        }

        public override int GetHashCode()
        {
            return (this.Name.GetHashCode() ^ this.PropertySettings.GetHashCode());
        }

        internal void InternalDeserialize(XmlReader reader, bool serializeCollectionKey)
        {
            this.DeserializeElement(reader, serializeCollectionKey);
        }

        internal void InternalReset(ProfileGroupSettings parentSettings)
        {
            base.Reset(parentSettings);
        }

        internal void InternalUnmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public ProfilePropertySettingsCollection PropertySettings
        {
            get
            {
                return (ProfilePropertySettingsCollection) base[_propProperties];
            }
        }
    }
}


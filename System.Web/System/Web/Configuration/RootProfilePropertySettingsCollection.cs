namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Util;
    using System.Xml;

    [ConfigurationCollection(typeof(ProfilePropertySettings))]
    public sealed class RootProfilePropertySettingsCollection : ProfilePropertySettingsCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private ProfileGroupSettingsCollection _propGroups = new ProfileGroupSettingsCollection();

        public override bool Equals(object rootProfilePropertySettingsCollection)
        {
            RootProfilePropertySettingsCollection objB = rootProfilePropertySettingsCollection as RootProfilePropertySettingsCollection;
            return (((objB != null) && object.Equals(this, objB)) && object.Equals(this.GroupSettings, objB.GroupSettings));
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(base.GetHashCode(), this.GroupSettings.GetHashCode());
        }

        protected override bool IsModified()
        {
            if (!base.IsModified())
            {
                return this.GroupSettings.InternalIsModified();
            }
            return true;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            if (elementName == "group")
            {
                ProfileGroupSettings parentSettings = null;
                string attribute = reader.GetAttribute("name");
                ProfileGroupSettingsCollection groupSettings = this.GroupSettings;
                if (attribute != null)
                {
                    parentSettings = groupSettings[attribute];
                }
                ProfileGroupSettings settings = new ProfileGroupSettings();
                settings.InternalReset(parentSettings);
                settings.InternalDeserialize(reader, false);
                groupSettings.AddOrReplace(settings);
                return true;
            }
            if (elementName == "clear")
            {
                this.GroupSettings.Clear();
            }
            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            RootProfilePropertySettingsCollection settingss = parentElement as RootProfilePropertySettingsCollection;
            base.Reset(parentElement);
            this.GroupSettings.InternalReset(settingss.GroupSettings);
        }

        protected override void ResetModified()
        {
            base.ResetModified();
            this.GroupSettings.InternalResetModified();
        }

        protected override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            bool flag = false;
            if (!base.SerializeElement(null, false) && !this.GroupSettings.InternalSerialize(null, false))
            {
                return flag;
            }
            flag |= base.SerializeElement(writer, false);
            return (flag | this.GroupSettings.InternalSerialize(writer, false));
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            RootProfilePropertySettingsCollection settingss = parentElement as RootProfilePropertySettingsCollection;
            RootProfilePropertySettingsCollection settingss2 = sourceElement as RootProfilePropertySettingsCollection;
            base.Unmerge(sourceElement, parentElement, saveMode);
            this.GroupSettings.InternalUnMerge(settingss2.GroupSettings, (settingss != null) ? settingss.GroupSettings : null, saveMode);
        }

        protected override bool AllowClear
        {
            get
            {
                return true;
            }
        }

        [ConfigurationProperty("group")]
        public ProfileGroupSettingsCollection GroupSettings
        {
            get
            {
                return this._propGroups;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }
    }
}


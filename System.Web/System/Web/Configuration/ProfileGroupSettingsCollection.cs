namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;
    using System.Xml;

    [ConfigurationCollection(typeof(ProfileGroupSettings), AddItemName="group")]
    public sealed class ProfileGroupSettingsCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private bool bModified;

        public ProfileGroupSettingsCollection()
        {
            base.AddElementName = "group";
            base.ClearElementName = string.Empty;
            base.EmitClear = false;
        }

        public void Add(ProfileGroupSettings group)
        {
            this.BaseAdd(group);
        }

        internal void AddOrReplace(ProfileGroupSettings groupSettings)
        {
            base.BaseAdd(groupSettings, false);
        }

        public void Clear()
        {
            int num = base.Count - 1;
            this.bModified = true;
            for (int i = num; i >= 0; i--)
            {
                ConfigurationElement element = base.BaseGet(i);
                if ((element != null) && element.ElementInformation.IsPresent)
                {
                    base.BaseRemoveAt(i);
                }
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProfileGroupSettings();
        }

        public ProfileGroupSettings Get(int index)
        {
            return (ProfileGroupSettings) base.BaseGet(index);
        }

        public ProfileGroupSettings Get(string name)
        {
            return (ProfileGroupSettings) base.BaseGet(name);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProfileGroupSettings) element).Name;
        }

        public string GetKey(int index)
        {
            return (string) base.BaseGetKey(index);
        }

        public int IndexOf(ProfileGroupSettings group)
        {
            return base.BaseIndexOf(group);
        }

        internal bool InternalIsModified()
        {
            return this.IsModified();
        }

        internal void InternalReset(ConfigurationElement parentElement)
        {
            this.Reset(parentElement);
        }

        internal void InternalResetModified()
        {
            this.ResetModified();
        }

        internal bool InternalSerialize(XmlWriter writer, bool serializeCollectionKey)
        {
            if (base.EmitClear)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Clear_not_valid"));
            }
            return this.SerializeElement(writer, serializeCollectionKey);
        }

        internal void InternalUnMerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            this.Unmerge(sourceElement, parentElement, saveMode);
            base.BaseClear();
            ProfileGroupSettingsCollection settingss = sourceElement as ProfileGroupSettingsCollection;
            ProfileGroupSettingsCollection settingss2 = parentElement as ProfileGroupSettingsCollection;
            foreach (ProfileGroupSettings settings in settingss)
            {
                ProfileGroupSettings settings2 = settingss2.Get(settings.Name);
                ProfileGroupSettings element = new ProfileGroupSettings();
                element.InternalUnmerge(settings, settings2, saveMode);
                this.BaseAdd(element);
            }
        }

        protected override bool IsModified()
        {
            return (this.bModified || base.IsModified());
        }

        public void Remove(string name)
        {
            ConfigurationElement element = base.BaseGet(name);
            if (element != null)
            {
                if (!element.ElementInformation.IsPresent)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_cannot_remove_inherited_items"));
                }
                base.BaseRemove(name);
            }
        }

        public void RemoveAt(int index)
        {
            ConfigurationElement element = base.BaseGet(index);
            if (element != null)
            {
                if (!element.ElementInformation.IsPresent)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_cannot_remove_inherited_items"));
                }
                base.BaseRemoveAt(index);
            }
        }

        protected override void ResetModified()
        {
            this.bModified = false;
            base.ResetModified();
        }

        public void Set(ProfileGroupSettings group)
        {
            base.BaseAdd(group, false);
        }

        public string[] AllKeys
        {
            get
            {
                return System.Web.Util.StringUtil.ObjectArrayToStringArray(base.BaseGetAllKeys());
            }
        }

        public ProfileGroupSettings this[string name]
        {
            get
            {
                return (ProfileGroupSettings) base.BaseGet(name);
            }
        }

        public ProfileGroupSettings this[int index]
        {
            get
            {
                return (ProfileGroupSettings) base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}


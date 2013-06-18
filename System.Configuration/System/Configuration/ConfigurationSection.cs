namespace System.Configuration
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Versioning;
    using System.Xml;

    public abstract class ConfigurationSection : ConfigurationElement
    {
        private System.Configuration.SectionInformation _section;

        protected ConfigurationSection()
        {
            this._section = new System.Configuration.SectionInformation(this);
        }

        protected internal virtual void DeserializeSection(XmlReader reader)
        {
            if (!reader.Read() || (reader.NodeType != XmlNodeType.Element))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_expected_to_find_element"), reader);
            }
            this.DeserializeElement(reader, false);
        }

        protected internal virtual object GetRuntimeObject()
        {
            return this;
        }

        protected internal override bool IsModified()
        {
            if (!this.SectionInformation.IsModifiedFlags())
            {
                return base.IsModified();
            }
            return true;
        }

        protected internal override void ResetModified()
        {
            this.SectionInformation.ResetModifiedFlags();
            base.ResetModified();
        }

        protected internal virtual string SerializeSection(ConfigurationElement parentElement, string name, ConfigurationSaveMode saveMode)
        {
            if (((base.CurrentConfiguration != null) && (base.CurrentConfiguration.TargetFramework != null)) && !this.ShouldSerializeSectionInTargetVersion(base.CurrentConfiguration.TargetFramework))
            {
                return string.Empty;
            }
            ConfigurationElement.ValidateElement(this, null, true);
            ConfigurationElement element = ConfigurationElement.CreateElement(base.GetType());
            element.Unmerge(this, parentElement, saveMode);
            StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter writer = new XmlTextWriter(w) {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            element.DataToWriteInternal = saveMode != ConfigurationSaveMode.Minimal;
            if ((base.CurrentConfiguration != null) && (base.CurrentConfiguration.TargetFramework != null))
            {
                base._configRecord.SectionsStack.Push(this);
            }
            element.SerializeToXmlElement(writer, name);
            if ((base.CurrentConfiguration != null) && (base.CurrentConfiguration.TargetFramework != null))
            {
                base._configRecord.SectionsStack.Pop();
            }
            writer.Flush();
            return w.ToString();
        }

        protected internal virtual bool ShouldSerializeElementInTargetVersion(ConfigurationElement element, string elementName, FrameworkName targetFramework)
        {
            return true;
        }

        protected internal virtual bool ShouldSerializePropertyInTargetVersion(ConfigurationProperty property, string propertyName, FrameworkName targetFramework, ConfigurationElement parentConfigurationElement)
        {
            return true;
        }

        protected internal virtual bool ShouldSerializeSectionInTargetVersion(FrameworkName targetFramework)
        {
            return true;
        }

        public System.Configuration.SectionInformation SectionInformation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._section;
            }
        }
    }
}


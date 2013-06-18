namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class ConfigurationSectionGroupCollection : NameObjectCollectionBase
    {
        private MgmtConfigurationRecord _configRecord;
        private ConfigurationSectionGroup _configSectionGroup;

        internal ConfigurationSectionGroupCollection(MgmtConfigurationRecord configRecord, ConfigurationSectionGroup configSectionGroup) : base(StringComparer.Ordinal)
        {
            this._configRecord = configRecord;
            this._configSectionGroup = configSectionGroup;
            foreach (DictionaryEntry entry in this._configRecord.SectionGroupFactories)
            {
                FactoryId id = (FactoryId) entry.Value;
                if (id.Group == this._configSectionGroup.SectionGroupName)
                {
                    base.BaseAdd(id.Name, id.Name);
                }
            }
        }

        public void Add(string name, ConfigurationSectionGroup sectionGroup)
        {
            this.VerifyIsAttachedToConfigRecord();
            this._configRecord.AddConfigurationSectionGroup(this._configSectionGroup.SectionGroupName, name, sectionGroup);
            base.BaseAdd(name, name);
        }

        public void Clear()
        {
            this.VerifyIsAttachedToConfigRecord();
            if (this._configSectionGroup.IsRoot)
            {
                this._configRecord.RemoveLocationWriteRequirement();
            }
            foreach (string str in base.BaseGetAllKeys())
            {
                this.Remove(str);
            }
        }

        public void CopyTo(ConfigurationSectionGroup[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int count = this.Count;
            if (array.Length < (count + index))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num2 = 0;
            for (int i = index; num2 < count; i++)
            {
                array[i] = this.Get(num2);
                num2++;
            }
        }

        internal void DetachFromConfigurationRecord()
        {
            this._configRecord = null;
            base.BaseClear();
        }

        public ConfigurationSectionGroup Get(int index)
        {
            return this.Get(this.GetKey(index));
        }

        public ConfigurationSectionGroup Get(string name)
        {
            this.VerifyIsAttachedToConfigRecord();
            if (string.IsNullOrEmpty(name))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("name");
            }
            if (name.IndexOf('/') >= 0)
            {
                return null;
            }
            string configKey = BaseConfigurationRecord.CombineConfigKey(this._configSectionGroup.SectionGroupName, name);
            return this._configRecord.GetSectionGroup(configKey);
        }

        public override IEnumerator GetEnumerator()
        {
            int count = this.Count;
            int iteratorVariable1 = 0;
            while (true)
            {
                if (iteratorVariable1 >= count)
                {
                    yield break;
                }
                yield return this[iteratorVariable1];
                iteratorVariable1++;
            }
        }

        public string GetKey(int index)
        {
            return base.BaseGetKey(index);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public void Remove(string name)
        {
            this.VerifyIsAttachedToConfigRecord();
            this._configRecord.RemoveConfigurationSectionGroup(this._configSectionGroup.SectionGroupName, name);
            string key = BaseConfigurationRecord.CombineConfigKey(this._configSectionGroup.SectionGroupName, name);
            if (!this._configRecord.SectionFactories.Contains(key))
            {
                base.BaseRemove(name);
            }
        }

        public void RemoveAt(int index)
        {
            this.VerifyIsAttachedToConfigRecord();
            this.Remove(this.GetKey(index));
        }

        private void VerifyIsAttachedToConfigRecord()
        {
            if (this._configRecord == null)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_cannot_edit_configurationsectiongroup_when_not_attached"));
            }
        }

        public override int Count
        {
            get
            {
                return base.Count;
            }
        }

        public ConfigurationSectionGroup this[string name]
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.Get(name);
            }
        }

        public ConfigurationSectionGroup this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.Get(index);
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return base.Keys;
            }
        }

    }
}


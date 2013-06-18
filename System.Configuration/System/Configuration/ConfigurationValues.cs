namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class ConfigurationValues : NameObjectCollectionBase
    {
        private BaseConfigurationRecord _configRecord;
        private bool _containsElement;
        private bool _containsInvalidValue;
        private static IEnumerable s_emptyCollection;

        internal ConfigurationValues() : base(StringComparer.Ordinal)
        {
        }

        internal void AssociateContext(BaseConfigurationRecord configRecord)
        {
            this._configRecord = configRecord;
            foreach (ConfigurationElement element in this.ConfigurationElements)
            {
                element.AssociateContext(this._configRecord);
            }
        }

        internal void ChangeSourceInfo(string key, PropertySourceInfo sourceInfo)
        {
            ConfigurationValue configValue = this.GetConfigValue(key);
            if (configValue != null)
            {
                configValue.SourceInfo = sourceInfo;
            }
        }

        internal void Clear()
        {
            base.BaseClear();
        }

        internal bool Contains(string key)
        {
            return (base.BaseGet(key) != null);
        }

        private ConfigurationValue CreateConfigValue(object value, ConfigurationValueFlags valueFlags, PropertySourceInfo sourceInfo)
        {
            if (value != null)
            {
                if (value is ConfigurationElement)
                {
                    this._containsElement = true;
                    ((ConfigurationElement) value).AssociateContext(this._configRecord);
                }
                else if (value is InvalidPropValue)
                {
                    this._containsInvalidValue = true;
                }
            }
            return new ConfigurationValue(value, valueFlags, sourceInfo);
        }

        internal ConfigurationValue GetConfigValue(int index)
        {
            return (ConfigurationValue) base.BaseGet(index);
        }

        internal ConfigurationValue GetConfigValue(string key)
        {
            return (ConfigurationValue) base.BaseGet(key);
        }

        internal string GetKey(int index)
        {
            return base.BaseGetKey(index);
        }

        internal PropertySourceInfo GetSourceInfo(string key)
        {
            ConfigurationValue configValue = this.GetConfigValue(key);
            if (configValue != null)
            {
                return configValue.SourceInfo;
            }
            return null;
        }

        internal bool IsInherited(string key)
        {
            ConfigurationValue value2 = (ConfigurationValue) base.BaseGet(key);
            return ((value2 != null) && ((value2.ValueFlags & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default));
        }

        internal bool IsModified(string key)
        {
            ConfigurationValue value2 = (ConfigurationValue) base.BaseGet(key);
            return ((value2 != null) && ((value2.ValueFlags & ConfigurationValueFlags.Modified) != ConfigurationValueFlags.Default));
        }

        internal ConfigurationValueFlags RetrieveFlags(string key)
        {
            ConfigurationValue value2 = (ConfigurationValue) base.BaseGet(key);
            if (value2 != null)
            {
                return value2.ValueFlags;
            }
            return ConfigurationValueFlags.Default;
        }

        internal void SetValue(string key, object value, ConfigurationValueFlags valueFlags, PropertySourceInfo sourceInfo)
        {
            ConfigurationValue value2 = this.CreateConfigValue(value, valueFlags, sourceInfo);
            base.BaseSet(key, value2);
        }

        internal IEnumerable ConfigurationElements
        {
            get
            {
                if (this._containsElement)
                {
                    return new ConfigurationElementsCollection(this);
                }
                return EmptyCollectionInstance;
            }
        }

        private static IEnumerable EmptyCollectionInstance
        {
            get
            {
                if (s_emptyCollection == null)
                {
                    s_emptyCollection = new EmptyCollection();
                }
                return s_emptyCollection;
            }
        }

        internal IEnumerable InvalidValues
        {
            get
            {
                if (this._containsInvalidValue)
                {
                    return new InvalidValuesCollection(this);
                }
                return EmptyCollectionInstance;
            }
        }

        internal object this[string key]
        {
            get
            {
                ConfigurationValue configValue = this.GetConfigValue(key);
                if (configValue != null)
                {
                    return configValue.Value;
                }
                return null;
            }
            set
            {
                this.SetValue(key, value, ConfigurationValueFlags.Modified, null);
            }
        }

        internal object this[int index]
        {
            get
            {
                ConfigurationValue configValue = this.GetConfigValue(index);
                if (configValue != null)
                {
                    return configValue.Value;
                }
                return null;
            }
        }

        internal object SyncRoot
        {
            get
            {
                return this;
            }
        }

        private class ConfigurationElementsCollection : IEnumerable
        {
            private ConfigurationValues _values;

            internal ConfigurationElementsCollection(ConfigurationValues values)
            {
                this._values = values;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                if (this._values._containsElement)
                {
                    for (int i = 0; i < this._values.Count; i++)
                    {
                        object iteratorVariable1 = this._values[i];
                        if (iteratorVariable1 is ConfigurationElement)
                        {
                            yield return iteratorVariable1;
                        }
                    }
                }
            }

        }

        private class EmptyCollection : IEnumerable
        {
            private IEnumerator _emptyEnumerator = new EmptyCollectionEnumerator();

            internal EmptyCollection()
            {
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._emptyEnumerator;
            }

            private class EmptyCollectionEnumerator : IEnumerator
            {
                bool IEnumerator.MoveNext()
                {
                    return false;
                }

                void IEnumerator.Reset()
                {
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return null;
                    }
                }
            }
        }

        private class InvalidValuesCollection : IEnumerable
        {
            private ConfigurationValues _values;

            internal InvalidValuesCollection(ConfigurationValues values)
            {
                this._values = values;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                if (this._values._containsInvalidValue)
                {
                    for (int i = 0; i < this._values.Count; i++)
                    {
                        object iteratorVariable1 = this._values[i];
                        if (iteratorVariable1 is InvalidPropValue)
                        {
                            yield return iteratorVariable1;
                        }
                    }
                }
            }

        }
    }
}


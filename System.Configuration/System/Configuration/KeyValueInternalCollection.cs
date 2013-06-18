namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;

    internal class KeyValueInternalCollection : NameValueCollection
    {
        private AppSettingsSection _root;

        public KeyValueInternalCollection(AppSettingsSection root)
        {
            this._root = root;
            foreach (KeyValueConfigurationElement element in this._root.Settings)
            {
                base.Add(element.Key, element.Value);
            }
        }

        public override void Add(string key, string value)
        {
            this._root.Settings.Add(new KeyValueConfigurationElement(key, value));
            base.Add(key, value);
        }

        public override void Clear()
        {
            this._root.Settings.Clear();
            base.Clear();
        }

        public override void Remove(string key)
        {
            this._root.Settings.Remove(key);
            base.Remove(key);
        }
    }
}


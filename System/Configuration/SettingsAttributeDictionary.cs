namespace System.Configuration
{
    using System;
    using System.Collections;

    [Serializable]
    public class SettingsAttributeDictionary : Hashtable
    {
        public SettingsAttributeDictionary()
        {
        }

        public SettingsAttributeDictionary(SettingsAttributeDictionary attributes) : base(attributes)
        {
        }
    }
}


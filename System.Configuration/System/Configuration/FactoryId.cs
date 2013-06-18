namespace System.Configuration
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("FactoryId {ConfigKey}")]
    internal class FactoryId
    {
        private string _configKey;
        private string _group;
        private string _name;

        internal FactoryId(string configKey, string group, string name)
        {
            this._configKey = configKey;
            this._group = group;
            this._name = name;
        }

        internal string ConfigKey
        {
            get
            {
                return this._configKey;
            }
        }

        internal string Group
        {
            get
            {
                return this._group;
            }
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}


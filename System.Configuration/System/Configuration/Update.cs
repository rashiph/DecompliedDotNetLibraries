namespace System.Configuration
{
    using System;

    internal abstract class Update
    {
        private string _configKey;
        private bool _moved;
        private bool _retrieved;
        private string _updatedXml;

        internal Update(string configKey, bool moved, string updatedXml)
        {
            this._configKey = configKey;
            this._moved = moved;
            this._updatedXml = updatedXml;
        }

        internal string ConfigKey
        {
            get
            {
                return this._configKey;
            }
        }

        internal bool Moved
        {
            get
            {
                return this._moved;
            }
        }

        internal bool Retrieved
        {
            get
            {
                return this._retrieved;
            }
            set
            {
                this._retrieved = value;
            }
        }

        internal string UpdatedXml
        {
            get
            {
                return this._updatedXml;
            }
        }
    }
}


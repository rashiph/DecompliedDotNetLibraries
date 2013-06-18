namespace System.ComponentModel.Design.Data
{
    using System;

    public sealed class DesignerDataConnection
    {
        private string _connectionString;
        private bool _isConfigured;
        private string _name;
        private string _providerName;

        public DesignerDataConnection(string name, string providerName, string connectionString) : this(name, providerName, connectionString, false)
        {
        }

        public DesignerDataConnection(string name, string providerName, string connectionString, bool isConfigured)
        {
            this._name = name;
            this._providerName = providerName;
            this._connectionString = connectionString;
            this._isConfigured = isConfigured;
        }

        public string ConnectionString
        {
            get
            {
                return this._connectionString;
            }
        }

        public bool IsConfigured
        {
            get
            {
                return this._isConfigured;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string ProviderName
        {
            get
            {
                return this._providerName;
            }
        }
    }
}


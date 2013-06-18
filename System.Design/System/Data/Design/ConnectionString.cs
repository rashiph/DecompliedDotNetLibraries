namespace System.Data.Design
{
    using System;

    internal class ConnectionString
    {
        private string connectionString;
        private string providerName;

        public ConnectionString(string providerName, string connectionString)
        {
            this.connectionString = connectionString;
            this.providerName = providerName;
        }

        public string ToFullString()
        {
            return this.connectionString.ToString();
        }
    }
}


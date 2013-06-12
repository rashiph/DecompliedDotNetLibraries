namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MultiPartTableName
    {
        private string _multipartName;
        private string _serverName;
        private string _catalogName;
        private string _schemaName;
        private string _tableName;
        internal static readonly MultiPartTableName Null;
        internal MultiPartTableName(string[] parts)
        {
            this._multipartName = null;
            this._serverName = parts[0];
            this._catalogName = parts[1];
            this._schemaName = parts[2];
            this._tableName = parts[3];
        }

        internal MultiPartTableName(string multipartName)
        {
            this._multipartName = multipartName;
            this._serverName = null;
            this._catalogName = null;
            this._schemaName = null;
            this._tableName = null;
        }

        internal string ServerName
        {
            get
            {
                this.ParseMultipartName();
                return this._serverName;
            }
            set
            {
                this._serverName = value;
            }
        }
        internal string CatalogName
        {
            get
            {
                this.ParseMultipartName();
                return this._catalogName;
            }
            set
            {
                this._catalogName = value;
            }
        }
        internal string SchemaName
        {
            get
            {
                this.ParseMultipartName();
                return this._schemaName;
            }
            set
            {
                this._schemaName = value;
            }
        }
        internal string TableName
        {
            get
            {
                this.ParseMultipartName();
                return this._tableName;
            }
            set
            {
                this._tableName = value;
            }
        }
        private void ParseMultipartName()
        {
            if (this._multipartName != null)
            {
                string[] strArray = MultipartIdentifier.ParseMultipartIdentifier(this._multipartName, "[\"", "]\"", "SQL_TDSParserTableName", false);
                this._serverName = strArray[0];
                this._catalogName = strArray[1];
                this._schemaName = strArray[2];
                this._tableName = strArray[3];
                this._multipartName = null;
            }
        }

        static MultiPartTableName()
        {
            string[] parts = new string[4];
            Null = new MultiPartTableName(parts);
        }
    }
}


namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;

    internal class SmiStorageMetaData : SmiExtendedMetaData
    {
        private bool _allowsDBNull;
        private string _catalogName;
        private string _columnName;
        private bool _isColumnSet;
        private bool _isIdentity;
        private SqlBoolean _isKey;
        private string _schemaName;
        private string _serverName;
        private string _tableName;

        internal SmiStorageMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3, bool allowsDBNull, string serverName, string catalogName, string schemaName, string tableName, string columnName, SqlBoolean isKey, bool isIdentity) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, false, null, null, name, typeSpecificNamePart1, typeSpecificNamePart2, typeSpecificNamePart3, allowsDBNull, serverName, catalogName, schemaName, tableName, columnName, isKey, isIdentity)
        {
        }

        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped. Use ctor without columns param.")]
        internal SmiStorageMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, SmiMetaData[] columns, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3, bool allowsDBNull, string serverName, string catalogName, string schemaName, string tableName, string columnName, SqlBoolean isKey, bool isIdentity) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, name, typeSpecificNamePart1, typeSpecificNamePart2, typeSpecificNamePart3, allowsDBNull, serverName, catalogName, schemaName, tableName, columnName, isKey, isIdentity)
        {
        }

        internal SmiStorageMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, bool isMultiValued, IList<SmiExtendedMetaData> fieldMetaData, SmiMetaDataPropertyCollection extendedProperties, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3, bool allowsDBNull, string serverName, string catalogName, string schemaName, string tableName, string columnName, SqlBoolean isKey, bool isIdentity) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, null, isMultiValued, fieldMetaData, extendedProperties, name, typeSpecificNamePart1, typeSpecificNamePart2, typeSpecificNamePart3, allowsDBNull, serverName, catalogName, schemaName, tableName, columnName, isKey, isIdentity, false)
        {
        }

        internal SmiStorageMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, string udtAssemblyQualifiedName, bool isMultiValued, IList<SmiExtendedMetaData> fieldMetaData, SmiMetaDataPropertyCollection extendedProperties, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3, bool allowsDBNull, string serverName, string catalogName, string schemaName, string tableName, string columnName, SqlBoolean isKey, bool isIdentity, bool isColumnSet) : base(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, udtAssemblyQualifiedName, isMultiValued, fieldMetaData, extendedProperties, name, typeSpecificNamePart1, typeSpecificNamePart2, typeSpecificNamePart3)
        {
            this._allowsDBNull = allowsDBNull;
            this._serverName = serverName;
            this._catalogName = catalogName;
            this._schemaName = schemaName;
            this._tableName = tableName;
            this._columnName = columnName;
            this._isKey = isKey;
            this._isIdentity = isIdentity;
            this._isColumnSet = isColumnSet;
        }

        internal override string TraceString(int indent)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}         AllowsDBNull={2}\n\t{1}           ServerName='{3}'\n\t{1}          CatalogName='{4}'\n\t{1}           SchemaName='{5}'\n\t{1}            TableName='{6}'\n\t{1}           ColumnName='{7}'\n\t{1}                IsKey={8}\n\t{1}           IsIdentity={9}\n\t", new object[] { base.TraceString(indent), new string(' ', indent), this.AllowsDBNull, (this.ServerName != null) ? this.ServerName : "<null>", (this.CatalogName != null) ? this.CatalogName : "<null>", (this.SchemaName != null) ? this.SchemaName : "<null>", (this.TableName != null) ? this.TableName : "<null>", (this.ColumnName != null) ? this.ColumnName : "<null>", this.IsKey, this.IsIdentity });
        }

        internal bool AllowsDBNull
        {
            get
            {
                return this._allowsDBNull;
            }
        }

        internal string CatalogName
        {
            get
            {
                return this._catalogName;
            }
        }

        internal string ColumnName
        {
            get
            {
                return this._columnName;
            }
        }

        internal bool IsColumnSet
        {
            get
            {
                return this._isColumnSet;
            }
        }

        internal bool IsIdentity
        {
            get
            {
                return this._isIdentity;
            }
        }

        internal SqlBoolean IsKey
        {
            get
            {
                return this._isKey;
            }
        }

        internal string SchemaName
        {
            get
            {
                return this._schemaName;
            }
        }

        internal string ServerName
        {
            get
            {
                return this._serverName;
            }
        }

        internal string TableName
        {
            get
            {
                return this._tableName;
            }
        }
    }
}


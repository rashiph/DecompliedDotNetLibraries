namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    internal class SqlMetaDataPriv
    {
        internal int codePage;
        internal SqlCollation collation;
        internal Encoding encoding;
        internal bool isMultiValued;
        internal bool isNullable;
        internal int length;
        internal MetaType metaType;
        internal byte precision = 0xff;
        internal byte scale = 0xff;
        internal IList<SmiMetaData> structuredFields;
        internal string structuredTypeDatabaseName;
        internal string structuredTypeName;
        internal string structuredTypeSchemaName;
        internal byte tdsType;
        internal SqlDbType type;
        internal string udtAssemblyQualifiedName;
        internal string udtDatabaseName;
        internal string udtSchemaName;
        internal Type udtType;
        internal string udtTypeName;
        internal string xmlSchemaCollectionDatabase;
        internal string xmlSchemaCollectionName;
        internal string xmlSchemaCollectionOwningSchema;

        internal SqlMetaDataPriv()
        {
        }
    }
}


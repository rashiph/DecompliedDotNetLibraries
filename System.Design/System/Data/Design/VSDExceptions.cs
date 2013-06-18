namespace System.Data.Design
{
    using System;

    internal class VSDExceptions
    {
        private VSDExceptions()
        {
        }

        internal class COMMON
        {
            internal const int NOT_A_NAMED_OBJECT_CODE = 2;
            internal const string NOT_A_NAMED_OBJECT_MSG = "Named object collection holds something that is not a named object";
            internal const int SIMPLENAMESERVICE_NAMEOVERFLOW_CODE = 1;
            internal const string SIMPLENAMESERVICE_NAMEOVERFLOW_MSG = "Failed to create unique name after many attempts";
            internal const int START_CODE = 0;

            private COMMON()
            {
            }
        }

        internal class DataSource
        {
            internal const int BAD_QUERY_FOR_GENERATING_IUD_CODE = 0x4e2e;
            internal const string BAD_QUERY_FOR_GENERATING_IUD_MSG = "Cannot generate updating statements from query that is not a SELECT query";
            internal const int BINDABLE_CTRL_NOT_FOUND_CODE = 0x4e3e;
            internal const string BINDABLE_CTRL_NOT_FOUND_MSG = "Bindable control could not be found; Visual Studio data binding configuration file might be corrupt";
            internal const int CANNOT_GET_COMMAND_BUILDER_FROM_DBSOURCE_CODE = 0x4e44;
            internal const string CANNOT_GET_COMMAND_BUILDER_FROM_DBSOURCE_MSG = "Failed to get a command builder for the DbSource.";
            internal const int CANNOT_GET_DATA_CONFIGURATION_CONTEXT_CODE = 0x4e31;
            internal const string CANNOT_GET_DATA_CONFIGURATION_CONTEXT_MSG = "Data configuration context could not be obtained";
            internal const int CANNOT_GET_IDBPROVIDER_CODE = 0x4e32;
            internal const int CANNOT_GET_IDBPROVIDER_FOR_CONNECTION_CODE = 0x4e35;
            internal const string CANNOT_GET_IDBPROVIDER_FOR_CONNECTION_MSG = "Could not get data provider information for connection of type {0}";
            internal const int CANNOT_GET_IDBPROVIDER_FOR_CSTRING_CODE = 0x4e36;
            internal const string CANNOT_GET_IDBPROVIDER_FOR_CSTRING_MSG = "Could not get data provider information for the following connection string:\r\n{0}\r\n";
            internal const string CANNOT_GET_IDBPROVIDER_MSG = "Data provider information could not be obtained";
            internal const int CANNOT_GET_PROVIDER_FACTORY_CODE = 0x4e3f;
            internal const string CANNOT_GET_PROVIDER_FACTORY_MSG = "Could not retrieve the provider factory.";
            internal const int COMMAND_NOT_SET_CODE = 0x4e2c;
            internal const string COMMAND_NOT_SET_MSG = "Command not set. Operation cannot be performed";
            internal const int COMMAND_OPERATION_INVALID_CODE = 0x4e3b;
            internal const string COMMAND_OPERATION_INVALID_MSG = "Specified command operation is invalid (can only be INSERT, UPDATE or DELETE)";
            internal const int CONF_CONTEXT_NULL_CODE = 0x4e3a;
            internal const string CONF_CONTEXT_NULL_MSG = "Data configuration context is null";
            internal const int CONNECTION_NOT_SET_CODE = 0x4e2d;
            internal const string CONNECTION_NOT_SET_MSG = "Connection not set. Operation cannot be performed";
            internal const int DATASOURCECOLLECTIONUNDOUNIT_INVALIDPARA_CODE = 0x4e34;
            internal const string DATASOURCECOLLECTIONUNDOUNIT_INVALIDPARA_MSG = "Invalid parameters in DataSourceCollectionUndounit";
            internal const int DBSOURCECMD_HAS_INVALID_PARENT_CODE = 0x4e33;
            internal const string DBSOURCECMD_HAS_INVALID_PARENT_MSG = "Parent of the DbSourceCommand is invalid";
            internal const int DESIGN_COLUMN_NEEDS_DATA_COLUMN_CODE = 0x4e29;
            internal const string DESIGN_COLUMN_NEEDS_DATA_COLUMN_MSG = "DesignColumn object needs a valid DataColumn";
            internal const int FACTORY_CANNOT_CREATE_ADAPTERS_CODE = 0x4e40;
            internal const string FACTORY_CANNOT_CREATE_ADAPTERS_MSG = "The provider factory does not support creating adapters.";
            internal const int FACTORY_CANNOT_CREATE_COMMAND_BUILDERS_CODE = 0x4e41;
            internal const string FACTORY_CANNOT_CREATE_COMMAND_BUILDERS_MSG = "The provider factory does not support creating command builders.";
            internal const int FACTORY_COULD_NOT_CREATE_ADAPTER_CODE = 0x4e42;
            internal const string FACTORY_COULD_NOT_CREATE_ADAPTER_MSG = "The provider factory failed to create an adapter.";
            internal const int FACTORY_COULD_NOT_CREATE_COMMAND_BUILDER_CODE = 0x4e43;
            internal const string FACTORY_COULD_NOT_CREATE_COMMAND_BUILDER_MSG = "The provider factory failed to create a command builder.";
            internal const int INVALID_COLLECTIONTYPE_CODE = 0x4e30;
            internal const string INVALID_COLLECTIONTYPE_MSG = "{0} can hold only {1} objects";
            internal const int INVALID_COLUMN_INDEX_CODE = 0x4e2b;
            internal const string INVALID_COLUMN_INDEX_MSG = "Index out of range in getting DesignColumn";
            internal const int INVALID_COLUMN_VALUE_CODE = 0x4e28;
            internal const string INVALID_COLUMN_VALUE_MSG = "DesignColumnCollection can only hold objects of type DesignColumn";
            internal const int INVALID_DATA_SOURCE_CODE = 0x4e37;
            internal const string INVALID_DATA_SOURCE_MSG = "Data source is invalid (null)";
            internal const int INVALID_DATA_SOURCE_NAME_CODE = 0x4e2f;
            internal const string INVALID_DATA_SOURCE_NAME_MSG = "Data source name is empty or invalid";
            internal const int INVALID_PARAMETER_VALUE_CODE = 0x4e25;
            internal const string INVALID_PARAMETER_VALUE_MSG = "Invalid parameter value (the object passed in must support IDbDataParameter interface)";
            internal const int INVALID_SOURCE_VALUE_CODE = 0x4e26;
            internal const string INVALID_SOURCE_VALUE_MSG = "SourceCollection can only hold objects of type Source";
            internal const int NO_CONNECTION_NAME_SERVICE_CODE = 0x4e21;
            internal const string NO_CONNECTION_NAME_SERVICE_MSG = "Failed to obtain name service for connection collection";
            internal const int NO_SPROC_GENERATOR_CODE = 0x4e3c;
            internal const string NO_SPROC_GENERATOR_MSG = "We have no stored procedure generator for specified backend";
            internal const int OP_VALID_FOR_RAD_TABLE_ONLY_CODE = 0x4e27;
            internal const string OP_VALID_FOR_RAD_TABLE_ONLY_MSG = "Operation invalid. Table gets data from something else than a database.";
            internal const int PARAMETER_NOT_FOUND_CODE = 0x4e24;
            internal const string PARAMETER_NOT_FOUND_MSG = "No parameter named '{0}' found";
            internal const int PK_COLUMNS_MISSING_CODE = 0x4e39;
            internal const string PK_COLUMNS_MISSING_MSG = "Some primary key columns are missing";
            internal const int RELATION_BELONGS_TO_OTHER_DATA_SOURCE_CODE = 0x4e2a;
            internal const string RELATION_BELONGS_TO_OTHER_DATA_SOURCE_MSG = "This relation belongs to another DataSource already";
            internal const int SELECT_COMMAND_TEXT_EMPTY_CODE = 0x4e38;
            internal const string SELECT_COMMAND_TEXT_EMPTY_MSG = "Select command text is null or empty";
            internal const int SOURCE_IS_NOT_DBSOURCE_CODE = 0x4e3d;
            internal const string SOURCE_IS_NOT_DBSOURCE_MSG = "The source for this table is not a DbSource";
            internal const int SPROC_NAME_EMPTY_CODE = 0x4e3c;
            internal const string SPROC_NAME_EMPTY_MSG = "The stored procedure name is null or empty";
            internal const int START_CODE = 0x4e20;
            internal const int TABLE_BELONGS_TO_OTHER_DATA_SOURCE_CODE = 0x4e22;
            internal const string TABLE_BELONGS_TO_OTHER_DATA_SOURCE_MSG = "This table belongs to another DataSource already";

            private DataSource()
            {
            }
        }
    }
}


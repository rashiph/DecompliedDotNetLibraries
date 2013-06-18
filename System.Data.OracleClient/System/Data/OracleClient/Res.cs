namespace System.Data.OracleClient
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class Res
    {
        internal const string ADP_BadBindValueType = "ADP_BadBindValueType";
        internal const string ADP_BadOracleClientImageFormat = "ADP_BadOracleClientImageFormat";
        internal const string ADP_BadOracleClientVersion = "ADP_BadOracleClientVersion";
        internal const string ADP_BufferExceeded = "ADP_BufferExceeded";
        internal const string ADP_CannotDeriveOverloaded = "ADP_CannotDeriveOverloaded";
        internal const string ADP_CannotOpenLobWithDifferentMode = "ADP_CannotOpenLobWithDifferentMode";
        internal const string ADP_ChangeDatabaseNotSupported = "ADP_ChangeDatabaseNotSupported";
        internal const string ADP_ClosedConnectionError = "ADP_ClosedConnectionError";
        internal const string ADP_ClosedDataReaderError = "ADP_ClosedDataReaderError";
        internal const string ADP_CollectionIndexInt32 = "ADP_CollectionIndexInt32";
        internal const string ADP_CollectionIndexString = "ADP_CollectionIndexString";
        internal const string ADP_CollectionInvalidType = "ADP_CollectionInvalidType";
        internal const string ADP_CollectionIsNotParent = "ADP_CollectionIsNotParent";
        internal const string ADP_CollectionIsParent = "ADP_CollectionIsParent";
        internal const string ADP_CollectionNullValue = "ADP_CollectionNullValue";
        internal const string ADP_CollectionRemoveInvalidObject = "ADP_CollectionRemoveInvalidObject";
        internal const string ADP_CommandTextRequired = "ADP_CommandTextRequired";
        internal const string ADP_ConfigUnableToLoadXmlMetaDataFile = "ADP_ConfigUnableToLoadXmlMetaDataFile";
        internal const string ADP_ConfigWrongNumberOfValues = "ADP_ConfigWrongNumberOfValues";
        internal const string ADP_ConnectionAlreadyOpen = "ADP_ConnectionAlreadyOpen";
        internal const string ADP_ConnectionRequired = "ADP_ConnectionRequired";
        internal const string ADP_ConnectionStateMsg = "ADP_ConnectionStateMsg";
        internal const string ADP_ConnectionStateMsg_Closed = "ADP_ConnectionStateMsg_Closed";
        internal const string ADP_ConnectionStateMsg_Connecting = "ADP_ConnectionStateMsg_Connecting";
        internal const string ADP_ConnectionStateMsg_Open = "ADP_ConnectionStateMsg_Open";
        internal const string ADP_ConnectionStateMsg_OpenExecuting = "ADP_ConnectionStateMsg_OpenExecuting";
        internal const string ADP_ConnectionStateMsg_OpenFetching = "ADP_ConnectionStateMsg_OpenFetching";
        internal const string ADP_ConnectionStringSyntax = "ADP_ConnectionStringSyntax";
        internal const string ADP_ConvertFailed = "ADP_ConvertFailed";
        internal const string ADP_CouldNotCreateEnvironment = "ADP_CouldNotCreateEnvironment";
        internal const string ADP_DataIsNull = "ADP_DataIsNull";
        internal const string ADP_DataReaderClosed = "ADP_DataReaderClosed";
        internal const string ADP_DataReaderNoData = "ADP_DataReaderNoData";
        internal const string ADP_DeriveParametersNotSupported = "ADP_DeriveParametersNotSupported";
        internal const string ADP_DistribTxRequiresOracle9i = "ADP_DistribTxRequiresOracle9i";
        internal const string ADP_DistribTxRequiresOracleServicesForMTS = "ADP_DistribTxRequiresOracleServicesForMTS";
        internal const string ADP_EmptyString = "ADP_EmptyString";
        internal const string ADP_IdentifierIsNotQuoted = "ADP_IdentifierIsNotQuoted";
        internal const string ADP_InputRefCursorNotSupported = "ADP_InputRefCursorNotSupported";
        internal const string ADP_InternalConnectionError = "ADP_InternalConnectionError";
        internal const string ADP_InternalError = "ADP_InternalError";
        internal const string ADP_InternalProviderError = "ADP_InternalProviderError";
        internal const string ADP_InvalidCommandType = "ADP_InvalidCommandType";
        internal const string ADP_InvalidConnectionOptionLength = "ADP_InvalidConnectionOptionLength";
        internal const string ADP_InvalidConnectionOptionValue = "ADP_InvalidConnectionOptionValue";
        internal const string ADP_InvalidDataDirectory = "ADP_InvalidDataDirectory";
        internal const string ADP_InvalidDataLength = "ADP_InvalidDataLength";
        internal const string ADP_InvalidDataType = "ADP_InvalidDataType";
        internal const string ADP_InvalidDataTypeForValue = "ADP_InvalidDataTypeForValue";
        internal const string ADP_InvalidDbType = "ADP_InvalidDbType";
        internal const string ADP_InvalidDestinationBufferIndex = "ADP_InvalidDestinationBufferIndex";
        internal const string ADP_InvalidEnumerationValue = "ADP_InvalidEnumerationValue";
        internal const string ADP_InvalidKey = "ADP_InvalidKey";
        internal const string ADP_InvalidLobType = "ADP_InvalidLobType";
        internal const string ADP_InvalidMinMaxPoolSizeValues = "ADP_InvalidMinMaxPoolSizeValues";
        internal const string ADP_InvalidOffsetValue = "ADP_InvalidOffsetValue";
        internal const string ADP_InvalidOracleType = "ADP_InvalidOracleType";
        internal const string ADP_InvalidSeekOrigin = "ADP_InvalidSeekOrigin";
        internal const string ADP_InvalidSizeValue = "ADP_InvalidSizeValue";
        internal const string ADP_InvalidSourceBufferIndex = "ADP_InvalidSourceBufferIndex";
        internal const string ADP_InvalidSourceOffset = "ADP_InvalidSourceOffset";
        internal const string ADP_InvalidValue = "ADP_InvalidValue";
        internal const string ADP_InvalidXMLBadVersion = "ADP_InvalidXMLBadVersion";
        internal const string ADP_KeywordNotSupported = "ADP_KeywordNotSupported";
        internal const string ADP_LobAmountExceeded = "ADP_LobAmountExceeded";
        internal const string ADP_LobAmountMustBeEven = "ADP_LobAmountMustBeEven";
        internal const string ADP_LobPositionMustBeEven = "ADP_LobPositionMustBeEven";
        internal const string ADP_LobWriteInvalidOnNull = "ADP_LobWriteInvalidOnNull";
        internal const string ADP_LobWriteRequiresTransaction = "ADP_LobWriteRequiresTransaction";
        internal const string ADP_MonthOutOfRange = "ADP_MonthOutOfRange";
        internal const string ADP_MustBePositive = "ADP_MustBePositive";
        internal const string ADP_NoCommandText = "ADP_NoCommandText";
        internal const string ADP_NoConnectionString = "ADP_NoConnectionString";
        internal const string ADP_NoData = "ADP_NoData";
        internal const string ADP_NoLocalTransactionInDistributedContext = "ADP_NoLocalTransactionInDistributedContext";
        internal const string ADP_NoMessageAvailable = "ADP_NoMessageAvailable";
        internal const string ADP_NoOptimizedDirectTableAccess = "ADP_NoOptimizedDirectTableAccess";
        internal const string ADP_NoParallelTransactions = "ADP_NoParallelTransactions";
        internal const string ADP_NotAPermissionElement = "ADP_NotAPermissionElement";
        internal const string ADP_OpenConnectionPropertySet = "ADP_OpenConnectionPropertySet";
        internal const string ADP_OpenConnectionRequired = "ADP_OpenConnectionRequired";
        internal const string ADP_OperationFailed = "ADP_OperationFailed";
        internal const string ADP_OperationResultedInOverflow = "ADP_OperationResultedInOverflow";
        internal const string ADP_ParameterConversionFailed = "ADP_ParameterConversionFailed";
        internal const string ADP_ParameterSizeIsMissing = "ADP_ParameterSizeIsMissing";
        internal const string ADP_ParameterSizeIsTooLarge = "ADP_ParameterSizeIsTooLarge";
        internal const string ADP_PermissionTypeMismatch = "ADP_PermissionTypeMismatch";
        internal const string ADP_PleaseUninstallTheBeta = "ADP_PleaseUninstallTheBeta";
        internal const string ADP_PooledOpenTimeout = "ADP_PooledOpenTimeout";
        internal const string ADP_ReadOnlyLob = "ADP_ReadOnlyLob";
        internal const string ADP_SeekBeyondEnd = "ADP_SeekBeyondEnd";
        internal const string ADP_SQLParserInternalError = "ADP_SQLParserInternalError";
        internal const string ADP_SyntaxErrorExpectedCommaAfterColumn = "ADP_SyntaxErrorExpectedCommaAfterColumn";
        internal const string ADP_SyntaxErrorExpectedCommaAfterTable = "ADP_SyntaxErrorExpectedCommaAfterTable";
        internal const string ADP_SyntaxErrorExpectedIdentifier = "ADP_SyntaxErrorExpectedIdentifier";
        internal const string ADP_SyntaxErrorExpectedNextPart = "ADP_SyntaxErrorExpectedNextPart";
        internal const string ADP_SyntaxErrorMissingParenthesis = "ADP_SyntaxErrorMissingParenthesis";
        internal const string ADP_SyntaxErrorTooManyNameParts = "ADP_SyntaxErrorTooManyNameParts";
        internal const string ADP_TransactionCompleted = "ADP_TransactionCompleted";
        internal const string ADP_TransactionConnectionMismatch = "ADP_TransactionConnectionMismatch";
        internal const string ADP_TransactionPresent = "ADP_TransactionPresent";
        internal const string ADP_TransactionRequired_Execute = "ADP_TransactionRequired_Execute";
        internal const string ADP_TypeNotSupported = "ADP_TypeNotSupported";
        internal const string ADP_UnexpectedReturnCode = "ADP_UnexpectedReturnCode";
        internal const string ADP_UnknownDataTypeCode = "ADP_UnknownDataTypeCode";
        internal const string ADP_UnsupportedIsolationLevel = "ADP_UnsupportedIsolationLevel";
        internal const string ADP_WriteByteForBinaryLobsOnly = "ADP_WriteByteForBinaryLobsOnly";
        internal const string ADP_WrongType = "ADP_WrongType";
        internal const string DataCategory_Advanced = "DataCategory_Advanced";
        internal const string DataCategory_Data = "DataCategory_Data";
        internal const string DataCategory_Initialization = "DataCategory_Initialization";
        internal const string DataCategory_Pooling = "DataCategory_Pooling";
        internal const string DataCategory_Security = "DataCategory_Security";
        internal const string DataCategory_Source = "DataCategory_Source";
        internal const string DataCategory_StateChange = "DataCategory_StateChange";
        internal const string DataCategory_Update = "DataCategory_Update";
        internal const string DbCommand_CommandText = "DbCommand_CommandText";
        internal const string DbCommand_CommandTimeout = "DbCommand_CommandTimeout";
        internal const string DbCommand_CommandType = "DbCommand_CommandType";
        internal const string DbCommand_Connection = "DbCommand_Connection";
        internal const string DbCommand_Parameters = "DbCommand_Parameters";
        internal const string DbCommand_Transaction = "DbCommand_Transaction";
        internal const string DbCommand_UpdatedRowSource = "DbCommand_UpdatedRowSource";
        internal const string DbConnection_State = "DbConnection_State";
        internal const string DbConnection_StateChange = "DbConnection_StateChange";
        internal const string DbConnectionString_ConnectionString = "DbConnectionString_ConnectionString";
        internal const string DbConnectionString_DataSource = "DbConnectionString_DataSource";
        internal const string DbConnectionString_Enlist = "DbConnectionString_Enlist";
        internal const string DbConnectionString_IntegratedSecurity = "DbConnectionString_IntegratedSecurity";
        internal const string DbConnectionString_LoadBalanceTimeout = "DbConnectionString_LoadBalanceTimeout";
        internal const string DbConnectionString_MaxPoolSize = "DbConnectionString_MaxPoolSize";
        internal const string DbConnectionString_MinPoolSize = "DbConnectionString_MinPoolSize";
        internal const string DbConnectionString_OmitOracleConnectionName = "DbConnectionString_OmitOracleConnectionName";
        internal const string DbConnectionString_Password = "DbConnectionString_Password";
        internal const string DbConnectionString_PersistSecurityInfo = "DbConnectionString_PersistSecurityInfo";
        internal const string DbConnectionString_Pooling = "DbConnectionString_Pooling";
        internal const string DbConnectionString_Unicode = "DbConnectionString_Unicode";
        internal const string DbConnectionString_UserID = "DbConnectionString_UserID";
        internal const string DbDataAdapter_DeleteCommand = "DbDataAdapter_DeleteCommand";
        internal const string DbDataAdapter_InsertCommand = "DbDataAdapter_InsertCommand";
        internal const string DbDataAdapter_RowUpdated = "DbDataAdapter_RowUpdated";
        internal const string DbDataAdapter_RowUpdating = "DbDataAdapter_RowUpdating";
        internal const string DbDataAdapter_SelectCommand = "DbDataAdapter_SelectCommand";
        internal const string DbDataAdapter_UpdateCommand = "DbDataAdapter_UpdateCommand";
        internal const string DbParameter_DbType = "DbParameter_DbType";
        internal const string DbParameter_Direction = "DbParameter_Direction";
        internal const string DbParameter_IsNullable = "DbParameter_IsNullable";
        internal const string DbParameter_Offset = "DbParameter_Offset";
        internal const string DbParameter_ParameterName = "DbParameter_ParameterName";
        internal const string DbParameter_Size = "DbParameter_Size";
        internal const string DbParameter_SourceColumn = "DbParameter_SourceColumn";
        internal const string DbParameter_SourceColumnNullMapping = "DbParameter_SourceColumnNullMapping";
        internal const string DbParameter_SourceVersion = "DbParameter_SourceVersion";
        internal const string DbParameter_Value = "DbParameter_Value";
        internal const string DbTable_Connection = "DbTable_Connection";
        internal const string DbTable_DeleteCommand = "DbTable_DeleteCommand";
        internal const string DbTable_InsertCommand = "DbTable_InsertCommand";
        internal const string DbTable_SelectCommand = "DbTable_SelectCommand";
        internal const string DbTable_UpdateCommand = "DbTable_UpdateCommand";
        private static Res loader;
        internal const string MDF_AmbigousCollectionName = "MDF_AmbigousCollectionName";
        internal const string MDF_CollectionNameISNotUnique = "MDF_CollectionNameISNotUnique";
        internal const string MDF_DataTableDoesNotExist = "MDF_DataTableDoesNotExist";
        internal const string MDF_IncorrectNumberOfDataSourceInformationRows = "MDF_IncorrectNumberOfDataSourceInformationRows";
        internal const string MDF_InvalidRestrictionValue = "MDF_InvalidRestrictionValue";
        internal const string MDF_InvalidXml = "MDF_InvalidXml";
        internal const string MDF_InvalidXmlInvalidValue = "MDF_InvalidXmlInvalidValue";
        internal const string MDF_InvalidXmlMissingColumn = "MDF_InvalidXmlMissingColumn";
        internal const string MDF_MissingDataSourceInformationColumn = "MDF_MissingDataSourceInformationColumn";
        internal const string MDF_MissingRestrictionColumn = "MDF_MissingRestrictionColumn";
        internal const string MDF_MissingRestrictionRow = "MDF_MissingRestrictionRow";
        internal const string MDF_NoColumns = "MDF_NoColumns";
        internal const string MDF_QueryFailed = "MDF_QueryFailed";
        internal const string MDF_TooManyRestrictions = "MDF_TooManyRestrictions";
        internal const string MDF_UnableToBuildCollection = "MDF_UnableToBuildCollection";
        internal const string MDF_UndefinedCollection = "MDF_UndefinedCollection";
        internal const string MDF_UndefinedPopulationMechanism = "MDF_UndefinedPopulationMechanism";
        internal const string MDF_UnsupportedVersion = "MDF_UnsupportedVersion";
        internal const string OracleCategory_Behavior = "OracleCategory_Behavior";
        internal const string OracleCategory_Data = "OracleCategory_Data";
        internal const string OracleCategory_Fill = "OracleCategory_Fill";
        internal const string OracleCategory_InfoMessage = "OracleCategory_InfoMessage";
        internal const string OracleCategory_StateChange = "OracleCategory_StateChange";
        internal const string OracleCategory_Update = "OracleCategory_Update";
        internal const string OracleCommandBuilder_DataAdapter = "OracleCommandBuilder_DataAdapter";
        internal const string OracleCommandBuilder_QuotePrefix = "OracleCommandBuilder_QuotePrefix";
        internal const string OracleCommandBuilder_QuoteSuffix = "OracleCommandBuilder_QuoteSuffix";
        internal const string OracleConnection_ConnectionString = "OracleConnection_ConnectionString";
        internal const string OracleConnection_DataSource = "OracleConnection_DataSource";
        internal const string OracleConnection_InfoMessage = "OracleConnection_InfoMessage";
        internal const string OracleConnection_ServerVersion = "OracleConnection_ServerVersion";
        internal const string OracleConnection_State = "OracleConnection_State";
        internal const string OracleConnection_StateChange = "OracleConnection_StateChange";
        internal const string OracleMetaDataFactory_XML = "OracleMetaDataFactory_XML";
        internal const string OracleParameter_OracleType = "OracleParameter_OracleType";
        private ResourceManager resources;
        internal const string SqlMisc_NullString = "SqlMisc_NullString";

        internal Res()
        {
            this.resources = new ResourceManager("System.Data.OracleClient", base.GetType().Assembly);
        }

        private static Res GetLoader()
        {
            if (loader == null)
            {
                Res res = new Res();
                Interlocked.CompareExchange<Res>(ref loader, res, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str = args[i] as string;
                if ((str != null) && (str.Length > 0x400))
                {
                    args[i] = str.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}


namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Transactions;

    internal sealed class SQL
    {
        internal static readonly byte[] AttentionHeader = new byte[] { 6, 1, 0, 8, 0, 0, 0, 0 };
        internal const string Connection = "Connection";
        internal const int SqlDependencyServerTimeout = 0x69780;
        internal const int SqlDependencyTimeoutDefault = 0;
        internal const string SqlNotificationServiceDefault = "SqlQueryNotificationService";
        internal const string SqlNotificationStoredProcedureDefault = "SqlQueryNotificationStoredProcedure";
        internal const string Transaction = "Transaction";
        internal const string WriteToServer = "WriteToServer";

        private SQL()
        {
        }

        internal static Exception ArgumentLengthMismatch(string arg1, string arg2)
        {
            return ADP.Argument(Res.GetString("SQL_ArgumentLengthMismatch", new object[] { arg1, arg2 }));
        }

        internal static Exception AsyncConnectionRequired()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_AsyncConnectionRequired"));
        }

        internal static Exception AsyncInProcNotSupported()
        {
            return ADP.NotSupported(Res.GetString("SQL_AsyncInProcNotSupported"));
        }

        internal static Exception BatchedUpdatesNotAvailableOnContextConnection()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BatchedUpdatesNotAvailableOnContextConnection"));
        }

        internal static Exception BulkLoadBulkLoadNotAllowDBNull(string columnName)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadNotAllowDBNull", new object[] { columnName }));
        }

        internal static Exception BulkLoadCannotConvertValue(Type sourcetype, MetaType metatype, Exception e)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadCannotConvertValue", new object[] { sourcetype.Name, metatype.TypeName }), e);
        }

        internal static Exception BulkLoadConflictingTransactionOption()
        {
            return ADP.Argument(Res.GetString("SQL_BulkLoadConflictingTransactionOption"));
        }

        internal static Exception BulkLoadExistingTransaction()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadExistingTransaction"));
        }

        internal static Exception BulkLoadInvalidDestinationTable(string tableName, Exception inner)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadInvalidDestinationTable", new object[] { tableName }), inner);
        }

        internal static Exception BulkLoadInvalidTimeout(int timeout)
        {
            return ADP.Argument(Res.GetString("SQL_BulkLoadInvalidTimeout", new object[] { timeout.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception BulkLoadInvalidVariantValue()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadInvalidVariantValue"));
        }

        internal static Exception BulkLoadLcidMismatch(int sourceLcid, string sourceColumnName, int destinationLcid, string destinationColumnName)
        {
            return ADP.InvalidOperation(Res.GetString("Sql_BulkLoadLcidMismatch", new object[] { sourceLcid, sourceColumnName, destinationLcid, destinationColumnName }));
        }

        internal static Exception BulkLoadMappingInaccessible()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadMappingInaccessible"));
        }

        internal static Exception BulkLoadMappingsNamesOrOrdinalsOnly()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadMappingsNamesOrOrdinalsOnly"));
        }

        internal static Exception BulkLoadMissingDestinationTable()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadMissingDestinationTable"));
        }

        internal static Exception BulkLoadNoCollation()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadNoCollation"));
        }

        internal static Exception BulkLoadNonMatchingColumnMapping()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadNonMatchingColumnMapping"));
        }

        internal static Exception BulkLoadNonMatchingColumnName(string columnName)
        {
            return BulkLoadNonMatchingColumnName(columnName, null);
        }

        internal static Exception BulkLoadNonMatchingColumnName(string columnName, Exception e)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadNonMatchingColumnName", new object[] { columnName }), e);
        }

        internal static Exception BulkLoadStringTooLong()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadStringTooLong"));
        }

        internal static Exception CannotCompleteDelegatedTransactionWithOpenResults()
        {
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            errorCollection.Add(new SqlError(-2, 0, 11, null, Res.GetString("ADP_OpenReaderExists"), "", 0));
            return SqlException.CreateException(errorCollection, null);
        }

        internal static Exception CannotGetDTCAddress()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_CannotGetDTCAddress"));
        }

        internal static Exception CannotModifyPropertyAsyncOperationInProgress(string property)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_CannotModifyPropertyAsyncOperationInProgress", new object[] { property }));
        }

        internal static Exception ChangePasswordArgumentMissing(string argumentName)
        {
            return ADP.ArgumentNull(Res.GetString("SQL_ChangePasswordArgumentMissing", new object[] { argumentName }));
        }

        internal static Exception ChangePasswordConflictsWithSSPI()
        {
            return ADP.Argument(Res.GetString("SQL_ChangePasswordConflictsWithSSPI"));
        }

        internal static Exception ChangePasswordRequiresYukon()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ChangePasswordRequiresYukon"));
        }

        internal static Exception ChangePasswordUseOfUnallowedKey(string key)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ChangePasswordUseOfUnallowedKey", new object[] { key }));
        }

        internal static Exception ConnectionDoomed()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ConnectionDoomed"));
        }

        internal static Exception ConnectionLockedForBcpEvent()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ConnectionLockedForBcpEvent"));
        }

        internal static Exception ContextAllowsLimitedKeywords()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ContextAllowsLimitedKeywords"));
        }

        internal static Exception ContextAllowsOnlyTypeSystem2005()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ContextAllowsOnlyTypeSystem2005"));
        }

        internal static Exception ContextConnectionIsInUse()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ContextConnectionIsInUse"));
        }

        internal static Exception ContextUnavailableOutOfProc()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ContextUnavailableOutOfProc"));
        }

        internal static Exception ContextUnavailableWhileInProc()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ContextUnavailableWhileInProc"));
        }

        internal static Exception DBNullNotSupportedForTVPValues(string paramName)
        {
            return ADP.NotSupported(Res.GetString("SqlParameter_DBNullNotSupportedForTVP", new object[] { paramName }));
        }

        internal static Exception DuplicateSortOrdinal(int sortOrdinal)
        {
            return ADP.InvalidOperation(Res.GetString("SqlProvider_DuplicateSortOrdinal", new object[] { sortOrdinal }));
        }

        internal static Exception EnumeratedRecordFieldCountChanged(int recordNumber)
        {
            return ADP.Argument(Res.GetString("SQL_EnumeratedRecordFieldCountChanged", new object[] { recordNumber }));
        }

        internal static Exception EnumeratedRecordMetaDataChanged(string fieldName, int recordNumber)
        {
            return ADP.Argument(Res.GetString("SQL_EnumeratedRecordMetaDataChanged", new object[] { fieldName, recordNumber }));
        }

        internal static Exception FatalTimeout()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_FatalTimeout"));
        }

        internal static string GetSNIErrorMessage(int sniError)
        {
            return Res.GetString(string.Format(null, "SNI_ERROR_{0}", new object[] { sniError }));
        }

        internal static Exception IEnumerableOfSqlDataRecordHasNoRows()
        {
            return ADP.Argument(Res.GetString("IEnumerableOfSqlDataRecordHasNoRows"));
        }

        internal static Exception InstanceFailure()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_InstanceFailure"));
        }

        internal static Exception InvalidColumnMaxLength(string columnName, long maxLength)
        {
            return ADP.Argument(Res.GetString("SqlProvider_InvalidDataColumnMaxLength", new object[] { columnName, maxLength }));
        }

        internal static Exception InvalidColumnPrecScale()
        {
            return ADP.Argument(Res.GetString("SqlMisc_InvalidPrecScaleMessage"));
        }

        internal static Exception InvalidInternalPacketSize(string str)
        {
            return ADP.ArgumentOutOfRange(str);
        }

        internal static Exception InvalidOperationInsideEvent()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_BulkLoadInvalidOperationInsideEvent"));
        }

        internal static Exception InvalidOptionLength(string key)
        {
            return ADP.Argument(Res.GetString("SQL_InvalidOptionLength", new object[] { key }));
        }

        internal static Exception InvalidPacketSize()
        {
            return ADP.ArgumentOutOfRange(Res.GetString("SQL_InvalidTDSPacketSize"));
        }

        internal static Exception InvalidPacketSizeValue()
        {
            return ADP.Argument(Res.GetString("SQL_InvalidPacketSizeValue"));
        }

        internal static Exception InvalidParameterNameLength(string value)
        {
            return ADP.Argument(Res.GetString("SQL_InvalidParameterNameLength", new object[] { value }));
        }

        internal static Exception InvalidParameterTypeNameFormat()
        {
            return ADP.Argument(Res.GetString("SQL_InvalidParameterTypeNameFormat"));
        }

        internal static Exception InvalidPartnerConfiguration(string server, string database)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_InvalidPartnerConfiguration", new object[] { server, database }));
        }

        internal static Exception InvalidRead()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_InvalidRead"));
        }

        internal static Exception InvalidSchemaTableOrdinals()
        {
            return ADP.Argument(Res.GetString("InvalidSchemaTableOrdinals"));
        }

        internal static Exception InvalidSortOrder(SortOrder order)
        {
            return ADP.InvalidEnumerationValue(typeof(SortOrder), (int) order);
        }

        internal static Exception InvalidSqlDbType(SqlDbType value)
        {
            return ADP.InvalidEnumerationValue(typeof(SqlDbType), (int) value);
        }

        internal static Exception InvalidSqlDbTypeForConstructor(SqlDbType type)
        {
            return ADP.Argument(Res.GetString("SqlMetaData_InvalidSqlDbTypeForConstructorFormat", new object[] { type.ToString() }));
        }

        internal static Exception InvalidSqlDbTypeOneAllowedType(SqlDbType invalidType, string method, SqlDbType allowedType)
        {
            return ADP.Argument(Res.GetString("SQL_InvalidSqlDbTypeWithOneAllowedType", new object[] { invalidType, method, allowedType }));
        }

        internal static ArgumentOutOfRangeException InvalidSqlDependencyTimeout(string param)
        {
            return ADP.ArgumentOutOfRange(Res.GetString("SqlDependency_InvalidTimeout"), param);
        }

        internal static Exception InvalidSQLServerVersionUnknown()
        {
            return ADP.DataAdapter(Res.GetString("SQL_InvalidSQLServerVersionUnknown"));
        }

        internal static Exception InvalidSSPIPacketSize()
        {
            return ADP.Argument(Res.GetString("SQL_InvalidSSPIPacketSize"));
        }

        internal static Exception InvalidTableDerivedPrecisionForTvp(string columnName, byte precision)
        {
            return ADP.InvalidOperation(Res.GetString("SqlParameter_InvalidTableDerivedPrecisionForTvp", new object[] { precision, columnName, SqlDecimal.MaxPrecision }));
        }

        internal static Exception InvalidTDSVersion()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_InvalidTDSVersion"));
        }

        internal static Exception InvalidUdt3PartNameFormat()
        {
            return ADP.Argument(Res.GetString("SQL_InvalidUdt3PartNameFormat"));
        }

        internal static Exception MARSUnspportedOnConnection()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_MarsUnsupportedOnConnection"));
        }

        internal static Exception MissingSortOrdinal(int sortOrdinal)
        {
            return ADP.InvalidOperation(Res.GetString("SqlProvider_MissingSortOrdinal", new object[] { sortOrdinal }));
        }

        internal static Exception MoneyOverflow(string moneyValue)
        {
            return ADP.Overflow(Res.GetString("SQL_MoneyOverflow", new object[] { moneyValue }));
        }

        internal static Exception MultiSubnetFailoverWithFailoverPartner(bool serverProvidedFailoverPartner)
        {
            string error = Res.GetString("SQLMSF_FailoverPartnerNotSupported");
            if (serverProvidedFailoverPartner)
            {
                return ADP.InvalidOperation(error);
            }
            return ADP.Argument(error);
        }

        internal static Exception MultiSubnetFailoverWithInstanceSpecified()
        {
            return ADP.Argument(GetSNIErrorMessage(0x30));
        }

        internal static Exception MultiSubnetFailoverWithMoreThan64IPs()
        {
            return ADP.InvalidOperation(GetSNIErrorMessage(0x2f));
        }

        internal static Exception MultiSubnetFailoverWithNonTcpProtocol()
        {
            return ADP.Argument(GetSNIErrorMessage(0x31));
        }

        internal static Exception MustSetTypeNameForParam(string paramType, string paramName)
        {
            return ADP.Argument(Res.GetString("SQL_ParameterTypeNameRequired", new object[] { paramType, paramName }));
        }

        internal static Exception MustSetUdtTypeNameForUdtParams()
        {
            return ADP.Argument(Res.GetString("SQLUDT_InvalidUdtTypeName"));
        }

        internal static Exception MustSpecifyBothSortOrderAndOrdinal(SortOrder order, int ordinal)
        {
            return ADP.InvalidOperation(Res.GetString("SqlMetaData_SpecifyBothSortOrderAndOrdinal", new object[] { order.ToString(), ordinal }));
        }

        internal static Exception NameTooLong(string parameterName)
        {
            return ADP.Argument(Res.GetString("SqlMetaData_NameTooLong"), parameterName);
        }

        internal static Exception NestedTransactionScopesNotSupported()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_NestedTransactionScopesNotSupported"));
        }

        internal static Exception NonBlobColumn(string columnName)
        {
            return ADP.InvalidCast(Res.GetString("SQL_NonBlobColumn", new object[] { columnName }));
        }

        internal static Exception NonCharColumn(string columnName)
        {
            return ADP.InvalidCast(Res.GetString("SQL_NonCharColumn", new object[] { columnName }));
        }

        internal static Exception NonLocalSSEInstance()
        {
            return ADP.NotSupported(Res.GetString("SQL_NonLocalSSEInstance"));
        }

        internal static Exception NonXmlResult()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_NonXmlResult"));
        }

        internal static Exception NotAvailableOnContextConnection()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_NotAvailableOnContextConnection"));
        }

        internal static Exception NotEnoughColumnsInStructuredType()
        {
            return ADP.Argument(Res.GetString("SqlProvider_NotEnoughColumnsInStructuredType"));
        }

        internal static Exception NotificationsNotAvailableOnContextConnection()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_NotificationsNotAvailableOnContextConnection"));
        }

        internal static Exception NotificationsRequireYukon()
        {
            return ADP.NotSupported(Res.GetString("SQL_NotificationsRequireYukon"));
        }

        internal static ArgumentOutOfRangeException NotSupportedCommandType(CommandType value)
        {
            return NotSupportedEnumerationValue(typeof(CommandType), (int) value);
        }

        internal static ArgumentOutOfRangeException NotSupportedEnumerationValue(Type type, int value)
        {
            return ADP.ArgumentOutOfRange(Res.GetString("SQL_NotSupportedEnumerationValue", new object[] { type.Name, value.ToString(CultureInfo.InvariantCulture) }), type.Name);
        }

        internal static ArgumentOutOfRangeException NotSupportedIsolationLevel(System.Data.IsolationLevel value)
        {
            return NotSupportedEnumerationValue(typeof(System.Data.IsolationLevel), (int) value);
        }

        internal static Exception NullEmptyTransactionName()
        {
            return ADP.Argument(Res.GetString("SQL_NullEmptyTransactionName"));
        }

        internal static Exception NullSchemaTableDataTypeNotSupported(string columnName)
        {
            return ADP.Argument(Res.GetString("NullSchemaTableDataTypeNotSupported", new object[] { columnName }));
        }

        internal static Exception OperationCancelled()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_OperationCancelled"));
        }

        internal static Exception ParameterInvalidVariant(string paramName)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ParameterInvalidVariant", new object[] { paramName }));
        }

        internal static Exception ParameterSizeRestrictionFailure(int index)
        {
            return ADP.InvalidOperation(Res.GetString("OleDb_CommandParameterError", new object[] { index.ToString(CultureInfo.InvariantCulture), "SqlParameter.Size" }));
        }

        internal static Exception ParsingError()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_ParsingError"));
        }

        internal static Exception PendingBeginXXXExists()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_PendingBeginXXXExists"));
        }

        internal static Exception PrecisionValueOutOfRange(byte precision)
        {
            return ADP.Argument(Res.GetString("SQL_PrecisionValueOutOfRange", new object[] { precision.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static TransactionPromotionException PromotionFailed(Exception inner)
        {
            TransactionPromotionException e = new TransactionPromotionException(Res.GetString("SqlDelegatedTransaction_PromotionFailed"), inner);
            ADP.TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception ROR_FailoverNotSupportedConnString()
        {
            return ADP.Argument(Res.GetString("SQLROR_FailoverNotSupported"));
        }

        internal static Exception ROR_FailoverNotSupportedServer()
        {
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            errorCollection.Add(new SqlError(0, 0, 20, null, Res.GetString("SQLROR_FailoverNotSupported"), "", 0));
            SqlException exception = SqlException.CreateException(errorCollection, null);
            exception._doNotReconnect = true;
            return exception;
        }

        internal static Exception ROR_InvalidRoutingInfo()
        {
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            errorCollection.Add(new SqlError(0, 0, 20, null, Res.GetString("SQLROR_InvalidRoutingInfo"), "", 0));
            SqlException exception = SqlException.CreateException(errorCollection, null);
            exception._doNotReconnect = true;
            return exception;
        }

        internal static Exception ROR_RecursiveRoutingNotSupported()
        {
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            errorCollection.Add(new SqlError(0, 0, 20, null, Res.GetString("SQLROR_RecursiveRoutingNotSupported"), "", 0));
            SqlException exception = SqlException.CreateException(errorCollection, null);
            exception._doNotReconnect = true;
            return exception;
        }

        internal static Exception ROR_TimeoutAfterRoutingInfo()
        {
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            errorCollection.Add(new SqlError(0, 0, 20, null, Res.GetString("SQLROR_TimeoutAfterRoutingInfo"), "", 0));
            SqlException exception = SqlException.CreateException(errorCollection, null);
            exception._doNotReconnect = true;
            return exception;
        }

        internal static Exception ROR_UnexpectedRoutingInfo()
        {
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            errorCollection.Add(new SqlError(0, 0, 20, null, Res.GetString("SQLROR_UnexpectedRoutingInfo"), "", 0));
            SqlException exception = SqlException.CreateException(errorCollection, null);
            exception._doNotReconnect = true;
            return exception;
        }

        internal static Exception ScaleValueOutOfRange(byte scale)
        {
            return ADP.Argument(Res.GetString("SQL_ScaleValueOutOfRange", new object[] { scale.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception SingleValuedStructNotSupported()
        {
            return ADP.NotSupported(Res.GetString("MetaType_SingleValuedStructNotSupported"));
        }

        internal static Exception SmallDateTimeOverflow(string datetime)
        {
            return ADP.Overflow(Res.GetString("SQL_SmallDateTimeOverflow", new object[] { datetime }));
        }

        internal static Exception SnapshotNotSupported(System.Data.IsolationLevel level)
        {
            return ADP.Argument(Res.GetString("SQL_SnapshotNotSupported", new object[] { typeof(System.Data.IsolationLevel), level.ToString() }));
        }

        internal static Exception SNIPacketAllocationFailure()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_SNIPacketAllocationFailure"));
        }

        internal static Exception SortOrdinalGreaterThanFieldCount(int columnOrdinal, int sortOrdinal)
        {
            return ADP.InvalidOperation(Res.GetString("SqlProvider_SortOrdinalGreaterThanFieldCount", new object[] { sortOrdinal, columnOrdinal }));
        }

        internal static Exception SqlCommandHasExistingSqlNotificationRequest()
        {
            return ADP.InvalidOperation(Res.GetString("SQLNotify_AlreadyHasCommand"));
        }

        internal static Exception SqlDepCannotBeCreatedInProc()
        {
            return ADP.InvalidOperation(Res.GetString("SqlNotify_SqlDepCannotBeCreatedInProc"));
        }

        internal static Exception SqlDepDefaultOptionsButNoStart()
        {
            return ADP.InvalidOperation(Res.GetString("SqlDependency_DefaultOptionsButNoStart"));
        }

        internal static Exception SqlDependencyDatabaseBrokerDisabled()
        {
            return ADP.InvalidOperation(Res.GetString("SqlDependency_DatabaseBrokerDisabled"));
        }

        internal static Exception SqlDependencyDuplicateStart()
        {
            return ADP.InvalidOperation(Res.GetString("SqlDependency_DuplicateStart"));
        }

        internal static Exception SqlDependencyEventNoDuplicate()
        {
            return ADP.InvalidOperation(Res.GetString("SqlDependency_EventNoDuplicate"));
        }

        internal static Exception SqlDependencyIdMismatch()
        {
            return ADP.InvalidOperation(Res.GetString("SqlDependency_IdMismatch"));
        }

        internal static Exception SqlDependencyNoMatchingServerDatabaseStart()
        {
            return ADP.InvalidOperation(Res.GetString("SqlDependency_NoMatchingServerDatabaseStart"));
        }

        internal static Exception SqlDependencyNoMatchingServerStart()
        {
            return ADP.InvalidOperation(Res.GetString("SqlDependency_NoMatchingServerStart"));
        }

        internal static Exception SqlMetaDataNoMetaData()
        {
            return ADP.InvalidOperation(Res.GetString("SqlMetaData_NoMetadata"));
        }

        internal static Exception SqlNotificationException(SqlNotificationEventArgs notify)
        {
            return ADP.InvalidOperation(Res.GetString("SQLNotify_ErrorFormat", new object[] { notify.Type, notify.Info, notify.Source }));
        }

        internal static SqlNullValueException SqlNullValue()
        {
            SqlNullValueException e = new SqlNullValueException();
            ADP.TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception SqlPipeAlreadyHasAnOpenResultSet(string methodName)
        {
            return ADP.InvalidOperation(Res.GetString("SqlPipe_AlreadyHasAnOpenResultSet", new object[] { methodName }));
        }

        internal static Exception SqlPipeCommandHookedUpToNonContextConnection()
        {
            return ADP.InvalidOperation(Res.GetString("SqlPipe_CommandHookedUpToNonContextConnection"));
        }

        internal static Exception SqlPipeDoesNotHaveAnOpenResultSet(string methodName)
        {
            return ADP.InvalidOperation(Res.GetString("SqlPipe_DoesNotHaveAnOpenResultSet", new object[] { methodName }));
        }

        internal static Exception SqlPipeErrorRequiresSendEnd()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_PipeErrorRequiresSendEnd"));
        }

        internal static Exception SqlPipeIsBusy()
        {
            return ADP.InvalidOperation(Res.GetString("SqlPipe_IsBusy"));
        }

        internal static Exception SqlPipeMessageTooLong(int messageLength)
        {
            return ADP.Argument(Res.GetString("SqlPipe_MessageTooLong", new object[] { messageLength }));
        }

        internal static Exception SqlRecordReadOnly(string methodname)
        {
            if (methodname == null)
            {
                return ADP.InvalidOperation(Res.GetString("SQL_SqlRecordReadOnly2"));
            }
            return ADP.InvalidOperation(Res.GetString("SQL_SqlRecordReadOnly", new object[] { methodname }));
        }

        internal static Exception SqlResultSetClosed(string methodname)
        {
            if (methodname == null)
            {
                return ADP.InvalidOperation(Res.GetString("SQL_SqlResultSetClosed2"));
            }
            return ADP.InvalidOperation(Res.GetString("SQL_SqlResultSetClosed", new object[] { methodname }));
        }

        internal static Exception SqlResultSetCommandNotInSameConnection()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_SqlResultSetCommandNotInSameConnection"));
        }

        internal static Exception SqlResultSetNoAcceptableCursor()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_SqlResultSetNoAcceptableCursor"));
        }

        internal static Exception SqlResultSetNoData(string methodname)
        {
            return ADP.InvalidOperation(Res.GetString("ADP_DataReaderNoData", new object[] { methodname }));
        }

        internal static Exception SqlResultSetRowDeleted(string methodname)
        {
            if (methodname == null)
            {
                return ADP.InvalidOperation(Res.GetString("SQL_SqlResultSetRowDeleted2"));
            }
            return ADP.InvalidOperation(Res.GetString("SQL_SqlResultSetRowDeleted", new object[] { methodname }));
        }

        internal static Exception StreamReadNotSupported()
        {
            return ADP.NotSupported(Res.GetString("SQL_StreamReadNotSupported"));
        }

        internal static Exception StreamSeekNotSupported()
        {
            return ADP.NotSupported(Res.GetString("SQL_StreamSeekNotSupported"));
        }

        internal static Exception StreamWriteNotSupported()
        {
            return ADP.NotSupported(Res.GetString("SQL_StreamWriteNotSupported"));
        }

        internal static Exception SubclassMustOverride()
        {
            return ADP.InvalidOperation(Res.GetString("SqlMisc_SubclassMustOverride"));
        }

        internal static Exception TableTypeCanOnlyBeParameter()
        {
            return ADP.Argument(Res.GetString("SQLTVP_TableTypeCanOnlyBeParameter"));
        }

        internal static Exception TimeOverflow(string time)
        {
            return ADP.Overflow(Res.GetString("SQL_TimeOverflow", new object[] { time }));
        }

        internal static Exception TimeScaleValueOutOfRange(byte scale)
        {
            return ADP.Argument(Res.GetString("SQL_TimeScaleValueOutOfRange", new object[] { scale.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception TooManyValues(string arg)
        {
            return ADP.Argument(Res.GetString("SQL_TooManyValues"), arg);
        }

        internal static Exception UDTInvalidSqlType(string typeName)
        {
            return ADP.Argument(Res.GetString("SQLUDT_InvalidSqlType", new object[] { typeName }));
        }

        internal static Exception UDTUnexpectedResult(string exceptionText)
        {
            return ADP.TypeLoad(Res.GetString("SQLUDT_Unexpected", new object[] { exceptionText }));
        }

        internal static Exception UnexpectedSmiEvent(SmiEventSink_Default.UnexpectedEventType eventType)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_UnexpectedSmiEvent", new object[] { (int) eventType }));
        }

        internal static Exception UnexpectedTypeNameForNonStructParams(string paramName)
        {
            return ADP.NotSupported(Res.GetString("SqlParameter_UnexpectedTypeNameForNonStruct", new object[] { paramName }));
        }

        internal static Exception UnexpectedUdtTypeNameForNonUdtParams()
        {
            return ADP.Argument(Res.GetString("SQLUDT_UnexpectedUdtTypeName"));
        }

        internal static Exception UnknownSysTxIsolationLevel(System.Transactions.IsolationLevel isolationLevel)
        {
            return ADP.InvalidOperation(Res.GetString("SQL_UnknownSysTxIsolationLevel", new object[] { isolationLevel.ToString() }));
        }

        internal static Exception UnsupportedColumnTypeForSqlProvider(string columnName, string typeName)
        {
            return ADP.Argument(Res.GetString("SqlProvider_InvalidDataColumnType", new object[] { columnName, typeName }));
        }

        internal static Exception UnsupportedTVPOutputParameter(ParameterDirection direction, string paramName)
        {
            return ADP.NotSupported(Res.GetString("SqlParameter_UnsupportedTVPOutputParameter", new object[] { direction.ToString(), paramName }));
        }

        internal static Exception UserInstanceFailoverNotCompatible()
        {
            return ADP.Argument(Res.GetString("SQL_UserInstanceFailoverNotCompatible"));
        }

        internal static Exception UserInstanceNotAvailableInProc()
        {
            return ADP.InvalidOperation(Res.GetString("SQL_UserInstanceNotAvailableInProc"));
        }
    }
}


namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Data;
    using System.Data.OracleClient;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Transactions;

    internal sealed class ADP
    {
        private static readonly Type AccessViolationType = typeof(AccessViolationException);
        internal static readonly Type ArgumentNullExceptionType = typeof(ArgumentNullException);
        internal static readonly int CharSize = 2;
        internal const CompareOptions compareOptions = (CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
        internal const string ConnectionString = "ConnectionString";
        internal static readonly byte[] EmptyByteArray = new byte[0];
        internal static readonly Type FormatExceptionType = typeof(FormatException);
        internal static readonly bool IsPlatformNT5 = (IsWindowsNT && (Environment.OSVersion.Version.Major >= 5));
        internal static readonly bool IsWindowsNT = (PlatformID.Win32NT == Environment.OSVersion.Platform);
        internal static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        private static readonly Type NullReferenceType = typeof(NullReferenceException);
        internal static readonly string NullString = System.Data.OracleClient.Res.GetString("SqlMisc_NullString");
        private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);
        internal static readonly Type OverflowExceptionType = typeof(OverflowException);
        internal const string Parameter = "Parameter";
        internal const string ParameterName = "ParameterName";
        internal static readonly int PtrSize = IntPtr.Size;
        private static readonly Type SecurityType = typeof(SecurityException);
        private static readonly Type StackOverflowType = typeof(StackOverflowException);
        internal static readonly string StrEmpty = "";
        private static readonly Type ThreadAbortType = typeof(ThreadAbortException);

        private ADP()
        {
        }

        internal static Exception AmbigousCollectionName(string collectionName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_AmbigousCollectionName", new object[] { collectionName }));
        }

        internal static ArgumentException Argument(string error)
        {
            ArgumentException e = new ArgumentException(error);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException Argument(string error, Exception inner)
        {
            ArgumentException e = new ArgumentException(error, inner);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException Argument(string error, string parameter)
        {
            ArgumentException e = new ArgumentException(error, parameter);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentNullException ArgumentNull(string parameter)
        {
            ArgumentNullException e = new ArgumentNullException(parameter);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentNullException ArgumentNull(string parameter, string error)
        {
            ArgumentNullException e = new ArgumentNullException(parameter, error);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentOutOfRangeException ArgumentOutOfRange(string argName, string message)
        {
            ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(argName, message);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception BadBindValueType(Type valueType, OracleType oracleType)
        {
            return InvalidCast(System.Data.OracleClient.Res.GetString("ADP_BadBindValueType", new object[] { valueType.ToString(), oracleType.ToString() }));
        }

        internal static Exception BadOracleClientImageFormat(Exception e)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_BadOracleClientImageFormat"), e);
        }

        internal static Exception BadOracleClientVersion()
        {
            return Simple(System.Data.OracleClient.Res.GetString("ADP_BadOracleClientVersion"));
        }

        internal static Exception BufferExceeded(string argName)
        {
            return ArgumentOutOfRange(argName, System.Data.OracleClient.Res.GetString("ADP_BufferExceeded"));
        }

        internal static Exception CannotDeriveOverloaded()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_CannotDeriveOverloaded"));
        }

        internal static Exception CannotOpenLobWithDifferentMode(OracleLobOpenMode newmode, OracleLobOpenMode current)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_CannotOpenLobWithDifferentMode", new object[] { newmode.ToString(), current.ToString() }));
        }

        internal static Exception ChangeDatabaseNotSupported()
        {
            return NotSupported(System.Data.OracleClient.Res.GetString("ADP_ChangeDatabaseNotSupported"));
        }

        internal static void CheckArgumentLength(string value, string parameterName)
        {
            CheckArgumentNull(value, parameterName);
            if (value.Length == 0)
            {
                throw Argument(System.Data.OracleClient.Res.GetString("ADP_EmptyString", new object[] { parameterName }));
            }
        }

        public static void CheckArgumentNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw ArgumentNull(parameterName);
            }
        }

        internal static Exception ClosedConnectionError()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_ClosedConnectionError"));
        }

        internal static Exception ClosedDataReaderError()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_ClosedDataReaderError"));
        }

        internal static Exception CollectionIndexInt32(int index, Type collection, int count)
        {
            return IndexOutOfRange(System.Data.OracleClient.Res.GetString("ADP_CollectionIndexInt32", new object[] { index.ToString(CultureInfo.InvariantCulture), collection.Name, count.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception CollectionIndexString(Type itemType, string propertyName, string propertyValue, Type collection)
        {
            return IndexOutOfRange(System.Data.OracleClient.Res.GetString("ADP_CollectionIndexString", new object[] { itemType.Name, propertyName, propertyValue, collection.Name }));
        }

        internal static Exception CollectionInvalidType(Type collection, Type itemType, object invalidValue)
        {
            return InvalidCast(System.Data.OracleClient.Res.GetString("ADP_CollectionInvalidType", new object[] { collection.Name, itemType.Name, invalidValue.GetType().Name }));
        }

        internal static Exception CollectionNameIsNotUnique(string collectionName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_CollectionNameISNotUnique", new object[] { collectionName }));
        }

        internal static Exception CollectionNullValue(string parameter, Type collection, Type itemType)
        {
            return ArgumentNull(parameter, System.Data.OracleClient.Res.GetString("ADP_CollectionNullValue", new object[] { collection.Name, itemType.Name }));
        }

        internal static ArgumentException CollectionRemoveInvalidObject(Type itemType, ICollection collection)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_CollectionRemoveInvalidObject", new object[] { itemType.Name, collection.GetType().Name }));
        }

        internal static Exception CommandTextRequired(string method)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_CommandTextRequired", new object[] { method }));
        }

        internal static bool CompareInsensitiveInvariant(string strvalue, string strconst)
        {
            return (0 == CultureInfo.InvariantCulture.CompareInfo.Compare(strvalue, strconst, CompareOptions.IgnoreCase));
        }

        internal static ConfigurationException ConfigUnableToLoadXmlMetaDataFile(string settingName)
        {
            return Configuration(System.Data.OracleClient.Res.GetString("ADP_ConfigUnableToLoadXmlMetaDataFile", new object[] { settingName }));
        }

        internal static ConfigurationException Configuration(string message)
        {
            ConfigurationException e = new ConfigurationErrorsException(message);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ConfigurationException ConfigWrongNumberOfValues(string settingName)
        {
            return Configuration(System.Data.OracleClient.Res.GetString("ADP_ConfigWrongNumberOfValues", new object[] { settingName }));
        }

        internal static Exception ConnectionAlreadyOpen(ConnectionState state)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_ConnectionAlreadyOpen", new object[] { ConnectionStateMsg(state) }));
        }

        internal static Exception ConnectionRequired(string method)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_ConnectionRequired", new object[] { method }));
        }

        private static string ConnectionStateMsg(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Closed:
                case (ConnectionState.Broken | ConnectionState.Connecting):
                    return System.Data.OracleClient.Res.GetString("ADP_ConnectionStateMsg_Closed");

                case ConnectionState.Open:
                    return System.Data.OracleClient.Res.GetString("ADP_ConnectionStateMsg_Open");

                case ConnectionState.Connecting:
                    return System.Data.OracleClient.Res.GetString("ADP_ConnectionStateMsg_Connecting");

                case (ConnectionState.Executing | ConnectionState.Open):
                    return System.Data.OracleClient.Res.GetString("ADP_ConnectionStateMsg_OpenExecuting");

                case (ConnectionState.Fetching | ConnectionState.Open):
                    return System.Data.OracleClient.Res.GetString("ADP_ConnectionStateMsg_OpenFetching");
            }
            return System.Data.OracleClient.Res.GetString("ADP_ConnectionStateMsg", new object[] { state.ToString() });
        }

        internal static ArgumentException ConnectionStringSyntax(int index)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_ConnectionStringSyntax", new object[] { index }));
        }

        internal static ArgumentException ConvertFailed(Type fromType, Type toType, Exception innerException)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_ConvertFailed", new object[] { fromType.FullName, toType.FullName }), innerException);
        }

        internal static Exception CouldNotCreateEnvironment(string methodname, int rc)
        {
            return Simple(System.Data.OracleClient.Res.GetString("ADP_CouldNotCreateEnvironment", new object[] { methodname, rc.ToString(CultureInfo.CurrentCulture) }));
        }

        internal static Exception DataIsNull()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_DataIsNull"));
        }

        internal static Exception DataReaderClosed(string method)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_DataReaderClosed", new object[] { method }));
        }

        internal static Exception DataReaderNoData()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_DataReaderNoData"));
        }

        internal static Exception DataTableDoesNotExist(string collectionName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_DataTableDoesNotExist", new object[] { collectionName }));
        }

        internal static Exception DeriveParametersNotSupported(IDbCommand value)
        {
            return ProviderException(System.Data.OracleClient.Res.GetString("ADP_DeriveParametersNotSupported", new object[] { value.GetType().Name, value.CommandType.ToString() }));
        }

        internal static Exception DistribTxRequiresOracle9i()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_DistribTxRequiresOracle9i"));
        }

        internal static Exception DistribTxRequiresOracleServicesForMTS(Exception inner)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_DistribTxRequiresOracleServicesForMTS"), inner);
        }

        internal static int DstCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
        }

        internal static Delegate FindBuilder(MulticastDelegate mcd)
        {
            if (mcd != null)
            {
                Delegate[] invocationList = mcd.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    if (invocationList[i].Target is DbCommandBuilder)
                    {
                        return invocationList[i];
                    }
                }
            }
            return null;
        }

        internal static Transaction GetCurrentTransaction()
        {
            return Transaction.Current;
        }

        internal static Stream GetFileStream(string filename)
        {
            Stream stream;
            new FileIOPermission(FileIOPermissionAccess.Read, filename).Assert();
            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return stream;
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        internal static string GetFullPath(string filename)
        {
            return Path.GetFullPath(filename);
        }

        internal static IDtcTransaction GetOletxTransaction(Transaction transaction)
        {
            IDtcTransaction dtcTransaction = null;
            if (null != transaction)
            {
                dtcTransaction = TransactionInterop.GetDtcTransaction(transaction);
            }
            return dtcTransaction;
        }

        internal static Stream GetXmlStream(string value, string errorString)
        {
            Stream fileStream;
            string runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            if (runtimeDirectory == null)
            {
                throw ConfigUnableToLoadXmlMetaDataFile(errorString);
            }
            StringBuilder builder = new StringBuilder((runtimeDirectory.Length + @"config\".Length) + value.Length);
            builder.Append(runtimeDirectory);
            builder.Append(@"config\");
            builder.Append(value);
            string filename = builder.ToString();
            if (GetFullPath(filename) != filename)
            {
                throw ConfigUnableToLoadXmlMetaDataFile(errorString);
            }
            try
            {
                fileStream = GetFileStream(filename);
            }
            catch (Exception exception)
            {
                if (!IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ConfigUnableToLoadXmlMetaDataFile(errorString);
            }
            return fileStream;
        }

        internal static Stream GetXmlStreamFromValues(string[] values, string errorString)
        {
            if (values.Length != 1)
            {
                throw ConfigWrongNumberOfValues(errorString);
            }
            return GetXmlStream(values[0], errorString);
        }

        internal static Exception IdentifierIsNotQuoted()
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_IdentifierIsNotQuoted"));
        }

        internal static Exception IncorrectNumberOfDataSourceInformationRows()
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_IncorrectNumberOfDataSourceInformationRows"));
        }

        internal static Exception IndexOutOfRange(string error)
        {
            return TraceException(new IndexOutOfRangeException(error));
        }

        internal static Exception InputRefCursorNotSupported(string parameterName)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_InputRefCursorNotSupported", new object[] { parameterName }));
        }

        internal static Exception InternalConnectionError(ConnectionError internalError)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_InternalConnectionError", new object[] { (int) internalError }));
        }

        internal static Exception InternalError(InternalErrorCode internalError)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_InternalProviderError", new object[] { (int) internalError }));
        }

        internal static IntPtr IntPtrOffset(IntPtr pbase, int offset)
        {
            if (4 == PtrSize)
            {
                return (IntPtr) (pbase.ToInt32() + offset);
            }
            return (IntPtr) (pbase.ToInt64() + offset);
        }

        internal static Exception InvalidCast()
        {
            return TraceException(new InvalidCastException());
        }

        internal static Exception InvalidCast(string error)
        {
            return TraceException(new InvalidCastException(error));
        }

        internal static Exception InvalidCommandType(CommandType cmdType)
        {
            object[] args = new object[] { ((int) cmdType).ToString(CultureInfo.CurrentCulture) };
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidCommandType", args));
        }

        internal static Exception InvalidConnectionOptionLength(string key, int maxLength)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidConnectionOptionLength", new object[] { key, maxLength }));
        }

        internal static Exception InvalidConnectionOptionValue(string key)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidConnectionOptionValue", new object[] { key }));
        }

        internal static Exception InvalidConnectionOptionValue(string key, Exception inner)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidConnectionOptionValue", new object[] { key }), inner);
        }

        internal static Exception InvalidDataDirectory()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_InvalidDataDirectory"));
        }

        internal static Exception InvalidDataLength(long length)
        {
            return IndexOutOfRange(System.Data.OracleClient.Res.GetString("ADP_InvalidDataLength", new object[] { length.ToString(CultureInfo.CurrentCulture) }));
        }

        internal static Exception InvalidDataRowVersion(DataRowVersion value)
        {
            return InvalidEnumerationValue(typeof(DataRowVersion), (int) value);
        }

        internal static Exception InvalidDataType(TypeCode tc)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidDataType", new object[] { tc.ToString() }));
        }

        internal static Exception InvalidDataTypeForValue(Type dataType, TypeCode tc)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidDataTypeForValue", new object[] { dataType.ToString(), tc.ToString() }));
        }

        internal static Exception InvalidDbType(DbType dbType)
        {
            return ArgumentOutOfRange("dbType", System.Data.OracleClient.Res.GetString("ADP_InvalidDbType", new object[] { dbType.ToString() }));
        }

        internal static Exception InvalidDestinationBufferIndex(int maxLen, int dstOffset, string parameterName)
        {
            return ArgumentOutOfRange(parameterName, System.Data.OracleClient.Res.GetString("ADP_InvalidDestinationBufferIndex", new object[] { maxLen.ToString(CultureInfo.CurrentCulture), dstOffset.ToString(CultureInfo.CurrentCulture) }));
        }

        internal static Exception InvalidEnumerationValue(Type type, int value)
        {
            return ArgumentOutOfRange(System.Data.OracleClient.Res.GetString("ADP_InvalidEnumerationValue", new object[] { type.Name, value.ToString(CultureInfo.InvariantCulture) }), type.Name);
        }

        internal static ArgumentException InvalidKeyname(string parameterName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidKey"), parameterName);
        }

        internal static Exception InvalidKeyRestrictionBehavior(KeyRestrictionBehavior value)
        {
            return InvalidEnumerationValue(typeof(KeyRestrictionBehavior), (int) value);
        }

        internal static Exception InvalidLobType(OracleType oracleType)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_InvalidLobType", new object[] { oracleType.ToString() }));
        }

        internal static Exception InvalidMinMaxPoolSizeValues()
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidMinMaxPoolSizeValues"));
        }

        internal static Exception InvalidOffsetValue(int value)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidOffsetValue", new object[] { value.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception InvalidOperation(string error)
        {
            return TraceException(new InvalidOperationException(error));
        }

        internal static Exception InvalidOperation(string error, Exception inner)
        {
            return TraceException(new InvalidOperationException(error, inner));
        }

        internal static Exception InvalidOracleType(OracleType oracleType)
        {
            return ArgumentOutOfRange("oracleType", System.Data.OracleClient.Res.GetString("ADP_InvalidOracleType", new object[] { oracleType.ToString() }));
        }

        internal static Exception InvalidParameterDirection(ParameterDirection value)
        {
            return InvalidEnumerationValue(typeof(ParameterDirection), (int) value);
        }

        internal static Exception InvalidParameterType(IDataParameterCollection collection, Type parameterType, object invalidValue)
        {
            return CollectionInvalidType(collection.GetType(), parameterType, invalidValue);
        }

        internal static Exception InvalidPermissionState(PermissionState value)
        {
            return InvalidEnumerationValue(typeof(PermissionState), (int) value);
        }

        internal static Exception InvalidSeekOrigin(SeekOrigin origin)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidSeekOrigin", new object[] { origin.ToString() }));
        }

        internal static Exception InvalidSizeValue(int value)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidSizeValue", new object[] { value.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception InvalidSourceBufferIndex(int maxLen, long srcOffset, string parameterName)
        {
            return ArgumentOutOfRange(parameterName, System.Data.OracleClient.Res.GetString("ADP_InvalidSourceBufferIndex", new object[] { maxLen.ToString(CultureInfo.CurrentCulture), srcOffset.ToString(CultureInfo.CurrentCulture) }));
        }

        internal static Exception InvalidSourceOffset(string argName, long minValue, long maxValue)
        {
            return ArgumentOutOfRange(argName, System.Data.OracleClient.Res.GetString("ADP_InvalidSourceOffset", new object[] { minValue.ToString(CultureInfo.CurrentCulture), maxValue.ToString(CultureInfo.CurrentCulture) }));
        }

        internal static Exception InvalidUpdateRowSource(UpdateRowSource value)
        {
            return InvalidEnumerationValue(typeof(UpdateRowSource), (int) value);
        }

        internal static ArgumentException InvalidValue(string parameterName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidValue"), parameterName);
        }

        internal static Exception InvalidXml()
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_InvalidXml"));
        }

        internal static Exception InvalidXMLBadVersion()
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_InvalidXMLBadVersion"));
        }

        internal static Exception InvalidXmlInvalidValue(string collectionName, string columnName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_InvalidXmlInvalidValue", new object[] { collectionName, columnName }));
        }

        internal static Exception InvalidXmlMissingColumn(string collectionName, string columnName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_InvalidXmlMissingColumn", new object[] { collectionName, columnName }));
        }

        internal static bool IsCatchableExceptionType(Exception e)
        {
            Type c = e.GetType();
            return (((((c != StackOverflowType) && (c != OutOfMemoryType)) && ((c != ThreadAbortType) && (c != NullReferenceType))) && (c != AccessViolationType)) && !SecurityType.IsAssignableFrom(c));
        }

        internal static bool IsDirection(IDataParameter value, ParameterDirection condition)
        {
            return (condition == (condition & value.Direction));
        }

        internal static bool IsDirection(ParameterDirection value, ParameterDirection condition)
        {
            return (condition == (condition & value));
        }

        internal static bool IsEmpty(string str)
        {
            if (str != null)
            {
                return (0 == str.Length);
            }
            return true;
        }

        internal static bool IsEmptyArray(string[] array)
        {
            if (array != null)
            {
                return (0 == array.Length);
            }
            return true;
        }

        internal static bool IsNull(object value)
        {
            if ((value == null) || (DBNull.Value == value))
            {
                return true;
            }
            INullable nullable = value as INullable;
            return ((nullable != null) && nullable.IsNull);
        }

        internal static ArgumentException KeywordNotSupported(string keyword)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_KeywordNotSupported", new object[] { keyword }));
        }

        internal static Exception LobAmountExceeded(string argName)
        {
            return ArgumentOutOfRange(argName, System.Data.OracleClient.Res.GetString("ADP_LobAmountExceeded"));
        }

        internal static Exception LobAmountMustBeEven(string argName)
        {
            return ArgumentOutOfRange(argName, System.Data.OracleClient.Res.GetString("ADP_LobAmountMustBeEven"));
        }

        internal static Exception LobPositionMustBeEven()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_LobPositionMustBeEven"));
        }

        internal static Exception LobWriteInvalidOnNull()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_LobWriteInvalidOnNull"));
        }

        internal static Exception LobWriteRequiresTransaction()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_LobWriteRequiresTransaction"));
        }

        internal static Exception MethodNotImplemented(string methodName)
        {
            NotImplementedException e = new NotImplementedException(methodName);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception MissingDataSourceInformationColumn()
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_MissingDataSourceInformationColumn"));
        }

        internal static Exception MissingRestrictionColumn()
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_MissingRestrictionColumn"));
        }

        internal static Exception MissingRestrictionRow()
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_MissingRestrictionRow"));
        }

        internal static Exception MonthOutOfRange()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_MonthOutOfRange"));
        }

        internal static Exception MustBePositive(string argName)
        {
            return ArgumentOutOfRange(argName, System.Data.OracleClient.Res.GetString("ADP_MustBePositive"));
        }

        internal static Exception NoColumns()
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_NoColumns"));
        }

        internal static Exception NoCommandText()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_NoCommandText"));
        }

        internal static Exception NoConnectionString()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_NoConnectionString"));
        }

        internal static Exception NoData()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_NoData"));
        }

        internal static Exception NoLocalTransactionInDistributedContext()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_NoLocalTransactionInDistributedContext"));
        }

        internal static Exception NoOptimizedDirectTableAccess()
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_NoOptimizedDirectTableAccess"));
        }

        internal static Exception NoParallelTransactions()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_NoParallelTransactions"));
        }

        internal static Exception NotAPermissionElement()
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_NotAPermissionElement"));
        }

        internal static Exception NotSupported()
        {
            return TraceException(new NotSupportedException());
        }

        internal static Exception NotSupported(string message)
        {
            return TraceException(new NotSupportedException(message));
        }

        internal static Exception ObjectDisposed(string name)
        {
            return TraceException(new ObjectDisposedException(name));
        }

        internal static Exception OpenConnectionPropertySet(string property, ConnectionState state)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_OpenConnectionPropertySet", new object[] { property, ConnectionStateMsg(state) }));
        }

        internal static Exception OpenConnectionRequired(string method, ConnectionState state)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_OpenConnectionRequired", new object[] { method, "ConnectionState", state.ToString() }));
        }

        internal static Exception OperationFailed(string method, int rc)
        {
            return Simple(System.Data.OracleClient.Res.GetString("ADP_OperationFailed", new object[] { method, rc }));
        }

        internal static Exception OperationResultedInOverflow()
        {
            return Overflow(System.Data.OracleClient.Res.GetString("ADP_OperationResultedInOverflow"));
        }

        internal static Exception OracleError(OciErrorHandle errorHandle, int rc)
        {
            return TraceException(OracleException.CreateException(errorHandle, rc));
        }

        internal static Exception OracleError(int rc, OracleInternalConnection internalConnection)
        {
            return TraceException(OracleException.CreateException(rc, internalConnection));
        }

        internal static Exception Overflow(string error)
        {
            return TraceException(new OverflowException(error));
        }

        internal static Exception ParameterConversionFailed(object value, Type destType, Exception inner)
        {
            Exception exception;
            string message = System.Data.OracleClient.Res.GetString("ADP_ParameterConversionFailed", new object[] { value.GetType().Name, destType.Name });
            if (inner is ArgumentException)
            {
                exception = new ArgumentException(message, inner);
            }
            else if (inner is FormatException)
            {
                exception = new FormatException(message, inner);
            }
            else if (inner is InvalidCastException)
            {
                exception = new InvalidCastException(message, inner);
            }
            else if (inner is OverflowException)
            {
                exception = new OverflowException(message, inner);
            }
            else
            {
                exception = inner;
            }
            TraceExceptionAsReturnValue(exception);
            return exception;
        }

        internal static Exception ParameterNull(string parameter, IDataParameterCollection collection, Type parameterType)
        {
            return CollectionNullValue(parameter, collection.GetType(), parameterType);
        }

        internal static Exception ParametersIsNotParent(Type parameterType, IDataParameterCollection collection)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_CollectionIsNotParent", new object[] { parameterType.Name, collection.GetType().Name }));
        }

        internal static ArgumentException ParametersIsParent(Type parameterType, IDataParameterCollection collection)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_CollectionIsNotParent", new object[] { parameterType.Name, collection.GetType().Name }));
        }

        internal static Exception ParameterSizeIsMissing(string parameterName, Type dataType)
        {
            return Simple(System.Data.OracleClient.Res.GetString("ADP_ParameterSizeIsMissing", new object[] { parameterName, dataType.Name }));
        }

        internal static Exception ParameterSizeIsTooLarge(string parameterName)
        {
            return Simple(System.Data.OracleClient.Res.GetString("ADP_ParameterSizeIsTooLarge", new object[] { parameterName }));
        }

        internal static Exception ParametersMappingIndex(int index, IDataParameterCollection collection)
        {
            return CollectionIndexInt32(index, collection.GetType(), collection.Count);
        }

        internal static Exception ParametersSourceIndex(string parameterName, IDataParameterCollection collection, Type parameterType)
        {
            return CollectionIndexString(parameterType, "ParameterName", parameterName, collection.GetType());
        }

        internal static Exception PermissionTypeMismatch()
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_PermissionTypeMismatch"));
        }

        internal static Exception PooledOpenTimeout()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_PooledOpenTimeout"));
        }

        internal static Exception ProviderException(string error)
        {
            return InvalidOperation(error);
        }

        internal static Exception QueryFailed(string collectionName, Exception e)
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("MDF_QueryFailed", new object[] { collectionName }), e);
        }

        internal static Exception ReadOnlyLob()
        {
            return NotSupported(System.Data.OracleClient.Res.GetString("ADP_ReadOnlyLob"));
        }

        internal static Exception SeekBeyondEnd(string parameter)
        {
            return ArgumentOutOfRange(parameter, System.Data.OracleClient.Res.GetString("ADP_SeekBeyondEnd"));
        }

        internal static Exception Simple(string message)
        {
            return TraceException(new Exception(message));
        }

        internal static int SrcCompare(string strA, string strB)
        {
            if (!(strA == strB))
            {
                return 1;
            }
            return 0;
        }

        internal static Exception SyntaxErrorExpectedCommaAfterColumn()
        {
            return TraceException(InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_SyntaxErrorExpectedCommaAfterColumn")));
        }

        internal static Exception SyntaxErrorExpectedCommaAfterTable()
        {
            return TraceException(InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_SyntaxErrorExpectedCommaAfterTable")));
        }

        internal static Exception SyntaxErrorExpectedIdentifier()
        {
            return TraceException(InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_SyntaxErrorExpectedIdentifier")));
        }

        internal static Exception SyntaxErrorExpectedNextPart()
        {
            return TraceException(InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_SyntaxErrorExpectedNextPart")));
        }

        internal static Exception SyntaxErrorMissingParenthesis()
        {
            return TraceException(InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_SyntaxErrorMissingParenthesis")));
        }

        internal static Exception SyntaxErrorTooManyNameParts()
        {
            return TraceException(InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_SyntaxErrorTooManyNameParts")));
        }

        internal static Exception TooManyRestrictions(string collectionName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_TooManyRestrictions", new object[] { collectionName }));
        }

        internal static Exception TraceException(Exception e)
        {
            TraceExceptionAsReturnValue(e);
            return e;
        }

        private static void TraceException(string trace, Exception e)
        {
            if (e != null)
            {
                Bid.Trace(trace, e.ToString());
            }
        }

        internal static void TraceExceptionAsReturnValue(Exception e)
        {
            TraceException("<oc|ERR|THROW> '%ls'\n", e);
        }

        internal static void TraceExceptionForCapture(Exception e)
        {
            TraceException("<comm.ADP.TraceException|ERR|CATCH> '%ls'\n", e);
        }

        internal static void TraceExceptionWithoutRethrow(Exception e)
        {
            TraceException("<oc|ERR|CATCH> '%ls'\n", e);
        }

        internal static Exception TransactionCompleted()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_TransactionCompleted"));
        }

        internal static Exception TransactionConnectionMismatch()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_TransactionConnectionMismatch"));
        }

        internal static Exception TransactionPresent()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_TransactionPresent"));
        }

        internal static Exception TransactionRequired()
        {
            return InvalidOperation(System.Data.OracleClient.Res.GetString("ADP_TransactionRequired_Execute"));
        }

        internal static Exception TypeNotSupported(OCI.DATATYPE ociType)
        {
            return NotSupported(System.Data.OracleClient.Res.GetString("ADP_TypeNotSupported", new object[] { ociType.ToString() }));
        }

        internal static Exception UndefinedCollection(string collectionName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_UndefinedCollection", new object[] { collectionName }));
        }

        internal static Exception UndefinedPopulationMechanism(string populationMechanism)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_UndefinedPopulationMechanism", new object[] { populationMechanism }));
        }

        internal static Exception UnknownDataTypeCode(Type dataType, TypeCode tc)
        {
            return Simple(System.Data.OracleClient.Res.GetString("ADP_UnknownDataTypeCode", new object[] { dataType.ToString(), tc.ToString() }));
        }

        internal static Exception UnsupportedIsolationLevel()
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_UnsupportedIsolationLevel"));
        }

        internal static Exception UnsupportedOracleDateTimeBinding(OracleType dtType)
        {
            return ArgumentOutOfRange("", System.Data.OracleClient.Res.GetString("ADP_BadBindValueType", new object[] { typeof(OracleDateTime).ToString(), dtType.ToString() }));
        }

        internal static Exception UnsupportedVersion(string collectionName)
        {
            return Argument(System.Data.OracleClient.Res.GetString("MDF_UnsupportedVersion", new object[] { collectionName }));
        }

        internal static Exception WriteByteForBinaryLobsOnly()
        {
            return NotSupported(System.Data.OracleClient.Res.GetString("ADP_WriteByteForBinaryLobsOnly"));
        }

        internal static Exception WrongType(Type got, Type expected)
        {
            return Argument(System.Data.OracleClient.Res.GetString("ADP_WrongType", new object[] { got.ToString(), expected.ToString() }));
        }

        internal enum ConnectionError
        {
            BeginGetConnectionReturnsNull,
            GetConnectionReturnsNull,
            ConnectionOptionsMissing,
            CouldNotSwitchToClosedPreviouslyOpenedState
        }

        internal enum InternalErrorCode
        {
            AttemptingToConstructReferenceCollectionOnStaticObject = 12,
            AttemptingToEnlistTwice = 13,
            AttemptingToPoolOnRestrictedToken = 8,
            ConvertSidToStringSidWReturnedNull = 10,
            CreateObjectReturnedNull = 5,
            CreateReferenceCollectionReturnedNull = 14,
            InvalidBuffer = 30,
            InvalidLongBuffer = 0x1f,
            InvalidNumberOfRows = 0x20,
            InvalidParserState1 = 0x15,
            InvalidParserState2 = 0x16,
            NameValuePairNext = 20,
            NewObjectCannotBePooled = 6,
            NonPooledObjectUsedMoreThanOnce = 7,
            PooledObjectHasOwner = 3,
            PooledObjectInPoolMoreThanOnce = 4,
            PooledObjectWithoutPool = 15,
            PushingObjectSecondTime = 2,
            UnexpectedWaitAnyResult = 0x10,
            UnpooledObjectHasOwner = 0,
            UnpooledObjectHasWrongOwner = 1
        }
    }
}


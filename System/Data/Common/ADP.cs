namespace System.Data.Common
{
    using Microsoft.SqlServer.Server;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.OleDb;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Transactions;
    using System.Xml;

    internal static class ADP
    {
        private static readonly Type AccessViolationType = typeof(AccessViolationException);
        internal const string Append = "Append";
        internal const string BeginExecuteNonQuery = "BeginExecuteNonQuery";
        internal const string BeginExecuteReader = "BeginExecuteReader";
        internal const string BeginExecuteXmlReader = "BeginExecuteXmlReader";
        internal const string BeginTransaction = "BeginTransaction";
        internal const string Cancel = "Cancel";
        internal const string ChangeDatabase = "ChangeDatabase";
        internal static readonly int CharSize = 2;
        internal const string Clone = "Clone";
        internal const string CommandTimeout = "CommandTimeout";
        internal const string CommitTransaction = "CommitTransaction";
        internal const CompareOptions compareOptions = (CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
        internal const string ConnectionString = "ConnectionString";
        internal const string DataSetColumn = "DataSetColumn";
        internal const string DataSetTable = "DataSetTable";
        internal const int DecimalMaxPrecision = 0x1d;
        internal const int DecimalMaxPrecision28 = 0x1c;
        internal const int DefaultCommandTimeout = 30;
        internal const int DefaultConnectionTimeout = 15;
        internal const string Delete = "Delete";
        internal const string DeleteCommand = "DeleteCommand";
        internal const string DeriveParameters = "DeriveParameters";
        internal const string EndExecuteNonQuery = "EndExecuteNonQuery";
        internal const string EndExecuteReader = "EndExecuteReader";
        internal const string EndExecuteXmlReader = "EndExecuteXmlReader";
        internal const string ExecuteNonQuery = "ExecuteNonQuery";
        internal const string ExecuteReader = "ExecuteReader";
        internal const string ExecuteRow = "ExecuteRow";
        internal const string ExecuteScalar = "ExecuteScalar";
        internal const string ExecuteSqlScalar = "ExecuteSqlScalar";
        internal const string ExecuteXmlReader = "ExecuteXmlReader";
        internal const float FailoverTimeoutStep = 0.08f;
        internal const string Fill = "Fill";
        internal const string FillPage = "FillPage";
        internal const string FillSchema = "FillSchema";
        internal const string GetBytes = "GetBytes";
        internal const string GetChars = "GetChars";
        internal const string GetOleDbSchemaTable = "GetOleDbSchemaTable";
        internal const string GetProperties = "GetProperties";
        internal const string GetSchema = "GetSchema";
        internal const string GetSchemaTable = "GetSchemaTable";
        internal const string GetServerTransactionLevel = "GetServerTransactionLevel";
        private static readonly string hexDigits = "0123456789abcdef";
        internal const string Insert = "Insert";
        internal static readonly IntPtr InvalidPtr = new IntPtr(-1);
        internal static readonly bool IsPlatformNT5 = (IsWindowsNT && (Environment.OSVersion.Version.Major >= 5));
        internal static readonly bool IsWindowsNT = (PlatformID.Win32NT == Environment.OSVersion.Platform);
        internal static readonly HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        private static readonly Type NullReferenceType = typeof(NullReferenceException);
        internal const string Open = "Open";
        private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);
        internal const string Parameter = "Parameter";
        internal const string ParameterBuffer = "buffer";
        internal const string ParameterCount = "count";
        internal const string ParameterDestinationType = "destinationType";
        internal const string ParameterName = "ParameterName";
        internal const string ParameterOffset = "offset";
        internal const string ParameterService = "Service";
        internal const string ParameterSetPosition = "set_Position";
        internal const string ParameterTimeout = "Timeout";
        internal const string ParameterUserData = "UserData";
        internal const string Prepare = "Prepare";
        internal static readonly int PtrSize = IntPtr.Size;
        internal static readonly IntPtr PtrZero = new IntPtr(0);
        internal const string QuoteIdentifier = "QuoteIdentifier";
        internal const string Read = "Read";
        internal static readonly IntPtr RecordsUnaffected = new IntPtr(-1);
        internal const string Remove = "Remove";
        internal const string RollbackTransaction = "RollbackTransaction";
        internal const string SaveTransaction = "SaveTransaction";
        private static readonly Type SecurityType = typeof(SecurityException);
        internal const string SetProperties = "SetProperties";
        internal const string SourceColumn = "SourceColumn";
        internal const string SourceTable = "SourceTable";
        internal const string SourceVersion = "SourceVersion";
        private static readonly Type StackOverflowType = typeof(StackOverflowException);
        internal static readonly string StrEmpty = "";
        private static readonly Type ThreadAbortType = typeof(ThreadAbortException);
        internal const string UnquoteIdentifier = "UnquoteIdentifier";
        internal const string Update = "Update";
        internal const string UpdateCommand = "UpdateCommand";
        internal const string UpdateRows = "UpdateRows";

        internal static Exception AmbigousCollectionName(string collectionName)
        {
            return Argument(System.Data.Res.GetString("MDF_AmbigousCollectionName", new object[] { collectionName }));
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

        internal static ArgumentException Argument(string error, string parameter, Exception inner)
        {
            ArgumentException e = new ArgumentException(error, parameter, inner);
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

        internal static ArgumentOutOfRangeException ArgumentOutOfRange(string parameterName)
        {
            ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(parameterName);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentOutOfRangeException ArgumentOutOfRange(string message, string parameterName)
        {
            ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(parameterName, message);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentOutOfRangeException ArgumentOutOfRange(string message, string parameterName, object value)
        {
            ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(parameterName, value, message);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException BadParameterName(string parameterName)
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_BadParameterName", new object[] { parameterName }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static string BuildQuotedString(string quotePrefix, string quoteSuffix, string unQuotedString)
        {
            StringBuilder builder = new StringBuilder();
            if (!IsEmpty(quotePrefix))
            {
                builder.Append(quotePrefix);
            }
            if (!IsEmpty(quoteSuffix))
            {
                builder.Append(unQuotedString.Replace(quoteSuffix, quoteSuffix + quoteSuffix));
                builder.Append(quoteSuffix);
            }
            else
            {
                builder.Append(unQuotedString);
            }
            return builder.ToString();
        }

        internal static void BuildSchemaTableInfoTableNames(string[] columnNameArray)
        {
            Dictionary<string, int> hash = new Dictionary<string, int>(columnNameArray.Length);
            int length = columnNameArray.Length;
            for (int i = columnNameArray.Length - 1; 0 <= i; i--)
            {
                string key = columnNameArray[i];
                if ((key != null) && (0 < key.Length))
                {
                    int num5;
                    key = key.ToLower(CultureInfo.InvariantCulture);
                    if (hash.TryGetValue(key, out num5))
                    {
                        length = Math.Min(length, num5);
                    }
                    hash[key] = i;
                }
                else
                {
                    columnNameArray[i] = StrEmpty;
                    length = i;
                }
            }
            int uniqueIndex = 1;
            for (int j = length; j < columnNameArray.Length; j++)
            {
                string str2 = columnNameArray[j];
                if (str2.Length == 0)
                {
                    columnNameArray[j] = "Column";
                    uniqueIndex = GenerateUniqueName(hash, ref columnNameArray[j], j, uniqueIndex);
                }
                else
                {
                    str2 = str2.ToLower(CultureInfo.InvariantCulture);
                    if (j != hash[str2])
                    {
                        GenerateUniqueName(hash, ref columnNameArray[j], j, 1);
                    }
                }
            }
        }

        internal static byte[] ByteArrayFromString(string hexString, string dataTypeName)
        {
            if ((hexString.Length & 1) != 0)
            {
                throw LiteralValueIsInvalid(dataTypeName);
            }
            char[] chArray = hexString.ToCharArray();
            byte[] buffer = new byte[hexString.Length / 2];
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            for (int i = 0; i < hexString.Length; i += 2)
            {
                int index = hexDigits.IndexOf(char.ToLower(chArray[i], invariantCulture));
                int num2 = hexDigits.IndexOf(char.ToLower(chArray[i + 1], invariantCulture));
                if ((index < 0) || (num2 < 0))
                {
                    throw LiteralValueIsInvalid(dataTypeName);
                }
                buffer[i / 2] = (byte) ((index << 4) | num2);
            }
            return buffer;
        }

        internal static void CheckArgumentLength(Array value, string parameterName)
        {
            CheckArgumentNull(value, parameterName);
            if (value.Length == 0)
            {
                throw Argument(System.Data.Res.GetString("ADP_EmptyArray", new object[] { parameterName }));
            }
        }

        internal static void CheckArgumentLength(string value, string parameterName)
        {
            CheckArgumentNull(value, parameterName);
            if (value.Length == 0)
            {
                throw Argument(System.Data.Res.GetString("ADP_EmptyString", new object[] { parameterName }));
            }
        }

        internal static void CheckArgumentNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw ArgumentNull(parameterName);
            }
        }

        internal static void CheckVersionMDAC(bool ifodbcelseoledb)
        {
            string fileVersion;
            int fileMinorPart;
            int fileMajorPart;
            int fileBuildPart;
            try
            {
                fileVersion = (string) LocalMachineRegistryValue(@"Software\Microsoft\DataAccess", "FullInstallVer");
                if (IsEmpty(fileVersion))
                {
                    string filename = (string) ClassesRootRegistryValue(@"CLSID\{2206CDB2-19C1-11D1-89E0-00C04FD7A829}\InprocServer32", StrEmpty);
                    FileVersionInfo versionInfo = GetVersionInfo(filename);
                    fileMajorPart = versionInfo.FileMajorPart;
                    fileMinorPart = versionInfo.FileMinorPart;
                    fileBuildPart = versionInfo.FileBuildPart;
                    fileVersion = versionInfo.FileVersion;
                }
                else
                {
                    string[] strArray = fileVersion.Split(new char[] { '.' });
                    fileMajorPart = int.Parse(strArray[0], NumberStyles.None, CultureInfo.InvariantCulture);
                    fileMinorPart = int.Parse(strArray[1], NumberStyles.None, CultureInfo.InvariantCulture);
                    fileBuildPart = int.Parse(strArray[2], NumberStyles.None, CultureInfo.InvariantCulture);
                    int.Parse(strArray[3], NumberStyles.None, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exception)
            {
                if (!IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ODB.MDACNotAvailable(exception);
            }
            if ((fileMajorPart < 2) || ((fileMajorPart == 2) && ((fileMinorPart < 60) || ((fileMinorPart == 60) && (fileBuildPart < 0x197e)))))
            {
                if (ifodbcelseoledb)
                {
                    throw DataAdapter(System.Data.Res.GetString("Odbc_MDACWrongVersion", new object[] { fileVersion }));
                }
                throw DataAdapter(System.Data.Res.GetString("OleDb_MDACWrongVersion", new object[] { fileVersion }));
            }
        }

        internal static object ClassesRootRegistryValue(string subkey, string queryvalue)
        {
            object obj2;
            new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_CLASSES_ROOT\" + subkey).Assert();
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(subkey, false))
                {
                    return ((key != null) ? key.GetValue(queryvalue) : null);
                }
            }
            catch (SecurityException exception)
            {
                TraceExceptionWithoutRethrow(exception);
                return null;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return obj2;
        }

        internal static Exception ClosedConnectionError()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_ClosedConnectionError"));
        }

        internal static IndexOutOfRangeException CollectionIndexInt32(int index, Type collection, int count)
        {
            return IndexOutOfRange(System.Data.Res.GetString("ADP_CollectionIndexInt32", new object[] { index.ToString(CultureInfo.InvariantCulture), collection.Name, count.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static IndexOutOfRangeException CollectionIndexString(Type itemType, string propertyName, string propertyValue, Type collection)
        {
            return IndexOutOfRange(System.Data.Res.GetString("ADP_CollectionIndexString", new object[] { itemType.Name, propertyName, propertyValue, collection.Name }));
        }

        internal static InvalidCastException CollectionInvalidType(Type collection, Type itemType, object invalidValue)
        {
            return InvalidCast(System.Data.Res.GetString("ADP_CollectionInvalidType", new object[] { collection.Name, itemType.Name, invalidValue.GetType().Name }));
        }

        internal static Exception CollectionNameIsNotUnique(string collectionName)
        {
            return Argument(System.Data.Res.GetString("MDF_CollectionNameISNotUnique", new object[] { collectionName }));
        }

        internal static ArgumentNullException CollectionNullValue(string parameter, Type collection, Type itemType)
        {
            return ArgumentNull(parameter, System.Data.Res.GetString("ADP_CollectionNullValue", new object[] { collection.Name, itemType.Name }));
        }

        internal static ArgumentException CollectionRemoveInvalidObject(Type itemType, ICollection collection)
        {
            return Argument(System.Data.Res.GetString("ADP_CollectionRemoveInvalidObject", new object[] { itemType.Name, collection.GetType().Name }));
        }

        internal static Exception CollectionUniqueValue(Type itemType, string propertyName, string propertyValue)
        {
            return Argument(System.Data.Res.GetString("ADP_CollectionUniqueValue", new object[] { itemType.Name, propertyName, propertyValue }));
        }

        internal static Exception ColumnsAddNullAttempt(string parameter)
        {
            return CollectionNullValue(parameter, typeof(DataColumnMappingCollection), typeof(DataColumnMapping));
        }

        internal static InvalidOperationException ColumnSchemaExpression(string srcColumn, string cacheColumn)
        {
            return DataMapping(System.Data.Res.GetString("ADP_ColumnSchemaExpression", new object[] { srcColumn, cacheColumn }));
        }

        internal static InvalidOperationException ColumnSchemaMismatch(string srcColumn, Type srcType, DataColumn column)
        {
            return DataMapping(System.Data.Res.GetString("ADP_ColumnSchemaMismatch", new object[] { srcColumn, srcType.Name, column.ColumnName, column.DataType.Name }));
        }

        internal static InvalidOperationException ColumnSchemaMissing(string cacheColumn, string tableName, string srcColumn)
        {
            if (IsEmpty(tableName))
            {
                return InvalidOperation(System.Data.Res.GetString("ADP_ColumnSchemaMissing1", new object[] { cacheColumn, tableName, srcColumn }));
            }
            return DataMapping(System.Data.Res.GetString("ADP_ColumnSchemaMissing2", new object[] { cacheColumn, tableName, srcColumn }));
        }

        internal static Exception ColumnsDataSetColumn(string cacheColumn)
        {
            return CollectionIndexString(typeof(DataColumnMapping), "DataSetColumn", cacheColumn, typeof(DataColumnMappingCollection));
        }

        internal static Exception ColumnsIndexInt32(int index, IColumnMappingCollection collection)
        {
            return CollectionIndexInt32(index, collection.GetType(), collection.Count);
        }

        internal static Exception ColumnsIndexSource(string srcColumn)
        {
            return CollectionIndexString(typeof(DataColumnMapping), "SourceColumn", srcColumn, typeof(DataColumnMappingCollection));
        }

        internal static Exception ColumnsIsNotParent(ICollection collection)
        {
            return ParametersIsNotParent(typeof(DataColumnMapping), collection);
        }

        internal static Exception ColumnsIsParent(ICollection collection)
        {
            return ParametersIsParent(typeof(DataColumnMapping), collection);
        }

        internal static Exception ColumnsUniqueSourceColumn(string srcColumn)
        {
            return CollectionUniqueValue(typeof(DataColumnMapping), "SourceColumn", srcColumn);
        }

        internal static InvalidOperationException CommandAsyncOperationCompleted()
        {
            return InvalidOperation(System.Data.Res.GetString("SQL_AsyncOperationCompleted"));
        }

        internal static Exception CommandTextRequired(string method)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_CommandTextRequired", new object[] { method }));
        }

        internal static bool CompareInsensitiveInvariant(string strvalue, string strconst)
        {
            return (0 == CultureInfo.InvariantCulture.CompareInfo.Compare(strvalue, strconst, CompareOptions.IgnoreCase));
        }

        internal static InvalidOperationException ComputerNameEx(int lastError)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_ComputerNameEx", new object[] { lastError }));
        }

        internal static ConfigurationException ConfigBaseElementsOnly(XmlNode node)
        {
            return Configuration(System.Data.Res.GetString("ConfigBaseElementsOnly"), node);
        }

        internal static ConfigurationException ConfigBaseNoChildNodes(XmlNode node)
        {
            return Configuration(System.Data.Res.GetString("ConfigBaseNoChildNodes"), node);
        }

        internal static InvalidOperationException ConfigProviderInvalid()
        {
            return InvalidOperation(System.Data.Res.GetString("ConfigProviderInvalid"));
        }

        internal static ConfigurationException ConfigProviderMissing()
        {
            return Configuration(System.Data.Res.GetString("ConfigProviderMissing"));
        }

        internal static ArgumentException ConfigProviderNotFound()
        {
            return Argument(System.Data.Res.GetString("ConfigProviderNotFound"));
        }

        internal static ConfigurationException ConfigProviderNotInstalled()
        {
            return Configuration(System.Data.Res.GetString("ConfigProviderNotInstalled"));
        }

        internal static ConfigurationException ConfigRequiredAttributeEmpty(string name, XmlNode node)
        {
            return Configuration(System.Data.Res.GetString("ConfigRequiredAttributeEmpty", new object[] { name }), node);
        }

        internal static ConfigurationException ConfigRequiredAttributeMissing(string name, XmlNode node)
        {
            return Configuration(System.Data.Res.GetString("ConfigRequiredAttributeMissing", new object[] { name }), node);
        }

        internal static ConfigurationException ConfigSectionsUnique(string sectionName)
        {
            return Configuration(System.Data.Res.GetString("ConfigSectionsUnique", new object[] { sectionName }));
        }

        internal static ConfigurationException ConfigUnableToLoadXmlMetaDataFile(string settingName)
        {
            return Configuration(System.Data.Res.GetString("OleDb_ConfigUnableToLoadXmlMetaDataFile", new object[] { settingName }));
        }

        internal static ConfigurationException ConfigUnrecognizedAttributes(XmlNode node)
        {
            return Configuration(System.Data.Res.GetString("ConfigUnrecognizedAttributes", new object[] { node.Attributes[0].Name }), node);
        }

        internal static ConfigurationException ConfigUnrecognizedElement(XmlNode node)
        {
            return Configuration(System.Data.Res.GetString("ConfigUnrecognizedElement"), node);
        }

        internal static ConfigurationException Configuration(string message)
        {
            ConfigurationException e = new ConfigurationErrorsException(message);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ConfigurationException Configuration(string message, XmlNode node)
        {
            ConfigurationException e = new ConfigurationErrorsException(message, node);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ConfigurationException ConfigWrongNumberOfValues(string settingName)
        {
            return Configuration(System.Data.Res.GetString("OleDb_ConfigWrongNumberOfValues", new object[] { settingName }));
        }

        internal static Exception ConnectionAlreadyOpen(ConnectionState state)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_ConnectionAlreadyOpen", new object[] { ConnectionStateMsg(state) }));
        }

        internal static Exception ConnectionIsDisabled(Exception InnerException)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_ConnectionIsDisabled"), InnerException);
        }

        internal static InvalidOperationException ConnectionRequired(string method)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_ConnectionRequired", new object[] { method }));
        }

        internal static InvalidOperationException ConnectionRequired_Res(string method)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_ConnectionRequired_" + method));
        }

        private static string ConnectionStateMsg(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Closed:
                case (ConnectionState.Broken | ConnectionState.Connecting):
                    return System.Data.Res.GetString("ADP_ConnectionStateMsg_Closed");

                case ConnectionState.Open:
                    return System.Data.Res.GetString("ADP_ConnectionStateMsg_Open");

                case ConnectionState.Connecting:
                    return System.Data.Res.GetString("ADP_ConnectionStateMsg_Connecting");

                case (ConnectionState.Executing | ConnectionState.Open):
                    return System.Data.Res.GetString("ADP_ConnectionStateMsg_OpenExecuting");

                case (ConnectionState.Fetching | ConnectionState.Open):
                    return System.Data.Res.GetString("ADP_ConnectionStateMsg_OpenFetching");
            }
            return System.Data.Res.GetString("ADP_ConnectionStateMsg", new object[] { state.ToString() });
        }

        internal static ArgumentException ConnectionStringSyntax(int index)
        {
            return Argument(System.Data.Res.GetString("ADP_ConnectionStringSyntax", new object[] { index }));
        }

        internal static ArgumentException ConvertFailed(Type fromType, Type toType, Exception innerException)
        {
            return Argument(System.Data.Res.GetString("SqlConvert_ConvertFailed", new object[] { fromType.FullName, toType.FullName }), innerException);
        }

        internal static DataException Data(string message)
        {
            DataException e = new DataException(message);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static InvalidOperationException DataAdapter(string error)
        {
            return InvalidOperation(error);
        }

        internal static InvalidOperationException DataAdapter(string error, Exception inner)
        {
            return InvalidOperation(error, inner);
        }

        internal static Exception DatabaseNameTooLong()
        {
            return Argument(System.Data.Res.GetString("ADP_DatabaseNameTooLong"));
        }

        private static InvalidOperationException DataMapping(string error)
        {
            return InvalidOperation(error);
        }

        internal static Exception DataReaderClosed(string method)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DataReaderClosed", new object[] { method }));
        }

        internal static Exception DataReaderNoData()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DataReaderNoData"));
        }

        internal static Exception DataTableDoesNotExist(string collectionName)
        {
            return Argument(System.Data.Res.GetString("MDF_DataTableDoesNotExist", new object[] { collectionName }));
        }

        internal static Exception DbRecordReadOnly(string methodname)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DbRecordReadOnly", new object[] { methodname }));
        }

        internal static ArgumentException DbTypeNotSupported(DbType type, Type enumtype)
        {
            return Argument(System.Data.Res.GetString("ADP_DbTypeNotSupported", new object[] { type.ToString(), enumtype.Name }));
        }

        internal static Exception DelegatedTransactionPresent()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DelegatedTransactionPresent"));
        }

        internal static Exception DeriveParametersNotSupported(IDbCommand value)
        {
            return DataAdapter(System.Data.Res.GetString("ADP_DeriveParametersNotSupported", new object[] { value.GetType().Name, value.CommandType.ToString() }));
        }

        internal static ArgumentException DoubleValuedProperty(string propertyName, string value1, string value2)
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_DoubleValuedProperty", new object[] { propertyName, value1, value2 }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static int DstCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
        }

        internal static InvalidOperationException DynamicSQLJoinUnsupported()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DynamicSQLJoinUnsupported"));
        }

        internal static InvalidOperationException DynamicSQLNestedQuote(string name, string quote)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DynamicSQLNestedQuote", new object[] { name, quote }));
        }

        internal static InvalidOperationException DynamicSQLNoKeyInfoDelete()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DynamicSQLNoKeyInfoDelete"));
        }

        internal static InvalidOperationException DynamicSQLNoKeyInfoRowVersionDelete()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DynamicSQLNoKeyInfoRowVersionDelete"));
        }

        internal static InvalidOperationException DynamicSQLNoKeyInfoRowVersionUpdate()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DynamicSQLNoKeyInfoRowVersionUpdate"));
        }

        internal static InvalidOperationException DynamicSQLNoKeyInfoUpdate()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DynamicSQLNoKeyInfoUpdate"));
        }

        internal static InvalidOperationException DynamicSQLNoTableInfo()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_DynamicSQLNoTableInfo"));
        }

        internal static Exception EmptyDatabaseName()
        {
            return Argument(System.Data.Res.GetString("ADP_EmptyDatabaseName"));
        }

        internal static void EscapeSpecialCharacters(string unescapedString, StringBuilder escapedString)
        {
            foreach (char ch in unescapedString)
            {
                if (@".$^{[(|)*+?\]".IndexOf(ch) >= 0)
                {
                    escapedString.Append(@"\");
                }
                escapedString.Append(ch);
            }
        }

        internal static Exception EvenLengthLiteralValue(string argumentName)
        {
            return Argument(System.Data.Res.GetString("ADP_EvenLengthLiteralValue"), argumentName);
        }

        internal static Exception ExceedsMaxDataLength(long specifiedLength, long maxLength)
        {
            return IndexOutOfRange(System.Data.Res.GetString("SQL_ExceedsMaxDataLength", new object[] { specifiedLength.ToString(CultureInfo.InvariantCulture), maxLength.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static Exception FillChapterAutoIncrement()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_FillChapterAutoIncrement"));
        }

        internal static Exception FillRequires(string parameter)
        {
            return ArgumentNull(parameter);
        }

        internal static Exception FillRequiresSourceTableName(string parameter)
        {
            return Argument(System.Data.Res.GetString("ADP_FillRequiresSourceTableName"), parameter);
        }

        internal static Exception FillSchemaRequiresSourceTableName(string parameter)
        {
            return Argument(System.Data.Res.GetString("ADP_FillSchemaRequiresSourceTableName"), parameter);
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

        internal static string FixUpDecimalSeparator(string numericString, bool formatLiteral, string decimalSeparator, char[] exponentSymbols)
        {
            if (numericString.IndexOfAny(exponentSymbols) == -1)
            {
                if (IsEmpty(decimalSeparator))
                {
                    decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                }
                if (formatLiteral)
                {
                    return numericString.Replace(".", decimalSeparator);
                }
                return numericString.Replace(decimalSeparator, ".");
            }
            return numericString;
        }

        private static int GenerateUniqueName(Dictionary<string, int> hash, ref string columnName, int index, int uniqueIndex)
        {
            while (true)
            {
                string str2 = columnName + uniqueIndex.ToString(CultureInfo.InvariantCulture);
                string key = str2.ToLower(CultureInfo.InvariantCulture);
                if (!hash.ContainsKey(key))
                {
                    columnName = str2;
                    hash.Add(key, index);
                    return uniqueIndex;
                }
                uniqueIndex++;
            }
        }

        internal static string GetComputerNameDnsFullyQualified()
        {
            if (!IsPlatformNT5)
            {
                return MachineName();
            }
            int bufferSize = 0;
            int lastError = 0;
            if (System.Data.Common.SafeNativeMethods.GetComputerNameEx(3, null, ref bufferSize) == 0)
            {
                lastError = Marshal.GetLastWin32Error();
            }
            if (((lastError != 0) && (lastError != 0xea)) || (bufferSize <= 0))
            {
                throw ComputerNameEx(lastError);
            }
            StringBuilder nameBuffer = new StringBuilder(bufferSize);
            bufferSize = nameBuffer.Capacity;
            if (System.Data.Common.SafeNativeMethods.GetComputerNameEx(3, nameBuffer, ref bufferSize) == 0)
            {
                throw ComputerNameEx(Marshal.GetLastWin32Error());
            }
            return nameBuffer.ToString();
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

        internal static FileVersionInfo GetVersionInfo(string filename)
        {
            FileVersionInfo versionInfo;
            new FileIOPermission(FileIOPermissionAccess.Read, filename).Assert();
            try
            {
                versionInfo = FileVersionInfo.GetVersionInfo(filename);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return versionInfo;
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

        internal static Exception HexDigitLiteralValue(string argumentName)
        {
            return Argument(System.Data.Res.GetString("ADP_HexDigitLiteralValue"), argumentName);
        }

        internal static ArgumentException IncorrectAsyncResult()
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_IncorrectAsyncResult"), "AsyncResult");
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception IncorrectNumberOfDataSourceInformationRows()
        {
            return Argument(System.Data.Res.GetString("MDF_IncorrectNumberOfDataSourceInformationRows"));
        }

        internal static IndexOutOfRangeException IndexOutOfRange(int value)
        {
            IndexOutOfRangeException e = new IndexOutOfRangeException(value.ToString(CultureInfo.InvariantCulture));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static IndexOutOfRangeException IndexOutOfRange(string error)
        {
            IndexOutOfRangeException e = new IndexOutOfRangeException(error);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception InternalConnectionError(ConnectionError internalError)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_InternalConnectionError", new object[] { (int) internalError }));
        }

        internal static Exception InternalError(InternalErrorCode internalError)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_InternalProviderError", new object[] { (int) internalError }));
        }

        internal static Exception InternalError(InternalErrorCode internalError, Exception innerException)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_InternalProviderError", new object[] { (int) internalError }), innerException);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static IntPtr IntPtrOffset(IntPtr pbase, int offset)
        {
            if (4 == PtrSize)
            {
                return (IntPtr) (pbase.ToInt32() + offset);
            }
            return (IntPtr) (pbase.ToInt64() + offset);
        }

        internal static int IntPtrToInt32(IntPtr value)
        {
            if (4 == PtrSize)
            {
                return (int) value;
            }
            long num = (long) value;
            num = Math.Min(0x7fffffffL, num);
            return (int) Math.Max(-2147483648L, num);
        }

        internal static ArgumentOutOfRangeException InvalidAcceptRejectRule(AcceptRejectRule value)
        {
            return InvalidEnumerationValue(typeof(AcceptRejectRule), (int) value);
        }

        internal static ArgumentException InvalidArgumentLength(string argumentName, int limit)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidArgumentLength", new object[] { argumentName, limit }));
        }

        internal static Exception InvalidArgumentValue(string methodName)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidArgumentValue", new object[] { methodName }));
        }

        internal static IndexOutOfRangeException InvalidBufferSizeOrIndex(int numBytes, int bufferIndex)
        {
            return IndexOutOfRange(System.Data.Res.GetString("SQL_InvalidBufferSizeOrIndex", new object[] { numBytes.ToString(CultureInfo.InvariantCulture), bufferIndex.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static InvalidCastException InvalidCast()
        {
            InvalidCastException e = new InvalidCastException();
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static InvalidCastException InvalidCast(string error)
        {
            return InvalidCast(error, null);
        }

        internal static InvalidCastException InvalidCast(string error, Exception inner)
        {
            InvalidCastException e = new InvalidCastException(error, inner);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentOutOfRangeException InvalidCatalogLocation(CatalogLocation value)
        {
            return InvalidEnumerationValue(typeof(CatalogLocation), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidCommandBehavior(CommandBehavior value)
        {
            return InvalidEnumerationValue(typeof(CommandBehavior), (int) value);
        }

        internal static Exception InvalidCommandTimeout(int value)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidCommandTimeout", new object[] { value.ToString(CultureInfo.InvariantCulture) }), "CommandTimeout");
        }

        internal static ArgumentOutOfRangeException InvalidCommandType(CommandType value)
        {
            return InvalidEnumerationValue(typeof(CommandType), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidConflictOptions(ConflictOption value)
        {
            return InvalidEnumerationValue(typeof(ConflictOption), (int) value);
        }

        internal static Exception InvalidConnectionOptionValue(string key)
        {
            return InvalidConnectionOptionValue(key, null);
        }

        internal static Exception InvalidConnectionOptionValue(string key, Exception inner)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidConnectionOptionValue", new object[] { key }), inner);
        }

        internal static Exception InvalidConnectionOptionValueLength(string key, int limit)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidConnectionOptionValueLength", new object[] { key, limit }));
        }

        internal static Exception InvalidConnectTimeoutValue()
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidConnectTimeoutValue"));
        }

        internal static InvalidOperationException InvalidDataDirectory()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_InvalidDataDirectory"));
        }

        internal static Exception InvalidDataLength(long length)
        {
            return IndexOutOfRange(System.Data.Res.GetString("SQL_InvalidDataLength", new object[] { length.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static ArgumentOutOfRangeException InvalidDataRowState(DataRowState value)
        {
            return InvalidEnumerationValue(typeof(DataRowState), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidDataRowVersion(DataRowVersion value)
        {
            return InvalidEnumerationValue(typeof(DataRowVersion), (int) value);
        }

        internal static ArgumentException InvalidDataType(TypeCode typecode)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidDataType", new object[] { typecode.ToString() }));
        }

        internal static InvalidOperationException InvalidDateTimeDigits(string dataTypeName)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_InvalidDateTimeDigits", new object[] { dataTypeName }));
        }

        internal static ArgumentOutOfRangeException InvalidDestinationBufferIndex(int maxLen, int dstOffset, string parameterName)
        {
            return ArgumentOutOfRange(System.Data.Res.GetString("ADP_InvalidDestinationBufferIndex", new object[] { maxLen.ToString(CultureInfo.InvariantCulture), dstOffset.ToString(CultureInfo.InvariantCulture) }), parameterName);
        }

        internal static ArgumentOutOfRangeException InvalidEnumerationValue(Type type, int value)
        {
            return ArgumentOutOfRange(System.Data.Res.GetString("ADP_InvalidEnumerationValue", new object[] { type.Name, value.ToString(CultureInfo.InvariantCulture) }), type.Name);
        }

        internal static Exception InvalidFormatValue()
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidFormatValue"));
        }

        internal static Exception InvalidImplicitConversion(Type fromtype, string totype)
        {
            return InvalidCast(System.Data.Res.GetString("ADP_InvalidImplicitConversion", new object[] { fromtype.Name, totype }));
        }

        internal static ArgumentOutOfRangeException InvalidIsolationLevel(System.Data.IsolationLevel value)
        {
            return InvalidEnumerationValue(typeof(System.Data.IsolationLevel), (int) value);
        }

        internal static ArgumentException InvalidKeyname(string parameterName)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidKey"), parameterName);
        }

        internal static ArgumentOutOfRangeException InvalidKeyRestrictionBehavior(KeyRestrictionBehavior value)
        {
            return InvalidEnumerationValue(typeof(KeyRestrictionBehavior), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidLoadOption(LoadOption value)
        {
            return InvalidEnumerationValue(typeof(LoadOption), (int) value);
        }

        internal static InvalidOperationException InvalidMaximumScale(string dataTypeName)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_InvalidMaximumScale", new object[] { dataTypeName }));
        }

        internal static Exception InvalidMaxRecords(string parameter, int max)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidMaxRecords", new object[] { max.ToString(CultureInfo.InvariantCulture) }), parameter);
        }

        internal static Exception InvalidMetaDataValue()
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidMetaDataValue"));
        }

        internal static ArgumentException InvalidMinMaxPoolSizeValues()
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidMinMaxPoolSizeValues"));
        }

        internal static ArgumentOutOfRangeException InvalidMissingMappingAction(MissingMappingAction value)
        {
            return InvalidEnumerationValue(typeof(MissingMappingAction), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidMissingSchemaAction(MissingSchemaAction value)
        {
            return InvalidEnumerationValue(typeof(MissingSchemaAction), (int) value);
        }

        internal static ArgumentException InvalidMultipartName(string property, string value)
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_InvalidMultipartName", new object[] { System.Data.Res.GetString(property), value }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException InvalidMultipartNameIncorrectUsageOfQuotes(string property, string value)
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_InvalidMultipartNameQuoteUsage", new object[] { System.Data.Res.GetString(property), value }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException InvalidMultipartNameToManyParts(string property, string value, int limit)
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_InvalidMultipartNameToManyParts", new object[] { System.Data.Res.GetString(property), value, limit }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException InvalidOffsetValue(int value)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidOffsetValue", new object[] { value.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static InvalidOperationException InvalidOperation(string error)
        {
            InvalidOperationException e = new InvalidOperationException(error);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static InvalidOperationException InvalidOperation(string error, Exception inner)
        {
            InvalidOperationException e = new InvalidOperationException(error, inner);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentOutOfRangeException InvalidParameterDirection(ParameterDirection value)
        {
            return InvalidEnumerationValue(typeof(ParameterDirection), (int) value);
        }

        internal static Exception InvalidParameterType(IDataParameterCollection collection, Type parameterType, object invalidValue)
        {
            return CollectionInvalidType(collection.GetType(), parameterType, invalidValue);
        }

        internal static ArgumentOutOfRangeException InvalidPermissionState(PermissionState value)
        {
            return InvalidEnumerationValue(typeof(PermissionState), (int) value);
        }

        internal static ArgumentException InvalidPrefixSuffix()
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_InvalidPrefixSuffix"));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException InvalidRestrictionValue(string collectionName, string restrictionName, string restrictionValue)
        {
            return Argument(System.Data.Res.GetString("MDF_InvalidRestrictionValue", new object[] { collectionName, restrictionName, restrictionValue }));
        }

        internal static ArgumentOutOfRangeException InvalidRule(Rule value)
        {
            return InvalidEnumerationValue(typeof(Rule), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidSchemaType(SchemaType value)
        {
            return InvalidEnumerationValue(typeof(SchemaType), (int) value);
        }

        internal static Exception InvalidSeekOrigin(string parameterName)
        {
            return ArgumentOutOfRange(System.Data.Res.GetString("ADP_InvalidSeekOrigin"), parameterName);
        }

        internal static ArgumentException InvalidSizeValue(int value)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidSizeValue", new object[] { value.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static ArgumentOutOfRangeException InvalidSourceBufferIndex(int maxLen, long srcOffset, string parameterName)
        {
            return ArgumentOutOfRange(System.Data.Res.GetString("ADP_InvalidSourceBufferIndex", new object[] { maxLen.ToString(CultureInfo.InvariantCulture), srcOffset.ToString(CultureInfo.InvariantCulture) }), parameterName);
        }

        internal static Exception InvalidSourceColumn(string parameter)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidSourceColumn"), parameter);
        }

        internal static Exception InvalidSourceTable(string parameter)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidSourceTable"), parameter);
        }

        internal static Exception InvalidStartRecord(string parameter, int start)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidStartRecord", new object[] { start.ToString(CultureInfo.InvariantCulture) }), parameter);
        }

        internal static ArgumentOutOfRangeException InvalidStatementType(StatementType value)
        {
            return InvalidEnumerationValue(typeof(StatementType), (int) value);
        }

        internal static ArgumentException InvalidUDL()
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidUDL"));
        }

        internal static ArgumentOutOfRangeException InvalidUpdateRowSource(UpdateRowSource value)
        {
            return InvalidEnumerationValue(typeof(UpdateRowSource), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidUpdateStatus(UpdateStatus value)
        {
            return InvalidEnumerationValue(typeof(UpdateStatus), (int) value);
        }

        internal static ArgumentOutOfRangeException InvalidUserDefinedTypeSerializationFormat(Format value)
        {
            return InvalidEnumerationValue(typeof(Format), (int) value);
        }

        internal static ArgumentException InvalidValue(string parameterName)
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidValue"), parameterName);
        }

        internal static Exception InvalidXml()
        {
            return Argument(System.Data.Res.GetString("MDF_InvalidXml"));
        }

        internal static Exception InvalidXMLBadVersion()
        {
            return Argument(System.Data.Res.GetString("ADP_InvalidXMLBadVersion"));
        }

        internal static Exception InvalidXmlInvalidValue(string collectionName, string columnName)
        {
            return Argument(System.Data.Res.GetString("MDF_InvalidXmlInvalidValue", new object[] { collectionName, columnName }));
        }

        internal static Exception InvalidXmlMissingColumn(string collectionName, string columnName)
        {
            return Argument(System.Data.Res.GetString("MDF_InvalidXmlMissingColumn", new object[] { collectionName, columnName }));
        }

        internal static bool IsCatchableExceptionType(Exception e)
        {
            Type c = e.GetType();
            return (((((c != StackOverflowType) && (c != OutOfMemoryType)) && ((c != ThreadAbortType) && (c != NullReferenceType))) && (c != AccessViolationType)) && !SecurityType.IsAssignableFrom(c));
        }

        internal static bool IsCatchableOrSecurityExceptionType(Exception e)
        {
            Type type = e.GetType();
            return ((((type != StackOverflowType) && (type != OutOfMemoryType)) && ((type != ThreadAbortType) && (type != NullReferenceType))) && (type != AccessViolationType));
        }

        internal static bool IsDirection(IDataParameter value, ParameterDirection condition)
        {
            return (condition == (condition & value.Direction));
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

        internal static bool IsNull(object value, out bool isINullable)
        {
            INullable nullable = value as INullable;
            isINullable = null != nullable;
            if ((!isINullable || !nullable.IsNull) && (value != null))
            {
                return (DBNull.Value == value);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool IsSysTxEqualSysEsTransaction()
        {
            return ((!ContextUtil.IsInTransaction && (null == Transaction.Current)) || (ContextUtil.IsInTransaction && (Transaction.Current == ContextUtil.SystemTransaction)));
        }

        internal static ArgumentException KeywordNotSupported(string keyword)
        {
            return Argument(System.Data.Res.GetString("ADP_KeywordNotSupported", new object[] { keyword }));
        }

        internal static Exception LiteralValueIsInvalid(string dataTypeName)
        {
            return Argument(System.Data.Res.GetString("ADP_LiteralValueIsInvalid", new object[] { dataTypeName }));
        }

        internal static object LocalMachineRegistryValue(string subkey, string queryvalue)
        {
            object obj2;
            new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\" + subkey).Assert();
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subkey, false))
                {
                    return ((key != null) ? key.GetValue(queryvalue) : null);
                }
            }
            catch (SecurityException exception)
            {
                TraceExceptionWithoutRethrow(exception);
                return null;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return obj2;
        }

        internal static Exception LocalTransactionPresent()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_LocalTransactionPresent"));
        }

        internal static string MachineName()
        {
            string machineName;
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.Assert();
            try
            {
                machineName = Environment.MachineName;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return machineName;
        }

        internal static InvalidOperationException MethodCalledTwice(string method)
        {
            InvalidOperationException e = new InvalidOperationException(System.Data.Res.GetString("ADP_CalledTwice", new object[] { method }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static NotImplementedException MethodNotImplemented(string methodName)
        {
            NotImplementedException e = new NotImplementedException(methodName);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception MismatchedAsyncResult(string expectedMethod, string gotMethod)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_MismatchedAsyncResult", new object[] { expectedMethod, gotMethod }));
        }

        internal static InvalidOperationException MissingColumnMapping(string srcColumn)
        {
            return DataMapping(System.Data.Res.GetString("ADP_MissingColumnMapping", new object[] { srcColumn }));
        }

        internal static Exception MissingConnectionOptionValue(string key, string requiredAdditionalKey)
        {
            return Argument(System.Data.Res.GetString("ADP_MissingConnectionOptionValue", new object[] { key, requiredAdditionalKey }));
        }

        internal static InvalidOperationException MissingDataReaderFieldType(int index)
        {
            return DataAdapter(System.Data.Res.GetString("ADP_MissingDataReaderFieldType", new object[] { index }));
        }

        internal static Exception MissingDataSourceInformationColumn()
        {
            return Argument(System.Data.Res.GetString("MDF_MissingDataSourceInformationColumn"));
        }

        internal static Exception MissingRestrictionColumn()
        {
            return Argument(System.Data.Res.GetString("MDF_MissingRestrictionColumn"));
        }

        internal static Exception MissingRestrictionRow()
        {
            return Argument(System.Data.Res.GetString("MDF_MissingRestrictionRow"));
        }

        internal static InvalidOperationException MissingSelectCommand(string method)
        {
            return Provider(System.Data.Res.GetString("ADP_MissingSelectCommand", new object[] { method }));
        }

        internal static InvalidOperationException MissingSourceCommand()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_MissingSourceCommand"));
        }

        internal static InvalidOperationException MissingSourceCommandConnection()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_MissingSourceCommandConnection"));
        }

        internal static InvalidOperationException MissingTableMapping(string srcTable)
        {
            return DataMapping(System.Data.Res.GetString("ADP_MissingTableMapping", new object[] { srcTable }));
        }

        internal static InvalidOperationException MissingTableMappingDestination(string dstTable)
        {
            return DataMapping(System.Data.Res.GetString("ADP_MissingTableMappingDestination", new object[] { dstTable }));
        }

        internal static InvalidOperationException MissingTableSchema(string cacheTable, string srcTable)
        {
            return DataMapping(System.Data.Res.GetString("ADP_MissingTableSchema", new object[] { cacheTable, srcTable }));
        }

        internal static ArgumentException MultipleReturnValue()
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_MultipleReturnValue"));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static bool NeedManualEnlistment()
        {
            if (IsWindowsNT)
            {
                bool flag = !InOutOfProcHelper.InProc;
                if ((flag && !IsSysTxEqualSysEsTransaction()) || (!flag && (null != Transaction.Current)))
                {
                    return true;
                }
            }
            return false;
        }

        internal static Exception NegativeParameter(string parameterName)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_NegativeParameter", new object[] { parameterName }));
        }

        internal static Exception NoColumns()
        {
            return Argument(System.Data.Res.GetString("MDF_NoColumns"));
        }

        internal static InvalidOperationException NoConnectionString()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_NoConnectionString"));
        }

        internal static Exception NonSeqByteAccess(long badIndex, long currIndex, string method)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_NonSeqByteAccess", new object[] { badIndex.ToString(CultureInfo.InvariantCulture), currIndex.ToString(CultureInfo.InvariantCulture), method }));
        }

        internal static InvalidOperationException NonSequentialColumnAccess(int badCol, int currCol)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_NonSequentialColumnAccess", new object[] { badCol.ToString(CultureInfo.InvariantCulture), currCol.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static InvalidOperationException NoQuoteChange()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_NoQuoteChange"));
        }

        internal static Exception NoStoredProcedureExists(string sproc)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_NoStoredProcedureExists", new object[] { sproc }));
        }

        internal static Exception NotADataColumnMapping(object value)
        {
            return CollectionInvalidType(typeof(DataColumnMappingCollection), typeof(DataColumnMapping), value);
        }

        internal static Exception NotADataTableMapping(object value)
        {
            return CollectionInvalidType(typeof(DataTableMappingCollection), typeof(DataTableMapping), value);
        }

        internal static Exception NotAPermissionElement()
        {
            return Argument(System.Data.Res.GetString("ADP_NotAPermissionElement"));
        }

        internal static NotImplementedException NotImplemented(string error)
        {
            NotImplementedException e = new NotImplementedException(error);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception NotRowType()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_NotRowType"));
        }

        internal static NotSupportedException NotSupported()
        {
            NotSupportedException e = new NotSupportedException();
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static NotSupportedException NotSupported(string error)
        {
            NotSupportedException e = new NotSupportedException(error);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentOutOfRangeException NotSupportedCommandBehavior(CommandBehavior value, string method)
        {
            return NotSupportedEnumerationValue(typeof(CommandBehavior), value.ToString(), method);
        }

        internal static ArgumentOutOfRangeException NotSupportedEnumerationValue(Type type, string value, string method)
        {
            return ArgumentOutOfRange(System.Data.Res.GetString("ADP_NotSupportedEnumerationValue", new object[] { type.Name, value, method }), type.Name);
        }

        internal static ArgumentOutOfRangeException NotSupportedStatementType(StatementType value, string method)
        {
            return NotSupportedEnumerationValue(typeof(StatementType), value.ToString(), method);
        }

        internal static ArgumentOutOfRangeException NotSupportedUserDefinedTypeSerializationFormat(Format value, string method)
        {
            return NotSupportedEnumerationValue(typeof(Format), value.ToString(), method);
        }

        internal static Exception NumericToDecimalOverflow()
        {
            return InvalidCast(System.Data.Res.GetString("ADP_NumericToDecimalOverflow"));
        }

        internal static ObjectDisposedException ObjectDisposed(object instance)
        {
            ObjectDisposedException e = new ObjectDisposedException(instance.GetType().Name);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception OdbcNoTypesFromProvider()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_OdbcNoTypesFromProvider"));
        }

        internal static Exception OffsetOutOfRangeException()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_OffsetOutOfRangeException"));
        }

        internal static InvalidOperationException OnlyOneTableForStartRecordOrMaxRecords()
        {
            return DataAdapter(System.Data.Res.GetString("ADP_OnlyOneTableForStartRecordOrMaxRecords"));
        }

        internal static Exception OpenConnectionPropertySet(string property, ConnectionState state)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_OpenConnectionPropertySet", new object[] { property, ConnectionStateMsg(state) }));
        }

        internal static InvalidOperationException OpenConnectionRequired(string method, ConnectionState state)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_OpenConnectionRequired", new object[] { method, ConnectionStateMsg(state) }));
        }

        internal static Exception OpenReaderExists()
        {
            return OpenReaderExists(null);
        }

        internal static Exception OpenReaderExists(Exception e)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_OpenReaderExists"), e);
        }

        internal static OverflowException Overflow(string error)
        {
            return Overflow(error, null);
        }

        internal static OverflowException Overflow(string error, Exception inner)
        {
            OverflowException e = new OverflowException(error, inner);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static Exception ParallelTransactionsNotSupported(IDbConnection obj)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_ParallelTransactionsNotSupported", new object[] { obj.GetType().Name }));
        }

        internal static Exception ParameterConversionFailed(object value, Type destType, Exception inner)
        {
            Exception exception;
            string message = System.Data.Res.GetString("ADP_ParameterConversionFailed", new object[] { value.GetType().Name, destType.Name });
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

        internal static ArgumentException ParametersIsNotParent(Type parameterType, ICollection collection)
        {
            return Argument(System.Data.Res.GetString("ADP_CollectionIsNotParent", new object[] { parameterType.Name, collection.GetType().Name }));
        }

        internal static ArgumentException ParametersIsParent(Type parameterType, ICollection collection)
        {
            return Argument(System.Data.Res.GetString("ADP_CollectionIsNotParent", new object[] { parameterType.Name, collection.GetType().Name }));
        }

        internal static Exception ParametersMappingIndex(int index, IDataParameterCollection collection)
        {
            return CollectionIndexInt32(index, collection.GetType(), collection.Count);
        }

        internal static Exception ParametersSourceIndex(string parameterName, IDataParameterCollection collection, Type parameterType)
        {
            return CollectionIndexString(parameterType, "ParameterName", parameterName, collection.GetType());
        }

        internal static ArgumentException ParameterValueOutOfRange(SqlDecimal value)
        {
            return Argument(System.Data.Res.GetString("ADP_ParameterValueOutOfRange", new object[] { value.ToString() }));
        }

        internal static ArgumentException ParameterValueOutOfRange(decimal value)
        {
            return Argument(System.Data.Res.GetString("ADP_ParameterValueOutOfRange", new object[] { value.ToString((IFormatProvider) null) }));
        }

        internal static Exception PermissionTypeMismatch()
        {
            return Argument(System.Data.Res.GetString("ADP_PermissionTypeMismatch"));
        }

        internal static Exception PooledOpenTimeout()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_PooledOpenTimeout"));
        }

        internal static Exception PrepareParameterScale(IDbCommand cmd, string type)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_PrepareParameterScale", new object[] { cmd.GetType().Name, type }));
        }

        internal static Exception PrepareParameterSize(IDbCommand cmd)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_PrepareParameterSize", new object[] { cmd.GetType().Name }));
        }

        internal static Exception PrepareParameterType(IDbCommand cmd)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_PrepareParameterType", new object[] { cmd.GetType().Name }));
        }

        internal static PlatformNotSupportedException PropertyNotSupported(string property)
        {
            PlatformNotSupportedException e = new PlatformNotSupportedException(System.Data.Res.GetString("ADP_PropertyNotSupported", new object[] { property }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        private static InvalidOperationException Provider(string error)
        {
            return InvalidOperation(error);
        }

        internal static Exception QueryFailed(string collectionName, Exception e)
        {
            return InvalidOperation(System.Data.Res.GetString("MDF_QueryFailed", new object[] { collectionName }), e);
        }

        internal static InvalidOperationException QuotePrefixNotSet(string method)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_QuotePrefixNotSet", new object[] { method }));
        }

        internal static bool RemoveStringQuotes(string quotePrefix, string quoteSuffix, string quotedString, out string unquotedString)
        {
            int num;
            int num2;
            if (quotePrefix == null)
            {
                num = 0;
            }
            else
            {
                num = quotePrefix.Length;
            }
            if (quoteSuffix == null)
            {
                num2 = 0;
            }
            else
            {
                num2 = quoteSuffix.Length;
            }
            if ((num2 + num) == 0)
            {
                unquotedString = quotedString;
                return true;
            }
            if (quotedString == null)
            {
                unquotedString = quotedString;
                return false;
            }
            int length = quotedString.Length;
            if (length < (num + num2))
            {
                unquotedString = quotedString;
                return false;
            }
            if ((num > 0) && !quotedString.StartsWith(quotePrefix, StringComparison.Ordinal))
            {
                unquotedString = quotedString;
                return false;
            }
            if (num2 > 0)
            {
                if (!quotedString.EndsWith(quoteSuffix, StringComparison.Ordinal))
                {
                    unquotedString = quotedString;
                    return false;
                }
                unquotedString = quotedString.Substring(num, length - (num + num2)).Replace(quoteSuffix + quoteSuffix, quoteSuffix);
            }
            else
            {
                unquotedString = quotedString.Substring(num, length - num);
            }
            return true;
        }

        internal static InvalidOperationException ResultsNotAllowedDuringBatch()
        {
            return DataAdapter(System.Data.Res.GetString("ADP_ResultsNotAllowedDuringBatch"));
        }

        internal static DataException RowUpdatedErrors()
        {
            return Data(System.Data.Res.GetString("ADP_RowUpdatedErrors"));
        }

        internal static DataException RowUpdatingErrors()
        {
            return Data(System.Data.Res.GetString("ADP_RowUpdatingErrors"));
        }

        internal static DataRow[] SelectAdapterRows(DataTable dataTable, bool sorted)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            DataRowCollection rows = dataTable.Rows;
            foreach (DataRow row3 in rows)
            {
                DataRowState rowState = row3.RowState;
                if (rowState != DataRowState.Added)
                {
                    if (rowState == DataRowState.Deleted)
                    {
                        goto Label_0048;
                    }
                    if (rowState == DataRowState.Modified)
                    {
                        goto Label_004E;
                    }
                }
                else
                {
                    num++;
                }
                continue;
            Label_0048:
                num2++;
                continue;
            Label_004E:
                num3++;
            }
            DataRow[] rowArray = new DataRow[(num + num2) + num3];
            if (sorted)
            {
                num3 = num + num2;
                num2 = num;
                num = 0;
                foreach (DataRow row in rows)
                {
                    DataRowState state = row.RowState;
                    if (state != DataRowState.Added)
                    {
                        if (state == DataRowState.Deleted)
                        {
                            goto Label_00CA;
                        }
                        if (state == DataRowState.Modified)
                        {
                            goto Label_00D5;
                        }
                    }
                    else
                    {
                        rowArray[num++] = row;
                    }
                    continue;
                Label_00CA:
                    rowArray[num2++] = row;
                    continue;
                Label_00D5:
                    rowArray[num3++] = row;
                }
                return rowArray;
            }
            int num4 = 0;
            foreach (DataRow row2 in rows)
            {
                if ((row2.RowState & (DataRowState.Modified | DataRowState.Deleted | DataRowState.Added)) != 0)
                {
                    rowArray[num4++] = row2;
                    if (num4 == rowArray.Length)
                    {
                        return rowArray;
                    }
                }
            }
            return rowArray;
        }

        internal static ArgumentException SingleValuedProperty(string propertyName, string value)
        {
            ArgumentException e = new ArgumentException(System.Data.Res.GetString("ADP_SingleValuedProperty", new object[] { propertyName, value }));
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static int SrcCompare(string strA, string strB)
        {
            if (!(strA == strB))
            {
                return 1;
            }
            return 0;
        }

        internal static Exception StreamClosed(string method)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_StreamClosed", new object[] { method }));
        }

        internal static int StringLength(string inputString)
        {
            if (inputString == null)
            {
                return 0;
            }
            return inputString.Length;
        }

        internal static Exception TablesAddNullAttempt(string parameter)
        {
            return CollectionNullValue(parameter, typeof(DataTableMappingCollection), typeof(DataTableMapping));
        }

        internal static Exception TablesDataSetTable(string cacheTable)
        {
            return CollectionIndexString(typeof(DataTableMapping), "DataSetTable", cacheTable, typeof(DataTableMappingCollection));
        }

        internal static Exception TablesIndexInt32(int index, ITableMappingCollection collection)
        {
            return CollectionIndexInt32(index, collection.GetType(), collection.Count);
        }

        internal static Exception TablesIsNotParent(ICollection collection)
        {
            return ParametersIsNotParent(typeof(DataTableMapping), collection);
        }

        internal static Exception TablesIsParent(ICollection collection)
        {
            return ParametersIsParent(typeof(DataTableMapping), collection);
        }

        internal static Exception TablesSourceIndex(string srcTable)
        {
            return CollectionIndexString(typeof(DataTableMapping), "SourceTable", srcTable, typeof(DataTableMappingCollection));
        }

        internal static Exception TablesUniqueSourceTable(string srcTable)
        {
            return CollectionUniqueValue(typeof(DataTableMapping), "SourceTable", srcTable);
        }

        internal static long TimerCurrent()
        {
            long lpSystemTimeAsFileTime = 0L;
            System.Data.Common.SafeNativeMethods.GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
            return lpSystemTimeAsFileTime;
        }

        internal static void TimerCurrent(out long ticks)
        {
            System.Data.Common.SafeNativeMethods.GetSystemTimeAsFileTime(out ticks);
        }

        internal static long TimerFromSeconds(int seconds)
        {
            return (seconds * 0x989680L);
        }

        internal static bool TimerHasExpired(long timerExpire)
        {
            return (TimerCurrent() > timerExpire);
        }

        internal static long TimerRemaining(long timerExpire)
        {
            long num2 = TimerCurrent();
            return (timerExpire - num2);
        }

        internal static long TimerRemainingMilliseconds(long timerExpire)
        {
            return TimerToMilliseconds(TimerRemaining(timerExpire));
        }

        internal static long TimerRemainingSeconds(long timerExpire)
        {
            return TimerToSeconds(TimerRemaining(timerExpire));
        }

        internal static long TimerToMilliseconds(long timerValue)
        {
            return (timerValue / 0x2710L);
        }

        private static long TimerToSeconds(long timerValue)
        {
            return (timerValue / 0x989680L);
        }

        internal static Exception TooManyRestrictions(string collectionName)
        {
            return Argument(System.Data.Res.GetString("MDF_TooManyRestrictions", new object[] { collectionName }));
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
            TraceException("<comm.ADP.TraceException|ERR|THROW> '%ls'\n", e);
        }

        internal static void TraceExceptionForCapture(Exception e)
        {
            TraceException("<comm.ADP.TraceException|ERR|CATCH> '%ls'\n", e);
        }

        internal static void TraceExceptionWithoutRethrow(Exception e)
        {
            TraceException("<comm.ADP.TraceException|ERR|CATCH> '%ls'\n", e);
        }

        internal static Exception TransactionCompleted()
        {
            return DataAdapter(System.Data.Res.GetString("ADP_TransactionCompleted"));
        }

        internal static InvalidOperationException TransactionCompletedButNotDisposed()
        {
            return Provider(System.Data.Res.GetString("ADP_TransactionCompletedButNotDisposed"));
        }

        internal static InvalidOperationException TransactionConnectionMismatch()
        {
            return Provider(System.Data.Res.GetString("ADP_TransactionConnectionMismatch"));
        }

        internal static Exception TransactionPresent()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_TransactionPresent"));
        }

        internal static InvalidOperationException TransactionRequired(string method)
        {
            return Provider(System.Data.Res.GetString("ADP_TransactionRequired", new object[] { method }));
        }

        internal static Exception TransactionZombied(IDbTransaction obj)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_TransactionZombied", new object[] { obj.GetType().Name }));
        }

        internal static TypeLoadException TypeLoad(string error)
        {
            TypeLoadException e = new TypeLoadException(error);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static ArgumentException UdlFileError(Exception inner)
        {
            return Argument(System.Data.Res.GetString("ADP_UdlFileError"), inner);
        }

        internal static Exception UnableToBuildCollection(string collectionName)
        {
            return Argument(System.Data.Res.GetString("MDF_UnableToBuildCollection", new object[] { collectionName }));
        }

        internal static InvalidOperationException UnableToCreateBooleanLiteral()
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_UnableToCreateBooleanLiteral"));
        }

        internal static Exception UndefinedCollection(string collectionName)
        {
            return Argument(System.Data.Res.GetString("MDF_UndefinedCollection", new object[] { collectionName }));
        }

        internal static Exception UndefinedPopulationMechanism(string populationMechanism)
        {
            return Argument(System.Data.Res.GetString("MDF_UndefinedPopulationMechanism", new object[] { populationMechanism }));
        }

        internal static Exception UninitializedParameterSize(int index, Type dataType)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_UninitializedParameterSize", new object[] { index.ToString(CultureInfo.InvariantCulture), dataType.Name }));
        }

        internal static ArgumentException UnknownDataType(Type dataType)
        {
            return Argument(System.Data.Res.GetString("ADP_UnknownDataType", new object[] { dataType.FullName }));
        }

        internal static ArgumentException UnknownDataTypeCode(Type dataType, TypeCode typeCode)
        {
            object[] args = new object[] { ((int) typeCode).ToString(CultureInfo.InvariantCulture), dataType.FullName };
            return Argument(System.Data.Res.GetString("ADP_UnknownDataTypeCode", args));
        }

        internal static Exception UnsupportedNativeDataTypeOleDb(string dataTypeName)
        {
            return Argument(System.Data.Res.GetString("ADP_UnsupportedNativeDataTypeOleDb", new object[] { dataTypeName }));
        }

        internal static Exception UnsupportedVersion(string collectionName)
        {
            return Argument(System.Data.Res.GetString("MDF_UnsupportedVersion", new object[] { collectionName }));
        }

        internal static ArgumentException UnwantedStatementType(StatementType statementType)
        {
            return Argument(System.Data.Res.GetString("ADP_UnwantedStatementType", new object[] { statementType.ToString() }));
        }

        internal static Exception UpdateConcurrencyViolation(StatementType statementType, int affected, int expected, DataRow[] dataRows)
        {
            string str;
            switch (statementType)
            {
                case StatementType.Update:
                    str = "ADP_UpdateConcurrencyViolation_Update";
                    break;

                case StatementType.Delete:
                    str = "ADP_UpdateConcurrencyViolation_Delete";
                    break;

                case StatementType.Batch:
                    str = "ADP_UpdateConcurrencyViolation_Batch";
                    break;

                default:
                    throw InvalidStatementType(statementType);
            }
            DBConcurrencyException e = new DBConcurrencyException(System.Data.Res.GetString(str, new object[] { affected.ToString(CultureInfo.InvariantCulture), expected.ToString(CultureInfo.InvariantCulture) }), null, dataRows);
            TraceExceptionAsReturnValue(e);
            return e;
        }

        internal static InvalidOperationException UpdateConnectionRequired(StatementType statementType, bool isRowUpdatingCommand)
        {
            string str;
            if (isRowUpdatingCommand)
            {
                str = "ADP_ConnectionRequired_Clone";
            }
            else
            {
                switch (statementType)
                {
                    case StatementType.Insert:
                        str = "ADP_ConnectionRequired_Insert";
                        goto Label_004C;

                    case StatementType.Update:
                        str = "ADP_ConnectionRequired_Update";
                        goto Label_004C;

                    case StatementType.Delete:
                        str = "ADP_ConnectionRequired_Delete";
                        goto Label_004C;

                    case StatementType.Batch:
                        str = "ADP_ConnectionRequired_Batch";
                        break;
                }
                throw InvalidStatementType(statementType);
            }
        Label_004C:
            return InvalidOperation(System.Data.Res.GetString(str));
        }

        internal static ArgumentException UpdateMismatchRowTable(int i)
        {
            return Argument(System.Data.Res.GetString("ADP_UpdateMismatchRowTable", new object[] { i.ToString(CultureInfo.InvariantCulture) }));
        }

        internal static InvalidOperationException UpdateOpenConnectionRequired(StatementType statementType, bool isRowUpdatingCommand, ConnectionState state)
        {
            string str;
            if (isRowUpdatingCommand)
            {
                str = "ADP_OpenConnectionRequired_Clone";
            }
            else
            {
                switch (statementType)
                {
                    case StatementType.Insert:
                        str = "ADP_OpenConnectionRequired_Insert";
                        goto Label_0042;

                    case StatementType.Update:
                        str = "ADP_OpenConnectionRequired_Update";
                        goto Label_0042;

                    case StatementType.Delete:
                        str = "ADP_OpenConnectionRequired_Delete";
                        goto Label_0042;
                }
                throw InvalidStatementType(statementType);
            }
        Label_0042:;
            return InvalidOperation(System.Data.Res.GetString(str, new object[] { ConnectionStateMsg(state) }));
        }

        internal static InvalidOperationException UpdateRequiresCommand(StatementType statementType, bool isRowUpdatingCommand)
        {
            string str;
            if (isRowUpdatingCommand)
            {
                str = "ADP_UpdateRequiresCommandClone";
            }
            else
            {
                switch (statementType)
                {
                    case StatementType.Select:
                        str = "ADP_UpdateRequiresCommandSelect";
                        goto Label_004C;

                    case StatementType.Insert:
                        str = "ADP_UpdateRequiresCommandInsert";
                        goto Label_004C;

                    case StatementType.Update:
                        str = "ADP_UpdateRequiresCommandUpdate";
                        goto Label_004C;

                    case StatementType.Delete:
                        str = "ADP_UpdateRequiresCommandDelete";
                        goto Label_004C;
                }
                throw InvalidStatementType(statementType);
            }
        Label_004C:
            return InvalidOperation(System.Data.Res.GetString(str));
        }

        internal static ArgumentNullException UpdateRequiresDataTable(string parameter)
        {
            return ArgumentNull(parameter);
        }

        internal static ArgumentNullException UpdateRequiresNonNullDataSet(string parameter)
        {
            return ArgumentNull(parameter);
        }

        internal static InvalidOperationException UpdateRequiresSourceTable(string defaultSrcTableName)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_UpdateRequiresSourceTable", new object[] { defaultSrcTableName }));
        }

        internal static InvalidOperationException UpdateRequiresSourceTableName(string srcTable)
        {
            return InvalidOperation(System.Data.Res.GetString("ADP_UpdateRequiresSourceTableName", new object[] { srcTable }));
        }

        internal static void ValidateCommandBehavior(CommandBehavior value)
        {
            if ((value < CommandBehavior.Default) || ((CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess | CommandBehavior.SingleRow | CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly | CommandBehavior.SingleResult) < value))
            {
                throw InvalidCommandBehavior(value);
            }
        }

        internal static ArgumentException VersionDoesNotSupportDataType(string typeName)
        {
            return Argument(System.Data.Res.GetString("ADP_VersionDoesNotSupportDataType", new object[] { typeName }));
        }

        internal static Exception WrongType(Type got, Type expected)
        {
            return Argument(System.Data.Res.GetString("SQL_WrongType", new object[] { got.ToString(), expected.ToString() }));
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
            InvalidParserState1 = 0x15,
            InvalidParserState2 = 0x16,
            InvalidParserState3 = 0x17,
            InvalidSmiCall = 0x29,
            NameValuePairNext = 20,
            NewObjectCannotBePooled = 6,
            NonPooledObjectUsedMoreThanOnce = 7,
            PooledObjectHasOwner = 3,
            PooledObjectInPoolMoreThanOnce = 4,
            PooledObjectWithoutPool = 15,
            PushingObjectSecondTime = 2,
            SqlDependencyCommandHashIsNotAssociatedWithNotification = 0x35,
            SqlDependencyObtainProcessDispatcherFailureObjectHandle = 50,
            SqlDependencyProcessDispatcherFailureAppDomain = 0x34,
            SqlDependencyProcessDispatcherFailureCreateInstance = 0x33,
            UnexpectedWaitAnyResult = 0x10,
            UnimplementedSMIMethod = 40,
            UnknownTransactionFailure = 60,
            UnpooledObjectHasOwner = 0,
            UnpooledObjectHasWrongOwner = 1
        }
    }
}


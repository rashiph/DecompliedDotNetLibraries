namespace System.Data.Common
{
    using System;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Transactions;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool CheckTokenMembership(IntPtr tokenHandle, byte[] sidToCheck, out bool isMember);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool ConvertSidToStringSidW(IntPtr sid, out IntPtr stringSid);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int CreateWellKnownSid(int sidType, byte[] domainSid, [Out] byte[] resultSid, ref uint resultSidLength);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
        internal static extern uint GetEffectiveRightsFromAclW(byte[] pAcl, ref Trustee pTrustee, out uint pAccessMask);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode)]
        internal static extern OleDbHResult GetErrorInfo([In] int dwReserved, [MarshalAs(UnmanagedType.Interface)] out IErrorInfo ppIErrorInfo);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool GetTokenInformation(IntPtr tokenHandle, uint token_class, IntPtr tokenStruct, uint tokenInformationLength, ref uint tokenString);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        internal static extern bool IsTokenRestricted(IntPtr tokenHandle);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int lstrlenW(IntPtr ptr);
        [DllImport("kernel32.dll")]
        internal static extern void SetLastError(int dwErrCode);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLAllocHandle(ODBC32.SQL_HANDLE HandleType, OdbcHandle InputHandle, out IntPtr OutputHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLAllocHandle(ODBC32.SQL_HANDLE HandleType, IntPtr InputHandle, out IntPtr OutputHandle);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLBindCol(OdbcStatementHandle StatementHandle, ushort ColumnNumber, ODBC32.SQL_C TargetType, IntPtr TargetValue, IntPtr BufferLength, IntPtr StrLen_or_Ind);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLBindCol(OdbcStatementHandle StatementHandle, ushort ColumnNumber, ODBC32.SQL_C TargetType, HandleRef TargetValue, IntPtr BufferLength, IntPtr StrLen_or_Ind);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLBindParameter(OdbcStatementHandle StatementHandle, ushort ParameterNumber, short ParamDirection, ODBC32.SQL_C SQLCType, short SQLType, IntPtr cbColDef, IntPtr ibScale, HandleRef rgbValue, IntPtr BufferLength, HandleRef StrLen_or_Ind);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLCancel(OdbcStatementHandle StatementHandle);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLCloseCursor(OdbcStatementHandle StatementHandle);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLColAttributeW(OdbcStatementHandle StatementHandle, short ColumnNumber, short FieldIdentifier, CNativeBuffer CharacterAttribute, short BufferLength, out short StringLength, out IntPtr NumericAttribute);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLColumnsW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string CatalogName, short NameLen1, [In, MarshalAs(UnmanagedType.LPWStr)] string SchemaName, short NameLen2, [In, MarshalAs(UnmanagedType.LPWStr)] string TableName, short NameLen3, [In, MarshalAs(UnmanagedType.LPWStr)] string ColumnName, short NameLen4);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLDisconnect(IntPtr ConnectionHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLDriverConnectW(OdbcConnectionHandle hdbc, IntPtr hwnd, [In, MarshalAs(UnmanagedType.LPWStr)] string connectionstring, short cbConnectionstring, IntPtr connectionstringout, short cbConnectionstringoutMax, out short cbConnectionstringout, short fDriverCompletion);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLEndTran(ODBC32.SQL_HANDLE HandleType, IntPtr Handle, short CompletionType);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLExecDirectW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string StatementText, int TextLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLExecute(OdbcStatementHandle StatementHandle);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLFetch(OdbcStatementHandle StatementHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLFreeHandle(ODBC32.SQL_HANDLE HandleType, IntPtr StatementHandle);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLFreeStmt(OdbcStatementHandle StatementHandle, ODBC32.STMT Option);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetConnectAttrW(OdbcConnectionHandle ConnectionHandle, ODBC32.SQL_ATTR Attribute, byte[] Value, int BufferLength, out int StringLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetData(OdbcStatementHandle StatementHandle, ushort ColumnNumber, ODBC32.SQL_C TargetType, CNativeBuffer TargetValue, IntPtr BufferLength, out IntPtr StrLen_or_Ind);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetDescFieldW(OdbcDescriptorHandle StatementHandle, short RecNumber, ODBC32.SQL_DESC FieldIdentifier, CNativeBuffer ValuePointer, int BufferLength, out int StringLength);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLGetDiagFieldW(ODBC32.SQL_HANDLE HandleType, OdbcHandle Handle, short RecNumber, short DiagIdentifier, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rchState, short BufferLength, out short StringLength);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLGetDiagRecW(ODBC32.SQL_HANDLE HandleType, OdbcHandle Handle, short RecNumber, StringBuilder rchState, out int NativeError, StringBuilder MessageText, short BufferLength, out short TextLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetFunctions(OdbcConnectionHandle hdbc, ODBC32.SQL_API fFunction, out short pfExists);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetInfoW(OdbcConnectionHandle hdbc, ODBC32.SQL_INFO fInfoType, byte[] rgbInfoValue, short cbInfoValueMax, out short pcbInfoValue);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetInfoW(OdbcConnectionHandle hdbc, ODBC32.SQL_INFO fInfoType, byte[] rgbInfoValue, short cbInfoValueMax, IntPtr pcbInfoValue);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetStmtAttrW(OdbcStatementHandle StatementHandle, ODBC32.SQL_ATTR Attribute, out IntPtr Value, int BufferLength, out int StringLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLGetTypeInfo(OdbcStatementHandle StatementHandle, short fSqlType);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLMoreResults(OdbcStatementHandle StatementHandle);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLNumResultCols(OdbcStatementHandle StatementHandle, out short ColumnCount);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLPrepareW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string StatementText, int TextLength);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLPrimaryKeysW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string CatalogName, short NameLen1, [In, MarshalAs(UnmanagedType.LPWStr)] string SchemaName, short NameLen2, [In, MarshalAs(UnmanagedType.LPWStr)] string TableName, short NameLen3);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLProcedureColumnsW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string CatalogName, short NameLen1, [In, MarshalAs(UnmanagedType.LPWStr)] string SchemaName, short NameLen2, [In, MarshalAs(UnmanagedType.LPWStr)] string ProcName, short NameLen3, [In, MarshalAs(UnmanagedType.LPWStr)] string ColumnName, short NameLen4);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLProceduresW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string CatalogName, short NameLen1, [In, MarshalAs(UnmanagedType.LPWStr)] string SchemaName, short NameLen2, [In, MarshalAs(UnmanagedType.LPWStr)] string ProcName, short NameLen3);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLRowCount(OdbcStatementHandle StatementHandle, out IntPtr RowCount);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLSetConnectAttrW(OdbcConnectionHandle ConnectionHandle, ODBC32.SQL_ATTR Attribute, IntPtr Value, int StringLength);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLSetConnectAttrW(OdbcConnectionHandle ConnectionHandle, ODBC32.SQL_ATTR Attribute, string Value, int StringLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLSetConnectAttrW(OdbcConnectionHandle ConnectionHandle, ODBC32.SQL_ATTR Attribute, IDtcTransaction Value, int StringLength);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLSetConnectAttrW(IntPtr ConnectionHandle, ODBC32.SQL_ATTR Attribute, IntPtr Value, int StringLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLSetDescFieldW(OdbcDescriptorHandle StatementHandle, short ColumnNumber, ODBC32.SQL_DESC FieldIdentifier, IntPtr CharacterAttribute, int BufferLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLSetDescFieldW(OdbcDescriptorHandle StatementHandle, short ColumnNumber, ODBC32.SQL_DESC FieldIdentifier, HandleRef CharacterAttribute, int BufferLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLSetEnvAttr(OdbcEnvironmentHandle EnvironmentHandle, ODBC32.SQL_ATTR Attribute, IntPtr Value, ODBC32.SQL_IS StringLength);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLSetStmtAttrW(OdbcStatementHandle StatementHandle, int Attribute, IntPtr Value, int StringLength);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLSpecialColumnsW(OdbcStatementHandle StatementHandle, ODBC32.SQL_SPECIALCOLS IdentifierType, [In, MarshalAs(UnmanagedType.LPWStr)] string CatalogName, short NameLen1, [In, MarshalAs(UnmanagedType.LPWStr)] string SchemaName, short NameLen2, [In, MarshalAs(UnmanagedType.LPWStr)] string TableName, short NameLen3, ODBC32.SQL_SCOPE Scope, ODBC32.SQL_NULLABILITY Nullable);
        [DllImport("odbc32.dll", CharSet=CharSet.Unicode)]
        internal static extern ODBC32.RetCode SQLStatisticsW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string CatalogName, short NameLen1, [In, MarshalAs(UnmanagedType.LPWStr)] string SchemaName, short NameLen2, [In, MarshalAs(UnmanagedType.LPWStr)] string TableName, short NameLen3, short Unique, short Reserved);
        [DllImport("odbc32.dll")]
        internal static extern ODBC32.RetCode SQLTablesW(OdbcStatementHandle StatementHandle, [In, MarshalAs(UnmanagedType.LPWStr)] string CatalogName, short NameLen1, [In, MarshalAs(UnmanagedType.LPWStr)] string SchemaName, short NameLen2, [In, MarshalAs(UnmanagedType.LPWStr)] string TableName, short NameLen3, [In, MarshalAs(UnmanagedType.LPWStr)] string TableType, short NameLen4);

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity, Guid("00000562-0000-0010-8000-00AA006D2EA4")]
        internal interface _ADORecord
        {
            [Obsolete("not used", true)]
            void get_Properties();
            object get_ActiveConnection();
            [Obsolete("not used", true)]
            void put_ActiveConnection();
            [Obsolete("not used", true)]
            void putref_ActiveConnection();
            [Obsolete("not used", true)]
            void get_State();
            [Obsolete("not used", true)]
            void get_Source();
            [Obsolete("not used", true)]
            void put_Source();
            [Obsolete("not used", true)]
            void putref_Source();
            [Obsolete("not used", true)]
            void get_Mode();
            [Obsolete("not used", true)]
            void put_Mode();
            [Obsolete("not used", true)]
            void get_ParentURL();
            [Obsolete("not used", true)]
            void MoveRecord();
            [Obsolete("not used", true)]
            void CopyRecord();
            [Obsolete("not used", true)]
            void DeleteRecord();
            [Obsolete("not used", true)]
            void Open();
            [PreserveSig]
            OleDbHResult Close();
        }

        [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("00000567-0000-0010-8000-00AA006D2EA4")]
        internal interface ADORecordConstruction
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object get_Row();
        }

        [ComImport, Guid("00000283-0000-0010-8000-00AA006D2EA4"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface ADORecordsetConstruction
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object get_Rowset();
            [Obsolete("not used", true)]
            void put_Rowset();
            IntPtr get_Chapter();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("0C733A8C-2A1C-11CE-ADE5-00AA0044773D")]
        internal interface IAccessor
        {
            [Obsolete("not used", true)]
            void AddRefAccessor();
            [PreserveSig]
            OleDbHResult CreateAccessor([In] int dwAccessorFlags, [In] IntPtr cBindings, [In] SafeHandle rgBindings, [In] IntPtr cbRowSize, out IntPtr phAccessor, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I4)] int[] rgStatus);
            [Obsolete("not used", true)]
            void GetBindings();
            [PreserveSig]
            OleDbHResult ReleaseAccessor([In] IntPtr hAccessor, out int pcRefCount);
        }

        [ComImport, Guid("0C733A93-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IChapteredRowset
        {
            [Obsolete("not used", true)]
            void AddRefChapter();
            [PreserveSig, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            OleDbHResult ReleaseChapter([In] IntPtr hChapter, out int pcRefCount);
        }

        [ComImport, Guid("0C733A11-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        internal interface IColumnsInfo
        {
            [PreserveSig]
            OleDbHResult GetColumnInfo(out IntPtr pcColumns, out IntPtr prgInfo, out IntPtr ppStringsBuffer);
        }

        [ComImport, Guid("0C733A10-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        internal interface IColumnsRowset
        {
            [PreserveSig]
            OleDbHResult GetAvailableColumns(out IntPtr pcOptColumns, out IntPtr prgOptColumns);
            [PreserveSig]
            OleDbHResult GetColumnsRowset([In] IntPtr pUnkOuter, [In] IntPtr cOptColumns, [In] SafeHandle rgOptColumns, [In] ref Guid riid, [In] int cPropertySets, [In] IntPtr rgPropertySets, [MarshalAs(UnmanagedType.Interface)] out UnsafeNativeMethods.IRowset ppColRowset);
        }

        [ComImport, Guid("0C733A26-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICommandPrepare
        {
            [PreserveSig]
            OleDbHResult Prepare([In] int cExpectedRuns);
        }

        [ComImport, Guid("0C733A79-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        internal interface ICommandProperties
        {
            [PreserveSig]
            OleDbHResult GetProperties([In] int cPropertyIDSets, [In] SafeHandle rgPropertyIDSets, out int pcPropertySets, out IntPtr prgPropertySets);
            [PreserveSig]
            OleDbHResult SetProperties([In] int cPropertySets, [In] SafeHandle rgPropertySets);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0C733A27-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity]
        internal interface ICommandText
        {
            [PreserveSig]
            OleDbHResult Cancel();
            [PreserveSig]
            OleDbHResult Execute([In] IntPtr pUnkOuter, [In] ref Guid riid, [In] tagDBPARAMS pDBParams, out IntPtr pcRowsAffected, [MarshalAs(UnmanagedType.Interface)] out object ppRowset);
            [Obsolete("not used", true)]
            void GetDBSession();
            [Obsolete("not used", true)]
            void GetCommandText();
            [PreserveSig]
            OleDbHResult SetCommandText([In] ref Guid rguidDialect, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0C733A64-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity]
        internal interface ICommandWithParameters
        {
            [Obsolete("not used", true)]
            void GetParameterInfo();
            [Obsolete("not used", true)]
            void MapParameterNames();
            [PreserveSig]
            OleDbHResult SetParameterInfo([In] IntPtr cParams, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] rgParamOrdinals, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.Struct)] tagDBPARAMBINDINFO[] rgParamBindInfo);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("2206CCB1-19C1-11D1-89E0-00C04FD7A829"), SuppressUnmanagedCodeSecurity]
        internal interface IDataInitialize
        {
        }

        [SuppressUnmanagedCodeSecurity]
        internal delegate OleDbHResult IDataInitializeGetDataSource(IntPtr pThis, IntPtr pUnkOuter, int dwClsCtx, [MarshalAs(UnmanagedType.LPWStr)] string pwszInitializationString, ref Guid riid, ref DataSourceWrapper ppDataSource);

        [SuppressUnmanagedCodeSecurity]
        internal delegate OleDbHResult IDBCreateCommandCreateCommand(IntPtr pThis, IntPtr pUnkOuter, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] ref object ppCommand);

        [SuppressUnmanagedCodeSecurity]
        internal delegate OleDbHResult IDBCreateSessionCreateSession(IntPtr pThis, IntPtr pUnkOuter, ref Guid riid, ref SessionWrapper ppDBSession);

        [ComImport, Guid("0C733A89-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        internal interface IDBInfo
        {
            [PreserveSig]
            OleDbHResult GetKeywords([MarshalAs(UnmanagedType.LPWStr)] out string ppwszKeywords);
            [PreserveSig]
            OleDbHResult GetLiteralInfo([In] int cLiterals, [In, MarshalAs(UnmanagedType.LPArray)] int[] rgLiterals, out int pcLiteralInfo, out IntPtr prgLiteralInfo, out IntPtr ppCharBuffer);
        }

        [SuppressUnmanagedCodeSecurity]
        internal delegate OleDbHResult IDBInitializeInitialize(IntPtr pThis);

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0C733A8A-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity]
        internal interface IDBProperties
        {
            [PreserveSig]
            OleDbHResult GetProperties([In] int cPropertyIDSets, [In] SafeHandle rgPropertyIDSets, out int pcPropertySets, out IntPtr prgPropertySets);
            [PreserveSig]
            OleDbHResult GetPropertyInfo([In] int cPropertyIDSets, [In] SafeHandle rgPropertyIDSets, out int pcPropertySets, out IntPtr prgPropertyInfoSets, out IntPtr ppDescBuffer);
            [PreserveSig]
            OleDbHResult SetProperties([In] int cPropertySets, [In] SafeHandle rgPropertySets);
        }

        [ComImport, SuppressUnmanagedCodeSecurity, Guid("0C733A7B-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDBSchemaRowset
        {
            [PreserveSig]
            OleDbHResult GetRowset([In] IntPtr pUnkOuter, [In] ref Guid rguidSchema, [In] int cRestrictions, [In, MarshalAs(UnmanagedType.LPArray)] object[] rgRestrictions, [In] ref Guid riid, [In] int cPropertySets, [In] IntPtr rgPropertySets, [MarshalAs(UnmanagedType.Interface)] out UnsafeNativeMethods.IRowset ppRowset);
            [PreserveSig]
            OleDbHResult GetSchemas(out int pcSchemas, out IntPtr rguidSchema, out IntPtr prgRestrictionSupport);
        }

        [ComImport, Guid("1CF2B120-547D-101B-8E65-08002B2BD119"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IErrorInfo
        {
            [Obsolete("not used", true)]
            void GetGUID();
            [PreserveSig]
            OleDbHResult GetSource([MarshalAs(UnmanagedType.BStr)] out string pBstrSource);
            [PreserveSig]
            OleDbHResult GetDescription([MarshalAs(UnmanagedType.BStr)] out string pBstrDescription);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0C733A67-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity]
        internal interface IErrorRecords
        {
            [Obsolete("not used", true)]
            void AddErrorRecord();
            [Obsolete("not used", true)]
            void GetBasicErrorInfo();
            [PreserveSig]
            OleDbHResult GetCustomErrorObject([In] int ulRecordNum, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out UnsafeNativeMethods.ISQLErrorInfo ppObject);
            [return: MarshalAs(UnmanagedType.Interface)]
            UnsafeNativeMethods.IErrorInfo GetErrorInfo([In] int ulRecordNum, [In] int lcid);
            [Obsolete("not used", true)]
            void GetErrorParameters();
            int GetRecordCount();
        }

        [ComImport, Guid("0C733A90-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMultipleResults
        {
            [PreserveSig]
            OleDbHResult GetResult([In] IntPtr pUnkOuter, [In] IntPtr lResultFlag, [In] ref Guid riid, out IntPtr pcRowsAffected, [MarshalAs(UnmanagedType.Interface)] out object ppRowset);
        }

        [ComImport, Guid("0C733A69-2A1C-11CE-ADE5-00AA0044773D"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOpenRowset
        {
            [PreserveSig]
            OleDbHResult OpenRowset([In] IntPtr pUnkOuter, [In] tagDBID pTableID, [In] IntPtr pIndexID, [In] ref Guid riid, [In] int cPropertySets, [In] IntPtr rgPropertySets, [MarshalAs(UnmanagedType.Interface)] out object ppRowset);
        }

        [ComImport, SuppressUnmanagedCodeSecurity, Guid("0C733AB4-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IRow
        {
            [PreserveSig]
            OleDbHResult GetColumns([In] IntPtr cColumns, [In, Out, MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.Struct)] tagDBCOLUMNACCESS[] rgColumns);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("0C733A7C-2A1C-11CE-ADE5-00AA0044773D")]
        internal interface IRowset
        {
            [Obsolete("not used", true)]
            void AddRefRows();
            [PreserveSig]
            OleDbHResult GetData([In] IntPtr hRow, [In] IntPtr hAccessor, [In] IntPtr pData);
            [PreserveSig]
            OleDbHResult GetNextRows([In] IntPtr hChapter, [In] IntPtr lRowsOffset, [In] IntPtr cRows, out IntPtr pcRowsObtained, [In] ref IntPtr pprghRows);
            [PreserveSig]
            OleDbHResult ReleaseRows([In] IntPtr cRows, [In] SafeHandle rghRows, [In] IntPtr rgRowOptions, [In] IntPtr rgRefCounts, [In] IntPtr rgRowStatus);
            [Obsolete("not used", true)]
            void RestartPosition();
        }

        [ComImport, SuppressUnmanagedCodeSecurity, Guid("0C733A55-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IRowsetInfo
        {
            [PreserveSig]
            OleDbHResult GetProperties([In] int cPropertyIDSets, [In] SafeHandle rgPropertyIDSets, out int pcPropertySets, out IntPtr prgPropertySets);
            [PreserveSig]
            OleDbHResult GetReferencedRowset([In] IntPtr iOrdinal, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out UnsafeNativeMethods.IRowset ppRowset);
        }

        [ComImport, Guid("0C733A74-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        internal interface ISQLErrorInfo
        {
            [return: MarshalAs(UnmanagedType.I4)]
            int GetSQLInfo([MarshalAs(UnmanagedType.BStr)] out string pbstrSQLState);
        }

        [ComImport, SuppressUnmanagedCodeSecurity, Guid("0C733A5F-2A1C-11CE-ADE5-00AA0044773D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ITransactionLocal
        {
            [Obsolete("not used", true)]
            void Commit();
            [Obsolete("not used", true)]
            void Abort();
            [Obsolete("not used", true)]
            void GetTransactionInfo();
            [Obsolete("not used", true)]
            void GetOptionsObject();
            [PreserveSig, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            OleDbHResult StartTransaction([In] int isoLevel, [In] int isoFlags, [In] IntPtr pOtherOptions, out int pulTransactionLevel);
        }

        [SuppressUnmanagedCodeSecurity]
        internal delegate int IUnknownQueryInterface(IntPtr pThis, ref Guid riid, ref IntPtr ppInterface);

        [ComImport, Guid("0000050E-0000-0010-8000-00AA006D2EA4"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface Recordset15
        {
            [Obsolete("not used", true)]
            void get_Properties();
            [Obsolete("not used", true)]
            void get_AbsolutePosition();
            [Obsolete("not used", true)]
            void put_AbsolutePosition();
            [Obsolete("not used", true)]
            void putref_ActiveConnection();
            [Obsolete("not used", true)]
            void put_ActiveConnection();
            object get_ActiveConnection();
            [Obsolete("not used", true)]
            void get_BOF();
            [Obsolete("not used", true)]
            void get_Bookmark();
            [Obsolete("not used", true)]
            void put_Bookmark();
            [Obsolete("not used", true)]
            void get_CacheSize();
            [Obsolete("not used", true)]
            void put_CacheSize();
            [Obsolete("not used", true)]
            void get_CursorType();
            [Obsolete("not used", true)]
            void put_CursorType();
            [Obsolete("not used", true)]
            void get_EOF();
            [Obsolete("not used", true)]
            void get_Fields();
            [Obsolete("not used", true)]
            void get_LockType();
            [Obsolete("not used", true)]
            void put_LockType();
            [Obsolete("not used", true)]
            void get_MaxRecords();
            [Obsolete("not used", true)]
            void put_MaxRecords();
            [Obsolete("not used", true)]
            void get_RecordCount();
            [Obsolete("not used", true)]
            void putref_Source();
            [Obsolete("not used", true)]
            void put_Source();
            [Obsolete("not used", true)]
            void get_Source();
            [Obsolete("not used", true)]
            void AddNew();
            [Obsolete("not used", true)]
            void CancelUpdate();
            [PreserveSig]
            OleDbHResult Close();
            [Obsolete("not used", true)]
            void Delete();
            [Obsolete("not used", true)]
            void GetRows();
            [Obsolete("not used", true)]
            void Move();
            [Obsolete("not used", true)]
            void MoveNext();
            [Obsolete("not used", true)]
            void MovePrevious();
            [Obsolete("not used", true)]
            void MoveFirst();
            [Obsolete("not used", true)]
            void MoveLast();
            [Obsolete("not used", true)]
            void Open();
            [Obsolete("not used", true)]
            void Requery();
            [Obsolete("not used", true)]
            void _xResync();
            [Obsolete("not used", true)]
            void Update();
            [Obsolete("not used", true)]
            void get_AbsolutePage();
            [Obsolete("not used", true)]
            void put_AbsolutePage();
            [Obsolete("not used", true)]
            void get_EditMode();
            [Obsolete("not used", true)]
            void get_Filter();
            [Obsolete("not used", true)]
            void put_Filter();
            [Obsolete("not used", true)]
            void get_PageCount();
            [Obsolete("not used", true)]
            void get_PageSize();
            [Obsolete("not used", true)]
            void put_PageSize();
            [Obsolete("not used", true)]
            void get_Sort();
            [Obsolete("not used", true)]
            void put_Sort();
            [Obsolete("not used", true)]
            void get_Status();
            [Obsolete("not used", true)]
            void get_State();
            [Obsolete("not used", true)]
            void _xClone();
            [Obsolete("not used", true)]
            void UpdateBatch();
            [Obsolete("not used", true)]
            void CancelBatch();
            [Obsolete("not used", true)]
            void get_CursorLocation();
            [Obsolete("not used", true)]
            void put_CursorLocation();
            [PreserveSig]
            OleDbHResult NextRecordset(out object RecordsAffected, [MarshalAs(UnmanagedType.Interface)] out object ppiRs);
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct Trustee
        {
            internal IntPtr _pMultipleTrustee;
            internal int _MultipleTrusteeOperation;
            internal int _TrusteeForm;
            internal int _TrusteeType;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string _name;
            internal Trustee(string name)
            {
                this._pMultipleTrustee = IntPtr.Zero;
                this._MultipleTrusteeOperation = 0;
                this._TrusteeForm = 1;
                this._TrusteeType = 1;
                this._name = name;
            }
        }
    }
}


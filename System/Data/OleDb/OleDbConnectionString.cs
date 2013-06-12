namespace System.Data.OleDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    internal sealed class OleDbConnectionString : DbConnectionOptions
    {
        private readonly string _expandedConnectionString;
        internal bool _hasSqlSupport;
        internal bool _hasSupportIRow;
        internal bool _hasSupportMultipleResults;
        private int _oledbServices;
        internal System.Data.OleDb.SchemaSupport[] _schemaSupport;
        internal int _sqlSupport;
        internal bool _supportIRow;
        internal bool _supportMultipleResults;
        internal readonly string ActualConnectionString;
        internal System.Data.Common.UnsafeNativeMethods.IUnknownQueryInterface DangerousDataSourceIUnknownQueryInterface;
        internal System.Data.Common.UnsafeNativeMethods.IDBCreateCommandCreateCommand DangerousIDBCreateCommandCreateCommand;
        internal System.Data.Common.UnsafeNativeMethods.IDBCreateSessionCreateSession DangerousIDBCreateSessionCreateSession;
        internal System.Data.Common.UnsafeNativeMethods.IDBInitializeInitialize DangerousIDBInitializeInitialize;
        internal bool HaveQueriedForCreateCommand;
        internal readonly bool PossiblePrompt;

        internal OleDbConnectionString(string connectionString, bool validate) : base(connectionString)
        {
            string str3 = base["prompt"];
            this.PossiblePrompt = (!ADP.IsEmpty(str3) && (string.Compare(str3, "noprompt", StringComparison.OrdinalIgnoreCase) != 0)) || !ADP.IsEmpty(base["window handle"]);
            if (!base.IsEmpty)
            {
                string str2 = null;
                if (!validate)
                {
                    int position = 0;
                    string filename = null;
                    this._expandedConnectionString = base.ExpandDataDirectories(ref filename, ref position);
                    if (!ADP.IsEmpty(filename))
                    {
                        filename = ADP.GetFullPath(filename);
                    }
                    if (filename != null)
                    {
                        str2 = LoadStringFromStorage(filename);
                        if (!ADP.IsEmpty(str2))
                        {
                            this._expandedConnectionString = string.Concat(new object[] { this._expandedConnectionString.Substring(0, position), str2, ';', this._expandedConnectionString.Substring(position) });
                        }
                    }
                }
                if (validate || ADP.IsEmpty(str2))
                {
                    this.ActualConnectionString = this.ValidateConnectionString(connectionString);
                }
            }
        }

        protected internal override PermissionSet CreatePermissionSet()
        {
            if (this.PossiblePrompt)
            {
                return new NamedPermissionSet("FullTrust");
            }
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new OleDbPermission(this));
            return set;
        }

        protected internal override string Expand()
        {
            if (this._expandedConnectionString != null)
            {
                return this._expandedConnectionString;
            }
            return base.Expand();
        }

        internal int GetSqlSupport(OleDbConnection connection)
        {
            int num = this._sqlSupport;
            if (!this._hasSqlSupport)
            {
                object dataSourcePropertyValue = connection.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 0x6d);
                if (dataSourcePropertyValue is int)
                {
                    num = (int) dataSourcePropertyValue;
                }
                this._sqlSupport = num;
                this._hasSqlSupport = true;
            }
            return num;
        }

        internal bool GetSupportIRow(OleDbConnection connection, OleDbCommand command)
        {
            bool flag = this._supportIRow;
            if (!this._hasSupportIRow)
            {
                flag = !(command.GetPropertyValue(OleDbPropertySetGuid.Rowset, 0x107) is OleDbPropertyStatus);
                this._supportIRow = flag;
                this._hasSupportIRow = true;
            }
            return flag;
        }

        internal bool GetSupportMultipleResults(OleDbConnection connection)
        {
            bool flag = this._supportMultipleResults;
            if (!this._hasSupportMultipleResults)
            {
                object dataSourcePropertyValue = connection.GetDataSourcePropertyValue(OleDbPropertySetGuid.DataSourceInfo, 0xc4);
                if (dataSourcePropertyValue is int)
                {
                    flag = 0 != ((int) dataSourcePropertyValue);
                }
                this._supportMultipleResults = flag;
                this._hasSupportMultipleResults = true;
            }
            return flag;
        }

        internal static bool IsMSDASQL(string progid)
        {
            if (!("msdasql" == progid) && !progid.StartsWith("msdasql.", StringComparison.Ordinal))
            {
                return ("microsoft ole db provider for odbc drivers" == progid);
            }
            return true;
        }

        private static string LoadStringFromFileStorage(string udlfilename)
        {
            Exception exception = null;
            string str = null;
            try
            {
                int count = ADP.CharSize * "﻿[oledb]\r\n; Everything after this line is an OLE DB initstring\r\n".Length;
                using (FileStream stream = new FileStream(udlfilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    long length = stream.Length;
                    if ((length < count) || (0L != (length % ((long) ADP.CharSize))))
                    {
                        exception = ADP.InvalidUDL();
                    }
                    else
                    {
                        byte[] buffer = new byte[count];
                        if (stream.Read(buffer, 0, buffer.Length) < count)
                        {
                            exception = ADP.InvalidUDL();
                        }
                        else if (Encoding.Unicode.GetString(buffer, 0, count) != "﻿[oledb]\r\n; Everything after this line is an OLE DB initstring\r\n")
                        {
                            exception = ADP.InvalidUDL();
                        }
                        else
                        {
                            buffer = new byte[length - count];
                            int num2 = stream.Read(buffer, 0, buffer.Length);
                            str = Encoding.Unicode.GetString(buffer, 0, num2);
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                if (!ADP.IsCatchableExceptionType(exception2))
                {
                    throw;
                }
                throw ADP.UdlFileError(exception2);
            }
            if (exception != null)
            {
                throw exception;
            }
            return str.Trim();
        }

        private static string LoadStringFromStorage(string udlfilename)
        {
            string str = null;
            Dictionary<string, string> dictionary = UDL._Pool;
            if ((dictionary == null) || !dictionary.TryGetValue(udlfilename, out str))
            {
                str = LoadStringFromFileStorage(udlfilename);
                if ((str == null) || (0 >= UdlPoolSize))
                {
                    return str;
                }
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, string>();
                    dictionary[udlfilename] = str;
                    lock (UDL._PoolLock)
                    {
                        if (UDL._Pool != null)
                        {
                            dictionary = UDL._Pool;
                        }
                        else
                        {
                            UDL._Pool = dictionary;
                            dictionary = null;
                        }
                    }
                }
                if (dictionary == null)
                {
                    return str;
                }
                lock (dictionary)
                {
                    dictionary[udlfilename] = str;
                }
            }
            return str;
        }

        internal static void ReleaseObjectPool()
        {
            UDL._PoolSizeInit = false;
            UDL._Pool = null;
        }

        private string ValidateConnectionString(string connectionString)
        {
            if (base.ConvertValueToBoolean("asynchronous processing", false))
            {
                throw ODB.AsynchronousNotSupported();
            }
            if (base.ConvertValueToInt32("connect timeout", 0) < 0)
            {
                throw ADP.InvalidConnectTimeoutValue();
            }
            string progid = base.ConvertValueToString("data provider", null);
            if (progid != null)
            {
                progid = progid.Trim();
                if (0 < progid.Length)
                {
                    ValidateProvider(progid);
                }
            }
            progid = base.ConvertValueToString("remote provider", null);
            if (progid != null)
            {
                progid = progid.Trim();
                if (0 < progid.Length)
                {
                    ValidateProvider(progid);
                }
            }
            progid = base.ConvertValueToString("provider", ADP.StrEmpty).Trim();
            ValidateProvider(progid);
            this._oledbServices = -13;
            if (!base.ContainsKey("ole db services") || ADP.IsEmpty(base["ole db services"]))
            {
                string g = (string) ADP.ClassesRootRegistryValue(progid + @"\CLSID", string.Empty);
                if ((g != null) && (0 < g.Length))
                {
                    Guid guid = new Guid(g);
                    if (ODB.CLSID_MSDASQL == guid)
                    {
                        throw ODB.MSDASQLNotSupported();
                    }
                    object obj2 = ADP.ClassesRootRegistryValue(@"CLSID\{" + guid.ToString("D", CultureInfo.InvariantCulture) + "}", "OLEDB_SERVICES");
                    if (obj2 == null)
                    {
                        return connectionString;
                    }
                    try
                    {
                        this._oledbServices = (int) obj2;
                    }
                    catch (InvalidCastException exception)
                    {
                        ADP.TraceExceptionWithoutRethrow(exception);
                    }
                    this._oledbServices &= -13;
                    StringBuilder builder = new StringBuilder();
                    builder.Append("ole db services");
                    builder.Append("=");
                    builder.Append(this._oledbServices.ToString(CultureInfo.InvariantCulture));
                    builder.Append(";");
                    builder.Append(connectionString);
                    connectionString = builder.ToString();
                }
                return connectionString;
            }
            this._oledbServices = base.ConvertValueToInt32("ole db services", -13);
            return connectionString;
        }

        private static void ValidateProvider(string progid)
        {
            if (ADP.IsEmpty(progid))
            {
                throw ODB.NoProviderSpecified();
            }
            if (0xff <= progid.Length)
            {
                throw ODB.InvalidProviderSpecified();
            }
            progid = progid.ToLower(CultureInfo.InvariantCulture);
            if (IsMSDASQL(progid))
            {
                throw ODB.MSDASQLNotSupported();
            }
        }

        internal int ConnectTimeout
        {
            get
            {
                return base.ConvertValueToInt32("connect timeout", 15);
            }
        }

        internal string DataSource
        {
            get
            {
                return base.ConvertValueToString("data source", ADP.StrEmpty);
            }
        }

        internal string InitialCatalog
        {
            get
            {
                return base.ConvertValueToString("initial catalog", ADP.StrEmpty);
            }
        }

        internal int OleDbServices
        {
            get
            {
                return this._oledbServices;
            }
        }

        internal string Provider
        {
            get
            {
                return base["provider"];
            }
        }

        internal System.Data.OleDb.SchemaSupport[] SchemaSupport
        {
            get
            {
                return this._schemaSupport;
            }
            set
            {
                this._schemaSupport = value;
            }
        }

        private static int UdlPoolSize
        {
            get
            {
                int num = UDL._PoolSize;
                if (!UDL._PoolSizeInit)
                {
                    object obj2 = ADP.LocalMachineRegistryValue(@"SOFTWARE\Microsoft\DataAccess\Udl Pooling", "Cache Size");
                    if (obj2 is int)
                    {
                        num = (int) obj2;
                        num = (0 < num) ? num : 0;
                        UDL._PoolSize = num;
                    }
                    UDL._PoolSizeInit = true;
                }
                return num;
            }
        }

        private static class UDL
        {
            internal static volatile Dictionary<string, string> _Pool;
            internal static object _PoolLock = new object();
            internal static int _PoolSize;
            internal static volatile bool _PoolSizeInit;
            internal const string Header = "﻿[oledb]\r\n; Everything after this line is an OLE DB initstring\r\n";
            internal const string Location = @"SOFTWARE\Microsoft\DataAccess\Udl Pooling";
            internal const string Pooling = "Cache Size";
        }
    }
}


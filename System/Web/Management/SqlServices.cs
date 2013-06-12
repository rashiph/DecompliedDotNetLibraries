namespace System.Web.Management
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.High)]
    public static class SqlServices
    {
        private static string ASPSTATE_DB = "ASPState";
        private static string DEFAULT_DB = "aspnetdb";
        private static string INSTALL_COMMON_SQL = "InstallCommon.sql";
        private static FeatureInfo[] s_featureInfos = new FeatureInfo[] { new FeatureInfo(SqlFeatures.Membership, new string[] { INSTALL_COMMON_SQL, "InstallMembership.sql" }, new string[] { "UninstallMembership.sql" }, new string[] { "aspnet_Membership" }, 1), new FeatureInfo(SqlFeatures.Profile, new string[] { INSTALL_COMMON_SQL, "InstallProfile.sql" }, new string[] { "UninstallProfile.sql" }, new string[] { "aspnet_Profile" }, 4), new FeatureInfo(SqlFeatures.RoleManager, new string[] { INSTALL_COMMON_SQL, "InstallRoles.sql" }, new string[] { "UninstallRoles.sql" }, new string[] { "aspnet_Roles", "aspnet_UsersInRoles" }, 2), new FeatureInfo(SqlFeatures.Personalization, new string[] { INSTALL_COMMON_SQL, "InstallPersonalization.sql" }, new string[] { "UninstallPersonalization.sql" }, new string[] { "aspnet_PersonalizationPerUser", "aspnet_Paths", "aspnet_PersonalizationAllUsers" }, 8), new FeatureInfo(SqlFeatures.SqlWebEventProvider, new string[] { INSTALL_COMMON_SQL, "InstallWebEventSqlProvider.sql" }, new string[] { "UninstallWebEventSqlProvider.sql" }, new string[] { "aspnet_WebEvent_Events" }, 0x10), new FeatureInfo(SqlFeatures.All, new string[0], new string[] { "UninstallCommon.sql" }, new string[] { "aspnet_Applications", "aspnet_Users", "aspnet_SchemaVersions" }, 0x7fffffff) };
        private static string SESSION_STATE_INSTALL_FILE = "InstallSqlState.sql";
        private static string SESSION_STATE_UNINSTALL_FILE = "UninstallSqlState.sql";
        private static string SSTYPE_CUSTOM = "sstype_custom";
        private static string SSTYPE_PERSISTED = "sstype_persisted";

        private static void ApplicationServicesParamCheck(SqlFeatures features, ref string database)
        {
            if (features != SqlFeatures.None)
            {
                if ((features & SqlFeatures.All) != features)
                {
                    throw new ArgumentException(System.Web.SR.GetString("SQL_Services_Invalid_Feature"));
                }
                CheckDatabaseName(ref database);
            }
        }

        private static void CheckDatabaseName(ref string database)
        {
            if (database != null)
            {
                database = database.TrimEnd(new char[0]);
                if (database.Length == 0)
                {
                    throw new ArgumentException(System.Web.SR.GetString("SQL_Services_Database_Empty_Or_Space_Only_Arg"));
                }
                database = RemoveSquareBrackets(database);
                if ((database.Contains("'") || database.Contains("[")) || database.Contains("]"))
                {
                    throw new ArgumentException(System.Web.SR.GetString("SQL_Services_Database_contains_invalid_chars"));
                }
            }
            if (database == null)
            {
                database = DEFAULT_DB;
            }
            else
            {
                database = "[" + database + "]";
            }
        }

        private static string ConstructConnectionString(string server, string user, string password, bool trusted)
        {
            string str = null;
            if (string.IsNullOrEmpty(server))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("server");
            }
            str = str + "server=" + server;
            if (trusted)
            {
                return (str + ";Trusted_Connection=true;");
            }
            if (string.IsNullOrEmpty(user))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("user");
            }
            string str2 = str;
            return (str2 + ";UID=" + user + ";PWD=" + password + ";");
        }

        private static void EnsureDatabaseExists(string database, SqlConnection sqlConnection)
        {
            string str = RemoveSquareBrackets(database);
            SqlCommand command = new SqlCommand("SELECT DB_ID(@database)", sqlConnection);
            command.Parameters.Add(new SqlParameter("@database", str));
            object obj2 = command.ExecuteScalar();
            if ((obj2 == null) || (obj2 == DBNull.Value))
            {
                throw new HttpException(System.Web.SR.GetString("SQL_Services_Error_Cant_Uninstall_Nonexisting_Database", new object[] { str }));
            }
        }

        private static void ExecuteFile(string file, string server, string database, string dbFileName, SqlConnection connection, bool sessionState, bool isInstall, SessionStateType sessionStatetype)
        {
            string str4;
            string content = File.ReadAllText(Path.Combine(HttpRuntime.AspInstallDirectory, file));
            string commands = null;
            if (file.Equals(INSTALL_COMMON_SQL))
            {
                content = FixContent(content, database, dbFileName, sessionState, sessionStatetype);
            }
            else
            {
                content = FixContent(content, database, null, sessionState, sessionStatetype);
            }
            StringReader reader = new StringReader(content);
            SqlCommand command = new SqlCommand(null, connection);
            do
            {
                bool flag = false;
                str4 = reader.ReadLine();
                if (str4 == null)
                {
                    flag = true;
                }
                else if (StringUtil.EqualsIgnoreCase(str4.Trim(), "GO"))
                {
                    flag = true;
                }
                else
                {
                    if (commands != null)
                    {
                        commands = commands + "\n";
                    }
                    commands = commands + str4;
                }
                if (flag & (commands != null))
                {
                    command.CommandText = commands;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                        SqlException sqlException = exception as SqlException;
                        if (sqlException != null)
                        {
                            int num = -1;
                            if (commands.IndexOf("sp_add_category", StringComparison.Ordinal) > -1)
                            {
                                num = 0x37b5;
                            }
                            else if (commands.IndexOf("sp_delete_job", StringComparison.Ordinal) > -1)
                            {
                                num = 0x37b6;
                                if (sessionState && !isInstall)
                                {
                                    throw new SqlExecutionException(System.Web.SR.GetString("SQL_Services_Error_Deleting_Session_Job"), server, database, file, commands, sqlException);
                                }
                            }
                            if (sqlException.Number != num)
                            {
                                throw new SqlExecutionException(System.Web.SR.GetString("SQL_Services_Error_Executing_Command", new object[] { file, sqlException.Number.ToString(CultureInfo.CurrentCulture), sqlException.Message }), server, database, file, commands, sqlException);
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    commands = null;
                }
            }
            while (str4 != null);
        }

        private static void ExecuteSessionFile(string file, string server, string database, string dbFileName, SqlConnection connection, bool isInstall, SessionStateType sessionStatetype)
        {
            ExecuteFile(file, server, database, dbFileName, connection, true, isInstall, sessionStatetype);
        }

        private static string FixContent(string content, string database, string dbFileName, bool sessionState, SessionStateType sessionStatetype)
        {
            if (database != null)
            {
                database = RemoveSquareBrackets(database);
            }
            if (sessionState)
            {
                if (sessionStatetype != SessionStateType.Temporary)
                {
                    if (sessionStatetype == SessionStateType.Persisted)
                    {
                        content = content.Replace("'sstype_temp'", "'" + SSTYPE_PERSISTED + "'");
                        content = content.Replace("[tempdb]", "[" + ASPSTATE_DB + "]");
                    }
                    else if (sessionStatetype == SessionStateType.Custom)
                    {
                        content = content.Replace("'sstype_temp'", "'" + SSTYPE_CUSTOM + "'");
                        content = content.Replace("[tempdb]", "[" + database + "]");
                        content = content.Replace("'ASPState'", "'" + database + "'");
                        content = content.Replace("[ASPState]", "[" + database + "]");
                    }
                }
            }
            else
            {
                content = content.Replace("'aspnetdb'", "'" + database.Replace("'", "''") + "'");
                content = content.Replace("[aspnetdb]", "[" + database + "]");
            }
            if (dbFileName != null)
            {
                if ((dbFileName.Contains("[") || dbFileName.Contains("]")) || dbFileName.Contains("'"))
                {
                    throw new ArgumentException(System.Web.SR.GetString("DbFileName_can_not_contain_invalid_chars"));
                }
                database = database.TrimStart(new char[] { '[' });
                database = database.TrimEnd(new char[] { ']' });
                string str = database + "_DAT";
                if (!char.IsLetter(str[0]))
                {
                    str = "A" + str;
                }
                content = content.Replace("SET @dboptions = N'/**/'", "SET @dboptions = N'" + ("ON ( NAME = " + str + ", FILENAME = ''" + dbFileName + "'', SIZE = 10MB, FILEGROWTH = 5MB )") + "'");
            }
            return content;
        }

        public static string GenerateApplicationServicesScripts(bool install, SqlFeatures features, string database)
        {
            StringBuilder builder = new StringBuilder();
            ApplicationServicesParamCheck(features, ref database);
            foreach (string str2 in GetFiles(install, features))
            {
                string content = File.ReadAllText(Path.Combine(HttpRuntime.AspInstallDirectory, str2));
                builder.Append(FixContent(content, database, null, false, SessionStateType.Temporary));
            }
            return builder.ToString();
        }

        public static string GenerateSessionStateScripts(bool install, SessionStateType type, string customDatabase)
        {
            SessionStateParamCheck(type, ref customDatabase);
            return FixContent(File.ReadAllText(Path.Combine(HttpRuntime.AspInstallDirectory, install ? SESSION_STATE_INSTALL_FILE : SESSION_STATE_UNINSTALL_FILE)), customDatabase, null, true, type);
        }

        private static ArrayList GetFiles(bool install, SqlFeatures features)
        {
            ArrayList list = new ArrayList();
            bool flag = false;
            for (int i = 0; i < s_featureInfos.Length; i++)
            {
                string[] strArray = null;
                if ((s_featureInfos[i]._feature & features) == s_featureInfos[i]._feature)
                {
                    if (install)
                    {
                        strArray = s_featureInfos[i]._installFiles;
                    }
                    else
                    {
                        strArray = s_featureInfos[i]._uninstallFiles;
                    }
                }
                if (strArray != null)
                {
                    for (int j = 0; j < strArray.Length; j++)
                    {
                        string str = strArray[j];
                        if ((str != null) && ((str != INSTALL_COMMON_SQL) || !flag))
                        {
                            list.Add(str);
                            if (!flag && (str == INSTALL_COMMON_SQL))
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            return list;
        }

        private static SqlConnection GetSqlConnection(string server, string user, string password, bool trusted, string connectionString)
        {
            SqlConnection connection;
            if (connectionString == null)
            {
                connectionString = ConstructConnectionString(server, user, password, trusted);
            }
            try
            {
                new SqlConnection(connectionString).Open();
            }
            catch (Exception exception)
            {
                connection = null;
                throw new HttpException(System.Web.SR.GetString("SQL_Services_Cant_connect_sql_database"), exception);
            }
            return connection;
        }

        internal static void Install(string database, string dbFileName, string connectionString)
        {
            SetupApplicationServices(null, null, null, false, connectionString, database, dbFileName, SqlFeatures.All, true);
        }

        public static void Install(string server, string database, SqlFeatures features)
        {
            SetupApplicationServices(server, null, null, true, null, database, null, features, true);
        }

        public static void Install(string database, SqlFeatures features, string connectionString)
        {
            SetupApplicationServices(null, null, null, true, connectionString, database, null, features, true);
        }

        public static void Install(string server, string user, string password, string database, SqlFeatures features)
        {
            SetupApplicationServices(server, user, password, false, null, database, null, features, true);
        }

        public static void InstallSessionState(string server, string customDatabase, SessionStateType type)
        {
            SetupSessionState(server, null, null, true, null, customDatabase, type, true);
        }

        public static void InstallSessionState(string customDatabase, SessionStateType type, string connectionString)
        {
            SetupSessionState(null, null, null, true, connectionString, customDatabase, type, true);
        }

        public static void InstallSessionState(string server, string user, string password, string customDatabase, SessionStateType type)
        {
            SetupSessionState(server, user, password, false, null, customDatabase, type, true);
        }

        private static string RemoveSquareBrackets(string database)
        {
            if (((database != null) && StringUtil.StringStartsWith(database, '[')) && StringUtil.StringEndsWith(database, ']'))
            {
                return database.Substring(1, database.Length - 2);
            }
            return database;
        }

        private static void SessionStateParamCheck(SessionStateType type, ref string customDatabase)
        {
            if ((type == SessionStateType.Custom) && string.IsNullOrEmpty(customDatabase))
            {
                throw new ArgumentException(System.Web.SR.GetString("SQL_Services_Error_missing_custom_database"), "customDatabase");
            }
            if ((type != SessionStateType.Custom) && (customDatabase != null))
            {
                throw new ArgumentException(System.Web.SR.GetString("SQL_Services_Error_Cant_use_custom_database"), "customDatabase");
            }
            CheckDatabaseName(ref customDatabase);
        }

        private static void SetupApplicationServices(string server, string user, string password, bool trusted, string connectionString, string database, string dbFileName, SqlFeatures features, bool install)
        {
            SqlConnection sqlConnection = null;
            ApplicationServicesParamCheck(features, ref database);
            ArrayList files = GetFiles(install, features);
            try
            {
                sqlConnection = GetSqlConnection(server, user, password, trusted, connectionString);
                if (!install)
                {
                    EnsureDatabaseExists(database, sqlConnection);
                    string databaseName = RemoveSquareBrackets(database);
                    if (sqlConnection.Database != databaseName)
                    {
                        sqlConnection.ChangeDatabase(databaseName);
                    }
                    int num = 0;
                    for (int i = 0; i < s_featureInfos.Length; i++)
                    {
                        if ((s_featureInfos[i]._feature & features) == s_featureInfos[i]._feature)
                        {
                            num |= s_featureInfos[i]._dataCheckBitMask;
                        }
                    }
                    SqlCommand command = new SqlCommand("dbo.aspnet_AnyDataInTables", sqlConnection);
                    command.Parameters.Add(new SqlParameter("@TablesToCheck", num));
                    command.CommandType = CommandType.StoredProcedure;
                    string str2 = null;
                    try
                    {
                        str2 = command.ExecuteScalar() as string;
                    }
                    catch (SqlException exception)
                    {
                        if (exception.Number != 0xafc)
                        {
                            throw;
                        }
                    }
                    if (!string.IsNullOrEmpty(str2))
                    {
                        throw new NotSupportedException(System.Web.SR.GetString("SQL_Services_Error_Cant_Uninstall_Nonempty_Table", new object[] { str2, database }));
                    }
                }
                foreach (string str3 in files)
                {
                    ExecuteFile(str3, server, database, dbFileName, sqlConnection, false, false, SessionStateType.Temporary);
                }
            }
            finally
            {
                if (sqlConnection != null)
                {
                    try
                    {
                        sqlConnection.Close();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        sqlConnection = null;
                    }
                }
            }
        }

        private static void SetupSessionState(string server, string user, string password, bool trusted, string connectionString, string customDatabase, SessionStateType type, bool install)
        {
            SqlConnection sqlConnection = null;
            SessionStateParamCheck(type, ref customDatabase);
            try
            {
                sqlConnection = GetSqlConnection(server, user, password, trusted, connectionString);
                if (!install && (type == SessionStateType.Custom))
                {
                    EnsureDatabaseExists(customDatabase, sqlConnection);
                }
                ExecuteSessionFile(install ? SESSION_STATE_INSTALL_FILE : SESSION_STATE_UNINSTALL_FILE, server, customDatabase, null, sqlConnection, install, type);
            }
            finally
            {
                if (sqlConnection != null)
                {
                    try
                    {
                        sqlConnection.Close();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        sqlConnection = null;
                    }
                }
            }
        }

        public static void Uninstall(string server, string database, SqlFeatures features)
        {
            SetupApplicationServices(server, null, null, true, null, database, null, features, false);
        }

        public static void Uninstall(string database, SqlFeatures features, string connectionString)
        {
            SetupApplicationServices(null, null, null, true, connectionString, database, null, features, false);
        }

        public static void Uninstall(string server, string user, string password, string database, SqlFeatures features)
        {
            SetupApplicationServices(server, user, password, false, null, database, null, features, false);
        }

        public static void UninstallSessionState(string server, string customDatabase, SessionStateType type)
        {
            SetupSessionState(server, null, null, true, null, customDatabase, type, false);
        }

        public static void UninstallSessionState(string customDatabase, SessionStateType type, string connectionString)
        {
            SetupSessionState(null, null, null, true, connectionString, customDatabase, type, false);
        }

        public static void UninstallSessionState(string server, string user, string password, string customDatabase, SessionStateType type)
        {
            SetupSessionState(server, user, password, false, null, customDatabase, type, false);
        }

        internal static ArrayList ApplicationServiceTables
        {
            get
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < s_featureInfos.Length; i++)
                {
                    list.InsertRange(list.Count, s_featureInfos[i]._tablesRemovedInUninstall);
                }
                return list;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FeatureInfo
        {
            internal SqlFeatures _feature;
            internal string[] _installFiles;
            internal string[] _uninstallFiles;
            internal string[] _tablesRemovedInUninstall;
            internal int _dataCheckBitMask;
            internal FeatureInfo(SqlFeatures feature, string[] installFiles, string[] uninstallFiles, string[] tablesRemovedInUninstall, int dataCheckBitMask)
            {
                this._feature = feature;
                this._installFiles = installFiles;
                this._uninstallFiles = uninstallFiles;
                this._tablesRemovedInUninstall = tablesRemovedInUninstall;
                this._dataCheckBitMask = dataCheckBitMask;
            }
        }
    }
}


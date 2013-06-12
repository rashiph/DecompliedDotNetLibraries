namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.DataAccess;
    using System.Web.Util;

    public class SqlPersonalizationProvider : PersonalizationProvider
    {
        private string _applicationName;
        private int _commandTimeout;
        private string _connectionString;
        private int _SchemaVersionCheck;
        private const int maxStringLength = 0x100;

        private void CheckSchemaVersion(SqlConnection connection)
        {
            string[] features = new string[] { "Personalization" };
            string version = "1";
            SecUtility.CheckSchemaVersion(this, connection, features, version, ref this._SchemaVersionCheck);
        }

        private SqlParameter CreateParameter(string name, SqlDbType dbType, object value)
        {
            return new SqlParameter(name, dbType) { Value = value };
        }

        private PersonalizationStateInfoCollection FindSharedState(string path, int pageIndex, int pageSize, out int totalRecords)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            SqlDataReader reader = null;
            PersonalizationStateInfoCollection infos2;
            totalRecords = 0;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_FindState", connection);
                    this.SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;
                    parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit)).Value = true;
                    parameters.AddWithValue("ApplicationName", this.ApplicationName);
                    parameters.AddWithValue("PageIndex", pageIndex);
                    parameters.AddWithValue("PageSize", pageSize);
                    SqlParameter parameter2 = new SqlParameter("@ReturnValue", SqlDbType.Int) {
                        Direction = ParameterDirection.ReturnValue
                    };
                    parameters.Add(parameter2);
                    SqlParameter parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null)
                    {
                        parameter.Value = path;
                    }
                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                    PersonalizationStateInfoCollection infos = new PersonalizationStateInfoCollection();
                    if (reader != null)
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string str = reader.GetString(0);
                                DateTime lastUpdatedDate = reader.IsDBNull(1) ? DateTime.MinValue : DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc);
                                int size = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                int sizeOfPersonalizations = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                                int countOfPersonalizations = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                infos.Add(new SharedPersonalizationStateInfo(str, lastUpdatedDate, size, sizeOfPersonalizations, countOfPersonalizations));
                            }
                        }
                        reader.Close();
                        reader = null;
                    }
                    if ((parameter2.Value != null) && (parameter2.Value is int))
                    {
                        totalRecords = (int) parameter2.Value;
                    }
                    infos2 = infos;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return infos2;
        }

        public override PersonalizationStateInfoCollection FindState(PersonalizationScope scope, PersonalizationStateQuery query, int pageIndex, int pageSize, out int totalRecords)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            PersonalizationProviderHelper.CheckPageIndexAndSize(pageIndex, pageSize);
            if (scope == PersonalizationScope.Shared)
            {
                string str = null;
                if (query != null)
                {
                    str = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, 0x100);
                }
                return this.FindSharedState(str, pageIndex, pageSize, out totalRecords);
            }
            string path = null;
            DateTime defaultInactiveSinceDate = PersonalizationAdministration.DefaultInactiveSinceDate;
            string username = null;
            if (query != null)
            {
                path = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, 0x100);
                defaultInactiveSinceDate = query.UserInactiveSinceDate;
                username = StringUtil.CheckAndTrimString(query.UsernameToMatch, "query.UsernameToMatch", false, 0x100);
            }
            return this.FindUserState(path, defaultInactiveSinceDate, username, pageIndex, pageSize, out totalRecords);
        }

        private PersonalizationStateInfoCollection FindUserState(string path, DateTime inactiveSinceDate, string username, int pageIndex, int pageSize, out int totalRecords)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            SqlDataReader reader = null;
            PersonalizationStateInfoCollection infos2;
            totalRecords = 0;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_FindState", connection);
                    this.SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;
                    parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit)).Value = false;
                    parameters.AddWithValue("ApplicationName", this.ApplicationName);
                    parameters.AddWithValue("PageIndex", pageIndex);
                    parameters.AddWithValue("PageSize", pageSize);
                    SqlParameter parameter2 = new SqlParameter("@ReturnValue", SqlDbType.Int) {
                        Direction = ParameterDirection.ReturnValue
                    };
                    parameters.Add(parameter2);
                    SqlParameter parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null)
                    {
                        parameter.Value = path;
                    }
                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    if (username != null)
                    {
                        parameter.Value = username;
                    }
                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);
                    if (inactiveSinceDate != PersonalizationAdministration.DefaultInactiveSinceDate)
                    {
                        parameter.Value = inactiveSinceDate.ToUniversalTime();
                    }
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                    PersonalizationStateInfoCollection infos = new PersonalizationStateInfoCollection();
                    if (reader != null)
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string str = reader.GetString(0);
                                DateTime lastUpdatedDate = DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc);
                                int size = reader.GetInt32(2);
                                string str2 = reader.GetString(3);
                                DateTime lastActivityDate = DateTime.SpecifyKind(reader.GetDateTime(4), DateTimeKind.Utc);
                                infos.Add(new UserPersonalizationStateInfo(str, lastUpdatedDate, size, str2, lastActivityDate));
                            }
                        }
                        reader.Close();
                        reader = null;
                    }
                    if ((parameter2.Value != null) && (parameter2.Value is int))
                    {
                        totalRecords = (int) parameter2.Value;
                    }
                    infos2 = infos;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return infos2;
        }

        private SqlConnectionHolder GetConnectionHolder()
        {
            SqlConnection connection = null;
            SqlConnectionHolder holder = SqlConnectionHelper.GetConnection(this._connectionString, true);
            if (holder != null)
            {
                connection = holder.Connection;
            }
            if (connection == null)
            {
                throw new ProviderException(System.Web.SR.GetString("PersonalizationProvider_CantAccess", new object[] { this.Name }));
            }
            return holder;
        }

        private int GetCountOfSharedState(string path)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int num = 0;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_GetCountOfState", connection);
                    this.SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;
                    parameters.Add(new SqlParameter("Count", SqlDbType.Int)).Direction = ParameterDirection.Output;
                    parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit)).Value = true;
                    parameters.AddWithValue("ApplicationName", this.ApplicationName);
                    SqlParameter parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null)
                    {
                        parameter.Value = path;
                    }
                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);
                    command.ExecuteNonQuery();
                    parameter = command.Parameters[0];
                    if (((parameter != null) && (parameter.Value != null)) && (parameter.Value is int))
                    {
                        num = (int) parameter.Value;
                    }
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        public override int GetCountOfState(PersonalizationScope scope, PersonalizationStateQuery query)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            if (scope == PersonalizationScope.Shared)
            {
                string str = null;
                if (query != null)
                {
                    str = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, 0x100);
                }
                return this.GetCountOfSharedState(str);
            }
            string path = null;
            DateTime defaultInactiveSinceDate = PersonalizationAdministration.DefaultInactiveSinceDate;
            string username = null;
            if (query != null)
            {
                path = StringUtil.CheckAndTrimString(query.PathToMatch, "query.PathToMatch", false, 0x100);
                defaultInactiveSinceDate = query.UserInactiveSinceDate;
                username = StringUtil.CheckAndTrimString(query.UsernameToMatch, "query.UsernameToMatch", false, 0x100);
            }
            return this.GetCountOfUserState(path, defaultInactiveSinceDate, username);
        }

        private int GetCountOfUserState(string path, DateTime inactiveSinceDate, string username)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int num = 0;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_GetCountOfState", connection);
                    this.SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;
                    parameters.Add(new SqlParameter("Count", SqlDbType.Int)).Direction = ParameterDirection.Output;
                    parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit)).Value = false;
                    parameters.AddWithValue("ApplicationName", this.ApplicationName);
                    SqlParameter parameter = parameters.Add("Path", SqlDbType.NVarChar);
                    if (path != null)
                    {
                        parameter.Value = path;
                    }
                    parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                    if (username != null)
                    {
                        parameter.Value = username;
                    }
                    parameter = parameters.Add("InactiveSinceDate", SqlDbType.DateTime);
                    if (inactiveSinceDate != PersonalizationAdministration.DefaultInactiveSinceDate)
                    {
                        parameter.Value = inactiveSinceDate.ToUniversalTime();
                    }
                    command.ExecuteNonQuery();
                    parameter = command.Parameters[0];
                    if (((parameter != null) && (parameter.Value != null)) && (parameter.Value is int))
                    {
                        num = (int) parameter.Value;
                    }
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        public override void Initialize(string name, NameValueCollection configSettings)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
            if (configSettings == null)
            {
                throw new ArgumentNullException("configSettings");
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "SqlPersonalizationProvider";
            }
            if (string.IsNullOrEmpty(configSettings["description"]))
            {
                configSettings.Remove("description");
                configSettings.Add("description", System.Web.SR.GetString("SqlPersonalizationProvider_Description"));
            }
            base.Initialize(name, configSettings);
            this._SchemaVersionCheck = 0;
            this._applicationName = configSettings["applicationName"];
            if (this._applicationName != null)
            {
                configSettings.Remove("applicationName");
                if (this._applicationName.Length > 0x100)
                {
                    object[] args = new object[] { 0x100.ToString(CultureInfo.CurrentCulture) };
                    throw new ProviderException(System.Web.SR.GetString("PersonalizationProvider_ApplicationNameExceedMaxLength", args));
                }
            }
            string str = configSettings["connectionStringName"];
            if (string.IsNullOrEmpty(str))
            {
                throw new ProviderException(System.Web.SR.GetString("PersonalizationProvider_NoConnection"));
            }
            configSettings.Remove("connectionStringName");
            string str2 = SqlConnectionHelper.GetConnectionString(str, true, true);
            if (string.IsNullOrEmpty(str2))
            {
                throw new ProviderException(System.Web.SR.GetString("PersonalizationProvider_BadConnection", new object[] { str }));
            }
            this._connectionString = str2;
            this._commandTimeout = SecUtility.GetIntValue(configSettings, "commandTimeout", -1, true, 0);
            configSettings.Remove("commandTimeout");
            if (configSettings.Count > 0)
            {
                string key = configSettings.GetKey(0);
                throw new ProviderException(System.Web.SR.GetString("PersonalizationProvider_UnknownProp", new object[] { key, name }));
            }
        }

        private byte[] LoadPersonalizationBlob(SqlConnection connection, string path, string userName)
        {
            SqlCommand command;
            if (userName != null)
            {
                command = new SqlCommand("dbo.aspnet_PersonalizationPerUser_GetPageSettings", connection);
            }
            else
            {
                command = new SqlCommand("dbo.aspnet_PersonalizationAllUsers_GetPageSettings", connection);
            }
            this.SetCommandTypeAndTimeout(command);
            command.Parameters.Add(this.CreateParameter("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
            command.Parameters.Add(this.CreateParameter("@Path", SqlDbType.NVarChar, path));
            if (userName != null)
            {
                command.Parameters.Add(this.CreateParameter("@UserName", SqlDbType.NVarChar, userName));
                command.Parameters.Add(this.CreateParameter("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
            }
            SqlDataReader reader = null;
            try
            {
                reader = command.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    int length = (int) reader.GetBytes(0, 0L, null, 0, 0);
                    byte[] buffer = new byte[length];
                    reader.GetBytes(0, 0L, buffer, 0, length);
                    return buffer;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return null;
        }

        protected override void LoadPersonalizationBlobs(WebPartManager webPartManager, string path, string userName, ref byte[] sharedDataBlob, ref byte[] userDataBlob)
        {
            sharedDataBlob = null;
            userDataBlob = null;
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    sharedDataBlob = this.LoadPersonalizationBlob(connection, path, null);
                    if (!string.IsNullOrEmpty(userName))
                    {
                        userDataBlob = this.LoadPersonalizationBlob(connection, path, userName);
                    }
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private int ResetAllState(PersonalizationScope scope)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int num = 0;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_DeleteAllState", connection);
                    this.SetCommandTypeAndTimeout(command);
                    SqlParameterCollection parameters = command.Parameters;
                    parameters.Add(new SqlParameter("AllUsersScope", SqlDbType.Bit)).Value = scope == PersonalizationScope.Shared;
                    parameters.AddWithValue("ApplicationName", this.ApplicationName);
                    parameters.Add(new SqlParameter("Count", SqlDbType.Int)).Direction = ParameterDirection.Output;
                    command.ExecuteNonQuery();
                    SqlParameter parameter = command.Parameters[2];
                    if (((parameter != null) && (parameter.Value != null)) && (parameter.Value is int))
                    {
                        num = (int) parameter.Value;
                    }
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        protected override void ResetPersonalizationBlob(WebPartManager webPartManager, string path, string userName)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    this.ResetPersonalizationState(connection, path, userName);
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void ResetPersonalizationState(SqlConnection connection, string path, string userName)
        {
            SqlCommand command;
            if (userName != null)
            {
                command = new SqlCommand("dbo.aspnet_PersonalizationPerUser_ResetPageSettings", connection);
            }
            else
            {
                command = new SqlCommand("dbo.aspnet_PersonalizationAllUsers_ResetPageSettings", connection);
            }
            this.SetCommandTypeAndTimeout(command);
            command.Parameters.Add(this.CreateParameter("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
            command.Parameters.Add(this.CreateParameter("@Path", SqlDbType.NVarChar, path));
            if (userName != null)
            {
                command.Parameters.Add(this.CreateParameter("@UserName", SqlDbType.NVarChar, userName));
                command.Parameters.Add(this.CreateParameter("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
            }
            command.ExecuteNonQuery();
        }

        private int ResetSharedState(string[] paths)
        {
            int num = 0;
            if (paths == null)
            {
                return this.ResetAllState(PersonalizationScope.Shared);
            }
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            try
            {
                bool flag = false;
                try
                {
                    try
                    {
                        connectionHolder = this.GetConnectionHolder();
                        connection = connectionHolder.Connection;
                        this.CheckSchemaVersion(connection);
                        SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_ResetSharedState", connection);
                        this.SetCommandTypeAndTimeout(command);
                        SqlParameterCollection parameters = command.Parameters;
                        parameters.Add(new SqlParameter("Count", SqlDbType.Int)).Direction = ParameterDirection.Output;
                        parameters.AddWithValue("ApplicationName", this.ApplicationName);
                        SqlParameter parameter = parameters.Add("Path", SqlDbType.NVarChar);
                        foreach (string str in paths)
                        {
                            if (!flag && (paths.Length > 1))
                            {
                                new SqlCommand("BEGIN TRANSACTION", connection).ExecuteNonQuery();
                                flag = true;
                            }
                            parameter.Value = str;
                            command.ExecuteNonQuery();
                            SqlParameter parameter2 = command.Parameters[0];
                            if (((parameter2 != null) && (parameter2.Value != null)) && (parameter2.Value is int))
                            {
                                num += (int) parameter2.Value;
                            }
                        }
                        if (flag)
                        {
                            new SqlCommand("COMMIT TRANSACTION", connection).ExecuteNonQuery();
                            flag = false;
                        }
                    }
                    catch
                    {
                        if (flag)
                        {
                            new SqlCommand("ROLLBACK TRANSACTION", connection).ExecuteNonQuery();
                            flag = false;
                        }
                        throw;
                    }
                    return num;
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        public override int ResetState(PersonalizationScope scope, string[] paths, string[] usernames)
        {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);
            paths = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(paths, "paths", false, false, 0x100);
            usernames = PersonalizationProviderHelper.CheckAndTrimNonEmptyStringEntries(usernames, "usernames", false, true, 0x100);
            if (scope == PersonalizationScope.Shared)
            {
                PersonalizationProviderHelper.CheckUsernamesInSharedScope(usernames);
                return this.ResetSharedState(paths);
            }
            PersonalizationProviderHelper.CheckOnlyOnePathWithUsers(paths, usernames);
            return this.ResetUserState(paths, usernames);
        }

        public override int ResetUserState(string path, DateTime userInactiveSinceDate)
        {
            path = StringUtil.CheckAndTrimString(path, "path", false, 0x100);
            string[] paths = (path == null) ? null : new string[] { path };
            return this.ResetUserState(ResetUserStateMode.PerInactiveDate, userInactiveSinceDate, paths, null);
        }

        private int ResetUserState(string[] paths, string[] usernames)
        {
            bool flag = (paths != null) && (paths.Length != 0);
            bool flag2 = (usernames != null) && (usernames.Length != 0);
            if (!flag && !flag2)
            {
                return this.ResetAllState(PersonalizationScope.User);
            }
            if (!flag2)
            {
                return this.ResetUserState(ResetUserStateMode.PerPaths, PersonalizationAdministration.DefaultInactiveSinceDate, paths, usernames);
            }
            return this.ResetUserState(ResetUserStateMode.PerUsers, PersonalizationAdministration.DefaultInactiveSinceDate, paths, usernames);
        }

        private int ResetUserState(ResetUserStateMode mode, DateTime userInactiveSinceDate, string[] paths, string[] usernames)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            int num = 0;
            try
            {
                bool flag = false;
                try
                {
                    try
                    {
                        connectionHolder = this.GetConnectionHolder();
                        connection = connectionHolder.Connection;
                        this.CheckSchemaVersion(connection);
                        SqlCommand command = new SqlCommand("dbo.aspnet_PersonalizationAdministration_ResetUserState", connection);
                        this.SetCommandTypeAndTimeout(command);
                        SqlParameterCollection parameters = command.Parameters;
                        parameters.Add(new SqlParameter("Count", SqlDbType.Int)).Direction = ParameterDirection.Output;
                        parameters.AddWithValue("ApplicationName", this.ApplicationName);
                        string str = ((paths != null) && (paths.Length > 0)) ? paths[0] : null;
                        if (mode == ResetUserStateMode.PerInactiveDate)
                        {
                            if (userInactiveSinceDate != PersonalizationAdministration.DefaultInactiveSinceDate)
                            {
                                parameters.Add("InactiveSinceDate", SqlDbType.DateTime).Value = userInactiveSinceDate.ToUniversalTime();
                            }
                            if (str != null)
                            {
                                parameters.AddWithValue("Path", str);
                            }
                            command.ExecuteNonQuery();
                            SqlParameter parameter2 = command.Parameters[0];
                            if (((parameter2 != null) && (parameter2.Value != null)) && (parameter2.Value is int))
                            {
                                num = (int) parameter2.Value;
                            }
                        }
                        else
                        {
                            SqlParameter parameter;
                            if (mode == ResetUserStateMode.PerPaths)
                            {
                                parameter = parameters.Add("Path", SqlDbType.NVarChar);
                                foreach (string str2 in paths)
                                {
                                    if (!flag && (paths.Length > 1))
                                    {
                                        new SqlCommand("BEGIN TRANSACTION", connection).ExecuteNonQuery();
                                        flag = true;
                                    }
                                    parameter.Value = str2;
                                    command.ExecuteNonQuery();
                                    SqlParameter parameter3 = command.Parameters[0];
                                    if (((parameter3 != null) && (parameter3.Value != null)) && (parameter3.Value is int))
                                    {
                                        num += (int) parameter3.Value;
                                    }
                                }
                            }
                            else
                            {
                                if (str != null)
                                {
                                    parameters.AddWithValue("Path", str);
                                }
                                parameter = parameters.Add("UserName", SqlDbType.NVarChar);
                                foreach (string str3 in usernames)
                                {
                                    if (!flag && (usernames.Length > 1))
                                    {
                                        new SqlCommand("BEGIN TRANSACTION", connection).ExecuteNonQuery();
                                        flag = true;
                                    }
                                    parameter.Value = str3;
                                    command.ExecuteNonQuery();
                                    SqlParameter parameter4 = command.Parameters[0];
                                    if (((parameter4 != null) && (parameter4.Value != null)) && (parameter4.Value is int))
                                    {
                                        num += (int) parameter4.Value;
                                    }
                                }
                            }
                        }
                        if (flag)
                        {
                            new SqlCommand("COMMIT TRANSACTION", connection).ExecuteNonQuery();
                            flag = false;
                        }
                    }
                    catch
                    {
                        if (flag)
                        {
                            new SqlCommand("ROLLBACK TRANSACTION", connection).ExecuteNonQuery();
                            flag = false;
                        }
                        throw;
                    }
                    return num;
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        protected override void SavePersonalizationBlob(WebPartManager webPartManager, string path, string userName, byte[] dataBlob)
        {
            SqlConnectionHolder connectionHolder = null;
            SqlConnection connection = null;
            try
            {
                try
                {
                    connectionHolder = this.GetConnectionHolder();
                    connection = connectionHolder.Connection;
                    this.CheckSchemaVersion(connection);
                    this.SavePersonalizationState(connection, path, userName, dataBlob);
                }
                catch (SqlException exception)
                {
                    if ((userName == null) || (((exception.Number != 0xa43) && (exception.Number != 0xa29)) && (exception.Number != 0x9d0)))
                    {
                        throw;
                    }
                    this.SavePersonalizationState(connection, path, userName, dataBlob);
                }
                finally
                {
                    if (connectionHolder != null)
                    {
                        connectionHolder.Close();
                        connectionHolder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void SavePersonalizationState(SqlConnection connection, string path, string userName, byte[] state)
        {
            SqlCommand command;
            if (userName != null)
            {
                command = new SqlCommand("dbo.aspnet_PersonalizationPerUser_SetPageSettings", connection);
            }
            else
            {
                command = new SqlCommand("dbo.aspnet_PersonalizationAllUsers_SetPageSettings", connection);
            }
            this.SetCommandTypeAndTimeout(command);
            command.Parameters.Add(this.CreateParameter("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
            command.Parameters.Add(this.CreateParameter("@Path", SqlDbType.NVarChar, path));
            command.Parameters.Add(this.CreateParameter("@PageSettings", SqlDbType.Image, state));
            command.Parameters.Add(this.CreateParameter("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
            if (userName != null)
            {
                command.Parameters.Add(this.CreateParameter("@UserName", SqlDbType.NVarChar, userName));
            }
            command.ExecuteNonQuery();
        }

        private void SetCommandTypeAndTimeout(SqlCommand command)
        {
            command.CommandType = CommandType.StoredProcedure;
            if (this._commandTimeout != -1)
            {
                command.CommandTimeout = this._commandTimeout;
            }
        }

        public override string ApplicationName
        {
            get
            {
                if (string.IsNullOrEmpty(this._applicationName))
                {
                    this._applicationName = SecUtility.GetDefaultAppName();
                }
                return this._applicationName;
            }
            set
            {
                if ((value != null) && (value.Length > 0x100))
                {
                    object[] args = new object[] { 0x100.ToString(CultureInfo.CurrentCulture) };
                    throw new ProviderException(System.Web.SR.GetString("PersonalizationProvider_ApplicationNameExceedMaxLength", args));
                }
                this._applicationName = value;
            }
        }

        private enum ResetUserStateMode
        {
            PerInactiveDate,
            PerPaths,
            PerUsers
        }
    }
}


namespace System.Web.Profile
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.DataAccess;
    using System.Web.Hosting;
    using System.Web.Util;

    public class SqlProfileProvider : ProfileProvider
    {
        private string _AppName;
        private int _CommandTimeout;
        private int _SchemaVersionCheck;
        private string _sqlConnectionString;

        private void CheckSchemaVersion(SqlConnection connection)
        {
            string[] features = new string[] { "Profile" };
            string version = "1";
            SecUtility.CheckSchemaVersion(this, connection, features, version, ref this._SchemaVersionCheck);
        }

        private SqlParameter CreateInputParam(string paramName, SqlDbType dbType, object objValue)
        {
            SqlParameter parameter = new SqlParameter(paramName, dbType);
            if (objValue == null)
            {
                objValue = string.Empty;
            }
            parameter.Value = objValue;
            return parameter;
        }

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int num;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(this._sqlConnectionString, true);
                    this.CheckSchemaVersion(connection.Connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_Profile_DeleteInactiveProfiles", connection.Connection) {
                        CommandTimeout = this.CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(this.CreateInputParam("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
                    command.Parameters.Add(this.CreateInputParam("@ProfileAuthOptions", SqlDbType.Int, (int) authenticationOption));
                    command.Parameters.Add(this.CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime()));
                    object obj2 = command.ExecuteScalar();
                    if ((obj2 == null) || !(obj2 is int))
                    {
                        return 0;
                    }
                    num = (int) obj2;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException("profiles");
            }
            if (profiles.Count < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("Parameter_collection_empty", new object[] { "profiles" }), "profiles");
            }
            string[] usernames = new string[profiles.Count];
            int num = 0;
            foreach (ProfileInfo info in profiles)
            {
                usernames[num++] = info.UserName;
            }
            return this.DeleteProfiles(usernames);
        }

        public override int DeleteProfiles(string[] usernames)
        {
            SecUtility.CheckArrayParameter(ref usernames, true, true, true, 0x100, "usernames");
            int num = 0;
            bool flag = false;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    try
                    {
                        connection = SqlConnectionHelper.GetConnection(this._sqlConnectionString, true);
                        this.CheckSchemaVersion(connection.Connection);
                        int length = usernames.Length;
                        while (length > 0)
                        {
                            SqlCommand command;
                            string objValue = usernames[usernames.Length - length];
                            length--;
                            for (int i = usernames.Length - length; i < usernames.Length; i++)
                            {
                                if (((objValue.Length + usernames[i].Length) + 1) >= 0xfa0)
                                {
                                    break;
                                }
                                objValue = objValue + "," + usernames[i];
                                length--;
                            }
                            if (!flag && (length > 0))
                            {
                                command = new SqlCommand("BEGIN TRANSACTION", connection.Connection);
                                command.ExecuteNonQuery();
                                flag = true;
                            }
                            command = new SqlCommand("dbo.aspnet_Profile_DeleteProfiles", connection.Connection) {
                                CommandTimeout = this.CommandTimeout,
                                CommandType = CommandType.StoredProcedure
                            };
                            command.Parameters.Add(this.CreateInputParam("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
                            command.Parameters.Add(this.CreateInputParam("@UserNames", SqlDbType.NVarChar, objValue));
                            object obj2 = command.ExecuteScalar();
                            if ((obj2 != null) && (obj2 is int))
                            {
                                num += (int) obj2;
                            }
                        }
                        if (flag)
                        {
                            new SqlCommand("COMMIT TRANSACTION", connection.Connection).ExecuteNonQuery();
                            flag = false;
                        }
                    }
                    catch
                    {
                        if (flag)
                        {
                            new SqlCommand("ROLLBACK TRANSACTION", connection.Connection).ExecuteNonQuery();
                            flag = false;
                        }
                        throw;
                    }
                    return num;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0x100, "username");
            SqlParameter[] args = new SqlParameter[] { this.CreateInputParam("@UserNameToMatch", SqlDbType.NVarChar, usernameToMatch), this.CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime()) };
            return this.GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0x100, "username");
            SqlParameter[] args = new SqlParameter[] { this.CreateInputParam("@UserNameToMatch", SqlDbType.NVarChar, usernameToMatch) };
            return this.GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            SqlParameter[] args = new SqlParameter[] { this.CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime()) };
            return this.GetProfilesForQuery(args, authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            return this.GetProfilesForQuery(new SqlParameter[0], authenticationOption, pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int num;
            try
            {
                SqlConnectionHolder connection = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(this._sqlConnectionString, true);
                    this.CheckSchemaVersion(connection.Connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_Profile_GetNumberOfInactiveProfiles", connection.Connection) {
                        CommandTimeout = this.CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(this.CreateInputParam("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
                    command.Parameters.Add(this.CreateInputParam("@ProfileAuthOptions", SqlDbType.Int, (int) authenticationOption));
                    command.Parameters.Add(this.CreateInputParam("@InactiveSinceDate", SqlDbType.DateTime, userInactiveSinceDate.ToUniversalTime()));
                    object obj2 = command.ExecuteScalar();
                    if ((obj2 == null) || !(obj2 is int))
                    {
                        return 0;
                    }
                    num = (int) obj2;
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        private ProfileInfoCollection GetProfilesForQuery(SqlParameter[] args, ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            ProfileInfoCollection infos2;
            if (pageIndex < 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_bad"), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageSize_bad"), "pageSize");
            }
            long num = ((pageIndex * pageSize) + pageSize) - 1L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentException(System.Web.SR.GetString("PageIndex_PageSize_bad"), "pageIndex and pageSize");
            }
            try
            {
                SqlConnectionHolder connection = null;
                SqlDataReader reader = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(this._sqlConnectionString, true);
                    this.CheckSchemaVersion(connection.Connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_Profile_GetProfiles", connection.Connection) {
                        CommandTimeout = this.CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(this.CreateInputParam("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
                    command.Parameters.Add(this.CreateInputParam("@ProfileAuthOptions", SqlDbType.Int, (int) authenticationOption));
                    command.Parameters.Add(this.CreateInputParam("@PageIndex", SqlDbType.Int, pageIndex));
                    command.Parameters.Add(this.CreateInputParam("@PageSize", SqlDbType.Int, pageSize));
                    foreach (SqlParameter parameter in args)
                    {
                        command.Parameters.Add(parameter);
                    }
                    reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                    ProfileInfoCollection infos = new ProfileInfoCollection();
                    while (reader.Read())
                    {
                        string username = reader.GetString(0);
                        bool boolean = reader.GetBoolean(1);
                        DateTime lastActivityDate = DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc);
                        DateTime lastUpdatedDate = DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc);
                        int size = reader.GetInt32(4);
                        infos.Add(new ProfileInfo(username, boolean, lastActivityDate, lastUpdatedDate, size));
                    }
                    totalRecords = infos.Count;
                    if (reader.NextResult() && reader.Read())
                    {
                        totalRecords = reader.GetInt32(0);
                    }
                    infos2 = infos;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return infos2;
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sc, SettingsPropertyCollection properties)
        {
            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();
            if (properties.Count >= 1)
            {
                string str = (string) sc["UserName"];
                foreach (SettingsProperty property in properties)
                {
                    if (property.SerializeAs == SettingsSerializeAs.ProviderSpecific)
                    {
                        if (property.PropertyType.IsPrimitive || (property.PropertyType == typeof(string)))
                        {
                            property.SerializeAs = SettingsSerializeAs.String;
                        }
                        else
                        {
                            property.SerializeAs = SettingsSerializeAs.Xml;
                        }
                    }
                    svc.Add(new SettingsPropertyValue(property));
                }
                if (!string.IsNullOrEmpty(str))
                {
                    this.GetPropertyValuesFromDatabase(str, svc);
                }
            }
            return svc;
        }

        private void GetPropertyValuesFromDatabase(string userName, SettingsPropertyValueCollection svc)
        {
            if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_PROFILE_BEGIN, HttpContext.Current.WorkerRequest);
            }
            HttpContext current = HttpContext.Current;
            string[] names = null;
            string values = null;
            byte[] buffer = null;
            if (current != null)
            {
                if (!current.Request.IsAuthenticated)
                {
                    string anonymousID = current.Request.AnonymousID;
                }
                else
                {
                    string name = current.User.Identity.Name;
                }
            }
            try
            {
                SqlConnectionHolder connection = null;
                SqlDataReader reader = null;
                try
                {
                    connection = SqlConnectionHelper.GetConnection(this._sqlConnectionString, true);
                    this.CheckSchemaVersion(connection.Connection);
                    SqlCommand command = new SqlCommand("dbo.aspnet_Profile_GetProperties", connection.Connection) {
                        CommandTimeout = this.CommandTimeout,
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.Add(this.CreateInputParam("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
                    command.Parameters.Add(this.CreateInputParam("@UserName", SqlDbType.NVarChar, userName));
                    command.Parameters.Add(this.CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                    reader = command.ExecuteReader(CommandBehavior.SingleRow);
                    if (reader.Read())
                    {
                        names = reader.GetString(0).Split(new char[] { ':' });
                        values = reader.GetString(1);
                        int length = (int) reader.GetBytes(2, 0L, null, 0, 0);
                        buffer = new byte[length];
                        reader.GetBytes(2, 0L, buffer, 0, length);
                    }
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                ProfileModule.ParseDataFromDB(names, values, buffer, svc);
                if (HostingEnvironment.IsHosted && EtwTrace.IsTraceEnabled(4, 8))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_PROFILE_END, HttpContext.Current.WorkerRequest, userName);
                }
            }
            catch
            {
                throw;
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if ((name == null) || (name.Length < 1))
            {
                name = "SqlProfileProvider";
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", System.Web.SR.GetString("ProfileSqlProvider_description"));
            }
            base.Initialize(name, config);
            this._SchemaVersionCheck = 0;
            this._sqlConnectionString = SecUtility.GetConnectionString(config);
            this._AppName = config["applicationName"];
            if (string.IsNullOrEmpty(this._AppName))
            {
                this._AppName = SecUtility.GetDefaultAppName();
            }
            if (this._AppName.Length > 0x100)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_application_name_too_long"));
            }
            this._CommandTimeout = SecUtility.GetIntValue(config, "commandTimeout", 30, true, 0);
            config.Remove("commandTimeout");
            config.Remove("connectionStringName");
            config.Remove("connectionString");
            config.Remove("applicationName");
            if (config.Count > 0)
            {
                string key = config.GetKey(0);
                if (!string.IsNullOrEmpty(key))
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_unrecognized_attribute", new object[] { key }));
                }
            }
        }

        public override void SetPropertyValues(SettingsContext sc, SettingsPropertyValueCollection properties)
        {
            string objValue = (string) sc["UserName"];
            bool userIsAuthenticated = (bool) sc["IsAuthenticated"];
            if (((objValue != null) && (objValue.Length >= 1)) && (properties.Count >= 1))
            {
                string allNames = string.Empty;
                string allValues = string.Empty;
                byte[] buf = null;
                ProfileModule.PrepareDataForSaving(ref allNames, ref allValues, ref buf, true, properties, userIsAuthenticated);
                if (allNames.Length != 0)
                {
                    try
                    {
                        SqlConnectionHolder connection = null;
                        try
                        {
                            connection = SqlConnectionHelper.GetConnection(this._sqlConnectionString, true);
                            this.CheckSchemaVersion(connection.Connection);
                            SqlCommand command = new SqlCommand("dbo.aspnet_Profile_SetProperties", connection.Connection) {
                                CommandTimeout = this.CommandTimeout,
                                CommandType = CommandType.StoredProcedure
                            };
                            command.Parameters.Add(this.CreateInputParam("@ApplicationName", SqlDbType.NVarChar, this.ApplicationName));
                            command.Parameters.Add(this.CreateInputParam("@UserName", SqlDbType.NVarChar, objValue));
                            command.Parameters.Add(this.CreateInputParam("@PropertyNames", SqlDbType.NText, allNames));
                            command.Parameters.Add(this.CreateInputParam("@PropertyValuesString", SqlDbType.NText, allValues));
                            command.Parameters.Add(this.CreateInputParam("@PropertyValuesBinary", SqlDbType.Image, buf));
                            command.Parameters.Add(this.CreateInputParam("@IsUserAnonymous", SqlDbType.Bit, !userIsAuthenticated));
                            command.Parameters.Add(this.CreateInputParam("@CurrentTimeUtc", SqlDbType.DateTime, DateTime.UtcNow));
                            command.ExecuteNonQuery();
                        }
                        finally
                        {
                            if (connection != null)
                            {
                                connection.Close();
                                connection = null;
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        public override string ApplicationName
        {
            get
            {
                return this._AppName;
            }
            set
            {
                if (value.Length > 0x100)
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_application_name_too_long"));
                }
                this._AppName = value;
            }
        }

        private int CommandTimeout
        {
            get
            {
                return this._CommandTimeout;
            }
        }
    }
}


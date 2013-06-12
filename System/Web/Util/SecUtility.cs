namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.Web;
    using System.Web.DataAccess;
    using System.Web.Hosting;

    internal static class SecUtility
    {
        internal static void CheckArrayParameter(ref string[] param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (param.Length < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("Parameter_array_empty", new object[] { paramName }), paramName);
            }
            Hashtable hashtable = new Hashtable(param.Length);
            for (int i = param.Length - 1; i >= 0; i--)
            {
                CheckParameter(ref param[i], checkForNull, checkIfEmpty, checkForCommas, maxSize, paramName + "[ " + i.ToString(CultureInfo.InvariantCulture) + " ]");
                if (hashtable.Contains(param[i]))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_duplicate_array_element", new object[] { paramName }), paramName);
                }
                hashtable.Add(param[i], param[i]);
            }
        }

        internal static void CheckParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize, string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                {
                    throw new ArgumentNullException(paramName);
                }
            }
            else
            {
                param = param.Trim();
                if (checkIfEmpty && (param.Length < 1))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_can_not_be_empty", new object[] { paramName }), paramName);
                }
                if ((maxSize > 0) && (param.Length > maxSize))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_too_long", new object[] { paramName, maxSize.ToString(CultureInfo.InvariantCulture) }), paramName);
                }
                if (checkForCommas && param.Contains(","))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Parameter_can_not_contain_comma", new object[] { paramName }), paramName);
                }
            }
        }

        internal static void CheckPasswordParameter(ref string param, int maxSize, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (param.Length < 1)
            {
                throw new ArgumentException(System.Web.SR.GetString("Parameter_can_not_be_empty", new object[] { paramName }), paramName);
            }
            if ((maxSize > 0) && (param.Length > maxSize))
            {
                throw new ArgumentException(System.Web.SR.GetString("Parameter_too_long", new object[] { paramName, maxSize.ToString(CultureInfo.InvariantCulture) }), paramName);
            }
        }

        internal static void CheckSchemaVersion(ProviderBase provider, SqlConnection connection, string[] features, string version, ref int schemaVersionCheck)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (features == null)
            {
                throw new ArgumentNullException("features");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (schemaVersionCheck == -1)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_Schema_Version_Not_Match", new object[] { provider.ToString(), version }));
            }
            if (schemaVersionCheck == 0)
            {
                lock (provider)
                {
                    if (schemaVersionCheck == -1)
                    {
                        throw new ProviderException(System.Web.SR.GetString("Provider_Schema_Version_Not_Match", new object[] { provider.ToString(), version }));
                    }
                    if (schemaVersionCheck == 0)
                    {
                        SqlCommand command = null;
                        SqlParameter parameter = null;
                        foreach (string str in features)
                        {
                            command = new SqlCommand("dbo.aspnet_CheckSchemaVersion", connection) {
                                CommandType = CommandType.StoredProcedure
                            };
                            parameter = new SqlParameter("@Feature", str);
                            command.Parameters.Add(parameter);
                            parameter = new SqlParameter("@CompatibleSchemaVersion", version);
                            command.Parameters.Add(parameter);
                            parameter = new SqlParameter("@ReturnValue", SqlDbType.Int) {
                                Direction = ParameterDirection.ReturnValue
                            };
                            command.Parameters.Add(parameter);
                            command.ExecuteNonQuery();
                            if (((parameter.Value != null) ? ((int) parameter.Value) : -1) != 0)
                            {
                                schemaVersionCheck = -1;
                                throw new ProviderException(System.Web.SR.GetString("Provider_Schema_Version_Not_Match", new object[] { provider.ToString(), version }));
                            }
                        }
                        schemaVersionCheck = 1;
                    }
                }
            }
        }

        internal static bool GetBooleanValue(NameValueCollection config, string valueName, bool defaultValue)
        {
            bool flag;
            string str = config[valueName];
            if (str == null)
            {
                return defaultValue;
            }
            if (!bool.TryParse(str, out flag))
            {
                throw new ProviderException(System.Web.SR.GetString("Value_must_be_boolean", new object[] { valueName }));
            }
            return flag;
        }

        internal static string GetConnectionString(NameValueCollection config)
        {
            string str = config["connectionString"];
            if (string.IsNullOrEmpty(str))
            {
                string str2 = config["connectionStringName"];
                if (string.IsNullOrEmpty(str2))
                {
                    throw new ProviderException(System.Web.SR.GetString("Connection_name_not_specified"));
                }
                bool lookupConnectionString = true;
                bool appLevel = true;
                str = SqlConnectionHelper.GetConnectionString(str2, lookupConnectionString, appLevel);
                if (string.IsNullOrEmpty(str))
                {
                    throw new ProviderException(System.Web.SR.GetString("Connection_string_not_found", new object[] { str2 }));
                }
            }
            return str;
        }

        internal static string GetDefaultAppName()
        {
            try
            {
                string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                if (string.IsNullOrEmpty(applicationVirtualPath))
                {
                    applicationVirtualPath = Process.GetCurrentProcess().MainModule.ModuleName;
                    int index = applicationVirtualPath.IndexOf('.');
                    if (index != -1)
                    {
                        applicationVirtualPath = applicationVirtualPath.Remove(index);
                    }
                }
                if (string.IsNullOrEmpty(applicationVirtualPath))
                {
                    return "/";
                }
                return applicationVirtualPath;
            }
            catch
            {
                return "/";
            }
        }

        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            int num;
            string s = config[valueName];
            if (s == null)
            {
                return defaultValue;
            }
            if (!int.TryParse(s, out num))
            {
                if (zeroAllowed)
                {
                    throw new ProviderException(System.Web.SR.GetString("Value_must_be_non_negative_integer", new object[] { valueName }));
                }
                throw new ProviderException(System.Web.SR.GetString("Value_must_be_positive_integer", new object[] { valueName }));
            }
            if (zeroAllowed && (num < 0))
            {
                throw new ProviderException(System.Web.SR.GetString("Value_must_be_non_negative_integer", new object[] { valueName }));
            }
            if (!zeroAllowed && (num <= 0))
            {
                throw new ProviderException(System.Web.SR.GetString("Value_must_be_positive_integer", new object[] { valueName }));
            }
            if ((maxValueAllowed > 0) && (num > maxValueAllowed))
            {
                throw new ProviderException(System.Web.SR.GetString("Value_too_big", new object[] { valueName, maxValueAllowed.ToString(CultureInfo.InvariantCulture) }));
            }
            return num;
        }

        internal static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }
            param = param.Trim();
            return (((!checkIfEmpty || (param.Length >= 1)) && ((maxSize <= 0) || (param.Length <= maxSize))) && (!checkForCommas || !param.Contains(",")));
        }

        internal static bool ValidatePasswordParameter(ref string param, int maxSize)
        {
            if (param == null)
            {
                return false;
            }
            if (param.Length < 1)
            {
                return false;
            }
            if ((maxSize > 0) && (param.Length > maxSize))
            {
                return false;
            }
            return true;
        }
    }
}


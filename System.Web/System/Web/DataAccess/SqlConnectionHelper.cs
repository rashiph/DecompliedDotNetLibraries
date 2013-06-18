namespace System.Web.DataAccess
{
    using System;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;

    internal static class SqlConnectionHelper
    {
        private static object s_lock = new object();
        internal const string s_strDataDir = "DataDirectory";
        internal const string s_strSqlExprFileExt = ".MDF";
        internal const string s_strUpperDataDirWithToken = "|DATADIRECTORY|";
        internal const string s_strUpperUserInstance = "USER INSTANCE";

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static void CreateMdfFile(string fullFileName, string dataDir, string connectionString)
        {
            bool flag = false;
            string database = null;
            HttpContext current = HttpContext.Current;
            string dbFileName = null;
            try
            {
                if (!Directory.Exists(dataDir))
                {
                    flag = true;
                    Directory.CreateDirectory(dataDir);
                    flag = false;
                    try
                    {
                        if (current != null)
                        {
                            HttpRuntime.RestrictIISFolders(current);
                        }
                    }
                    catch
                    {
                    }
                }
                fullFileName = fullFileName.ToUpper(CultureInfo.InvariantCulture);
                char[] chArray = Path.GetFileNameWithoutExtension(fullFileName).ToCharArray();
                for (int i = 0; i < chArray.Length; i++)
                {
                    if (!char.IsLetterOrDigit(chArray[i]))
                    {
                        chArray[i] = '_';
                    }
                }
                string str3 = new string(chArray);
                if (str3.Length > 30)
                {
                    database = str3.Substring(0, 30) + "_" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                }
                else
                {
                    database = str3 + "_" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                }
                dbFileName = Path.Combine(Path.GetDirectoryName(fullFileName), str3 + "_TMP.MDF");
                SqlServices.Install(database, dbFileName, connectionString);
                DetachDB(database, connectionString);
                try
                {
                    File.Move(dbFileName, fullFileName);
                }
                catch
                {
                    if (!File.Exists(fullFileName))
                    {
                        File.Copy(dbFileName, fullFileName);
                        try
                        {
                            File.Delete(dbFileName);
                        }
                        catch
                        {
                        }
                    }
                }
                try
                {
                    File.Delete(dbFileName.Replace("_TMP.MDF", "_TMP_log.LDF"));
                }
                catch
                {
                }
            }
            catch (Exception exception)
            {
                if ((current == null) || current.IsCustomErrorEnabled)
                {
                    throw;
                }
                HttpException exception2 = new HttpException(exception.Message, exception);
                if (exception is UnauthorizedAccessException)
                {
                    exception2.SetFormatter(new SqlExpressConnectionErrorFormatter(flag ? DataConnectionErrorEnum.CanNotCreateDataDir : DataConnectionErrorEnum.CanNotWriteToDataDir));
                }
                else
                {
                    exception2.SetFormatter(new SqlExpressDBFileAutoCreationErrorFormatter(exception));
                }
                throw exception2;
            }
        }

        private static void DetachDB(string databaseName, string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                new SqlCommand("USE master", connection).ExecuteNonQuery();
                SqlCommand command = new SqlCommand("sp_detach_db", connection) {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@dbname", databaseName);
                command.Parameters.AddWithValue("@skipchecks", "true");
                command.ExecuteNonQuery();
            }
            catch
            {
            }
            finally
            {
                connection.Close();
            }
        }

        private static void EnsureSqlExpressDBFile(string connectionString)
        {
            string str = null;
            string path = null;
            string dataDirectory = GetDataDirectory();
            bool flag = true;
            bool flag2 = true;
            string[] strArray = connectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            bool flag3 = true;
            bool flag4 = true;
            foreach (string str4 in strArray)
            {
                string str5 = str4.ToUpper(CultureInfo.InvariantCulture).Trim();
                if (flag && str5.Contains("|DATADIRECTORY|"))
                {
                    flag = false;
                    connectionString = connectionString.Replace(str4, "Pooling=false");
                    int startIndex = str5.IndexOf("|DATADIRECTORY|", StringComparison.Ordinal) + "|DATADIRECTORY|".Length;
                    str = str5.Substring(startIndex).Trim();
                    while (str.StartsWith(@"\", StringComparison.Ordinal))
                    {
                        str = str.Substring(1);
                    }
                    if (str.Contains(".."))
                    {
                        str = null;
                    }
                    else
                    {
                        path = Path.Combine(dataDirectory, str);
                    }
                    if (flag2)
                    {
                        continue;
                    }
                    break;
                }
                if (flag2 && (str5.StartsWith("INITIAL CATALOG", StringComparison.Ordinal) || str5.StartsWith("DATABASE", StringComparison.Ordinal)))
                {
                    flag2 = false;
                    connectionString = connectionString.Replace(str4, "Database=master");
                    if (flag)
                    {
                        continue;
                    }
                    break;
                }
                if (flag3 && str5.StartsWith("USER INSTANCE", StringComparison.Ordinal))
                {
                    flag3 = false;
                    int index = str5.IndexOf('=');
                    if ((index >= 0) && !(str5.Substring(index + 1).Trim() != "TRUE"))
                    {
                        continue;
                    }
                    return;
                }
                if (flag4 && str5.StartsWith("CONNECT TIMEOUT", StringComparison.Ordinal))
                {
                    flag4 = false;
                }
            }
            if (!flag3)
            {
                if (path == null)
                {
                    throw new ProviderException(System.Web.SR.GetString("SqlExpress_file_not_found_in_connection_string"));
                }
                if (!File.Exists(path))
                {
                    if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.High))
                    {
                        throw new ProviderException(System.Web.SR.GetString("Provider_can_not_create_file_in_this_trust_level"));
                    }
                    if (!connectionString.Contains("Database=master"))
                    {
                        connectionString = connectionString + ";Database=master";
                    }
                    if (flag4)
                    {
                        connectionString = connectionString + ";Connect Timeout=45";
                    }
                    using (new ApplicationImpersonationContext())
                    {
                        lock (s_lock)
                        {
                            if (!File.Exists(path))
                            {
                                CreateMdfFile(path, dataDirectory, connectionString);
                            }
                        }
                    }
                }
            }
        }

        internal static SqlConnectionHolder GetConnection(string connectionString, bool revertImpersonation)
        {
            if (connectionString.ToUpperInvariant().Contains("|DATADIRECTORY|"))
            {
                EnsureSqlExpressDBFile(connectionString);
            }
            SqlConnectionHolder holder = new SqlConnectionHolder(connectionString);
            bool flag = true;
            try
            {
                try
                {
                    holder.Open(null, revertImpersonation);
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        holder.Close();
                        holder = null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return holder;
        }

        internal static string GetConnectionString(string specifiedConnectionString, bool lookupConnectionString, bool appLevel)
        {
            if ((specifiedConnectionString == null) || (specifiedConnectionString.Length < 1))
            {
                return null;
            }
            string connectionString = null;
            if (lookupConnectionString)
            {
                RuntimeConfig config = appLevel ? RuntimeConfig.GetAppConfig() : RuntimeConfig.GetConfig();
                ConnectionStringSettings settings = config.ConnectionStrings.ConnectionStrings[specifiedConnectionString];
                if (settings != null)
                {
                    connectionString = settings.ConnectionString;
                }
                if (connectionString != null)
                {
                    return connectionString;
                }
                return null;
            }
            return specifiedConnectionString;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal static string GetDataDirectory()
        {
            if (HostingEnvironment.IsHosted)
            {
                return Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data");
            }
            string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(data))
            {
                string directoryName = null;
                Process currentProcess = Process.GetCurrentProcess();
                ProcessModule module = (currentProcess != null) ? currentProcess.MainModule : null;
                string str3 = (module != null) ? module.FileName : null;
                if (!string.IsNullOrEmpty(str3))
                {
                    directoryName = Path.GetDirectoryName(str3);
                }
                if (string.IsNullOrEmpty(directoryName))
                {
                    directoryName = Environment.CurrentDirectory;
                }
                data = Path.Combine(directoryName, "App_Data");
                AppDomain.CurrentDomain.SetData("DataDirectory", data, new FileIOPermission(FileIOPermissionAccess.PathDiscovery, data));
            }
            return data;
        }
    }
}


namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    public sealed class SqlCacheDependency : CacheDependency
    {
        private int _sql7ChangeId;
        private DatabaseNotifState _sql7DatabaseState;
        private Sql7DependencyInfo _sql7DepInfo;
        private SqlDependency _sqlYukonDep;
        private string _uniqueID;
        internal static bool s_hasSqlClientPermission;
        internal static bool s_hasSqlClientPermissionInited;
        private const string SQL9_CACHE_DEPENDENCY_DIRECTIVE = "CommandNotification";
        internal const string SQL9_OUTPUT_CACHE_DEPENDENCY_COOKIE = "MS.SqlDependencyCookie";

        private SqlCacheDependency()
        {
            this.CreateSqlDep(null);
            this.InitUniqueID();
        }

        public SqlCacheDependency(SqlCommand sqlCmd)
        {
            HttpContext current = HttpContext.Current;
            if (sqlCmd == null)
            {
                throw new ArgumentNullException("sqlCmd");
            }
            if (((current != null) && (current.SqlDependencyCookie != null)) && sqlCmd.NotificationAutoEnlist)
            {
                throw new HttpException(System.Web.SR.GetString("SqlCacheDependency_OutputCache_Conflict"));
            }
            this.CreateSqlDep(sqlCmd);
            this.InitUniqueID();
        }

        public SqlCacheDependency(string databaseEntryName, string tableName) : base(0, null, new string[] { GetDependKey(databaseEntryName, tableName) })
        {
            this._sql7DatabaseState = SqlCacheDependencyManager.AddRef(databaseEntryName);
            this._sql7DepInfo._database = databaseEntryName;
            this._sql7DepInfo._table = tableName;
            object obj2 = HttpRuntime.CacheInternal[GetDependKey(databaseEntryName, tableName)];
            if (obj2 == null)
            {
                this._sql7ChangeId = -1;
            }
            else
            {
                this._sql7ChangeId = (int) obj2;
            }
            base.FinishInit();
            this.InitUniqueID();
        }

        private static void CheckPermission()
        {
            if (!s_hasSqlClientPermissionInited)
            {
                if (!HostingEnvironment.IsHosted)
                {
                    try
                    {
                        new SqlClientPermission(PermissionState.Unrestricted).Demand();
                        s_hasSqlClientPermission = true;
                    }
                    catch (SecurityException)
                    {
                    }
                }
                else
                {
                    s_hasSqlClientPermission = Permission.HasSqlClientPermission();
                }
                s_hasSqlClientPermissionInited = true;
            }
            if (!s_hasSqlClientPermission)
            {
                throw new HttpException(System.Web.SR.GetString("SqlCacheDependency_permission_denied"));
            }
        }

        public static CacheDependency CreateOutputCacheDependency(string dependency)
        {
            Sql7DependencyInfo info;
            if (dependency == null)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_sqlDependency_argument", new object[] { dependency }));
            }
            if (StringUtil.EqualsIgnoreCase(dependency, "CommandNotification"))
            {
                HttpContext current = HttpContext.Current;
                SqlCacheDependency dependency2 = new SqlCacheDependency();
                current.SqlDependencyCookie = dependency2._sqlYukonDep.Id;
                return dependency2;
            }
            AggregateCacheDependency dependency3 = null;
            ArrayList list = ParseSql7OutputCacheDependency(dependency);
            if (list.Count == 1)
            {
                info = (Sql7DependencyInfo) list[0];
                return CreateSql7SqlCacheDependencyForOutputCache(info._database, info._table, dependency);
            }
            dependency3 = new AggregateCacheDependency();
            for (int i = 0; i < list.Count; i++)
            {
                info = (Sql7DependencyInfo) list[i];
                dependency3.Add(new CacheDependency[] { CreateSql7SqlCacheDependencyForOutputCache(info._database, info._table, dependency) });
            }
            return dependency3;
        }

        private static SqlCacheDependency CreateSql7SqlCacheDependencyForOutputCache(string database, string table, string depString)
        {
            SqlCacheDependency dependency;
            try
            {
                dependency = new SqlCacheDependency(database, table);
            }
            catch (HttpException exception)
            {
                HttpException e = new HttpException(System.Web.SR.GetString("Invalid_sqlDependency_argument2", new object[] { depString, exception.Message }), exception);
                e.SetFormatter(new UseLastUnhandledErrorFormatter(e));
                throw e;
            }
            return dependency;
        }

        private void CreateSqlDep(SqlCommand sqlCmd)
        {
            this._sqlYukonDep = new SqlDependency();
            if (sqlCmd != null)
            {
                this._sqlYukonDep.AddCommandDependency(sqlCmd);
            }
            this._sqlYukonDep.OnChange += new OnChangeEventHandler(this.OnSQL9SqlDependencyChanged);
        }

        protected override void DependencyDispose()
        {
            if (this._sql7DatabaseState != null)
            {
                SqlCacheDependencyManager.Release(this._sql7DatabaseState);
            }
        }

        private static string GetDependKey(string database, string tableName)
        {
            CheckPermission();
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (tableName.Length == 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("Cache_null_table"));
            }
            string moniterKey = SqlCacheDependencyManager.GetMoniterKey(database, tableName);
            SqlCacheDependencyManager.EnsureTableIsRegisteredAndPolled(database, tableName);
            return moniterKey;
        }

        public override string GetUniqueID()
        {
            return this._uniqueID;
        }

        private void InitUniqueID()
        {
            if (this._sqlYukonDep != null)
            {
                this._uniqueID = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            }
            else if (this._sql7ChangeId == -1)
            {
                this._uniqueID = null;
            }
            else
            {
                this._uniqueID = this._sql7DepInfo._database + ":" + this._sql7DepInfo._table + ":" + this._sql7ChangeId.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void OnSQL9SqlDependencyChanged(object sender, SqlNotificationEventArgs e)
        {
            base.NotifyDependencyChanged(sender, e);
        }

        internal static ArrayList ParseSql7OutputCacheDependency(string outputCacheString)
        {
            ArrayList list2;
            bool flag = false;
            int startIndex = 0;
            int num2 = -1;
            string s = null;
            ArrayList list = null;
            try
            {
                for (int i = 0; i < (outputCacheString.Length + 1); i++)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else if ((i != outputCacheString.Length) && (outputCacheString[i] == '\\'))
                    {
                        flag = true;
                    }
                    else
                    {
                        int num3;
                        if ((i == outputCacheString.Length) || (outputCacheString[i] == ';'))
                        {
                            if (s == null)
                            {
                                throw new ArgumentException();
                            }
                            num3 = i - num2;
                            if (num3 == 0)
                            {
                                throw new ArgumentException();
                            }
                            Sql7DependencyInfo info = new Sql7DependencyInfo {
                                _database = VerifyAndRemoveEscapeCharacters(s),
                                _table = VerifyAndRemoveEscapeCharacters(outputCacheString.Substring(num2, num3))
                            };
                            if (list == null)
                            {
                                list = new ArrayList(1);
                            }
                            list.Add(info);
                            startIndex = i + 1;
                            s = null;
                        }
                        if (i == outputCacheString.Length)
                        {
                            break;
                        }
                        if (outputCacheString[i] == ':')
                        {
                            if (s != null)
                            {
                                throw new ArgumentException();
                            }
                            num3 = i - startIndex;
                            if (num3 == 0)
                            {
                                throw new ArgumentException();
                            }
                            s = outputCacheString.Substring(startIndex, num3);
                            num2 = i + 1;
                        }
                    }
                }
                list2 = list;
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_sqlDependency_argument", new object[] { outputCacheString }));
            }
            return list2;
        }

        internal static void ValidateOutputCacheDependencyString(string depString, bool page)
        {
            if (depString == null)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_sqlDependency_argument", new object[] { depString }));
            }
            if (StringUtil.EqualsIgnoreCase(depString, "CommandNotification"))
            {
                if (!page)
                {
                    throw new HttpException(System.Web.SR.GetString("Attrib_Sql9_not_allowed"));
                }
            }
            else
            {
                ParseSql7OutputCacheDependency(depString);
            }
        }

        private static string VerifyAndRemoveEscapeCharacters(string s)
        {
            bool flag = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (flag)
                {
                    if (((s[i] != '\\') && (s[i] != ':')) && (s[i] != ';'))
                    {
                        throw new ArgumentException();
                    }
                    flag = false;
                }
                else if (s[i] == '\\')
                {
                    if ((i + 1) == s.Length)
                    {
                        throw new ArgumentException();
                    }
                    flag = true;
                    s = s.Remove(i, 1);
                    i--;
                }
            }
            return s;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Sql7DependencyInfo
        {
            internal string _database;
            internal string _table;
        }
    }
}


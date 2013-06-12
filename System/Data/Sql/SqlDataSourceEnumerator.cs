namespace System.Data.Sql
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Text;

    public sealed class SqlDataSourceEnumerator : DbDataSourceEnumerator
    {
        private static string _Cluster = "Clustered:";
        private static int _clusterLength = _Cluster.Length;
        private static string _Version = "Version:";
        private static int _versionLength = _Version.Length;
        internal const string InstanceName = "InstanceName";
        internal const string IsClustered = "IsClustered";
        internal const string ServerName = "ServerName";
        private static readonly SqlDataSourceEnumerator SingletonInstance = new SqlDataSourceEnumerator();
        private const int timeoutSeconds = 30;
        private long timeoutTime;
        internal const string Version = "Version";

        private SqlDataSourceEnumerator()
        {
        }

        public override DataTable GetDataSources()
        {
            new NamedPermissionSet("FullTrust").Demand();
            char[] wStr = null;
            StringBuilder builder = new StringBuilder();
            int pcbBuf = 0x400;
            int charCount = 0;
            wStr = new char[pcbBuf];
            bool fMore = true;
            bool flag = false;
            IntPtr ptrZero = ADP.PtrZero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.timeoutTime = TdsParserStaticMethods.GetTimeoutSeconds(30);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    ptrZero = SNINativeMethodWrapper.SNIServerEnumOpen();
                }
                if (ADP.PtrZero != ptrZero)
                {
                    while (fMore && !TdsParserStaticMethods.TimeoutHasExpired(this.timeoutTime))
                    {
                        charCount = SNINativeMethodWrapper.SNIServerEnumRead(ptrZero, wStr, pcbBuf, ref fMore);
                        if (charCount > pcbBuf)
                        {
                            flag = true;
                            fMore = false;
                        }
                        else if (0 < charCount)
                        {
                            builder.Append(wStr, 0, charCount);
                        }
                    }
                }
            }
            finally
            {
                if (ADP.PtrZero != ptrZero)
                {
                    SNINativeMethodWrapper.SNIServerEnumClose(ptrZero);
                }
            }
            if (flag)
            {
                Bid.Trace("<sc.SqlDataSourceEnumerator.GetDataSources|ERR> GetDataSources:SNIServerEnumRead returned bad length, requested %d, received %d", pcbBuf, charCount);
                throw ADP.ArgumentOutOfRange("readLength");
            }
            return ParseServerEnumString(builder.ToString());
        }

        private static DataTable ParseServerEnumString(string serverInstances)
        {
            DataTable table = new DataTable("SqlDataSources") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.Add("ServerName", typeof(string));
            table.Columns.Add("InstanceName", typeof(string));
            table.Columns.Add("IsClustered", typeof(string));
            table.Columns.Add("Version", typeof(string));
            DataRow row = null;
            string str = null;
            string str2 = null;
            string str3 = null;
            string str6 = null;
            foreach (string str9 in serverInstances.Split(new char[1]))
            {
                string str8 = str9.Trim(new char[1]);
                if (str8.Length != 0)
                {
                    foreach (string str5 in str8.Split(new char[] { ';' }))
                    {
                        if (str == null)
                        {
                            foreach (string str7 in str5.Split(new char[] { '\\' }))
                            {
                                if (str == null)
                                {
                                    str = str7;
                                }
                                else
                                {
                                    str2 = str7;
                                }
                            }
                        }
                        else if (str3 == null)
                        {
                            str3 = str5.Substring(_clusterLength);
                        }
                        else
                        {
                            str6 = str5.Substring(_versionLength);
                        }
                    }
                    string filterExpression = "ServerName='" + str + "'";
                    if (!ADP.IsEmpty(str2))
                    {
                        filterExpression = filterExpression + " AND InstanceName='" + str2 + "'";
                    }
                    if (table.Select(filterExpression).Length == 0)
                    {
                        row = table.NewRow();
                        row[0] = str;
                        row[1] = str2;
                        row[2] = str3;
                        row[3] = str6;
                        table.Rows.Add(row);
                    }
                    str = null;
                    str2 = null;
                    str3 = null;
                    str6 = null;
                }
            }
            foreach (DataColumn column in table.Columns)
            {
                column.ReadOnly = true;
            }
            return table;
        }

        public static SqlDataSourceEnumerator Instance
        {
            get
            {
                return SingletonInstance;
            }
        }
    }
}


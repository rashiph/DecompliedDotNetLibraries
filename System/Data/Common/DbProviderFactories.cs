namespace System.Data.Common
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.Data.SqlClient;
    using System.Reflection;

    public static class DbProviderFactories
    {
        private static ConnectionState _initState;
        private static object _lockobj = new object();
        private static DataTable _providerTable;
        private const string AssemblyQualifiedName = "AssemblyQualifiedName";
        private const string Description = "Description";
        private const string Instance = "Instance";
        private const string InvariantName = "InvariantName";
        private const string Name = "Name";

        public static DbProviderFactory GetFactory(DbConnection connection)
        {
            ADP.CheckArgumentNull(connection, "connection");
            return connection.ProviderFactory;
        }

        public static DbProviderFactory GetFactory(DataRow providerRow)
        {
            ADP.CheckArgumentNull(providerRow, "providerRow");
            DataColumn column = providerRow.Table.Columns["AssemblyQualifiedName"];
            if (column != null)
            {
                string str = providerRow[column] as string;
                if (!ADP.IsEmpty(str))
                {
                    Type type = Type.GetType(str);
                    if (null == type)
                    {
                        throw ADP.ConfigProviderNotInstalled();
                    }
                    FieldInfo field = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    if ((null != field) && field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
                    {
                        object obj2 = field.GetValue(null);
                        if (obj2 != null)
                        {
                            return (DbProviderFactory) obj2;
                        }
                    }
                    throw ADP.ConfigProviderInvalid();
                }
            }
            throw ADP.ConfigProviderMissing();
        }

        public static DbProviderFactory GetFactory(string providerInvariantName)
        {
            ADP.CheckArgumentLength(providerInvariantName, "providerInvariantName");
            DataTable providerTable = GetProviderTable();
            if (providerTable != null)
            {
                DataRow providerRow = providerTable.Rows.Find(providerInvariantName);
                if (providerRow != null)
                {
                    return GetFactory(providerRow);
                }
            }
            throw ADP.ConfigProviderNotFound();
        }

        public static DataTable GetFactoryClasses()
        {
            DataTable providerTable = GetProviderTable();
            if (providerTable != null)
            {
                return providerTable.Copy();
            }
            return DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();
        }

        private static DataTable GetProviderTable()
        {
            Initialize();
            return _providerTable;
        }

        private static DataTable IncludeFrameworkFactoryClasses(DataTable configDataTable)
        {
            DataTable table = DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();
            Type type3 = typeof(SqlClientFactory);
            string factoryAssemblyQualifiedName = type3.AssemblyQualifiedName.ToString().Replace("System.Data.SqlClient.SqlClientFactory, System.Data,", "System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient,");
            DbProviderFactoryConfigSection[] sectionArray = new DbProviderFactoryConfigSection[] { new DbProviderFactoryConfigSection(typeof(OdbcFactory), "Odbc Data Provider", ".Net Framework Data Provider for Odbc"), new DbProviderFactoryConfigSection(typeof(OleDbFactory), "OleDb Data Provider", ".Net Framework Data Provider for OleDb"), new DbProviderFactoryConfigSection("OracleClient Data Provider", "System.Data.OracleClient", ".Net Framework Data Provider for Oracle", factoryAssemblyQualifiedName), new DbProviderFactoryConfigSection(typeof(SqlClientFactory), "SqlClient Data Provider", ".Net Framework Data Provider for SqlServer") };
            for (int i = 0; i < sectionArray.Length; i++)
            {
                if (!sectionArray[i].IsNull())
                {
                    bool flag2 = false;
                    if (i == 2)
                    {
                        Type type2 = Type.GetType(sectionArray[i].AssemblyQualifiedName);
                        if (type2 != null)
                        {
                            FieldInfo field = type2.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                            if (((null != field) && field.FieldType.IsSubclassOf(typeof(DbProviderFactory))) && (field.GetValue(null) != null))
                            {
                                flag2 = true;
                            }
                        }
                    }
                    else
                    {
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        DataRow row = table.NewRow();
                        row["Name"] = sectionArray[i].Name;
                        row["InvariantName"] = sectionArray[i].InvariantName;
                        row["Description"] = sectionArray[i].Description;
                        row["AssemblyQualifiedName"] = sectionArray[i].AssemblyQualifiedName;
                        table.Rows.Add(row);
                    }
                }
            }
            for (int j = 0; (configDataTable != null) && (j < configDataTable.Rows.Count); j++)
            {
                try
                {
                    bool flag = false;
                    if (configDataTable.Rows[j]["AssemblyQualifiedName"].ToString().ToLowerInvariant().Contains("System.Data.OracleClient".ToString().ToLowerInvariant()))
                    {
                        Type type = Type.GetType(configDataTable.Rows[j]["AssemblyQualifiedName"].ToString());
                        if (type != null)
                        {
                            FieldInfo info = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                            if (((null != info) && info.FieldType.IsSubclassOf(typeof(DbProviderFactory))) && (info.GetValue(null) != null))
                            {
                                flag = true;
                            }
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        table.Rows.Add(configDataTable.Rows[j].ItemArray);
                    }
                }
                catch (ConstraintException)
                {
                }
            }
            return table;
        }

        private static void Initialize()
        {
            if (ConnectionState.Open != _initState)
            {
                lock (_lockobj)
                {
                    switch (_initState)
                    {
                        case ConnectionState.Closed:
                            break;

                        case ConnectionState.Open:
                        case ConnectionState.Connecting:
                            return;

                        default:
                            return;
                    }
                    _initState = ConnectionState.Connecting;
                    try
                    {
                        DataSet section = System.Configuration.PrivilegedConfigurationManager.GetSection("system.data") as DataSet;
                        _providerTable = (section != null) ? IncludeFrameworkFactoryClasses(section.Tables["DbProviderFactories"]) : IncludeFrameworkFactoryClasses(null);
                    }
                    finally
                    {
                        _initState = ConnectionState.Open;
                    }
                }
            }
        }
    }
}


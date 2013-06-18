namespace System.Data.Common
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Globalization;
    using System.Xml;

    public class DbProviderFactoriesConfigurationHandler : IConfigurationSectionHandler
    {
        internal const string odbcProviderDescription = ".Net Framework Data Provider for Odbc";
        internal const string odbcProviderName = "Odbc Data Provider";
        internal const string oledbProviderDescription = ".Net Framework Data Provider for OleDb";
        internal const string oledbProviderName = "OleDb Data Provider";
        internal const string oracleclientPartialAssemblyQualifiedName = "System.Data.OracleClient.OracleClientFactory, System.Data.OracleClient,";
        internal const string oracleclientProviderDescription = ".Net Framework Data Provider for Oracle";
        internal const string oracleclientProviderName = "OracleClient Data Provider";
        internal const string oracleclientProviderNamespace = "System.Data.OracleClient";
        internal const string providerGroup = "DbProviderFactories";
        internal const string sectionName = "system.data";
        internal const string sqlclientPartialAssemblyQualifiedName = "System.Data.SqlClient.SqlClientFactory, System.Data,";
        internal const string sqlclientProviderDescription = ".Net Framework Data Provider for SqlServer";
        internal const string sqlclientProviderName = "SqlClient Data Provider";

        public virtual object Create(object parent, object configContext, XmlNode section)
        {
            return CreateStatic(parent, configContext, section);
        }

        internal static DataTable CreateProviderDataTable()
        {
            DataColumn column4 = new DataColumn("Name", typeof(string)) {
                ReadOnly = true
            };
            DataColumn column3 = new DataColumn("Description", typeof(string)) {
                ReadOnly = true
            };
            DataColumn column = new DataColumn("InvariantName", typeof(string)) {
                ReadOnly = true
            };
            DataColumn column2 = new DataColumn("AssemblyQualifiedName", typeof(string)) {
                ReadOnly = true
            };
            DataColumn[] columnArray4 = new DataColumn[] { column };
            DataColumn[] columns = new DataColumn[] { column4, column3, column, column2 };
            DataTable table = new DataTable("DbProviderFactories") {
                Locale = CultureInfo.InvariantCulture
            };
            table.Columns.AddRange(columns);
            table.PrimaryKey = columnArray4;
            return table;
        }

        internal static object CreateStatic(object parent, object configContext, XmlNode section)
        {
            object obj2 = parent;
            if (section != null)
            {
                obj2 = System.Data.Common.HandlerBase.CloneParent(parent as DataSet, false);
                bool flag = false;
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(section);
                foreach (XmlNode node in section.ChildNodes)
                {
                    if (!System.Data.Common.HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                    {
                        string str2;
                        string name = node.Name;
                        if (((str2 = name) == null) || !(str2 == "DbProviderFactories"))
                        {
                            throw ADP.ConfigUnrecognizedElement(node);
                        }
                        if (flag)
                        {
                            throw ADP.ConfigSectionsUnique("DbProviderFactories");
                        }
                        flag = true;
                        HandleProviders(obj2 as DataSet, configContext, node, name);
                    }
                }
            }
            return obj2;
        }

        private static void HandleProviders(DataSet config, object configContext, XmlNode section, string sectionName)
        {
            DataTableCollection tables = config.Tables;
            DataTable table = tables[sectionName];
            bool flag = null != table;
            table = DbProviderDictionarySectionHandler.CreateStatic(table, configContext, section);
            if (!flag)
            {
                tables.Add(table);
            }
        }

        private static class DbProviderDictionarySectionHandler
        {
            internal static DataTable CreateStatic(DataTable config, object context, XmlNode section)
            {
                if (section != null)
                {
                    System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(section);
                    if (config == null)
                    {
                        config = DbProviderFactoriesConfigurationHandler.CreateProviderDataTable();
                    }
                    foreach (XmlNode node in section.ChildNodes)
                    {
                        if (!System.Data.Common.HandlerBase.IsIgnorableAlsoCheckForNonElement(node))
                        {
                            string name = node.Name;
                            if (name == null)
                            {
                                goto Label_0086;
                            }
                            if (!(name == "add"))
                            {
                                if (name == "remove")
                                {
                                    goto Label_0074;
                                }
                                if (name == "clear")
                                {
                                    goto Label_007D;
                                }
                                goto Label_0086;
                            }
                            HandleAdd(node, config);
                        }
                        continue;
                    Label_0074:
                        HandleRemove(node, config);
                        continue;
                    Label_007D:
                        HandleClear(node, config);
                        continue;
                    Label_0086:
                        throw ADP.ConfigUnrecognizedElement(node);
                    }
                    config.AcceptChanges();
                }
                return config;
            }

            private static void HandleAdd(XmlNode child, DataTable config)
            {
                System.Data.Common.HandlerBase.CheckForChildNodes(child);
                DataRow row = config.NewRow();
                row[0] = System.Data.Common.HandlerBase.RemoveAttribute(child, "name", true, false);
                row[1] = System.Data.Common.HandlerBase.RemoveAttribute(child, "description", true, false);
                row[2] = System.Data.Common.HandlerBase.RemoveAttribute(child, "invariant", true, false);
                row[3] = System.Data.Common.HandlerBase.RemoveAttribute(child, "type", true, false);
                System.Data.Common.HandlerBase.RemoveAttribute(child, "support", false, false);
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Rows.Add(row);
            }

            private static void HandleClear(XmlNode child, DataTable config)
            {
                System.Data.Common.HandlerBase.CheckForChildNodes(child);
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(child);
                config.Clear();
            }

            private static void HandleRemove(XmlNode child, DataTable config)
            {
                System.Data.Common.HandlerBase.CheckForChildNodes(child);
                string key = System.Data.Common.HandlerBase.RemoveAttribute(child, "invariant", true, false);
                System.Data.Common.HandlerBase.CheckForUnrecognizedAttributes(child);
                DataRow row = config.Rows.Find(key);
                if (row != null)
                {
                    row.Delete();
                }
            }
        }
    }
}


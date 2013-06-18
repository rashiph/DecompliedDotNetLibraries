namespace System.Data.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Data;
    using System.Design;
    using System.Runtime.InteropServices;

    internal sealed class DataSourceNameHandler
    {
        private bool languageCaseInsensitive;
        private static string relationsPropertyName = "Relations";
        private static string tablesPropertyName = "Tables";
        private MemberNameValidator validator;

        internal void GenerateMemberNames(DesignDataSource dataSource, CodeDomProvider codeProvider, bool languageCaseInsensitive, ArrayList problemList)
        {
            this.languageCaseInsensitive = languageCaseInsensitive;
            this.validator = new MemberNameValidator(new string[] { tablesPropertyName, relationsPropertyName }, codeProvider, this.languageCaseInsensitive);
            this.ProcessMemberNames(dataSource);
        }

        private string PlainRowClassName(DataTable table, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = (string) table.ExtendedProperties["typedName"];
            if (StringUtil.Empty(str))
            {
                usesAnnotations = false;
                str = table.TableName + "Row";
            }
            return str;
        }

        private string PlainRowEventArgClassName(DataTable table, out bool usesAnnotations)
        {
            return (this.PlainRowClassName(table, out usesAnnotations) + "ChangeEvent");
        }

        private string PlainRowEventHandlerName(DataTable table, out bool usesAnnotations)
        {
            return (this.PlainRowClassName(table, out usesAnnotations) + "ChangeEventHandler");
        }

        private string PlainTableClassName(DataTable table, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string tableName = (string) table.ExtendedProperties["typedPlural"];
            if (StringUtil.Empty(tableName))
            {
                tableName = (string) table.ExtendedProperties["typedName"];
                if (StringUtil.Empty(tableName))
                {
                    usesAnnotations = false;
                    tableName = table.TableName;
                }
            }
            return (tableName + "DataTable");
        }

        private string PlainTablePropertyName(DataTable table, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = (string) table.ExtendedProperties["typedPlural"];
            if (!StringUtil.Empty(str))
            {
                return str;
            }
            str = (string) table.ExtendedProperties["typedName"];
            if (StringUtil.Empty(str))
            {
                usesAnnotations = false;
                return table.TableName;
            }
            return (str + "Table");
        }

        private string PlainTableVariableName(DataTable table, out bool usesAnnotations)
        {
            return ("table" + this.PlainTablePropertyName(table, out usesAnnotations));
        }

        internal void ProcessDataSourceName(DesignDataSource dataSource)
        {
            if (StringUtil.Empty(dataSource.Name))
            {
                throw new DataSourceGeneratorException(System.Design.SR.GetString("CG_EmptyDSName"));
            }
            if (!StringUtil.EqualValue(dataSource.Name, dataSource.UserDataSetName, this.languageCaseInsensitive) || StringUtil.Empty(dataSource.GeneratorDataSetName))
            {
                dataSource.GeneratorDataSetName = NameHandler.FixIdName(dataSource.Name);
            }
            else
            {
                dataSource.GeneratorDataSetName = this.validator.GenerateIdName(dataSource.GeneratorDataSetName);
            }
            dataSource.UserDataSetName = dataSource.Name;
            if (!StringUtil.EqualValue(NameHandler.FixIdName(dataSource.Name), dataSource.GeneratorDataSetName))
            {
                dataSource.NamingPropertyNames.Add(DesignDataSource.EXTPROPNAME_USER_DATASETNAME);
                dataSource.NamingPropertyNames.Add(DesignDataSource.EXTPROPNAME_GENERATOR_DATASETNAME);
            }
        }

        internal void ProcessMemberNames(DesignDataSource dataSource)
        {
            this.ProcessDataSourceName(dataSource);
            if (dataSource.DesignTables != null)
            {
                foreach (DesignTable table in dataSource.DesignTables)
                {
                    this.ProcessTableRelatedNames(table);
                }
            }
            if (dataSource.DesignRelations != null)
            {
                foreach (DesignRelation relation in dataSource.DesignRelations)
                {
                    this.ProcessRelationRelatedNames(relation);
                }
            }
        }

        internal void ProcessRelationRelatedNames(DesignRelation relation)
        {
            if (relation.DataRelation != null)
            {
                if (!StringUtil.EqualValue(relation.Name, relation.UserRelationName, this.languageCaseInsensitive) || StringUtil.Empty(relation.GeneratorRelationVarName))
                {
                    relation.GeneratorRelationVarName = this.validator.GenerateIdName(this.RelationVariableName(relation.DataRelation));
                }
                else
                {
                    relation.GeneratorRelationVarName = this.validator.GenerateIdName(relation.GeneratorRelationVarName);
                }
            }
        }

        internal void ProcessTableRelatedNames(DesignTable table)
        {
            bool usesAnnotations = false;
            bool flag2 = false;
            bool flag3 = !StringUtil.EqualValue(table.Name, table.UserTableName, this.languageCaseInsensitive);
            string name = this.TablePropertyName(table.DataTable, out usesAnnotations);
            string str2 = this.PlainTablePropertyName(table.DataTable, out usesAnnotations);
            if (usesAnnotations)
            {
                table.GeneratorTablePropName = str2;
            }
            else
            {
                if (flag3 || StringUtil.Empty(table.GeneratorTablePropName))
                {
                    table.GeneratorTablePropName = this.validator.GenerateIdName(name);
                }
                else
                {
                    table.GeneratorTablePropName = this.validator.GenerateIdName(table.GeneratorTablePropName);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(name), table.GeneratorTablePropName))
                {
                    table.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_TABLEPROPNAME);
                    flag2 = true;
                }
            }
            string str3 = this.TableVariableName(table.DataTable, out usesAnnotations);
            string str4 = this.PlainTableVariableName(table.DataTable, out usesAnnotations);
            if (usesAnnotations)
            {
                table.GeneratorTableVarName = str4;
            }
            else
            {
                if (flag3 || StringUtil.Empty(table.GeneratorTableVarName))
                {
                    table.GeneratorTableVarName = this.validator.GenerateIdName(str3);
                }
                else
                {
                    table.GeneratorTableVarName = this.validator.GenerateIdName(table.GeneratorTableVarName);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(str3), table.GeneratorTableVarName))
                {
                    table.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_TABLEVARNAME);
                    flag2 = true;
                }
            }
            string str5 = this.TableClassName(table.DataTable, out usesAnnotations);
            string str6 = this.PlainTableClassName(table.DataTable, out usesAnnotations);
            if (usesAnnotations)
            {
                table.GeneratorTableClassName = str6;
            }
            else
            {
                if (flag3 || StringUtil.Empty(table.GeneratorTableClassName))
                {
                    table.GeneratorTableClassName = this.validator.GenerateIdName(str5);
                }
                else
                {
                    table.GeneratorTableClassName = this.validator.GenerateIdName(table.GeneratorTableClassName);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(str5), table.GeneratorTableClassName))
                {
                    table.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_TABLECLASSNAME);
                    flag2 = true;
                }
            }
            string str7 = this.RowClassName(table.DataTable, out usesAnnotations);
            string str8 = this.PlainRowClassName(table.DataTable, out usesAnnotations);
            if (usesAnnotations)
            {
                table.GeneratorRowClassName = str8;
            }
            else
            {
                if (flag3 || StringUtil.Empty(table.GeneratorRowClassName))
                {
                    table.GeneratorRowClassName = this.validator.GenerateIdName(str7);
                }
                else
                {
                    table.GeneratorRowClassName = this.validator.GenerateIdName(table.GeneratorRowClassName);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(str7), table.GeneratorRowClassName))
                {
                    table.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_ROWCLASSNAME);
                    flag2 = true;
                }
            }
            string str9 = this.RowEventHandlerName(table.DataTable, out usesAnnotations);
            string str10 = this.PlainRowEventHandlerName(table.DataTable, out usesAnnotations);
            if (usesAnnotations)
            {
                table.GeneratorRowEvHandlerName = str10;
            }
            else
            {
                if (flag3 || StringUtil.Empty(table.GeneratorRowEvHandlerName))
                {
                    table.GeneratorRowEvHandlerName = this.validator.GenerateIdName(str9);
                }
                else
                {
                    table.GeneratorRowEvHandlerName = this.validator.GenerateIdName(table.GeneratorRowEvHandlerName);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(str9), table.GeneratorRowEvHandlerName))
                {
                    table.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_ROWEVHANDLERNAME);
                    flag2 = true;
                }
            }
            string str11 = this.RowEventArgClassName(table.DataTable, out usesAnnotations);
            string str12 = this.PlainRowEventArgClassName(table.DataTable, out usesAnnotations);
            if (usesAnnotations)
            {
                table.GeneratorRowEvArgName = str12;
            }
            else
            {
                if (flag3 || StringUtil.Empty(table.GeneratorRowEvArgName))
                {
                    table.GeneratorRowEvArgName = this.validator.GenerateIdName(str11);
                }
                else
                {
                    table.GeneratorRowEvArgName = this.validator.GenerateIdName(table.GeneratorRowEvArgName);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(str11), table.GeneratorRowEvArgName))
                {
                    table.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_ROWEVARGNAME);
                    flag2 = true;
                }
            }
            if (flag2)
            {
                table.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_USER_TABLENAME);
            }
        }

        private string RelationVariableName(DataRelation relation)
        {
            return NameHandler.FixIdName("relation" + relation.RelationName);
        }

        private string RowClassName(DataTable table, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = (string) table.ExtendedProperties["typedName"];
            if (StringUtil.Empty(str))
            {
                usesAnnotations = false;
                str = NameHandler.FixIdName(table.TableName) + "Row";
            }
            return str;
        }

        private string RowEventArgClassName(DataTable table, out bool usesAnnotations)
        {
            return (this.RowClassName(table, out usesAnnotations) + "ChangeEvent");
        }

        private string RowEventHandlerName(DataTable table, out bool usesAnnotations)
        {
            return (this.RowClassName(table, out usesAnnotations) + "ChangeEventHandler");
        }

        private string TableClassName(DataTable table, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = (string) table.ExtendedProperties["typedPlural"];
            if (StringUtil.Empty(str))
            {
                str = (string) table.ExtendedProperties["typedName"];
                if (StringUtil.Empty(str))
                {
                    usesAnnotations = false;
                    str = NameHandler.FixIdName(table.TableName);
                }
            }
            return (str + "DataTable");
        }

        private string TablePropertyName(DataTable table, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = (string) table.ExtendedProperties["typedPlural"];
            if (!StringUtil.Empty(str))
            {
                return str;
            }
            str = (string) table.ExtendedProperties["typedName"];
            if (StringUtil.Empty(str))
            {
                usesAnnotations = false;
                return NameHandler.FixIdName(table.TableName);
            }
            return (str + "Table");
        }

        private string TableVariableName(DataTable table, out bool usesAnnotations)
        {
            return ("table" + this.TablePropertyName(table, out usesAnnotations));
        }

        internal static string RelationsPropertyName
        {
            get
            {
                return relationsPropertyName;
            }
        }

        internal static string TablesPropertyName
        {
            get
            {
                return tablesPropertyName;
            }
        }
    }
}


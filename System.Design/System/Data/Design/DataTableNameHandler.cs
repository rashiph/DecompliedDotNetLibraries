namespace System.Data.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Data;
    using System.Runtime.InteropServices;

    internal sealed class DataTableNameHandler
    {
        private bool languageCaseInsensitive;
        private const string onRowChangedMethodName = "OnRowChanged";
        private const string onRowChangingMethodName = "OnRowChanging";
        private const string onRowDeletedMethodName = "OnRowDeleted";
        private const string onRowDeletingMethodName = "OnRowDeleting";
        private MemberNameValidator validator;

        private void AddReservedNames()
        {
            this.validator.GetNewMemberName("OnRowChanging");
            this.validator.GetNewMemberName("OnRowChanged");
            this.validator.GetNewMemberName("OnRowDeleting");
            this.validator.GetNewMemberName("OnRowDeleted");
        }

        private string ChildPropertyName(DataRelation relation, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = (string) relation.ExtendedProperties["typedChildren"];
            if (!StringUtil.Empty(str))
            {
                return str;
            }
            string str2 = (string) relation.ChildTable.ExtendedProperties["typedPlural"];
            if (StringUtil.Empty(str2))
            {
                str2 = (string) relation.ChildTable.ExtendedProperties["typedName"];
                if (StringUtil.Empty(str2))
                {
                    usesAnnotations = false;
                    str = "Get" + relation.ChildTable.TableName + "Rows";
                    if (1 < TablesConnectedness(relation.ParentTable, relation.ChildTable))
                    {
                        str = str + "By" + relation.RelationName;
                    }
                    return NameHandler.FixIdName(str);
                }
                str2 = str2 + "Rows";
            }
            return ("Get" + str2);
        }

        private DesignRelation FindCorrespondingDesignRelation(DesignTable designTable, DataRelation relation)
        {
            DesignDataSource owner = designTable.Owner;
            if (owner == null)
            {
                throw new InternalException("Unable to find DataSource for table.");
            }
            foreach (DesignRelation relation2 in owner.DesignRelations)
            {
                if (((relation2.DataRelation != null) && StringUtil.EqualValue(relation2.DataRelation.ChildTable.TableName, relation.ChildTable.TableName)) && (StringUtil.EqualValue(relation2.DataRelation.ParentTable.TableName, relation.ParentTable.TableName) && StringUtil.EqualValue(relation2.Name, relation.RelationName)))
                {
                    return relation2;
                }
            }
            return null;
        }

        internal void GenerateMemberNames(DesignTable designTable, CodeDomProvider codeProvider, bool languageCaseInsensitive, ArrayList problemList)
        {
            this.languageCaseInsensitive = languageCaseInsensitive;
            this.validator = new MemberNameValidator(null, codeProvider, this.languageCaseInsensitive);
            this.AddReservedNames();
            this.ProcessMemberNames(designTable);
        }

        private string ParentPropertyName(DataRelation relation, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = null;
            str = (string) relation.ExtendedProperties["typedParent"];
            if (StringUtil.Empty(str))
            {
                str = this.RowClassName(relation.ParentTable, out usesAnnotations);
                if ((relation.ChildTable == relation.ParentTable) || (relation.ChildColumns.Length != 1))
                {
                    str = str + "Parent";
                }
                if (1 < TablesConnectedness(relation.ParentTable, relation.ChildTable))
                {
                    str = str + "By" + NameHandler.FixIdName(relation.RelationName);
                }
            }
            return str;
        }

        private string PlainRowColumnPropertyName(DataColumn column, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string columnName = (string) column.ExtendedProperties["typedName"];
            if (StringUtil.Empty(columnName))
            {
                usesAnnotations = false;
                columnName = column.ColumnName;
            }
            return columnName;
        }

        private string PlainTableColumnPropertyName(DataColumn column, out bool usesAnnotations)
        {
            return (this.PlainRowColumnPropertyName(column, out usesAnnotations) + "Column");
        }

        private string PlainTableColumnVariableName(DataColumn column, out bool usesAnnotations)
        {
            return ("column" + this.PlainRowColumnPropertyName(column, out usesAnnotations));
        }

        internal void ProcessChildRelationName(DesignRelation relation)
        {
            bool flag = (!StringUtil.EqualValue(relation.Name, relation.UserRelationName, this.languageCaseInsensitive) || !StringUtil.EqualValue(relation.ChildDesignTable.Name, relation.UserChildTable, this.languageCaseInsensitive)) || !StringUtil.EqualValue(relation.ParentDesignTable.Name, relation.UserParentTable, this.languageCaseInsensitive);
            bool usesAnnotations = false;
            string name = this.ChildPropertyName(relation.DataRelation, out usesAnnotations);
            if (usesAnnotations)
            {
                relation.GeneratorChildPropName = name;
            }
            else if (flag || StringUtil.Empty(relation.GeneratorChildPropName))
            {
                relation.GeneratorChildPropName = this.validator.GenerateIdName(name);
            }
            else
            {
                relation.GeneratorChildPropName = this.validator.GenerateIdName(relation.GeneratorChildPropName);
            }
        }

        private void ProcessColumnRelatedNames(DesignColumn column)
        {
            bool flag = !StringUtil.EqualValue(column.Name, column.UserColumnName, this.languageCaseInsensitive);
            bool usesAnnotations = false;
            bool flag3 = false;
            string name = this.TableColumnPropertyName(column.DataColumn, out usesAnnotations);
            string str2 = this.PlainTableColumnPropertyName(column.DataColumn, out usesAnnotations);
            if (usesAnnotations)
            {
                column.GeneratorColumnPropNameInTable = str2;
            }
            else
            {
                if (flag || StringUtil.Empty(column.GeneratorColumnPropNameInTable))
                {
                    column.GeneratorColumnPropNameInTable = this.validator.GenerateIdName(name);
                }
                else
                {
                    column.GeneratorColumnPropNameInTable = this.validator.GenerateIdName(column.GeneratorColumnPropNameInTable);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(name), column.GeneratorColumnPropNameInTable))
                {
                    column.NamingPropertyNames.Add(DesignColumn.EXTPROPNAME_GENERATOR_COLUMNPROPNAMEINTABLE);
                    flag3 = true;
                }
            }
            string str3 = this.TableColumnVariableName(column.DataColumn, out usesAnnotations);
            string str4 = this.PlainTableColumnVariableName(column.DataColumn, out usesAnnotations);
            if (usesAnnotations)
            {
                column.GeneratorColumnVarNameInTable = str4;
            }
            else
            {
                if (flag || StringUtil.Empty(column.GeneratorColumnVarNameInTable))
                {
                    column.GeneratorColumnVarNameInTable = this.validator.GenerateIdName(str3);
                }
                else
                {
                    column.GeneratorColumnVarNameInTable = this.validator.GenerateIdName(column.GeneratorColumnVarNameInTable);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(str3), column.GeneratorColumnVarNameInTable))
                {
                    column.NamingPropertyNames.Add(DesignColumn.EXTPROPNAME_GENERATOR_COLUMNVARNAMEINTABLE);
                    flag3 = true;
                }
            }
            string str5 = this.RowColumnPropertyName(column.DataColumn, out usesAnnotations);
            string str6 = this.PlainRowColumnPropertyName(column.DataColumn, out usesAnnotations);
            if (usesAnnotations)
            {
                column.GeneratorColumnPropNameInRow = str6;
            }
            else
            {
                if (flag || StringUtil.Empty(column.GeneratorColumnPropNameInRow))
                {
                    column.GeneratorColumnPropNameInRow = this.validator.GenerateIdName(str5);
                }
                else
                {
                    column.GeneratorColumnPropNameInRow = this.validator.GenerateIdName(column.GeneratorColumnPropNameInRow);
                }
                if (!StringUtil.EqualValue(this.validator.GenerateIdName(str5), column.GeneratorColumnPropNameInRow))
                {
                    column.NamingPropertyNames.Add(DesignColumn.EXTPROPNAME_GENERATOR_COLUMNPROPNAMEINROW);
                    flag3 = true;
                }
            }
            column.UserColumnName = column.Name;
            if (flag3)
            {
                column.NamingPropertyNames.Add(DesignColumn.EXTPROPNAME_USER_COLUMNNAME);
            }
        }

        internal void ProcessEventNames(DesignTable designTable)
        {
            bool flag = false;
            bool flag2 = !StringUtil.EqualValue(designTable.Name, designTable.UserTableName, this.languageCaseInsensitive);
            string name = designTable.GeneratorRowClassName + "Changing";
            if (flag2 || StringUtil.Empty(designTable.GeneratorRowChangingName))
            {
                designTable.GeneratorRowChangingName = this.validator.GenerateIdName(name);
            }
            else
            {
                designTable.GeneratorRowChangingName = this.validator.GenerateIdName(designTable.GeneratorRowChangingName);
            }
            if (!StringUtil.EqualValue(this.validator.GenerateIdName(name), designTable.GeneratorRowChangingName))
            {
                designTable.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_ROWCHANGINGNAME);
                flag = true;
            }
            string str2 = designTable.GeneratorRowClassName + "Changed";
            if (flag2 || StringUtil.Empty(designTable.GeneratorRowChangedName))
            {
                designTable.GeneratorRowChangedName = this.validator.GenerateIdName(str2);
            }
            else
            {
                designTable.GeneratorRowChangedName = this.validator.GenerateIdName(designTable.GeneratorRowChangedName);
            }
            if (!StringUtil.EqualValue(this.validator.GenerateIdName(str2), designTable.GeneratorRowChangedName))
            {
                designTable.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_ROWCHANGEDNAME);
                flag = true;
            }
            string str3 = designTable.GeneratorRowClassName + "Deleting";
            if (flag2 || StringUtil.Empty(designTable.GeneratorRowDeletingName))
            {
                designTable.GeneratorRowDeletingName = this.validator.GenerateIdName(str3);
            }
            else
            {
                designTable.GeneratorRowDeletingName = this.validator.GenerateIdName(designTable.GeneratorRowDeletingName);
            }
            if (!StringUtil.EqualValue(this.validator.GenerateIdName(str3), designTable.GeneratorRowDeletingName))
            {
                designTable.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_ROWDELETINGNAME);
                flag = true;
            }
            string str4 = designTable.GeneratorRowClassName + "Deleted";
            if (flag2 || StringUtil.Empty(designTable.GeneratorRowDeletedName))
            {
                designTable.GeneratorRowDeletedName = this.validator.GenerateIdName(str4);
            }
            else
            {
                designTable.GeneratorRowDeletedName = this.validator.GenerateIdName(designTable.GeneratorRowDeletedName);
            }
            if (!StringUtil.EqualValue(this.validator.GenerateIdName(str4), designTable.GeneratorRowDeletedName))
            {
                designTable.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_GENERATOR_ROWDELETEDNAME);
                flag = true;
            }
            if (flag && !designTable.NamingPropertyNames.Contains(DesignTable.EXTPROPNAME_USER_TABLENAME))
            {
                designTable.NamingPropertyNames.Add(DesignTable.EXTPROPNAME_USER_TABLENAME);
            }
        }

        private void ProcessMemberNames(DesignTable designTable)
        {
            if (designTable.DesignColumns != null)
            {
                foreach (DesignColumn column in designTable.DesignColumns)
                {
                    this.ProcessColumnRelatedNames(column);
                }
            }
            DataRelationCollection childRelations = designTable.DataTable.ChildRelations;
            if (childRelations != null)
            {
                foreach (DataRelation relation in childRelations)
                {
                    DesignRelation relation2 = this.FindCorrespondingDesignRelation(designTable, relation);
                    this.ProcessChildRelationName(relation2);
                }
            }
            DataRelationCollection parentRelations = designTable.DataTable.ParentRelations;
            if (parentRelations != null)
            {
                foreach (DataRelation relation3 in parentRelations)
                {
                    DesignRelation relation4 = this.FindCorrespondingDesignRelation(designTable, relation3);
                    this.ProcessParentRelationName(relation4);
                }
            }
            this.ProcessEventNames(designTable);
        }

        internal void ProcessParentRelationName(DesignRelation relation)
        {
            bool flag = (!StringUtil.EqualValue(relation.Name, relation.UserRelationName, this.languageCaseInsensitive) || !StringUtil.EqualValue(relation.ChildDesignTable.Name, relation.UserChildTable, this.languageCaseInsensitive)) || !StringUtil.EqualValue(relation.ParentDesignTable.Name, relation.UserParentTable, this.languageCaseInsensitive);
            bool usesAnnotations = false;
            string name = this.ParentPropertyName(relation.DataRelation, out usesAnnotations);
            if (usesAnnotations)
            {
                relation.GeneratorParentPropName = name;
            }
            else if (flag || StringUtil.Empty(relation.GeneratorParentPropName))
            {
                relation.GeneratorParentPropName = this.validator.GenerateIdName(name);
            }
            else
            {
                relation.GeneratorParentPropName = this.validator.GenerateIdName(relation.GeneratorParentPropName);
            }
        }

        private string RowClassName(DataTable table, out bool usesAnnotations)
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

        private string RowColumnPropertyName(DataColumn column, out bool usesAnnotations)
        {
            usesAnnotations = true;
            string str = (string) column.ExtendedProperties["typedName"];
            if (StringUtil.Empty(str))
            {
                usesAnnotations = false;
                str = NameHandler.FixIdName(column.ColumnName);
            }
            return str;
        }

        private string TableColumnPropertyName(DataColumn column, out bool usesAnnotations)
        {
            return (this.RowColumnPropertyName(column, out usesAnnotations) + "Column");
        }

        private string TableColumnVariableName(DataColumn column, out bool usesAnnotations)
        {
            string str = this.RowColumnPropertyName(column, out usesAnnotations);
            string str2 = null;
            if (StringUtil.EqualValue("column", str, true))
            {
                str2 = "columnField" + str;
            }
            else
            {
                str2 = "column" + str;
            }
            if (!StringUtil.EqualValue(str2, "Columns", this.languageCaseInsensitive))
            {
                return str2;
            }
            return ("_" + str2);
        }

        private static int TablesConnectedness(DataTable parentTable, DataTable childTable)
        {
            int num = 0;
            DataRelationCollection parentRelations = childTable.ParentRelations;
            for (int i = 0; i < parentRelations.Count; i++)
            {
                if (parentRelations[i].ParentTable == parentTable)
                {
                    num++;
                }
            }
            return num;
        }
    }
}


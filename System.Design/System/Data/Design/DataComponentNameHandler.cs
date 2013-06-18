namespace System.Data.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;

    internal sealed class DataComponentNameHandler
    {
        private static readonly string adapterPropertyName = "Adapter";
        private static readonly string adapterVariableName = "_adapter";
        private static readonly string clearBeforeFillPropertyName = "ClearBeforeFill";
        private static readonly string clearBeforeFillVariableName = "_clearBeforeFill";
        private static readonly string defaultConnectionPropertyName = "Connection";
        private static readonly string defaultConnectionVariableName = "_connection";
        private static readonly string deleteMethodName = "Delete";
        private bool globalSources;
        private static readonly string initAdapter = "InitAdapter";
        private static readonly string initCmdCollection = "InitCommandCollection";
        private static readonly string initConnection = "InitConnection";
        private static readonly string initMethodName = "InitClass";
        private static readonly string insertMethodName = "Insert";
        private bool languageCaseInsensitive;
        private static readonly string pagingMethodSuffix = "Page";
        private static readonly string selectCmdCollectionPropertyName = "CommandCollection";
        private static readonly string selectCmdCollectionVariableName = "_commandCollection";
        private static readonly string transactionPropertyName = "Transaction";
        private static readonly string transactionVariableName = "_transaction";
        private static readonly string updateMethodName = "Update";
        private MemberNameValidator validator;

        private void AddReservedNames()
        {
            this.validator.GetNewMemberName(initMethodName);
            this.validator.GetNewMemberName(deleteMethodName);
            this.validator.GetNewMemberName(insertMethodName);
            this.validator.GetNewMemberName(updateMethodName);
            this.validator.GetNewMemberName(adapterVariableName);
            this.validator.GetNewMemberName(adapterPropertyName);
            this.validator.GetNewMemberName(initAdapter);
            this.validator.GetNewMemberName(selectCmdCollectionVariableName);
            this.validator.GetNewMemberName(selectCmdCollectionPropertyName);
            this.validator.GetNewMemberName(initCmdCollection);
            this.validator.GetNewMemberName(defaultConnectionVariableName);
            this.validator.GetNewMemberName(defaultConnectionPropertyName);
            this.validator.GetNewMemberName(transactionVariableName);
            this.validator.GetNewMemberName(transactionPropertyName);
            this.validator.GetNewMemberName(initConnection);
            this.validator.GetNewMemberName(clearBeforeFillVariableName);
            this.validator.GetNewMemberName(clearBeforeFillPropertyName);
            this.validator.GetNewMemberName("TableAdapterManager");
            this.validator.GetNewMemberName("UpdateAll");
        }

        internal void GenerateMemberNames(DesignTable designTable, CodeDomProvider codeProvider, bool languageCaseInsensitive, ArrayList problemList)
        {
            this.languageCaseInsensitive = languageCaseInsensitive;
            this.validator = new MemberNameValidator(null, codeProvider, this.languageCaseInsensitive);
            this.validator.UseSuffix = true;
            this.AddReservedNames();
            this.ProcessMemberNames(designTable);
        }

        internal void ProcessClassName(DesignTable table)
        {
            if (!StringUtil.EqualValue(table.DataAccessorName, table.UserDataComponentName, this.languageCaseInsensitive) || StringUtil.Empty(table.GeneratorDataComponentClassName))
            {
                table.GeneratorDataComponentClassName = this.validator.GenerateIdName(table.DataAccessorName);
            }
            else
            {
                table.GeneratorDataComponentClassName = this.validator.GenerateIdName(table.GeneratorDataComponentClassName);
            }
        }

        private void ProcessMemberNames(DesignTable designTable)
        {
            this.ProcessClassName(designTable);
            if (!this.GlobalSources && (designTable.MainSource != null))
            {
                this.ProcessSourceName((DbSource) designTable.MainSource);
            }
            if (designTable.Sources != null)
            {
                foreach (Source source in designTable.Sources)
                {
                    this.ProcessSourceName((DbSource) source);
                }
            }
        }

        internal void ProcessSourceName(DbSource source)
        {
            bool flag = !StringUtil.EqualValue(source.Name, source.UserSourceName, this.languageCaseInsensitive);
            bool flag2 = !StringUtil.EqualValue(source.GetMethodName, source.UserGetMethodName, this.languageCaseInsensitive);
            if ((source.GenerateMethods == GenerateMethodTypes.Fill) || (source.GenerateMethods == GenerateMethodTypes.Both))
            {
                if (flag || StringUtil.Empty(source.GeneratorSourceName))
                {
                    source.GeneratorSourceName = this.validator.GenerateIdName(source.Name);
                }
                else
                {
                    source.GeneratorSourceName = this.validator.GenerateIdName(source.GeneratorSourceName);
                }
            }
            if ((source.QueryType == QueryType.Rowset) && ((source.GenerateMethods == GenerateMethodTypes.Get) || (source.GenerateMethods == GenerateMethodTypes.Both)))
            {
                if (flag2 || StringUtil.Empty(source.GeneratorGetMethodName))
                {
                    source.GeneratorGetMethodName = this.validator.GenerateIdName(source.GetMethodName);
                }
                else
                {
                    source.GeneratorGetMethodName = this.validator.GenerateIdName(source.GeneratorGetMethodName);
                }
            }
            if ((source.QueryType == QueryType.Rowset) && source.GeneratePagingMethods)
            {
                if ((source.GenerateMethods == GenerateMethodTypes.Fill) || (source.GenerateMethods == GenerateMethodTypes.Both))
                {
                    if (flag || StringUtil.Empty(source.GeneratorSourceNameForPaging))
                    {
                        source.GeneratorSourceNameForPaging = this.validator.GenerateIdName(source.Name + pagingMethodSuffix);
                    }
                    else
                    {
                        source.GeneratorSourceNameForPaging = this.validator.GenerateIdName(source.GeneratorSourceNameForPaging);
                    }
                }
                if ((source.GenerateMethods == GenerateMethodTypes.Get) || (source.GenerateMethods == GenerateMethodTypes.Both))
                {
                    if (flag2 || StringUtil.Empty(source.GeneratorGetMethodNameForPaging))
                    {
                        source.GeneratorGetMethodNameForPaging = this.validator.GenerateIdName(source.GetMethodName + pagingMethodSuffix);
                    }
                    else
                    {
                        source.GeneratorGetMethodNameForPaging = this.validator.GenerateIdName(source.GeneratorGetMethodNameForPaging);
                    }
                }
            }
        }

        internal static string AdapterPropertyName
        {
            get
            {
                return adapterPropertyName;
            }
        }

        internal static string AdapterVariableName
        {
            get
            {
                return adapterVariableName;
            }
        }

        internal static string ClearBeforeFillPropertyName
        {
            get
            {
                return clearBeforeFillPropertyName;
            }
        }

        internal static string ClearBeforeFillVariableName
        {
            get
            {
                return clearBeforeFillVariableName;
            }
        }

        internal static string DefaultConnectionPropertyName
        {
            get
            {
                return defaultConnectionPropertyName;
            }
        }

        internal static string DefaultConnectionVariableName
        {
            get
            {
                return defaultConnectionVariableName;
            }
        }

        internal static string DeleteMethodName
        {
            get
            {
                return deleteMethodName;
            }
        }

        internal bool GlobalSources
        {
            get
            {
                return this.globalSources;
            }
            set
            {
                this.globalSources = value;
            }
        }

        internal static string InitAdapter
        {
            get
            {
                return initAdapter;
            }
        }

        internal static string InitCmdCollection
        {
            get
            {
                return initCmdCollection;
            }
        }

        internal static string InitConnection
        {
            get
            {
                return initConnection;
            }
        }

        internal static string InsertMethodName
        {
            get
            {
                return insertMethodName;
            }
        }

        internal static string PagingMethodSuffix
        {
            get
            {
                return pagingMethodSuffix;
            }
        }

        internal static string SelectCmdCollectionPropertyName
        {
            get
            {
                return selectCmdCollectionPropertyName;
            }
        }

        internal static string SelectCmdCollectionVariableName
        {
            get
            {
                return selectCmdCollectionVariableName;
            }
        }

        internal static string TransactionPropertyName
        {
            get
            {
                return transactionPropertyName;
            }
        }

        internal static string TransactionVariableName
        {
            get
            {
                return transactionVariableName;
            }
        }

        internal static string UpdateMethodName
        {
            get
            {
                return updateMethodName;
            }
        }
    }
}


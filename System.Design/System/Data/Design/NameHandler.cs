namespace System.Data.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Data;
    using System.Reflection;

    internal sealed class NameHandler
    {
        private static CodeDomProvider codeProvider = null;
        private DataSourceNameHandler dataSourceHandler;
        private const string FunctionsTableName = "Queries";
        private bool languageCaseInsensitive;
        private static Hashtable lookupIdentifiers;

        internal NameHandler(CodeDomProvider codeProvider)
        {
            if (codeProvider == null)
            {
                throw new ArgumentException("codeProvider");
            }
            NameHandler.codeProvider = codeProvider;
        }

        internal static string FixIdName(string inVarName)
        {
            if (lookupIdentifiers == null)
            {
                InitLookupIdentifiers();
            }
            string str = (string) lookupIdentifiers[inVarName];
            if (str == null)
            {
                str = MemberNameValidator.GenerateIdName(inVarName, codeProvider, false);
                while (lookupIdentifiers.ContainsValue(str))
                {
                    str = '_' + str;
                }
                lookupIdentifiers[inVarName] = str;
            }
            return str;
        }

        internal void GenerateMemberNames(DesignDataSource dataSource, ArrayList problemList)
        {
            if ((dataSource == null) || (codeProvider == null))
            {
                throw new InternalException("DesignDataSource or/and CodeDomProvider parameters are null.");
            }
            InitLookupIdentifiers();
            this.dataSourceHandler = new DataSourceNameHandler();
            this.dataSourceHandler.GenerateMemberNames(dataSource, codeProvider, this.languageCaseInsensitive, problemList);
            foreach (DesignTable table in dataSource.DesignTables)
            {
                new DataTableNameHandler().GenerateMemberNames(table, codeProvider, this.languageCaseInsensitive, problemList);
                new DataComponentNameHandler().GenerateMemberNames(table, codeProvider, this.languageCaseInsensitive, problemList);
            }
            if ((dataSource.Sources != null) && (dataSource.Sources.Count > 0))
            {
                DesignTable designTable = new DesignTable {
                    TableType = TableType.RadTable,
                    DataAccessorName = dataSource.FunctionsComponentName,
                    UserDataComponentName = dataSource.UserFunctionsComponentName,
                    GeneratorDataComponentClassName = dataSource.GeneratorFunctionsComponentClassName
                };
                foreach (Source source in dataSource.Sources)
                {
                    designTable.Sources.Add(source);
                }
                new DataComponentNameHandler { GlobalSources = true }.GenerateMemberNames(designTable, codeProvider, this.languageCaseInsensitive, problemList);
                dataSource.GeneratorFunctionsComponentClassName = designTable.GeneratorDataComponentClassName;
            }
        }

        private static void InitLookupIdentifiers()
        {
            lookupIdentifiers = new Hashtable();
            foreach (PropertyInfo info in typeof(DataRow).GetProperties())
            {
                lookupIdentifiers[info.Name] = '_' + info.Name;
            }
        }
    }
}


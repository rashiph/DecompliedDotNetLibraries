namespace System.Data.Design
{
    using System;
    using System.CodeDom.Compiler;
    using System.Globalization;

    internal sealed class TableAdapterManagerNameHandler
    {
        internal const string BackupDataSetBeforeUpdateProperty = "BackupDataSetBeforeUpdate";
        internal const string BackupDataSetBeforeUpdateVar = "_backupDataSetBeforeUpdate";
        private CodeDomProvider codePrivider;
        internal const string ConnectionProperty = "Connection";
        internal const string ConnectionVar = "_connection";
        internal const string GetRealUpdatedRowsMethod = "GetRealUpdatedRows";
        private bool languageCaseInsensitive;
        internal const string MatchTAConnectionMethod = "MatchTableAdapterConnection";
        internal const string SelfRefComparerClass = "SelfReferenceComparer";
        internal const string SortSelfRefRowsMethod = "SortSelfReferenceRows";
        internal const string TableAdapterInstanceCountProperty = "TableAdapterInstanceCount";
        internal const string TableAdapterManagerClassName = "TableAdapterManager";
        private MemberNameValidator tableAdapterManagerValidator;
        internal const string UpdateAllMethod = "UpdateAll";
        internal const string UpdateAllRevertConnectionsVar = "revertConnections";
        internal const string UpdateDeletedRowsMethod = "UpdateDeletedRows";
        internal const string UpdateInsertedRowsMethod = "UpdateInsertedRows";
        internal const string UpdateOrderOptionEnum = "UpdateOrderOption";
        internal const string UpdateOrderOptionEnumIUD = "InsertUpdateDelete";
        internal const string UpdateOrderOptionEnumUID = "UpdateInsertDelete";
        internal const string UpdateOrderOptionProperty = "UpdateOrder";
        internal const string UpdateOrderOptionVar = "_updateOrder";
        internal const string UpdateUpdatedRowsMethod = "UpdateUpdatedRows";

        public TableAdapterManagerNameHandler(CodeDomProvider provider)
        {
            this.codePrivider = provider;
            this.languageCaseInsensitive = (this.codePrivider.LanguageOptions & LanguageOptions.CaseInsensitive) == LanguageOptions.CaseInsensitive;
        }

        internal string GetNewMemberName(string memberName)
        {
            return this.TableAdapterManagerValidator.GetNewMemberName(memberName);
        }

        internal string GetTableAdapterPropName(string className)
        {
            return this.GetNewMemberName(className);
        }

        internal string GetTableAdapterVarName(string propName)
        {
            propName = "_" + char.ToLower(propName[0], CultureInfo.InvariantCulture) + propName.Remove(0, 1);
            return this.GetNewMemberName(propName);
        }

        private MemberNameValidator TableAdapterManagerValidator
        {
            get
            {
                if (this.tableAdapterManagerValidator == null)
                {
                    this.tableAdapterManagerValidator = new MemberNameValidator(new string[] { "SelfReferenceComparer", "UpdateAll", "SortSelfReferenceRows", "MatchTableAdapterConnection", "_connection", "Connection", "_backupDataSetBeforeUpdate", "BackupDataSetBeforeUpdate", "TableAdapterInstanceCount", "UpdateOrder", "_updateOrder", "UpdateOrderOption", "UpdateUpdatedRows", "UpdateInsertedRows", "UpdateDeletedRows", "GetRealUpdatedRows" }, this.codePrivider, this.languageCaseInsensitive);
                }
                return this.tableAdapterManagerValidator;
            }
        }
    }
}


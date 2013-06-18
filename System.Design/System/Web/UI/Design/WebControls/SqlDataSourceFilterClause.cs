namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design.Data;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class SqlDataSourceFilterClause
    {
        private System.ComponentModel.Design.Data.DesignerDataColumn _designerDataColumn;
        private DesignerDataConnection _designerDataConnection;
        private DesignerDataTableBase _designerDataTable;
        private bool _isBinary;
        private string _operatorFormat;
        private System.Web.UI.WebControls.Parameter _parameter;
        private string _value;

        public SqlDataSourceFilterClause(DesignerDataConnection designerDataConnection, DesignerDataTableBase designerDataTable, System.ComponentModel.Design.Data.DesignerDataColumn designerDataColumn, string operatorFormat, bool isBinary, string value, System.Web.UI.WebControls.Parameter parameter)
        {
            this._designerDataConnection = designerDataConnection;
            this._designerDataTable = designerDataTable;
            this._designerDataColumn = designerDataColumn;
            this._isBinary = isBinary;
            this._operatorFormat = operatorFormat;
            this._value = value;
            this._parameter = parameter;
        }

        public override string ToString()
        {
            SqlDataSourceColumnData data = new SqlDataSourceColumnData(this._designerDataConnection, this._designerDataColumn);
            if (this._isBinary)
            {
                return string.Format(CultureInfo.InvariantCulture, this._operatorFormat, new object[] { data.EscapedName, this._value });
            }
            return string.Format(CultureInfo.InvariantCulture, this._operatorFormat, new object[] { data.EscapedName });
        }

        public System.ComponentModel.Design.Data.DesignerDataColumn DesignerDataColumn
        {
            get
            {
                return this._designerDataColumn;
            }
        }

        public bool IsBinary
        {
            get
            {
                return this._isBinary;
            }
        }

        public string OperatorFormat
        {
            get
            {
                return this._operatorFormat;
            }
        }

        public System.Web.UI.WebControls.Parameter Parameter
        {
            get
            {
                return this._parameter;
            }
        }

        public string Value
        {
            get
            {
                return this._value;
            }
        }
    }
}


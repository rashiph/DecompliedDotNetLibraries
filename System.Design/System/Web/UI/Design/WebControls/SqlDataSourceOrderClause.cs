namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design.Data;

    internal sealed class SqlDataSourceOrderClause
    {
        private System.ComponentModel.Design.Data.DesignerDataColumn _designerDataColumn;
        private DesignerDataConnection _designerDataConnection;
        private DesignerDataTableBase _designerDataTable;
        private bool _isDescending;

        public SqlDataSourceOrderClause(DesignerDataConnection designerDataConnection, DesignerDataTableBase designerDataTable, System.ComponentModel.Design.Data.DesignerDataColumn designerDataColumn, bool isDescending)
        {
            this._designerDataConnection = designerDataConnection;
            this._designerDataTable = designerDataTable;
            this._designerDataColumn = designerDataColumn;
            this._isDescending = isDescending;
        }

        public override string ToString()
        {
            SqlDataSourceColumnData data = new SqlDataSourceColumnData(this._designerDataConnection, this._designerDataColumn);
            if (this._isDescending)
            {
                return (data.EscapedName + " DESC");
            }
            return data.EscapedName;
        }

        public System.ComponentModel.Design.Data.DesignerDataColumn DesignerDataColumn
        {
            get
            {
                return this._designerDataColumn;
            }
        }

        public bool IsDescending
        {
            get
            {
                return this._isDescending;
            }
        }
    }
}


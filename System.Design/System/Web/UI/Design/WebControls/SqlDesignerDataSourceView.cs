namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SqlDesignerDataSourceView : DesignerDataSourceView
    {
        private SqlDataSourceDesigner _owner;

        public SqlDesignerDataSourceView(SqlDataSourceDesigner owner, string viewName) : base(owner, viewName)
        {
            this._owner = owner;
        }

        public override IEnumerable GetDesignTimeData(int minimumRows, out bool isSampleData)
        {
            DataTable table = this._owner.LoadSchema();
            if (table != null)
            {
                isSampleData = true;
                return DesignTimeData.GetDesignTimeDataSource(DesignTimeData.CreateSampleDataTable(new DataView(table), true), minimumRows);
            }
            return base.GetDesignTimeData(minimumRows, out isSampleData);
        }

        public override bool CanDelete
        {
            get
            {
                return (this._owner.SqlDataSource.DeleteCommand.Length > 0);
            }
        }

        public override bool CanInsert
        {
            get
            {
                return (this._owner.SqlDataSource.InsertCommand.Length > 0);
            }
        }

        public override bool CanPage
        {
            get
            {
                return false;
            }
        }

        public override bool CanRetrieveTotalRowCount
        {
            get
            {
                return false;
            }
        }

        public override bool CanSort
        {
            get
            {
                if (this._owner.SqlDataSource.DataSourceMode != SqlDataSourceMode.DataSet)
                {
                    return (this._owner.SqlDataSource.SortParameterName.Length > 0);
                }
                return true;
            }
        }

        public override bool CanUpdate
        {
            get
            {
                return (this._owner.SqlDataSource.UpdateCommand.Length > 0);
            }
        }

        public override IDataSourceViewSchema Schema
        {
            get
            {
                DataTable dataTable = this._owner.LoadSchema();
                if (dataTable == null)
                {
                    return null;
                }
                return new DataSetViewSchema(dataTable);
            }
        }
    }
}


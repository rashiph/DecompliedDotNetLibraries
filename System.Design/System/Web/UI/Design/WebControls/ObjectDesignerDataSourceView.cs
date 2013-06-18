namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.UI.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ObjectDesignerDataSourceView : DesignerDataSourceView
    {
        private ObjectDataSourceDesigner _owner;

        public ObjectDesignerDataSourceView(ObjectDataSourceDesigner owner, string viewName) : base(owner, viewName)
        {
            this._owner = owner;
        }

        public override IEnumerable GetDesignTimeData(int minimumRows, out bool isSampleData)
        {
            isSampleData = true;
            DataTable[] tableArray = this._owner.LoadSchema();
            if ((tableArray != null) && (tableArray.Length > 0))
            {
                if (base.Name.Length == 0)
                {
                    return DesignTimeData.GetDesignTimeDataSource(DesignTimeData.CreateSampleDataTable(new DataView(tableArray[0]), true), minimumRows);
                }
                foreach (DataTable table in tableArray)
                {
                    if (string.Equals(table.TableName, base.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return DesignTimeData.GetDesignTimeDataSource(DesignTimeData.CreateSampleDataTable(new DataView(table), true), minimumRows);
                    }
                }
            }
            return base.GetDesignTimeData(minimumRows, out isSampleData);
        }

        public override bool CanDelete
        {
            get
            {
                return (this._owner.ObjectDataSource.DeleteMethod.Length > 0);
            }
        }

        public override bool CanInsert
        {
            get
            {
                return (this._owner.ObjectDataSource.InsertMethod.Length > 0);
            }
        }

        public override bool CanPage
        {
            get
            {
                return this._owner.ObjectDataSource.EnablePaging;
            }
        }

        public override bool CanRetrieveTotalRowCount
        {
            get
            {
                return (this._owner.ObjectDataSource.SelectCountMethod.Length > 0);
            }
        }

        public override bool CanSort
        {
            get
            {
                if (this._owner.ObjectDataSource.SortParameterName.Length <= 0)
                {
                    Type selectMethodReturnType = this._owner.SelectMethodReturnType;
                    if (selectMethodReturnType == null)
                    {
                        return false;
                    }
                    if (!typeof(DataSet).IsAssignableFrom(selectMethodReturnType) && !typeof(DataTable).IsAssignableFrom(selectMethodReturnType))
                    {
                        return typeof(DataView).IsAssignableFrom(selectMethodReturnType);
                    }
                }
                return true;
            }
        }

        public override bool CanUpdate
        {
            get
            {
                return (this._owner.ObjectDataSource.UpdateMethod.Length > 0);
            }
        }

        public override IDataSourceViewSchema Schema
        {
            get
            {
                DataTable[] tableArray = this._owner.LoadSchema();
                if ((tableArray != null) && (tableArray.Length > 0))
                {
                    if (base.Name.Length == 0)
                    {
                        return new DataSetViewSchema(tableArray[0]);
                    }
                    foreach (DataTable table in tableArray)
                    {
                        if (string.Equals(table.TableName, base.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            return new DataSetViewSchema(table);
                        }
                    }
                }
                return null;
            }
        }
    }
}


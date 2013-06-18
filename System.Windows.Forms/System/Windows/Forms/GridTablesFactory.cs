namespace System.Windows.Forms
{
    using System;

    public sealed class GridTablesFactory
    {
        private GridTablesFactory()
        {
        }

        public static DataGridTableStyle[] CreateGridTables(DataGridTableStyle gridTable, object dataSource, string dataMember, BindingContext bindingManager)
        {
            return new DataGridTableStyle[] { gridTable };
        }
    }
}


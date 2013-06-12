namespace System.Web.UI.WebControls
{
    using System;
    using System.Reflection;
    using System.Web.UI;

    [SupportsEventValidation]
    internal sealed class LayoutTable : Table
    {
        public LayoutTable(int rows, int columns, Page page)
        {
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException("rows");
            }
            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException("columns");
            }
            if (page != null)
            {
                this.Page = page;
            }
            for (int i = 0; i < rows; i++)
            {
                TableRow row = new TableRow();
                this.Rows.Add(row);
                for (int j = 0; j < columns; j++)
                {
                    TableCell cell = new LayoutTableCell();
                    row.Cells.Add(cell);
                }
            }
        }

        public TableCell this[int row, int column]
        {
            get
            {
                return this.Rows[row].Cells[column];
            }
        }
    }
}


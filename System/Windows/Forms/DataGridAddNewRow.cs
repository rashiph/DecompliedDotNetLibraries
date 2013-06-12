namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    internal class DataGridAddNewRow : DataGridRow
    {
        private bool dataBound;

        public DataGridAddNewRow(DataGrid dGrid, DataGridTableStyle gridTable, int rowNum) : base(dGrid, gridTable, rowNum)
        {
        }

        internal override void LoseChildFocus(Rectangle rowHeader, bool alignToRight)
        {
        }

        public override void OnEdit()
        {
            if (!this.DataBound)
            {
                base.DataGrid.AddNewRow();
            }
        }

        public override void OnRowLeave()
        {
            if (this.DataBound)
            {
                this.DataBound = false;
            }
        }

        public override int Paint(Graphics g, Rectangle bounds, Rectangle trueRowBounds, int firstVisibleColumn, int columnCount)
        {
            return this.Paint(g, bounds, trueRowBounds, firstVisibleColumn, columnCount, false);
        }

        public override int Paint(Graphics g, Rectangle bounds, Rectangle trueRowBounds, int firstVisibleColumn, int columnCount, bool alignToRight)
        {
            DataGridLineStyle gridLineStyle;
            Rectangle rectangle = bounds;
            if (base.dgTable.IsDefault)
            {
                gridLineStyle = base.DataGrid.GridLineStyle;
            }
            else
            {
                gridLineStyle = base.dgTable.GridLineStyle;
            }
            int borderWidth = (base.DataGrid == null) ? 0 : ((gridLineStyle == DataGridLineStyle.Solid) ? 1 : 0);
            rectangle.Height -= borderWidth;
            int dataWidth = base.PaintData(g, rectangle, firstVisibleColumn, columnCount, alignToRight);
            if (borderWidth > 0)
            {
                this.PaintBottomBorder(g, bounds, dataWidth, borderWidth, alignToRight);
            }
            return dataWidth;
        }

        protected override void PaintCellContents(Graphics g, Rectangle cellBounds, DataGridColumnStyle column, Brush backBr, Brush foreBrush, bool alignToRight)
        {
            if (this.DataBound)
            {
                CurrencyManager listManager = base.DataGrid.ListManager;
                column.Paint(g, cellBounds, listManager, base.RowNumber, alignToRight);
            }
            else
            {
                base.PaintCellContents(g, cellBounds, column, backBr, foreBrush, alignToRight);
            }
        }

        internal override bool ProcessTabKey(Keys keyData, Rectangle rowHeaders, bool alignToRight)
        {
            return false;
        }

        public bool DataBound
        {
            get
            {
                return this.dataBound;
            }
            set
            {
                this.dataBound = value;
            }
        }
    }
}


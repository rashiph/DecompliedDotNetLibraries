namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), TypeConverter(typeof(TableLayoutPanelCellPositionTypeConverter))]
    public struct TableLayoutPanelCellPosition
    {
        private int row;
        private int column;
        public TableLayoutPanelCellPosition(int column, int row)
        {
            if (row < -1)
            {
                throw new ArgumentOutOfRangeException("row", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "row", row.ToString(CultureInfo.CurrentCulture) }));
            }
            if (column < -1)
            {
                throw new ArgumentOutOfRangeException("column", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "column", column.ToString(CultureInfo.CurrentCulture) }));
            }
            this.row = row;
            this.column = column;
        }

        public int Row
        {
            get
            {
                return this.row;
            }
            set
            {
                this.row = value;
            }
        }
        public int Column
        {
            get
            {
                return this.column;
            }
            set
            {
                this.column = value;
            }
        }
        public override bool Equals(object other)
        {
            if (!(other is TableLayoutPanelCellPosition))
            {
                return false;
            }
            TableLayoutPanelCellPosition position = (TableLayoutPanelCellPosition) other;
            return ((position.row == this.row) && (position.column == this.column));
        }

        public static bool operator ==(TableLayoutPanelCellPosition p1, TableLayoutPanelCellPosition p2)
        {
            return ((p1.Row == p2.Row) && (p1.Column == p2.Column));
        }

        public static bool operator !=(TableLayoutPanelCellPosition p1, TableLayoutPanelCellPosition p2)
        {
            return !(p1 == p2);
        }

        public override string ToString()
        {
            return (this.Column.ToString(CultureInfo.CurrentCulture) + "," + this.Row.ToString(CultureInfo.CurrentCulture));
        }

        public override int GetHashCode()
        {
            return WindowsFormsUtils.GetCombinedHashCodes(new int[] { this.row, this.column });
        }
    }
}


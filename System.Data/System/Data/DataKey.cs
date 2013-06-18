namespace System.Data
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DataKey
    {
        internal const int COLUMN = 0xffff;
        internal const int DESCENDING = -2147483648;
        private const int maxColumns = 0x20;
        private readonly DataColumn[] columns;
        internal DataKey(DataColumn[] columns, bool copyColumns)
        {
            if (columns == null)
            {
                throw ExceptionBuilder.ArgumentNull("columns");
            }
            if (columns.Length == 0)
            {
                throw ExceptionBuilder.KeyNoColumns();
            }
            if (columns.Length > 0x20)
            {
                throw ExceptionBuilder.KeyTooManyColumns(0x20);
            }
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i] == null)
                {
                    throw ExceptionBuilder.ArgumentNull("column");
                }
            }
            for (int j = 0; j < columns.Length; j++)
            {
                for (int k = 0; k < j; k++)
                {
                    if (columns[j] == columns[k])
                    {
                        throw ExceptionBuilder.KeyDuplicateColumns(columns[j].ColumnName);
                    }
                }
            }
            if (copyColumns)
            {
                this.columns = new DataColumn[columns.Length];
                for (int m = 0; m < columns.Length; m++)
                {
                    this.columns[m] = columns[m];
                }
            }
            else
            {
                this.columns = columns;
            }
            this.CheckState();
        }

        internal DataColumn[] ColumnsReference
        {
            get
            {
                return this.columns;
            }
        }
        internal bool HasValue
        {
            get
            {
                return (null != this.columns);
            }
        }
        internal DataTable Table
        {
            get
            {
                return this.columns[0].Table;
            }
        }
        internal void CheckState()
        {
            DataTable table = this.columns[0].Table;
            if (table == null)
            {
                throw ExceptionBuilder.ColumnNotInAnyTable();
            }
            for (int i = 1; i < this.columns.Length; i++)
            {
                if (this.columns[i].Table == null)
                {
                    throw ExceptionBuilder.ColumnNotInAnyTable();
                }
                if (this.columns[i].Table != table)
                {
                    throw ExceptionBuilder.KeyTableMismatch();
                }
            }
        }

        internal bool ColumnsEqual(DataKey key)
        {
            DataColumn[] columns = this.columns;
            DataColumn[] columnArray = key.columns;
            if (columns != columnArray)
            {
                if ((columns == null) || (columnArray == null))
                {
                    return false;
                }
                if (columns.Length != columnArray.Length)
                {
                    return false;
                }
                for (int i = 0; i < columns.Length; i++)
                {
                    bool flag = false;
                    for (int j = 0; j < columnArray.Length; j++)
                    {
                        if (columns[i].Equals(columnArray[j]))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool ContainsColumn(DataColumn column)
        {
            for (int i = 0; i < this.columns.Length; i++)
            {
                if (column == this.columns[i])
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DataKey x, DataKey y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(DataKey x, DataKey y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object value)
        {
            return this.Equals((DataKey) value);
        }

        internal bool Equals(DataKey value)
        {
            DataColumn[] columns = this.columns;
            DataColumn[] columnArray2 = value.columns;
            if (columns != columnArray2)
            {
                if ((columns == null) || (columnArray2 == null))
                {
                    return false;
                }
                if (columns.Length != columnArray2.Length)
                {
                    return false;
                }
                for (int i = 0; i < columns.Length; i++)
                {
                    if (!columns[i].Equals(columnArray2[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal string[] GetColumnNames()
        {
            string[] strArray = new string[this.columns.Length];
            for (int i = 0; i < this.columns.Length; i++)
            {
                strArray[i] = this.columns[i].ColumnName;
            }
            return strArray;
        }

        internal IndexField[] GetIndexDesc()
        {
            IndexField[] fieldArray = new IndexField[this.columns.Length];
            for (int i = 0; i < this.columns.Length; i++)
            {
                fieldArray[i] = new IndexField(this.columns[i], false);
            }
            return fieldArray;
        }

        internal object[] GetKeyValues(int record)
        {
            object[] objArray = new object[this.columns.Length];
            for (int i = 0; i < this.columns.Length; i++)
            {
                objArray[i] = this.columns[i][record];
            }
            return objArray;
        }

        internal Index GetSortIndex()
        {
            return this.GetSortIndex(DataViewRowState.CurrentRows);
        }

        internal Index GetSortIndex(DataViewRowState recordStates)
        {
            IndexField[] indexDesc = this.GetIndexDesc();
            return this.columns[0].Table.GetIndex(indexDesc, recordStates, null);
        }

        internal bool RecordsEqual(int record1, int record2)
        {
            for (int i = 0; i < this.columns.Length; i++)
            {
                if (this.columns[i].Compare(record1, record2) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        internal DataColumn[] ToArray()
        {
            DataColumn[] columnArray = new DataColumn[this.columns.Length];
            for (int i = 0; i < this.columns.Length; i++)
            {
                columnArray[i] = this.columns[i];
            }
            return columnArray;
        }

        internal static int ColumnOrder(int indexDesc)
        {
            return (indexDesc & 0xffff);
        }

        internal static bool SortDecending(int indexDesc)
        {
            return ((indexDesc & -2147483648) != 0);
        }
    }
}


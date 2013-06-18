namespace System.Data
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class DataError
    {
        private int count;
        private ColumnError[] errorList;
        internal const int initialCapacity = 1;
        private string rowError;

        internal DataError()
        {
            this.rowError = string.Empty;
        }

        internal DataError(string rowError)
        {
            this.rowError = string.Empty;
            this.SetText(rowError);
        }

        internal void Clear()
        {
            for (int i = 0; i < this.count; i++)
            {
                this.errorList[i].column.errors--;
            }
            this.count = 0;
            this.rowError = string.Empty;
        }

        internal void Clear(DataColumn column)
        {
            if (this.count != 0)
            {
                for (int i = 0; i < this.count; i++)
                {
                    if (this.errorList[i].column == column)
                    {
                        Array.Copy(this.errorList, i + 1, this.errorList, i, (this.count - i) - 1);
                        this.count--;
                        column.errors--;
                    }
                }
            }
        }

        internal string GetColumnError(DataColumn column)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.errorList[i].column == column)
                {
                    return this.errorList[i].error;
                }
            }
            return string.Empty;
        }

        internal DataColumn[] GetColumnsInError()
        {
            DataColumn[] columnArray = new DataColumn[this.count];
            for (int i = 0; i < this.count; i++)
            {
                columnArray[i] = this.errorList[i].column;
            }
            return columnArray;
        }

        internal int IndexOf(DataColumn column)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.errorList[i].column == column)
                {
                    return i;
                }
            }
            if (this.count >= this.errorList.Length)
            {
                ColumnError[] destinationArray = new ColumnError[Math.Min(this.count * 2, column.Table.Columns.Count)];
                Array.Copy(this.errorList, 0, destinationArray, 0, this.count);
                this.errorList = destinationArray;
            }
            return this.count;
        }

        internal void SetColumnError(DataColumn column, string error)
        {
            if ((error == null) || (error.Length == 0))
            {
                this.Clear(column);
            }
            else
            {
                if (this.errorList == null)
                {
                    this.errorList = new ColumnError[1];
                }
                int index = this.IndexOf(column);
                this.errorList[index].column = column;
                this.errorList[index].error = error;
                column.errors++;
                if (index == this.count)
                {
                    this.count++;
                }
            }
        }

        private void SetText(string errorText)
        {
            if (errorText == null)
            {
                errorText = string.Empty;
            }
            this.rowError = errorText;
        }

        internal bool HasErrors
        {
            get
            {
                if (this.rowError.Length == 0)
                {
                    return (this.count != 0);
                }
                return true;
            }
        }

        internal string Text
        {
            get
            {
                return this.rowError;
            }
            set
            {
                this.SetText(value);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ColumnError
        {
            internal DataColumn column;
            internal string error;
        }
    }
}


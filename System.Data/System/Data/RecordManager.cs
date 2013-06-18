namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Reflection;

    internal sealed class RecordManager
    {
        private readonly List<int> freeRecordList = new List<int>();
        private int lastFreeRecord;
        private int minimumCapacity = 50;
        private int recordCapacity;
        private DataRow[] rows;
        private readonly DataTable table;

        internal RecordManager(DataTable table)
        {
            if (table == null)
            {
                throw ExceptionBuilder.ArgumentNull("table");
            }
            this.table = table;
        }

        internal void Clear(bool clearAll)
        {
            if (clearAll)
            {
                for (int i = 0; i < this.recordCapacity; i++)
                {
                    this.rows[i] = null;
                }
                int count = this.table.columnCollection.Count;
                for (int j = 0; j < count; j++)
                {
                    DataColumn column = this.table.columnCollection[j];
                    for (int k = 0; k < this.recordCapacity; k++)
                    {
                        column.FreeRecord(k);
                    }
                }
                this.lastFreeRecord = 0;
                this.freeRecordList.Clear();
            }
            else
            {
                this.freeRecordList.Capacity = this.freeRecordList.Count + this.table.Rows.Count;
                for (int m = 0; m < this.recordCapacity; m++)
                {
                    if ((this.rows[m] != null) && (this.rows[m].rowID != -1L))
                    {
                        int record = m;
                        this.FreeRecord(ref record);
                    }
                }
            }
        }

        internal int CopyRecord(DataTable src, int record, int copy)
        {
            if (record == -1)
            {
                return copy;
            }
            int num = -1;
            try
            {
                if (copy == -1)
                {
                    num = this.table.NewUninitializedRecord();
                }
                else
                {
                    num = copy;
                }
                int count = this.table.Columns.Count;
                for (int i = 0; i < count; i++)
                {
                    DataColumn column = this.table.Columns[i];
                    DataColumn column2 = src.Columns[column.ColumnName];
                    if (column2 != null)
                    {
                        object obj2 = column2[record];
                        ICloneable cloneable = obj2 as ICloneable;
                        if (cloneable != null)
                        {
                            column[num] = cloneable.Clone();
                        }
                        else
                        {
                            column[num] = obj2;
                        }
                    }
                    else if (-1 == copy)
                    {
                        column.Init(num);
                    }
                }
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableOrSecurityExceptionType(exception) && (-1 == copy))
                {
                    this.FreeRecord(ref num);
                }
                throw;
            }
            return num;
        }

        internal void FreeRecord(ref int record)
        {
            if (-1 != record)
            {
                this[record] = null;
                int count = this.table.columnCollection.Count;
                for (int i = 0; i < count; i++)
                {
                    this.table.columnCollection[i].FreeRecord(record);
                }
                if (this.lastFreeRecord == (record + 1))
                {
                    this.lastFreeRecord--;
                }
                else if (record < this.lastFreeRecord)
                {
                    this.freeRecordList.Add(record);
                }
                record = -1;
            }
        }

        private void GrowRecordCapacity()
        {
            if (NewCapacity(this.recordCapacity) < this.NormalizedMinimumCapacity(this.minimumCapacity))
            {
                this.RecordCapacity = this.NormalizedMinimumCapacity(this.minimumCapacity);
            }
            else
            {
                this.RecordCapacity = NewCapacity(this.recordCapacity);
            }
            DataRow[] destinationArray = this.table.NewRowArray(this.recordCapacity);
            if (this.rows != null)
            {
                Array.Copy(this.rows, 0, destinationArray, 0, Math.Min(this.lastFreeRecord, this.rows.Length));
            }
            this.rows = destinationArray;
        }

        internal int ImportRecord(DataTable src, int record)
        {
            return this.CopyRecord(src, record, -1);
        }

        internal static int NewCapacity(int capacity)
        {
            if (capacity >= 0x80)
            {
                return (capacity + capacity);
            }
            return 0x80;
        }

        internal int NewRecordBase()
        {
            int lastFreeRecord;
            if (this.freeRecordList.Count != 0)
            {
                lastFreeRecord = this.freeRecordList[this.freeRecordList.Count - 1];
                this.freeRecordList.RemoveAt(this.freeRecordList.Count - 1);
                return lastFreeRecord;
            }
            if (this.lastFreeRecord >= this.recordCapacity)
            {
                this.GrowRecordCapacity();
            }
            lastFreeRecord = this.lastFreeRecord;
            this.lastFreeRecord++;
            return lastFreeRecord;
        }

        private int NormalizedMinimumCapacity(int capacity)
        {
            if (capacity >= 0x3f6)
            {
                return ((((capacity + 10) >> 10) + 1) << 10);
            }
            if (capacity >= 0xf6)
            {
                return 0x400;
            }
            if (capacity < 0x36)
            {
                return 0x40;
            }
            return 0x100;
        }

        internal void SetKeyValues(int record, DataKey key, object[] keyValues)
        {
            for (int i = 0; i < keyValues.Length; i++)
            {
                key.ColumnsReference[i][record] = keyValues[i];
            }
        }

        internal void SetRowCache(DataRow[] newRows)
        {
            this.rows = newRows;
            this.lastFreeRecord = this.rows.Length;
            this.recordCapacity = this.lastFreeRecord;
        }

        [Conditional("DEBUG")]
        internal void VerifyRecord(int record)
        {
        }

        [Conditional("DEBUG")]
        internal void VerifyRecord(int record, DataRow row)
        {
        }

        internal DataRow this[int record]
        {
            get
            {
                return this.rows[record];
            }
            set
            {
                this.rows[record] = value;
            }
        }

        internal int LastFreeRecord
        {
            get
            {
                return this.lastFreeRecord;
            }
        }

        internal int MinimumCapacity
        {
            get
            {
                return this.minimumCapacity;
            }
            set
            {
                if (this.minimumCapacity != value)
                {
                    if (value < 0)
                    {
                        throw ExceptionBuilder.NegativeMinimumCapacity();
                    }
                    this.minimumCapacity = value;
                }
            }
        }

        internal int RecordCapacity
        {
            get
            {
                return this.recordCapacity;
            }
            set
            {
                if (this.recordCapacity != value)
                {
                    for (int i = 0; i < this.table.Columns.Count; i++)
                    {
                        this.table.Columns[i].SetCapacity(value);
                    }
                    this.recordCapacity = value;
                }
            }
        }
    }
}


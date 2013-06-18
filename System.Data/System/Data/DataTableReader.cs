namespace System.Data
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    public sealed class DataTableReader : DbDataReader
    {
        private DataRow currentDataRow;
        private DataTable currentDataTable;
        private bool currentRowRemoved;
        private bool hasRows;
        private bool isOpen;
        private DataTableReaderListener listener;
        private bool reachEORows;
        private bool readerIsInvalid;
        private int rowCounter;
        private bool schemaIsChanged;
        private DataTable schemaTable;
        private bool started;
        private bool tableCleared;
        private int tableCounter;
        private readonly DataTable[] tables;

        public DataTableReader(DataTable dataTable)
        {
            this.isOpen = true;
            this.tableCounter = -1;
            this.rowCounter = -1;
            this.hasRows = true;
            if (dataTable == null)
            {
                throw ExceptionBuilder.ArgumentNull("DataTable");
            }
            this.tables = new DataTable[] { dataTable };
            this.init();
        }

        public DataTableReader(DataTable[] dataTables)
        {
            this.isOpen = true;
            this.tableCounter = -1;
            this.rowCounter = -1;
            this.hasRows = true;
            if (dataTables == null)
            {
                throw ExceptionBuilder.ArgumentNull("DataTable");
            }
            if (dataTables.Length == 0)
            {
                throw ExceptionBuilder.DataTableReaderArgumentIsEmpty();
            }
            this.tables = new DataTable[dataTables.Length];
            for (int i = 0; i < dataTables.Length; i++)
            {
                if (dataTables[i] == null)
                {
                    throw ExceptionBuilder.ArgumentNull("DataTable");
                }
                this.tables[i] = dataTables[i];
            }
            this.init();
        }

        public override void Close()
        {
            if (this.isOpen)
            {
                if (this.listener != null)
                {
                    this.listener.CleanUp();
                }
                this.listener = null;
                this.schemaTable = null;
                this.isOpen = false;
            }
        }

        internal void DataChanged(DataRowChangeEventArgs args)
        {
            if (this.started && ((this.rowCounter != -1) || this.tableCleared))
            {
                DataRowAction action = args.Action;
                if (action <= DataRowAction.Rollback)
                {
                    if ((action != DataRowAction.Delete) && (action != DataRowAction.Rollback))
                    {
                        return;
                    }
                }
                else if (action != DataRowAction.Commit)
                {
                    if (action == DataRowAction.Add)
                    {
                        this.ValidateRow(this.rowCounter + 1);
                        if (this.currentDataRow == this.currentDataTable.Rows[this.rowCounter + 1])
                        {
                            this.rowCounter++;
                            return;
                        }
                    }
                    return;
                }
                if (args.Row.RowState == DataRowState.Detached)
                {
                    if (args.Row != this.currentDataRow)
                    {
                        if (this.rowCounter != 0)
                        {
                            this.ValidateRow(this.rowCounter - 1);
                            if (this.currentDataRow == this.currentDataTable.Rows[this.rowCounter - 1])
                            {
                                this.rowCounter--;
                            }
                        }
                    }
                    else
                    {
                        this.currentRowRemoved = true;
                        if (this.rowCounter > 0)
                        {
                            this.rowCounter--;
                            this.currentDataRow = this.currentDataTable.Rows[this.rowCounter];
                        }
                        else
                        {
                            this.rowCounter = -1;
                            this.currentDataRow = null;
                        }
                    }
                }
            }
        }

        internal void DataTableCleared()
        {
            if (this.started)
            {
                this.rowCounter = -1;
                if (!this.reachEORows)
                {
                    this.currentRowRemoved = true;
                }
            }
        }

        public override bool GetBoolean(int ordinal)
        {
            bool flag;
            this.ValidateState("GetBoolean");
            this.ValidateReader();
            try
            {
                flag = (bool) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return flag;
        }

        public override byte GetByte(int ordinal)
        {
            byte num;
            this.ValidateState("GetByte");
            this.ValidateReader();
            try
            {
                num = (byte) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return num;
        }

        public override long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            byte[] buffer2;
            this.ValidateState("GetBytes");
            this.ValidateReader();
            try
            {
                buffer2 = (byte[]) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            if (buffer == null)
            {
                return (long) buffer2.Length;
            }
            int num2 = (int) dataIndex;
            int num = Math.Min(buffer2.Length - num2, length);
            if (num2 < 0)
            {
                throw ADP.InvalidSourceBufferIndex(buffer2.Length, (long) num2, "dataIndex");
            }
            if ((bufferIndex < 0) || ((bufferIndex > 0) && (bufferIndex >= buffer.Length)))
            {
                throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }
            if (0 < num)
            {
                Array.Copy(buffer2, dataIndex, buffer, (long) bufferIndex, (long) num);
            }
            else
            {
                if (length < 0)
                {
                    throw ADP.InvalidDataLength((long) length);
                }
                num = 0;
            }
            return (long) num;
        }

        public override char GetChar(int ordinal)
        {
            char ch;
            this.ValidateState("GetChar");
            this.ValidateReader();
            try
            {
                ch = (char) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return ch;
        }

        public override long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            char[] chArray;
            this.ValidateState("GetChars");
            this.ValidateReader();
            try
            {
                chArray = (char[]) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            if (buffer == null)
            {
                return (long) chArray.Length;
            }
            int num2 = (int) dataIndex;
            int num = Math.Min(chArray.Length - num2, length);
            if (num2 < 0)
            {
                throw ADP.InvalidSourceBufferIndex(chArray.Length, (long) num2, "dataIndex");
            }
            if ((bufferIndex < 0) || ((bufferIndex > 0) && (bufferIndex >= buffer.Length)))
            {
                throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }
            if (0 < num)
            {
                Array.Copy(chArray, dataIndex, buffer, (long) bufferIndex, (long) num);
            }
            else
            {
                if (length < 0)
                {
                    throw ADP.InvalidDataLength((long) length);
                }
                num = 0;
            }
            return (long) num;
        }

        public override string GetDataTypeName(int ordinal)
        {
            this.ValidateOpen("GetDataTypeName");
            this.ValidateReader();
            return this.GetFieldType(ordinal).Name;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            DateTime time;
            this.ValidateState("GetDateTime");
            this.ValidateReader();
            try
            {
                time = (DateTime) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return time;
        }

        public override decimal GetDecimal(int ordinal)
        {
            decimal num;
            this.ValidateState("GetDecimal");
            this.ValidateReader();
            try
            {
                num = (decimal) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return num;
        }

        public override double GetDouble(int ordinal)
        {
            double num;
            this.ValidateState("GetDouble");
            this.ValidateReader();
            try
            {
                num = (double) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return num;
        }

        public override IEnumerator GetEnumerator()
        {
            this.ValidateOpen("GetEnumerator");
            return new DbEnumerator(this);
        }

        public override Type GetFieldType(int ordinal)
        {
            Type dataType;
            this.ValidateOpen("GetFieldType");
            this.ValidateReader();
            try
            {
                dataType = this.currentDataTable.Columns[ordinal].DataType;
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return dataType;
        }

        public override float GetFloat(int ordinal)
        {
            float num;
            this.ValidateState("GetFloat");
            this.ValidateReader();
            try
            {
                num = (float) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return num;
        }

        public override Guid GetGuid(int ordinal)
        {
            Guid guid;
            this.ValidateState("GetGuid");
            this.ValidateReader();
            try
            {
                guid = (Guid) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return guid;
        }

        public override short GetInt16(int ordinal)
        {
            short num;
            this.ValidateState("GetInt16");
            this.ValidateReader();
            try
            {
                num = (short) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return num;
        }

        public override int GetInt32(int ordinal)
        {
            int num;
            this.ValidateState("GetInt32");
            this.ValidateReader();
            try
            {
                num = (int) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return num;
        }

        public override long GetInt64(int ordinal)
        {
            long num;
            this.ValidateState("GetInt64");
            this.ValidateReader();
            try
            {
                num = (long) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return num;
        }

        public override string GetName(int ordinal)
        {
            string columnName;
            this.ValidateOpen("GetName");
            this.ValidateReader();
            try
            {
                columnName = this.currentDataTable.Columns[ordinal].ColumnName;
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return columnName;
        }

        public override int GetOrdinal(string name)
        {
            this.ValidateOpen("GetOrdinal");
            this.ValidateReader();
            DataColumn column = this.currentDataTable.Columns[name];
            if (column == null)
            {
                throw ExceptionBuilder.ColumnNotInTheTable(name, this.currentDataTable.TableName);
            }
            return column.Ordinal;
        }

        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            this.ValidateOpen("GetProviderSpecificFieldType");
            this.ValidateReader();
            return this.GetFieldType(ordinal);
        }

        public override object GetProviderSpecificValue(int ordinal)
        {
            this.ValidateOpen("GetProviderSpecificValue");
            this.ValidateReader();
            return this.GetValue(ordinal);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            this.ValidateOpen("GetProviderSpecificValues");
            this.ValidateReader();
            return this.GetValues(values);
        }

        public override DataTable GetSchemaTable()
        {
            this.ValidateOpen("GetSchemaTable");
            this.ValidateReader();
            if (this.schemaTable == null)
            {
                this.schemaTable = GetSchemaTableFromDataTable(this.currentDataTable);
            }
            return this.schemaTable;
        }

        internal static DataTable GetSchemaTableFromDataTable(DataTable table)
        {
            if (table == null)
            {
                throw ExceptionBuilder.ArgumentNull("DataTable");
            }
            DataTable table2 = new DataTable("SchemaTable") {
                Locale = CultureInfo.InvariantCulture
            };
            DataColumn column22 = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
            DataColumn column21 = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
            DataColumn column7 = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
            DataColumn column27 = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
            DataColumn column26 = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
            DataColumn column20 = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
            DataColumn column25 = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));
            DataColumn column19 = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
            DataColumn column18 = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
            DataColumn column6 = new DataColumn(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
            DataColumn column17 = new DataColumn(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
            DataColumn column16 = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
            DataColumn column5 = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
            DataColumn column4 = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
            DataColumn column24 = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
            DataColumn column15 = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
            DataColumn column14 = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
            DataColumn column13 = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
            DataColumn column3 = new DataColumn(SchemaTableOptionalColumn.AutoIncrementSeed, typeof(long));
            DataColumn column2 = new DataColumn(SchemaTableOptionalColumn.AutoIncrementStep, typeof(long));
            DataColumn column12 = new DataColumn(SchemaTableOptionalColumn.DefaultValue, typeof(object));
            DataColumn column11 = new DataColumn(SchemaTableOptionalColumn.Expression, typeof(string));
            DataColumn column10 = new DataColumn(SchemaTableOptionalColumn.ColumnMapping, typeof(MappingType));
            DataColumn column9 = new DataColumn(SchemaTableOptionalColumn.BaseTableNamespace, typeof(string));
            DataColumn column8 = new DataColumn(SchemaTableOptionalColumn.BaseColumnNamespace, typeof(string));
            column7.DefaultValue = -1;
            if (table.DataSet != null)
            {
                column15.DefaultValue = table.DataSet.DataSetName;
            }
            column14.DefaultValue = table.TableName;
            column9.DefaultValue = table.Namespace;
            column17.DefaultValue = false;
            column19.DefaultValue = false;
            column6.DefaultValue = false;
            column5.DefaultValue = false;
            column4.DefaultValue = false;
            column3.DefaultValue = 0;
            column2.DefaultValue = 1;
            table2.Columns.Add(column22);
            table2.Columns.Add(column21);
            table2.Columns.Add(column7);
            table2.Columns.Add(column27);
            table2.Columns.Add(column26);
            table2.Columns.Add(column20);
            table2.Columns.Add(column25);
            table2.Columns.Add(column19);
            table2.Columns.Add(column18);
            table2.Columns.Add(column6);
            table2.Columns.Add(column17);
            table2.Columns.Add(column16);
            table2.Columns.Add(column5);
            table2.Columns.Add(column4);
            table2.Columns.Add(column15);
            table2.Columns.Add(column24);
            table2.Columns.Add(column14);
            table2.Columns.Add(column13);
            table2.Columns.Add(column3);
            table2.Columns.Add(column2);
            table2.Columns.Add(column12);
            table2.Columns.Add(column11);
            table2.Columns.Add(column10);
            table2.Columns.Add(column9);
            table2.Columns.Add(column8);
            foreach (DataColumn column in table.Columns)
            {
                DataRow row = table2.NewRow();
                row[column22] = column.ColumnName;
                row[column21] = column.Ordinal;
                row[column20] = column.DataType;
                if (column.DataType == typeof(string))
                {
                    row[column7] = column.MaxLength;
                }
                row[column18] = column.AllowDBNull;
                row[column6] = column.ReadOnly;
                row[column16] = column.Unique;
                if (column.AutoIncrement)
                {
                    row[column4] = true;
                    row[column3] = column.AutoIncrementSeed;
                    row[column2] = column.AutoIncrementStep;
                }
                if (column.DefaultValue != DBNull.Value)
                {
                    row[column12] = column.DefaultValue;
                }
                if (column.Expression.Length != 0)
                {
                    bool flag = false;
                    DataColumn[] dependency = column.DataExpression.GetDependency();
                    for (int i = 0; i < dependency.Length; i++)
                    {
                        if (dependency[i].Table != table)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        row[column11] = column.Expression;
                    }
                }
                row[column10] = column.ColumnMapping;
                row[column13] = column.ColumnName;
                row[column8] = column.Namespace;
                table2.Rows.Add(row);
            }
            foreach (DataColumn column23 in table.PrimaryKey)
            {
                table2.Rows[column23.Ordinal][column5] = true;
            }
            table2.AcceptChanges();
            return table2;
        }

        public override string GetString(int ordinal)
        {
            string str;
            this.ValidateState("GetString");
            this.ValidateReader();
            try
            {
                str = (string) this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return str;
        }

        public override object GetValue(int ordinal)
        {
            object obj2;
            this.ValidateState("GetValue");
            this.ValidateReader();
            try
            {
                obj2 = this.currentDataRow[ordinal];
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return obj2;
        }

        public override int GetValues(object[] values)
        {
            this.ValidateState("GetValues");
            this.ValidateReader();
            if (values == null)
            {
                throw ExceptionBuilder.ArgumentNull("values");
            }
            Array.Copy(this.currentDataRow.ItemArray, values, (this.currentDataRow.ItemArray.Length > values.Length) ? values.Length : this.currentDataRow.ItemArray.Length);
            if (this.currentDataRow.ItemArray.Length <= values.Length)
            {
                return this.currentDataRow.ItemArray.Length;
            }
            return values.Length;
        }

        private void init()
        {
            this.tableCounter = 0;
            this.reachEORows = false;
            this.schemaIsChanged = false;
            this.currentDataTable = this.tables[this.tableCounter];
            this.hasRows = this.currentDataTable.Rows.Count > 0;
            this.ReaderIsInvalid = false;
            this.listener = new DataTableReaderListener(this);
        }

        public override bool IsDBNull(int ordinal)
        {
            bool flag;
            this.ValidateState("IsDBNull");
            this.ValidateReader();
            try
            {
                flag = this.currentDataRow.IsNull(ordinal);
            }
            catch (IndexOutOfRangeException exception)
            {
                ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            return flag;
        }

        public override bool NextResult()
        {
            this.ValidateOpen("NextResult");
            if (this.tableCounter == (this.tables.Length - 1))
            {
                return false;
            }
            this.currentDataTable = this.tables[++this.tableCounter];
            if (this.listener != null)
            {
                this.listener.UpdataTable(this.currentDataTable);
            }
            this.schemaTable = null;
            this.rowCounter = -1;
            this.currentRowRemoved = false;
            this.reachEORows = false;
            this.schemaIsChanged = false;
            this.started = false;
            this.ReaderIsInvalid = false;
            this.tableCleared = false;
            this.hasRows = this.currentDataTable.Rows.Count > 0;
            return true;
        }

        public override bool Read()
        {
            if (!this.started)
            {
                this.started = true;
            }
            this.ValidateOpen("Read");
            this.ValidateReader();
            if (this.reachEORows)
            {
                return false;
            }
            if (this.rowCounter >= (this.currentDataTable.Rows.Count - 1))
            {
                this.reachEORows = true;
                if (this.listener != null)
                {
                    this.listener.CleanUp();
                }
                return false;
            }
            this.rowCounter++;
            this.ValidateRow(this.rowCounter);
            this.currentDataRow = this.currentDataTable.Rows[this.rowCounter];
            while (this.currentDataRow.RowState == DataRowState.Deleted)
            {
                this.rowCounter++;
                if (this.rowCounter == this.currentDataTable.Rows.Count)
                {
                    this.reachEORows = true;
                    if (this.listener != null)
                    {
                        this.listener.CleanUp();
                    }
                    return false;
                }
                this.ValidateRow(this.rowCounter);
                this.currentDataRow = this.currentDataTable.Rows[this.rowCounter];
            }
            if (this.currentRowRemoved)
            {
                this.currentRowRemoved = false;
            }
            return true;
        }

        internal void SchemaChanged()
        {
            this.IsSchemaChanged = true;
        }

        private void ValidateOpen(string caller)
        {
            if (!this.isOpen)
            {
                throw ADP.DataReaderClosed(caller);
            }
        }

        private void ValidateReader()
        {
            if (this.ReaderIsInvalid)
            {
                throw ExceptionBuilder.InvalidDataTableReader(this.currentDataTable.TableName);
            }
            if (this.IsSchemaChanged)
            {
                throw ExceptionBuilder.DataTableReaderSchemaIsInvalid(this.currentDataTable.TableName);
            }
        }

        private void ValidateRow(int rowPosition)
        {
            if (this.ReaderIsInvalid)
            {
                throw ExceptionBuilder.InvalidDataTableReader(this.currentDataTable.TableName);
            }
            if ((0 > rowPosition) || (this.currentDataTable.Rows.Count <= rowPosition))
            {
                this.ReaderIsInvalid = true;
                throw ExceptionBuilder.InvalidDataTableReader(this.currentDataTable.TableName);
            }
        }

        private void ValidateState(string caller)
        {
            this.ValidateOpen(caller);
            if (this.tableCleared)
            {
                throw ExceptionBuilder.EmptyDataTableReader(this.currentDataTable.TableName);
            }
            if ((this.currentDataRow == null) || (this.currentDataTable == null))
            {
                this.ReaderIsInvalid = true;
                throw ExceptionBuilder.InvalidDataTableReader(this.currentDataTable.TableName);
            }
            if (((this.currentDataRow.RowState == DataRowState.Deleted) || (this.currentDataRow.RowState == DataRowState.Detached)) || this.currentRowRemoved)
            {
                throw ExceptionBuilder.InvalidCurrentRowInDataTableReader();
            }
            if ((0 > this.rowCounter) || (this.currentDataTable.Rows.Count <= this.rowCounter))
            {
                this.ReaderIsInvalid = true;
                throw ExceptionBuilder.InvalidDataTableReader(this.currentDataTable.TableName);
            }
        }

        internal DataTable CurrentDataTable
        {
            get
            {
                return this.currentDataTable;
            }
        }

        public override int Depth
        {
            get
            {
                this.ValidateOpen("Depth");
                this.ValidateReader();
                return 0;
            }
        }

        public override int FieldCount
        {
            get
            {
                this.ValidateOpen("FieldCount");
                this.ValidateReader();
                return this.currentDataTable.Columns.Count;
            }
        }

        public override bool HasRows
        {
            get
            {
                this.ValidateOpen("HasRows");
                this.ValidateReader();
                return this.hasRows;
            }
        }

        public override bool IsClosed
        {
            get
            {
                return !this.isOpen;
            }
        }

        private bool IsSchemaChanged
        {
            get
            {
                return this.schemaIsChanged;
            }
            set
            {
                if (value && (this.schemaIsChanged != value))
                {
                    this.schemaIsChanged = value;
                    if (this.listener != null)
                    {
                        this.listener.CleanUp();
                    }
                }
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                object obj2;
                this.ValidateOpen("Item");
                this.ValidateReader();
                if ((this.currentDataRow == null) || (this.currentDataRow.RowState == DataRowState.Deleted))
                {
                    this.ReaderIsInvalid = true;
                    throw ExceptionBuilder.InvalidDataTableReader(this.currentDataTable.TableName);
                }
                try
                {
                    obj2 = this.currentDataRow[ordinal];
                }
                catch (IndexOutOfRangeException exception)
                {
                    ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                    throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
                }
                return obj2;
            }
        }

        public override object this[string name]
        {
            get
            {
                this.ValidateOpen("Item");
                this.ValidateReader();
                if ((this.currentDataRow != null) && (this.currentDataRow.RowState != DataRowState.Deleted))
                {
                    return this.currentDataRow[name];
                }
                this.ReaderIsInvalid = true;
                throw ExceptionBuilder.InvalidDataTableReader(this.currentDataTable.TableName);
            }
        }

        private bool ReaderIsInvalid
        {
            get
            {
                return this.readerIsInvalid;
            }
            set
            {
                if (this.readerIsInvalid != value)
                {
                    this.readerIsInvalid = value;
                    if (this.readerIsInvalid && (this.listener != null))
                    {
                        this.listener.CleanUp();
                    }
                }
            }
        }

        public override int RecordsAffected
        {
            get
            {
                this.ValidateReader();
                return 0;
            }
        }
    }
}


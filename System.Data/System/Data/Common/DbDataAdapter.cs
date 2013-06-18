namespace System.Data.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.ProviderBase;
    using System.Runtime.InteropServices;

    public abstract class DbDataAdapter : DataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
    {
        private IDbCommand _deleteCommand;
        private CommandBehavior _fillCommandBehavior;
        private IDbCommand _insertCommand;
        private IDbCommand _selectCommand;
        private IDbCommand _updateCommand;
        public const string DefaultSourceTableName = "Table";
        internal static readonly object ParameterValueNonNullValue = 0;
        internal static readonly object ParameterValueNullValue = 1;

        protected DbDataAdapter()
        {
        }

        protected DbDataAdapter(DbDataAdapter adapter) : base(adapter)
        {
            this.CloneFrom(adapter);
        }

        protected virtual int AddToBatch(IDbCommand command)
        {
            throw ADP.NotSupported();
        }

        protected virtual void ClearBatch()
        {
            throw ADP.NotSupported();
        }

        private IDbCommand CloneCommand(IDbCommand command)
        {
            return ((command is ICloneable) ? ((IDbCommand) ((ICloneable) command).Clone()) : null);
        }

        private void CloneFrom(DbDataAdapter from)
        {
            IDbDataAdapter adapter = from._IDbDataAdapter;
            this._IDbDataAdapter.SelectCommand = this.CloneCommand(adapter.SelectCommand);
            this._IDbDataAdapter.InsertCommand = this.CloneCommand(adapter.InsertCommand);
            this._IDbDataAdapter.UpdateCommand = this.CloneCommand(adapter.UpdateCommand);
            this._IDbDataAdapter.DeleteCommand = this.CloneCommand(adapter.DeleteCommand);
        }

        protected virtual RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new RowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected virtual RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new RowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IDbDataAdapter adapter = this;
                adapter.SelectCommand = null;
                adapter.InsertCommand = null;
                adapter.UpdateCommand = null;
                adapter.DeleteCommand = null;
            }
            base.Dispose(disposing);
        }

        protected virtual int ExecuteBatch()
        {
            throw ADP.NotSupported();
        }

        public override int Fill(DataSet dataSet)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> %d#, dataSet\n", base.ObjectID);
            try
            {
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                num = this.Fill(dataSet, 0, 0, "Table", selectCommand, fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public int Fill(DataTable dataTable)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> %d#, dataTable\n", base.ObjectID);
            try
            {
                DataTable[] dataTables = new DataTable[] { dataTable };
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                num = this.Fill(dataTables, 0, 0, selectCommand, fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public int Fill(DataSet dataSet, string srcTable)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> %d#, dataSet, srcTable='%ls'\n", base.ObjectID, srcTable);
            try
            {
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                num = this.Fill(dataSet, 0, 0, srcTable, selectCommand, fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        protected virtual int Fill(DataTable dataTable, IDbCommand command, CommandBehavior behavior)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> dataTable, command, behavior=%d{ds.CommandBehavior}%d#\n", base.ObjectID, (int) behavior);
            try
            {
                DataTable[] dataTables = new DataTable[] { dataTable };
                num = this.Fill(dataTables, 0, 0, command, behavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public int Fill(int startRecord, int maxRecords, params DataTable[] dataTables)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> %d#, startRecord=%d, maxRecords=%d, dataTable[]\n", base.ObjectID, startRecord, maxRecords);
            try
            {
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                num = this.Fill(dataTables, startRecord, maxRecords, selectCommand, fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> %d#, dataSet, startRecord=%d, maxRecords=%d, srcTable='%ls'\n", base.ObjectID, startRecord, maxRecords, srcTable);
            try
            {
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                num = this.Fill(dataSet, startRecord, maxRecords, srcTable, selectCommand, fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        protected virtual int Fill(DataTable[] dataTables, int startRecord, int maxRecords, IDbCommand command, CommandBehavior behavior)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> %d#, dataTables[], startRecord, maxRecords, command, behavior=%d{ds.CommandBehavior}\n", base.ObjectID, (int) behavior);
            try
            {
                if (((dataTables == null) || (dataTables.Length == 0)) || (dataTables[0] == null))
                {
                    throw ADP.FillRequires("dataTable");
                }
                if (startRecord < 0)
                {
                    throw ADP.InvalidStartRecord("startRecord", startRecord);
                }
                if (maxRecords < 0)
                {
                    throw ADP.InvalidMaxRecords("maxRecords", maxRecords);
                }
                if ((1 < dataTables.Length) && ((startRecord != 0) || (maxRecords != 0)))
                {
                    throw ADP.OnlyOneTableForStartRecordOrMaxRecords();
                }
                if (command == null)
                {
                    throw ADP.MissingSelectCommand("Fill");
                }
                if (1 == dataTables.Length)
                {
                    behavior |= CommandBehavior.SingleResult;
                }
                num = this.FillInternal(null, dataTables, startRecord, maxRecords, null, command, behavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        protected virtual int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior)
        {
            int num;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Fill|API> %d#, dataSet, startRecord, maxRecords, srcTable, command, behavior=%d{ds.CommandBehavior}\n", base.ObjectID, (int) behavior);
            try
            {
                if (dataSet == null)
                {
                    throw ADP.FillRequires("dataSet");
                }
                if (startRecord < 0)
                {
                    throw ADP.InvalidStartRecord("startRecord", startRecord);
                }
                if (maxRecords < 0)
                {
                    throw ADP.InvalidMaxRecords("maxRecords", maxRecords);
                }
                if (ADP.IsEmpty(srcTable))
                {
                    throw ADP.FillRequiresSourceTableName("srcTable");
                }
                if (command == null)
                {
                    throw ADP.MissingSelectCommand("Fill");
                }
                num = this.FillInternal(dataSet, null, startRecord, maxRecords, srcTable, command, behavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        private int FillInternal(DataSet dataset, DataTable[] datatables, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior)
        {
            bool flag = null == command.Connection;
            try
            {
                IDbConnection connection = GetConnection3(this, command, "Fill");
                ConnectionState open = ConnectionState.Open;
                if (MissingSchemaAction.AddWithKey == base.MissingSchemaAction)
                {
                    behavior |= CommandBehavior.KeyInfo;
                }
                try
                {
                    QuietOpen(connection, out open);
                    behavior |= CommandBehavior.SequentialAccess;
                    using (IDataReader reader = null)
                    {
                        reader = command.ExecuteReader(behavior);
                        if (datatables != null)
                        {
                            return this.Fill(datatables, reader, startRecord, maxRecords);
                        }
                        return this.Fill(dataset, srcTable, reader, startRecord, maxRecords);
                    }
                }
                finally
                {
                    QuietClose(connection, open);
                }
            }
            finally
            {
                if (flag)
                {
                    command.Transaction = null;
                    command.Connection = null;
                }
            }
            return 0;
        }

        public override DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
        {
            DataTable[] tableArray;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.FillSchema|API> %d#, dataSet, schemaType=%d{ds.SchemaType}\n", base.ObjectID, (int) schemaType);
            try
            {
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                if (base.DesignMode && (((selectCommand == null) || (selectCommand.Connection == null)) || ADP.IsEmpty(selectCommand.CommandText)))
                {
                    return new DataTable[0];
                }
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                tableArray = this.FillSchema(dataSet, schemaType, selectCommand, "Table", fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return tableArray;
        }

        public DataTable FillSchema(DataTable dataTable, SchemaType schemaType)
        {
            DataTable table;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.FillSchema|API> %d#, dataTable, schemaType=%d{ds.SchemaType}\n", base.ObjectID, (int) schemaType);
            try
            {
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                table = this.FillSchema(dataTable, schemaType, selectCommand, fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return table;
        }

        public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, string srcTable)
        {
            DataTable[] tableArray;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.FillSchema|API> %d#, dataSet, schemaType=%d{ds.SchemaType}, srcTable=%ls%\n", base.ObjectID, (int) schemaType, srcTable);
            try
            {
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                CommandBehavior fillCommandBehavior = this.FillCommandBehavior;
                tableArray = this.FillSchema(dataSet, schemaType, selectCommand, srcTable, fillCommandBehavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return tableArray;
        }

        protected virtual DataTable FillSchema(DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior)
        {
            DataTable table;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.FillSchema|API> %d#, dataTable, schemaType, command, behavior=%d{ds.CommandBehavior}\n", base.ObjectID, (int) behavior);
            try
            {
                if (dataTable == null)
                {
                    throw ADP.ArgumentNull("dataTable");
                }
                if ((SchemaType.Source != schemaType) && (SchemaType.Mapped != schemaType))
                {
                    throw ADP.InvalidSchemaType(schemaType);
                }
                if (command == null)
                {
                    throw ADP.MissingSelectCommand("FillSchema");
                }
                string tableName = dataTable.TableName;
                int num = base.IndexOfDataSetTable(tableName);
                if (-1 != num)
                {
                    tableName = base.TableMappings[num].SourceTable;
                }
                table = (DataTable) this.FillSchemaInternal(null, dataTable, schemaType, command, tableName, behavior | CommandBehavior.SingleResult);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return table;
        }

        protected virtual DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior)
        {
            DataTable[] tableArray;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.FillSchema|API> %d#, dataSet, schemaType, command, srcTable, behavior=%d{ds.CommandBehavior}\n", base.ObjectID, (int) behavior);
            try
            {
                if (dataSet == null)
                {
                    throw ADP.ArgumentNull("dataSet");
                }
                if ((SchemaType.Source != schemaType) && (SchemaType.Mapped != schemaType))
                {
                    throw ADP.InvalidSchemaType(schemaType);
                }
                if (ADP.IsEmpty(srcTable))
                {
                    throw ADP.FillSchemaRequiresSourceTableName("srcTable");
                }
                if (command == null)
                {
                    throw ADP.MissingSelectCommand("FillSchema");
                }
                tableArray = (DataTable[]) this.FillSchemaInternal(dataSet, null, schemaType, command, srcTable, behavior);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return tableArray;
        }

        private object FillSchemaInternal(DataSet dataset, DataTable datatable, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior)
        {
            bool flag = null == command.Connection;
            try
            {
                IDbConnection connection = GetConnection3(this, command, "FillSchema");
                ConnectionState open = ConnectionState.Open;
                try
                {
                    QuietOpen(connection, out open);
                    using (IDataReader reader = command.ExecuteReader((behavior | CommandBehavior.SchemaOnly) | CommandBehavior.KeyInfo))
                    {
                        if (datatable != null)
                        {
                            return this.FillSchema(datatable, schemaType, reader);
                        }
                        return this.FillSchema(dataset, schemaType, srcTable, reader);
                    }
                }
                finally
                {
                    QuietClose(connection, open);
                }
            }
            finally
            {
                if (flag)
                {
                    command.Transaction = null;
                    command.Connection = null;
                }
            }
            return null;
        }

        protected virtual IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
        {
            throw ADP.NotSupported();
        }

        protected virtual bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out Exception error)
        {
            recordsAffected = 1;
            error = null;
            return true;
        }

        private static IDbConnection GetConnection1(DbDataAdapter adapter)
        {
            IDbCommand selectCommand = adapter._IDbDataAdapter.SelectCommand;
            if (selectCommand == null)
            {
                selectCommand = adapter._IDbDataAdapter.InsertCommand;
                if (selectCommand == null)
                {
                    selectCommand = adapter._IDbDataAdapter.UpdateCommand;
                    if (selectCommand == null)
                    {
                        selectCommand = adapter._IDbDataAdapter.DeleteCommand;
                    }
                }
            }
            IDbConnection connection = null;
            if (selectCommand != null)
            {
                connection = selectCommand.Connection;
            }
            if (connection == null)
            {
                throw ADP.UpdateConnectionRequired(StatementType.Batch, false);
            }
            return connection;
        }

        private static IDbConnection GetConnection3(DbDataAdapter adapter, IDbCommand command, string method)
        {
            IDbConnection connection = command.Connection;
            if (connection == null)
            {
                throw ADP.ConnectionRequired_Res(method);
            }
            return connection;
        }

        private static IDbConnection GetConnection4(DbDataAdapter adapter, IDbCommand command, StatementType statementType, bool isCommandFromRowUpdating)
        {
            IDbConnection connection = command.Connection;
            if (connection == null)
            {
                throw ADP.UpdateConnectionRequired(statementType, isCommandFromRowUpdating);
            }
            return connection;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override IDataParameter[] GetFillParameters()
        {
            IDataParameter[] array = null;
            IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
            if (selectCommand != null)
            {
                IDataParameterCollection parameters = selectCommand.Parameters;
                if (parameters != null)
                {
                    array = new IDataParameter[parameters.Count];
                    parameters.CopyTo(array, 0);
                }
            }
            if (array == null)
            {
                array = new IDataParameter[0];
            }
            return array;
        }

        private static DataRowVersion GetParameterSourceVersion(StatementType statementType, IDataParameter parameter)
        {
            switch (statementType)
            {
                case StatementType.Select:
                case StatementType.Batch:
                    throw ADP.UnwantedStatementType(statementType);

                case StatementType.Insert:
                    return DataRowVersion.Current;

                case StatementType.Update:
                    return parameter.SourceVersion;

                case StatementType.Delete:
                    return DataRowVersion.Original;
            }
            throw ADP.InvalidStatementType(statementType);
        }

        internal DataTableMapping GetTableMapping(DataTable dataTable)
        {
            DataTableMapping mapping = null;
            int num = base.IndexOfDataSetTable(dataTable.TableName);
            if (-1 != num)
            {
                mapping = base.TableMappings[num];
            }
            if (mapping != null)
            {
                return mapping;
            }
            if (MissingMappingAction.Error == base.MissingMappingAction)
            {
                throw ADP.MissingTableMappingDestination(dataTable.TableName);
            }
            return new DataTableMapping(dataTable.TableName, dataTable.TableName);
        }

        protected virtual void InitializeBatching()
        {
            throw ADP.NotSupported();
        }

        protected virtual void OnRowUpdated(RowUpdatedEventArgs value)
        {
        }

        protected virtual void OnRowUpdating(RowUpdatingEventArgs value)
        {
        }

        private void ParameterInput(IDataParameterCollection parameters, StatementType typeIndex, DataRow row, DataTableMapping mappings)
        {
            MissingMappingAction updateMappingAction = this.UpdateMappingAction;
            MissingSchemaAction updateSchemaAction = this.UpdateSchemaAction;
            foreach (IDataParameter parameter in parameters)
            {
                if ((parameter != null) && ((ParameterDirection.Input & parameter.Direction) != ((ParameterDirection) 0)))
                {
                    string sourceColumn = parameter.SourceColumn;
                    if (!ADP.IsEmpty(sourceColumn))
                    {
                        DataColumn column = mappings.GetDataColumn(sourceColumn, null, row.Table, updateMappingAction, updateSchemaAction);
                        if (column != null)
                        {
                            DataRowVersion parameterSourceVersion = GetParameterSourceVersion(typeIndex, parameter);
                            parameter.Value = row[column, parameterSourceVersion];
                        }
                        else
                        {
                            parameter.Value = null;
                        }
                        DbParameter parameter2 = parameter as DbParameter;
                        if ((parameter2 != null) && parameter2.SourceColumnNullMapping)
                        {
                            parameter.Value = ADP.IsNull(parameter.Value) ? ParameterValueNullValue : ParameterValueNonNullValue;
                        }
                    }
                }
            }
        }

        private void ParameterOutput(IDataParameterCollection parameters, DataRow row, DataTableMapping mappings)
        {
            MissingMappingAction updateMappingAction = this.UpdateMappingAction;
            MissingSchemaAction updateSchemaAction = this.UpdateSchemaAction;
            foreach (IDataParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    this.ParameterOutput(parameter, row, mappings, updateMappingAction, updateSchemaAction);
                }
            }
        }

        private void ParameterOutput(IDataParameter parameter, DataRow row, DataTableMapping mappings, MissingMappingAction missingMapping, MissingSchemaAction missingSchema)
        {
            if ((ParameterDirection.Output & parameter.Direction) != ((ParameterDirection) 0))
            {
                object obj2 = parameter.Value;
                if (obj2 != null)
                {
                    string sourceColumn = parameter.SourceColumn;
                    if (!ADP.IsEmpty(sourceColumn))
                    {
                        DataColumn column = mappings.GetDataColumn(sourceColumn, null, row.Table, missingMapping, missingSchema);
                        if (column != null)
                        {
                            if (column.ReadOnly)
                            {
                                try
                                {
                                    column.ReadOnly = false;
                                    row[column] = obj2;
                                    return;
                                }
                                finally
                                {
                                    column.ReadOnly = true;
                                }
                            }
                            row[column] = obj2;
                        }
                    }
                }
            }
        }

        private static void QuietClose(IDbConnection connection, ConnectionState originalState)
        {
            if ((connection != null) && (originalState == ConnectionState.Closed))
            {
                connection.Close();
            }
        }

        private static void QuietOpen(IDbConnection connection, out ConnectionState originalState)
        {
            originalState = connection.State;
            if (originalState == ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        object ICloneable.Clone()
        {
            DbDataAdapter adapter = (DbDataAdapter) this.CloneInternals();
            adapter.CloneFrom(this);
            return adapter;
        }

        protected virtual void TerminateBatching()
        {
            throw ADP.NotSupported();
        }

        public override int Update(DataSet dataSet)
        {
            return this.Update(dataSet, "Table");
        }

        public int Update(DataRow[] dataRows)
        {
            int num3;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Update|API> %d#, dataRows[]\n", base.ObjectID);
            try
            {
                if (dataRows == null)
                {
                    throw ADP.ArgumentNull("dataRows");
                }
                int num2 = 0;
                if ((dataRows != null) || (dataRows.Length != 0))
                {
                    DataTable dataTable = null;
                    for (int i = 0; i < dataRows.Length; i++)
                    {
                        if ((dataRows[i] != null) && (dataTable != dataRows[i].Table))
                        {
                            if (dataTable != null)
                            {
                                throw ADP.UpdateMismatchRowTable(i);
                            }
                            dataTable = dataRows[i].Table;
                        }
                    }
                    if (dataTable != null)
                    {
                        DataTableMapping tableMapping = this.GetTableMapping(dataTable);
                        num2 = this.Update(dataRows, tableMapping);
                    }
                }
                num3 = num2;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num3;
        }

        public int Update(DataTable dataTable)
        {
            int num2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Update|API> %d#, dataTable", base.ObjectID);
            try
            {
                if (dataTable == null)
                {
                    throw ADP.UpdateRequiresDataTable("dataTable");
                }
                DataTableMapping tableMapping = null;
                int num = base.IndexOfDataSetTable(dataTable.TableName);
                if (-1 != num)
                {
                    tableMapping = base.TableMappings[num];
                }
                if (tableMapping == null)
                {
                    if (MissingMappingAction.Error == base.MissingMappingAction)
                    {
                        throw ADP.MissingTableMappingDestination(dataTable.TableName);
                    }
                    tableMapping = new DataTableMapping("Table", dataTable.TableName);
                }
                num2 = this.UpdateFromDataTable(dataTable, tableMapping);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num2;
        }

        protected virtual int Update(DataRow[] dataRows, DataTableMapping tableMapping)
        {
            int num10;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Update|API> %d#, dataRows[], tableMapping", base.ObjectID);
            try
            {
                int num3 = 0;
                IDbConnection[] connections = new IDbConnection[5];
                ConnectionState[] connectionStates = new ConnectionState[5];
                bool useSelectConnectionState = false;
                IDbCommand selectCommand = this._IDbDataAdapter.SelectCommand;
                if (selectCommand != null)
                {
                    connections[0] = selectCommand.Connection;
                    if (connections[0] != null)
                    {
                        connectionStates[0] = connections[0].State;
                        useSelectConnectionState = true;
                    }
                }
                int length = Math.Min(this.UpdateBatchSize, dataRows.Length);
                if (length < 1)
                {
                    length = dataRows.Length;
                }
                BatchCommandInfo[] batchCommands = new BatchCommandInfo[length];
                DataRow[] rowArray = new DataRow[length];
                int index = 0;
                try
                {
                    try
                    {
                        if (1 != length)
                        {
                            this.InitializeBatching();
                        }
                        StatementType select = StatementType.Select;
                        IDbCommand insertCommand = null;
                        foreach (DataRow row in dataRows)
                        {
                            if (row == null)
                            {
                                continue;
                            }
                            bool isRowUpdatingCommand = false;
                            switch (row.RowState)
                            {
                                case DataRowState.Detached:
                                case DataRowState.Unchanged:
                                {
                                    continue;
                                }
                                case DataRowState.Added:
                                    select = StatementType.Insert;
                                    insertCommand = this._IDbDataAdapter.InsertCommand;
                                    break;

                                case DataRowState.Deleted:
                                    select = StatementType.Delete;
                                    insertCommand = this._IDbDataAdapter.DeleteCommand;
                                    break;

                                case DataRowState.Modified:
                                    select = StatementType.Update;
                                    insertCommand = this._IDbDataAdapter.UpdateCommand;
                                    break;

                                default:
                                    throw ADP.InvalidDataRowState(row.RowState);
                            }
                            RowUpdatingEventArgs args3 = this.CreateRowUpdatingEvent(row, insertCommand, select, tableMapping);
                            try
                            {
                                row.RowError = null;
                                if (insertCommand != null)
                                {
                                    this.ParameterInput(insertCommand.Parameters, select, row, tableMapping);
                                }
                            }
                            catch (Exception exception5)
                            {
                                if (!ADP.IsCatchableExceptionType(exception5))
                                {
                                    throw;
                                }
                                ADP.TraceExceptionForCapture(exception5);
                                args3.Errors = exception5;
                                args3.Status = UpdateStatus.ErrorsOccurred;
                            }
                            this.OnRowUpdating(args3);
                            IDbCommand command2 = args3.Command;
                            isRowUpdatingCommand = insertCommand != command2;
                            insertCommand = command2;
                            command2 = null;
                            UpdateStatus status = args3.Status;
                            if (status != UpdateStatus.Continue)
                            {
                                if (UpdateStatus.ErrorsOccurred != status)
                                {
                                    if (UpdateStatus.SkipCurrentRow != status)
                                    {
                                        if (UpdateStatus.SkipAllRemainingRows != status)
                                        {
                                            throw ADP.InvalidUpdateStatus(status);
                                        }
                                        if (DataRowState.Unchanged == row.RowState)
                                        {
                                            num3++;
                                        }
                                        break;
                                    }
                                    if (DataRowState.Unchanged == row.RowState)
                                    {
                                        num3++;
                                    }
                                }
                                else
                                {
                                    this.UpdatingRowStatusErrors(args3, row);
                                }
                                continue;
                            }
                            args3 = null;
                            RowUpdatedEventArgs args = null;
                            if (1 == length)
                            {
                                if (insertCommand != null)
                                {
                                    batchCommands[0].CommandIdentifier = 0;
                                    batchCommands[0].ParameterCount = insertCommand.Parameters.Count;
                                    batchCommands[0].StatementType = select;
                                    batchCommands[0].UpdatedRowSource = insertCommand.UpdatedRowSource;
                                }
                                batchCommands[0].Row = row;
                                rowArray[0] = row;
                                index = 1;
                                goto Label_0383;
                            }
                            Exception exception = null;
                            try
                            {
                                if (insertCommand != null)
                                {
                                    if ((UpdateRowSource.FirstReturnedRecord & insertCommand.UpdatedRowSource) == UpdateRowSource.None)
                                    {
                                        batchCommands[index].CommandIdentifier = this.AddToBatch(insertCommand);
                                        batchCommands[index].ParameterCount = insertCommand.Parameters.Count;
                                        batchCommands[index].Row = row;
                                        batchCommands[index].StatementType = select;
                                        batchCommands[index].UpdatedRowSource = insertCommand.UpdatedRowSource;
                                        rowArray[index] = row;
                                        index++;
                                        if (index >= length)
                                        {
                                            goto Label_0314;
                                        }
                                        continue;
                                    }
                                    exception = ADP.ResultsNotAllowedDuringBatch();
                                }
                                else
                                {
                                    exception = ADP.UpdateRequiresCommand(select, isRowUpdatingCommand);
                                }
                            }
                            catch (Exception exception4)
                            {
                                if (!ADP.IsCatchableExceptionType(exception4))
                                {
                                    throw;
                                }
                                ADP.TraceExceptionForCapture(exception4);
                                exception = exception4;
                            }
                        Label_0314:
                            if (exception != null)
                            {
                                args = this.CreateRowUpdatedEvent(row, insertCommand, StatementType.Batch, tableMapping);
                                args.Errors = exception;
                                args.Status = UpdateStatus.ErrorsOccurred;
                                this.OnRowUpdated(args);
                                if (exception != args.Errors)
                                {
                                    for (int j = 0; j < batchCommands.Length; j++)
                                    {
                                        batchCommands[j].Errors = null;
                                    }
                                }
                                num3 += this.UpdatedRowStatus(args, batchCommands, index);
                                if (UpdateStatus.SkipAllRemainingRows != args.Status)
                                {
                                    continue;
                                }
                                break;
                            }
                        Label_0383:
                            args = this.CreateRowUpdatedEvent(row, insertCommand, select, tableMapping);
                            try
                            {
                                if (1 != length)
                                {
                                    IDbConnection connection3 = GetConnection1(this);
                                    ConnectionState state = this.UpdateConnectionOpen(connection3, StatementType.Batch, connections, connectionStates, useSelectConnectionState);
                                    args.AdapterInit(rowArray);
                                    if (ConnectionState.Open == state)
                                    {
                                        this.UpdateBatchExecute(batchCommands, index, args);
                                    }
                                    else
                                    {
                                        args.Errors = ADP.UpdateOpenConnectionRequired(StatementType.Batch, false, state);
                                        args.Status = UpdateStatus.ErrorsOccurred;
                                    }
                                }
                                else if (insertCommand != null)
                                {
                                    IDbConnection connection2 = GetConnection4(this, insertCommand, select, isRowUpdatingCommand);
                                    ConnectionState state3 = this.UpdateConnectionOpen(connection2, select, connections, connectionStates, useSelectConnectionState);
                                    if (ConnectionState.Open == state3)
                                    {
                                        this.UpdateRowExecute(args, insertCommand, select);
                                        batchCommands[0].RecordsAffected = new int?(args.RecordsAffected);
                                        batchCommands[0].Errors = null;
                                    }
                                    else
                                    {
                                        args.Errors = ADP.UpdateOpenConnectionRequired(select, isRowUpdatingCommand, state3);
                                        args.Status = UpdateStatus.ErrorsOccurred;
                                    }
                                }
                                else
                                {
                                    args.Errors = ADP.UpdateRequiresCommand(select, isRowUpdatingCommand);
                                    args.Status = UpdateStatus.ErrorsOccurred;
                                }
                            }
                            catch (Exception exception3)
                            {
                                if (!ADP.IsCatchableExceptionType(exception3))
                                {
                                    throw;
                                }
                                ADP.TraceExceptionForCapture(exception3);
                                args.Errors = exception3;
                                args.Status = UpdateStatus.ErrorsOccurred;
                            }
                            bool flag3 = UpdateStatus.ErrorsOccurred == args.Status;
                            Exception errors = args.Errors;
                            this.OnRowUpdated(args);
                            if (errors != args.Errors)
                            {
                                for (int k = 0; k < batchCommands.Length; k++)
                                {
                                    batchCommands[k].Errors = null;
                                }
                            }
                            num3 += this.UpdatedRowStatus(args, batchCommands, index);
                            if (UpdateStatus.SkipAllRemainingRows == args.Status)
                            {
                                if (flag3 && (1 != length))
                                {
                                    this.ClearBatch();
                                    index = 0;
                                }
                                break;
                            }
                            if (1 != length)
                            {
                                this.ClearBatch();
                                index = 0;
                            }
                            for (int i = 0; i < batchCommands.Length; i++)
                            {
                                batchCommands[i] = new BatchCommandInfo();
                            }
                            index = 0;
                        }
                        if ((1 != length) && (0 < index))
                        {
                            RowUpdatedEventArgs rowUpdatedEvent = this.CreateRowUpdatedEvent(null, insertCommand, select, tableMapping);
                            try
                            {
                                IDbConnection connection = GetConnection1(this);
                                ConnectionState state2 = this.UpdateConnectionOpen(connection, StatementType.Batch, connections, connectionStates, useSelectConnectionState);
                                DataRow[] destinationArray = rowArray;
                                if (index < rowArray.Length)
                                {
                                    destinationArray = new DataRow[index];
                                    Array.Copy(rowArray, destinationArray, index);
                                }
                                rowUpdatedEvent.AdapterInit(destinationArray);
                                if (ConnectionState.Open == state2)
                                {
                                    this.UpdateBatchExecute(batchCommands, index, rowUpdatedEvent);
                                }
                                else
                                {
                                    rowUpdatedEvent.Errors = ADP.UpdateOpenConnectionRequired(StatementType.Batch, false, state2);
                                    rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                                }
                            }
                            catch (Exception exception2)
                            {
                                if (!ADP.IsCatchableExceptionType(exception2))
                                {
                                    throw;
                                }
                                ADP.TraceExceptionForCapture(exception2);
                                rowUpdatedEvent.Errors = exception2;
                                rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                            }
                            Exception exception6 = rowUpdatedEvent.Errors;
                            this.OnRowUpdated(rowUpdatedEvent);
                            if (exception6 != rowUpdatedEvent.Errors)
                            {
                                for (int m = 0; m < batchCommands.Length; m++)
                                {
                                    batchCommands[m].Errors = null;
                                }
                            }
                            num3 += this.UpdatedRowStatus(rowUpdatedEvent, batchCommands, index);
                        }
                    }
                    finally
                    {
                        if (1 != length)
                        {
                            this.TerminateBatching();
                        }
                    }
                }
                finally
                {
                    for (int n = 0; n < connections.Length; n++)
                    {
                        QuietClose(connections[n], connectionStates[n]);
                    }
                }
                num10 = num3;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num10;
        }

        public int Update(DataSet dataSet, string srcTable)
        {
            int num2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<comm.DbDataAdapter.Update|API> %d#, dataSet, srcTable='%ls'", base.ObjectID, srcTable);
            try
            {
                if (dataSet == null)
                {
                    throw ADP.UpdateRequiresNonNullDataSet("dataSet");
                }
                if (ADP.IsEmpty(srcTable))
                {
                    throw ADP.UpdateRequiresSourceTableName("srcTable");
                }
                int num = 0;
                MissingMappingAction updateMappingAction = this.UpdateMappingAction;
                DataTableMapping tableMapping = base.GetTableMappingBySchemaAction(srcTable, srcTable, this.UpdateMappingAction);
                MissingSchemaAction updateSchemaAction = this.UpdateSchemaAction;
                DataTable dataTableBySchemaAction = tableMapping.GetDataTableBySchemaAction(dataSet, updateSchemaAction);
                if (dataTableBySchemaAction != null)
                {
                    num = this.UpdateFromDataTable(dataTableBySchemaAction, tableMapping);
                }
                else if (!base.HasTableMappings() || (-1 == base.TableMappings.IndexOf(tableMapping)))
                {
                    throw ADP.UpdateRequiresSourceTable(srcTable);
                }
                num2 = num;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num2;
        }

        private void UpdateBatchExecute(BatchCommandInfo[] batchCommands, int commandCount, RowUpdatedEventArgs rowUpdatedEvent)
        {
            try
            {
                int recordsAffected = this.ExecuteBatch();
                rowUpdatedEvent.AdapterInit(recordsAffected);
            }
            catch (DbException exception)
            {
                ADP.TraceExceptionForCapture(exception);
                rowUpdatedEvent.Errors = exception;
                rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
            }
            MissingMappingAction updateMappingAction = this.UpdateMappingAction;
            MissingSchemaAction updateSchemaAction = this.UpdateSchemaAction;
            int num4 = 0;
            bool flag = false;
            List<DataRow> list = null;
            for (int i = 0; i < commandCount; i++)
            {
                int num3;
                BatchCommandInfo info = batchCommands[i];
                StatementType statementType = info.StatementType;
                if (this.GetBatchedRecordsAffected(info.CommandIdentifier, out num3, out batchCommands[i].Errors))
                {
                    batchCommands[i].RecordsAffected = new int?(num3);
                }
                if ((batchCommands[i].Errors == null) && batchCommands[i].RecordsAffected.HasValue)
                {
                    if ((StatementType.Update == statementType) || (StatementType.Delete == statementType))
                    {
                        num4++;
                        if (num3 == 0)
                        {
                            if (list == null)
                            {
                                list = new List<DataRow>();
                            }
                            batchCommands[i].Errors = ADP.UpdateConcurrencyViolation(batchCommands[i].StatementType, 0, 1, new DataRow[] { rowUpdatedEvent.Rows[i] });
                            flag = true;
                            list.Add(rowUpdatedEvent.Rows[i]);
                        }
                    }
                    if (((StatementType.Insert == statementType) || (StatementType.Update == statementType)) && (((UpdateRowSource.OutputParameters & info.UpdatedRowSource) != UpdateRowSource.None) && (num3 != 0)))
                    {
                        if (StatementType.Insert == statementType)
                        {
                            rowUpdatedEvent.Rows[i].AcceptChanges();
                        }
                        for (int j = 0; j < info.ParameterCount; j++)
                        {
                            IDataParameter batchedParameter = this.GetBatchedParameter(info.CommandIdentifier, j);
                            this.ParameterOutput(batchedParameter, info.Row, rowUpdatedEvent.TableMapping, updateMappingAction, updateSchemaAction);
                        }
                    }
                }
            }
            if ((((rowUpdatedEvent.Errors == null) && (rowUpdatedEvent.Status == UpdateStatus.Continue)) && (0 < num4)) && ((rowUpdatedEvent.RecordsAffected == 0) || flag))
            {
                DataRow[] dataRows = (list != null) ? list.ToArray() : rowUpdatedEvent.Rows;
                rowUpdatedEvent.Errors = ADP.UpdateConcurrencyViolation(StatementType.Batch, commandCount - dataRows.Length, commandCount, dataRows);
                rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
            }
        }

        private ConnectionState UpdateConnectionOpen(IDbConnection connection, StatementType statementType, IDbConnection[] connections, ConnectionState[] connectionStates, bool useSelectConnectionState)
        {
            int index = (int) statementType;
            if (connection != connections[index])
            {
                QuietClose(connections[index], connectionStates[index]);
                connections[index] = connection;
                connectionStates[index] = ConnectionState.Closed;
                QuietOpen(connection, out connectionStates[index]);
                if (useSelectConnectionState && (connections[0] == connection))
                {
                    connectionStates[index] = connections[0].State;
                }
            }
            return connection.State;
        }

        private int UpdatedRowStatus(RowUpdatedEventArgs rowUpdatedEvent, BatchCommandInfo[] batchCommands, int commandCount)
        {
            switch (rowUpdatedEvent.Status)
            {
                case UpdateStatus.Continue:
                    return this.UpdatedRowStatusContinue(rowUpdatedEvent, batchCommands, commandCount);

                case UpdateStatus.ErrorsOccurred:
                    return this.UpdatedRowStatusErrors(rowUpdatedEvent, batchCommands, commandCount);

                case UpdateStatus.SkipCurrentRow:
                case UpdateStatus.SkipAllRemainingRows:
                    return this.UpdatedRowStatusSkip(batchCommands, commandCount);
            }
            throw ADP.InvalidUpdateStatus(rowUpdatedEvent.Status);
        }

        private int UpdatedRowStatusContinue(RowUpdatedEventArgs rowUpdatedEvent, BatchCommandInfo[] batchCommands, int commandCount)
        {
            int num2 = 0;
            bool acceptChangesDuringUpdate = base.AcceptChangesDuringUpdate;
            for (int i = 0; i < commandCount; i++)
            {
                DataRow row = batchCommands[i].Row;
                if (((batchCommands[i].Errors == null) && batchCommands[i].RecordsAffected.HasValue) && (batchCommands[i].RecordsAffected.Value != 0))
                {
                    if (acceptChangesDuringUpdate && (((DataRowState.Modified | DataRowState.Deleted | DataRowState.Added) & row.RowState) != 0))
                    {
                        row.AcceptChanges();
                    }
                    num2++;
                }
            }
            return num2;
        }

        private int UpdatedRowStatusErrors(RowUpdatedEventArgs rowUpdatedEvent, BatchCommandInfo[] batchCommands, int commandCount)
        {
            Exception errors = rowUpdatedEvent.Errors;
            if (errors == null)
            {
                errors = ADP.RowUpdatedErrors();
                rowUpdatedEvent.Errors = errors;
            }
            int num3 = 0;
            bool flag = false;
            string message = errors.Message;
            for (int i = 0; i < commandCount; i++)
            {
                DataRow row = batchCommands[i].Row;
                if (batchCommands[i].Errors != null)
                {
                    string str = batchCommands[i].Errors.Message;
                    if (string.IsNullOrEmpty(str))
                    {
                        str = message;
                    }
                    row.RowError = row.RowError + str;
                    flag = true;
                }
            }
            if (!flag)
            {
                for (int j = 0; j < commandCount; j++)
                {
                    batchCommands[j].Row.RowError = batchCommands[j].Row.RowError + message;
                }
            }
            else
            {
                num3 = this.UpdatedRowStatusContinue(rowUpdatedEvent, batchCommands, commandCount);
            }
            if (!base.ContinueUpdateOnError)
            {
                throw errors;
            }
            return num3;
        }

        private int UpdatedRowStatusSkip(BatchCommandInfo[] batchCommands, int commandCount)
        {
            int num2 = 0;
            for (int i = 0; i < commandCount; i++)
            {
                DataRow row = batchCommands[i].Row;
                if (((DataRowState.Unchanged | DataRowState.Detached) & row.RowState) != 0)
                {
                    num2++;
                }
            }
            return num2;
        }

        private int UpdateFromDataTable(DataTable dataTable, DataTableMapping tableMapping)
        {
            int num = 0;
            DataRow[] dataRows = ADP.SelectAdapterRows(dataTable, false);
            if ((dataRows != null) && (0 < dataRows.Length))
            {
                num = this.Update(dataRows, tableMapping);
            }
            return num;
        }

        private void UpdateRowExecute(RowUpdatedEventArgs rowUpdatedEvent, IDbCommand dataCommand, StatementType cmdIndex)
        {
            bool flag = true;
            UpdateRowSource updatedRowSource = dataCommand.UpdatedRowSource;
            if ((StatementType.Delete == cmdIndex) || ((UpdateRowSource.FirstReturnedRecord & updatedRowSource) == UpdateRowSource.None))
            {
                int recordsAffected = dataCommand.ExecuteNonQuery();
                rowUpdatedEvent.AdapterInit(recordsAffected);
            }
            else if ((StatementType.Insert == cmdIndex) || (StatementType.Update == cmdIndex))
            {
                using (IDataReader reader = dataCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    DataReaderContainer dataReader = DataReaderContainer.Create(reader, this.ReturnProviderSpecificTypes);
                    try
                    {
                        bool flag2 = false;
                        do
                        {
                            if (0 < dataReader.FieldCount)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        while (reader.NextResult());
                        if (flag2 && (reader.RecordsAffected != 0))
                        {
                            SchemaMapping mapping = new SchemaMapping(this, null, rowUpdatedEvent.Row.Table, dataReader, false, SchemaType.Mapped, rowUpdatedEvent.TableMapping.SourceTable, true, null, null);
                            if (((mapping.DataTable != null) && (mapping.DataValues != null)) && reader.Read())
                            {
                                if ((StatementType.Insert == cmdIndex) && flag)
                                {
                                    rowUpdatedEvent.Row.AcceptChanges();
                                    flag = false;
                                }
                                mapping.ApplyToDataRow(rowUpdatedEvent.Row);
                            }
                        }
                    }
                    finally
                    {
                        reader.Close();
                        int num = reader.RecordsAffected;
                        rowUpdatedEvent.AdapterInit(num);
                    }
                }
            }
            if (((StatementType.Insert == cmdIndex) || (StatementType.Update == cmdIndex)) && (((UpdateRowSource.OutputParameters & updatedRowSource) != UpdateRowSource.None) && (rowUpdatedEvent.RecordsAffected != 0)))
            {
                if ((StatementType.Insert == cmdIndex) && flag)
                {
                    rowUpdatedEvent.Row.AcceptChanges();
                }
                this.ParameterOutput(dataCommand.Parameters, rowUpdatedEvent.Row, rowUpdatedEvent.TableMapping);
            }
            if (rowUpdatedEvent.Status == UpdateStatus.Continue)
            {
                switch (cmdIndex)
                {
                    case StatementType.Update:
                    case StatementType.Delete:
                        if (rowUpdatedEvent.RecordsAffected == 0)
                        {
                            rowUpdatedEvent.Errors = ADP.UpdateConcurrencyViolation(cmdIndex, rowUpdatedEvent.RecordsAffected, 1, new DataRow[] { rowUpdatedEvent.Row });
                            rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                        }
                        return;
                }
            }
        }

        private void UpdatingRowStatusErrors(RowUpdatingEventArgs rowUpdatedEvent, DataRow dataRow)
        {
            Exception errors = rowUpdatedEvent.Errors;
            if (errors == null)
            {
                errors = ADP.RowUpdatingErrors();
                rowUpdatedEvent.Errors = errors;
            }
            string message = errors.Message;
            dataRow.RowError = dataRow.RowError + message;
            if (!base.ContinueUpdateOnError)
            {
                throw errors;
            }
        }

        private IDbDataAdapter _IDbDataAdapter
        {
            get
            {
                return this;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DbCommand DeleteCommand
        {
            get
            {
                return (DbCommand) this._IDbDataAdapter.DeleteCommand;
            }
            set
            {
                this._IDbDataAdapter.DeleteCommand = value;
            }
        }

        protected internal CommandBehavior FillCommandBehavior
        {
            get
            {
                return (this._fillCommandBehavior | CommandBehavior.SequentialAccess);
            }
            set
            {
                this._fillCommandBehavior = value | CommandBehavior.SequentialAccess;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DbCommand InsertCommand
        {
            get
            {
                return (DbCommand) this._IDbDataAdapter.InsertCommand;
            }
            set
            {
                this._IDbDataAdapter.InsertCommand = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DbCommand SelectCommand
        {
            get
            {
                return (DbCommand) this._IDbDataAdapter.SelectCommand;
            }
            set
            {
                this._IDbDataAdapter.SelectCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get
            {
                return this._deleteCommand;
            }
            set
            {
                this._deleteCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.InsertCommand
        {
            get
            {
                return this._insertCommand;
            }
            set
            {
                this._insertCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.SelectCommand
        {
            get
            {
                return this._selectCommand;
            }
            set
            {
                this._selectCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get
            {
                return this._updateCommand;
            }
            set
            {
                this._updateCommand = value;
            }
        }

        [ResDescription("DbDataAdapter_UpdateBatchSize"), DefaultValue(1), ResCategory("DataCategory_Update")]
        public virtual int UpdateBatchSize
        {
            get
            {
                return 1;
            }
            set
            {
                if (1 != value)
                {
                    throw ADP.NotSupported();
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DbCommand UpdateCommand
        {
            get
            {
                return (DbCommand) this._IDbDataAdapter.UpdateCommand;
            }
            set
            {
                this._IDbDataAdapter.UpdateCommand = value;
            }
        }

        private MissingMappingAction UpdateMappingAction
        {
            get
            {
                if (MissingMappingAction.Passthrough == base.MissingMappingAction)
                {
                    return MissingMappingAction.Passthrough;
                }
                return MissingMappingAction.Error;
            }
        }

        private MissingSchemaAction UpdateSchemaAction
        {
            get
            {
                MissingSchemaAction missingSchemaAction = base.MissingSchemaAction;
                if ((MissingSchemaAction.Add != missingSchemaAction) && (MissingSchemaAction.AddWithKey != missingSchemaAction))
                {
                    return MissingSchemaAction.Error;
                }
                return MissingSchemaAction.Ignore;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BatchCommandInfo
        {
            internal int CommandIdentifier;
            internal int ParameterCount;
            internal DataRow Row;
            internal System.Data.StatementType StatementType;
            internal UpdateRowSource UpdatedRowSource;
            internal int? RecordsAffected;
            internal Exception Errors;
        }
    }
}


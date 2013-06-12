namespace System.Data.OleDb
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ToolboxItem("Microsoft.VSDesigner.Data.VS.OleDbDataAdapterToolboxItem, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("RowUpdated"), Designer("Microsoft.VSDesigner.Data.VS.OleDbDataAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class OleDbDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
    {
        private OleDbCommand _deleteCommand;
        private OleDbCommand _insertCommand;
        private OleDbCommand _selectCommand;
        private OleDbCommand _updateCommand;
        private static readonly object EventRowUpdated = new object();
        private static readonly object EventRowUpdating = new object();

        [ResDescription("DbDataAdapter_RowUpdated"), ResCategory("DataCategory_Update")]
        public event OleDbRowUpdatedEventHandler RowUpdated
        {
            add
            {
                base.Events.AddHandler(EventRowUpdated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowUpdated, value);
            }
        }

        [ResDescription("DbDataAdapter_RowUpdating"), ResCategory("DataCategory_Update")]
        public event OleDbRowUpdatingEventHandler RowUpdating
        {
            add
            {
                OleDbRowUpdatingEventHandler mcd = (OleDbRowUpdatingEventHandler) base.Events[EventRowUpdating];
                if ((mcd != null) && (value.Target is DbCommandBuilder))
                {
                    OleDbRowUpdatingEventHandler handler = (OleDbRowUpdatingEventHandler) ADP.FindBuilder(mcd);
                    if (handler != null)
                    {
                        base.Events.RemoveHandler(EventRowUpdating, handler);
                    }
                }
                base.Events.AddHandler(EventRowUpdating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRowUpdating, value);
            }
        }

        public OleDbDataAdapter()
        {
            GC.SuppressFinalize(this);
        }

        public OleDbDataAdapter(OleDbCommand selectCommand) : this()
        {
            this.SelectCommand = selectCommand;
        }

        private OleDbDataAdapter(OleDbDataAdapter from) : base(from)
        {
            GC.SuppressFinalize(this);
        }

        public OleDbDataAdapter(string selectCommandText, OleDbConnection selectConnection) : this()
        {
            this.SelectCommand = new OleDbCommand(selectCommandText, selectConnection);
        }

        public OleDbDataAdapter(string selectCommandText, string selectConnectionString) : this()
        {
            OleDbConnection connection = new OleDbConnection(selectConnectionString);
            this.SelectCommand = new OleDbCommand(selectCommandText, connection);
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new OleDbRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new OleDbRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        public int Fill(DataTable dataTable, object ADODBRecordSet)
        {
            int num;
            IntPtr ptr;
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(OleDbConnection.ExecutePermission);
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbDataAdapter.Fill|API> %d#, dataTable, ADODBRecordSet\n", base.ObjectID);
            try
            {
                if (dataTable == null)
                {
                    throw ADP.ArgumentNull("dataTable");
                }
                if (ADODBRecordSet == null)
                {
                    throw ADP.ArgumentNull("adodb");
                }
                num = this.FillFromADODB(dataTable, ADODBRecordSet, null, false);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public int Fill(DataSet dataSet, object ADODBRecordSet, string srcTable)
        {
            int num;
            IntPtr ptr;
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(OleDbConnection.ExecutePermission);
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbDataAdapter.Fill|API> %d#, dataSet, ADODBRecordSet, srcTable='%ls'\n", base.ObjectID, srcTable);
            try
            {
                if (dataSet == null)
                {
                    throw ADP.ArgumentNull("dataSet");
                }
                if (ADODBRecordSet == null)
                {
                    throw ADP.ArgumentNull("adodb");
                }
                if (ADP.IsEmpty(srcTable))
                {
                    throw ADP.FillRequiresSourceTableName("srcTable");
                }
                num = this.FillFromADODB(dataSet, ADODBRecordSet, srcTable, true);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        private void FillClose(bool isrecordset, object value)
        {
            OleDbHResult result;
            if (isrecordset)
            {
                Bid.Trace("<oledb.Recordset15.Close|API|ADODB>\n");
                result = ((System.Data.Common.UnsafeNativeMethods.Recordset15) value).Close();
                Bid.Trace("<oledb.Recordset15.Close|API|ADODB|RET> %08X{HRESULT}\n", result);
            }
            else
            {
                Bid.Trace("<oledb._ADORecord.Close|API|ADODB>\n");
                result = ((System.Data.Common.UnsafeNativeMethods._ADORecord) value).Close();
                Bid.Trace("<oledb._ADORecord.Close|API|ADODB|RET> %08X{HRESULT}\n", result);
            }
            if ((OleDbHResult.S_OK < result) && (((OleDbHResult) (-2146824584)) != result))
            {
                System.Data.Common.UnsafeNativeMethods.IErrorInfo ppIErrorInfo = null;
                System.Data.Common.UnsafeNativeMethods.GetErrorInfo(0, out ppIErrorInfo);
                string message = string.Empty;
                if (ppIErrorInfo != null)
                {
                    ODB.GetErrorDescription(ppIErrorInfo, result, out message);
                }
                throw new COMException(message, (int) result);
            }
        }

        internal static void FillDataTable(OleDbDataReader dataReader, params DataTable[] dataTables)
        {
            new OleDbDataAdapter().Fill(dataTables, dataReader, 0, 0);
        }

        private int FillFromADODB(object data, object adodb, string srcTable, bool multipleResults)
        {
            string sourceTableName;
            bool flag2;
            bool flag = multipleResults;
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|ADODB> ADORecordsetConstruction\n");
            System.Data.Common.UnsafeNativeMethods.ADORecordsetConstruction recordset = adodb as System.Data.Common.UnsafeNativeMethods.ADORecordsetConstruction;
            System.Data.Common.UnsafeNativeMethods.ADORecordConstruction record = null;
            if (recordset != null)
            {
                if (multipleResults)
                {
                    Bid.Trace("<oledb.Recordset15.get_ActiveConnection|API|ADODB>\n");
                    if (((System.Data.Common.UnsafeNativeMethods.Recordset15) adodb).get_ActiveConnection() == null)
                    {
                        multipleResults = false;
                    }
                }
            }
            else
            {
                Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|ADODB> ADORecordConstruction\n");
                record = adodb as System.Data.Common.UnsafeNativeMethods.ADORecordConstruction;
                if (record != null)
                {
                    multipleResults = false;
                }
            }
            int num = 0;
            if (recordset == null)
            {
                if (record == null)
                {
                    throw ODB.Fill_NotADODB("adodb");
                }
                num = this.FillFromRecord(data, record, srcTable);
                if (flag)
                {
                    this.FillClose(false, record);
                }
                return num;
            }
            int index = 0;
            object[] objArray = new object[1];
        Label_0068:
            sourceTableName = null;
            if (data is DataSet)
            {
                sourceTableName = GetSourceTableName(srcTable, index);
            }
            num += this.FillFromRecordset(data, recordset, sourceTableName, out flag2);
            if (multipleResults)
            {
                object obj2;
                object obj4;
                objArray[0] = DBNull.Value;
                Bid.Trace("<oledb.Recordset15.NextRecordset|API|ADODB>\n");
                OleDbHResult result = ((System.Data.Common.UnsafeNativeMethods.Recordset15) adodb).NextRecordset(out obj4, out obj2);
                Bid.Trace("<oledb.Recordset15.NextRecordset|API|ADODB|RET> %08X{HRESULT}\n", result);
                if (OleDbHResult.S_OK > result)
                {
                    if (((OleDbHResult) (-2146825037)) != result)
                    {
                        System.Data.Common.UnsafeNativeMethods.IErrorInfo ppIErrorInfo = null;
                        System.Data.Common.UnsafeNativeMethods.GetErrorInfo(0, out ppIErrorInfo);
                        string message = string.Empty;
                        if (ppIErrorInfo != null)
                        {
                            ODB.GetErrorDescription(ppIErrorInfo, result, out message);
                        }
                        throw new COMException(message, (int) result);
                    }
                }
                else
                {
                    adodb = obj2;
                    if (adodb != null)
                    {
                        Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|ADODB> ADORecordsetConstruction\n");
                        recordset = (System.Data.Common.UnsafeNativeMethods.ADORecordsetConstruction) adodb;
                        if (flag2)
                        {
                            index++;
                        }
                        if (recordset != null)
                        {
                            goto Label_0068;
                        }
                    }
                }
            }
            if ((recordset != null) && (flag || (adodb == null)))
            {
                this.FillClose(true, recordset);
            }
            return num;
        }

        private int FillFromRecord(object data, System.Data.Common.UnsafeNativeMethods.ADORecordConstruction record, string srcTable)
        {
            object result = null;
            try
            {
                Bid.Trace("<oledb.ADORecordConstruction.get_Row|API|ADODB>\n");
                result = record.get_Row();
                Bid.Trace("<oledb.ADORecordConstruction.get_Row|API|ADODB|RET> %08X{HRESULT}\n", 0);
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ODB.Fill_EmptyRecord("adodb", exception);
            }
            if (result != null)
            {
                CommandBehavior commandBehavior = (MissingSchemaAction.AddWithKey != base.MissingSchemaAction) ? CommandBehavior.Default : CommandBehavior.KeyInfo;
                commandBehavior |= CommandBehavior.SequentialAccess | CommandBehavior.SingleRow;
                OleDbDataReader dataReader = null;
                try
                {
                    dataReader = new OleDbDataReader(null, null, 0, commandBehavior);
                    dataReader.InitializeIRow(result, ADP.RecordsUnaffected);
                    dataReader.BuildMetaInfo();
                    if (data is DataTable)
                    {
                        return base.Fill((DataTable) data, dataReader);
                    }
                    return base.Fill((DataSet) data, srcTable, dataReader, 0, 0);
                }
                finally
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                    }
                }
            }
            return 0;
        }

        private int FillFromRecordset(object data, System.Data.Common.UnsafeNativeMethods.ADORecordsetConstruction recordset, string srcTable, out bool incrementResultCount)
        {
            IntPtr ptr;
            incrementResultCount = false;
            object result = null;
            try
            {
                Bid.Trace("<oledb.ADORecordsetConstruction.get_Rowset|API|ADODB>\n");
                result = recordset.get_Rowset();
                Bid.Trace("<oledb.ADORecordsetConstruction.get_Rowset|API|ADODB|RET> %08X{HRESULT}\n", 0);
                Bid.Trace("<oledb.ADORecordsetConstruction.get_Chapter|API|ADODB>\n");
                ptr = recordset.get_Chapter();
                Bid.Trace("<oledb.ADORecordsetConstruction.get_Chapter|API|ADODB|RET> %08X{HRESULT}\n", 0);
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw ODB.Fill_EmptyRecordSet("ADODBRecordSet", exception);
            }
            if (result != null)
            {
                CommandBehavior commandBehavior = (MissingSchemaAction.AddWithKey != base.MissingSchemaAction) ? CommandBehavior.Default : CommandBehavior.KeyInfo;
                commandBehavior |= CommandBehavior.SequentialAccess;
                OleDbDataReader dataReader = null;
                try
                {
                    ChapterHandle chapterHandle = ChapterHandle.CreateChapterHandle(ptr);
                    dataReader = new OleDbDataReader(null, null, 0, commandBehavior);
                    dataReader.InitializeIRowset(result, chapterHandle, ADP.RecordsUnaffected);
                    dataReader.BuildMetaInfo();
                    incrementResultCount = 0 < dataReader.FieldCount;
                    if (incrementResultCount)
                    {
                        if (data is DataTable)
                        {
                            return base.Fill((DataTable) data, dataReader);
                        }
                        return base.Fill((DataSet) data, srcTable, dataReader, 0, 0);
                    }
                }
                finally
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                    }
                }
            }
            return 0;
        }

        private static string GetSourceTableName(string srcTable, int index)
        {
            if (index == 0)
            {
                return srcTable;
            }
            return (srcTable + index.ToString(CultureInfo.InvariantCulture));
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            OleDbRowUpdatedEventHandler handler = (OleDbRowUpdatedEventHandler) base.Events[EventRowUpdated];
            if ((handler != null) && (value is OleDbRowUpdatedEventArgs))
            {
                handler(this, (OleDbRowUpdatedEventArgs) value);
            }
            base.OnRowUpdated(value);
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            OleDbRowUpdatingEventHandler handler = (OleDbRowUpdatingEventHandler) base.Events[EventRowUpdating];
            if ((handler != null) && (value is OleDbRowUpdatingEventArgs))
            {
                handler(this, (OleDbRowUpdatingEventArgs) value);
            }
            base.OnRowUpdating(value);
        }

        object ICloneable.Clone()
        {
            return new OleDbDataAdapter(this);
        }

        [ResDescription("DbDataAdapter_DeleteCommand"), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResCategory("DataCategory_Update"), DefaultValue((string) null)]
        public OleDbCommand DeleteCommand
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

        [ResDescription("DbDataAdapter_InsertCommand"), DefaultValue((string) null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResCategory("DataCategory_Update")]
        public OleDbCommand InsertCommand
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

        [Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResDescription("DbDataAdapter_SelectCommand"), ResCategory("DataCategory_Fill"), DefaultValue((string) null)]
        public OleDbCommand SelectCommand
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

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get
            {
                return this._deleteCommand;
            }
            set
            {
                this._deleteCommand = (OleDbCommand) value;
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
                this._insertCommand = (OleDbCommand) value;
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
                this._selectCommand = (OleDbCommand) value;
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
                this._updateCommand = (OleDbCommand) value;
            }
        }

        [ResCategory("DataCategory_Update"), ResDescription("DbDataAdapter_UpdateCommand"), DefaultValue((string) null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public OleDbCommand UpdateCommand
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
    }
}


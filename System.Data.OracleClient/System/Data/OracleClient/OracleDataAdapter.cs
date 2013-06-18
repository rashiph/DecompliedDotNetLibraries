namespace System.Data.OracleClient
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.InteropServices;

    [Obsolete("OracleDataAdapter has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260", false), DefaultEvent("RowUpdated"), ToolboxItem("Microsoft.VSDesigner.Data.VS.OracleDataAdapterToolboxItem, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Designer("Microsoft.VSDesigner.Data.VS.OracleDataAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class OracleDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
    {
        private OracleCommandSet _commandSet;
        private OracleCommand _deleteCommand;
        private OracleCommand _insertCommand;
        private OracleCommand _selectCommand;
        private int _updateBatchSize;
        private OracleCommand _updateCommand;
        internal static readonly object EventRowUpdated = new object();
        internal static readonly object EventRowUpdating = new object();

        [System.Data.OracleClient.ResDescription("DbDataAdapter_RowUpdated"), System.Data.OracleClient.ResCategory("OracleCategory_Update")]
        public event OracleRowUpdatedEventHandler RowUpdated
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

        [System.Data.OracleClient.ResDescription("DbDataAdapter_RowUpdating"), System.Data.OracleClient.ResCategory("OracleCategory_Update")]
        public event OracleRowUpdatingEventHandler RowUpdating
        {
            add
            {
                OracleRowUpdatingEventHandler mcd = (OracleRowUpdatingEventHandler) base.Events[EventRowUpdating];
                if ((mcd != null) && (value.Target is OracleCommandBuilder))
                {
                    OracleRowUpdatingEventHandler handler = (OracleRowUpdatingEventHandler) System.Data.Common.ADP.FindBuilder(mcd);
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

        public OracleDataAdapter()
        {
            this._updateBatchSize = 1;
            GC.SuppressFinalize(this);
        }

        public OracleDataAdapter(OracleCommand selectCommand) : this()
        {
            this.SelectCommand = selectCommand;
        }

        private OracleDataAdapter(OracleDataAdapter from) : base(from)
        {
            this._updateBatchSize = 1;
            GC.SuppressFinalize(this);
        }

        public OracleDataAdapter(string selectCommandText, OracleConnection selectConnection) : this()
        {
            this.SelectCommand = new OracleCommand();
            this.SelectCommand.Connection = selectConnection;
            this.SelectCommand.CommandText = selectCommandText;
        }

        public OracleDataAdapter(string selectCommandText, string selectConnectionString) : this()
        {
            OracleConnection connection = new OracleConnection(selectConnectionString);
            this.SelectCommand = new OracleCommand();
            this.SelectCommand.Connection = connection;
            this.SelectCommand.CommandText = selectCommandText;
        }

        protected override int AddToBatch(IDbCommand command)
        {
            int commandCount = this._commandSet.CommandCount;
            this._commandSet.Append((OracleCommand) command);
            return commandCount;
        }

        protected override void ClearBatch()
        {
            this._commandSet.Clear();
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new OracleRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new OracleRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override int ExecuteBatch()
        {
            return this._commandSet.ExecuteNonQuery();
        }

        protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
        {
            return this._commandSet.GetParameter(commandIdentifier, parameterIndex);
        }

        protected override bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out Exception error)
        {
            error = null;
            return this._commandSet.GetBatchedRecordsAffected(commandIdentifier, out recordsAffected);
        }

        protected override void InitializeBatching()
        {
            this._commandSet = new OracleCommandSet();
            OracleCommand selectCommand = this.SelectCommand;
            if (selectCommand == null)
            {
                selectCommand = this.InsertCommand;
                if (selectCommand == null)
                {
                    selectCommand = this.UpdateCommand;
                    if (selectCommand == null)
                    {
                        selectCommand = this.DeleteCommand;
                    }
                }
            }
            if (selectCommand != null)
            {
                this._commandSet.Connection = selectCommand.Connection;
                this._commandSet.Transaction = selectCommand.Transaction;
                this._commandSet.CommandTimeout = selectCommand.CommandTimeout;
            }
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            OracleRowUpdatedEventHandler handler = (OracleRowUpdatedEventHandler) base.Events[EventRowUpdated];
            if ((handler != null) && (value is OracleRowUpdatedEventArgs))
            {
                handler(this, (OracleRowUpdatedEventArgs) value);
            }
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            OracleRowUpdatingEventHandler handler = (OracleRowUpdatingEventHandler) base.Events[EventRowUpdating];
            if ((handler != null) && (value is OracleRowUpdatingEventArgs))
            {
                handler(this, (OracleRowUpdatingEventArgs) value);
            }
        }

        object ICloneable.Clone()
        {
            return new OracleDataAdapter(this);
        }

        protected override void TerminateBatching()
        {
            if (this._commandSet != null)
            {
                this._commandSet.Dispose();
                this._commandSet = null;
            }
        }

        [System.Data.OracleClient.ResDescription("DbDataAdapter_DeleteCommand"), System.Data.OracleClient.ResCategory("OracleCategory_Update"), DefaultValue((string) null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public OracleCommand DeleteCommand
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

        [System.Data.OracleClient.ResDescription("DbDataAdapter_InsertCommand"), DefaultValue((string) null), System.Data.OracleClient.ResCategory("OracleCategory_Update"), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public OracleCommand InsertCommand
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

        [System.Data.OracleClient.ResCategory("OracleCategory_Fill"), DefaultValue((string) null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), System.Data.OracleClient.ResDescription("DbDataAdapter_SelectCommand")]
        public OracleCommand SelectCommand
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
                this._deleteCommand = (OracleCommand) value;
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
                this._insertCommand = (OracleCommand) value;
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
                this._selectCommand = (OracleCommand) value;
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
                this._updateCommand = (OracleCommand) value;
            }
        }

        public override int UpdateBatchSize
        {
            get
            {
                return this._updateBatchSize;
            }
            set
            {
                if (0 > value)
                {
                    throw System.Data.Common.ADP.MustBePositive("UpdateBatchSize");
                }
                this._updateBatchSize = value;
            }
        }

        [System.Data.OracleClient.ResCategory("OracleCategory_Update"), DefaultValue((string) null), Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), System.Data.OracleClient.ResDescription("DbDataAdapter_UpdateCommand")]
        public OracleCommand UpdateCommand
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


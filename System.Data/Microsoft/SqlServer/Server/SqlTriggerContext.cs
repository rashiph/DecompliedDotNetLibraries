namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;

    public sealed class SqlTriggerContext
    {
        private bool[] _columnsUpdated;
        private SqlXml _eventInstanceData;
        private Microsoft.SqlServer.Server.TriggerAction _triggerAction;

        internal SqlTriggerContext(Microsoft.SqlServer.Server.TriggerAction triggerAction, bool[] columnsUpdated, SqlXml eventInstanceData)
        {
            this._triggerAction = triggerAction;
            this._columnsUpdated = columnsUpdated;
            this._eventInstanceData = eventInstanceData;
        }

        public bool IsUpdatedColumn(int columnOrdinal)
        {
            if (this._columnsUpdated == null)
            {
                throw ADP.IndexOutOfRange(columnOrdinal);
            }
            return this._columnsUpdated[columnOrdinal];
        }

        public int ColumnCount
        {
            get
            {
                int length = 0;
                if (this._columnsUpdated != null)
                {
                    length = this._columnsUpdated.Length;
                }
                return length;
            }
        }

        public SqlXml EventData
        {
            get
            {
                return this._eventInstanceData;
            }
        }

        public Microsoft.SqlServer.Server.TriggerAction TriggerAction
        {
            get
            {
                return this._triggerAction;
            }
        }
    }
}


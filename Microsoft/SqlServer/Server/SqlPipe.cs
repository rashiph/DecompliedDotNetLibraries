namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    public sealed class SqlPipe
    {
        private SmiEventSink_Default _eventSink;
        private bool _hadErrorInResultSet;
        private bool _isBusy;
        private SqlMetaData[] _metaDataSent;
        private SmiRecordBuffer _recordBufferSent;
        private SmiContext _smiContext;

        internal SqlPipe(SmiContext smiContext)
        {
            this._smiContext = smiContext;
            this._eventSink = new SmiEventSink_Default();
        }

        private void ClearPipeBusy()
        {
            this._isBusy = false;
        }

        private void EnsureNormalSendValid(string methodName)
        {
            if (this.IsSendingResults)
            {
                throw SQL.SqlPipeAlreadyHasAnOpenResultSet(methodName);
            }
        }

        private void EnsureResultStarted(string methodName)
        {
            if (!this.IsSendingResults)
            {
                throw SQL.SqlPipeDoesNotHaveAnOpenResultSet(methodName);
            }
        }

        public void ExecuteAndSend(SqlCommand command)
        {
            this.SetPipeBusy();
            try
            {
                this.EnsureNormalSendValid("ExecuteAndSend");
                if (command == null)
                {
                    throw ADP.ArgumentNull("command");
                }
                SqlConnection connection2 = command.Connection;
                if (connection2 == null)
                {
                    using (SqlConnection connection = new SqlConnection("Context Connection=true"))
                    {
                        connection.Open();
                        try
                        {
                            command.Connection = connection;
                            command.ExecuteToPipe(this._smiContext);
                        }
                        finally
                        {
                            command.Connection = null;
                        }
                        return;
                    }
                }
                if (ConnectionState.Open != connection2.State)
                {
                    throw ADP.ClosedConnectionError();
                }
                if (!(connection2.InnerConnection is SqlInternalConnectionSmi))
                {
                    throw SQL.SqlPipeCommandHookedUpToNonContextConnection();
                }
                command.ExecuteToPipe(this._smiContext);
            }
            finally
            {
                this.ClearPipeBusy();
            }
        }

        internal void OnOutOfScope()
        {
            this._metaDataSent = null;
            this._recordBufferSent = null;
            this._hadErrorInResultSet = false;
            this._isBusy = false;
        }

        public void Send(SqlDataRecord record)
        {
            ADP.CheckArgumentNull(record, "record");
            this.SetPipeBusy();
            try
            {
                this.EnsureNormalSendValid("Send");
                if (record.FieldCount != 0)
                {
                    SmiRecordBuffer recordBuffer;
                    if (record.RecordContext == this._smiContext)
                    {
                        recordBuffer = record.RecordBuffer;
                    }
                    else
                    {
                        SmiExtendedMetaData[] smiMetaData = record.InternalGetSmiMetaData();
                        recordBuffer = this._smiContext.CreateRecordBuffer(smiMetaData, this._eventSink);
                        if (SmiContextFactory.Instance.NegotiatedSmiVersion >= 210L)
                        {
                            ValueUtilsSmi.FillCompatibleSettersFromRecord(this._eventSink, recordBuffer, smiMetaData, record, null);
                        }
                        else
                        {
                            ValueUtilsSmi.FillCompatibleITypedSettersFromRecord(this._eventSink, recordBuffer, smiMetaData, record);
                        }
                    }
                    this._smiContext.SendResultsStartToPipe(recordBuffer, this._eventSink);
                    this._eventSink.ProcessMessagesAndThrow();
                    try
                    {
                        this._smiContext.SendResultsRowToPipe(recordBuffer, this._eventSink);
                        this._eventSink.ProcessMessagesAndThrow();
                    }
                    finally
                    {
                        this._smiContext.SendResultsEndToPipe(recordBuffer, this._eventSink);
                        this._eventSink.ProcessMessagesAndThrow();
                    }
                }
            }
            finally
            {
                this.ClearPipeBusy();
            }
        }

        public void Send(SqlDataReader reader)
        {
            ADP.CheckArgumentNull(reader, "reader");
            this.SetPipeBusy();
            try
            {
                this.EnsureNormalSendValid("Send");
                do
                {
                    SmiExtendedMetaData[] internalSmiMetaData = reader.GetInternalSmiMetaData();
                    if ((internalSmiMetaData != null) && (internalSmiMetaData.Length != 0))
                    {
                        using (SmiRecordBuffer buffer = this._smiContext.CreateRecordBuffer(internalSmiMetaData, this._eventSink))
                        {
                            this._eventSink.ProcessMessagesAndThrow();
                            this._smiContext.SendResultsStartToPipe(buffer, this._eventSink);
                            this._eventSink.ProcessMessagesAndThrow();
                            try
                            {
                                while (reader.Read())
                                {
                                    if (SmiContextFactory.Instance.NegotiatedSmiVersion >= 210L)
                                    {
                                        ValueUtilsSmi.FillCompatibleSettersFromReader(this._eventSink, buffer, new List<SmiExtendedMetaData>(internalSmiMetaData), reader);
                                    }
                                    else
                                    {
                                        ValueUtilsSmi.FillCompatibleITypedSettersFromReader(this._eventSink, buffer, internalSmiMetaData, reader);
                                    }
                                    this._smiContext.SendResultsRowToPipe(buffer, this._eventSink);
                                    this._eventSink.ProcessMessagesAndThrow();
                                }
                            }
                            finally
                            {
                                this._smiContext.SendResultsEndToPipe(buffer, this._eventSink);
                                this._eventSink.ProcessMessagesAndThrow();
                            }
                        }
                    }
                }
                while (reader.NextResult());
            }
            finally
            {
                this.ClearPipeBusy();
            }
        }

        public void Send(string message)
        {
            ADP.CheckArgumentNull(message, "message");
            if (0xfa0L < message.Length)
            {
                throw SQL.SqlPipeMessageTooLong(message.Length);
            }
            this.SetPipeBusy();
            try
            {
                this.EnsureNormalSendValid("Send");
                this._smiContext.SendMessageToPipe(message, this._eventSink);
                this._eventSink.ProcessMessagesAndThrow();
            }
            finally
            {
                this.ClearPipeBusy();
            }
        }

        public void SendResultsEnd()
        {
            this.SetPipeBusy();
            try
            {
                this.EnsureResultStarted("SendResultsEnd");
                this._smiContext.SendResultsEndToPipe(this._recordBufferSent, this._eventSink);
                this._metaDataSent = null;
                this._recordBufferSent = null;
                this._hadErrorInResultSet = false;
                this._eventSink.ProcessMessagesAndThrow();
            }
            finally
            {
                this.ClearPipeBusy();
            }
        }

        public void SendResultsRow(SqlDataRecord record)
        {
            ADP.CheckArgumentNull(record, "record");
            this.SetPipeBusy();
            try
            {
                SmiRecordBuffer recordBuffer;
                this.EnsureResultStarted("SendResultsRow");
                if (this._hadErrorInResultSet)
                {
                    throw SQL.SqlPipeErrorRequiresSendEnd();
                }
                this._hadErrorInResultSet = true;
                if (record.RecordContext == this._smiContext)
                {
                    recordBuffer = record.RecordBuffer;
                }
                else
                {
                    SmiExtendedMetaData[] smiMetaData = record.InternalGetSmiMetaData();
                    recordBuffer = this._smiContext.CreateRecordBuffer(smiMetaData, this._eventSink);
                    if (SmiContextFactory.Instance.NegotiatedSmiVersion >= 210L)
                    {
                        ValueUtilsSmi.FillCompatibleSettersFromRecord(this._eventSink, recordBuffer, smiMetaData, record, null);
                    }
                    else
                    {
                        ValueUtilsSmi.FillCompatibleITypedSettersFromRecord(this._eventSink, recordBuffer, smiMetaData, record);
                    }
                }
                this._smiContext.SendResultsRowToPipe(recordBuffer, this._eventSink);
                this._eventSink.ProcessMessagesAndThrow();
                this._hadErrorInResultSet = false;
            }
            finally
            {
                this.ClearPipeBusy();
            }
        }

        public void SendResultsStart(SqlDataRecord record)
        {
            ADP.CheckArgumentNull(record, "record");
            this.SetPipeBusy();
            try
            {
                this.EnsureNormalSendValid("SendResultsStart");
                SmiRecordBuffer recordBuffer = record.RecordBuffer;
                if (record.RecordContext == this._smiContext)
                {
                    recordBuffer = record.RecordBuffer;
                }
                else
                {
                    recordBuffer = this._smiContext.CreateRecordBuffer(record.InternalGetSmiMetaData(), this._eventSink);
                }
                this._smiContext.SendResultsStartToPipe(recordBuffer, this._eventSink);
                this._eventSink.ProcessMessagesAndThrow();
                this._recordBufferSent = recordBuffer;
                this._metaDataSent = record.InternalGetMetaData();
            }
            finally
            {
                this.ClearPipeBusy();
            }
        }

        private void SetPipeBusy()
        {
            if (this._isBusy)
            {
                throw SQL.SqlPipeIsBusy();
            }
            this._isBusy = true;
        }

        public bool IsSendingResults
        {
            get
            {
                return (null != this._metaDataSent);
            }
        }
    }
}


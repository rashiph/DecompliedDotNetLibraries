namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlClient;

    internal class SmiEventSink_Default : SmiEventSink
    {
        private SqlErrorCollection _errors;
        private SmiEventSink _parent;
        private SqlErrorCollection _warnings;

        internal SmiEventSink_Default()
        {
        }

        internal SmiEventSink_Default(SmiEventSink parent)
        {
            this._parent = parent;
        }

        internal override void BatchCompleted()
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.BatchCompleted);
            }
            this._parent.BatchCompleted();
        }

        internal override void DefaultDatabaseChanged(string databaseName)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.DefaultDatabaseChanged);
            }
            this._parent.DefaultDatabaseChanged(databaseName);
        }

        protected virtual void DispatchMessages(bool ignoreNonFatalMessages)
        {
            SmiEventSink_Default default2 = (SmiEventSink_Default) this._parent;
            if (default2 != null)
            {
                default2.DispatchMessages(ignoreNonFatalMessages);
            }
            else
            {
                SqlException exception = this.ProcessMessages(true, ignoreNonFatalMessages);
                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        internal override void MessagePosted(int number, byte state, byte errorClass, string server, string message, string procedure, int lineNumber)
        {
            if (this._parent == null)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SmiEventSink_Default.MessagePosted|ADV> %d#, number=%d state=%d errorClass=%d server='%ls' message='%ls' procedure='%ls' linenumber=%d.\n", 0, number, state, errorClass, (server != null) ? server : "<null>", (message != null) ? message : "<null>", (procedure != null) ? procedure : "<null>", lineNumber);
                }
                SqlError error = new SqlError(number, state, errorClass, server, message, procedure, lineNumber);
                if (error.Class < 11)
                {
                    this.Warnings.Add(error);
                }
                else
                {
                    this.Errors.Add(error);
                }
            }
            else
            {
                this._parent.MessagePosted(number, state, errorClass, server, message, procedure, lineNumber);
            }
        }

        internal override void MetaDataAvailable(SmiQueryMetaData[] metaData, bool nextEventIsRow)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.MetaDataAvailable);
            }
            this._parent.MetaDataAvailable(metaData, nextEventIsRow);
        }

        internal override void ParameterAvailable(SmiParameterMetaData metaData, SmiTypedGetterSetter paramValue, int ordinal)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.ParameterAvailable);
            }
            this._parent.ParameterAvailable(metaData, paramValue, ordinal);
        }

        internal override void ParametersAvailable(SmiParameterMetaData[] metaData, ITypedGettersV3 paramValues)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.ParametersAvailable);
            }
            this._parent.ParametersAvailable(metaData, paramValues);
        }

        protected SqlException ProcessMessages(bool ignoreWarnings, bool ignoreNonFatalMessages)
        {
            SqlErrorCollection errorCollection = null;
            SqlException exception = null;
            if (this._errors != null)
            {
                if (ignoreNonFatalMessages)
                {
                    errorCollection = new SqlErrorCollection();
                    foreach (SqlError error in this._errors)
                    {
                        if (error.Class >= 20)
                        {
                            errorCollection.Add(error);
                        }
                    }
                    if (errorCollection.Count <= 0)
                    {
                        errorCollection = null;
                    }
                }
                else
                {
                    if (this._warnings != null)
                    {
                        foreach (SqlError error2 in this._warnings)
                        {
                            this._errors.Add(error2);
                        }
                    }
                    errorCollection = this._errors;
                }
                this._errors = null;
                this._warnings = null;
            }
            else
            {
                if (!ignoreWarnings)
                {
                    errorCollection = this._warnings;
                }
                this._warnings = null;
            }
            if (errorCollection != null)
            {
                exception = SqlException.CreateException(errorCollection, this.ServerVersion);
            }
            return exception;
        }

        internal void ProcessMessagesAndThrow()
        {
            this.ProcessMessagesAndThrow(false);
        }

        internal void ProcessMessagesAndThrow(bool ignoreNonFatalMessages)
        {
            if (this.HasMessages)
            {
                this.DispatchMessages(ignoreNonFatalMessages);
            }
        }

        internal override void RowAvailable(ITypedGetters rowData)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.RowAvailable);
            }
            this._parent.RowAvailable(rowData);
        }

        internal override void RowAvailable(ITypedGettersV3 rowData)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.RowAvailable);
            }
            this._parent.RowAvailable(rowData);
        }

        internal override void StatementCompleted(int rowsAffected)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.StatementCompleted);
            }
            this._parent.StatementCompleted(rowsAffected);
        }

        internal override void TransactionCommitted(long transactionId)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.TransactionCommitted);
            }
            this._parent.TransactionCommitted(transactionId);
        }

        internal override void TransactionDefected(long transactionId)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.TransactionDefected);
            }
            this._parent.TransactionDefected(transactionId);
        }

        internal override void TransactionEnded(long transactionId)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.TransactionEnded);
            }
            this._parent.TransactionEnded(transactionId);
        }

        internal override void TransactionEnlisted(long transactionId)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.TransactionEnlisted);
            }
            this._parent.TransactionEnlisted(transactionId);
        }

        internal override void TransactionRolledBack(long transactionId)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.TransactionRolledBack);
            }
            this._parent.TransactionRolledBack(transactionId);
        }

        internal override void TransactionStarted(long transactionId)
        {
            if (this._parent == null)
            {
                throw SQL.UnexpectedSmiEvent(UnexpectedEventType.TransactionStarted);
            }
            this._parent.TransactionStarted(transactionId);
        }

        private SqlErrorCollection Errors
        {
            get
            {
                if (this._errors == null)
                {
                    this._errors = new SqlErrorCollection();
                }
                return this._errors;
            }
        }

        internal bool HasMessages
        {
            get
            {
                SmiEventSink_Default default2 = (SmiEventSink_Default) this._parent;
                if (default2 != null)
                {
                    return default2.HasMessages;
                }
                return ((this._errors != null) || (null != this._warnings));
            }
        }

        internal SmiEventSink Parent
        {
            get
            {
                return this._parent;
            }
            set
            {
                this._parent = value;
            }
        }

        internal virtual string ServerVersion
        {
            get
            {
                return null;
            }
        }

        private SqlErrorCollection Warnings
        {
            get
            {
                if (this._warnings == null)
                {
                    this._warnings = new SqlErrorCollection();
                }
                return this._warnings;
            }
        }

        internal enum UnexpectedEventType
        {
            BatchCompleted,
            ColumnInfoAvailable,
            DefaultDatabaseChanged,
            MessagePosted,
            MetaDataAvailable,
            ParameterAvailable,
            ParametersAvailable,
            RowAvailable,
            StatementCompleted,
            TableNameAvailable,
            TransactionCommitted,
            TransactionDefected,
            TransactionEnlisted,
            TransactionEnded,
            TransactionRolledBack,
            TransactionStarted
        }
    }
}


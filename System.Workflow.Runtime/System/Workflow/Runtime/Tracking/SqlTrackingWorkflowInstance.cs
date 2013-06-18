namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime;
    using System.Xml;

    public class SqlTrackingWorkflowInstance
    {
        private List<ActivityTrackingRecord> _activityEvents;
        private DateTime _actMinDT;
        private bool _autoRefresh;
        private bool _changed;
        private DateTime _changesMinDT;
        private DateTime _childMinDT;
        private string _connectionString;
        private DateTime _currDT;
        private static int _deadlock = 0x4b5;
        private Activity _def;
        private Guid _id;
        private DateTime _initialized;
        private DateTime _instMinDT;
        private long _internalId;
        private DateTime _invMinDT;
        private List<SqlTrackingWorkflowInstance> _invoked;
        private Guid _invoker;
        private static short _retries = 5;
        private WorkflowStatus _status;
        private List<UserTrackingRecord> _userEvents;
        private DateTime _userMinDT;
        private List<WorkflowTrackingRecord> _workflowEvents;
        private Type _workflowType;

        private SqlTrackingWorkflowInstance()
        {
            this._currDT = DateTime.UtcNow;
            this._actMinDT = SqlDateTime.MinValue.Value;
            this._userMinDT = SqlDateTime.MinValue.Value;
            this._instMinDT = SqlDateTime.MinValue.Value;
            this._childMinDT = SqlDateTime.MinValue.Value;
            this._changesMinDT = SqlDateTime.MinValue.Value;
            this._invMinDT = SqlDateTime.MinValue.Value;
            this._internalId = -1L;
            this._invoker = Guid.Empty;
            this._activityEvents = new List<ActivityTrackingRecord>();
            this._userEvents = new List<UserTrackingRecord>();
            this._workflowEvents = new List<WorkflowTrackingRecord>();
            this._invoked = new List<SqlTrackingWorkflowInstance>();
        }

        internal SqlTrackingWorkflowInstance(string connectionString)
        {
            this._currDT = DateTime.UtcNow;
            this._actMinDT = SqlDateTime.MinValue.Value;
            this._userMinDT = SqlDateTime.MinValue.Value;
            this._instMinDT = SqlDateTime.MinValue.Value;
            this._childMinDT = SqlDateTime.MinValue.Value;
            this._changesMinDT = SqlDateTime.MinValue.Value;
            this._invMinDT = SqlDateTime.MinValue.Value;
            this._internalId = -1L;
            this._invoker = Guid.Empty;
            this._activityEvents = new List<ActivityTrackingRecord>();
            this._userEvents = new List<UserTrackingRecord>();
            this._workflowEvents = new List<WorkflowTrackingRecord>();
            this._invoked = new List<SqlTrackingWorkflowInstance>();
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            this._connectionString = connectionString;
        }

        private SqlCommand CreateInternalIdDateTimeCommand(string commandText, DateTime minDT)
        {
            return this.CreateInternalIdDateTimeCommand(commandText, minDT, this._currDT);
        }

        private SqlCommand CreateInternalIdDateTimeCommand(string commandText, DateTime minDT, DateTime maxDT)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("commandText");
            }
            SqlCommand command = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = commandText
            };
            SqlParameter parameter = new SqlParameter("@WorkflowInstanceInternalId", SqlDbType.BigInt) {
                Value = this._internalId
            };
            command.Parameters.Add(parameter);
            parameter = new SqlParameter("@BeginDateTime", SqlDbType.DateTime) {
                Value = minDT
            };
            command.Parameters.Add(parameter);
            parameter = new SqlParameter("@EndDateTime", SqlDbType.DateTime) {
                Value = maxDT
            };
            command.Parameters.Add(parameter);
            return command;
        }

        private void ExecuteRetried(SqlCommand cmd, LoadFromReader loader)
        {
            this.ExecuteRetried(cmd, loader, null);
        }

        private void ExecuteRetried(SqlCommand cmd, LoadFromReader loader, object loadFromReaderParam)
        {
            SqlDataReader reader = null;
            short num = 0;
        Label_0004:;
            try
            {
                using (SqlConnection connection = new SqlConnection(this._connectionString))
                {
                    cmd.Connection = connection;
                    cmd.Connection.Open();
                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    loader(reader, loadFromReaderParam);
                }
            }
            catch (SqlException exception)
            {
                if ((_deadlock != exception.Number) || ((num = (short) (num + 1)) >= _retries))
                {
                    throw;
                }
                goto Label_0004;
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
            }
        }

        private void LoadActivityEvents()
        {
            SqlCommand cmd = this.CreateInternalIdDateTimeCommand("[dbo].[GetActivityEventsWithDetails]", this._actMinDT);
            this.ExecuteRetried(cmd, new LoadFromReader(this.LoadActivityEventsFromReader));
        }

        private void LoadActivityEventsFromReader(SqlDataReader reader, object parameter)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            Dictionary<long, ActivityTrackingRecord> dictionary = new Dictionary<long, ActivityTrackingRecord>();
            DateTime time = SqlDateTime.MinValue.Value;
            while (reader.Read())
            {
                string qualifiedName = reader.GetString(0);
                ActivityExecutionStatus executionStatus = (ActivityExecutionStatus) reader[1];
                DateTime dateTime = reader.GetDateTime(2);
                Guid contextGuid = reader.GetGuid(3);
                Guid guid = reader.GetGuid(4);
                int eventOrder = reader.GetInt32(5);
                if (reader.IsDBNull(6) || reader.IsDBNull(7))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.SqlTrackingTypeNotFound, new object[] { qualifiedName }));
                }
                Type activityType = Type.GetType(reader.GetString(6) + ", " + reader.GetString(7), true, false);
                long key = reader.GetInt64(8);
                DateTime time3 = reader.GetDateTime(9);
                dictionary.Add(key, new ActivityTrackingRecord(activityType, qualifiedName, contextGuid, guid, executionStatus, dateTime, eventOrder, null));
                if (time3 > time)
                {
                    time = time3;
                }
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidActivityEventReader);
            }
            while (reader.Read())
            {
                long num3 = reader.GetInt64(0);
                string str2 = null;
                if (!reader.IsDBNull(1))
                {
                    str2 = reader.GetString(1);
                }
                ActivityTrackingRecord record = null;
                if (dictionary.TryGetValue(num3, out record) && (record != null))
                {
                    record.Annotations.Add(str2);
                }
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidActivityEventReader);
            }
            BinaryFormatter formatter = new BinaryFormatter();
            Dictionary<long, TrackingDataItem> dictionary2 = new Dictionary<long, TrackingDataItem>();
            while (reader.Read())
            {
                long num4 = reader.GetInt64(0);
                long num5 = reader.GetInt64(1);
                string str3 = reader.GetString(2);
                string str4 = null;
                object obj2 = null;
                if (!reader.IsDBNull(3))
                {
                    str4 = reader.GetString(3);
                }
                if (!reader.IsDBNull(4))
                {
                    obj2 = formatter.Deserialize(new MemoryStream((byte[]) reader[4]));
                }
                TrackingDataItem item = new TrackingDataItem {
                    FieldName = str3
                };
                if (obj2 != null)
                {
                    item.Data = obj2;
                }
                else
                {
                    item.Data = str4;
                }
                dictionary2.Add(num5, item);
                ActivityTrackingRecord record2 = null;
                if (dictionary.TryGetValue(num4, out record2) && (record2 != null))
                {
                    record2.Body.Add(item);
                }
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidActivityEventReader);
            }
            while (reader.Read())
            {
                long num6 = reader.GetInt64(0);
                string str5 = null;
                if (!reader.IsDBNull(1))
                {
                    str5 = reader.GetString(1);
                }
                TrackingDataItem item2 = null;
                if (dictionary2.TryGetValue(num6, out item2) && (item2 != null))
                {
                    item2.Annotations.Add(str5);
                }
            }
            this._activityEvents.AddRange(dictionary.Values);
            if (time > SqlDateTime.MinValue.Value)
            {
                this._actMinDT = time;
            }
        }

        private void LoadChangesFromReader(SqlDataReader reader, object parameter)
        {
            if (reader.Read())
            {
                DateTime dateTime = this._changesMinDT;
                if (!reader.IsDBNull(0))
                {
                    dateTime = reader.GetDateTime(0);
                }
                if (reader.NextResult())
                {
                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                    DesignerSerializationManager serializationManager = new DesignerSerializationManager();
                    while (reader.Read())
                    {
                        using (StringReader reader2 = new StringReader(reader.GetString(0)))
                        {
                            using (serializationManager.CreateSession())
                            {
                                using (XmlReader reader3 = XmlReader.Create(reader2))
                                {
                                    ActivityChangeAction action = serializer.Deserialize(serializationManager, reader3) as ActivityChangeAction;
                                    IList errors = serializationManager.Errors;
                                    if (action == null)
                                    {
                                        throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);
                                    }
                                    action.ApplyTo(this._def);
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (dateTime > this._changesMinDT)
                {
                    this._changed = true;
                    this._changesMinDT = dateTime;
                }
            }
        }

        private void LoadDef()
        {
            SqlCommand cmd = null;
            if (this._def == null)
            {
                cmd = new SqlCommand {
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "[dbo].[GetWorkflowDefinition]"
                };
                cmd.Parameters.Add(new SqlParameter("@WorkflowInstanceInternalId", this._internalId));
                this.ExecuteRetried(cmd, new LoadFromReader(this.LoadDefFromReader));
            }
            cmd = this.CreateInternalIdDateTimeCommand("[dbo].[GetWorkflowChanges]", this._changesMinDT);
            this.ExecuteRetried(cmd, new LoadFromReader(this.LoadChangesFromReader));
        }

        private void LoadDefFromReader(SqlDataReader reader, object parameter)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (!reader.Read())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidDefinitionReader);
            }
            StringReader input = new StringReader(reader.GetString(0));
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            IList errors = null;
            try
            {
                using (serializationManager.CreateSession())
                {
                    using (XmlReader reader3 = XmlReader.Create(input))
                    {
                        this._def = serializer.Deserialize(serializationManager, reader3) as Activity;
                        errors = serializationManager.Errors;
                    }
                }
            }
            finally
            {
                input.Close();
            }
            if ((this._def == null) || ((errors != null) && (errors.Count > 0)))
            {
                throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);
            }
        }

        private void LoadInvokedWorkflows()
        {
            SqlCommand cmd = new SqlCommand {
                CommandText = "[dbo].[GetInvokedWorkflows]",
                CommandType = CommandType.StoredProcedure
            };
            SqlParameter parameter = new SqlParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier) {
                Value = this._id
            };
            cmd.Parameters.Add(parameter);
            parameter = new SqlParameter("@BeginDateTime", SqlDbType.DateTime) {
                Value = this._invMinDT
            };
            cmd.Parameters.Add(parameter);
            parameter = new SqlParameter("@EndDateTime", SqlDbType.DateTime) {
                Value = this._currDT
            };
            cmd.Parameters.Add(parameter);
            this.ExecuteRetried(cmd, new LoadFromReader(this.LoadInvokedWorkflowsFromReader));
        }

        private void LoadInvokedWorkflowsFromReader(SqlDataReader reader, object parameter)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            DateTime initialized = SqlDateTime.MinValue.Value;
            while (reader.Read())
            {
                SqlTrackingWorkflowInstance item = SqlTrackingQuery.BuildInstance(reader, this._connectionString);
                if (item.Initialized > initialized)
                {
                    initialized = item.Initialized;
                }
                this._invoked.Add(item);
            }
            if (initialized > SqlDateTime.MinValue.Value)
            {
                this._invMinDT = initialized;
            }
        }

        private void LoadUserEvents()
        {
            SqlCommand cmd = this.CreateInternalIdDateTimeCommand("[dbo].[GetUserEventsWithDetails]", this._userMinDT);
            this.ExecuteRetried(cmd, new LoadFromReader(this.LoadUserEventsFromReader));
        }

        private void LoadUserEventsFromReader(SqlDataReader reader, object parameter)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            BinaryFormatter formatter = new BinaryFormatter();
            Dictionary<long, UserTrackingRecord> dictionary = new Dictionary<long, UserTrackingRecord>();
            DateTime time = SqlDateTime.MinValue.Value;
            while (reader.Read())
            {
                string qualifiedName = reader.GetString(0);
                DateTime dateTime = reader.GetDateTime(1);
                Guid contextGuid = reader.GetGuid(2);
                Guid guid = reader.GetGuid(3);
                int eventOrder = reader.GetInt32(4);
                string userDataKey = null;
                if (!reader.IsDBNull(5))
                {
                    userDataKey = reader.GetString(5);
                }
                object userData = null;
                if (!reader.IsDBNull(7))
                {
                    userData = formatter.Deserialize(new MemoryStream((byte[]) reader[7]));
                }
                else if (!reader.IsDBNull(6))
                {
                    userData = reader.GetString(6);
                }
                if (reader.IsDBNull(8) || reader.IsDBNull(9))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.SqlTrackingTypeNotFound, new object[] { qualifiedName }));
                }
                Type activityType = Type.GetType(reader.GetString(8) + ", " + reader.GetString(9), true, false);
                long key = reader.GetInt64(10);
                DateTime time3 = reader.GetDateTime(11);
                dictionary.Add(key, new UserTrackingRecord(activityType, qualifiedName, contextGuid, guid, dateTime, eventOrder, userDataKey, userData));
                if (time3 > time)
                {
                    time = time3;
                }
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidUserEventReader);
            }
            while (reader.Read())
            {
                long num3 = reader.GetInt64(0);
                string str3 = null;
                if (!reader.IsDBNull(1))
                {
                    str3 = reader.GetString(1);
                }
                UserTrackingRecord record = null;
                if (dictionary.TryGetValue(num3, out record) && (record != null))
                {
                    record.Annotations.Add(str3);
                }
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidUserEventReader);
            }
            Dictionary<long, TrackingDataItem> dictionary2 = new Dictionary<long, TrackingDataItem>();
            while (reader.Read())
            {
                long num4 = reader.GetInt64(0);
                long num5 = reader.GetInt64(1);
                string str4 = reader.GetString(2);
                string str5 = null;
                object obj3 = null;
                if (!reader.IsDBNull(3))
                {
                    str5 = reader.GetString(3);
                }
                if (!reader.IsDBNull(4))
                {
                    obj3 = formatter.Deserialize(new MemoryStream((byte[]) reader[4]));
                }
                TrackingDataItem item = new TrackingDataItem {
                    FieldName = str4
                };
                if (obj3 != null)
                {
                    item.Data = obj3;
                }
                else
                {
                    item.Data = str5;
                }
                dictionary2.Add(num5, item);
                UserTrackingRecord record2 = null;
                if (dictionary.TryGetValue(num4, out record2) && (record2 != null))
                {
                    record2.Body.Add(item);
                }
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidUserEventReader);
            }
            while (reader.Read())
            {
                long num6 = reader.GetInt64(0);
                string str6 = null;
                if (!reader.IsDBNull(1))
                {
                    str6 = reader.GetString(1);
                }
                TrackingDataItem item2 = null;
                if (dictionary2.TryGetValue(num6, out item2) && (item2 != null))
                {
                    item2.Annotations.Add(str6);
                }
            }
            this._userEvents.AddRange(dictionary.Values);
            if (time > SqlDateTime.MinValue.Value)
            {
                this._userMinDT = time;
            }
        }

        private void LoadWorkflowChangeEventArgsFromReader(SqlDataReader reader, object parameter)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }
            WorkflowTrackingRecord record = parameter as WorkflowTrackingRecord;
            if (record == null)
            {
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsParameter, "parameter");
            }
            if (!reader.Read())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsReader);
            }
            StringReader input = new StringReader(reader.GetString(0));
            Activity rootActivity = null;
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            IList errors = null;
            try
            {
                using (serializationManager.CreateSession())
                {
                    using (XmlReader reader3 = XmlReader.Create(input))
                    {
                        rootActivity = serializer.Deserialize(serializationManager, reader3) as Activity;
                        errors = serializationManager.Errors;
                    }
                }
            }
            finally
            {
                input.Close();
            }
            if ((rootActivity == null) || ((errors != null) && (errors.Count > 0)))
            {
                throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsReader);
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsReader);
            }
            List<WorkflowChangeAction> changes = new List<WorkflowChangeAction>();
            DateTime minValue = DateTime.MinValue;
            int num = -1;
            while (reader.Read())
            {
                DateTime dateTime = reader.GetDateTime(1);
                int num2 = reader.GetInt32(2);
                int num3 = reader.GetInt32(3);
                if ((dateTime > minValue) && (num2 > num))
                {
                    num = num2;
                    minValue = dateTime;
                    changes = new List<WorkflowChangeAction>();
                }
                using (input = new StringReader(reader.GetString(0)))
                {
                    using (serializationManager.CreateSession())
                    {
                        using (XmlReader reader4 = XmlReader.Create(input))
                        {
                            ActivityChangeAction item = serializer.Deserialize(serializationManager, reader4) as ActivityChangeAction;
                            errors = serializationManager.Errors;
                            if (item == null)
                            {
                                throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);
                            }
                            changes.Add(item);
                            item.ApplyTo(rootActivity);
                        }
                    }
                    continue;
                }
            }
            record.EventArgs = new TrackingWorkflowChangedEventArgs(changes, rootActivity);
        }

        private void LoadWorkflowEvents()
        {
            SqlCommand cmd = this.CreateInternalIdDateTimeCommand("[dbo].[GetWorkflowInstanceEventsWithDetails]", this._instMinDT);
            this.ExecuteRetried(cmd, new LoadFromReader(this.LoadWorkflowEventsFromReader));
        }

        private void LoadWorkflowEventsFromReader(SqlDataReader reader, object parameter)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            DateTime time = SqlDateTime.MinValue.Value;
            Dictionary<long, WorkflowTrackingRecord> dictionary = new Dictionary<long, WorkflowTrackingRecord>();
            while (reader.Read())
            {
                TrackingWorkflowEvent trackingWorkflowEvent = (TrackingWorkflowEvent) reader[0];
                DateTime dateTime = reader.GetDateTime(1);
                int eventOrder = reader.GetInt32(2);
                object obj2 = null;
                EventArgs eventArgs = null;
                if (!reader.IsDBNull(3))
                {
                    obj2 = new BinaryFormatter().Deserialize(new MemoryStream((byte[]) reader[3]));
                    if (obj2 is EventArgs)
                    {
                        eventArgs = (EventArgs) obj2;
                    }
                }
                long key = reader.GetInt64(4);
                DateTime time3 = reader.GetDateTime(5);
                dictionary.Add(key, new WorkflowTrackingRecord(trackingWorkflowEvent, dateTime, eventOrder, eventArgs));
                if (time3 > time)
                {
                    time = time3;
                }
            }
            if (!reader.NextResult())
            {
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowInstanceEventReader);
            }
            while (reader.Read())
            {
                long num3 = reader.GetInt64(0);
                string item = null;
                if (!reader.IsDBNull(1))
                {
                    item = reader.GetString(1);
                }
                WorkflowTrackingRecord record = null;
                if (dictionary.TryGetValue(num3, out record) && (record != null))
                {
                    record.Annotations.Add(item);
                }
            }
            if (!reader.IsClosed)
            {
                reader.Close();
            }
            foreach (KeyValuePair<long, WorkflowTrackingRecord> pair in dictionary)
            {
                WorkflowTrackingRecord loadFromReaderParam = pair.Value;
                if (TrackingWorkflowEvent.Changed == loadFromReaderParam.TrackingWorkflowEvent)
                {
                    SqlCommand cmd = new SqlCommand {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "[dbo].[GetWorkflowChangeEventArgs]"
                    };
                    cmd.Parameters.Add(new SqlParameter("@WorkflowInstanceInternalId", this._internalId));
                    cmd.Parameters.Add(new SqlParameter("@BeginDateTime", SqlDateTime.MinValue.Value));
                    cmd.Parameters.Add(new SqlParameter("@WorkflowInstanceEventId", pair.Key));
                    this.ExecuteRetried(cmd, new LoadFromReader(this.LoadWorkflowChangeEventArgsFromReader), loadFromReaderParam);
                }
            }
            this._workflowEvents.AddRange(dictionary.Values);
            if (time > SqlDateTime.MinValue.Value)
            {
                this._instMinDT = time;
            }
        }

        public void Refresh()
        {
            this._currDT = DateTime.UtcNow;
        }

        public IList<ActivityTrackingRecord> ActivityEvents
        {
            get
            {
                if (this._autoRefresh)
                {
                    this.Refresh();
                }
                this.LoadActivityEvents();
                return this._activityEvents;
            }
        }

        public bool AutoRefresh
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._autoRefresh;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._autoRefresh = value;
            }
        }

        public DateTime Initialized
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._initialized;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._initialized = value;
            }
        }

        public IList<SqlTrackingWorkflowInstance> InvokedWorkflows
        {
            get
            {
                if (this._autoRefresh)
                {
                    this.Refresh();
                }
                this.LoadInvokedWorkflows();
                return this._invoked;
            }
        }

        public Guid InvokingWorkflowInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._invoker;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._invoker = value;
            }
        }

        public WorkflowStatus Status
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._status;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._status = value;
            }
        }

        public IList<UserTrackingRecord> UserEvents
        {
            get
            {
                if (this._autoRefresh)
                {
                    this.Refresh();
                }
                this.LoadUserEvents();
                return this._userEvents;
            }
        }

        public Activity WorkflowDefinition
        {
            get
            {
                if (this._autoRefresh)
                {
                    this.Refresh();
                }
                this.LoadDef();
                return this._def;
            }
        }

        public bool WorkflowDefinitionUpdated
        {
            get
            {
                if (this._autoRefresh)
                {
                    this.Refresh();
                }
                this.LoadDef();
                return this._changed;
            }
        }

        public IList<WorkflowTrackingRecord> WorkflowEvents
        {
            get
            {
                if (this._autoRefresh)
                {
                    this.Refresh();
                }
                this.LoadWorkflowEvents();
                return this._workflowEvents;
            }
        }

        public Guid WorkflowInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._id;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._id = value;
            }
        }

        public long WorkflowInstanceInternalId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._internalId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._internalId = value;
            }
        }

        public Type WorkflowType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workflowType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._workflowType = value;
            }
        }

        private delegate void LoadFromReader(SqlDataReader reader, object parameter);
    }
}


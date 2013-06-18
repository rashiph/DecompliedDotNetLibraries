namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using System.Transactions;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;
    using System.Xml;

    public sealed class SqlTrackingService : TrackingService, IProfileNotification
    {
        private System.Workflow.Runtime.Hosting.DbResourceAllocator _dbResourceAllocator;
        private bool _defaultProfile;
        private bool _enableRetries;
        private bool _ignoreCommonEnableRetries;
        private double _interval;
        private bool _isTrans;
        private DateTime _lastProfileCheck;
        private NameValueCollection _parameters;
        private bool _partition;
        private System.Timers.Timer _timer;
        private WorkflowCommitWorkBatchService _transactionService;
        private object _typeCacheLock;
        private TypeKeyedCollection _types;
        private string _unvalidatedConnectionString;
        private static Version UnknownProfileVersionId = new Version(0, 0);

        public event EventHandler<ProfileRemovedEventArgs> ProfileRemoved;

        public event EventHandler<ProfileUpdatedEventArgs> ProfileUpdated;

        public SqlTrackingService(NameValueCollection parameters)
        {
            this._isTrans = true;
            this._defaultProfile = true;
            this._timer = new System.Timers.Timer();
            this._interval = 60000.0;
            this._types = new TypeKeyedCollection();
            this._typeCacheLock = new object();
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters", ExecutionStringManager.MissingParameters);
            }
            if (parameters.Count > 0)
            {
                foreach (string str in parameters.Keys)
                {
                    if (string.Compare("IsTransactional", str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._isTrans = bool.Parse(parameters[str]);
                    }
                    else if (string.Compare("UseDefaultProfile", str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._defaultProfile = bool.Parse(parameters[str]);
                    }
                    else if (string.Compare("PartitionOnCompletion", str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._partition = bool.Parse(parameters[str]);
                    }
                    else if (string.Compare("ProfileChangeCheckInterval", str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._interval = double.Parse(parameters[str], NumberFormatInfo.InvariantInfo);
                        if (this._interval <= 0.0)
                        {
                            throw new ArgumentException(ExecutionStringManager.InvalidProfileCheckValue);
                        }
                    }
                    else if (string.Compare("ConnectionString", str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._unvalidatedConnectionString = parameters[str];
                    }
                    else if (string.Compare("EnableRetries", str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._enableRetries = bool.Parse(parameters[str]);
                        this._ignoreCommonEnableRetries = true;
                    }
                }
            }
            this._parameters = parameters;
        }

        public SqlTrackingService(string connectionString)
        {
            this._isTrans = true;
            this._defaultProfile = true;
            this._timer = new System.Timers.Timer();
            this._interval = 60000.0;
            this._types = new TypeKeyedCollection();
            this._typeCacheLock = new object();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);
            }
            this._unvalidatedConnectionString = connectionString;
        }

        private void CheckProfileChanges(object sender, ElapsedEventArgs e)
        {
            DbCommand command = null;
            DbDataReader reader = null;
            try
            {
                Type type;
                if ((this.ProfileUpdated != null) || (this.ProfileRemoved != null))
                {
                    command = this._dbResourceAllocator.NewCommand();
                    command.CommandText = "GetUpdatedTrackingProfiles";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@LastCheckDateTime", this._lastProfileCheck));
                    DbParameter parameter = this._dbResourceAllocator.NewDbParameter();
                    parameter.ParameterName = "@MaxCheckDateTime";
                    parameter.DbType = DbType.DateTime;
                    parameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(parameter);
                    reader = this.ExecuteReaderRetried(command, CommandBehavior.CloseConnection);
                    if (reader.HasRows)
                    {
                        goto Label_0154;
                    }
                }
                return;
            Label_00AA:
                type = null;
                string s = null;
                TrackingProfile profile = null;
                type = Assembly.Load(reader[1] as string).GetType(reader[0] as string);
                if (null != type)
                {
                    s = reader[2] as string;
                    if (s == null)
                    {
                        if (this.ProfileRemoved != null)
                        {
                            this.ProfileRemoved(this, new ProfileRemovedEventArgs(type));
                        }
                    }
                    else
                    {
                        TrackingProfileSerializer serializer = new TrackingProfileSerializer();
                        StringReader reader2 = null;
                        try
                        {
                            reader2 = new StringReader(s);
                            profile = serializer.Deserialize(reader2);
                        }
                        finally
                        {
                            if (reader2 != null)
                            {
                                reader2.Close();
                            }
                        }
                        if (this.ProfileUpdated != null)
                        {
                            this.ProfileUpdated(this, new ProfileUpdatedEventArgs(type, profile));
                        }
                    }
                }
            Label_0154:
                if (reader.Read())
                {
                    goto Label_00AA;
                }
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
                if ((command != null) && (command.Parameters[1].Value != null))
                {
                    this._lastProfileCheck = (DateTime) command.Parameters[1].Value;
                }
                if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                {
                    command.Connection.Close();
                }
                this._timer.Start();
            }
        }

        internal static XmlWriter CreateXmlWriter(TextWriter output)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "\t",
                OmitXmlDeclaration = true,
                CloseOutput = true
            };
            return XmlWriter.Create(output, settings);
        }

        private void ExecuteNonQueryRetried(DbCommand command)
        {
            short retryCount = 0;
            DbRetry retry = new DbRetry(this._enableRetries);
        Label_000E:
            try
            {
                this.ResetConnectionForCommand(command);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryRetried ExecuteNonQuery start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                command.ExecuteNonQuery();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryRetried ExecuteNonQuery end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteNonQueryRetried caught exception from ExecuteNonQuery: " + exception.ToString());
                if (!retry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryRetried retrying.");
                goto Label_000E;
            }
        }

        private void ExecuteNonQueryWithTxRetried(DbCommand command)
        {
            try
            {
                short retryCount = 0;
                DbRetry retry = new DbRetry(this._enableRetries);
            Label_000E:;
                try
                {
                    this.ResetConnectionForCommand(command);
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried ExecuteNonQuery start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                    command.Transaction = command.Connection.BeginTransaction();
                    command.ExecuteNonQuery();
                    command.Transaction.Commit();
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried ExecuteNonQuery end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                }
                catch (Exception exception)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried caught exception from ExecuteNonQuery: " + exception.ToString());
                    try
                    {
                        if (command.Transaction != null)
                        {
                            command.Transaction.Rollback();
                        }
                    }
                    catch
                    {
                    }
                    if (!retry.TryDoRetry(ref retryCount))
                    {
                        throw;
                    }
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried retrying.");
                    goto Label_000E;
                }
            }
            finally
            {
                if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                {
                    command.Connection.Close();
                }
            }
        }

        private DbDataReader ExecuteReaderRetried(DbCommand command, CommandBehavior behavior)
        {
            DbDataReader reader = null;
            short retryCount = 0;
            DbRetry retry = new DbRetry(this._enableRetries);
        Label_0010:
            try
            {
                this.ResetConnectionForCommand(command);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteReaderRetried ExecuteReader start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                reader = command.ExecuteReader(behavior);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteReaderRetried ExecuteReader end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteReaderRetried caught exception from ExecuteReader: " + exception.ToString());
                if (!retry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteReaderRetried retrying.");
                goto Label_0010;
            }
            return reader;
        }

        private void ExecuteRetried(ExecuteRetriedDelegate executeRetried, object param)
        {
            short retryCount = 0;
            DbRetry retry = new DbRetry(this._enableRetries);
        Label_000E:
            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteRetried " + executeRetried.Method.Name + " start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                executeRetried(param);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteRetried " + executeRetried.Method.Name + " end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteRetried caught exception: " + exception.ToString());
                if (!retry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteRetried " + executeRetried.Method.Name + " retrying.");
                goto Label_000E;
            }
        }

        protected internal override TrackingProfile GetProfile(Guid scheduleInstanceId)
        {
            TrackingProfile profile = null;
            this.GetProfile(scheduleInstanceId, out profile);
            return profile;
        }

        private bool GetProfile(Guid scheduleInstanceId, out TrackingProfile profile)
        {
            bool flag;
            profile = null;
            DbCommand command = this._dbResourceAllocator.NewCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[dbo].[GetInstanceTrackingProfile]";
            command.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@InstanceId", scheduleInstanceId));
            DbDataReader reader = null;
            try
            {
                reader = this.ExecuteReaderRetried(command, CommandBehavior.CloseConnection);
                if (!reader.HasRows)
                {
                    reader.Close();
                    profile = null;
                    return false;
                }
                if (!reader.Read())
                {
                    reader.Close();
                    profile = null;
                    return false;
                }
                if (reader.IsDBNull(0))
                {
                    profile = null;
                }
                else
                {
                    string s = reader.GetString(0);
                    TrackingProfileSerializer serializer = new TrackingProfileSerializer();
                    StringReader reader2 = null;
                    try
                    {
                        reader2 = new StringReader(s);
                        profile = serializer.Deserialize(reader2);
                    }
                    finally
                    {
                        if (reader2 != null)
                        {
                            reader2.Close();
                        }
                    }
                }
                flag = true;
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
                if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                {
                    command.Connection.Close();
                }
            }
            return flag;
        }

        protected internal override TrackingProfile GetProfile(Type workflowType, Version profileVersion)
        {
            if (null == workflowType)
            {
                throw new ArgumentNullException("workflowType");
            }
            return this.GetProfileByScheduleType(workflowType, profileVersion, false);
        }

        private TrackingProfile GetProfileByScheduleType(Type workflowType, Version profileVersionId, bool wantToCreateDefault)
        {
            DbCommand command = this._dbResourceAllocator.NewCommand();
            DbDataReader reader = null;
            TrackingProfile profile = null;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "dbo.GetTrackingProfile";
            command.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@TypeFullName", workflowType.FullName));
            command.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@AssemblyFullName", workflowType.Assembly.FullName));
            if (profileVersionId != UnknownProfileVersionId)
            {
                command.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@Version", profileVersionId.ToString()));
            }
            command.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@CreateDefault", wantToCreateDefault));
            try
            {
                reader = this.ExecuteReaderRetried(command, CommandBehavior.CloseConnection);
                if (!reader.Read())
                {
                    return profile;
                }
                string s = reader[0] as string;
                if (s == null)
                {
                    return profile;
                }
                TrackingProfileSerializer serializer = new TrackingProfileSerializer();
                StringReader reader2 = null;
                try
                {
                    reader2 = new StringReader(s);
                    profile = serializer.Deserialize(reader2);
                }
                finally
                {
                    if (reader2 != null)
                    {
                        reader2.Close();
                    }
                }
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    reader.Close();
                }
                if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                {
                    command.Connection.Close();
                }
            }
            return profile;
        }

        protected internal override TrackingChannel GetTrackingChannel(TrackingParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            return new SqlTrackingChannel(parameters, this);
        }

        private void ResetConnectionForCommand(DbCommand command)
        {
            if ((command != null) && ((command.Connection != null) && (ConnectionState.Open != command.Connection.State)))
            {
                if (command.Connection.State != ConnectionState.Closed)
                {
                    command.Connection.Close();
                }
                command.Connection.Dispose();
                command.Connection = this._dbResourceAllocator.OpenNewConnectionNoEnlist();
            }
        }

        protected internal override void Start()
        {
            this._lastProfileCheck = DateTime.UtcNow;
            this._dbResourceAllocator = new System.Workflow.Runtime.Hosting.DbResourceAllocator(base.Runtime, this._parameters, this._unvalidatedConnectionString);
            this._transactionService = base.Runtime.GetService<WorkflowCommitWorkBatchService>();
            this._dbResourceAllocator.DetectSharedConnectionConflict(this._transactionService);
            if (!this._ignoreCommonEnableRetries && (base.Runtime != null))
            {
                NameValueConfigurationCollection commonParameters = base.Runtime.CommonParameters;
                if (commonParameters != null)
                {
                    foreach (string str in commonParameters.AllKeys)
                    {
                        if (string.Compare("EnableRetries", str, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._enableRetries = bool.Parse(commonParameters[str].Value);
                            break;
                        }
                    }
                }
            }
            this._timer.Interval = this._interval;
            this._timer.AutoReset = false;
            this._timer.Elapsed += new ElapsedEventHandler(this.CheckProfileChanges);
            this._timer.Start();
            base.Start();
        }

        protected internal override bool TryGetProfile(Type workflowType, out TrackingProfile profile)
        {
            if (null == workflowType)
            {
                throw new ArgumentNullException("workflowType");
            }
            profile = this.GetProfileByScheduleType(workflowType, UnknownProfileVersionId, this._defaultProfile);
            if (profile == null)
            {
                return false;
            }
            return true;
        }

        protected internal override bool TryReloadProfile(Type workflowType, Guid scheduleInstanceId, out TrackingProfile profile)
        {
            if (null == workflowType)
            {
                throw new ArgumentNullException("workflowType");
            }
            if (this.GetProfile(scheduleInstanceId, out profile))
            {
                return true;
            }
            profile = null;
            return false;
        }

        public string ConnectionString
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._unvalidatedConnectionString;
            }
        }

        internal System.Workflow.Runtime.Hosting.DbResourceAllocator DbResourceAllocator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._dbResourceAllocator;
            }
        }

        public bool EnableRetries
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._enableRetries;
            }
            set
            {
                this._enableRetries = value;
                this._ignoreCommonEnableRetries = true;
            }
        }

        public bool IsTransactional
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._isTrans;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._isTrans = value;
            }
        }

        public bool PartitionOnCompletion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._partition;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._partition = value;
            }
        }

        public double ProfileChangeCheckInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._interval;
            }
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidProfileCheckValue);
                }
                this._interval = value;
                this._timer.Interval = this._interval;
            }
        }

        public bool UseDefaultProfile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._defaultProfile;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._defaultProfile = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AddedActivity
        {
            public string ActivityTypeFullName;
            public string ActivityTypeAssemblyFullName;
            public string QualifiedName;
            public string ParentQualifiedName;
            public string AddedActivityActionXoml;
            public int Order;
        }

        private delegate void ExecuteRetriedDelegate(object param);

        [StructLayout(LayoutKind.Sequential)]
        private struct RemovedActivity
        {
            public string QualifiedName;
            public string ParentQualifiedName;
            public string RemovedActivityActionXoml;
            public int Order;
        }

        private class SerializedDataItem : TrackingDataItem
        {
            public bool NonSerializable;
            public byte[] SerializedData;
            public string StringData;
            public System.Type Type;
        }

        private class SerializedEventArgs : EventArgs
        {
            public byte[] SerializedArgs;
            public System.Type Type;
        }

        private class SerializedWorkflowChangedEventArgs : SqlTrackingService.SerializedEventArgs
        {
            public IList<SqlTrackingService.AddedActivity> AddedActivities = new List<SqlTrackingService.AddedActivity>();
            public IList<SqlTrackingService.RemovedActivity> RemovedActivities = new List<SqlTrackingService.RemovedActivity>();
        }

        internal class SqlTrackingChannel : TrackingChannel, IPendingWork
        {
            private static int _activityEventBatchSize = 5;
            private Dictionary<string, long> _activityInstanceId;
            private string _callPathKey;
            private bool _completedTerminated;
            private static int _dataItemAnnotationBatchSize = 5;
            private static int _dataItemBatchSize = 5;
            private static int _eventAnnotationBatchSize = 5;
            private long _internalId;
            private bool _isTrans;
            private TrackingParameters _parameters;
            private string _parentCallPathKey;
            private bool _pendingArchive;
            private SqlTrackingService _service;
            private Dictionary<string, long> _tmpActivityInstanceId;
            private long _tmpInternalId;

            protected SqlTrackingChannel()
            {
                this._internalId = -1L;
                this._tmpInternalId = -1L;
                this._activityInstanceId = new Dictionary<string, long>(0x20);
                this._tmpActivityInstanceId = new Dictionary<string, long>(10);
            }

            public SqlTrackingChannel(TrackingParameters parameters, SqlTrackingService service)
            {
                this._internalId = -1L;
                this._tmpInternalId = -1L;
                this._activityInstanceId = new Dictionary<string, long>(0x20);
                this._tmpActivityInstanceId = new Dictionary<string, long>(10);
                if (service != null)
                {
                    this._service = service;
                    this._parameters = parameters;
                    this._isTrans = service.IsTransactional;
                    this.GetCallPathKeys(parameters.CallPath);
                    if (!this._isTrans)
                    {
                        this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.ExecuteInsertWorkflowInstance), null);
                    }
                }
            }

            private void AddAddedActivity(Activity added, IList<SqlTrackingService.AddedActivity> activities)
            {
                SqlTrackingService.AddedActivity item = new SqlTrackingService.AddedActivity {
                    Order = -1
                };
                Type type = added.GetType();
                item.ActivityTypeFullName = type.FullName;
                item.ActivityTypeAssemblyFullName = type.Assembly.FullName;
                item.QualifiedName = added.QualifiedName;
                if (added.Parent != null)
                {
                    item.ParentQualifiedName = added.Parent.QualifiedName;
                }
                activities.Add(item);
                if (added is CompositeActivity)
                {
                    foreach (Activity activity2 in ((CompositeActivity) added).Activities)
                    {
                        this.AddAddedActivity(activity2, activities);
                    }
                }
            }

            private void AddAddedActivity(AddedActivityAction addedAction, int order, IList<SqlTrackingService.AddedActivity> activities)
            {
                Activity addedActivity = addedAction.AddedActivity;
                SqlTrackingService.AddedActivity item = new SqlTrackingService.AddedActivity {
                    Order = order
                };
                Type type = addedActivity.GetType();
                item.ActivityTypeFullName = type.FullName;
                item.ActivityTypeAssemblyFullName = type.Assembly.FullName;
                item.QualifiedName = addedActivity.QualifiedName;
                if (addedActivity.Parent != null)
                {
                    item.ParentQualifiedName = addedActivity.Parent.QualifiedName;
                }
                item.AddedActivityActionXoml = this.GetXomlDocument(addedAction);
                activities.Add(item);
                if (addedActivity is CompositeActivity)
                {
                    foreach (Activity activity3 in ((CompositeActivity) addedActivity).Activities)
                    {
                        this.AddAddedActivity(activity3, activities);
                    }
                }
            }

            private void AddRemovedActivity(Activity removed, IList<SqlTrackingService.RemovedActivity> activities)
            {
                SqlTrackingService.RemovedActivity item = new SqlTrackingService.RemovedActivity {
                    Order = -1,
                    QualifiedName = removed.QualifiedName
                };
                if (removed.Parent != null)
                {
                    item.ParentQualifiedName = removed.Parent.QualifiedName;
                }
                activities.Add(item);
                if (removed is CompositeActivity)
                {
                    foreach (Activity activity2 in ((CompositeActivity) removed).Activities)
                    {
                        this.AddRemovedActivity(activity2, activities);
                    }
                }
            }

            private void AddRemovedActivity(RemovedActivityAction removedAction, int order, IList<SqlTrackingService.RemovedActivity> activities)
            {
                Activity originalRemovedActivity = removedAction.OriginalRemovedActivity;
                SqlTrackingService.RemovedActivity item = new SqlTrackingService.RemovedActivity {
                    Order = order,
                    QualifiedName = originalRemovedActivity.QualifiedName
                };
                if (originalRemovedActivity.Parent != null)
                {
                    item.ParentQualifiedName = originalRemovedActivity.Parent.QualifiedName;
                }
                item.RemovedActivityActionXoml = this.GetXomlDocument(removedAction);
                activities.Add(item);
                if (originalRemovedActivity is CompositeActivity)
                {
                    foreach (Activity activity3 in ((CompositeActivity) originalRemovedActivity).Activities)
                    {
                        this.AddRemovedActivity(activity3, activities);
                    }
                }
            }

            private void BatchExecuteInsertEventAnnotation(long internalId, char eventTypeId, IList<KeyValuePair<long, string>> annotations, DbCommand command)
            {
                if ((annotations != null) && (annotations.Count > 0))
                {
                    if (annotations.Count <= _eventAnnotationBatchSize)
                    {
                        this.ExecuteInsertEventAnnotation(internalId, eventTypeId, annotations, command);
                    }
                    else
                    {
                        List<KeyValuePair<long, string>> list = new List<KeyValuePair<long, string>>(_eventAnnotationBatchSize);
                        foreach (KeyValuePair<long, string> pair in annotations)
                        {
                            list.Add(pair);
                            if (list.Count == _eventAnnotationBatchSize)
                            {
                                this.ExecuteInsertEventAnnotation(internalId, eventTypeId, list, command);
                                list.Clear();
                            }
                        }
                        if (list.Count > 0)
                        {
                            this.ExecuteInsertEventAnnotation(internalId, eventTypeId, list, command);
                        }
                    }
                }
            }

            private void BatchExecuteInsertTrackingDataItems(long internalId, char eventTypeId, IList<KeyValuePair<long, TrackingDataItem>> items, DbCommand command)
            {
                if ((items != null) && (items.Count > 0))
                {
                    if (items.Count <= _dataItemBatchSize)
                    {
                        this.ExecuteInsertTrackingDataItems(internalId, eventTypeId, items, command);
                    }
                    else
                    {
                        List<KeyValuePair<long, TrackingDataItem>> list = new List<KeyValuePair<long, TrackingDataItem>>(_dataItemBatchSize);
                        foreach (KeyValuePair<long, TrackingDataItem> pair in items)
                        {
                            list.Add(pair);
                            if (list.Count == _dataItemBatchSize)
                            {
                                this.ExecuteInsertTrackingDataItems(internalId, eventTypeId, list, command);
                                list.Clear();
                            }
                        }
                        if (list.Count > 0)
                        {
                            this.ExecuteInsertTrackingDataItems(internalId, eventTypeId, list, command);
                        }
                    }
                }
            }

            private void BuildInsertActivityStatusEventParameters(long internalId, long activityInstanceId, int parameterId, ActivityTrackingRecord record, DbCommand command)
            {
                string str = parameterId.ToString(CultureInfo.InvariantCulture);
                DbParameter parameter = this.DbResourceAllocator.NewDbParameter("@ActivityInstanceId" + str, DbType.Int64, ParameterDirection.InputOutput);
                command.Parameters.Add(parameter);
                if (activityInstanceId > 0L)
                {
                    parameter.Value = activityInstanceId;
                }
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@QualifiedName" + str, record.QualifiedName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ContextGuid" + str, record.ContextGuid));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ParentContextGuid" + str, record.ParentContextGuid));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ExecutionStatusId" + str, (int) record.ExecutionStatus));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventDateTime" + str, record.EventDateTime));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventOrder" + str, record.EventOrder));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ActivityExecutionStatusEventId" + str, DbType.Int64, ParameterDirection.Output));
            }

            private void BuildInsertUserEventParameters(long internalId, long activityInstanceId, UserTrackingRecord record, DbCommand command)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertUserEvent]";
                command.Parameters.Clear();
                DbParameter parameter = this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", DbType.Int64);
                command.Parameters.Add(parameter);
                parameter.Value = internalId;
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventOrder", record.EventOrder));
                parameter = this.DbResourceAllocator.NewDbParameter("@ActivityInstanceId", DbType.Int64, ParameterDirection.InputOutput);
                command.Parameters.Add(parameter);
                if (activityInstanceId > 0L)
                {
                    parameter.Value = activityInstanceId;
                }
                else
                {
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@QualifiedName", record.QualifiedName));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ContextGuid", record.ContextGuid));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ParentContextGuid", record.ParentContextGuid));
                }
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventDateTime", record.EventDateTime));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@UserDataKey", record.UserDataKey));
                if (record.UserData != null)
                {
                    Type type = record.UserData.GetType();
                    byte[] state = null;
                    bool nonSerializable = false;
                    string stringData = null;
                    if (!(record.UserData is SqlTrackingService.SerializedDataItem))
                    {
                        this.SerializeDataItem(record.UserData, out state, out nonSerializable);
                    }
                    SqlTrackingService.SerializedDataItem userData = record.UserData as SqlTrackingService.SerializedDataItem;
                    state = userData.SerializedData;
                    nonSerializable = userData.NonSerializable;
                    stringData = userData.StringData;
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@UserDataTypeFullName", type.FullName));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@UserDataAssemblyFullName", type.Assembly.FullName));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@UserData_Str", stringData));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@UserData_Blob", state));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@UserDataNonSerializable", nonSerializable));
                }
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@UserEventId", DbType.Int64, ParameterDirection.Output));
            }

            private void BuildInsertWorkflowInstanceEventParameters(long internalId, WorkflowTrackingRecord record1, WorkflowTrackingRecord record2, DbCommand command)
            {
                if (record1 == null)
                {
                    throw new ArgumentNullException("record");
                }
                if (command == null)
                {
                    throw new ArgumentNullException("command");
                }
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertWorkflowInstanceEvent]";
                command.Parameters.Clear();
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TrackingWorkflowEventId1", (int) record1.TrackingWorkflowEvent));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventDateTime1", record1.EventDateTime));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventOrder1", record1.EventOrder));
                if (record1.EventArgs != null)
                {
                    Type type = record1.EventArgs.GetType();
                    byte[] serializedArgs = null;
                    if (!(record1.EventArgs is SqlTrackingService.SerializedEventArgs))
                    {
                        record1 = this.SerializeRecord(record1);
                    }
                    SqlTrackingService.SerializedEventArgs eventArgs = record1.EventArgs as SqlTrackingService.SerializedEventArgs;
                    serializedArgs = eventArgs.SerializedArgs;
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventArgTypeFullName1", type.FullName));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventArgAssemblyFullName1", type.Assembly.FullName));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventArg1", serializedArgs));
                }
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId1", DbType.Int64, ParameterDirection.Output));
                if (record2 != null)
                {
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TrackingWorkflowEventId2", (int) record2.TrackingWorkflowEvent));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventDateTime2", record2.EventDateTime));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventOrder2", record2.EventOrder));
                    if (record2.EventArgs != null)
                    {
                        Type type2 = record2.EventArgs.GetType();
                        byte[] buffer2 = null;
                        if (!(record2.EventArgs is SqlTrackingService.SerializedEventArgs))
                        {
                            record2 = this.SerializeRecord(record2);
                        }
                        SqlTrackingService.SerializedEventArgs args2 = record2.EventArgs as SqlTrackingService.SerializedEventArgs;
                        buffer2 = args2.SerializedArgs;
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventArgTypeFullName2", type2.FullName));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventArgAssemblyFullName2", type2.Assembly.FullName));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventArg2", buffer2));
                    }
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId2", DbType.Int64, ParameterDirection.Output));
                }
            }

            private void BuildInsertWorkflowInstanceParameters(DbCommand command)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertWorkflowInstance]";
                command.Parameters.Clear();
                bool flag = false;
                string str = this._parameters.RootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
                if ((str != null) && (str.Length > 0))
                {
                    flag = true;
                }
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceId", this._parameters.InstanceId));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TypeFullName", flag ? this._parameters.InstanceId.ToString() : this._parameters.WorkflowType.FullName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@AssemblyFullName", flag ? this._parameters.InstanceId.ToString() : this._parameters.WorkflowType.Assembly.FullName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ContextGuid", this._parameters.ContextGuid));
                if (Guid.Empty != this._parameters.CallerInstanceId)
                {
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@CallerInstanceId", this._parameters.CallerInstanceId));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@CallPath", this._callPathKey));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@CallerContextGuid", this._parameters.CallerContextGuid));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@CallerParentContextGuid", this._parameters.CallerParentContextGuid));
                }
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventDateTime", this.GetSqlDateTimeString(DateTime.UtcNow)));
            }

            private string BuildQualifiedNameVarName(string qId, Guid context, Guid parentContext)
            {
                Guid guid = HashHelper.HashServiceType(qId);
                return (guid.ToString().Replace('-', '_') + "_" + context.ToString().Replace('-', '_') + "_" + parentContext.ToString().Replace('-', '_'));
            }

            public void Commit(Transaction transaction, ICollection items)
            {
                if ((items != null) && (items.Count != 0))
                {
                    DbCommand command = null;
                    DbConnection dbConnection = null;
                    bool isNewConnection = false;
                    DbTransaction localTransaction = null;
                    bool flag2 = false;
                    try
                    {
                        dbConnection = this.DbResourceAllocator.GetEnlistedConnection(this.WorkflowCommitWorkBatchService, transaction, out isNewConnection);
                        localTransaction = System.Workflow.Runtime.Hosting.DbResourceAllocator.GetLocalTransaction(this.WorkflowCommitWorkBatchService, transaction);
                        if (localTransaction == null)
                        {
                            localTransaction = dbConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                            flag2 = true;
                        }
                        command = System.Workflow.Runtime.Hosting.DbResourceAllocator.NewCommand(dbConnection);
                        command.Transaction = localTransaction;
                        long internalId = -1L;
                        if (this._internalId <= 0L)
                        {
                            this.ExecuteInsertWorkflowInstance(command);
                            internalId = this._tmpInternalId;
                        }
                        else
                        {
                            internalId = this._internalId;
                        }
                        IList<ActivityTrackingRecord> activities = new List<ActivityTrackingRecord>(5);
                        WorkflowTrackingRecord record = null;
                        foreach (object obj2 in items)
                        {
                            if (obj2 is TrackingRecord)
                            {
                                if (obj2 is ActivityTrackingRecord)
                                {
                                    if (record != null)
                                    {
                                        this.ExecuteInsertWorkflowInstanceEvent(internalId, record, null, command);
                                        record = null;
                                    }
                                    ActivityTrackingRecord item = (ActivityTrackingRecord) obj2;
                                    activities.Add(item);
                                    if (_activityEventBatchSize == activities.Count)
                                    {
                                        this.ExecuteInsertActivityStatusInstance(internalId, activities, command);
                                        activities = new List<ActivityTrackingRecord>(5);
                                    }
                                }
                                else if (obj2 is UserTrackingRecord)
                                {
                                    if (activities.Count > 0)
                                    {
                                        this.ExecuteInsertActivityStatusInstance(internalId, activities, command);
                                        activities.Clear();
                                    }
                                    if (record != null)
                                    {
                                        this.ExecuteInsertWorkflowInstanceEvent(internalId, record, null, command);
                                        record = null;
                                    }
                                    this.ExecuteInsertUserEvent(internalId, (UserTrackingRecord) obj2, command);
                                }
                                else if (obj2 is WorkflowTrackingRecord)
                                {
                                    if (activities.Count > 0)
                                    {
                                        this.ExecuteInsertActivityStatusInstance(internalId, activities, command);
                                        activities.Clear();
                                    }
                                    WorkflowTrackingRecord record3 = (WorkflowTrackingRecord) obj2;
                                    if (TrackingWorkflowEvent.Changed == record3.TrackingWorkflowEvent)
                                    {
                                        if (record != null)
                                        {
                                            this.ExecuteInsertWorkflowInstanceEvent(internalId, record, null, command);
                                            record = null;
                                        }
                                        this.ExecuteInsertWorkflowChange(internalId, record3, command);
                                    }
                                    else if (record != null)
                                    {
                                        this.ExecuteInsertWorkflowInstanceEvent(internalId, record, record3, command);
                                        record = null;
                                    }
                                    else
                                    {
                                        record = record3;
                                    }
                                }
                            }
                        }
                        if (activities.Count > 0)
                        {
                            this.ExecuteInsertActivityStatusInstance(internalId, activities, command);
                        }
                        if (record != null)
                        {
                            this.ExecuteInsertWorkflowInstanceEvent(internalId, record, null, command);
                            record = null;
                        }
                        if (this._completedTerminated)
                        {
                            this.ExecuteSetEndDate(internalId, command);
                        }
                        if (flag2)
                        {
                            localTransaction.Commit();
                        }
                    }
                    catch (DbException exception)
                    {
                        if (flag2)
                        {
                            localTransaction.Rollback();
                        }
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Error writing tracking data to database: " + exception);
                        throw;
                    }
                    finally
                    {
                        if (isNewConnection)
                        {
                            dbConnection.Dispose();
                        }
                    }
                }
            }

            public void Complete(bool succeeded, ICollection items)
            {
                if (!succeeded)
                {
                    this._completedTerminated = false;
                    this._pendingArchive = false;
                    this._tmpInternalId = -1L;
                    this._tmpActivityInstanceId.Clear();
                }
                else
                {
                    if ((-1L == this._internalId) && (this._tmpInternalId > 0L))
                    {
                        this._internalId = this._tmpInternalId;
                    }
                    if ((this._tmpActivityInstanceId != null) && (this._tmpActivityInstanceId.Count > 0))
                    {
                        foreach (string str in this._tmpActivityInstanceId.Keys)
                        {
                            if (!this._activityInstanceId.ContainsKey(str))
                            {
                                this._activityInstanceId.Add(str, this._tmpActivityInstanceId[str]);
                            }
                        }
                        this._tmpActivityInstanceId.Clear();
                    }
                    if (this._pendingArchive)
                    {
                        try
                        {
                            this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.PartitionInstance), null);
                        }
                        catch (Exception exception)
                        {
                            WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, string.Format(CultureInfo.InvariantCulture, "Error partitioning instance {0}: {1}", new object[] { this._parameters.InstanceId, exception.ToString() }));
                        }
                    }
                }
            }

            private void ExecuteInsertActivityStatusInstance(object param)
            {
                ActivityTrackingRecord record = param as ActivityTrackingRecord;
                if (record == null)
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidActivityTrackingRecordParameter, "param");
                }
                DbConnection connection = this.DbResourceAllocator.OpenNewConnection();
                DbTransaction transaction = null;
                try
                {
                    transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    DbCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    IList<ActivityTrackingRecord> activities = new List<ActivityTrackingRecord>(1) {
                        record
                    };
                    this.ExecuteInsertActivityStatusInstance(this._internalId, activities, command);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
                finally
                {
                    if ((connection != null) && (connection.State != ConnectionState.Closed))
                    {
                        connection.Close();
                    }
                }
            }

            private void ExecuteInsertActivityStatusInstance(long internalId, IList<ActivityTrackingRecord> activities, DbCommand command)
            {
                if ((activities != null) && (activities.Count > 0))
                {
                    if (activities.Count > _activityEventBatchSize)
                    {
                        throw new ArgumentOutOfRangeException("activities");
                    }
                    if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                    {
                        throw new ArgumentException();
                    }
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[InsertActivityExecutionStatusEventMultiple]";
                    command.Parameters.Clear();
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceId", this._parameters.InstanceId));
                    DbParameter parameter = this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", DbType.Int64, ParameterDirection.InputOutput);
                    command.Parameters.Add(parameter);
                    if (internalId > 0L)
                    {
                        parameter.Value = internalId;
                    }
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceContextGuid", this._parameters.ContextGuid));
                    string[] strArray = new string[5];
                    for (int i = 0; i < activities.Count; i++)
                    {
                        ActivityTrackingRecord record = activities[i];
                        long id = -1L;
                        strArray[i] = this.BuildQualifiedNameVarName(record.QualifiedName, record.ContextGuid, record.ParentContextGuid);
                        this.TryGetActivityInstanceId(strArray[i], out id);
                        this.BuildInsertActivityStatusEventParameters(internalId, id, i + 1, record, command);
                    }
                    command.ExecuteNonQuery();
                    long[] numArray = new long[] { -1L, -1L, -1L, -1L, -1L };
                    for (int j = 0; j < activities.Count; j++)
                    {
                        string str = (j + 1).ToString(CultureInfo.InvariantCulture);
                        long num4 = (long) command.Parameters["@ActivityInstanceId" + str].Value;
                        if (ActivityExecutionStatus.Closed != activities[j].ExecutionStatus)
                        {
                            this.SetActivityInstanceId(strArray[j], num4);
                        }
                        else
                        {
                            this.RemoveActivityInstanceId(strArray[j]);
                        }
                        numArray[j] = (long) command.Parameters["@ActivityExecutionStatusEventId" + str].Value;
                    }
                    List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(10);
                    List<KeyValuePair<long, TrackingDataItem>> items = new List<KeyValuePair<long, TrackingDataItem>>(10);
                    for (int k = 0; k < activities.Count; k++)
                    {
                        ActivityTrackingRecord record2 = activities[k];
                        long key = numArray[k];
                        if (key <= 0L)
                        {
                            throw new InvalidOperationException();
                        }
                        foreach (string str2 in record2.Annotations)
                        {
                            annotations.Add(new KeyValuePair<long, string>(key, str2));
                        }
                        foreach (TrackingDataItem item in record2.Body)
                        {
                            items.Add(new KeyValuePair<long, TrackingDataItem>(key, item));
                        }
                    }
                    this.BatchExecuteInsertEventAnnotation(internalId, 'a', annotations, command);
                    this.BatchExecuteInsertTrackingDataItems(internalId, 'a', items, command);
                }
            }

            private void ExecuteInsertAddedActivity(long internalId, string qualifiedName, string parentQualifiedName, string typeFullName, string assemblyFullName, string addedActivityActionXoml, long eventId, int order, DbCommand command)
            {
                if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                {
                    throw new ArgumentNullException("command");
                }
                command.Parameters.Clear();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertAddedActivity]";
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId", eventId));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@QualifiedName", qualifiedName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TypeFullName", typeFullName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@AssemblyFullName", assemblyFullName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ParentQualifiedName", parentQualifiedName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@AddedActivityAction", addedActivityActionXoml));
                if (-1 == order)
                {
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Order", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Order", order));
                }
                command.ExecuteNonQuery();
            }

            private void ExecuteInsertAnnotation(long internalId, IList<KeyValuePair<long, string>> annotations, DbCommand command)
            {
                if ((annotations != null) && (annotations.Count > 0))
                {
                    if (annotations.Count > _dataItemAnnotationBatchSize)
                    {
                        throw new ArgumentOutOfRangeException("annotations");
                    }
                    if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                    {
                        throw new ArgumentNullException("command");
                    }
                    command.Parameters.Clear();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[InsertTrackingDataItemAnnotationMultiple]";
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                    int num = 1;
                    foreach (KeyValuePair<long, string> pair in annotations)
                    {
                        string str = num++.ToString(CultureInfo.InvariantCulture);
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@HasData" + str, true));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TrackingDataItemId" + str, pair.Key));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Annotation" + str, pair.Value));
                    }
                    command.ExecuteNonQuery();
                }
            }

            private void ExecuteInsertEventAnnotation(long internalId, char eventTypeId, IList<KeyValuePair<long, string>> annotations, DbCommand command)
            {
                if ((annotations != null) && (annotations.Count > 0))
                {
                    if (annotations.Count > _eventAnnotationBatchSize)
                    {
                        throw new ArgumentOutOfRangeException("annotations");
                    }
                    if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                    {
                        throw new ArgumentNullException("command");
                    }
                    command.Parameters.Clear();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[InsertEventAnnotationMultiple]";
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventTypeId", eventTypeId));
                    int num = 1;
                    foreach (KeyValuePair<long, string> pair in annotations)
                    {
                        string str = num++.ToString(CultureInfo.InvariantCulture);
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@HasData" + str, true));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventId" + str, pair.Key));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Annotation" + str, pair.Value));
                    }
                    command.ExecuteNonQuery();
                }
            }

            private void ExecuteInsertRemovedActivity(long internalId, string qualifiedName, string parentQualifiedName, string removedActivityActionXoml, long eventId, int order, DbCommand command)
            {
                if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                {
                    throw new ArgumentNullException("command");
                }
                command.Parameters.Clear();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertRemovedActivity]";
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId", eventId));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@QualifiedName", qualifiedName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@ParentQualifiedName", parentQualifiedName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@RemovedActivityAction", removedActivityActionXoml));
                if (-1 == order)
                {
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Order", DBNull.Value));
                }
                else
                {
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Order", order));
                }
                command.ExecuteNonQuery();
            }

            private void ExecuteInsertTrackingDataItems(long internalId, char eventTypeId, IList<KeyValuePair<long, TrackingDataItem>> items, DbCommand command)
            {
                if ((items != null) && (items.Count > 0))
                {
                    if (items.Count > _dataItemAnnotationBatchSize)
                    {
                        throw new ArgumentOutOfRangeException("items");
                    }
                    if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                    {
                        throw new ArgumentException();
                    }
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[dbo].[InsertTrackingDataItemMultiple]";
                    command.Parameters.Clear();
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventTypeId", eventTypeId));
                    int index = 1;
                    foreach (KeyValuePair<long, TrackingDataItem> pair in items)
                    {
                        string str = index++.ToString(CultureInfo.InvariantCulture);
                        SqlTrackingService.SerializedDataItem item = pair.Value as SqlTrackingService.SerializedDataItem;
                        if (item == null)
                        {
                            item = this.SerializeDataItem(pair.Value);
                        }
                        Type type = item.Type;
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EventId" + str, pair.Key));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@FieldName" + str, item.FieldName));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TypeFullName" + str, (null == type) ? null : type.FullName));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@AssemblyFullName" + str, (null == type) ? null : type.Assembly.FullName));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Data_Str" + str, item.StringData));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Data_Blob" + str, item.SerializedData));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@DataNonSerializable" + str, item.NonSerializable));
                        command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TrackingDataItemId" + str, DbType.Int64, ParameterDirection.Output));
                    }
                    command.ExecuteNonQuery();
                    List<long> list = new List<long>(_dataItemAnnotationBatchSize);
                    for (index = 0; index < items.Count; index++)
                    {
                        string str2 = (index + 1).ToString(CultureInfo.InvariantCulture);
                        list.Insert(index, (long) command.Parameters["@TrackingDataItemId" + str2].Value);
                    }
                    List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(_dataItemAnnotationBatchSize);
                    index = 0;
                    foreach (KeyValuePair<long, TrackingDataItem> pair2 in items)
                    {
                        TrackingDataItem item2 = pair2.Value;
                        long key = list[index++];
                        foreach (string str3 in item2.Annotations)
                        {
                            annotations.Add(new KeyValuePair<long, string>(key, str3));
                            if (annotations.Count == _dataItemAnnotationBatchSize)
                            {
                                this.ExecuteInsertAnnotation(internalId, annotations, command);
                                annotations.Clear();
                            }
                        }
                    }
                    if (annotations.Count > 0)
                    {
                        this.ExecuteInsertAnnotation(internalId, annotations, command);
                    }
                }
            }

            private void ExecuteInsertUserEvent(object param)
            {
                UserTrackingRecord record = param as UserTrackingRecord;
                if (record == null)
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidUserTrackingRecordParameter, "param");
                }
                DbConnection connection = this.DbResourceAllocator.OpenNewConnection();
                DbTransaction transaction = null;
                try
                {
                    transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    DbCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    this.ExecuteInsertUserEvent(this._internalId, record, command);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
                finally
                {
                    if ((connection != null) && (connection.State != ConnectionState.Closed))
                    {
                        connection.Close();
                    }
                }
            }

            private void ExecuteInsertUserEvent(long internalId, UserTrackingRecord record, DbCommand command)
            {
                if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                {
                    throw new ArgumentException();
                }
                long id = -1L;
                bool flag = false;
                string key = this.BuildQualifiedNameVarName(record.QualifiedName, record.ContextGuid, record.ParentContextGuid);
                if (this.TryGetActivityInstanceId(key, out id))
                {
                    flag = true;
                }
                this.BuildInsertUserEventParameters(internalId, id, record, command);
                command.ExecuteNonQuery();
                if (!flag)
                {
                    this.SetActivityInstanceId(key, (long) command.Parameters["@ActivityInstanceId"].Value);
                }
                long num2 = (long) command.Parameters["@UserEventId"].Value;
                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(10);
                List<KeyValuePair<long, TrackingDataItem>> items = new List<KeyValuePair<long, TrackingDataItem>>(10);
                foreach (string str2 in record.Annotations)
                {
                    annotations.Add(new KeyValuePair<long, string>(num2, str2));
                }
                foreach (TrackingDataItem item in record.Body)
                {
                    items.Add(new KeyValuePair<long, TrackingDataItem>(num2, item));
                }
                this.BatchExecuteInsertEventAnnotation(internalId, 'u', annotations, command);
                this.BatchExecuteInsertTrackingDataItems(internalId, 'u', items, command);
            }

            private void ExecuteInsertWorkflowChange(object param)
            {
                WorkflowTrackingRecord record = param as WorkflowTrackingRecord;
                if (record == null)
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidWorkflowTrackingRecordParameter, "param");
                }
                DbCommand command = this.DbResourceAllocator.NewCommand();
                try
                {
                    if (ConnectionState.Open != command.Connection.State)
                    {
                        command.Connection.Open();
                    }
                    command.Transaction = command.Connection.BeginTransaction();
                    this.ExecuteInsertWorkflowChange(this._internalId, record, command);
                    command.Transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        if ((command != null) && (command.Transaction != null))
                        {
                            command.Transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
                finally
                {
                    if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                    {
                        command.Connection.Close();
                    }
                }
            }

            private void ExecuteInsertWorkflowChange(long internalId, WorkflowTrackingRecord record, DbCommand command)
            {
                if (record == null)
                {
                    throw new ArgumentNullException("record");
                }
                if (record.EventArgs == null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.InvalidWorkflowChangeArgs);
                }
                if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                {
                    throw new ArgumentNullException("command");
                }
                if (!(record.EventArgs is SqlTrackingService.SerializedWorkflowChangedEventArgs))
                {
                    record = this.SerializeRecord(record);
                }
                this.BuildInsertWorkflowInstanceEventParameters(internalId, record, null, command);
                command.ExecuteNonQuery();
                long eventId = (long) command.Parameters["@WorkflowInstanceEventId1"].Value;
                SqlTrackingService.SerializedWorkflowChangedEventArgs eventArgs = (SqlTrackingService.SerializedWorkflowChangedEventArgs) record.EventArgs;
                if ((eventArgs.AddedActivities != null) && (eventArgs.AddedActivities.Count > 0))
                {
                    foreach (SqlTrackingService.AddedActivity activity in eventArgs.AddedActivities)
                    {
                        this.ExecuteInsertAddedActivity(internalId, activity.QualifiedName, activity.ParentQualifiedName, activity.ActivityTypeFullName, activity.ActivityTypeAssemblyFullName, activity.AddedActivityActionXoml, eventId, activity.Order, command);
                    }
                }
                if ((eventArgs.RemovedActivities != null) && (eventArgs.RemovedActivities.Count > 0))
                {
                    foreach (SqlTrackingService.RemovedActivity activity2 in eventArgs.RemovedActivities)
                    {
                        this.ExecuteInsertRemovedActivity(internalId, activity2.QualifiedName, activity2.ParentQualifiedName, activity2.RemovedActivityActionXoml, eventId, activity2.Order, command);
                    }
                }
                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(record.Annotations.Count);
                foreach (string str in record.Annotations)
                {
                    annotations.Add(new KeyValuePair<long, string>(eventId, str));
                }
                this.BatchExecuteInsertEventAnnotation(internalId, 'w', annotations, command);
            }

            private long ExecuteInsertWorkflowInstance(DbCommand command)
            {
                long num;
                if (command == null)
                {
                    throw new ArgumentNullException("command");
                }
                if ((command.Connection == null) || (ConnectionState.Open != command.Connection.State))
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidCommandBadConnection, "command");
                }
                string str = this._parameters.RootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
                if ((str != null) && (str.Length > 0))
                {
                    this.InsertWorkflow(command, this._parameters.InstanceId, null, this._parameters.RootActivity);
                }
                else
                {
                    this.InsertWorkflow(command, this._parameters.InstanceId, this._parameters.WorkflowType, this._parameters.RootActivity);
                }
                this.BuildInsertWorkflowInstanceParameters(command);
                DbDataReader reader = null;
                try
                {
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        this._tmpInternalId = reader.GetInt64(0);
                    }
                    num = this._tmpInternalId;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                return num;
            }

            private void ExecuteInsertWorkflowInstance(object param)
            {
                DbConnection dbConnection = this.DbResourceAllocator.OpenNewConnection();
                DbCommand command = System.Workflow.Runtime.Hosting.DbResourceAllocator.NewCommand(dbConnection);
                DbTransaction transaction = null;
                try
                {
                    transaction = dbConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    command.Connection = dbConnection;
                    command.Transaction = transaction;
                    this._internalId = this.ExecuteInsertWorkflowInstance(command);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
                finally
                {
                    if ((dbConnection != null) && (dbConnection.State != ConnectionState.Closed))
                    {
                        dbConnection.Close();
                    }
                }
            }

            private void ExecuteInsertWorkflowInstanceEvent(object param)
            {
                WorkflowTrackingRecord record = param as WorkflowTrackingRecord;
                if (record == null)
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidWorkflowTrackingRecordParameter, "param");
                }
                DbConnection dbConnection = this.DbResourceAllocator.OpenNewConnection();
                DbCommand command = System.Workflow.Runtime.Hosting.DbResourceAllocator.NewCommand(dbConnection);
                DbTransaction transaction = null;
                try
                {
                    transaction = dbConnection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    command.Connection = dbConnection;
                    command.Transaction = transaction;
                    this.ExecuteInsertWorkflowInstanceEvent(this._internalId, record, null, command);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    throw;
                }
                finally
                {
                    if ((dbConnection != null) && (dbConnection.State != ConnectionState.Closed))
                    {
                        dbConnection.Close();
                    }
                }
            }

            private void ExecuteInsertWorkflowInstanceEvent(long internalId, WorkflowTrackingRecord record1, WorkflowTrackingRecord record2, DbCommand command)
            {
                if (((command == null) || (command.Connection == null)) || (ConnectionState.Open != command.Connection.State))
                {
                    throw new ArgumentException();
                }
                this.BuildInsertWorkflowInstanceEventParameters(internalId, record1, record2, command);
                command.ExecuteNonQuery();
                long key = (long) command.Parameters["@WorkflowInstanceEventId1"].Value;
                long num2 = -1L;
                if (record2 != null)
                {
                    num2 = (long) command.Parameters["@WorkflowInstanceEventId2"].Value;
                }
                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(record1.Annotations.Count + ((record2 == null) ? 0 : record2.Annotations.Count));
                foreach (string str in record1.Annotations)
                {
                    annotations.Add(new KeyValuePair<long, string>(key, str));
                }
                if (record2 != null)
                {
                    foreach (string str2 in record2.Annotations)
                    {
                        annotations.Add(new KeyValuePair<long, string>(num2, str2));
                    }
                }
                this.BatchExecuteInsertEventAnnotation(internalId, 'w', annotations, command);
            }

            private void ExecuteSetEndDate(object param)
            {
                DbCommand command = null;
                try
                {
                    command = System.Workflow.Runtime.Hosting.DbResourceAllocator.NewCommand(this.DbResourceAllocator.OpenNewConnection(false));
                    this.ExecuteSetEndDate(this._internalId, command);
                }
                finally
                {
                    if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                    {
                        command.Connection.Close();
                    }
                }
            }

            private void ExecuteSetEndDate(long internalId, DbCommand command)
            {
                if (command == null)
                {
                    throw new ArgumentNullException("command");
                }
                command.Parameters.Clear();
                command.CommandText = "[dbo].[SetWorkflowInstanceEndDateTime]";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@EndDateTime", DateTime.UtcNow));
                command.ExecuteNonQuery();
            }

            private string GetActivitiesXml(CompositeActivity root)
            {
                if (root == null)
                {
                    return null;
                }
                StringBuilder output = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(output);
                try
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Activities");
                    this.WriteActivity(root, writer);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                finally
                {
                    writer.Flush();
                    writer.Close();
                }
                return output.ToString();
            }

            private IList<Activity> GetAllEnabledActivities(CompositeActivity compositeActivity)
            {
                if (compositeActivity == null)
                {
                    throw new ArgumentNullException("compositeActivity");
                }
                List<Activity> list = new List<Activity>(compositeActivity.EnabledActivities);
                foreach (Activity activity in ((ISupportAlternateFlow) compositeActivity).AlternateFlowActivities)
                {
                    if (!list.Contains(activity))
                    {
                        list.Add(activity);
                    }
                }
                return list;
            }

            private void GetCallPathKeys(IList<string> callPath)
            {
                if ((callPath != null) && (callPath.Count > 0))
                {
                    for (int i = 0; i < callPath.Count; i++)
                    {
                        this._callPathKey = this._callPathKey + "." + callPath[i];
                        if (i < (callPath.Count - 1))
                        {
                            this._parentCallPathKey = this._parentCallPathKey + "." + callPath[i];
                        }
                    }
                    if (this._callPathKey != null)
                    {
                        this._callPathKey = this.SqlEscape(this._callPathKey.Substring(1));
                    }
                    if (this._parentCallPathKey != null)
                    {
                        this._parentCallPathKey = this.SqlEscape(this._parentCallPathKey.Substring(1));
                    }
                }
            }

            private string GetSqlDateTimeString(DateTime dateTime)
            {
                return (dateTime.Year.ToString(CultureInfo.InvariantCulture) + this.PadToDblDigit(dateTime.Month) + this.PadToDblDigit(dateTime.Day) + " " + dateTime.Hour.ToString(CultureInfo.InvariantCulture) + ":" + dateTime.Minute.ToString(CultureInfo.InvariantCulture) + ":" + dateTime.Second.ToString(CultureInfo.InvariantCulture) + ":" + dateTime.Millisecond.ToString(CultureInfo.InvariantCulture));
            }

            internal string GetXomlDocument(object obj)
            {
                string str = null;
                using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    using (XmlWriter writer2 = SqlTrackingService.CreateXmlWriter(writer))
                    {
                        new WorkflowMarkupSerializer().Serialize(writer2, obj);
                        str = writer.ToString();
                    }
                }
                return str;
            }

            private void InsertWorkflow(DbCommand command, Guid workflowInstanceId, Type workflowType, Activity rootActivity)
            {
                string xomlDocument = null;
                if (null != workflowType)
                {
                    lock (this._service._typeCacheLock)
                    {
                        if (this._service._types.Contains(workflowType.AssemblyQualifiedName))
                        {
                            return;
                        }
                        xomlDocument = this.GetXomlDocument(rootActivity);
                        goto Label_0082;
                    }
                }
                lock (this._service._typeCacheLock)
                {
                    xomlDocument = this.GetXomlDocument(rootActivity);
                }
            Label_0082:
                command.Parameters.Clear();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertWorkflow]";
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@TypeFullName", (null == workflowType) ? workflowInstanceId.ToString() : workflowType.FullName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@AssemblyFullName", (null == workflowType) ? workflowInstanceId.ToString() : workflowType.Assembly.FullName));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@IsInstanceType", null == workflowType));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowDefinition", xomlDocument));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowId", DbType.Int32, ParameterDirection.Output));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Exists", DbType.Boolean, ParameterDirection.Output));
                command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@Activities", this.GetActivitiesXml((CompositeActivity) rootActivity)));
                command.ExecuteNonQuery();
                if (null != workflowType)
                {
                    lock (this._service._typeCacheLock)
                    {
                        if (!this._service._types.Contains(workflowType.AssemblyQualifiedName))
                        {
                            this._service._types.Add(workflowType);
                        }
                    }
                }
            }

            protected internal override void InstanceCompletedOrTerminated()
            {
                if (this._isTrans)
                {
                    this._completedTerminated = true;
                    if (this._service.PartitionOnCompletion)
                    {
                        this._pendingArchive = true;
                    }
                }
                else
                {
                    this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.ExecuteSetEndDate), null);
                    if (this._service.PartitionOnCompletion)
                    {
                        this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.PartitionInstance), null);
                    }
                }
            }

            public bool MustCommit(ICollection items)
            {
                return false;
            }

            private string PadToDblDigit(int num)
            {
                string str = num.ToString(CultureInfo.InvariantCulture);
                if (str.Length == 1)
                {
                    return ("0" + str);
                }
                return str;
            }

            private void PartitionInstance(object param)
            {
                DbCommand command = null;
                try
                {
                    command = System.Workflow.Runtime.Hosting.DbResourceAllocator.NewCommand(this.DbResourceAllocator.OpenNewConnection(false));
                    command.CommandText = "[dbo].[PartitionWorkflowInstance]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(this.DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", this._internalId));
                    command.ExecuteNonQuery();
                }
                finally
                {
                    if (((command != null) && (command.Connection != null)) && (command.Connection.State != ConnectionState.Closed))
                    {
                        command.Connection.Close();
                    }
                }
            }

            private void RemoveActivityInstanceId(string key)
            {
                if (this._isTrans && this._tmpActivityInstanceId.ContainsKey(key))
                {
                    this._tmpActivityInstanceId.Remove(key);
                }
                if (this._activityInstanceId.ContainsKey(key))
                {
                    this._activityInstanceId.Remove(key);
                }
            }

            protected internal override void Send(TrackingRecord record)
            {
                if ((Guid.Empty == this._parameters.InstanceId) || (record == null))
                {
                    throw new ArgumentException(ExecutionStringManager.MissingParametersTrack);
                }
                if (record is ActivityTrackingRecord)
                {
                    ActivityTrackingRecord record2 = record as ActivityTrackingRecord;
                    if (this._isTrans)
                    {
                        WorkflowEnvironment.WorkBatch.Add(this, this.SerializeRecord(record2));
                    }
                    else
                    {
                        this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.ExecuteInsertActivityStatusInstance), this.SerializeRecord(record2));
                    }
                }
                else if (record is WorkflowTrackingRecord)
                {
                    WorkflowTrackingRecord record3 = (WorkflowTrackingRecord) record;
                    if (this._isTrans)
                    {
                        WorkflowEnvironment.WorkBatch.Add(this, this.SerializeRecord(record3));
                    }
                    else if (TrackingWorkflowEvent.Changed == record3.TrackingWorkflowEvent)
                    {
                        this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.ExecuteInsertWorkflowChange), this.SerializeRecord(record3));
                    }
                    else
                    {
                        this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.ExecuteInsertWorkflowInstanceEvent), this.SerializeRecord(record3));
                    }
                }
                else if (record is UserTrackingRecord)
                {
                    UserTrackingRecord record4 = (UserTrackingRecord) record;
                    if (this._isTrans)
                    {
                        WorkflowEnvironment.WorkBatch.Add(this, this.SerializeRecord(record4));
                    }
                    else
                    {
                        this._service.ExecuteRetried(new SqlTrackingService.ExecuteRetriedDelegate(this.ExecuteInsertUserEvent), this.SerializeRecord(record4));
                    }
                }
            }

            private SqlTrackingService.SerializedDataItem SerializeDataItem(TrackingDataItem item)
            {
                if (item == null)
                {
                    return null;
                }
                SqlTrackingService.SerializedDataItem item2 = new SqlTrackingService.SerializedDataItem {
                    Data = item.Data
                };
                item2.Annotations.AddRange(item.Annotations);
                item2.FieldName = item.FieldName;
                if (item.Data != null)
                {
                    bool flag;
                    byte[] state = null;
                    this.SerializeDataItem(item.Data, out state, out flag);
                    item2.SerializedData = state;
                    item2.StringData = item.Data.ToString();
                    item2.Type = item.Data.GetType();
                    item2.NonSerializable = flag;
                }
                return item2;
            }

            private void SerializeDataItem(object data, out byte[] state, out bool nonSerializable)
            {
                nonSerializable = false;
                state = null;
                if (data != null)
                {
                    MemoryStream serializationStream = new MemoryStream(0x400);
                    BinaryFormatter formatter = new BinaryFormatter();
                    try
                    {
                        formatter.Serialize(serializationStream, data);
                        state = new byte[serializationStream.Length];
                        serializationStream.Position = 0L;
                        if (serializationStream.Length <= 0x7fffffffL)
                        {
                            int num = 0;
                            int offset = 0;
                            int count = 0;
                            do
                            {
                                offset += num;
                                count = ((int) serializationStream.Length) - offset;
                                num = serializationStream.Read(state, offset, count);
                            }
                            while (num > 0);
                        }
                    }
                    catch (SerializationException)
                    {
                        nonSerializable = true;
                    }
                    finally
                    {
                        serializationStream.Close();
                    }
                }
            }

            private ActivityTrackingRecord SerializeRecord(ActivityTrackingRecord record)
            {
                if ((record.Body != null) && (record.Body.Count != 0))
                {
                    for (int i = 0; i < record.Body.Count; i++)
                    {
                        record.Body[i] = this.SerializeDataItem(record.Body[i]);
                    }
                }
                return record;
            }

            private UserTrackingRecord SerializeRecord(UserTrackingRecord record)
            {
                if (((record.Body != null) && (record.Body.Count != 0)) || ((record.EventArgs != null) || (record.UserData != null)))
                {
                    if (record.UserData != null)
                    {
                        bool flag;
                        SqlTrackingService.SerializedDataItem item = new SqlTrackingService.SerializedDataItem();
                        byte[] state = null;
                        this.SerializeDataItem(record.UserData, out state, out flag);
                        item.Type = record.UserData.GetType();
                        item.StringData = record.UserData.ToString();
                        item.SerializedData = state;
                        item.NonSerializable = flag;
                        record.UserData = item;
                    }
                    for (int i = 0; i < record.Body.Count; i++)
                    {
                        record.Body[i] = this.SerializeDataItem(record.Body[i]);
                    }
                }
                return record;
            }

            private WorkflowTrackingRecord SerializeRecord(WorkflowTrackingRecord record)
            {
                if (record.EventArgs != null)
                {
                    SqlTrackingService.SerializedEventArgs args;
                    if (TrackingWorkflowEvent.Changed == record.TrackingWorkflowEvent)
                    {
                        SqlTrackingService.SerializedWorkflowChangedEventArgs args2 = new SqlTrackingService.SerializedWorkflowChangedEventArgs();
                        TrackingWorkflowChangedEventArgs eventArgs = (TrackingWorkflowChangedEventArgs) record.EventArgs;
                        if (eventArgs != null)
                        {
                            for (int i = 0; i < eventArgs.Changes.Count; i++)
                            {
                                WorkflowChangeAction action = eventArgs.Changes[i];
                                if (action is RemovedActivityAction)
                                {
                                    this.AddRemovedActivity((RemovedActivityAction) action, i, args2.RemovedActivities);
                                }
                                else if (action is AddedActivityAction)
                                {
                                    this.AddAddedActivity((AddedActivityAction) action, i, args2.AddedActivities);
                                }
                            }
                        }
                        args = args2;
                    }
                    else
                    {
                        bool flag;
                        args = new SqlTrackingService.SerializedEventArgs();
                        byte[] state = null;
                        this.SerializeDataItem(record.EventArgs, out state, out flag);
                        args.SerializedArgs = state;
                        if (flag)
                        {
                            Exception exception;
                            switch (record.TrackingWorkflowEvent)
                            {
                                case TrackingWorkflowEvent.Exception:
                                    exception = ((TrackingWorkflowExceptionEventArgs) record.EventArgs).Exception;
                                    if (exception != null)
                                    {
                                        this.SerializeDataItem(exception.ToString(), out state, out flag);
                                        args.SerializedArgs = state;
                                    }
                                    break;

                                case TrackingWorkflowEvent.Terminated:
                                    exception = ((TrackingWorkflowTerminatedEventArgs) record.EventArgs).Exception;
                                    if (exception != null)
                                    {
                                        this.SerializeDataItem(exception.ToString(), out state, out flag);
                                        args.SerializedArgs = state;
                                    }
                                    break;
                            }
                        }
                    }
                    args.Type = record.EventArgs.GetType();
                    record.EventArgs = args;
                }
                return record;
            }

            private void SetActivityInstanceId(string key, long id)
            {
                if (this._isTrans)
                {
                    if (!this._tmpActivityInstanceId.ContainsKey(key))
                    {
                        this._tmpActivityInstanceId.Add(key, id);
                    }
                }
                else if (!this._activityInstanceId.ContainsKey(key))
                {
                    this._activityInstanceId.Add(key, id);
                }
            }

            private string SqlEscape(string val)
            {
                if (val == null)
                {
                    return null;
                }
                return val.Replace("'", "''");
            }

            private bool TryGetActivityInstanceId(string key, out long id)
            {
                return (this._activityInstanceId.TryGetValue(key, out id) || (this._isTrans && this._tmpActivityInstanceId.TryGetValue(key, out id)));
            }

            private void WriteActivity(Activity activity, XmlWriter writer)
            {
                if (activity != null)
                {
                    if (writer == null)
                    {
                        throw new ArgumentNullException("writer");
                    }
                    Type type = activity.GetType();
                    writer.WriteStartElement("Activity");
                    writer.WriteElementString("TypeFullName", type.FullName);
                    writer.WriteElementString("AssemblyFullName", type.Assembly.FullName);
                    writer.WriteElementString("QualifiedName", activity.QualifiedName);
                    if (activity.Parent != null)
                    {
                        writer.WriteElementString("ParentQualifiedName", activity.Parent.QualifiedName);
                    }
                    writer.WriteEndElement();
                    if (activity is CompositeActivity)
                    {
                        foreach (Activity activity2 in this.GetAllEnabledActivities((CompositeActivity) activity))
                        {
                            this.WriteActivity(activity2, writer);
                        }
                    }
                }
            }

            private System.Workflow.Runtime.Hosting.DbResourceAllocator DbResourceAllocator
            {
                get
                {
                    return this._service.DbResourceAllocator;
                }
            }

            private System.Workflow.Runtime.Hosting.WorkflowCommitWorkBatchService WorkflowCommitWorkBatchService
            {
                get
                {
                    return this._service._transactionService;
                }
            }
        }

        private class TypeKeyedCollection : KeyedCollection<string, Type>
        {
            protected override string GetKeyForItem(Type item)
            {
                return item.AssemblyQualifiedName;
            }
        }
    }
}


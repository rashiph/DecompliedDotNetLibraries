namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    public sealed class EtwTrackingParticipant : TrackingParticipant
    {
        private DiagnosticTrace diagnosticTrace;
        private static Hashtable diagnosticTraceCache = new Hashtable();
        private const string emptyItemsTag = "<items />";
        private Guid etwProviderId;
        private const string itemsTag = "items";
        private const string itemTag = "item";
        private const string nameAttribute = "name";
        private const string truncatedItemsTag = "<items>...</items>";
        private const string typeAttribute = "type";
        private NetDataContractSerializer variableSerializer;

        public EtwTrackingParticipant()
        {
            this.EtwProviderId = DiagnosticTrace.DefaultEtwProviderId;
            this.ApplicationReference = string.Empty;
        }

        protected internal override IAsyncResult BeginTrack(TrackingRecord record, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.Track(record, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected internal override void EndTrack(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        private void InitializeEtwTrackingProvider(Guid providerId)
        {
            this.diagnosticTrace = (DiagnosticTrace) diagnosticTraceCache[providerId];
            if (this.diagnosticTrace == null)
            {
                lock (diagnosticTraceCache)
                {
                    this.diagnosticTrace = (DiagnosticTrace) diagnosticTraceCache[providerId];
                    if (this.diagnosticTrace == null)
                    {
                        this.diagnosticTrace = new DiagnosticTrace(null, providerId);
                        diagnosticTraceCache.Add(providerId, this.diagnosticTrace);
                    }
                }
            }
            this.etwProviderId = providerId;
        }

        private static string PrepareAnnotations(IDictionary<string, string> data)
        {
            string fullName = typeof(string).FullName;
            StringBuilder output = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings {
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(output, settings))
            {
                writer.WriteStartElement("items");
                if (data != null)
                {
                    foreach (KeyValuePair<string, string> pair in data)
                    {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("name", pair.Key);
                        writer.WriteAttributeString("type", fullName);
                        if (pair.Value == null)
                        {
                            writer.WriteValue(string.Empty);
                        }
                        else
                        {
                            writer.WriteValue(pair.Value);
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.Flush();
                return output.ToString();
            }
        }

        private string PrepareDictionary(IDictionary<string, object> data)
        {
            StringBuilder output = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings {
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(output, settings))
            {
                writer.WriteStartElement("items");
                if (data != null)
                {
                    foreach (KeyValuePair<string, object> pair in data)
                    {
                        writer.WriteStartElement("item");
                        writer.WriteAttributeString("name", pair.Key);
                        if (pair.Value == null)
                        {
                            writer.WriteAttributeString("type", string.Empty);
                            writer.WriteValue(string.Empty);
                        }
                        else
                        {
                            Type type = pair.Value.GetType();
                            writer.WriteAttributeString("type", type.FullName);
                            if ((((type == typeof(int)) || (type == typeof(float))) || ((type == typeof(double)) || (type == typeof(long)))) || ((((type == typeof(bool)) || (type == typeof(uint))) || ((type == typeof(ushort)) || (type == typeof(short)))) || (((type == typeof(ulong)) || (type == typeof(string))) || (type == typeof(DateTimeOffset)))))
                            {
                                writer.WriteValue(pair.Value);
                            }
                            else if (type == typeof(Guid))
                            {
                                writer.WriteValue(((Guid) pair.Value).ToString());
                            }
                            else if (type == typeof(DateTime))
                            {
                                DateTime time = ((DateTime) pair.Value).ToUniversalTime();
                                writer.WriteValue(time);
                            }
                            else
                            {
                                if (this.variableSerializer == null)
                                {
                                    this.variableSerializer = new NetDataContractSerializer();
                                }
                                try
                                {
                                    this.variableSerializer.WriteObject(writer, pair.Value);
                                }
                                catch (Exception exception)
                                {
                                    if (Fx.IsFatal(exception))
                                    {
                                        throw;
                                    }
                                    TraceItemNotSerializable(pair.Key, exception);
                                }
                            }
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.Flush();
                return output.ToString();
            }
        }

        private static void TraceItemNotSerializable(string item, Exception e)
        {
            FxTrace.Exception.AsInformation(e);
            if (TD.TrackingValueNotSerializableIsEnabled())
            {
                TD.TrackingValueNotSerializable(item);
            }
        }

        private void TraceTrackingRecordDropped(long recordNumber)
        {
            if (TD.TrackingRecordDroppedIsEnabled())
            {
                TD.TrackingRecordDropped(recordNumber, this.EtwProviderId);
            }
        }

        private void TraceTrackingRecordTruncated(long recordNumber)
        {
            if (TD.TrackingRecordTruncatedIsEnabled())
            {
                TD.TrackingRecordTruncated(recordNumber, this.EtwProviderId);
            }
        }

        protected internal override void Track(TrackingRecord record, TimeSpan timeout)
        {
            if (this.diagnosticTrace.IsEtwProviderEnabled)
            {
                if (record is ActivityStateRecord)
                {
                    this.TrackActivityRecord((ActivityStateRecord) record);
                }
                else if (record is WorkflowInstanceRecord)
                {
                    this.TrackWorkflowRecord((WorkflowInstanceRecord) record);
                }
                else if (record is BookmarkResumptionRecord)
                {
                    this.TrackBookmarkRecord((BookmarkResumptionRecord) record);
                }
                else if (record is ActivityScheduledRecord)
                {
                    this.TrackActivityScheduledRecord((ActivityScheduledRecord) record);
                }
                else if (record is CancelRequestedRecord)
                {
                    this.TrackCancelRequestedRecord((CancelRequestedRecord) record);
                }
                else if (record is FaultPropagationRecord)
                {
                    this.TrackFaultPropagationRecord((FaultPropagationRecord) record);
                }
                else
                {
                    this.TrackCustomRecord((CustomTrackingRecord) record);
                }
            }
        }

        private void TrackActivityRecord(ActivityStateRecord record)
        {
            if (EtwTrackingParticipantTrackRecords.ActivityStateRecordIsEnabled(this.diagnosticTrace) && !EtwTrackingParticipantTrackRecords.ActivityStateRecord(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.State, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, (record.Arguments.Count > 0) ? this.PrepareDictionary(record.Arguments) : "<items />", (record.Variables.Count > 0) ? this.PrepareDictionary(record.Variables) : "<items />", record.HasAnnotations ? PrepareAnnotations(record.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
            {
                if (EtwTrackingParticipantTrackRecords.ActivityStateRecord(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.State, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, "<items>...</items>", "<items>...</items>", "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    this.TraceTrackingRecordTruncated(record.RecordNumber);
                }
                else
                {
                    this.TraceTrackingRecordDropped(record.RecordNumber);
                }
            }
        }

        private void TrackActivityScheduledRecord(ActivityScheduledRecord scheduledRecord)
        {
            if (EtwTrackingParticipantTrackRecords.ActivityScheduledRecordIsEnabled(this.diagnosticTrace) && !EtwTrackingParticipantTrackRecords.ActivityScheduledRecord(this.diagnosticTrace, scheduledRecord.InstanceId, scheduledRecord.RecordNumber, scheduledRecord.EventTime.ToFileTime(), (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.Name, (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.Id, (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.InstanceId, (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.TypeName, scheduledRecord.Child.Name, scheduledRecord.Child.Id, scheduledRecord.Child.InstanceId, scheduledRecord.Child.TypeName, scheduledRecord.HasAnnotations ? PrepareAnnotations(scheduledRecord.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
            {
                if (EtwTrackingParticipantTrackRecords.ActivityScheduledRecord(this.diagnosticTrace, scheduledRecord.InstanceId, scheduledRecord.RecordNumber, scheduledRecord.EventTime.ToFileTime(), (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.Name, (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.Id, (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.InstanceId, (scheduledRecord.Activity == null) ? string.Empty : scheduledRecord.Activity.TypeName, scheduledRecord.Child.Name, scheduledRecord.Child.Id, scheduledRecord.Child.InstanceId, scheduledRecord.Child.TypeName, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    this.TraceTrackingRecordTruncated(scheduledRecord.RecordNumber);
                }
                else
                {
                    this.TraceTrackingRecordDropped(scheduledRecord.RecordNumber);
                }
            }
        }

        private void TrackBookmarkRecord(BookmarkResumptionRecord record)
        {
            if (EtwTrackingParticipantTrackRecords.BookmarkResumptionRecordIsEnabled(this.diagnosticTrace) && !EtwTrackingParticipantTrackRecords.BookmarkResumptionRecord(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.BookmarkName, record.BookmarkScope, record.Owner.Name, record.Owner.Id, record.Owner.InstanceId, record.Owner.TypeName, record.HasAnnotations ? PrepareAnnotations(record.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
            {
                if (EtwTrackingParticipantTrackRecords.BookmarkResumptionRecord(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.BookmarkName, record.BookmarkScope, record.Owner.Name, record.Owner.Id, record.Owner.InstanceId, record.Owner.TypeName, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    this.TraceTrackingRecordTruncated(record.RecordNumber);
                }
                else
                {
                    this.TraceTrackingRecordDropped(record.RecordNumber);
                }
            }
        }

        private void TrackCancelRequestedRecord(CancelRequestedRecord cancelRecord)
        {
            if (EtwTrackingParticipantTrackRecords.CancelRequestedRecordIsEnabled(this.diagnosticTrace) && !EtwTrackingParticipantTrackRecords.CancelRequestedRecord(this.diagnosticTrace, cancelRecord.InstanceId, cancelRecord.RecordNumber, cancelRecord.EventTime.ToFileTime(), (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.Name, (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.Id, (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.InstanceId, (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.TypeName, cancelRecord.Child.Name, cancelRecord.Child.Id, cancelRecord.Child.InstanceId, cancelRecord.Child.TypeName, cancelRecord.HasAnnotations ? PrepareAnnotations(cancelRecord.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
            {
                if (EtwTrackingParticipantTrackRecords.CancelRequestedRecord(this.diagnosticTrace, cancelRecord.InstanceId, cancelRecord.RecordNumber, cancelRecord.EventTime.ToFileTime(), (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.Name, (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.Id, (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.InstanceId, (cancelRecord.Activity == null) ? string.Empty : cancelRecord.Activity.TypeName, cancelRecord.Child.Name, cancelRecord.Child.Id, cancelRecord.Child.InstanceId, cancelRecord.Child.TypeName, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    this.TraceTrackingRecordTruncated(cancelRecord.RecordNumber);
                }
                else
                {
                    this.TraceTrackingRecordDropped(cancelRecord.RecordNumber);
                }
            }
        }

        private void TrackCustomRecord(CustomTrackingRecord record)
        {
            switch (record.Level)
            {
                case TraceLevel.Error:
                    if (!EtwTrackingParticipantTrackRecords.CustomTrackingRecordErrorIsEnabled(this.diagnosticTrace) || EtwTrackingParticipantTrackRecords.CustomTrackingRecordError(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.Name, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, this.PrepareDictionary(record.Data), record.HasAnnotations ? PrepareAnnotations(record.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        break;
                    }
                    if (!EtwTrackingParticipantTrackRecords.CustomTrackingRecordError(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.Name, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, "<items>...</items>", "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        this.TraceTrackingRecordDropped(record.RecordNumber);
                        return;
                    }
                    this.TraceTrackingRecordTruncated(record.RecordNumber);
                    return;

                case TraceLevel.Warning:
                    if (!EtwTrackingParticipantTrackRecords.CustomTrackingRecordWarningIsEnabled(this.diagnosticTrace) || EtwTrackingParticipantTrackRecords.CustomTrackingRecordWarning(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.Name, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, this.PrepareDictionary(record.Data), record.HasAnnotations ? PrepareAnnotations(record.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        break;
                    }
                    if (!EtwTrackingParticipantTrackRecords.CustomTrackingRecordWarning(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.Name, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, "<items>...</items>", "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        this.TraceTrackingRecordDropped(record.RecordNumber);
                        return;
                    }
                    this.TraceTrackingRecordTruncated(record.RecordNumber);
                    return;

                default:
                    if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordInfoIsEnabled(this.diagnosticTrace) && !EtwTrackingParticipantTrackRecords.CustomTrackingRecordInfo(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.Name, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, this.PrepareDictionary(record.Data), record.HasAnnotations ? PrepareAnnotations(record.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.CustomTrackingRecordInfo(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.Name, record.Activity.Name, record.Activity.Id, record.Activity.InstanceId, record.Activity.TypeName, "<items>...</items>", "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            this.TraceTrackingRecordTruncated(record.RecordNumber);
                            return;
                        }
                        this.TraceTrackingRecordDropped(record.RecordNumber);
                    }
                    break;
            }
        }

        private void TrackFaultPropagationRecord(FaultPropagationRecord faultRecord)
        {
            if (EtwTrackingParticipantTrackRecords.FaultPropagationRecordIsEnabled(this.diagnosticTrace) && !EtwTrackingParticipantTrackRecords.FaultPropagationRecord(this.diagnosticTrace, faultRecord.InstanceId, faultRecord.RecordNumber, faultRecord.EventTime.ToFileTime(), faultRecord.FaultSource.Name, faultRecord.FaultSource.Id, faultRecord.FaultSource.InstanceId, faultRecord.FaultSource.TypeName, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.Name : string.Empty, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.Id : string.Empty, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.InstanceId : string.Empty, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.TypeName : string.Empty, faultRecord.Fault.ToString(), faultRecord.IsFaultSource, faultRecord.HasAnnotations ? PrepareAnnotations(faultRecord.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
            {
                if (EtwTrackingParticipantTrackRecords.FaultPropagationRecord(this.diagnosticTrace, faultRecord.InstanceId, faultRecord.RecordNumber, faultRecord.EventTime.ToFileTime(), faultRecord.FaultSource.Name, faultRecord.FaultSource.Id, faultRecord.FaultSource.InstanceId, faultRecord.FaultSource.TypeName, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.Name : string.Empty, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.Id : string.Empty, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.InstanceId : string.Empty, (faultRecord.FaultHandler != null) ? faultRecord.FaultHandler.TypeName : string.Empty, faultRecord.Fault.ToString(), faultRecord.IsFaultSource, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    this.TraceTrackingRecordTruncated(faultRecord.RecordNumber);
                }
                else
                {
                    this.TraceTrackingRecordDropped(faultRecord.RecordNumber);
                }
            }
        }

        private void TrackWorkflowRecord(WorkflowInstanceRecord record)
        {
            if (record is WorkflowInstanceUnhandledExceptionRecord)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecordIsEnabled(this.diagnosticTrace))
                {
                    WorkflowInstanceUnhandledExceptionRecord record2 = record as WorkflowInstanceUnhandledExceptionRecord;
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecord(this.diagnosticTrace, record2.InstanceId, record2.RecordNumber, record2.EventTime.ToFileTime(), record2.ActivityDefinitionId, record2.FaultSource.Name, record2.FaultSource.Id, record2.FaultSource.InstanceId, record2.FaultSource.TypeName, (record2.UnhandledException == null) ? string.Empty : record2.UnhandledException.ToString(), record2.HasAnnotations ? PrepareAnnotations(record2.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceUnhandledExceptionRecord(this.diagnosticTrace, record2.InstanceId, record2.RecordNumber, record2.EventTime.ToFileTime(), record2.ActivityDefinitionId, record2.FaultSource.Name, record2.FaultSource.Id, record2.FaultSource.InstanceId, record2.FaultSource.TypeName, (record2.UnhandledException == null) ? string.Empty : record2.UnhandledException.ToString(), "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            this.TraceTrackingRecordTruncated(record2.RecordNumber);
                        }
                        else
                        {
                            this.TraceTrackingRecordDropped(record2.RecordNumber);
                        }
                    }
                }
            }
            else if (record is WorkflowInstanceAbortedRecord)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecordIsEnabled(this.diagnosticTrace))
                {
                    WorkflowInstanceAbortedRecord record3 = record as WorkflowInstanceAbortedRecord;
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecord(this.diagnosticTrace, record3.InstanceId, record3.RecordNumber, record3.EventTime.ToFileTime(), record3.ActivityDefinitionId, record3.Reason, record3.HasAnnotations ? PrepareAnnotations(record3.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceAbortedRecord(this.diagnosticTrace, record3.InstanceId, record3.RecordNumber, record3.EventTime.ToFileTime(), record3.ActivityDefinitionId, record3.Reason, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            this.TraceTrackingRecordTruncated(record3.RecordNumber);
                        }
                        else
                        {
                            this.TraceTrackingRecordDropped(record3.RecordNumber);
                        }
                    }
                }
            }
            else if (record is WorkflowInstanceSuspendedRecord)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecordIsEnabled(this.diagnosticTrace))
                {
                    WorkflowInstanceSuspendedRecord record4 = record as WorkflowInstanceSuspendedRecord;
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecord(this.diagnosticTrace, record4.InstanceId, record4.RecordNumber, record4.EventTime.ToFileTime(), record4.ActivityDefinitionId, record4.Reason, record4.HasAnnotations ? PrepareAnnotations(record4.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceSuspendedRecord(this.diagnosticTrace, record4.InstanceId, record4.RecordNumber, record4.EventTime.ToFileTime(), record4.ActivityDefinitionId, record4.Reason, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            this.TraceTrackingRecordTruncated(record4.RecordNumber);
                        }
                        else
                        {
                            this.TraceTrackingRecordDropped(record4.RecordNumber);
                        }
                    }
                }
            }
            else if (record is WorkflowInstanceTerminatedRecord)
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecordIsEnabled(this.diagnosticTrace))
                {
                    WorkflowInstanceTerminatedRecord record5 = record as WorkflowInstanceTerminatedRecord;
                    if (!EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecord(this.diagnosticTrace, record5.InstanceId, record5.RecordNumber, record5.EventTime.ToFileTime(), record5.ActivityDefinitionId, record5.Reason, record5.HasAnnotations ? PrepareAnnotations(record5.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                    {
                        if (EtwTrackingParticipantTrackRecords.WorkflowInstanceTerminatedRecord(this.diagnosticTrace, record5.InstanceId, record5.RecordNumber, record5.EventTime.ToFileTime(), record5.ActivityDefinitionId, record5.Reason, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                        {
                            this.TraceTrackingRecordTruncated(record5.RecordNumber);
                        }
                        else
                        {
                            this.TraceTrackingRecordDropped(record5.RecordNumber);
                        }
                    }
                }
            }
            else if (EtwTrackingParticipantTrackRecords.WorkflowInstanceRecordIsEnabled(this.diagnosticTrace) && !EtwTrackingParticipantTrackRecords.WorkflowInstanceRecord(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.ActivityDefinitionId, record.State, record.HasAnnotations ? PrepareAnnotations(record.Annotations) : "<items />", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
            {
                if (EtwTrackingParticipantTrackRecords.WorkflowInstanceRecord(this.diagnosticTrace, record.InstanceId, record.RecordNumber, record.EventTime.ToFileTime(), record.ActivityDefinitionId, record.State, "<items>...</items>", (this.TrackingProfile == null) ? string.Empty : this.TrackingProfile.Name, this.ApplicationReference))
                {
                    this.TraceTrackingRecordTruncated(record.RecordNumber);
                }
                else
                {
                    this.TraceTrackingRecordDropped(record.RecordNumber);
                }
            }
        }

        public string ApplicationReference { get; set; }

        public Guid EtwProviderId
        {
            get
            {
                return this.etwProviderId;
            }
            set
            {
                if (value == Guid.Empty)
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("value");
                }
                this.InitializeEtwTrackingProvider(value);
            }
        }
    }
}


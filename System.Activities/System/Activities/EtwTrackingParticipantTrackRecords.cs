namespace System.Activities
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;

    internal class EtwTrackingParticipantTrackRecords
    {
        [SecurityCritical]
        private static EventDescriptor[] eventDescriptors;
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private EtwTrackingParticipantTrackRecords()
        {
        }

        internal static bool ActivityScheduledRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string Name, string ActivityId, string ActivityInstanceId, string ActivityTypeName, string ChildActivityName, string ChildActivityId, string ChildActivityInstanceId, string ChildActivityTypeName, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 0))
            {
                flag = WriteEtwEvent(trace, 0, InstanceId, RecordNumber, EventTime, Name, ActivityId, ActivityInstanceId, ActivityTypeName, ChildActivityName, ChildActivityId, ChildActivityInstanceId, ChildActivityTypeName, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool ActivityScheduledRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 0);
        }

        internal static bool ActivityStateRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string State, string Name, string ActivityId, string ActivityInstanceId, string ActivityTypeName, string Arguments, string Variables, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 1))
            {
                flag = WriteEtwEvent(trace, 1, InstanceId, RecordNumber, EventTime, State, Name, ActivityId, ActivityInstanceId, ActivityTypeName, Arguments, Variables, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool ActivityStateRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 1);
        }

        internal static bool BookmarkResumptionRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string Name, Guid SubInstanceID, string OwnerActivityName, string OwnerActivityId, string OwnerActivityInstanceId, string OwnerActivityTypeName, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 2))
            {
                flag = WriteEtwEvent(trace, 2, InstanceId, RecordNumber, EventTime, Name, SubInstanceID, OwnerActivityName, OwnerActivityId, OwnerActivityInstanceId, OwnerActivityTypeName, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool BookmarkResumptionRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 2);
        }

        internal static bool CancelRequestedRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string Name, string ActivityId, string ActivityInstanceId, string ActivityTypeName, string ChildActivityName, string ChildActivityId, string ChildActivityInstanceId, string ChildActivityTypeName, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 3))
            {
                flag = WriteEtwEvent(trace, 3, InstanceId, RecordNumber, EventTime, Name, ActivityId, ActivityInstanceId, ActivityTypeName, ChildActivityName, ChildActivityId, ChildActivityInstanceId, ChildActivityTypeName, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool CancelRequestedRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 3);
        }

        internal static bool CustomTrackingRecordError(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string Name, string ActivityName, string ActivityId, string ActivityInstanceId, string ActivityTypeName, string Data, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 6))
            {
                flag = WriteEtwEvent(trace, 6, InstanceId, RecordNumber, EventTime, Name, ActivityName, ActivityId, ActivityInstanceId, ActivityTypeName, Data, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool CustomTrackingRecordErrorIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 6);
        }

        internal static bool CustomTrackingRecordInfo(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string Name, string ActivityName, string ActivityId, string ActivityInstanceId, string ActivityTypeName, string Data, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 5))
            {
                flag = WriteEtwEvent(trace, 5, InstanceId, RecordNumber, EventTime, Name, ActivityName, ActivityId, ActivityInstanceId, ActivityTypeName, Data, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool CustomTrackingRecordInfoIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 5);
        }

        internal static bool CustomTrackingRecordWarning(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string Name, string ActivityName, string ActivityId, string ActivityInstanceId, string ActivityTypeName, string Data, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 7))
            {
                flag = WriteEtwEvent(trace, 7, InstanceId, RecordNumber, EventTime, Name, ActivityName, ActivityId, ActivityInstanceId, ActivityTypeName, Data, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool CustomTrackingRecordWarningIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 7);
        }

        [SecuritySafeCritical]
        private static void EnsureEventDescriptors()
        {
            if (object.ReferenceEquals(eventDescriptors, null))
            {
                eventDescriptors = new EventDescriptor[] { new EventDescriptor(0x68, 0, 0x12, 4, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x67, 0, 0x12, 4, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x6b, 0, 0x12, 4, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x6a, 0, 0x12, 4, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x69, 0, 0x12, 3, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x6c, 0, 0x12, 4, 0, 0, 0x20000000001e0040L), new EventDescriptor(0x6f, 0, 0x12, 2, 0, 0, 0x20000000001e0040L), new EventDescriptor(110, 0, 0x12, 3, 0, 0, 0x20000000001e0040L), new EventDescriptor(0x66, 0, 0x12, 3, 0, 0, 0x20000000000e0040L), new EventDescriptor(100, 0, 0x12, 4, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x65, 0, 0x12, 2, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x70, 0, 0x12, 4, 0, 0, 0x20000000000e0040L), new EventDescriptor(0x71, 0, 0x12, 2, 0, 0, 0x20000000000e0040L) };
            }
        }

        internal static bool FaultPropagationRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string FaultSourceActivityName, string FaultSourceActivityId, string FaultSourceActivityInstanceId, string FaultSourceActivityTypeName, string FaultHandlerActivityName, string FaultHandlerActivityId, string FaultHandlerActivityInstanceId, string FaultHandlerActivityTypeName, string Fault, bool IsFaultSource, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 4))
            {
                flag = WriteEtwEvent(trace, 4, InstanceId, RecordNumber, EventTime, FaultSourceActivityName, FaultSourceActivityId, FaultSourceActivityInstanceId, FaultSourceActivityTypeName, FaultHandlerActivityName, FaultHandlerActivityId, FaultHandlerActivityInstanceId, FaultHandlerActivityTypeName, Fault, IsFaultSource, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool FaultPropagationRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 4);
        }

        [SecuritySafeCritical]
        private static bool IsEtwEventEnabled(DiagnosticTrace trace, int eventIndex)
        {
            EnsureEventDescriptors();
            return trace.IsEtwEventEnabled(ref eventDescriptors[eventIndex]);
        }

        internal static bool WorkflowInstanceAbortedRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string ActivityDefinitionId, string Reason, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 8))
            {
                flag = WriteEtwEvent(trace, 8, InstanceId, RecordNumber, EventTime, ActivityDefinitionId, Reason, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool WorkflowInstanceAbortedRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 8);
        }

        internal static bool WorkflowInstanceRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string ActivityDefinitionId, string State, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 9))
            {
                flag = WriteEtwEvent(trace, 9, InstanceId, RecordNumber, EventTime, ActivityDefinitionId, State, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool WorkflowInstanceRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 9);
        }

        internal static bool WorkflowInstanceSuspendedRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string ActivityDefinitionId, string Reason, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 11))
            {
                flag = WriteEtwEvent(trace, 11, InstanceId, RecordNumber, EventTime, ActivityDefinitionId, Reason, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool WorkflowInstanceSuspendedRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 11);
        }

        internal static bool WorkflowInstanceTerminatedRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string ActivityDefinitionId, string Reason, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 12))
            {
                flag = WriteEtwEvent(trace, 12, InstanceId, RecordNumber, EventTime, ActivityDefinitionId, Reason, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool WorkflowInstanceTerminatedRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 12);
        }

        internal static bool WorkflowInstanceUnhandledExceptionRecord(DiagnosticTrace trace, Guid InstanceId, long RecordNumber, long EventTime, string ActivityDefinitionId, string SourceName, string SourceId, string SourceInstanceId, string SourceTypeName, string Exception, string Annotations, string ProfileName, string reference)
        {
            bool flag = true;
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 10))
            {
                flag = WriteEtwEvent(trace, 10, InstanceId, RecordNumber, EventTime, ActivityDefinitionId, SourceName, SourceId, SourceInstanceId, SourceTypeName, Exception, Annotations, ProfileName, reference, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool WorkflowInstanceUnhandledExceptionRecordIsEnabled(DiagnosticTrace trace)
        {
            return IsEtwEventEnabled(trace, 10);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, Guid eventParam0, long eventParam1, long eventParam2, string eventParam3, string eventParam4, string eventParam5, string eventParam6, string eventParam7, string eventParam8)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6, eventParam7, eventParam8);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, Guid eventParam0, long eventParam1, long eventParam2, string eventParam3, Guid eventParam4, string eventParam5, string eventParam6, string eventParam7, string eventParam8, string eventParam9, string eventParam10, string eventParam11, string eventParam12)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6, eventParam7, eventParam8, eventParam9, eventParam10, eventParam11, eventParam12);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, Guid eventParam0, long eventParam1, long eventParam2, string eventParam3, string eventParam4, string eventParam5, string eventParam6, string eventParam7, string eventParam8, string eventParam9, string eventParam10, string eventParam11, string eventParam12)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6, eventParam7, eventParam8, eventParam9, eventParam10, eventParam11, eventParam12);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, Guid eventParam0, long eventParam1, long eventParam2, string eventParam3, string eventParam4, string eventParam5, string eventParam6, string eventParam7, string eventParam8, string eventParam9, string eventParam10, string eventParam11, string eventParam12, string eventParam13)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6, eventParam7, eventParam8, eventParam9, eventParam10, eventParam11, eventParam12, eventParam13);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, Guid eventParam0, long eventParam1, long eventParam2, string eventParam3, string eventParam4, string eventParam5, string eventParam6, string eventParam7, string eventParam8, string eventParam9, string eventParam10, string eventParam11, string eventParam12, string eventParam13, string eventParam14)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6, eventParam7, eventParam8, eventParam9, eventParam10, eventParam11, eventParam12, eventParam13, eventParam14);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, Guid eventParam0, long eventParam1, long eventParam2, string eventParam3, string eventParam4, string eventParam5, string eventParam6, string eventParam7, string eventParam8, string eventParam9, string eventParam10, string eventParam11, bool eventParam12, string eventParam13, string eventParam14, string eventParam15, string eventParam16)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6, eventParam7, eventParam8, eventParam9, eventParam10, eventParam11, eventParam12, eventParam13, eventParam14, eventParam15, eventParam16);
        }

        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    resourceManager = new System.Resources.ResourceManager("System.Activities.EtwTrackingParticipantTrackRecords", typeof(EtwTrackingParticipantTrackRecords).Assembly);
                }
                return resourceManager;
            }
        }
    }
}


namespace System.ServiceModel.Activities
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;

    internal class TD
    {
        [SecurityCritical]
        private static EventDescriptor[] eventDescriptors;
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private TD()
        {
        }

        internal static void BufferOutOfOrderMessageNoBookmark(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(7))
            {
                WriteEtwEvent(7, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("BufferOutOfOrderMessageNoBookmark", Culture), new object[] { param0, param1 });
                WriteTraceSource(7, description, payload);
            }
        }

        internal static bool BufferOutOfOrderMessageNoBookmarkIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(7);
            }
            return true;
        }

        internal static void BufferOutOfOrderMessageNoInstance(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(6))
            {
                WriteEtwEvent(6, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("BufferOutOfOrderMessageNoInstance", Culture), new object[] { param0 });
                WriteTraceSource(6, description, payload);
            }
        }

        internal static bool BufferOutOfOrderMessageNoInstanceIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(6);
            }
            return true;
        }

        internal static void CreateWorkflowServiceHostStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(9))
            {
                WriteEtwEvent(9, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CreateWorkflowServiceHostStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(9));
        }

        internal static void CreateWorkflowServiceHostStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(10))
            {
                WriteEtwEvent(10, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CreateWorkflowServiceHostStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(10));
        }

        internal static void DuplicateCorrelationQuery(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(2))
            {
                WriteEtwEvent(2, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("DuplicateCorrelationQuery", Culture), new object[] { param0 });
                WriteTraceSource(2, description, payload);
            }
        }

        internal static bool DuplicateCorrelationQueryIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(2);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static void EnsureEventDescriptors()
        {
            if (object.ReferenceEquals(TD.eventDescriptors, null))
            {
                EventDescriptor[] eventDescriptors = new EventDescriptor[] { new EventDescriptor(0xdad, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xdae, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xdaf, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0xdb3, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(440, 0, 0x12, 4, 1, 0, 0x2000000000080000L), new EventDescriptor(0x1b9, 0, 0x12, 4, 2, 0, 0x2000000000080000L), new EventDescriptor(0xdde, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xddf, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xde0, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0xde3, 0, 20, 4, 1, 0, 0x800000000000080L), new EventDescriptor(0xde4, 0, 20, 4, 2, 0, 0x800000000000080L), new EventDescriptor(0xde5, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xdb4, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe1, 0, 0x12, 4, 0, 0, 0x2000000000080004L) };
                FxTrace.UpdateEventDefinitions(eventDescriptors);
                TD.eventDescriptors = eventDescriptors;
            }
        }

        internal static void InferredContractDescription(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0))
            {
                WriteEtwEvent(0, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InferredContractDescription", Culture), new object[] { param0, param1 });
                WriteTraceSource(0, description, payload);
            }
        }

        internal static bool InferredContractDescriptionIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0);
            }
            return true;
        }

        internal static void InferredOperationDescription(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(1))
            {
                WriteEtwEvent(1, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InferredOperationDescription", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(1, description, payload);
            }
        }

        internal static bool InferredOperationDescriptionIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(1);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool IsEtwEventEnabled(int eventIndex)
        {
            EnsureEventDescriptors();
            return FxTrace.IsEventEnabled(eventIndex);
        }

        internal static void MaxPendingMessagesPerChannelExceeded(int limit)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(8))
            {
                WriteEtwEvent(8, limit, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("MaxPendingMessagesPerChannelExceeded", Culture), new object[] { limit });
                WriteTraceSource(8, description, payload);
            }
        }

        internal static bool MaxPendingMessagesPerChannelExceededIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(8);
            }
            return true;
        }

        internal static void ServiceEndpointAdded(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(3))
            {
                WriteEtwEvent(3, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ServiceEndpointAdded", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(3, description, payload);
            }
        }

        internal static bool ServiceEndpointAddedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(3);
            }
            return true;
        }

        internal static void StartSignpostEvent(TraceRecord traceRecord)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(4))
            {
                WriteEtwEvent(4, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartSignpostEvent", Culture), new object[0]);
                WriteTraceSource(4, description, payload);
            }
        }

        internal static bool StartSignpostEventIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(4);
            }
            return true;
        }

        internal static void StopSignpostEvent(TraceRecord traceRecord)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(5))
            {
                WriteEtwEvent(5, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StopSignpostEvent", Culture), new object[0]);
                WriteTraceSource(5, description, payload);
            }
        }

        internal static bool StopSignpostEventIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(5);
            }
            return true;
        }

        internal static void TraceCorrelationKeys(Guid InstanceKey, string Values, string ParentScope)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(13))
            {
                WriteEtwEvent(13, InstanceKey, Values, ParentScope, payload.HostReference, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TraceCorrelationKeys", Culture), new object[] { InstanceKey, Values, ParentScope });
                WriteTraceSource(13, description, payload);
            }
        }

        internal static bool TraceCorrelationKeysIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(13);
            }
            return true;
        }

        internal static void TrackingProfileNotFound(string TrackingProfile, string ActivityDefinitionId)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(12))
            {
                WriteEtwEvent(12, TrackingProfile, ActivityDefinitionId, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TrackingProfileNotFound", Culture), new object[] { TrackingProfile, ActivityDefinitionId });
                WriteTraceSource(12, description, payload);
            }
        }

        internal static bool TrackingProfileNotFoundIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(12);
            }
            return true;
        }

        internal static void TransactedReceiveScopeEndCommitFailed(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(11))
            {
                WriteEtwEvent(11, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TransactedReceiveScopeEndCommitFailed", Culture), new object[] { param0, param1 });
                WriteTraceSource(11, description, payload);
            }
        }

        internal static bool TransactedReceiveScopeEndCommitFailedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(11);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, int eventParam0, string eventParam1)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], new object[] { eventParam0, eventParam1 });
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, Guid eventParam0, string eventParam1, string eventParam2, string eventParam3, string eventParam4)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], new object[] { eventParam0, eventParam1, eventParam2, eventParam3, eventParam4 });
        }

        [SecuritySafeCritical]
        private static void WriteTraceSource(int eventIndex, string description, TracePayload payload)
        {
            EnsureEventDescriptors();
            FxTrace.Trace.WriteTraceSource(ref eventDescriptors[eventIndex], description, payload);
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
                    resourceManager = new System.Resources.ResourceManager("System.ServiceModel.Activities.TD", typeof(TD).Assembly);
                }
                return resourceManager;
            }
        }
    }
}


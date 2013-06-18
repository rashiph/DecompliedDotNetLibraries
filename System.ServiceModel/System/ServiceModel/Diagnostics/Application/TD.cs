namespace System.ServiceModel.Diagnostics.Application
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
        private static EventDescriptor[] eventDescriptors;
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private TD()
        {
        }

        internal static void ClientMessageInspectorAfterReceiveInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(1))
            {
                WriteEtwEvent(1, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ClientMessageInspectorAfterReceiveInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(1));
        }

        internal static void ClientMessageInspectorBeforeSendInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(2))
            {
                WriteEtwEvent(2, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ClientMessageInspectorBeforeSendInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(2));
        }

        internal static void ClientOperationCompleted(string Action, string ContractName, string Destination)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(0x17))
            {
                WriteEtwEvent(0x17, Action, ContractName, Destination, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ClientOperationCompletedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x17));
        }

        internal static void ClientOperationPrepared(string Action, string ContractName, string Destination)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(0))
            {
                WriteEtwEvent(0, Action, ContractName, Destination, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ClientOperationPreparedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0));
        }

        internal static void ClientParameterInspectorAfterCallInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(3))
            {
                WriteEtwEvent(3, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ClientParameterInspectorAfterCallInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(3));
        }

        internal static void ClientParameterInspectorBeforeCallInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(4))
            {
                WriteEtwEvent(4, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ClientParameterInspectorBeforeCallInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(4));
        }

        internal static void CommunicationObjectOpenStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(30))
            {
                WriteEtwEvent(30, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CommunicationObjectOpenStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(30));
        }

        internal static void CommunicationObjectOpenStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x1f))
            {
                WriteEtwEvent(0x1f, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CommunicationObjectOpenStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x1f));
        }

        [SecuritySafeCritical]
        private static void EnsureEventDescriptors()
        {
            if (object.ReferenceEquals(TD.eventDescriptors, null))
            {
                EventDescriptor[] eventDescriptors = new EventDescriptor[] { 
                    new EventDescriptor(0xd9, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xc9, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xca, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xcb, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xcc, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xcd, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xce, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xcf, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xd0, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xd1, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(210, 0, 0x12, 3, 0, 0, 0x20000000000e0004L), new EventDescriptor(0xd3, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xd4, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xd6, 0, 0x12, 4, 0, 0, 0x20000000000e0004L), new EventDescriptor(0xd7, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xd8, 0, 0x12, 4, 0, 0, 0x2000000000080004L), 
                    new EventDescriptor(0x1c3, 0, 0x12, 4, 0, 0, 0x2000000000080020L), new EventDescriptor(0x1c4, 0, 0x12, 3, 0, 0, 0x2000000000080020L), new EventDescriptor(0x11f8, 0, 0x13, 3, 0, 0, 0x1000000000000020L), new EventDescriptor(0x194, 0, 0x12, 4, 7, 0, 0x2000000000080000L), new EventDescriptor(0x192, 0, 0x12, 4, 1, 0, 0x2000000000080000L), new EventDescriptor(0x191, 0, 0x12, 4, 2, 0, 0x2000000000080000L), new EventDescriptor(0x193, 0, 0x12, 4, 8, 0, 0x2000000000080000L), new EventDescriptor(0xda, 0, 0x12, 4, 0, 0, 0x2000000000080004L), new EventDescriptor(0xdb, 0, 0x12, 2, 0, 0, 0x20000000000e0004L), new EventDescriptor(0xde, 0, 0x12, 3, 0, 0, 0x20000000000e0004L), new EventDescriptor(0xdf, 0, 0x12, 3, 0, 0, 0x20000000000e0004L), new EventDescriptor(0xe0, 0, 0x12, 3, 0, 0, 0x20000000000e0004L), new EventDescriptor(0xdd, 0, 0x12, 4, 0, 0, 0x20000000000a0004L), new EventDescriptor(220, 0, 0x12, 4, 0, 0, 0x20000000000a0004L), new EventDescriptor(0x1fd, 0, 20, 4, 1, 0x9c9, 0x800000000000100L), new EventDescriptor(510, 0, 20, 4, 2, 0x9c9, 0x800000000000100L), 
                    new EventDescriptor(0xce5, 0, 0x13, 3, 0, 0, 0x1000000000000004L), new EventDescriptor(0xce7, 0, 0x13, 4, 0, 0, 0x1000000000000004L), new EventDescriptor(0xce4, 0, 0x13, 3, 0, 0, 0x1000000000000004L), new EventDescriptor(0xce6, 0, 0x13, 3, 0, 0, 0x1000000000000004L)
                 };
                FxTrace.UpdateEventDefinitions(eventDescriptors);
                TD.eventDescriptors = eventDescriptors;
            }
        }

        internal static void ErrorHandlerInvoked(string TypeName, bool Handled, string ExceptionTypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(6))
            {
                WriteEtwEvent(6, TypeName, Handled, ExceptionTypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ErrorHandlerInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(6));
        }

        internal static void FaultProviderInvoked(string TypeName, string ExceptionTypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(7))
            {
                WriteEtwEvent(7, TypeName, ExceptionTypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool FaultProviderInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(7));
        }

        private static bool IsEtwEventEnabled(int eventIndex)
        {
            EnsureEventDescriptors();
            return FxTrace.IsEventEnabled(eventIndex);
        }

        internal static void MessageInspectorAfterReceiveInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(8))
            {
                WriteEtwEvent(8, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageInspectorAfterReceiveInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(8));
        }

        internal static void MessageInspectorBeforeSendInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(9))
            {
                WriteEtwEvent(9, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageInspectorBeforeSendInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(9));
        }

        internal static void MessageLogEventSizeExceeded()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x12))
            {
                WriteEtwEvent(0x12, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageLogEventSizeExceededIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(0x12));
        }

        internal static bool MessageLogInfo(string param0)
        {
            bool flag = true;
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x10))
            {
                flag = WriteEtwEvent(0x10, param0, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool MessageLogInfoIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x10));
        }

        internal static bool MessageLogWarning(string param0)
        {
            bool flag = true;
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x11))
            {
                flag = WriteEtwEvent(0x11, param0, payload.AppDomainFriendlyName);
            }
            return flag;
        }

        internal static bool MessageLogWarningIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(0x11));
        }

        internal static void MessageReceivedByTransport(string ListenAddress)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(14))
            {
                WriteEtwEvent(14, ListenAddress, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageReceivedByTransportIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(14));
        }

        internal static void MessageReceivedFromTransport(Guid CorrelationId, string reference)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x1c))
            {
                WriteEtwEvent(0x1c, CorrelationId, reference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageReceivedFromTransportIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x1c));
        }

        internal static void MessageSentByTransport(string DestinationAddress)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(15))
            {
                WriteEtwEvent(15, DestinationAddress, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageSentByTransportIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(15));
        }

        internal static void MessageSentToTransport(Guid CorrelationId)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(0x1d))
            {
                WriteEtwEvent(0x1d, CorrelationId, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageSentToTransportIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x1d));
        }

        internal static void MessageThrottleAtSeventyPercent(string ThrottleName, long Limit)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(0x1b))
            {
                WriteEtwEvent(0x1b, ThrottleName, Limit, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageThrottleAtSeventyPercentIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(0x1b));
        }

        internal static void MessageThrottleExceeded(string ThrottleName, long Limit)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(10))
            {
                WriteEtwEvent(10, ThrottleName, Limit, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool MessageThrottleExceededIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(10));
        }

        internal static void OperationCompleted(string MethodName, long Duration)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(13))
            {
                WriteEtwEvent(13, MethodName, Duration, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool OperationCompletedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(13));
        }

        internal static void OperationFailed(string MethodName, long Duration)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(0x19))
            {
                WriteEtwEvent(0x19, MethodName, Duration, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool OperationFailedIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(0x19));
        }

        internal static void OperationFaulted(string MethodName, long Duration)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(0x1a))
            {
                WriteEtwEvent(0x1a, MethodName, Duration, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool OperationFaultedIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(0x1a));
        }

        internal static void OperationInvoked(string MethodName, string CallerInfo)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(5))
            {
                WriteEtwEvent(5, MethodName, CallerInfo, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool OperationInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(5));
        }

        internal static void ParameterInspectorAfterCallInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(11))
            {
                WriteEtwEvent(11, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ParameterInspectorAfterCallInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(11));
        }

        internal static void ParameterInspectorBeforeCallInvoked(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(12))
            {
                WriteEtwEvent(12, TypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ParameterInspectorBeforeCallInvokedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(12));
        }

        internal static void ReceiveContextAbandonFailed(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x20))
            {
                WriteEtwEvent(0x20, TypeName, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ReceiveContextAbandonFailed", Culture), new object[] { TypeName });
                WriteTraceSource(0x20, description, payload);
            }
        }

        internal static bool ReceiveContextAbandonFailedIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x20);
            }
            return true;
        }

        internal static void ReceiveContextAbandonWithException(string TypeName, string ExceptionToString)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x21))
            {
                WriteEtwEvent(0x21, TypeName, ExceptionToString, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ReceiveContextAbandonWithException", Culture), new object[] { TypeName, ExceptionToString });
                WriteTraceSource(0x21, description, payload);
            }
        }

        internal static bool ReceiveContextAbandonWithExceptionIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x21);
            }
            return true;
        }

        internal static void ReceiveContextCompleteFailed(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x22))
            {
                WriteEtwEvent(0x22, TypeName, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ReceiveContextCompleteFailed", Culture), new object[] { TypeName });
                WriteTraceSource(0x22, description, payload);
            }
        }

        internal static bool ReceiveContextCompleteFailedIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x22);
            }
            return true;
        }

        internal static void ReceiveContextFaulted(string TypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x23))
            {
                WriteEtwEvent(0x23, TypeName, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ReceiveContextFaulted", Culture), new object[] { TypeName });
                WriteTraceSource(0x23, description, payload);
            }
        }

        internal static bool ReceiveContextFaultedIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x23);
            }
            return true;
        }

        internal static void ResumeSignpostEvent(TraceRecord traceRecord)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(0x13))
            {
                WriteEtwEvent(0x13, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ResumeSignpostEventIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x13));
        }

        internal static void ServiceException(string ExceptionToString, string ExceptionTypeName)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null, true);
            if (IsEtwEventEnabled(0x18))
            {
                WriteEtwEvent(0x18, ExceptionToString, ExceptionTypeName, payload.HostReference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ServiceExceptionIsEnabled()
        {
            return (FxTrace.ShouldTraceError && FxTrace.IsEventEnabled(0x18));
        }

        internal static void StartSignpostEvent(TraceRecord traceRecord)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(20))
            {
                WriteEtwEvent(20, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
        }

        internal static bool StartSignpostEventIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(20));
        }

        internal static void StopSignpostEvent(TraceRecord traceRecord)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(0x15))
            {
                WriteEtwEvent(0x15, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
        }

        internal static bool StopSignpostEventIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x15));
        }

        internal static void SuspendSignpostEvent(TraceRecord traceRecord)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(0x16))
            {
                WriteEtwEvent(0x16, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
        }

        internal static bool SuspendSignpostEventIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x16));
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, Guid eventParam0, string eventParam1, string eventParam2)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, long eventParam1, string eventParam2, string eventParam3)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, bool eventParam1, string eventParam2, string eventParam3, string eventParam4)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], new object[] { eventParam0, eventParam1, eventParam2, eventParam3, eventParam4 });
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3, string eventParam4)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4);
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
                    resourceManager = new System.Resources.ResourceManager("System.ServiceModel.Diagnostics.Application.TD", typeof(TD).Assembly);
                }
                return resourceManager;
            }
        }
    }
}


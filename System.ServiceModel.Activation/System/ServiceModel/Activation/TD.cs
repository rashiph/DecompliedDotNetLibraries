namespace System.ServiceModel.Activation
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;
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

        internal static void AspNetRoute(string AspNetRoutePrefix, string ServiceType, string ServiceHostFactoryType)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(14))
            {
                WriteEtwEvent(14, AspNetRoutePrefix, ServiceType, ServiceHostFactoryType, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("AspNetRoute", Culture), new object[] { AspNetRoutePrefix, ServiceType, ServiceHostFactoryType });
                WriteTraceSource(14, description, payload);
            }
        }

        internal static bool AspNetRouteIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(14);
            }
            return true;
        }

        internal static void AspNetRoutingService(string IncomingAddress)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(13))
            {
                WriteEtwEvent(13, IncomingAddress, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("AspNetRoutingService", Culture), new object[] { IncomingAddress });
                WriteTraceSource(13, description, payload);
            }
        }

        internal static bool AspNetRoutingServiceIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(13);
            }
            return true;
        }

        internal static void CBAEntryRead(string RelativeAddress, string NormalizedAddress)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x10))
            {
                WriteEtwEvent(0x10, RelativeAddress, NormalizedAddress, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CBAEntryRead", Culture), new object[] { RelativeAddress, NormalizedAddress });
                WriteTraceSource(0x10, description, payload);
            }
        }

        internal static bool CBAEntryReadIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x10);
            }
            return true;
        }

        internal static void CBAMatchFound(string IncomingAddress)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x11))
            {
                WriteEtwEvent(0x11, IncomingAddress, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CBAMatchFound", Culture), new object[] { IncomingAddress });
                WriteTraceSource(0x11, description, payload);
            }
        }

        internal static bool CBAMatchFoundIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x11);
            }
            return true;
        }

        internal static void CompilationStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(1))
            {
                WriteEtwEvent(1, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CompilationStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(1));
        }

        internal static void CompilationStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(2))
            {
                WriteEtwEvent(2, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CompilationStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(2));
        }

        internal static void CreateServiceHostStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(5))
            {
                WriteEtwEvent(5, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CreateServiceHostStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(5));
        }

        internal static void CreateServiceHostStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(6))
            {
                WriteEtwEvent(6, payload.AppDomainFriendlyName);
            }
        }

        internal static bool CreateServiceHostStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(6));
        }

        internal static void DecrementBusyCount(string Data)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x15))
            {
                WriteEtwEvent(0x15, Data, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("DecrementBusyCount", Culture), new object[] { Data });
                WriteTraceSource(0x15, description, payload);
            }
        }

        internal static bool DecrementBusyCountIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x15);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static void EnsureEventDescriptors()
        {
            if (object.ReferenceEquals(TD.eventDescriptors, null))
            {
                EventDescriptor[] eventDescriptors = new EventDescriptor[] { 
                    new EventDescriptor(0x3ce8, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1f5, 0, 20, 4, 1, 0x9c5, 0x800000000000080L), new EventDescriptor(0x1f6, 0, 20, 4, 2, 0x9c5, 0x800000000000080L), new EventDescriptor(0x1f7, 0, 20, 4, 1, 0x9c6, 0x800000000000080L), new EventDescriptor(0x1f8, 0, 20, 4, 2, 0x9c6, 0x800000000000080L), new EventDescriptor(0x1f9, 0, 20, 4, 1, 0x9c7, 0x800000000000080L), new EventDescriptor(0x1fa, 0, 20, 4, 2, 0x9c7, 0x800000000000080L), new EventDescriptor(0x1fb, 0, 20, 4, 1, 0x9c8, 0x800000000000080L), new EventDescriptor(0x1fc, 0, 20, 4, 2, 0x9c8, 0x800000000000080L), new EventDescriptor(0x1ff, 0, 20, 4, 1, 0, 0x800000000000080L), new EventDescriptor(0x200, 0, 20, 4, 2, 0, 0x800000000000080L), new EventDescriptor(0x201, 0, 20, 4, 1, 0, 0x800000000000080L), new EventDescriptor(0x202, 0, 20, 4, 2, 0, 0x800000000000080L), new EventDescriptor(0x25b, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x25c, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0xd5, 0, 0x12, 0, 0, 0, 0x20000000000e0001L), 
                    new EventDescriptor(0x259, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x25a, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0xde1, 0, 20, 4, 1, 0, 0x800000000000080L), new EventDescriptor(0xde2, 0, 20, 4, 2, 0, 0x800000000000080L), new EventDescriptor(0x25d, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x25e, 0, 0x13, 5, 0, 0, 0x1000000000000000L)
                 };
                FxTrace.UpdateEventDefinitions(eventDescriptors);
                TD.eventDescriptors = eventDescriptors;
            }
        }

        internal static void HostedTransportConfigurationManagerConfigInitStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(7))
            {
                WriteEtwEvent(7, payload.AppDomainFriendlyName);
            }
        }

        internal static bool HostedTransportConfigurationManagerConfigInitStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(7));
        }

        internal static void HostedTransportConfigurationManagerConfigInitStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(8))
            {
                WriteEtwEvent(8, payload.AppDomainFriendlyName);
            }
        }

        internal static bool HostedTransportConfigurationManagerConfigInitStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(8));
        }

        internal static void HttpHandlerPickedForUrl(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0))
            {
                WriteEtwEvent(0, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("HttpHandlerPickedForUrl", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0, description, payload);
            }
        }

        internal static bool HttpHandlerPickedForUrlIsEnabled()
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

        internal static void IncrementBusyCount(string Data)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(20))
            {
                WriteEtwEvent(20, Data, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("IncrementBusyCount", Culture), new object[] { Data });
                WriteTraceSource(20, description, payload);
            }
        }

        internal static bool IncrementBusyCountIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(20);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool IsEtwEventEnabled(int eventIndex)
        {
            EnsureEventDescriptors();
            return FxTrace.IsEventEnabled(eventIndex);
        }

        internal static void ServiceHostFactoryCreationStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(3))
            {
                WriteEtwEvent(3, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ServiceHostFactoryCreationStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(3));
        }

        internal static void ServiceHostFactoryCreationStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(4))
            {
                WriteEtwEvent(4, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ServiceHostFactoryCreationStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(4));
        }

        internal static void ServiceHostOpenStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(9))
            {
                WriteEtwEvent(9, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ServiceHostOpenStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(9));
        }

        internal static void ServiceHostOpenStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(10))
            {
                WriteEtwEvent(10, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ServiceHostOpenStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(10));
        }

        internal static void ServiceHostStarted(string ServiceTypeName, string reference)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(15))
            {
                WriteEtwEvent(15, ServiceTypeName, reference, payload.AppDomainFriendlyName);
            }
        }

        internal static bool ServiceHostStartedIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(15));
        }

        internal static void WebHostRequestStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(11))
            {
                WriteEtwEvent(11, payload.AppDomainFriendlyName);
            }
        }

        internal static bool WebHostRequestStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(11));
        }

        internal static void WebHostRequestStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(12))
            {
                WriteEtwEvent(12, payload.AppDomainFriendlyName);
            }
        }

        internal static bool WebHostRequestStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(12));
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
        private static void WriteTraceSource(int eventIndex, string description, TracePayload payload)
        {
            EnsureEventDescriptors();
            FxTrace.Trace.WriteTraceSource(ref eventDescriptors[eventIndex], description, payload);
        }

        internal static void XamlServicesLoadStart()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x12))
            {
                WriteEtwEvent(0x12, payload.AppDomainFriendlyName);
            }
        }

        internal static bool XamlServicesLoadStartIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x12));
        }

        internal static void XamlServicesLoadStop()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x13))
            {
                WriteEtwEvent(0x13, payload.AppDomainFriendlyName);
            }
        }

        internal static bool XamlServicesLoadStopIsEnabled()
        {
            return (FxTrace.ShouldTraceInformation && FxTrace.IsEventEnabled(0x13));
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
                    resourceManager = new System.Resources.ResourceManager("System.ServiceModel.Activation.TD", typeof(TD).Assembly);
                }
                return resourceManager;
            }
        }
    }
}


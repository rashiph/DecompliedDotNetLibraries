namespace System.Activities.DurableInstancing
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

        internal static void EndSqlCommandExecute(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0))
            {
                WriteEtwEvent(0, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("EndSqlCommandExecute", Culture), new object[] { param0 });
                WriteTraceSource(0, description, payload);
            }
        }

        internal static bool EndSqlCommandExecuteIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static void EnsureEventDescriptors()
        {
            if (object.ReferenceEquals(TD.eventDescriptors, null))
            {
                EventDescriptor[] eventDescriptors = new EventDescriptor[] { new EventDescriptor(0x1069, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x106a, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x106b, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0x106d, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0x106e, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0x106f, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1070, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1071, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1072, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1074, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1073, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1075, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0x1076, 0, 0x13, 2, 0, 0, 0x1000000000000000L) };
                FxTrace.UpdateEventDefinitions(eventDescriptors);
                TD.eventDescriptors = eventDescriptors;
            }
        }

        internal static void FoundProcessingError(string param0, Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(3))
            {
                WriteEtwEvent(3, param0, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceErrorToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("FoundProcessingError", Culture), new object[] { param0 });
                WriteTraceSource(3, description, payload);
            }
        }

        internal static bool FoundProcessingErrorIsEnabled()
        {
            if (!FxTrace.ShouldTraceError)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceErrorToTraceSource)
            {
                return FxTrace.IsEventEnabled(3);
            }
            return true;
        }

        internal static void InstanceLocksRecoveryError(Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(12))
            {
                WriteEtwEvent(12, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceErrorToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InstanceLocksRecoveryError", Culture), new object[0]);
                WriteTraceSource(12, description, payload);
            }
        }

        internal static bool InstanceLocksRecoveryErrorIsEnabled()
        {
            if (!FxTrace.ShouldTraceError)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceErrorToTraceSource)
            {
                return FxTrace.IsEventEnabled(12);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool IsEtwEventEnabled(int eventIndex)
        {
            EnsureEventDescriptors();
            return FxTrace.IsEventEnabled(eventIndex);
        }

        internal static void LockRetryTimeout(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(9))
            {
                WriteEtwEvent(9, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("LockRetryTimeout", Culture), new object[] { param0 });
                WriteTraceSource(9, description, payload);
            }
        }

        internal static bool LockRetryTimeoutIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(9);
            }
            return true;
        }

        internal static void MaximumRetriesExceededForSqlCommand()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(5))
            {
                WriteEtwEvent(5, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("MaximumRetriesExceededForSqlCommand", Culture), new object[0]);
                WriteTraceSource(5, description, payload);
            }
        }

        internal static bool MaximumRetriesExceededForSqlCommandIsEnabled()
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

        internal static void QueingSqlRetry(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(10))
            {
                WriteEtwEvent(10, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("QueingSqlRetry", Culture), new object[] { param0 });
                WriteTraceSource(10, description, payload);
            }
        }

        internal static bool QueingSqlRetryIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(10);
            }
            return true;
        }

        internal static void RenewLockSystemError()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(2))
            {
                WriteEtwEvent(2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceErrorToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("RenewLockSystemError", Culture), new object[0]);
                WriteTraceSource(2, description, payload);
            }
        }

        internal static bool RenewLockSystemErrorIsEnabled()
        {
            if (!FxTrace.ShouldTraceError)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceErrorToTraceSource)
            {
                return FxTrace.IsEventEnabled(2);
            }
            return true;
        }

        internal static void RetryingSqlCommandDueToSqlError(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(6))
            {
                WriteEtwEvent(6, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("RetryingSqlCommandDueToSqlError", Culture), new object[] { param0 });
                WriteTraceSource(6, description, payload);
            }
        }

        internal static bool RetryingSqlCommandDueToSqlErrorIsEnabled()
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

        internal static void RunnableInstancesDetectionError(Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(11))
            {
                WriteEtwEvent(11, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceErrorToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("RunnableInstancesDetectionError", Culture), new object[0]);
                WriteTraceSource(11, description, payload);
            }
        }

        internal static bool RunnableInstancesDetectionErrorIsEnabled()
        {
            if (!FxTrace.ShouldTraceError)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceErrorToTraceSource)
            {
                return FxTrace.IsEventEnabled(11);
            }
            return true;
        }

        internal static void SqlExceptionCaught(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(8))
            {
                WriteEtwEvent(8, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("SqlExceptionCaught", Culture), new object[] { param0, param1 });
                WriteTraceSource(8, description, payload);
            }
        }

        internal static bool SqlExceptionCaughtIsEnabled()
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

        internal static void StartSqlCommandExecute(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(1))
            {
                WriteEtwEvent(1, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartSqlCommandExecute", Culture), new object[] { param0 });
                WriteTraceSource(1, description, payload);
            }
        }

        internal static bool StartSqlCommandExecuteIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(1);
            }
            return true;
        }

        internal static void TimeoutOpeningSqlConnection(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(7))
            {
                WriteEtwEvent(7, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceErrorToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TimeoutOpeningSqlConnection", Culture), new object[] { param0 });
                WriteTraceSource(7, description, payload);
            }
        }

        internal static bool TimeoutOpeningSqlConnectionIsEnabled()
        {
            if (!FxTrace.ShouldTraceError)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceErrorToTraceSource)
            {
                return FxTrace.IsEventEnabled(7);
            }
            return true;
        }

        internal static void UnlockInstanceException(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(4))
            {
                WriteEtwEvent(4, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceErrorToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("UnlockInstanceException", Culture), new object[] { param0 });
                WriteTraceSource(4, description, payload);
            }
        }

        internal static bool UnlockInstanceExceptionIsEnabled()
        {
            if (!FxTrace.ShouldTraceError)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceErrorToTraceSource)
            {
                return FxTrace.IsEventEnabled(4);
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
                    resourceManager = new System.Resources.ResourceManager("System.Activities.DurableInstancing.TD", typeof(TD).Assembly);
                }
                return resourceManager;
            }
        }
    }
}


namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Administration;

    internal abstract class ServicePerformanceCountersBase : PerformanceCountersBase
    {
        private const int hashLength = 2;
        private string instanceName;
        private const int maxCounterLength = 0x40;
        protected static readonly string[] perfCounterNames = new string[] { 
            "Calls", "Calls Per Second", "Calls Outstanding", "Calls Failed", "Calls Failed Per Second", "Calls Faulted", "Calls Faulted Per Second", "Calls Duration", "Calls Duration Base", "Security Validation and Authentication Failures", "Security Validation and Authentication Failures Per Second", "Security Calls Not Authorized", "Security Calls Not Authorized Per Second", "Instances", "Instances Created Per Second", "Reliable Messaging Sessions Faulted", 
            "Reliable Messaging Sessions Faulted Per Second", "Reliable Messaging Messages Dropped", "Reliable Messaging Messages Dropped Per Second", "Transactions Flowed", "Transactions Flowed Per Second", "Transacted Operations Committed", "Transacted Operations Committed Per Second", "Transacted Operations Aborted", "Transacted Operations Aborted Per Second", "Transacted Operations In Doubt", "Transacted Operations In Doubt Per Second", "Queued Poison Messages", "Queued Poison Messages Per Second", "Queued Messages Rejected", "Queued Messages Rejected Per Second", "Queued Messages Dropped", 
            "Queued Messages Dropped Per Second", "Percent Of Max Concurrent Calls", "Percent Of Max Concurrent Calls Base", "Percent Of Max Concurrent Instances", "Percent Of Max Concurrent Instances Base", "Percent Of Max Concurrent Sessions", "Percent Of Max Concurrent Sessions Base"
         };

        internal ServicePerformanceCountersBase(ServiceHostBase serviceHost)
        {
            this.instanceName = CreateFriendlyInstanceName(serviceHost);
        }

        internal abstract void AuthenticationFailed();
        internal abstract void AuthorizationFailed();
        internal static string CreateFriendlyInstanceName(ServiceHostBase serviceHost)
        {
            string firstAddress;
            ServiceInfo info = new ServiceInfo(serviceHost);
            string serviceName = info.ServiceName;
            if (!TryGetFullVirtualPath(serviceHost, out firstAddress))
            {
                firstAddress = info.FirstAddress;
            }
            int totalLen = (serviceName.Length + firstAddress.Length) + 2;
            if (totalLen > 0x40)
            {
                int num2 = 0;
                truncOptions options = GetCompressionTasks(totalLen, serviceName.Length, firstAddress.Length);
                if ((options & (truncOptions.NoBits | truncOptions.service32)) > truncOptions.NoBits)
                {
                    num2 = 0x20;
                    serviceName = PerformanceCountersBase.GetHashedString(serviceName, num2 - 2, (serviceName.Length - num2) + 2, true);
                }
                if ((options & (truncOptions.NoBits | truncOptions.uri31)) > truncOptions.NoBits)
                {
                    num2 = 0x1f;
                    firstAddress = PerformanceCountersBase.GetHashedString(firstAddress, 0, (firstAddress.Length - num2) + 2, false);
                }
            }
            return (serviceName + "@" + firstAddress.Replace('/', '|'));
        }

        internal abstract void DecrementThrottlePercent(int counterIndex);
        private static truncOptions GetCompressionTasks(int totalLen, int serviceLen, int uriLen)
        {
            truncOptions noBits = truncOptions.NoBits;
            if (totalLen > 0x40)
            {
                int num = totalLen;
                if ((num > 0x40) && (serviceLen > 0x20))
                {
                    noBits |= truncOptions.NoBits | truncOptions.service32;
                    num -= serviceLen - 0x20;
                }
                if ((num > 0x40) && (uriLen > 0x1f))
                {
                    noBits |= truncOptions.NoBits | truncOptions.uri31;
                }
            }
            return noBits;
        }

        internal abstract void IncrementThrottlePercent(int counterIndex);
        internal abstract void MessageDropped();
        internal abstract void MethodCalled();
        internal abstract void MethodReturnedError();
        internal abstract void MethodReturnedFault();
        internal abstract void MethodReturnedSuccess();
        internal abstract void MsmqDroppedMessage();
        internal abstract void MsmqPoisonMessage();
        internal abstract void MsmqRejectedMessage();
        internal abstract void SaveCallDuration(long time);
        internal abstract void ServiceInstanceCreated();
        internal abstract void ServiceInstanceRemoved();
        internal abstract void SessionFaulted();
        internal abstract void SetThrottleBase(int counterIndex, long denominator);
        private static bool TryGetFullVirtualPath(ServiceHostBase serviceHost, out string uri)
        {
            VirtualPathExtension extension = serviceHost.Extensions.Find<VirtualPathExtension>();
            if (extension == null)
            {
                uri = null;
                return false;
            }
            uri = extension.ApplicationVirtualPath + extension.VirtualPath.ToString().Replace("~", "");
            return (uri != null);
        }

        internal abstract void TxAborted(long count);
        internal abstract void TxCommitted(long count);
        internal abstract void TxFlowed();
        internal abstract void TxInDoubt(long count);

        internal override string[] CounterNames
        {
            get
            {
                return perfCounterNames;
            }
        }

        internal override string InstanceName
        {
            get
            {
                return this.instanceName;
            }
        }

        internal override int PerfCounterEnd
        {
            get
            {
                return 0x27;
            }
        }

        internal override int PerfCounterStart
        {
            get
            {
                return 0;
            }
        }

        internal enum PerfCounters
        {
            Calls,
            CallsPerSecond,
            CallsOutstanding,
            CallsFailed,
            CallsFailedPerSecond,
            CallsFaulted,
            CallsFaultedPerSecond,
            CallDuration,
            CallDurationBase,
            SecurityValidationAuthenticationFailures,
            SecurityValidationAuthenticationFailuresPerSecond,
            CallsNotAuthorized,
            CallsNotAuthorizedPerSecond,
            Instances,
            InstancesRate,
            RMSessionsFaulted,
            RMSessionsFaultedPerSecond,
            RMMessagesDropped,
            RMMessagesDroppedPerSecond,
            TxFlowed,
            TxFlowedPerSecond,
            TxCommitted,
            TxCommittedPerSecond,
            TxAborted,
            TxAbortedPerSecond,
            TxInDoubt,
            TxInDoubtPerSecond,
            MsmqPoisonMessages,
            MsmqPoisonMessagesPerSecond,
            MsmqRejectedMessages,
            MsmqRejectedMessagesPerSecond,
            MsmqDroppedMessages,
            MsmqDroppedMessagesPerSecond,
            CallsPercentMaxCalls,
            CallsPercentMaxCallsBase,
            InstancesPercentMaxInstances,
            InstancesPercentMaxInstancesBase,
            SessionsPercentMaxSessions,
            SessionsPercentMaxSessionsBase,
            TotalCounters
        }

        [Flags]
        private enum truncOptions : uint
        {
            NoBits = 0,
            service32 = 1,
            uri31 = 4
        }
    }
}


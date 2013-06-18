namespace System.ServiceModel.Diagnostics
{
    using System;

    internal abstract class EndpointPerformanceCountersBase : PerformanceCountersBase
    {
        private const int hashLength = 2;
        protected string instanceName;
        private const int maxCounterLength = 0x40;
        protected static readonly string[] perfCounterNames = new string[] { 
            "Calls", "Calls Per Second", "Calls Outstanding", "Calls Failed", "Calls Failed Per Second", "Calls Faulted", "Calls Faulted Per Second", "Calls Duration", "Calls Duration Base", "Security Validation and Authentication Failures", "Security Validation and Authentication Failures Per Second", "Security Calls Not Authorized", "Security Calls Not Authorized Per Second", "Reliable Messaging Sessions Faulted", "Reliable Messaging Sessions Faulted Per Second", "Reliable Messaging Messages Dropped", 
            "Reliable Messaging Messages Dropped Per Second", "Transactions Flowed", "Transactions Flowed Per Second"
         };

        internal EndpointPerformanceCountersBase(string service, string contract, string uri)
        {
            this.instanceName = CreateFriendlyInstanceName(service, contract, uri);
        }

        internal abstract void AuthenticationFailed();
        internal abstract void AuthorizationFailed();
        internal static string CreateFriendlyInstanceName(string service, string contract, string uri)
        {
            int totalLen = ((service.Length + contract.Length) + uri.Length) + 2;
            if (totalLen > 0x40)
            {
                int num2 = 0;
                truncOptions options = GetCompressionTasks(totalLen, service.Length, contract.Length, uri.Length);
                if ((options & (truncOptions.NoBits | truncOptions.service15)) > truncOptions.NoBits)
                {
                    num2 = 15;
                    service = PerformanceCountersBase.GetHashedString(service, num2 - 2, (service.Length - num2) + 2, true);
                }
                if ((options & truncOptions.contract16) > truncOptions.NoBits)
                {
                    num2 = 0x10;
                    contract = PerformanceCountersBase.GetHashedString(contract, num2 - 2, (contract.Length - num2) + 2, true);
                }
                if ((options & (truncOptions.NoBits | truncOptions.uri31)) > truncOptions.NoBits)
                {
                    num2 = 0x1f;
                    uri = PerformanceCountersBase.GetHashedString(uri, 0, (uri.Length - num2) + 2, false);
                }
            }
            return (service + "." + contract + "@" + uri.Replace('/', '|'));
        }

        private static truncOptions GetCompressionTasks(int totalLen, int serviceLen, int contractLen, int uriLen)
        {
            truncOptions noBits = truncOptions.NoBits;
            if (totalLen > 0x40)
            {
                int num = totalLen;
                if ((num > 0x40) && (serviceLen > 15))
                {
                    noBits |= truncOptions.NoBits | truncOptions.service15;
                    num -= serviceLen - 15;
                }
                if ((num > 0x40) && (contractLen > 0x10))
                {
                    noBits |= truncOptions.contract16;
                    num -= contractLen - 0x10;
                }
                if ((num > 0x40) && (uriLen > 0x1f))
                {
                    noBits |= truncOptions.NoBits | truncOptions.uri31;
                }
            }
            return noBits;
        }

        internal abstract void MessageDropped();
        internal abstract void MethodCalled();
        internal abstract void MethodReturnedError();
        internal abstract void MethodReturnedFault();
        internal abstract void MethodReturnedSuccess();
        internal abstract void SaveCallDuration(long time);
        internal abstract void SessionFaulted();
        internal abstract void TxFlowed();

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
                return 0x13;
            }
        }

        internal override int PerfCounterStart
        {
            get
            {
                return 0;
            }
        }

        protected enum PerfCounters
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
            RMSessionsFaulted,
            RMSessionsFaultedPerSecond,
            RMMessagesDropped,
            RMMessagesDroppedPerSecond,
            TxFlowed,
            TxFlowedPerSecond,
            TotalCounters
        }

        [Flags]
        private enum truncOptions : uint
        {
            contract16 = 2,
            NoBits = 0,
            service15 = 1,
            uri31 = 4
        }
    }
}


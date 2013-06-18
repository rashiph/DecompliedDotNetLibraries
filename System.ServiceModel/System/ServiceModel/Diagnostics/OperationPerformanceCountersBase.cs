namespace System.ServiceModel.Diagnostics
{
    using System;

    internal abstract class OperationPerformanceCountersBase : PerformanceCountersBase
    {
        private const int hashLength = 2;
        protected string instanceName;
        private const int maxCounterLength = 0x40;
        protected string operationName;
        protected static readonly string[] perfCounterNames = new string[] { "Calls", "Calls Per Second", "Calls Outstanding", "Calls Failed", "Call Failed Per Second", "Calls Faulted", "Calls Faulted Per Second", "Calls Duration", "Calls Duration Base", "Security Validation and Authentication Failures", "Security Validation and Authentication Failures Per Second", "Security Calls Not Authorized", "Security Calls Not Authorized Per Second", "Transactions Flowed", "Transactions Flowed Per Second" };

        internal OperationPerformanceCountersBase(string service, string contract, string operationName, string uri)
        {
            this.operationName = operationName;
            this.instanceName = CreateFriendlyInstanceName(service, contract, operationName, uri);
        }

        internal abstract void AuthenticationFailed();
        internal abstract void AuthorizationFailed();
        internal static string CreateFriendlyInstanceName(string service, string contract, string operation, string uri)
        {
            int totalLen = (((service.Length + contract.Length) + operation.Length) + uri.Length) + 3;
            if (totalLen > 0x40)
            {
                int num2 = 0;
                truncOptions options = GetCompressionTasks(totalLen, service.Length, contract.Length, operation.Length, uri.Length);
                if ((options & (truncOptions.NoBits | truncOptions.service7)) > truncOptions.NoBits)
                {
                    num2 = 7;
                    service = PerformanceCountersBase.GetHashedString(service, num2 - 2, (service.Length - num2) + 2, true);
                }
                if ((options & truncOptions.contract7) > truncOptions.NoBits)
                {
                    num2 = 7;
                    contract = PerformanceCountersBase.GetHashedString(contract, num2 - 2, (contract.Length - num2) + 2, true);
                }
                if ((options & (truncOptions.NoBits | truncOptions.operation15)) > truncOptions.NoBits)
                {
                    num2 = 15;
                    operation = PerformanceCountersBase.GetHashedString(operation, num2 - 2, (operation.Length - num2) + 2, true);
                }
                if ((options & (truncOptions.NoBits | truncOptions.uri32)) > truncOptions.NoBits)
                {
                    num2 = 0x20;
                    uri = PerformanceCountersBase.GetHashedString(uri, 0, (uri.Length - num2) + 2, false);
                }
            }
            return (service + "." + contract + "." + operation + "@" + uri.Replace('/', '|'));
        }

        private static truncOptions GetCompressionTasks(int totalLen, int serviceLen, int contractLen, int operationLen, int uriLen)
        {
            truncOptions noBits = truncOptions.NoBits;
            if (totalLen > 0x40)
            {
                int num = totalLen;
                if ((num > 0x40) && (serviceLen > 8))
                {
                    noBits |= truncOptions.NoBits | truncOptions.service7;
                    num -= serviceLen - 7;
                }
                if ((num > 0x40) && (contractLen > 7))
                {
                    noBits |= truncOptions.contract7;
                    num -= contractLen - 7;
                }
                if ((num > 0x40) && (operationLen > 15))
                {
                    noBits |= truncOptions.NoBits | truncOptions.operation15;
                    num -= operationLen - 15;
                }
                if ((num > 0x40) && (uriLen > 0x20))
                {
                    noBits |= truncOptions.NoBits | truncOptions.uri32;
                }
            }
            return noBits;
        }

        internal abstract void MethodCalled();
        internal abstract void MethodReturnedError();
        internal abstract void MethodReturnedFault();
        internal abstract void MethodReturnedSuccess();
        internal abstract void SaveCallDuration(long time);
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

        internal string OperationName
        {
            get
            {
                return this.operationName;
            }
        }

        internal override int PerfCounterEnd
        {
            get
            {
                return 15;
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
            TxFlowed,
            TxFlowedPerSecond,
            TotalCounters
        }

        [Flags]
        private enum truncOptions : uint
        {
            contract7 = 2,
            NoBits = 0,
            operation15 = 4,
            service7 = 1,
            uri32 = 8
        }
    }
}


namespace System.Diagnostics.Contracts.Internal
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    public static class ContractHelper
    {
        private static EventHandler<ContractFailedEventArgs> contractFailedEvent;
        private static readonly object lockObject = new object();

        internal static  event EventHandler<ContractFailedEventArgs> InternalContractFailed
        {
            [SecurityCritical] add
            {
                RuntimeHelpers.PrepareContractedDelegate(value);
                lock (lockObject)
                {
                    contractFailedEvent = (EventHandler<ContractFailedEventArgs>) Delegate.Combine(contractFailedEvent, value);
                }
            }
            [SecurityCritical] remove
            {
                lock (lockObject)
                {
                    contractFailedEvent = (EventHandler<ContractFailedEventArgs>) Delegate.Remove(contractFailedEvent, value);
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static string GetDisplayMessage(ContractFailureKind failureKind, string userMessage, string conditionText)
        {
            string key = null;
            switch (failureKind)
            {
                case ContractFailureKind.Precondition:
                    key = "PreconditionFailed";
                    break;

                case ContractFailureKind.Postcondition:
                    key = "PostconditionFailed";
                    break;

                case ContractFailureKind.PostconditionOnException:
                    key = "PostconditionOnExceptionFailed";
                    break;

                case ContractFailureKind.Invariant:
                    key = "InvariantFailed";
                    break;

                case ContractFailureKind.Assert:
                    key = "AssertionFailed";
                    break;

                case ContractFailureKind.Assume:
                    key = "AssumptionFailed";
                    break;

                default:
                    Contract.Assume(false, "Unreachable code");
                    key = "AssumptionFailed";
                    break;
            }
            if (!string.IsNullOrEmpty(conditionText))
            {
                key = key + "_Cnd";
            }
            string runtimeResourceString = Environment.GetRuntimeResourceString(key);
            if (!string.IsNullOrEmpty(conditionText))
            {
                if (!string.IsNullOrEmpty(userMessage))
                {
                    return (string.Format(CultureInfo.CurrentUICulture, runtimeResourceString, new object[] { conditionText }) + "  " + userMessage);
                }
                return string.Format(CultureInfo.CurrentUICulture, runtimeResourceString, new object[] { conditionText });
            }
            if (!string.IsNullOrEmpty(userMessage))
            {
                return (runtimeResourceString + "  " + userMessage);
            }
            return runtimeResourceString;
        }

        [SecuritySafeCritical, DebuggerNonUserCode, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static string RaiseContractFailedEvent(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException)
        {
            string resultFailureMessage = "Contract failed";
            RaiseContractFailedEventImplementation(failureKind, userMessage, conditionText, innerException, ref resultFailureMessage);
            return resultFailureMessage;
        }

        [DebuggerNonUserCode, SecuritySafeCritical]
        private static void RaiseContractFailedEventImplementation(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException, ref string resultFailureMessage)
        {
            string str2;
            if ((failureKind < ContractFailureKind.Precondition) || (failureKind > ContractFailureKind.Assume))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { failureKind }), "failureKind");
            }
            string message = "contract failed.";
            ContractFailedEventArgs e = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                message = GetDisplayMessage(failureKind, userMessage, conditionText);
                if (contractFailedEvent != null)
                {
                    e = new ContractFailedEventArgs(failureKind, message, conditionText, innerException);
                    foreach (EventHandler<ContractFailedEventArgs> handler in contractFailedEvent.GetInvocationList())
                    {
                        try
                        {
                            handler(null, e);
                        }
                        catch (Exception exception)
                        {
                            e.thrownDuringHandler = exception;
                            e.SetUnwind();
                        }
                    }
                    if (e.Unwind)
                    {
                        if (Environment.IsCLRHosted)
                        {
                            TriggerCodeContractEscalationPolicy(failureKind, message, conditionText, innerException);
                        }
                        if (innerException == null)
                        {
                            innerException = e.thrownDuringHandler;
                        }
                        throw new ContractException(failureKind, message, userMessage, conditionText, innerException);
                    }
                }
            }
            finally
            {
                if ((e != null) && e.Handled)
                {
                    str2 = null;
                }
                else
                {
                    str2 = message;
                }
            }
            resultFailureMessage = str2;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical, DebuggerNonUserCode]
        private static void TriggerCodeContractEscalationPolicy(ContractFailureKind failureKind, string message, string conditionText, Exception innerException)
        {
            string exceptionAsString = null;
            if (innerException != null)
            {
                exceptionAsString = innerException.ToString();
            }
            Environment.TriggerCodeContractFailure(failureKind, message, conditionText, exceptionAsString);
        }

        [SecuritySafeCritical, DebuggerNonUserCode, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void TriggerFailure(ContractFailureKind kind, string displayMessage, string userMessage, string conditionText, Exception innerException)
        {
            TriggerFailureImplementation(kind, displayMessage, userMessage, conditionText, innerException);
        }

        [DebuggerNonUserCode, SecuritySafeCritical]
        private static void TriggerFailureImplementation(ContractFailureKind kind, string displayMessage, string userMessage, string conditionText, Exception innerException)
        {
            if (Environment.IsCLRHosted)
            {
                TriggerCodeContractEscalationPolicy(kind, displayMessage, conditionText, innerException);
            }
            if (!Environment.UserInteractive)
            {
                Environment.FailFast(displayMessage);
            }
            Assert.Check(false, conditionText, displayMessage);
        }
    }
}


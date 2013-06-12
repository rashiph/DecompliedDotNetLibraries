namespace System.Diagnostics.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts.Internal;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    public static class Contract
    {
        private static bool _assertingMustUseRewriter;

        public static  event EventHandler<ContractFailedEventArgs> ContractFailed
        {
            [SecurityCritical] add
            {
                ContractHelper.InternalContractFailed += value;
            }
            [SecurityCritical] remove
            {
                ContractHelper.InternalContractFailed -= value;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("DEBUG"), Conditional("CONTRACTS_FULL")]
        public static void Assert(bool condition)
        {
            if (!condition)
            {
                ReportFailure(ContractFailureKind.Assert, null, null, null);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("DEBUG"), Conditional("CONTRACTS_FULL")]
        public static void Assert(bool condition, string userMessage)
        {
            if (!condition)
            {
                ReportFailure(ContractFailureKind.Assert, userMessage, null, null);
            }
        }

        private static void AssertMustUseRewriter(ContractFailureKind kind, string contractKind)
        {
            if (_assertingMustUseRewriter)
            {
                System.Diagnostics.Assert.Fail("Asserting that we must use the rewriter went reentrant.", "Didn't rewrite this mscorlib?");
            }
            _assertingMustUseRewriter = true;
            ContractHelper.TriggerFailure(kind, "Must use the rewriter when using Contract." + contractKind, null, null, null);
            _assertingMustUseRewriter = false;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("DEBUG"), Conditional("CONTRACTS_FULL")]
        public static void Assume(bool condition)
        {
            if (!condition)
            {
                ReportFailure(ContractFailureKind.Assume, null, null, null);
            }
        }

        [Conditional("DEBUG"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("CONTRACTS_FULL")]
        public static void Assume(bool condition, string userMessage)
        {
            if (!condition)
            {
                ReportFailure(ContractFailureKind.Assume, userMessage, null, null);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), Conditional("CONTRACTS_FULL")]
        public static void EndContractBlock()
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("CONTRACTS_FULL")]
        public static void Ensures(bool condition)
        {
            AssertMustUseRewriter(ContractFailureKind.Postcondition, "Ensures");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("CONTRACTS_FULL")]
        public static void Ensures(bool condition, string userMessage)
        {
            AssertMustUseRewriter(ContractFailureKind.Postcondition, "Ensures");
        }

        [Conditional("CONTRACTS_FULL"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void EnsuresOnThrow<TException>(bool condition) where TException: Exception
        {
            AssertMustUseRewriter(ContractFailureKind.PostconditionOnException, "EnsuresOnThrow");
        }

        [Conditional("CONTRACTS_FULL"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void EnsuresOnThrow<TException>(bool condition, string userMessage) where TException: Exception
        {
            AssertMustUseRewriter(ContractFailureKind.PostconditionOnException, "EnsuresOnThrow");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            foreach (T local in collection)
            {
                if (predicate(local))
                {
                    return true;
                }
            }
            return false;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static bool Exists(int fromInclusive, int toExclusive, Predicate<int> predicate)
        {
            if (fromInclusive > toExclusive)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ToExclusiveLessThanFromExclusive"));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                if (predicate(i))
                {
                    return true;
                }
            }
            return false;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static bool ForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            foreach (T local in collection)
            {
                if (!predicate(local))
                {
                    return false;
                }
            }
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static bool ForAll(int fromInclusive, int toExclusive, Predicate<int> predicate)
        {
            if (fromInclusive > toExclusive)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ToExclusiveLessThanFromExclusive"));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                if (!predicate(i))
                {
                    return false;
                }
            }
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("CONTRACTS_FULL")]
        public static void Invariant(bool condition)
        {
            AssertMustUseRewriter(ContractFailureKind.Invariant, "Invariant");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("CONTRACTS_FULL")]
        public static void Invariant(bool condition, string userMessage)
        {
            AssertMustUseRewriter(ContractFailureKind.Invariant, "Invariant");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static T OldValue<T>(T value)
        {
            return default(T);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DebuggerNonUserCode]
        private static void ReportFailure(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException)
        {
            if ((failureKind < ContractFailureKind.Precondition) || (failureKind > ContractFailureKind.Assume))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { failureKind }), "failureKind");
            }
            string displayMessage = ContractHelper.RaiseContractFailedEvent(failureKind, userMessage, conditionText, innerException);
            if (displayMessage != null)
            {
                ContractHelper.TriggerFailure(failureKind, displayMessage, userMessage, conditionText, innerException);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("CONTRACTS_FULL")]
        public static void Requires(bool condition)
        {
            AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void Requires<TException>(bool condition) where TException: Exception
        {
            AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires<TException>");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), Conditional("CONTRACTS_FULL")]
        public static void Requires(bool condition, string userMessage)
        {
            AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void Requires<TException>(bool condition, string userMessage) where TException: Exception
        {
            AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires<TException>");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static T Result<T>()
        {
            return default(T);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static T ValueAtReturn<T>(out T value)
        {
            value = default(T);
            return value;
        }
    }
}


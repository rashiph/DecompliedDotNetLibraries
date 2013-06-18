namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.ServiceModel.Channels;

    internal class CoordinatorRegistrationFailedFault : Fault
    {
        private CoordinatorRegistrationFailedFault(string coordinatorReason) : base(AtomicTransactionStrings.Version(ProtocolVersion.Version10).FaultAction, Faults.CoordinatorRegistrationFailedCode, coordinatorReason)
        {
        }

        public static CoordinatorRegistrationFailedFault CreateFault(MessageFault fault)
        {
            string str;
            if (fault == null)
            {
                str = Microsoft.Transactions.SR.GetString("CoordinatorRegistrationFailedReason");
            }
            else
            {
                string faultCodeName = Library.GetFaultCodeName(fault);
                if (faultCodeName == null)
                {
                    str = Microsoft.Transactions.SR.GetString("CoordinatorRegistrationFaultedUnknownReason");
                }
                else
                {
                    str = Microsoft.Transactions.SR.GetString("CoordinatorRegistrationFaultedReason", new object[] { faultCodeName });
                }
            }
            return new CoordinatorRegistrationFailedFault(str);
        }
    }
}


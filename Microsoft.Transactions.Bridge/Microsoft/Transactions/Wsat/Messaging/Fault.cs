namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;

    internal class Fault
    {
        private string action;
        private FaultCode code;
        private FaultReason reason;
        private string reasonText;

        public Fault(string action, FaultCode code, string reasonText)
        {
            this.action = action;
            this.code = code;
            this.reasonText = reasonText;
            this.reason = new FaultReason(reasonText, CultureInfo.CurrentCulture);
        }

        public string Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
        }

        public FaultCode Code
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.code;
            }
        }

        public FaultReason Reason
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.reason;
            }
        }

        public string ReasonText
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.reasonText;
            }
        }
    }
}


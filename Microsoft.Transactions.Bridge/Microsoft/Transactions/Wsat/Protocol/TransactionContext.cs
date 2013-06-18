namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime;
    using System.ServiceModel.Security;

    internal class TransactionContext
    {
        private Microsoft.Transactions.Wsat.Messaging.CoordinationContext context;
        private RequestSecurityTokenResponse issuedToken;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TransactionContext(Microsoft.Transactions.Wsat.Messaging.CoordinationContext context, RequestSecurityTokenResponse issuedToken)
        {
            this.context = context;
            this.issuedToken = issuedToken;
        }

        public override string ToString()
        {
            return this.context.Identifier;
        }

        public Microsoft.Transactions.Wsat.Messaging.CoordinationContext CoordinationContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
        }

        public RequestSecurityTokenResponse IssuedToken
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.issuedToken;
            }
        }
    }
}


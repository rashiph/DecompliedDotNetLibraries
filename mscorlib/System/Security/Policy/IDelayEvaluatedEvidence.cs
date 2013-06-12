namespace System.Security.Policy
{
    using System;
    using System.Security;

    internal interface IDelayEvaluatedEvidence
    {
        void MarkUsed();

        bool IsVerified { [SecurityCritical] get; }

        bool WasUsed { get; }
    }
}

